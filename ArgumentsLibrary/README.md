README.md
============

**Arguments library** is a tool for efficient processing of command line arguments.

Requirements
============
- .NET

Compilation
============

### Windows ###
Open Visual Studio and Build it

### Mono based ###
A Makefile is included with following targets:

- Build: **make all**
- Build lib: **make lib**
- Build debug lib: **make debug**
- Generate docs: **make doxygen**
- Test: **make test**
- Clean: **make clean**

API
============

The API for this library comprises following classes:
- ArgumentsLibrary.Arguments
- ArgumentsLibrary.Builders.OptionBuilder
- ArgumentsLibrary.Builders.ArgumentBuilder
- ArgumentsLibrary.CommandLine

How to Use Arguments
============

### Example 1: Basic usage ###

~~~{.cs}
private static void Main(string[] args) {
    var arguments = new Arguments();
    arguments.AddOption( "option" );
    var cmd = arguments.Parse( args );
    // optionSet is true if args contain "--option" item
    var optionSet = cmd.IsOptionSet( "option" );
    ...
}
~~~

### Example 2: Programming practices API demo ###

~~~{.cs}
var arguments = new Arguments();

// Implicit recognition of short/long option name
// AddOption returns OptionBuilder instance
arguments.AddOption( "v|verbose" )
    // Description is used in help text generation
    .WithDescription( "Verbose option description" );

arguments.AddOption( "s|size" )
    .WithDescription( "Size option with default value of 42" )
    // Mandatory argument of Integer type.
    // Name SIZE is used in help text generation
    // OptionBuilder.WithArgument<T> returns ArgumentBuilder<T> instance
    .WithOptionalArgument<int>( "SIZE" )
    // Set default value
    .WithDefaultValue( 42 )
    // Set value conditions
    .WithCondition( v => v > 0 );

try {
    //Parse arguments
    // args: { "-v", "--size=42", "plain1", "plain2" }
    var commandLine = arguments.Parse( args );

    Console.WriteLine( "verbose = {0}", commandLine.IsOptionSet("verbose") );
    Console.WriteLine( "size = {0}", commandLine.GetOptionValue<int>("size") );
    Console.WriteLine( "arguments = {0}", String.Join( " ", commandLine.GetPlainArguments() ) );
    Console.WriteLine( arguments.BuildHelpText() );
}
catch (ParseException e) {
    Console.WriteLine( arguments.BuildHelpText() );
}
~~~

### Example 3: Fluent interface logic ###

~~~{.cs}
var arguments = new Arguments();

// API class hierarchy:
arguments.AddOption("t") // Arguments API provides OptionBuilder API
    .WithDescription("Popis") // OptionBuilder API
    .WithArgument<double>("CISLO") // OptionBuilder API provides ArgumentBuilder<double> API
    .WithCondition(d => d < 0.5); // ArgumentBuilder<double> API
~~~

### Example 4: Option aliases recognition ###

By default, single letters are considered short options and multiple letters are
considered long options. Particular option types can be forced using prefixes
**-** or **--**

~~~{.cs}
var arguments = new Arguments();

arguments.AddOption( "v" ) // -v
    .WithAlias( "verbose" ) // --verbose
    .WithAlias( "--v" ) // --v
    .WithAlias( "-verbose" ) // -verbose
    .WithDescription( "This is a description of this option" );

