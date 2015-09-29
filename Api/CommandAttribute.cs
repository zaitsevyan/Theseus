//
//  File: CommandAttribute.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    
    /// <summary>
    /// Command attribute. You should define it for your own commands. See <see cref="Api.RolesAttribute"/> too.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute {

        /// <summary>
        /// Command name
        /// </summary>
        public readonly String Name;

        /// <summary>
        /// How to use this command. Shouldn't contains command name at begin. 
        /// It describes additional arguments and options for command
        /// </summary>
        public readonly String Usage;

        /// <summary>
        /// Help text.
        /// </summary>
        public readonly String Description;

        public CommandAttribute(String name, String usage, String description) {
            Name = name;
            Usage = usage ?? "";
            Description = description ?? "";
        }
    }
}

