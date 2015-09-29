using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArgumentsLibrary {

    /// <summary>
    /// Help text generator.
    /// </summary>
    internal class HelpTextGenerator {

        /// <summary>
        /// Default usage string to be used in help text header
        /// </summary>
        private const string DEFAULT_USAGE = "app [options] [[<argument>]...]";

        /// <summary>
        /// Usage description label
        /// </summary>
        private const string USAGE_HEADER = "Usage: ";

        /// <summary>
        /// Options description label
        /// </summary>
        private const string OPTIONS_HEADER = "Options:";

        /// <summary>
        /// Separator between option aliases
        /// </summary>
        private const string OPTION_ALIASES_SEPARATOR = ", ";

        /// <summary>
        /// Format of an argument, i.e. &lt;FILE&gt; for argument FILE
        /// </summary>
        private const string ARGUMENT_NAME_FORMAT = "<{0}>";

        /// <summary>
        /// Outer format of an optional argument, i.e. [=&lt;FILE&gt;]
        /// </summary>
        private const string OPTIONAL_ARGUMENT_FORMAT = "[{0}]";

        /// <summary>
        /// Separator of a short option and its argument
        /// </summary>
        private const string SHORT_ARGUMENT_SEPARATOR = " ";

        /// <summary>
        /// Separator of a long option and its argument
        /// </summary>
        private const string LONG_ARGUMENT_SEPARATOR = "=";

        /// <summary>
        /// Newline character
        /// </summary>
        private const char NEWLINE = '\n';

        /// <summary>
        /// Whitespace character
        /// </summary>
        private const char WHITESPACE = ' ';

        /// <summary>
        /// Minimum space between option signature and its description
        /// </summary>
        private const int MINIMUM_DESCRIPTION_INDENT = 3;

        /// <summary>
        /// Maximum offset of an option description from the beginning of line
        /// </summary>
        private const int MAXIMUM_DESCRIPTION_OFFSET = 30;

        /// <summary>
        /// Length of each line
        /// </summary>
        private const int LINE_LENGTH = 80;

        /// <summary>
        /// Contains generated help text
        /// </summary>
        private StringBuilder Builder { get; set; }

        /// <summary>
        /// Options to provide help text for
        /// </summary>
        private List<Option> Options { get; set; }

        /// <summary>
        /// Calculated maximum description offset based on length of options
        /// signatures
        /// </summary>
        private int DescriptionOffset { get; set; }

        /// <summary>
        /// Private constructor, generates the help text using input Options
        /// and specified usage parameter
        /// </summary>
        /// <param name="options">Options to generate help text for</param>
        /// <param name="usage">Usage string of the program</param>
        private HelpTextGenerator(
            Dictionary<OptionAlias, Option> options, string usage ) {
            Builder = new StringBuilder();
            Options = options.Values.Distinct().ToList();

            if ( usage == null ) {
                usage = DEFAULT_USAGE;
            }

            PrintHeader( usage );

            if ( Options.Any() ) {
                DescriptionOffset = CalculateDescriptionOffset();
                PrintOptionsHeader();
                PrintOptionDefinitions();
            }

            PrintFooter();
        }

        /// <summary>
        /// Generate help text for all options
        /// </summary>
        /// <param name="options">
        /// Dictionary of Options indexed by their OptionAliases
        /// </param>
        /// <param name="usage">
        /// Usage string to be included in help text header
        /// </param>
        internal static string Generate( 
            Dictionary<OptionAlias, Option> options, string usage ) {
            var generator = new HelpTextGenerator( options, usage );
            return generator.Builder.ToString();
        }

        /// <summary>
        /// Generates the usage header
        /// </summary>
        /// <param name="usage">User specified usage string</param>
        private void PrintHeader( string usage ) {
            Builder.Append( USAGE_HEADER );
            Builder.AppendLine( usage );
        }

        /// <summary>
        /// Generates the header for options
        /// </summary>
        private void PrintOptionsHeader() {
            Builder.AppendLine( OPTIONS_HEADER );
        }

        /// <summary>
        /// Generates the footer
        /// </summary>
        private void PrintFooter() {
            Builder.AppendLine();
        }

        /// <summary>
        /// Prints help text for all options
        /// </summary>
        private void PrintOptionDefinitions() {
            Options.ForEach( PrintOptionDefinition );
        }

        /// <summary>
        /// Calculates an ideal description offset from the beginning of each
        /// line in help text based on length of option aliases so that the
        /// final text is justified
        /// </summary>
        /// <returns>
        /// Number of characters to prepend to each description line
        /// </returns>
        private int CalculateDescriptionOffset() {
            var locatedMaximum = Options
                .Select( option => GenerateOptionSignature( option ).Length )
                .Max() + MINIMUM_DESCRIPTION_INDENT;
            return Math.Min( locatedMaximum, MAXIMUM_DESCRIPTION_OFFSET );
        }

        /// <summary>
        /// Prints option aliases and its description
        /// </summary>
        /// <param name="option">Option to print the help text for</param>
        private void PrintOptionDefinition( Option option ) {
            var signature = GenerateOptionSignature( option );
            Builder.Append( signature );
            PrintOptionDescription( option, signature.Length );
        }

        /// <summary>
        /// Generates a signature for an option, i.e. list of option aliases
        /// with argument indications included
        /// </summary>
        /// <param name="option">Option to create a signature for</param>
        /// <returns>Signature of the option</returns>
        private string GenerateOptionSignature( Option option ) {
            var partialSignatures = option.Aliases.Select(
                alias => GeneratePartialOptionSignature( option, alias )
                );

            return String.Join( OPTION_ALIASES_SEPARATOR, partialSignatures );
        }

        /// <summary>
        /// Generates a signature for a single option alias
        /// </summary>
        /// <param name="option">
        /// Option to create a partial signature for
        /// </param>
        /// <param name="alias">Particular option alias</param>
        /// <returns>Signature for a single option alias</returns>
        private string GeneratePartialOptionSignature( Option option,
            OptionAlias alias ) {
            return alias + GenerateOptionArgument( option, alias.Type );
        }

        /// <summary>
        /// Generates an argument part of a partial option signature
        /// </summary>
        /// <param name="option">Parent option</param>
        /// <param name="type">Short or Long option type</param>
        /// <returns></returns>
        private string GenerateOptionArgument( Option option, OptionType type ) {
            if ( option.Argument == null ) {
                return "";
            }

            // Combine a prefix with the Argument name
            var prefix = type == OptionType.Short
                ? SHORT_ARGUMENT_SEPARATOR
                : LONG_ARGUMENT_SEPARATOR;
            var signature = prefix
                + String.Format( ARGUMENT_NAME_FORMAT, option.Argument.Name );

            // Apply special formatting if the Argument is optional
            if ( option.Argument.Optional ) {
                signature = String.Format( OPTIONAL_ARGUMENT_FORMAT, signature );
            }

            return signature;
        }

        /// <summary>
        /// Prints option description to a justified block of text
        /// </summary>
        /// <param name="option">Option to print a description for</param>
        /// <param name="offset">Length of the option signature</param>
        private void PrintOptionDescription( Option option, int offset ) {
            if ( offset > DescriptionOffset ) {
                // Option signature is longer than desired description offset
                Builder.AppendLine();
                offset = 0;
            }

            // Split description into paragraphs and print them independently
            option.Description.Split( NEWLINE )
                .ToList()
                .ForEach( description => {
                    PrintDescriptionParagraph( description, offset );
                    offset = 0; // reset offset for additional paragraphs
                } );
        }

        /// <summary>
        /// Prints a paragraph of option description as a justified block of text
        /// </summary>
        /// <param name="line">Description line to process</param>
        /// <param name="lineOffset">
        /// Offset from the beginning of the line in output
        /// </param>
        private void PrintDescriptionParagraph( string line, int lineOffset ) {
            // Real possible length of single description line
            var availableSpace = LINE_LENGTH - DescriptionOffset;

            // Pad any already written characters specified by the line offset
            // to a desired description offset
            Builder.Append( String.Empty
                .PadRight( DescriptionOffset - lineOffset ) );

            if ( line.Length <= availableSpace ) {
                // The paragraph ends here
                Builder.AppendLine( line );
                return;
            }

            // Calculate the amount of text that can be appended to the line
            var chunk = line.Substring( 0, availableSpace + 1 );
            var chunkCut = chunk.LastIndexOf( WHITESPACE );
            var restCut = chunkCut + 1;
            if ( chunkCut == -1 ) {
                // No whitespace detected, we need to break and existing word
                chunkCut = availableSpace;
                restCut = chunkCut;
            }

            // Append the calculated part of the paragraph
            Builder.AppendLine( chunk.Substring( 0, chunkCut ) );
            // Process the remainder
            PrintDescriptionParagraph( line.Substring( restCut ), 0 );
        }

    }

}