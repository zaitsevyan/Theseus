//
//  File: Handler.cs
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
    public class Handler : Plugin {

        /// <summary>
        /// Current <see cref="Api.IHandlerManager"/> manager.
        /// </summary>
        public readonly IHandlerManager Manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Handler"/> class.
        /// </summary>
        /// <param name="name">Handler name.</param>
        /// <param name="config">Handler configuration. See <see cref="Theseus.Configuration"/> class.</param>
        /// <param name="manager">Responsible manager.</param>
        public Handler(String name, Dictionary<String, Object> config, IHandlerManager manager)
            : base(name, config) {
            Manager = manager;
        }
    }
}

