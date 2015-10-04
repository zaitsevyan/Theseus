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

        [Command("Servers_Command", "Servers_Usage", "Servers_Note", 
            ResourceType = typeof(MinecraftStrings))]
        [Roles(Role.Normal)]
        public Task<Response> Servers(Sender sender, String[] args){
            var response = new Response(Channel.Same);
            response.SetMessage("{1}: {0}", String.Join(", ", GetServersList()), MinecraftStrings.Servers_PrintTitle);
            return Task.FromResult(response);
        }

        [Command("Players_Command", "Players_Usage", "Players_Note",
            ResourceType = typeof(MinecraftStrings))]
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
                sb.AppendLine(MinecraftStrings.Players_PrintTitle);
                prefix = "    ";
            }
            var tasks = new List<Task<String>>();
            foreach (var serverName in servers) {
                var server = (Config["servers"] as Dictionary<String, Object>)[serverName] as Dictionary<String, Object>;
                if (server == null)
                    continue;
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
                Logger.Debug("{3} {0}[{1}:{2}]...", serverName, host, port, MinecraftStrings.Ping);
                Logger.Debug("Start Thread = {0}, Culture = {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.CurrentCulture);
                var ping = await new MCServerPing.ServerPing(host, port, CancellationToken).Ping();
                Logger.Debug("End Thread = {0}, Culture = {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.CurrentCulture);

                sb.AppendFormat("{0}/{1} - {2}", ping.Players.Online, ping.Players.Max, ping.Motd);
                sb.AppendLine();
            }
            catch (SocketException e) {
                Logger.Trace(e);
                sb.AppendLine(MinecraftStrings.ConnectionError);
            }
            catch (JsonReaderException e) {
                Logger.Trace(e);
                sb.AppendLine(MinecraftStrings.ParseError);
            }
            return sb.ToString();
        }

        private List<String> GetServersList(){
            return new List<String>((Config["servers"] as Dictionary<String, Object>).Keys);
        }

        [Command("Unban_Command", "Unban_Usage", "Unban_Note",
            ResourceType = typeof(MinecraftStrings))]
        [Roles(Role.Moderator)]
        public Task<Response> Unban(Sender sender, String[] args){
            if (args.Length != 1) {
                var response = new Response(Channel.Private);
                response.SetError(MinecraftStrings.PlayerNameMissing);
                return Task.FromResult(response);
            }
            else {
                var response = new Response(Channel.Same);
                response.SetMessage(MinecraftStrings.Unban_Ok, args[0]);
                return Task.FromResult(response);
            }
        }


        private Arguments banArguments;

        [Command("Ban_Command", "Ban_Usage", "Ban_Note",
            ResourceType = typeof(MinecraftStrings))]
        [Roles(Role.Moderator)]
        public Task<Response> Ban(Sender sender, String[] args){
            SetupBanArguments();
            try {
                var options = banArguments.Parse(args);

                var plain = new List<String>(options.GetPlainArguments<String>());
                if (plain.Count != 1) {
                    var response = new Response(Channel.Private);
                    response.SetError(MinecraftStrings.PlayerNameMissing);
                    return Task.FromResult(response);
                }
                else {
                    var player = plain[0];
                    var time = Time.Forever;
                    if (options.IsOptionSet("time")) {
                        time = options.GetOptionValue<Time>("time");
                    }
                    var response = new Response(Channel.Same);
                    response.SetMessage(MinecraftStrings.Ban_Ok, player, time);
                    return Task.FromResult(response);
                }
            }
            catch (ArgumentsException e) {
                Logger.Trace(e);
                var response = new Response(Channel.Private);
                response.SetError(MinecraftStrings.OptionsParseError);
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
                        else if (c == 'W')
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
                    return MinecraftStrings.Time_Forever;
                else {
                    List<String> parts = new List<String>();
                    var time = Interval;
                    if (time.TotalDays > 360) {
                        parts.Add(String.Format("{0} Y", time.Days / 360));
                        time = TimeSpan.FromDays(time.Days % 360);
                    }
                    if (time.TotalDays > 30) {
                        parts.Add(String.Format("{0} M", time.Days / 30));
                        time = TimeSpan.FromDays(time.Days % 30);
                    }
                    if (time.Days > 0) {
                        parts.Add(String.Format("{0} d", time.Days));
                    }
                    if (time.Hours > 0) {
                        parts.Add(String.Format("{0} h", time.Hours));
                    }
                    if (time.Minutes > 0) {
                        parts.Add(String.Format("{0} m", time.Minutes));
                    }
                    return String.Join(" ", parts);
                }
            }
        }
    }
}

