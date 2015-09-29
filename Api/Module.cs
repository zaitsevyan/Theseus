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
    public class Module : Plugin {

        public readonly IModuleManager Manager;

        public Module(String name, Dictionary<String, Object> config, IModuleManager manager)
            : base(name, config) {
            Manager = manager;
        }
    }
}

