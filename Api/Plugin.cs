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
    public class Plugin {

        public readonly String Name;
        public readonly Version Version;
        public readonly Dictionary<String, Object> Config;
        public bool IsRunning { get; private set; }
        public Thread MainLoopThread { get; private set; }

        public Plugin(String name, Dictionary<String, Object> config) {
            Name = name;
            Version = this.GetType().Assembly.GetName().Version;
            Config = config ?? new Dictionary<string, object>();
        }

        public virtual void Start(CancellationToken token) {
            IsRunning = true;
            MainLoopThread = Thread.CurrentThread;
        }

        public virtual void Finish() {
            IsRunning = false;
        }

        public override string ToString(){
            return string.Format("[Plugin: IsRunning={0}, Name={1}, Version={2}]", IsRunning, Name, Version);
        }
    }
}

