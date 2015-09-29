//
//  File: RolesAttribute.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Collections.Generic;

namespace Api {
    /// <summary>
    /// Roles attribute. You should define it for your own commands. See <see cref="Api.CommandAttribute"/> too.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RolesAttribute : Attribute {

        /// <summary>
        /// Set of allowed roles.
        /// </summary>
        private HashSet<Role> allowedRoles = new HashSet<Role>();

        /// <summary>
        /// Set of excepted roles.
        /// </summary>
        private HashSet<Role> exceptedRoles = new HashSet<Role>();

        /// <summary>
        /// The minimal allowed role.
        /// </summary>
        private Role minAllowedRole;

        public RolesAttribute(Role minAccepted = Role.Admin, 
                              Role[] allowed = null, 
                              Role[] excepted = null) {
            minAllowedRole = minAccepted;
            if (allowed != null)
                foreach (var role in allowed) {
                    allowedRoles.Add(role);
                }
            if (excepted != null)
                foreach (var role in excepted) {
                    exceptedRoles.Add(role);
                }
        }

        /// <summary>
        /// Determines whether specified role is allowed with current rules.
        /// </summary>
        /// <returns><c>true</c> if current rules allowed the specified role,<c>false</c> otherwise.</returns>
        /// <param name="role">Role.</param>
        public bool IsRoleAllowed(Role role){
            bool allowed = role >=
                minAllowedRole || allowedRoles.Contains(role);
            allowed &= !exceptedRoles.Contains(role);
            return allowed;
        }
    }
}

