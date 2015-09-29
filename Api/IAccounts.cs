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
    /// <summary>
    /// Accounts subsystem. See <see cref="Theseus.Accounts"/>
    /// </summary>
    public interface IAccounts {

        /// <summary>
        /// Checks does account exist.
        /// </summary>
        /// <returns><c>true</c> if account exists, <c>false</c> otherwise.</returns>
        /// <param name="username">Account ID.</param>
        /// <param name="password">Account password.</param>
        Task<bool> ExistsAccount(String ID, String password);

        /// <summary>
        /// Gets the account.
        /// </summary>
        /// <returns>The account if account exists, <c>null</c> otherwis.</returns>
        /// <param name="ID">Account ID</param>
        /// <param name="password">Account Password.</param>
        Task<Account> GetAccount(String ID, String password);
    }
}

