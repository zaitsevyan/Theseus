//
//  File: MinecraftQuery.cs
//  Created: 29.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using NLog;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using ArgumentsLibrary;
using ArgumentsLibrary.Exceptions;

namespace Handlers {
    public class Minecraft : Handler {
        private Logger Logger { get; set; }

        private System.Threading.CancellationToken CancellationToken { get; set; }

        public Minecraft(Dictionary<String, Object> config, IHandlerManager manager)
            : base("Minecraft", config, manager) {
            Logger = Manager.GetLogger(this);
        }

        public override void Start(System.Threading.CancellationToken token){
            base.Start(token);
            CancellationToken = token;
            token.Register(Finish);
        }

        [Command("servers", "", "List of configured servers")]
        [Roles(Role.Normal)]
        public Task<Response> Servers(Sender sender, String[] args){
            var response = new Response(Channel.Same);
            response.SetMessage("Servers: {0}", String.Join(", ", GetServersList()));
            return Task.FromResult(response);
        }

        [Command("players", "[server]", "Current [server] online. If [server] is not defined, returns online for every configured server")]
        [Roles(Role.Normal)]
        public async Task<Response> Players(Sender sender, String[] args){
            var sb = new StringBuilder();
            var prefix = "";
            List<String> servers;
            if (args.Length == 0) {
                servers = GetServersList();
            }
            else {
                servers = new List<string>(args);
            }
            if (servers.Count > 1) {
                sb.AppendLine("Online");
                prefix = "    ";
            }
            var tasks = new List<Task<String>>();
            foreach (var serverName in servers) {
                var server = (Config["servers"] as Dictionary<String, Object>)[serverName] as Dictionary<String, Object>;
                var task = PingServer(server, serverName, prefix);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks) {
                sb.Append(task.Result);
            }
            var response = new Response(Channel.Same);
            response.SetMessage(sb.ToString());
            return response;
        }

        private async Task<String> PingServer(Dictionary<String, Object> server, String serverName, String prefix){
            var sb = new StringBuilder();
            sb.AppendFormat("{0}{1}: ", prefix, serverName);
            try {
                var host = server["host"] as String;
                var port = short.Parse(server["port"].ToString());
                Logger.Debug("Pinging {0}[{1}:{2}]...", serverName, host, port);
                var ping = await new MCServerPing.ServerPing(host, port, CancellationToken).Ping();

                sb.AppendFormat("{0}/{1} - {2}", ping.Players.Online, ping.Players.Max, ping.Motd);
                sb.AppendLine();
            }
            catch (SocketException e) {
                Logger.Trace(e);
                sb.AppendLine("Connection error");
            }
            catch (JsonReaderException e) {
                Logger.Trace(e);
                sb.AppendLine("Parsing error");
            }
            return sb.ToString();
        }

        private List<String> GetServersList(){
            return new List<String>((Config["servers"] as Dictionary<String, Object>).Keys);
        }

        [Command("unban", "<player>", "Unban <player>")]
        [Roles(Role.Moderator)]
        public Task<Response> Unban(Sender sender, String[] args){
            if (args.Length != 1) {
                var response = new Response(Channel.Private);
                response.SetError("Player name missed");
                return Task.FromResult(response);
            }
            else {
                var response = new Response(Channel.Same);
                response.SetMessage("Player {0} unbaned", args[0]);
                return Task.FromResult(response);
            }
        }


        private Arguments banArguments;

        [Command("ban", "[--time=1d] <player>", "Ban <player>. Time modifiers are Y, M, w, d, h, m")]
        [Roles(Role.Moderator)]
        public Task<Response> Ban(Sender sender, String[] args){
            SetupBanArguments();
            try {
                var options = banArguments.Parse(args);

                var plain = new List<String>(options.GetPlainArguments<String>());
                if (plain.Count != 1) {
                    var response = new Response(Channel.Private);
                    response.SetError("Player name missed");
                    return Task.FromResult(response);
                }
                else {
                    var player = plain[0];
                    var time = Time.Forever;
                    if(options.IsOptionSet("time")) {
                        time = options.GetOptionValue<Time>("time");
                    }
                    var response = new Response(Channel.Same);
                    response.SetMessage("Player {0} banned: {1}", player, time);
                    return Task.FromResult(response);
                }
            }
            catch (ArgumentsException e) {
                Logger.Trace(e);
                var response = new Response(Channel.Private);
                response.SetError("Cannot parse command options");
                return Task.FromResult(response);
            }
        }

        private void SetupBanArguments(){
            if (banArguments != null)
                return;
            banArguments = new Arguments();
            banArguments.AddOption("t|time")
                .WithArgument<Time>("TIME")
                .WithDefaultValue(Time.Forever);
            banArguments.RegisterTypeConverter(Time.Parse);
        }

        public class Time {
            public readonly TimeSpan Interval;
            public static Time Forever = new Time(int.MaxValue);

            public Time(int minutes) {
                Interval = TimeSpan.FromMinutes(minutes);
            }

            public Time(TimeSpan interval) {
                Interval = interval;
            }

            public static Time Parse(string input){
                var time = new TimeSpan();
                string numberPart = "";
                foreach (var c in input.ToCharArray()) {
                    if (Char.IsDigit(c)) {
                        numberPart += c;
                    }
                    else {
                        if (numberPart.Length == 0)
                            continue;
                        int number = int.Parse(numberPart);
                        numberPart = "";
                        if (c == 'Y')
                            time = time.Add(TimeSpan.FromDays(360 * number));
                        else if (c == 'M')
                            time = time.Add(TimeSpan.FromDays(30 * number));
                        else if (c == 'w')
                            time = time.Add(TimeSpan.FromDays(7 * number));
                        else if (c == 'd')
                            time = time.Add(TimeSpan.FromDays(number));
                        else if (c == 'h')
                            time = time.Add(TimeSpan.FromHours(number));
                        else if (c == 'm')
                            time = time.Add(TimeSpan.FromMinutes(number));
                    }
                }
                if (numberPart.Length > 0) {
                    time = time.Add(TimeSpan.FromMinutes(int.Parse(numberPart)));
                }
                return new Time(time);
            }

            public override string ToString(){
                if (this.Interval == Forever.Interval)
                    return "forever";
                else {
                    List<String> parts = new List<String>();
                    var time = Interval;
                    if (time.TotalDays > 360) {
                        parts.Add(String.Format("{0} years", time.Days / 360));
                        time = TimeSpan.FromDays(time.Days % 360);
                    }
                    if (time.TotalDays > 30) {
                        parts.Add(String.Format("{0} monthes", time.Days / 30));
                        time = TimeSpan.FromDays(time.Days % 30);
                    }
                    if (time.Days > 0) {
                        parts.Add(String.Format("{0} days", time.Days));
                    }
                    if (time.Hours > 0) {
                        parts.Add(String.Format("{0} hours", time.Hours));
                    }
                    if (time.Minutes > 0) {
                        parts.Add(String.Format("{0} minutes", time.Minutes));
                    }
                    return String.Join(" ", parts);
                }
            }
        }
    }
}

