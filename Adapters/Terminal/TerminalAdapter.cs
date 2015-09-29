
//
//  File: TerminalAdapter.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Adapters {
    public sealed class TerminalAdapter : Adapter {
        private Sender Sender {get; set; }

        public TerminalAdapter(Dictionary<String, Object> config, IAdapterManager manager)
            : base("Terminal", config, manager) {
            Sender = new Sender(new Account("Developer", Role.Owner));
        }

        public override void Start(CancellationToken token){
            try {
                base.Start(token);
                using (var abort = token.Register(Thread.CurrentThread.Abort)) {
                    while (true) {

                        var command = Console.ReadLine();
                        var message = new Request(Sender, command);
                        Manager.Process(this, message);
                    }
                }
            }
            catch (ThreadAbortException) {
                //It is OK.
            }
            finally {
                Finish();
            }
        }

        public override void Process(Request request, Response response){
            base.Process(request, response);
            Console.WriteLine(response.Info);
        }
    }
}

