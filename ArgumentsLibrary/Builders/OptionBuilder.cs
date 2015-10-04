using System;
using ArgumentsLibrary.Exceptions;

namespace ArgumentsLibrary.Builders {

    /// <summary>
    /// Fluent interface provider for building Options
    /// </summary>
    public class OptionBuilder {
        #region Internals

        /// <summary>
        /// Option instance to build with this builder
        /// </summary>
        internal Option Option { get; private set; }

        /// <summary>
        /// Action delegate to register Option and its aliases
        /// </summary>
        private Action<Option, String> RegisterAliasesAction { get; set; }

        internal OptionBuilder( Action<Option, String> registerAliasesAction ) {
            if ( registerAliasesAction == null ) {
                throw new ArgumentNullException( "registerAliasesAction" );
            }
            Option = new Option();
            RegisterAliasesAction = registerAliasesAction;
        }

        /// <summary>
        /// Associates and argument with this Option instance
        /// </summary>
        /// <param name="argument">Argument{T} instance</param>
        internal void RegisterArgument( dynamic argument, Type argumentType ) {
            if ( argument == null ) {
                throw new ArgumentNullException( "argument" );
            }

            var expectedType =
                Type.GetType( String.Format( "ArgumentsLibrary.Argument`1[{0}]",
                    argumentType.ToString() ) );

            if (expectedType !=null && argument.GetType() != expectedType) {
                throw new ArgumentException(
                    "Argument object and its type do not match" );
            }

            Option.Argument = argument;
        }

        #endregion

        #region API

        /// <summary>
        /// Sets an indicator whether the Option is mandatory or not.
        /// </summary>
        /// <param name="flag">True if mandatory, False otherwise</param>
        /// <returns></returns>
        public OptionBuilder SetMandatory( bool flag = true ) {
            Option.Mandatory = flag;
            return this;
        }

        /// <summary>
        /// Adds aliases to current Option, same as <see cref="WithAlias" />
        /// </summary>
        /// <param name="aliases">One or more aliases</param>
        /// <returns>OptionBuilder fluent interface</returns>
        public OptionBuilder WithAliases( string aliases ) {
            if ( aliases == null ) {
                throw new SetupException( "Option aliases cannot be null" );
            }
            RegisterAliasesAction( Option, aliases );
            return this;
        }

        /// <summary>
        /// Adds aliases to current Option, same as <see cref="WithAliases" />
        /// </summary>
        /// <param name="alias">One or more aliases</param>
        /// <returns>OptionBuilder fluent interface</returns>
        public OptionBuilder WithAlias( string alias ) {
            return WithAliases( alias );
        }

        /// <summary>
        /// Description of the Option to be used when showing help
        /// </summary>
        /// <param name="description"></param>
        /// <returns>OptionBuilder fluent interface</returns>
        public OptionBuilder WithDescription( string description ) {
            if ( description == null ) {
                throw new SetupException( "Option description cannot be null" );
            }
            Option.Description = description;
            return this;
        }

        /// <summary>
        /// Adds an Action to be called when the Option is detected
        /// among console input arguments. Multiple actions can be specified.
        /// </summary>
        /// <param name="action">
        /// Action to be performed when
        /// the Option is detected among input arguments
        /// </param>
        /// <returns>OptionBuilder fluent interface</returns>
        public OptionBuilder WithAction( Action action ) {
            if ( action == null ) {
                throw new SetupException( "Option action cannot be null" );
            }
            Option.Actions.Add( action );
            return this;
        }

        /// <summary>
        /// Defines the Option Argument with specific type and name.
        /// </summary>
        /// <typeparam name="T">Type of the Option Argument</typeparam>
        /// <param name="name">Name of the argument to be used in help</param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> WithArgument<T>( string name ) {
            return new ArgumentBuilder<T>( RegisterArgument )
                .SetName( name );
        }

        /// <summary>
        /// Same as <see cref="WithArgument{T}" />, implicitly typed as string.
        /// </summary>
        /// <param name="name">Name of the argument to be used in help</param>
        /// <returns>ArgumentBuilder{string} fluent interface</returns>
        public ArgumentBuilder<string> WithArgument( string name ) {
            return WithArgument<string>( name );
        }

        /// <summary>
        /// Defines an optional Argument
        /// Same as <see cref="WithArgument{T}" />,
        /// </summary>
        /// <typeparam name="T">Type of the Option Argument</typeparam>
        /// <param name="name">Name of the argument to be used in help</param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> WithOptionalArgument<T>( string name ) {
            return WithArgument<T>( name ).SetOptional();
        }

        /// <summary>
        /// Same as <see cref="WithOptionalArgument{T}" />, implicitly typed
        /// as string
        /// </summary>
        /// <param name="name">Name of the argument to be used in help</param>
        /// <returns>ArgumentBuilder{string} fluent interface</returns>
        public ArgumentBuilder<string> WithOptionalArgument( string name ) {
            return WithOptionalArgument<string>( name );
        }

        #endregion
    }

}