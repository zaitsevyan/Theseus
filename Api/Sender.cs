//
//  File: ISender.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    /// <summary>
    /// Command sender
    /// </summary>
    public class Sender {
        /// <summary>
        /// Sender identification
        /// </summary>
        private Account account;

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>Sender identification.</value>
        public Account Account
        {
            get{ return account; }
            set
            {
                account = value;
                Role = value.Role;
            }
        }

        /// <summary>
        /// Current permission group
        /// </summary>
        public Role Role;

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Sender"/> class.
        /// </summary>
        /// <param name="account">Sender identification.</param>
        public Sender(Account account) {
            Account = account;
        }

        public override string ToString(){
            return string.Format("{0}-{1}", Account, Role);
        }
    }
}

