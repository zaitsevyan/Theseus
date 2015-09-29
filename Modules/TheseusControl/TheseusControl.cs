//
//  File: TheseusControl.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Modules {
    public class TheseusControl : Module {
        public TheseusControl(Dictionary<String, Object> config, IModuleManager manager)
            : base("Theseus Control", config, manager) {
        }

        public override void Start(System.Threading.CancellationToken token){
            base.Start(token);
            token.Register(Finish);
        }

        [Command("shutdown", "", "Stop Theseus core")]
        [Roles(Role.Owner)]
        public Task<Response> Shutdown(Sender sender, String[] args){
            Manager.GetCore().Stop();
            return Task.FromResult<Response>(null);
        }

        [Command("help", "[command1] ... [commandN]", "Get help about all commands")]
        [Roles(Role.Normal)]
        public Task<Response> Help(Sender sender, String[] args){
            List<String> commands;
            if (args.Length == 0)
                commands = Manager.GetAllowedCommands(sender);
            else
                commands = new List<string>(args);
            StringBuilder sb = new StringBuilder();
            if (commands.Count > 1) {
                sb.AppendLine("Help");
            }
            foreach (var commandName in commands) {
                var command = Manager.GetCommandInfo(commandName);

                // Print command name
                sb.AppendFormat("    {0}{1}", Manager.GetCommandPrefix(), command.Name);

                // Print usage
                if (command.Usage.Length > 0) {
                    sb.AppendFormat(" {0}", command.Usage);
                }

                // Print description
                if (command.Description.Length > 0) {
                    sb.AppendFormat(" - {0}", command.Description);
                }
                sb.AppendLine();
            }
            var response = new Response(Channel.Same);
            response.SetMessage(sb.ToString());
            return Task.FromResult<Response>(response);
        }
    }
}

