﻿//
//  File: ModuleManager.cs
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

namespace Theseus {
    /// <summary>
    /// Module manager.
    /// </summary>
    public sealed class ModuleManager : PluginManager<Module>, IModuleManager {
        /// <summary>
        /// Default command prefix
        /// </summary>
        private static readonly String COMMAND_PREFIX = "/";

        /// <summary>
        /// Wrapper for command processor
        /// </summary>
        private class CommandProcessor {
            /// <summary>
            /// Command's module
            /// </summary>
            public Module Module;

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
            /// Initializes a new instance of the <see cref="Theseus.ModuleManager+CommandProcessor"/> class.
            /// </summary>
            /// <param name="module">Command's module.</param>
            /// <param name="methodInfo">Responsible command method.</param>
            /// <param name="command">Command information.</param>
            /// <param name="roles">Role rules.</param>
            public CommandProcessor(Module module, System.Reflection.MethodInfo methodInfo, CommandAttribute command, RolesAttribute roles = null) {
                Module = module;
                Roles = roles ?? new RolesAttribute();
                MethodInfo = methodInfo;
                Command = command;
            }

            /// <summary>
            /// Invokes the command processor.
            /// </summary>
            /// <returns>Response object.</returns>
            /// <param name="sender">Initial sender.</param>
            /// <param name="args">Arguments.</param>
            public async Task<Response> InvokeProcessor(Sender sender, String[] args){
                Task<Response> response = (Task<Response>)MethodInfo.Invoke(Module, new object[]{ sender, args });
                return await response;
            }
        }

        /// <summary>
        /// The allowed commands.
        /// </summary>
        private Dictionary<String, CommandProcessor> allowedCommands = new Dictionary<String, CommandProcessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Theseus.ModuleManager"/> class.
        /// </summary>
        /// <param name="core">Platworm core.</param>
        /// <param name="configs">Adapter configurations. See <see cref="Theseus.Configuration"/>.</param>
        public ModuleManager(ICore core, List<Configuration.Plugin> configs)
            : base(core, configs) {
        }

        /// <summary>
        /// Gets the allowed commands.
        /// </summary>
        /// <returns>The allowed commands.</returns>
        /// <param name="sender">Sender. It is used to filter commands by sender permissions role</param>
        public List<string> GetAllowedCommands(Sender sender){
            List<String> commands = new List<string>();
            foreach (var command in allowedCommands) {
                if (command.Value.Roles.IsRoleAllowed(sender.Role))
                    commands.Add(command.Key);
            }
            return commands;
        }

        /// <summary>
        /// Gets the information about command.
        /// </summary>
        /// <returns>The command information.</returns>
        /// <param name="command">Command.</param>
        public CommandAttribute GetCommandInfo(string command){
            if (allowedCommands.ContainsKey(command))
                return allowedCommands[command].Command;
            return null;
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
        /// <param name="module">Module.</param>
        public override void AddPlugin(Module module){
            base.AddPlugin(module);
            AddCommandsMap(module);
        }

        /// <summary>
        /// Removes the plugin.
        /// </summary>
        /// <param name="module">Module.</param>
        public override void RemovePlugin(Module module){
            base.RemovePlugin(module);
            RemoveCommandsMap(module);
        }

        /// <summary>
        /// Parse module for command processors
        /// </summary>
        /// <param name="module">Module.</param>
        private void AddCommandsMap(Module module){
            foreach (var method in module.GetType().GetMethods()) {
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
                    allowedCommands[commandAttribute.Name] = 
                        new CommandProcessor(module, method, commandAttribute, rolesAttribute);
                }
            }
        }

        /// <summary>
        /// Removes parsed commands.
        /// </summary>
        /// <param name="module">Module.</param>
        private void RemoveCommandsMap(Module module){
            var keysToRemove = new List<String>();
            foreach (var mapping in allowedCommands) {
                if (mapping.Value.Module == module) {
                    keysToRemove.Add(mapping.Key);
                }
            }
            foreach (var key in keysToRemove) {
                allowedCommands.Remove(key);
            }
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
                    else if (nesting.Peek() == element) {
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

            //Looking for command processor
            if (allowedCommands.ContainsKey(commandName)) {
                var processor = allowedCommands[commandName];

                //Check permission
                if (processor.Roles.IsRoleAllowed(request.Sender.Role)) {
                    return await processor.InvokeProcessor(request.Sender, args.ToArray());
                }
                else {
                    var error = new Response(Channel.Private);
                    error.SetError("You aren't permitted to use this command");
                    return error;
                }
            }
            else {
                var error = new Response(Channel.Private);
                error.SetError("Command processor does not exists");
                return error;
            }
        }
    }
}

