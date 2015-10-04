//
//  File: AdapterManager.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Theseus {
    /// <summary>
    /// Adapter manager implementation.
    /// </summary>
    public sealed class AdapterManager : PluginManager<Adapter>, IAdapterManager {

        /// <summary>
        /// Initializes a new instance of the <see cref="Theseus.AdapterManager"/> class.
        /// </summary>
        /// <param name="core">Platworm core.</param>
        /// <param name="configs">Adapter configurations. See <see cref="Theseus.Configuration"/>.</param>
        public AdapterManager(ICore core, List<Configuration.Plugin> configs)
            : base(core, configs) {
        }

        /// <summary>
        /// Start specified request processing. It is asynchronous.
        /// </summary>
        /// <param name="adapter">Invoking adapter.</param>
        /// <param name="request">Request.</param>
        public void Process(Adapter adapter, Request request){
            if (!adapter.IsRunning) {
                return;
            }
            if (!Core.GetHandlerManager().ShouldProcessRequest(request)) {
                return;
            }
            Logger.Trace("Processing {0} started", request);
            Task.Factory.StartNew(async delegate() {
                try {
                    var response = await Core.GetHandlerManager().Process(request);
                    InvokeAdapter(adapter, request, response);
                }
                catch (Exception e) {
                    Logger.Error(e);
                    InvokeAdapter(adapter, request, new Response(Channel.Private));
                }
            });
        }

        /// <summary>
        /// Invokes the adapter and send response
        /// </summary>
        /// <param name="adapter">Adapter.</param>
        /// <param name="request">Initial request.</param>
        /// <param name="response">Processed response.</param>
        public void InvokeAdapter(Adapter adapter, Request request, Response response){
            if (response == null) {
                Logger.Trace("Processing {0} ended", request);
            }
            else {
                Logger.Trace("Processing {0} => {1} ended", request, response);
            }
            if (adapter.IsRunning && request!=null && !response.IsEmpty)
                adapter.Process(request, response);
        }
    }
}

