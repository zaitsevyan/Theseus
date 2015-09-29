//
//  File: CommandAttribute.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute {
        public readonly String Name;
        public readonly String Usage;
        public readonly String Description;
        public CommandAttribute(String name, String usage, String description) {
            Name = name;
            Usage = usage ?? "";
            Description = description ?? "";
        }
    }
}

