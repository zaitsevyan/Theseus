//
//  File: Message.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    public class Request {
        
        public readonly String Command;
        public readonly Sender Sender;

        public Request(Sender sender, String command) {
            Sender = sender;
            Command = command;
        }

        public override string ToString(){
            return string.Format("[Request {0}:'{1}']", Sender, Command);
        }
    }
}

