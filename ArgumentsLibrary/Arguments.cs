using System;
using System.Collections.Generic;
using ArgumentsLibrary.Builders;
using ArgumentsLibrary.Exceptions;

namespace ArgumentsLibrary {

    /// <summary>
    /// Arguments Library entry point
    /// </summary>
    /// <example>
    /// <code>
    /// var arguments = new Arguments(); <br/>
    /// arguments.AddOption(...) <br/>
    /// ...  <br/>
    /// var cmd = arguments.parse(args) <br/>
    /// </code>
    /// </example>
    public sealed class Arguments {

        #region Internals

        /// <summary>
        /// Gets or sets the converter.
        /// </summary>
        /// <value>Type converters.</value>
        private Converter Converter { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>Option-Alias dictionary</value>
        private Dictionary<OptionAlias, Option> Options { get; set; }


        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ArgumentsLibrary.Arguments" /> class.
        /// </summary>
        public Arguments() {
            Converter = new Converter();
            Options = new Dictionary<OptionAlias, Option>();
            RegisterDefaultTypeConverters();
        }

        /// <summary>
        /// Registers default type converters
        /// </summary>
        /// <remarks>
        /// List of default types: string, int, float, double, bool
        /// </remarks>
        private void RegisterDefaultTypeConverters() {
            RegisterTypeConverter( string.Copy );
            RegisterTypeConverter( int.Parse );
            RegisterTypeConverter( float.Parse );
            RegisterTypeConverter( double.Parse );
            RegisterTypeConverter( bool.Parse );
        }

        /// <summary>
        /// Registers an option using multiple aliases
        /// </summary>
        /// <param name="option">Option to be registered</param>
        /// <param name="aliases">
        /// List of aliases; see <see cref="Parser.ParseAliases" /> method
        /// </param>
        private void RegisterOptionAliases( Option option, string aliases ) {
            try {
                foreach ( var alias in Parser.ParseAliases( aliases ) ) {
                    RegisterOptionAlias( option, alias );
                }
            }
            catch ( ArgumentException exception ) {
                throw new SetupException( exception.Message );
            }
        }

        /// <summary>
        /// Registers an option using an alias
        /// </summary>
        /// <param name="option">Option to be registered</param>
        /// <param name="alias">
        /// Option alias; see <see cref="OptionAlias" /> class
        /// </param>
        private void RegisterOptionAlias( Option option, OptionAlias alias ) {
            Options.Add( alias, option );
            option.Aliases.Add( alias );
        }

        #endregion

        #region API

        /// <summary>
        /// Registers types to be used as Option or plain arguments, along with
        /// their converter function. The converter function converts input
        /// string to the given type and returns the result.
        /// </summary>
        /// <typeparam name="T">Target converted type</typeparam>
        /// <param name="converterFunc">
        /// Function to convert string to type T
        /// </param>
        public void RegisterTypeConverter<T>( Func<string, T> converterFunc ) {
            if ( converterFunc == null ) {
                throw new SetupException( "Converter function cannot be null" );
            }
            Converter.RegisterTypeConverter( converterFunc );
        }

        /// <summary>
        /// Adds an Option to the current configuration with given aliases.
        /// Aliases is a string comprising of aliases separated by "|".
        /// One lettered alias is implicitly considered a short option,
        /// multiple letters indicate a long option alias. Short and long
        /// indication can be forced by prefixing the aliases with "-"
        /// and "--" respectively.
        /// </summary>
        /// <example>
        /// The following examples do exactly the same thing:
        /// <code>
        /// The following examples do exactly the same thing
        /// Arguments.AddOption("v|verbose"); <br/>
        /// Arguments.AddOption("-v|--verbose"); <br/>
        /// Arguments.AddOption("v").WithAlias("verbose"); <br/>
        /// Arguments.AddOption("-v").WithAlias("--verbose"); <br/>
        /// </code>
        /// <BR />
        /// The following examples present non-standard usage:
        /// <code>
        /// Arguments.AddOption("v|-verbose"); <br/>
        /// Arguments.AddOption("--v").WithAlias("--verbose"); <br/>
        /// </code>
        /// </example>
        /// <param name="aliases">One or more option aliases</param>
        /// <returns>OptionBuilder instance</returns>
        public OptionBuilder AddOption( string aliases ) {
            return new OptionBuilder( RegisterOptionAliases )
                .WithAliases( aliases );
        }

        /// <summary>
        /// Adds a mandatory Option to the current configuration with given
        /// aliases. Same as <see cref="AddOption">AddOption</see>
        /// </summary>
        /// <param name="aliases">One or more option aliases</param>
        /// <returns>OptionBuilder instance</returns>
        public OptionBuilder AddMandatoryOption( string aliases ) {
            return AddOption( aliases ).SetMandatory();
        }

        /// <summary>
        /// Processes the command line input arguments.
        /// </summary>
        /// <param name="args">Standard list of args as passed to the Main</param>
        /// <exception cref="ParseException">
        /// Passed args do not satisfy the definition
        /// </exception>
        public CommandLine Parse( IEnumerable<string> args ) {
            if ( args == null ) {
                throw new ParseException( "Passed arguments cannot be null" );
            }

            return Parser.ParseArguments( args, Converter, Options );
        }

        /// <summary>
        /// Builds help text for all defined options with their descriptions
        /// </summary>
        /// <example>
        /// <code>
        /// var arguments = new Arguments();
        /// ...
        /// Console.Write(arguments.BuildHelpText("app [options] param1");
        /// // Help text will start with following line:
        /// // Usage: app [options] param1
        /// </code>
        /// </example>
        public string BuildHelpText( string usage = null ) {
            return HelpTextGenerator.Generate( Options, usage );
        }

        #endregion
    }

}