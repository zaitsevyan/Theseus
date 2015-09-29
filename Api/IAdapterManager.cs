//
//  File: IAdapterManager.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Threading.Tasks;

namespace Api {
    /// <summary>
    /// Adapter manager interface
    /// </summary>
    public interface IAdapterManager : IPluginManager<Adapter> {
        /// <summary>
        /// Start specified request processing. It is asynchronous.
        /// </summary>
        /// <param name="adapter">Invoking adapter.</param>
        /// <param name="request">Request.</param>
        void Process(Adapter adapter, Request request);
    }
}

