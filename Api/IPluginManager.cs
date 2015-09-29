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
    /// <summary>
    /// Plugin manager interface
    /// </summary>
    public interface IPluginManager<T> {
        /// <summary>
        /// Gets the platform core.
        /// </summary>
        /// <returns>The platform core.</returns>
        ICore GetCore();

        /// <summary>
        /// Gets the logger for plugin. See <see cref="NLog.Logger"/>.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <param name="plugin">Plugin.</param>
        Logger GetLogger(T plugin);
    }
}

