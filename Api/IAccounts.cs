//
//  File: IAccounts.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Threading.Tasks;

namespace Api {
    public interface IAccounts {

        Task<bool> ExistsAccount(String username, String password);
        Task<Account> GetAccount(String username, String password);
    }
}

