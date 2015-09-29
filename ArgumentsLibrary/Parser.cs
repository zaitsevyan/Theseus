using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ArgumentsLibrary.Exceptions;

namespace ArgumentsLibrary {

    /// <summary>
    /// This class is used for processing command line input arguments.
    /// </summary>
    internal class Parser {

        /// <summary>
        /// Definition of a prefix for a short option, i.e. -o
        /// </summary>
        private const string SHORT_OPTION_PREFIX = "-";

        /// <summary>
        /// Regular expression for detecting a short option, i.e. -o, -option
        /// </summary>
        /// <remarks>
        /// An option is considered short if it starts with short option prefix
        /// </remarks>
        private const string SHORT_OPTION_REGEX = "^-([a-zA-Z][a-zA-Z-_]*)$";

        /// <summary>
        /// Regular expression for detecting a short option alias,
        /// i.e. o, -o, -option
        /// </summary>
        /// <remarks>
        /// A single letter is considered a short option
        /// </remarks>
        private const string SHORT_OPTION_ALIAS_REGEX =
            "^(([a-zA-Z])|-([a-zA-Z][a-zA-Z-_]*))$";

        /// <summary>
        /// Definition of a prefix for a long option, i.e. --option
        /// </summary>
        private const string LONG_OPTION_PREFIX = "--";

        /// <summary>
        /// Definition of a separator of option definition and its value,
        /// i.e. "=" in --option=value
        /// </summary>
        private const string LONG_OPTION_VALUE_SEPARATOR = "=";

        /// <summary>
        /// Regular expression for detecting a long option,
        /// i.e. --o, --option, --option=value
        /// </summary>
        /// <remarks>
        /// An option is considered long if it starts with long option prefix
        /// </remarks>
        private const string LONG_OPTION_REGEX =
            "^--([a-zA-Z][a-zA-Z-_]*)(=.+)?$";

        /// <summary>
        /// Regular expression for detecting a long option alias,
        /// i.e. option, --o, --option
        /// </summary>
        /// <remarks>
        /// An array of at least two letters is considered a long option
        /// </remarks>
        private const string LONG_OPTION_ALIAS_REGEX =
            "^(([a-zA-Z][a-zA-Z-_]+)|--([a-zA-Z][a-zA-Z-_]*))$";

        /// <summary>
        /// Definition of a separator of option aliases,
        /// i.e. "o|option" stands for -o and -option
        /// </summary>
        private const string OPTION_ALIAS_SEPARATOR = "|";

        /// <summary>
        /// Definition of a manual plain command line arguments separator,
        /// when this separator is detected, the rest of the arguments are
        /// considered plain arguments (not options) even though they could
        /// be formatted as options
        /// </summary>
        private const string PLAIN_ARGUMENTS_SEPARATOR = "--";

        /// <summary>
        /// Regex instance of <see cref="SHORT_OPTION_REGEX" />
        /// </summary>
        private static readonly Regex _shortOptionRegex =
            new Regex( SHORT_OPTION_REGEX );

        /// <summary>
        /// Regex instance of <see cref="SHORT_OPTION_ALIAS_REGEX" />
        /// </summary>
        private static readonly Regex _shortOptionAliasRegex =
            new Regex( SHORT_OPTION_ALIAS_REGEX );

        /// <summary>
        /// Regex instance of <see cref="LONG_OPTION_REGEX" />
        /// </summary>
        private static readonly Regex _longOptionRegex =
            new Regex( LONG_OPTION_REGEX );

        /// <summary>
        /// Regex instance of <see cref="LONG_OPTION_ALIAS_REGEX" />
        /// </summary>
        private static readonly Regex _longOptionAliasRegex =
            new Regex( LONG_OPTION_ALIAS_REGEX );

        /// <summary>
        /// CommandLine instance to insert detected options and arguments into
        /// </summary>
        private CommandLine CommandLine { get; set; }

        /// <summary>
        /// Input command line arguments to process
        /// </summary>
        private Queue<string> Args { get; set; }

        /// <summary>
        /// Converter instance to convert string arguments to various types
        /// </summary>
        private Converter Converter { get; set; }

        /// <summary>
        /// Definition of setup Options
        /// </summary>
        private Dictionary<OptionAlias, Option> Options { get; set; }

