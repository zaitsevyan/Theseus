//
//  File: Plugin.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace Api {
    /// <summary>
    /// Plugin base class
    /// </summary>
    public class Plugin {

        /// <summary>
        /// Plugin name
        /// </summary>
        public readonly String Name;
        /// <summary>
        /// Plugin version.
        /// It is equal to Assembly version.
        /// </summary>
        public readonly Version Version;

        /// <summary>
        /// Plugin configuration. See <see cref="Theseus.Configuration"/> class.
        /// </summary>
        public readonly Dictionary<String, Object> Config;

        /// <summary>
        /// Gets a value indicating whether this plugin is running.
        /// </summary>
        /// <value><c>true</c> if this plugin is running; otherwise, <c>false</c>.</value>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the main loop thread at which this plugin was started.
        /// </summary>
        /// <value>The thread.</value>
        public Thread MainLoopThread { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Plugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="config">Plugin configuration. See <see cref="Theseus.Configuration"/> class.</param>
        public Plugin(String name, Dictionary<String, Object> config) {
            Name = name;
            Version = this.GetType().Assembly.GetName().Version;
            Config = config ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Start plugin with specified cancellation token.
        /// If you override this method, you should call base implementation!.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        public virtual void Start(CancellationToken token) {
            IsRunning = true;
            MainLoopThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Finish this plugin.
        /// If cancellation token is cancelled, you should abort your operations and call this method.
        /// </summary>
        public virtual void Finish() {
            IsRunning = false;
        }

        public override string ToString(){
            return string.Format("[Plugin: IsRunning={0}, Name={1}, Version={2}]", IsRunning, Name, Version);
        }
    }
}

