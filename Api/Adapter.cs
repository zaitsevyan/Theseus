//
//  File: Adapter.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Api {
    /// <summary>
    /// Communication adapter base class
    /// </summary>
    public class Adapter : Plugin {

        /// <summary>
        /// Current <see cref="Api.IAdapterManager"/> manager
        /// </summary>
        public readonly IAdapterManager Manager;

        /// <summary>
        /// Gets or sets the adapter destination locale.
        /// </summary>
        /// <value>The culture.</value>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Api.Adapter"/> class.
        /// </summary>
        /// <param name="name">Adapter name.</param>
        /// <param name="config">Adapter configuration. See <see cref="Theseus.Configuration"/> class</param>
        /// <param name="manager">Responsible manager.</param>
        public Adapter(String name, Dictionary<String, Object> config, IAdapterManager manager)
            : base(name, config) {
            Manager = manager;
        }

        /// <summary>
        /// Process the response.
        /// </summary>
        /// <param name="request">Initial request.</param>
        /// <param name="response">Processed response.</param>
        /// <summary>It will not be called, if response is null or empty</summary>
        public virtual void Process(Request request, Response response){
        }
    }
}

