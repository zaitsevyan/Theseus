//
//  File: Response.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    /// <summary>
    /// Response status.
    /// </summary>
    public enum ResponseStatus {
        OK,
        Error
    }

    /// <summary>
    /// Processing response.
    /// </summary>
    public class Response {

        /// <summary>
        /// Delivery channel
        /// </summary>
        public readonly Channel Channel;

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public ResponseStatus Status { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this response contains error message.
        /// </summary>
        /// <value><c>true</c> if this response contains error message; otherwise, <c>false</c>.</value>
        public bool IsError { get { return Status == ResponseStatus.Error; } }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public String Message { get { return IsError ? "" : data; } }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public String Error { get { return IsError ? data : ""; } }

        /// <summary>
        /// Gets the string line.
        /// </summary>
        /// <value>String message. If response contains error, it will be prefixed with "Error:"</value>
        public String Info { get { return IsError ? 
                String.Format(ApiStrings.Response_Error, Error) : 
                String.Format(ApiStrings.Response_Ok, Message); } }

        /// <summary>
        /// Gets a value indicating whether this response is empty.
        /// </summary>
        /// <value><c>true</c> if this response is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty { get { return data == null || data.Length == 0; } }

        /// <summary>
        /// The internal data container.
        /// </summary>
        private String data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Response"/> class.
        /// </summary>
        /// <param name="channel">Delivery channel.</param>
        public Response(Channel channel) {
            Channel = channel;
            data = null;
            Status = ResponseStatus.OK;
        }

        /// <summary>
        /// Sets the message.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public void SetMessage(String format, params Object[] args){
            data = String.Format(format, args);
            data = data.Trim(' ', '\n', '\r', '\t');
            Status = ResponseStatus.OK;
        }

        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public void SetError(String format, params Object[] args){
            data = String.Format(format, args);
            data = data.Trim(' ', '\n', '\r', '\t');
            Status = ResponseStatus.Error;
        }

        public override string ToString(){
            return string.Format("[Response {0} to {1} channel]", Info, Channel);
        }
    }
}

