//
//  File: Account.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    /// <summary>
    /// Account record for identifying command <see cref="Api.Sender"/> senders
    /// </summary>
    public class Account {

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>Visibe name.</value>
        public String Name { get; private set;}

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <value>Account permissions group</value>
        public Role Role { get; private set;}

        /// <summary>
        /// Gets the ID
        /// </summary>
        /// <value>Account unique identifier.</value>
        public String ID { get; private set;}

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>Account password</value>
        private String Password { get; set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Account"/> class.
        /// </summary>
        /// <param name="name">Visible Name.</param>
        /// <param name="role">Permissions group.</param>
        /// <param name="id">Account identifier. If is null, it will be same as name</param>
        /// <param name="password">Account password</param>
        public Account(String name, Role role, String id = null, String password = null) {
            if (id == null)
                id = name;
            Name = name;
            Role = role;
            ID = id;
            Password = password;
        }

        /// <summary>
        /// Determines whether this account could be authorized with the specified identifier and password.
        /// </summary>
        /// <returns><c>true</c> if this instance can auth with the specified ID and password; otherwise, <c>false</c>.</returns>
        /// <param name="id">Account identifier.</param>
        /// <param name="password">Account Password.</param>
        public bool CanAuthWith(String id, String password){
            return ID == id && String.Equals(Password, password, StringComparison.CurrentCulture);
        }

        public override string ToString(){
            return string.Format("{0}<{1}>", Name, ID);
        }
    }
}

