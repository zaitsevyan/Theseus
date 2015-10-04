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
using System.Resources;
using System.Linq;

namespace Handlers {
    public class TheseusControl : Handler {
        public TheseusControl(Dictionary<String, Object> config, IHandlerManager manager)
            : base("Theseus Control", config, manager) {
        }

        public override void Start(System.Threading.CancellationToken token){
            base.Start(token);
            token.Register(Finish);
        }

        [Command("Shutdown_Command", "Shutdown_Usage", "Shutdown_Command",
            ResourceType = typeof(TheseusControlStrings))]
        [Roles(Role.Owner)]
        public Task<Response> Shutdown(Sender sender, String[] args){
            Manager.GetCore().Stop();
            return Task.FromResult<Response>(null);
        }

        [Command(new String[]{ "?", "Help_Command" }, "Help_Usage", "Help_Note",
            ResourceType = typeof(TheseusControlStrings))]
        [Roles(Role.Normal)]
        public Task<Response> Help(Sender sender, String[] args){
            List<CommandAttribute> commands;
            if (args.Length == 0)
                commands = Manager.GetAllowedCommands(sender);
            else
                commands = (from name in args
                                        select Manager.GetCommandInfo(name)).ToList();
            StringBuilder sb = new StringBuilder();
            if (commands.Count > 1) {
                sb.AppendLine(TheseusControlStrings.Help_PrintTitle);
            }
            foreach (var command in commands) {
                PrintCommandInfo(command, sb);
            }
            var response = new Response(Channel.Same);
            response.SetMessage(sb.ToString());
            return Task.FromResult<Response>(response);
        }

        private void PrintCommandInfo(CommandAttribute command, StringBuilder sb){
            // Print command name
            sb.AppendFormat("    {0}", command.NamesWithPrefix(Manager.GetCommandPrefix()));

            // Print usage
            if (command.Usage.Length > 0) {
                sb.AppendFormat(" {0}", command.Usage);
            }

            // Print description
            if (command.Note.Length > 0) {
                sb.AppendFormat(" - {0}", command.Note);
            }
            sb.AppendLine();
        }
    }
}