        /// <summary>
        /// Private constructor used for parsing command line arguments
        /// </summary>
        /// <param name="converter">Converter instance</param>
        /// <param name="options">Definition of setup Options</param>
        private Parser( IEnumerable<string> args, Converter converter,
            Dictionary<OptionAlias, Option> options ) {
            CommandLine = new CommandLine( converter );
            Args = new Queue<string>( args );
            Converter = converter;
            Options = options;
        }

        /// <summary>
        /// Class entry point for parsing command line arguments. It accepts
        /// arguments, user definition of Options, user definition of Converter
        /// and uses this information to process the command line arguments
        /// and outputs a CommandLine instance with detected options, their
        /// values and plain arguments
        /// </summary>
        /// <param name="args">Command line input arguments</param>
        /// <param name="converter">Converter instance</param>
        /// <param name="options">Definition of setup Options</param>
        /// <returns>
        /// CommandLine instance with detected options and arguments
        /// </returns>
        internal static CommandLine ParseArguments( IEnumerable<string> args,
            Converter converter, Dictionary<OptionAlias, Option> options ) {
            var parser = new Parser( args, converter, options );
            parser.ProcessArguments();
            return parser.CommandLine;
        }

        /// <summary>
        /// Detects types of Option aliases separated by separator
        /// <see cref="OPTION_ALIAS_SEPARATOR" />
        /// </summary>
        /// <remarks>
        /// For details on parsing aliases see <see cref="ParseAlias" />
        /// </remarks>
        /// <param name="aliases">User specified aliases</param>
        /// <returns>Set of detected OptionAlias instances</returns>
        internal static IEnumerable<OptionAlias> ParseAliases( string aliases ) {
            if ( aliases == null ) {
                throw new ArgumentNullException( "aliases" );
            }

            return aliases
                .Split( OPTION_ALIAS_SEPARATOR.ToCharArray() )
                .Select( ParseAlias ).ToList();
        }

        /// <summary>
        /// Detects type of the Option alias and removes its prefix if present.
        /// </summary>
        /// <example>
        /// Alias = "v", OptionType = Short: <br />
        /// <code>
        /// var optionAlias1 = Parser.ParseAlias("v"); <br />
        /// var optionAlias2 = Parser.ParseAlias("-v"); <br />
        /// </code>
        /// Alias = "verbose", OptionType = Long: <br />
        /// <code>
        /// var optionAlias3 = Parser.ParseAlias("verbose"); <br />
        /// var optionAlias4 = Parser.ParseAlias("--verbose"); <br />
        /// </code>
        /// Alias = "v", OptionType = Long: <br />
        /// <code>
        /// var optionAlias5 = Parser.ParseAlias("--v"); <br />
        /// </code>
        /// Alias = "verbose", OptionType = Short: <br />
        /// <code>
        /// var optionAlias6 = Parser.ParseAlias("-verbose"); <br />
        /// </code>
        /// </example>
        /// <param name="alias">User specified alias</param>
        /// <returns>OptionAlias with alias and its type</returns>
        internal static OptionAlias ParseAlias( string alias ) {
            OptionType type;
            string realAlias;
            if ( _shortOptionAliasRegex.IsMatch( alias ) ) {
                type = OptionType.Short;
                realAlias = RemoveOptionalPrefix( alias, SHORT_OPTION_PREFIX );
            }
            else if ( _longOptionAliasRegex.IsMatch( alias ) ) {
                type = OptionType.Long;
                realAlias = RemoveOptionalPrefix( alias, LONG_OPTION_PREFIX );
            }
            else {
                throw new ArgumentException(
                    "Input alias is not in valid format" );
            }

            return new OptionAlias( realAlias, type );
        }

        /// <summary>
        /// Inserts a "is set" marker to the CommandLine for input Option
        /// </summary>
        /// <param name="option">Option that is detected in arguments</param>
        private void SetCommandLineOptionIsSet( Option option ) {
            option.Aliases.ForEach( alias => {
                if ( !CommandLine.Options.ContainsKey( alias ) ) {
                    CommandLine.Options[ alias ] = null; // is set indicator
                }
            } );
        }

        /// <summary>
        /// Inserts a value for specified Option to the CommandLine
        /// </summary>
        /// <param name="option">Detected Option</param>
        /// <param name="value">Option value</param>
        private void SetCommandLineOptionValue( Option option, object value ) {
            option.Aliases.ForEach(
                alias => { CommandLine.Options[ alias ] = value; } );
        }

