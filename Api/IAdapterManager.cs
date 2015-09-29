//
//  File: IAdapterManager.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Threading.Tasks;

namespace Api {
    public interface IAdapterManager : IPluginManager<Adapter> {
        void Process(Adapter adapter, Request request);
    }
}

