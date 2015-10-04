//
//  File: ICore.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using NLog;
using System.Threading.Tasks;

namespace Api {
    /// <summary>
    /// Platform core interface
    /// </summary>
    public interface ICore {

        /// <summary>
        /// Gets the adapter manager.
        /// </summary>
        /// <returns>The adapter manager.</returns>
        IAdapterManager GetAdapterManager();

        /// <summary>
        /// Gets the handler manager.
        /// </summary>
        /// <returns>The handler manager.</returns>
        IHandlerManager GetHandlerManager();

        /// <summary>
        /// Gets the accountsDB subsystem.
        /// </summary>
        /// <returns>The accounts subsystem.</returns>
        IAccounts GetAccountsDB();

        /// <summary>
        /// Start platform.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop platform.
        /// </summary>
        Task Stop();

        /// <summary>
        /// Wait until all plugins is ended. Yous should call Stop() to initiate shutdowning.
        /// </summary>
        void Wait();
    }
}

