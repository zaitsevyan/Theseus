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
    public interface ICore {
        IAdapterManager GetAdapterManager();
        IModuleManager GetModuleManager();
        IAccounts GetAccountsDB();

        void Start();
        Task Stop();
        void Wait();
    }
}

