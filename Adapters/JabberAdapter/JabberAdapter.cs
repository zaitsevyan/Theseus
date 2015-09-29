//
//  File: JabberAdapter.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using Api;
using NLog;

namespace Adapters {
    public sealed class JabberAdapter : Adapter {
        
        private class JabberRequest : Request {
            public readonly MessageType MessageType;

            public JabberRequest(JabberSender sender, String command, MessageType messageType)
                : base(sender, command) {
                MessageType = messageType;
            }
        }

        private class JabberSender : Sender {
            public readonly Jid Jid;

            public JabberSender(Jid jid, Api.Role role)
                : base(new Account(jid.Resource, role, jid.ToString()))
            {
                Jid = jid;
            }
        }

        private Logger Logger { get; set; }

        private Dictionary<String, JabberSender> accounts = new Dictionary<String, JabberSender>();

        private XmppClientConnection Connection { get; set; }

        private Jid Room;

        public JabberAdapter(Dictionary<String, Object> config, IAdapterManager manager)
            : base("Jabber", config, manager) {
            Logger = Manager.GetLogger(this);
        }

        public override void Start(CancellationToken token){
            base.Start(token);
            token.Register(Disconnect);
            Connection = new XmppClientConnection();
            Connection.Server = Config["server"] as String;
            Connection.ConnectServer = Config["server"] as String;
            Connection.Username = Config["username"] as String;
            Connection.Password = Config["password"] as String;
            Connection.Resource = "";
            Connection.AutoAgents = false;
            Connection.AutoPresence = true;
            Connection.AutoRoster = true;
            Connection.AutoResolveConnectServer = true;
            Connection.Priority = 0;
            Connection.OnLogin += new ObjectHandler(OnLoginEvent);
            Connection.OnPresence += async delegate(object sender, Presence pres) {
                User user = pres.SelectSingleElement(typeof(User)) as User;
                if (user != null) {
                    var userID = pres.From.ToString();
                    var role = Api.Role.Normal;
                    if (user.Item.Role == agsXMPP.protocol.x.muc.Role.moderator) {
                        role = Api.Role.Moderator;
                    }
                    var account = await Manager.GetCore().GetAccountsDB().GetAccount(userID, null);
                    if (account!=null && account.Role > role) {
                        role = account.Role;
                    }
                    accounts[userID] = new JabberSender(pres.From, role);
                    Logger.Debug("User: {0} - {1}", userID, role);
                }
            };

            Connection.OnError += delegate(object sender, Exception ex) {
                Logger.Error(ex);
                Disconnect();
            };
            Connection.OnWriteXml += new XmlHandler(delegate(object sender, string xml) {
                    Logger.Trace("Write: {0}", xml);
                });
            Connection.OnReadXml += new XmlHandler(delegate(object sender, string xml) {
                    Logger.Trace("Read: {0}", xml);
                });
            Connection.OnAuthError += delegate(object sender, agsXMPP.Xml.Dom.Element e) {
                Logger.Error("Cannot log in as {0} to {1}", Connection.Username, Connection.Server);
                Disconnect();
            };

            Connection.Open();
        }

        private void Disconnect(){
            if (Connection != null)
                Connection.Close();
            Finish();
        }

        private void OnLoginEvent(object sender){
            Logger.Info("Connected to {0}!", Connection.Server);
            Connection.SendMyPresence();
            MucManager mucManager = new MucManager(Connection);
            Room = new Jid(Config["conference"] as String);
            mucManager.AcceptDefaultConfiguration(Room);
            mucManager.JoinRoom(Room, Config["nickname"] as String);
            Presence p = new Presence(ShowType.chat, "Online");
            p.Type = PresenceType.available;
            Connection.Send(p);
            Task.Factory.StartNew(() => {
                    Thread.Sleep(1000);
                    Logger.Info("Start accepting messages...");
                    Connection.OnMessage += new MessageHandler(OnMessage);
                });
        }

        private void OnMessage(object s, Message msg){
            if (msg.XDelay != null)
                return;
            if (msg.Body == null || msg.Body == null)
                return;
            Logger.Debug("Message[{1}]: {0}", msg.Body, msg.Chatstate);

            var userID = msg.From.ToString();
            JabberSender sender;
            if (accounts.ContainsKey(userID)) {
                sender = accounts[userID];
            }
            else {
                sender = new JabberSender(msg.From, Api.Role.Normal);
            }
            var request = new JabberRequest(sender, msg.Body, msg.Type);
            Manager.Process(this, request);
        }

        public override void Process(Request r, Response response){
            base.Process(r, response);
            var request = r as JabberRequest;
            var sender = request.Sender as JabberSender;
            if (request == null || sender == null)
                return;
            if (response.Channel == Channel.Private || 
                (response.Channel == Channel.Same && request.MessageType != MessageType.groupchat)) {
                Connection.Send(new Message(sender.Jid, response.Info));
            }
            else {
                Connection.Send(new Message(Room, MessageType.groupchat, sender.Account.Name + ": " + response.Info));
            }
        }
    }
}

