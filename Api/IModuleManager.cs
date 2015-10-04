//
//  File: IHandlerManager.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api {
    /// <summary>
    /// Handler manager interface
    /// </summary>
    public interface IHandlerManager : IPluginManager<Handler> {
        /// <summary>
        /// Process the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        Task<Response> Process(Request request);

        /// <summary>
        /// Shoulds the process request.
        /// </summary>
        /// <returns><c>true</c>, if IAdapterManager should process request, <c>false</c> otherwise.</returns>
        /// <param name="request">Request.</param>
        bool ShouldProcessRequest(Request request);

        /// <summary>
        /// Gets the allowed commands.
        /// </summary>
        /// <returns>The allowed commands.</returns>
        /// <param name="sender">Sender. It is used to filter commands by sender permissions role</param>
        List<CommandAttribute> GetAllowedCommands(Sender sender);

        /// <summary>
        /// Gets the information about command.
        /// </summary>
        /// <returns>The command information.</returns>
        /// <param name="command">Command.</param>
        CommandAttribute GetCommandInfo(String command);

        /// <summary>
        /// Gets the command prefix.
        /// It is '/' by default.
        /// </summary>
        /// <returns>The command prefix.</returns>
        String GetCommandPrefix();
    }
}

