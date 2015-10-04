//
//  File: CommandAttribute.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Api {
    
    /// <summary>
    /// Command attribute. You should define it for your own commands. See <see cref="Api.RolesAttribute"/> too.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute {

        /// <summary>
        /// Command name
        /// </summary>
        private String[] names;

        /// <summary>
        /// Gets the normalized localized names.
        /// </summary>
        /// <value>The normalized names.</value>
        public IEnumerable<String> NormalizedNames { get { return from name in Names
                select name.RemoveDiacritics(); } }

        /// <summary>
        /// Gets the localized names.
        /// </summary>
        /// <value>The names.</value>
        public IEnumerable<String> Names { get { return 
                    (from name in names
                              select GetStringLookup(name)); } }

        /// <summary>
        /// How to use this command. Shouldn't contains command name at begin. 
        /// It describes additional arguments and options for command
        /// </summary>
        private String usage;

        public String Usage { get { return GetStringLookup(usage); } private set { usage = value; } }

        /// <summary>
        /// Help note.
        /// </summary>
        private String note;

        public String Note { get { return GetStringLookup(note); } private set { note = value; } }

        public Type ResourceType { get; set; }

        public CommandAttribute(String[] names, String usage, String note) {
            if (names.Length == 0)
                throw new ArgumentException("Names should contains at least one name", "names");
            this.names = names;
            Usage = usage ?? "";
            Note = note ?? "";
        }

        public CommandAttribute(String name, String usage, String note)
            : this(new String[]{ name }, usage, note) {
        }

        public String GetStringLookup(string name){
            if (name == null)
                throw new ArgumentNullException("name");
            
            if (ResourceType == null)
                return name;
            
            PropertyInfo property = ResourceType.GetProperty(name, BindingFlags.Public |
                                        BindingFlags.Static |
                                        BindingFlags.NonPublic);
            if (property == null || property.PropertyType != typeof(String))
                return name;
            return (String)property.GetValue(null, null);
        }

        public String NamesWithPrefix(String prefix){
            return String.Join(", ", from name in Names
                                                 select prefix + name);
        }
    }
}

