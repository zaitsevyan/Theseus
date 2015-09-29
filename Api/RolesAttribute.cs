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
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RolesAttribute : Attribute {

        private HashSet<Role> allowedRoles = new HashSet<Role>();
        private HashSet<Role> exceptedRoles = new HashSet<Role>();
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

        public bool IsRoleAllowed(Role role){
            bool allowed = role >=
                minAllowedRole || allowedRoles.Contains(role);
            allowed &= !exceptedRoles.Contains(role);
            return allowed;
        }
    }
}

