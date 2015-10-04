//
//  File: HandlerManager.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace Theseus {
    /// <summary>
    /// Handler manager.
    /// </summary>
    public sealed class HandlerManager : PluginManager<Handler>, IHandlerManager {
        /// <summary>
        /// Default command prefix
        /// </summary>
        private static readonly String COMMAND_PREFIX = "/";

        /// <summary>
        /// Wrapper for command handler
        /// </summary>
        private class CommandHandler {
            /// <summary>
            /// Command's handler
            /// </summary>
            public Handler Handler;

            /// <summary>
            /// Role rules.
            /// </summary>
            public RolesAttribute Roles;

            /// <summary>
            /// Command information
            /// </summary>
            public CommandAttribute Command;

            /// <summary>
            /// Responsible command method
            /// </summary>
            private System.Reflection.MethodInfo MethodInfo;

            /// <summary>
            /// Initializes a new instance of the <see cref="Theseus.HandlerManager+CommandHandler"/> class.
            /// </summary>
            /// <param name="handler">Command's handler.</param>
            /// <param name="methodInfo">Responsible command method.</param>
            /// <param name="command">Command information.</param>
            /// <param name="roles">Role rules.</param>
            public CommandHandler(Handler handler, System.Reflection.MethodInfo methodInfo, CommandAttribute command, RolesAttribute roles = null) {
                Handler = handler;
                Roles = roles ?? new RolesAttribute();
                MethodInfo = methodInfo;
                Command = command;
            }


            /// <summary>
            /// Determines whether this instance is responsible for command name the specified commandName.
            /// </summary>
            /// <returns><c>true</c> if this instance is responsible for command name the specified commandName; otherwise, <c>false</c>.</returns>
            /// <param name="commandName">Command name.</param>
            public bool IsResponsibleForCommandName(String commandName) {
                var c = Thread.CurrentThread.CurrentCulture;
                var uic = Thread.CurrentThread.CurrentUICulture;
                if (Handler.Culture != null) {
                    Thread.CurrentThread.CurrentCulture = Handler.Culture;
                    Thread.CurrentThread.CurrentUICulture = Handler.Culture;
                }
                bool result = Command.NormalizedNames.Any((name) => String.Equals(name, commandName, StringComparison.CurrentCultureIgnoreCase));

                Thread.CurrentThread.CurrentCulture = c;
                Thread.CurrentThread.CurrentUICulture = uic;
                return result;
            }

            /// <summary>
            /// Invokes the command handler.
            /// </summary>
            /// <returns>Response object.</returns>
            /// <param name="sender">Initial sender.</param>
            /// <param name="args">Arguments.</param>
            public async Task<Response> InvokeHandler(Sender sender, String[] args){
                if(Handler.Culture != null)
                {
                    Thread.CurrentThread.CurrentCulture = Handler.Culture;
                    Thread.CurrentThread.CurrentUICulture = Handler.Culture;
                    SynchronizationContext.SetSynchronizationContext(new CultureAwareSynchronizationContext());
                }

                Task<Response> response = (Task<Response>)MethodInfo.Invoke(Handler, new object[]{ sender, args });
                return await response;
            }
        }

        /// <summary>
        /// The allowed commands.
        /// </summary>
        private List<CommandHandler> allowedCommands = new List<CommandHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Theseus.HandlerManager"/> class.
        /// </summary>
        /// <param name="core">Platworm core.</param>
        /// <param name="configs">Adapter configurations. See <see cref="Theseus.Configuration"/>.</param>
        public HandlerManager(ICore core, List<Configuration.Plugin> configs)
            : base(core, configs) {
        }

        /// <summary>
        /// Gets the allowed commands.
        /// </summary>
        /// <returns>The allowed commands.</returns>
        /// <param name="sender">Sender. It is used to filter commands by sender permissions role</param>
        public List<CommandAttribute> GetAllowedCommands(Sender sender){
            var commands = new List<CommandAttribute>();
            foreach (var handler in allowedCommands) {
                if (handler.Roles.IsRoleAllowed(sender.Role))
                    commands.Add(handler.Command);
            }
            return commands;
        }

        /// <summary>
        /// Gets the information about command.
        /// </summary>
        /// <returns>The command information.</returns>
        /// <param name="commandName">Command name.</param>
        public CommandAttribute GetCommandInfo(String commandName){
            return GetCommandHandler(commandName)?.Command;
        }

        /// <summary>
        /// Gets the command handler.
        /// </summary>
        /// <returns>The command handler.</returns>
        /// <param name="commandName">Command name.</param>
        private CommandHandler GetCommandHandler(String commandName) {
            commandName = commandName.RemoveDiacritics();
            return allowedCommands.FirstOrDefault((handler) => handler.IsResponsibleForCommandName(commandName));
        }

        /// <summary>
        /// Gets the command prefix.
        /// It is '/' by default.
        /// </summary>
        /// <returns>The command prefix.</returns>
        public String GetCommandPrefix() {
            return COMMAND_PREFIX;
        }

        /// <summary>
        /// Adds the plugin.
        /// </summary>
        /// <param name="handler">Handler.</param>
        public override void AddPlugin(Handler handler){
            base.AddPlugin(handler);
            AddCommandsMap(handler);
        }

        /// <summary>
        /// Removes the plugin.
        /// </summary>
        /// <param name="handler">Handler.</param>
        public override void RemovePlugin(Handler handler){
            base.RemovePlugin(handler);
            RemoveCommandsMap(handler);
        }

        /// <summary>
        /// Parse plugin for command handlers
        /// </summary>
        /// <param name="handler">Handler.</param>
        private void AddCommandsMap(Handler handler){
            foreach (var method in handler.GetType().GetMethods()) {
                CommandAttribute commandAttribute = null;
                RolesAttribute rolesAttribute = null;
                foreach (var attribute in method.GetCustomAttributes(false)) {
                    if (attribute is CommandAttribute) {
                        commandAttribute = attribute as CommandAttribute;
                    }
                    else if (attribute is RolesAttribute) {
                        rolesAttribute = attribute as RolesAttribute;
                    }
                }

                if (commandAttribute != null) {
                    allowedCommands.Add(new CommandHandler(handler, method, commandAttribute, rolesAttribute));
                }
            }
        }

        /// <summary>
        /// Removes parsed command handlers.
        /// </summary>
        /// <param name="handler">Handler.</param>
        private void RemoveCommandsMap(Handler handler){
            allowedCommands.RemoveAll((info) => info.Handler == handler);
        }

        /// <summary>
        /// Shoulds the process request.
        /// </summary>
        /// <returns><c>true</c>, if IAdapterManager should process request, <c>false</c> otherwise.</returns>
        /// <param name="request">Request.</param>
        public bool ShouldProcessRequest(Request request){
            return request.Command != null 
                && request.Command.StartsWith(COMMAND_PREFIX) 
                && request.Command.Length > COMMAND_PREFIX.Length;
        }

        /// <summary>
        /// Process the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public async Task<Response> Process(Request request){
            var args = new List<String>();
            var enumerator = StringInfo.GetTextElementEnumerator(request.Command);
            string currentElement = "";
            Stack<String> nesting = new Stack<String>();
            //Parse string "/login admin 'long password with spaces and " another brackets"'" 
            // as [/login,admin, long password with spaces and " another brackets"]
            // I implement correct 'brackets' sequence: "'""'", next algorithm will return '""' argument instead of "'","'"
            while(enumerator.MoveNext()) {
                var element = enumerator.Current.ToString();
                if (element == " " && nesting.Count == 0) { // simply split by whitespace
                    if (currentElement.Length > 0) {
                        args.Add(currentElement);
                    }
                    currentElement = "";
                }
                else if (element == "\"" || element == "'") { // ignore whitespace splitting
                    if (nesting.Count == 0) { //start ignoring
                        nesting.Push(element);
                    }
                    else if (String.Equals(nesting.Peek(), element, StringComparison.InvariantCulture)) {
                        nesting.Pop();
                        if (nesting.Count > 0) { //continue ignoring
                            currentElement += element;
                        }
                    }
                    else {
                        nesting.Push(element);
                        currentElement += element;
                    }
                }
                else {
                    currentElement += element;
                }
            }
            if (currentElement.Length > 0)
                args.Add(currentElement);
            var commandName = args[0];
            args.RemoveAt(0);
            commandName = commandName.Substring(COMMAND_PREFIX.Length); //Filter command name

            //Looking for command handler
            var handler = GetCommandHandler(commandName);
            if (handler == null) {
                var error = new Response(Channel.Private);
                error.SetError(TheseusStrings.CommandNotFound);
                return error;
            }

            //Check permission
            if (handler.Roles.IsRoleAllowed(request.Sender.Role)) {
                return await handler.InvokeHandler(request.Sender, args.ToArray());
            }
            else {
                var error = new Response(Channel.Private);
                error.SetError(TheseusStrings.CommandPermissionsError);
                return error;
            }
        }
    }
}