        /// <summary>
        /// Inserts a plain argument to the CommandLine
        /// </summary>
        /// <param name="value">Plain argument value</param>
        private void AddCommandLinePlainArgument( string value ) {
            CommandLine.PlainArguments.Add( value );
        }

        /// <summary>
        /// Processes input command line arguments, i.e. detects Options and
        /// their values, invokes user specified callbacks, detects plain
        /// arguments and constructs a inputs this information to the
        /// CommandLine object
        /// </summary>
        private void ProcessArguments() {
            while ( Args.Any() ) {
                if ( DetectPlainArgumentsSeparator() ) {
                    // The rest of the arguments are plain arguments except
                    // the first one, which is the detected separator
                    Args.Dequeue();
                    // Add all arguments to the CommandLine
                    Args.ToList().ForEach( AddCommandLinePlainArgument );
                    Args.Clear();
                    // Finish the cycle as there are no more args to process
                    break;
                }

                // Check if the next argument could be an Option
                var optionAlias = DetectPotentialOption();

                if ( optionAlias == null ) {
                    // All remaining arguments should be plain arguments
                    AddCommandLinePlainArgument( Args.Dequeue() );
                }
                else if ( !Options.ContainsKey( optionAlias ) ) {
                    // Unknown option was detected
                    throw new ParseException(
                        "Unknown program option detected: {0}",
                        optionAlias.ToString() );
                }
                else if ( CommandLine.PlainArguments.Any() ) {
                    // There were already some plain arguments detected
                    throw new ParseException(
                        "Unexpected option detected in plain arguments: {0}",
                        optionAlias.ToString() );
                }
                else {
                    // Detected option and correct placement in input arguments
                    ProcessDetectedOption( optionAlias );
                }
            }

            CheckMandatoryOptionsPresent();
        }

        /// <summary>
        /// Checks if the next argument is plain arguments separator,
        /// i.e. if it equals to <see cref="PLAIN_ARGUMENTS_SEPARATOR" />
        /// </summary>
        /// <returns>
        /// True if next argument in Args is a plain arguments separator
        /// </returns>
        private bool DetectPlainArgumentsSeparator() {
            return Args.First().Equals( PLAIN_ARGUMENTS_SEPARATOR );
        }

        /// <summary>
        /// Decides whether the next argument is an Option or not,
        /// i.e. whether next argument string is in format "-o" or "--option=*"
        /// </summary>
        /// <returns>OptionAlias of detected potential Option or null</returns>
        private OptionAlias DetectPotentialOption() {
            var arg = Args.First();
            var longMatch = _longOptionRegex.Match( arg );
            if ( longMatch.Success ) {
                return new OptionAlias( longMatch.Groups[ 1 ].Value,
                    OptionType.Long );
            }
            var shortMatch = _shortOptionRegex.Match( arg );
            if ( shortMatch.Success ) {
                return new OptionAlias( shortMatch.Groups[ 1 ].Value,
                    OptionType.Short );
            }
            return null;
        }

        /// <summary>
        /// Processes detected Option, i.e. invokes user specified callbacks
        /// and checks potential Option arguments
        /// </summary>
        /// <param name="optionAlias">Option specified by its alias</param>
        private void ProcessDetectedOption( OptionAlias optionAlias ) {
            // locate corresponding Option
            var option = Options[ optionAlias ];
            // set default value to null
            SetCommandLineOptionIsSet( option );
            // invoke user callbacks
            option.InvokeActions();

            if ( optionAlias.Type == OptionType.Long ) {
                ProcessLongOptionArgument( optionAlias, option );
            }
            else {
                ProcessShortOptionArgument( optionAlias, option );
            }
        }

