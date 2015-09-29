//
//  File: IPluginManager.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using NLog;

namespace Api {
    public interface IPluginManager<T> {
        ICore GetCore();
        Logger GetLogger(T plugin);
    }
}

