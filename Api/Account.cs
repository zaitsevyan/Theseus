//
//  File: Account.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    public class Account {

        public String Name { get; private set;}
        public Role Role { get; private set;}

        public String ID { get; private set;}
        private String Password { get; set;}


        public Account(String name, Role role, String id = null, String password = null) {
            if (id == null)
                id = name;
            Name = name;
            Role = role;
            ID = id;
            Password = password;
        }

        public bool CanAuthWith(String id, String password){
            return ID == id && Password == password;
        }

        public override string ToString(){
            return string.Format("{0}<{1}>", Name, ID);
        }
    }
}

