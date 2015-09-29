//
//  File: Response.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    public enum ResponseStatus {
        OK,
        Error
    }

    public class Response {
        public readonly Channel Channel;

        public ResponseStatus Status { get; private set; }

        public bool IsError { get { return Status == ResponseStatus.Error; } }

        public String Message { get { return IsError ? "" : data; } }

        public String Error { get { return IsError ? data : ""; } }

        public String Info { get { return IsError ? "Error: " + Error : Message;
            } }

        public bool IsEmpty { get { return data == null || data.Length == 0; } }

        private String data;

        public Response(Channel channel) {
            Channel = channel;
            data = null;
            Status = ResponseStatus.OK;
        }

        public void SetMessage(String format, params Object[] args){
            data = String.Format(format, args);
            data = data.Trim(' ','\n','\r','\t');
            Status = ResponseStatus.OK;
        }

        public void SetError(String format, params Object[] args){
            data = String.Format(format, args);
            data = data.Trim(' ','\n','\r','\t');
            Status = ResponseStatus.Error;
        }

        public override string ToString(){
            return string.Format("[Response {0} to {1} channel]", Info, Channel);
        }
    }
}

