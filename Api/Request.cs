//
//  File: Message.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    /// <summary>
    /// Processing request
    /// </summary>
    public class Request {
        /// <summary>
        /// Sender input. It is full line received from sender.
        /// </summary>
        public readonly String Command;

        /// <summary>
        /// Initial sender
        /// </summary>
        public readonly Sender Sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Request"/> class.
        /// </summary>
        /// <param name="sender">Initial sender.</param>
        /// <param name="command">Full command line.</param>
        public Request(Sender sender, String command) {
            Sender = sender;
            Command = command;
        }

        public override string ToString(){
            return string.Format("[Request {0}:'{1}']", Sender, Command);
        }
    }
}

