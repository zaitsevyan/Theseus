//
//  File: Adapter.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Collections.Generic;

namespace Api {
    public class Adapter : Plugin {
        
        public readonly IAdapterManager Manager;

        public Adapter(String name, Dictionary<String, Object> config, IAdapterManager manager)
            : base(name, config) {
            Manager = manager;
        }

        public virtual void Process(Request request, Response response) {
        }
    }
}

