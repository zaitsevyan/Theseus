using System;
using System.Collections.Generic;
using ArgumentsLibrary.Exceptions;

namespace ArgumentsLibrary {

    /// <summary>
    /// Arguments parse result class.
    /// It gives access to options, which are parsed from args string.
    /// See <see cref="Arguments.Parse" /> method.
    /// </summary>
    public class CommandLine {

        /// <summary>
        /// Gets the converter.
        /// </summary>
        /// <value>Type converters</value>
        internal Converter Converter { get; private set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>Option-Alias dictionary</value>
        internal Dictionary<OptionAlias, object> Options { get; private set; }

        /// <summary>
        /// Gets the plain arguments.
        /// </summary>
        /// <value>The plain arguments.</value>
        internal List<string> PlainArguments { get; private set; }


        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ArgumentsLibrary.CommandLine" /> class
        /// </summary>
        /// <param name="converter">Type converters</param>
        internal CommandLine( Converter converter ) {
            Converter = converter;
            Options = new Dictionary<OptionAlias, object>();
            PlainArguments = new List<string>();
        }

        /// <summary>
        /// Checks whether the user specified an option or not
        /// </summary>
        /// <param name="alias">Option alias</param>
        /// <exception cref="CommandLineException">
        /// Throws when option alias is null or has invalid format
        /// </exception>
        /// <returns>
        /// True if user specified an option or if a default value is defined
        /// </returns>
        public bool IsOptionSet( string alias ) {
            if ( alias == null ) {
                throw new CommandLineException( "Option alias cannot be null" );
            }
            try {
                return Options.ContainsKey( Parser.ParseAlias( alias ) );
            }
            catch ( ArgumentException ) {
                throw new CommandLineException( "Option alias format invalid" );
            }
        }

        /// <summary>
        /// Gets Option argument converted to the specified type.
        /// </summary>
        /// <typeparam name="T">Return type of the value</typeparam>
        /// <param name="alias">One of the Option aliases</param>
        /// <exception cref="CommandLineException">
        /// Thrown when the option with the required alias is not set or when
        /// trying to access a value of a differently typed option argument
        /// </exception>
        /// <returns>Typed Option value</returns>
        public T GetOptionValue<T>( string alias ) {
            if ( !IsOptionSet( alias ) ) {
                throw new CommandLineException(
                    "Option with alias {0} is not set", alias );
            }

            var optionAlias = Parser.ParseAlias( alias );
            try {
                return ( T ) Options[ optionAlias ];
            }
            catch ( Exception ) {
                throw new CommandLineException(
                    "Unable to cast option {0} as type {1}", alias,
                    typeof ( T ).ToString() );
            }
        }

        /// <summary>
        /// Gets Option argument as string. Same as
        /// <see cref="GetOptionValue{T}" />, implicitly typed.
        /// </summary>
        /// <param name="alias">One of the Option aliases</param>
        /// <returns>Option value as string</returns>
        public string GetOptionValue( string alias ) {
            return GetOptionValue<string>( alias );
        }

        /// <summary>
        /// Returns a list of all arguments that do not correspond to Options
        /// as a list of {T}.
        /// </summary>
        /// <exception cref="CommandLineException">
        /// Thrown when it is not possible to convert all plain arguments
        /// to the desired type
        /// </exception>
        /// <returns>List of all plain arguments</returns>
        public IEnumerable<T> GetPlainArguments<T>() {
            try {
                return PlainArguments.ConvertAll( s => Converter.Convert<T>( s ) );
            }
            catch ( Exception ) {
                throw new CommandLineException(
                    "Cannot convert all arguments to type {0}", typeof ( T ) );
            }
        }

        /// <summary>
        /// Implicit alternative to <see cref="GetPlainArguments{T}" />.
        /// Returns a list of all arguments that do not correspond to Options
        /// as a list of strings.
        /// </summary>
        /// <returns>List of all plain arguments</returns>
        public IEnumerable<string> GetPlainArguments() {
            return GetPlainArguments<string>();
        }

    }

}