arguments.AddOption( "o|option ) // both -o and --option
    .WithAliases( "--n|-number ) // both --n and -number
    .WithDescription( "This is a second option description" );
~~~

### Example 5: Mandatory options ###

An option can be defined as mandatory. Omiting a mandatory option in command
line arguments denotes an invalid input and will result in an exception. Options
are **not mandatory** by default.

~~~{.cs}
var arguments = new Arguments();

arguments.AddMandatoryOption( "f|file" )
    .WithDescription( "File to be processed" )
    .WithArgument( "FILE" );

// args = { "-v", "no-file" }
var cmd = arguments.Parse( args );
// ParseException is thrown - option -f is missing
~~~

### Example 6: Optional option arguments ###

An option argument can be defined as optional. Omiting an optional argument will
not result in an exception. If a default value is provided, it will be used instead.
Option arguments are **not optional** by default.

~~~{.cs}
var arguments = new Arguments();

arguments.AddOption( "f|file" )
    .WithDescription( "File to be processed" )
    .WithOptionalArgument( "FILE" )
    .WithDefaultValue( "my-file.txt" );

// args = { "--file", "argument" }
var cmd = arguments.Parse( args );
// No long option value is provided, the default one is used instead
~~~
    
### Example 7: Using callbacks ###

~~~{.cs}
var arguments = new Arguments();

bool verbose = false;
arguments.AddOption( "v|verbose" )
    .WithDescription( "Verbose option description" )
    .WithAction( () => verbose = true );

int number = 0;
arguments.AddOption( "n|number" )
    .WithDescription( "Option with an integer parameter" )
    .WithArgument<int>( "INTEGER" )
    .WithAction( i => number = i );

arguments.Parse( args );
// verbose and number now contain values from args
~~~

### Example 8: Accessing options and arguments via CommandLine ###

Besides using callbacks, it is also possible to access parsed options and plain
arguments using CommandLine class.

~~~{.cs}
var arguments = new Arguments();

arguments.AddOption( "v|verbose" )
    .WithDescription( "Verbose option description" );

arguments.AddOption( "n|number" )
    .WithDescription( "Option with an integer parameter" )
    .WithArgument<int>( "INTEGER" );

var cmd = arguments.Parse( args );

bool verbose = cmd.IsOptionSet( "verbose" );
// verbose is true if the option is detected in args
if ( cmd.IsOptionSet( "number") ) {
    // If the option is not set, the line below throws an exception
    int number = cmd.GetOptionValue<int>( "-n" );
    // number contains an integer value 
}
List<string> plainArguments = cmd.GetPlainArguments();
// plainArguments contain the rest of the command line arguments that are not
// recognized as options
~~~

### Example 9: Explicit help text option and usage ###

The following option definition writes a help text to the console immediately
when the help option is detected among input command line arguments. The example
also shows a custom program usage definition.

The output help message is formatted so that it fits into the console. Each line
is 80 chars long at maximum and option descriptions are justified in a block
of text.

~~~{.cs}
var arguments = new Arguments();

var usage = "copy [options] <file>";

arguments.AddOption( "h|help" )
    .WithDescription( "Shows help text" )
    .WithAction( () =>
        Console.WriteLine( arguments.BuildHelpText( usage) ) );

arguments.AddOption( "r|recursive" )
    .WithDescription( "Denotes in depth copying" );

arguments.AddMandatoryOption( "t" )
    .WithDescription( "Copy target\nThis option is mandatory!" )
    .WithArgument( "TARGET" );

// args = { "--help" }
arguments.Parse(args);
// Option --help is triggered and writes following lines to the console:
//
// Usage: copy [options] <file>
// Options:
// -h, --help        Shows help text
// -r, --recursive   Denotes in depth copying
// -t <TARGET>       Copy target
//                   This option is mandatory!
~~~

### Example 10: Conditions and enumerations ###

Conditions and enumerations can be used to further restrict potential argument
value. Multiple conditions and enumerations can be used, the argument value must
satisfy all of them to pass as a valid option argument.

~~~{.cs}
var arguments = new Arguments();

arguments.AddMandatoryOption( "i|integer" ) // -i, --integer
    .WithDescription( "This is a description of an integer option" )
    // Optional argument of type int
    .WithOptionalArgument<int>( "INTEGER" )
    // Example condition of limiting the number
    .WithCondition( i => i > 0 && i <= 10 )
    // This line does the same as the one above
    .WithEnumeratedValue( 5, 10, 20 )
    .WithAction( i => {
        // This action is called when an integer 5 or 10 is detected
    } );

var cmd = arguments.Parse( args );
...
~~~

### Example 11: Custom argument types ###

Arguments library support following types conversion by default: string, int,
float, double and bool. It is possible to define custom type convertors or even
overwrite the default ones.
     
~~~{.cs}
var arguments = new Arguments();

arguments.RegisterTypeConverter( long.Parse );
arguments.AddOption( "l|long" )
    .WithDescription( "Long option description" )
    .WithArgument<long>( "LONG" );

var cmd = arguments.Parse( args );
var longValue = cmd.GetOptionValue<long>( "long" );
...
~~~

### Example 12: Advanced custom types ###

The following example shows an emulation of integer lists argument using custom
type converter. The converter accepts a comma separated string and converts each
part to an integer.
    
~~~{.cs}
var arguments = new Arguments();

arguments.RegisterTypeConverter<List<int>>(
    s => s.Split( ',' ).Select( int.Parse ).ToList());
arguments.AddOption( "i|int-list" )
    .WithDescription( "Integer list emulation" )
    .WithArgument<List<int>>( "INTEGERS" );

// args = { "-i", "1,2,3" }
var cmd = arguments.Parse( args );
var intList = cmd.GetOptionValue<List<int>>( "i" );
// intList contains integers 1, 2 and 3
...
~~~

### Example 13: Alternative approach to lists ###

The following example shows an emulation of integer lists argument using
a sequence of the same options.
    
~~~{.cs}
var arguments = new Arguments();

var intList = new List<int>();

arguments.AddOption( "i|integer" )
    .WithDescription( "Sequence integer list option" )
    .WithArgument<int>( "INTEGER" )
    .WithAction(intList.Add);

// args = { "-i", "1", "-i", "2","-i", "3", }
var cmd = arguments.Parse( args );
// intList now contains integers 1, 2 and 3
...
~~~



