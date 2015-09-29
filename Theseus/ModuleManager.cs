//
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
    public sealed class ModuleManager : PluginManager<Module>, IModuleManager {
        private static readonly String COMMAND_PREFIX = "/";
        private class CommandProcessor {
            public Module Module;
            public RolesAttribute Roles;
            public CommandAttribute Command;
            private System.Reflection.MethodInfo MethodInfo;

            public CommandProcessor(Module module, System.Reflection.MethodInfo methodInfo, CommandAttribute command, RolesAttribute roles = null) {
                Module = module;
                Roles = roles ?? new RolesAttribute();
                MethodInfo = methodInfo;
                Command = command;
            }

            public async Task<Response> InvokeProcessor(Sender sender, String[] args){
                Task<Response> response = (Task<Response>)MethodInfo.Invoke(Module, new object[]{ sender, args });
                return await response;
            }
        }

        private Dictionary<String, CommandProcessor> allowedCommands = new Dictionary<String, CommandProcessor>();

        public ModuleManager(ICore core, List<Configuration.Plugin> configs)
            : base(core, configs) {
        }

        public List<string> GetAllowedCommands(Sender sender){
            List<String> commands = new List<string>();
            foreach (var command in allowedCommands) {
                if (command.Value.Roles.IsRoleAllowed(sender.Role))
                    commands.Add(command.Key);
            }
            return commands;
        }

        public CommandAttribute GetCommandInfo(string command){
            if (allowedCommands.ContainsKey(command))
                return allowedCommands[command].Command;
            return null;
        }

        public String GetCommandPrefix() {
            return COMMAND_PREFIX;
        }

        public override void AddPlugin(Module module){
            base.AddPlugin(module);
            AddCommandsMap(module);
        }

        public override void RemovePlugin(Module module){
            base.RemovePlugin(module);
            RemoveCommandsMap(module);
        }

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

        public bool ShouldProcessRequest(Request request){
            return request.Command != null 
                && request.Command.StartsWith(COMMAND_PREFIX) 
                && request.Command.Length > COMMAND_PREFIX.Length;
        }

        public async Task<Response> Process(Request request){
            var args = new List<String>();
            var enumerator = StringInfo.GetTextElementEnumerator(request.Command);
            string currentElement = "";
            Stack<String> nesting = new Stack<String>();
            //Parse string "/login admin 'long password with spaces and " another brackets"'" 
            // as [/login,admin, long password with spaces and " another brackets"]
            while(enumerator.MoveNext()) {
                var element = enumerator.Current.ToString();
                if (element == " " && nesting.Count == 0) {
                    if (currentElement.Length > 0) {
                        args.Add(currentElement);
                    }
                    currentElement = "";
                }
                else if (element == "\"" || element == "'") {
                    if (nesting.Count == 0) {
                        nesting.Push(element);
                    }
                    else if (nesting.Peek() == element) {
                        nesting.Pop();
                        if (nesting.Count > 0) {
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
            commandName = commandName.Substring(COMMAND_PREFIX.Length);
            if (allowedCommands.ContainsKey(commandName)) {
                var processor = allowedCommands[commandName];
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

