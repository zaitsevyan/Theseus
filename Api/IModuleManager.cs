//
//  File: IModuleManager.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api {
    public interface IModuleManager : IPluginManager<Module> {
        Task<Response> Process(Request request);
        bool ShouldProcessRequest(Request request);
        List<String> GetAllowedCommands(Sender sender);
        CommandAttribute GetCommandInfo(String command);
        String GetCommandPrefix();
    }
}

