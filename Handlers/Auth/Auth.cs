//
//  File: Auth.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Handlers {
    public class Auth : Handler {
        public Auth(Dictionary<String, Object> config, IHandlerManager manager)
            : base("Auth", config, manager) {
        }

        public override void Start(System.Threading.CancellationToken token){
            base.Start(token);
            token.Register(Finish);
        }

        [Command("Whoami_Command", "Whoami_Usage", "Whoami_Note", 
            ResourceType = typeof(AuthStrings))]
        [Roles(Role.Ignore)]
        public Task<Response> WhoAmI(Sender sender, String[] args){
            var response = new Response(Channel.Same);
            response.SetMessage("You are {0} with {1} role", sender.Account, sender.Role);
            return Task.FromResult(response);
        }

        [Command("Login_Command", "Login_Usage", "Login_Note", 
            ResourceType = typeof(AuthStrings))]
        [Roles(Role.Ignore)]
        public async Task<Response> Login(Sender sender, String[] args){
            if (args.Length != 2) {
                var response = new Response(Channel.Private);
                response.SetError(AuthStrings.Login_ArgsError);
                return response;
            }

            var username = args[0];
            var password = args[1];
            Account account = await Manager.GetCore().GetAccountsDB().GetAccount(username, password);
            if (account != null) {
                if (account.Role <= sender.Role) {
                    var response = new Response(Channel.Private);
                    response.SetMessage(AuthStrings.Login_BetterRoleWarning);
                    return response;
                }
                else {
                    sender.Account = account;
                    var response = new Response(Channel.Private);
                    response.SetMessage(AuthStrings.Login_Ok, account.Role);
                    return response;
                }
            }
            else {
                var response = new Response(Channel.Private);
                response.SetError(AuthStrings.Login_InvalidLoginPass);
                return response;
            }
        }
    }
}

