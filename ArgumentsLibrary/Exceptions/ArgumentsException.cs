using System;

namespace ArgumentsLibrary.Exceptions {

    /// <summary>
    /// This is a base layer exception for Arguments library exceptions
    /// </summary>
    public abstract class ArgumentsException : Exception {

        internal ArgumentsException( string message, params object[] args )
            : base( args == null ? message : String.Format( message, args ) ) {
        }

    }

}