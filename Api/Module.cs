//
//  File: Module.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Collections.Generic;

namespace Api {
    /// <summary>
    /// Command processor base class
    /// </summary>
    public class Module : Plugin {

        /// <summary>
        /// Current <see cref="Api.IModuleManager"/> manager.
        /// </summary>
        public readonly IModuleManager Manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Module"/> class.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="config">Module configuration. See <see cref="Theseus.Configuration"/> class.</param>
        /// <param name="manager">Responsible manager.</param>
        public Module(String name, Dictionary<String, Object> config, IModuleManager manager)
            : base(name, config) {
            Manager = manager;
        }
    }
}

