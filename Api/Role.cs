//
//  File: SenderRole.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;

namespace Api {
    /// <summary>
    /// Permission roles
    /// </summary>
    public enum Role {
        /// <summary>
        /// Lowest group. Usually this group is not allowed by most of commands.
        /// </summary>
        Ignore,
        /// <summary>
        /// Default permissions group
        /// </summary>
        Normal,
        /// <summary>
        /// Group with more permissions than Normal
        /// </summary>
        Moderator,
        /// <summary>
        /// Group with more permissions than Moder
        /// </summary>
        Admin,
        /// <summary>
        /// The application owner. It doesn't mean, that he can use any commands. Commands could define role rules with exceptions.
        /// </summary>
        Owner
    }
}

