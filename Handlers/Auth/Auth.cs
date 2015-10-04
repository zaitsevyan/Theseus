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

        [Command("whoami", "", "Print your name and role")]
        [Roles(Role.Ignore)]
        public Task<Response> WhoAmI(Sender sender, String[] args){
            var response = new Response(Channel.Same);
            response.SetMessage("You are {0} with {1} role", sender.Account, sender.Role);
            return Task.FromResult(response);
        }

        [Command("login", "<username> <password>", "Login as another user")]
        [Roles(Role.Ignore)]
        public async Task<Response> Login(Sender sender, String[] args){
            if (args.Length != 2) {
                var response = new Response(Channel.Private);
                response.SetError("Incorrect options, /login :username :password");
                return response;
            }

            var username = args[0];
            var password = args[1];
            Account account = await Manager.GetCore().GetAccountsDB().GetAccount(username, password);
            if (account != null) {
                if (account.Role <= sender.Role) {
                    var response = new Response(Channel.Private);
                    response.SetMessage("Your current role is better than authorized!");
                    return response;
                }
                else {
                    sender.Account = account;
                    var response = new Response(Channel.Private);
                    response.SetMessage("You are logger as {0} now.", account.Role);
                    return response;
                }
            }
            else {
                var response = new Response(Channel.Private);
                response.SetError("Incorrect username or/and password!");
                return response;
            }
        }
    }
}

