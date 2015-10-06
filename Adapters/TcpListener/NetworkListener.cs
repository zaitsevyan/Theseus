//
//  File: TcpListener.cs
//  Created: 6.10.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Collections.Generic;
using NLog;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Adapters {
    public class NetworkListener: Adapter {
        private class Connection : Sender {
            public TcpClient Client;
            public StreamWriter Writer;
            public StreamReader Reader;

            public Connection(TcpClient client, Api.Role role)
                : base(new Account(client.ToString(), role)) {
                Client = client;
            }
        }

        private Logger Logger { get; set; }

        private CancellationToken CancellationToken { get; set; }

        private TcpListener Listener = null;
        private int Port = 5090;

        public NetworkListener(Dictionary<String, Object> config, IAdapterManager manager)
            : base("Network Listener", config, manager) {
            Logger = manager.GetLogger(this);
        }

        public override void Start(CancellationToken token){
            base.Start(token);
            token.Register(Finish);
            CancellationToken = token;
            if (Config.ContainsKey("port")) {
                int.TryParse(Config["port"].ToString(), out Port);
            }
            BeginAccept(Port);
        }

        public override void Finish(){
            base.Finish();
            if (Listener == null)
                return;
            Listener.Stop();
            Logger.Info("Stop listening on port {0}.", Port);
        }

        private async Task BeginAccept(int port){
            try {
                Logger.Info("Start listening on port {0}...", Port);
                Listener = new TcpListener(IPAddress.Any, Port);
                Listener.Start();
                Logger.Info("Start accepting on port {0}...", port);
                while (!CancellationToken.IsCancellationRequested) {
                    var acceptTask = await Listener.AcceptTcpClientAsync();
                    ProcessClient(acceptTask);
                }
                Logger.Info("Stop accepting on port {0}.", port);
            }
            catch (SocketException e) {
                Logger.Error(e);
            } 
        }

        private async Task ProcessClient(TcpClient client){
            var connection = new Connection(client, Role.Normal);
            try {
                using (var registration = CancellationToken.Register(client.Close))
                using (connection.Reader = new StreamReader(client.GetStream()))
                using (connection.Writer = new StreamWriter(client.GetStream())) {
                    while (!CancellationToken.IsCancellationRequested) {
                        var line = await connection.Reader.ReadLineAsync();
                        Manager.Process(this, new Request(connection, line));
                    }
                }
            }
            catch (SocketException e) {
                //It is ok.
                Logger.Debug(e);
            }

        }

        public override async void Process(Request request, Response response){
            base.Process(request, response);
            if (!(request.Sender is Connection))
                return;
            var connection = request.Sender as Connection;

            if (connection.Client.Connected) {
                await connection.Writer.WriteLineAsync(response.Info);
                await connection.Writer.FlushAsync();
            }
        }
    }
}