        /// <summary>
        /// Parse and process long option argument.
        /// </summary>
        /// <param name="optionAlias">Option alias.</param>
        /// <param name="option">Option.</param>
        private void ProcessLongOptionArgument( OptionAlias optionAlias,
            Option option ) {
            var stringValue = ExtractLongOptionValue( Args.First() );

            if ( stringValue.Length > 0 && option.Argument == null ) {
                // There is an Option value present, but it should not be there
                throw new ParseException(
                    "Unexpected long option value: {0}", Args.First() );
            }

            if ( stringValue.Length == 0 && option.Argument != null ) {
                // An argument or default value is expected
                if ( !option.Argument.Optional ) {
                    // Argument is mandatory and no value is provided
                    throw new ParseException(
                        "No option value specified for option {0}",
                        Args.First() );
                }
                option.Argument.InvokeActions( option.Argument.DefaultValue );
                SetCommandLineOptionValue( option, option.Argument.DefaultValue );
            }
            else if ( stringValue.Length > 0 && option.Argument != null ) {
                // An argument is expected and its value is set
                dynamic value;
                try {
                    value = option.Argument
                        .Parse( stringValue, Converter );
                    option.Argument.AssertConditions( value );
                }
                catch ( ArgumentOutOfRangeException ) {
                    // Value does not satisfy Argument Conditions
                    throw new ParseException(
                        "Value for option {0} does not meet conditions",
                        optionAlias.ToString() );
                }
                catch ( Exception ) {
                    // Value cannot be converted to specified format
                    throw new ParseException(
                        "Value for option {0} is not of required type",
                        optionAlias.ToString() );
                }
                option.Argument.InvokeActions( value );
                SetCommandLineOptionValue( option, value );
            }

            // Remove processed option from arguments list
            Args.Dequeue();
        }

        /// <summary>
        /// Parse and process short option argument
        /// </summary>
        /// <param name="optionAlias">Option alias.</param>
        /// <param name="option">Option.</param>
        private void ProcessShortOptionArgument( OptionAlias optionAlias,
            Option option ) {
            // Remove processed option from arguments list
            Args.Dequeue();

            if ( option.Argument == null ) {
                // No argument specified for the Option
                return;
            }

            var stringValue = Args.Any() ? Args.First() : "";

            // An argument is expected and its value is set
            dynamic value = null;
            string exceptionMessage = null;
            try {
                if ( stringValue.Length == 0 ) {
                    throw new ArgumentNullException();
                }
                value = option.Argument
                    .Parse( stringValue, Converter );
                option.Argument.AssertConditions( value );
            }
            catch ( ArgumentNullException ) {
                // Argument is mandatory and no value is provided
                exceptionMessage =
                    "No option value specified for option {0}";
            }
            catch ( ArgumentOutOfRangeException ) {
                // Value does not satisfy Argument Conditions
                exceptionMessage =
                    "Value for option {0} does not meet conditions";
            }
            catch ( Exception ) {
                // Value cannot be converted to specified format
                exceptionMessage =
                    "Value for option {0} is not of required type";
            }

            if ( exceptionMessage != null ) {
                if ( option.Argument.DefaultValueIsSet ) {
                    // Provided value is not valid and a default value exists
                    value = option.Argument.DefaultValue;
                }
                else if ( !option.Argument.Optional ) {
                    // Provided value is not valid and it is mandatory
                    throw new ParseException(
                        exceptionMessage, optionAlias.ToString() );
                }
            }
            else {
                // Remove correct Option argument from the argument list
                Args.Dequeue();
            }

            option.Argument.InvokeActions( value );
            SetCommandLineOptionValue( option, value );
        }

        /// <summary>
        /// Checks if all mandatory options are present in the CommandLine
        /// </summary>
        /// <exception cref="ParseException">
        /// Thrown when at least one mandatory option is not set
        /// </exception>
        private void CheckMandatoryOptionsPresent() {
            var mandatoryOptions =
                Options.Values.Distinct().Where( option => option.Mandatory );
            var allMandatoryPresent = mandatoryOptions.All( option => {
                return CommandLine.Options.ContainsKey( option.Aliases.First() );
            } );
            if ( !allMandatoryPresent ) {
                throw new ParseException(
                    "Not all mandatory options are present" );
            }
        }

        /// <summary>
        /// Divide string into option alias and argument value
        /// </summary>
        /// <returns>The long option value.</returns>
        /// <param name="arg">Option string.</param>
        private static string ExtractLongOptionValue( string arg ) {
            var longMatch = _longOptionRegex.Match( arg );
            return RemoveOptionalPrefix( longMatch.Groups[ 2 ].Value,
                LONG_OPTION_VALUE_SEPARATOR );
        }

        /// <summary>
        /// Removes a prefix from the beginning of a string, if any
        /// </summary>
        /// <param name="value">String to be shortened</param>
        /// <param name="prefix">Prefix to cut</param>
        /// <returns>String without the prefix</returns>
        private static string RemoveOptionalPrefix( string value, string prefix ) {
            return value.StartsWith( prefix )
                ? value.Remove( 0, prefix.Length )
                : value;
        }

    }

}