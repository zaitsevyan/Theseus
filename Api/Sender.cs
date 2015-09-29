//
//  File: ISender.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    public class Sender {
        private Account account;

        public Account Account
        {
            get{ return account; }
            set
            {
                account = value;
                Role = value.Role;
            }
        }

        public Role Role;

        public Sender(Account account) {
            Account = account;
        }

        public override string ToString(){
            return string.Format("{0}-{1}", Account, Role);
        }
    }
}

