using System;
using System.Collections.Generic;
using System.Linq;

namespace ArgumentsLibrary {

    /// <summary>
    /// Representation of a defined option argument of particular type
    /// </summary>
    internal class Argument<T> {

        /// <summary>
        /// Definition of default name for argument
        /// </summary>
        internal const string DEFAULT_NAME = "argument";

        /// <summary>
        /// Definition of default behaviour: Optional or Mandatory
        /// </summary>
        internal const bool DEFAULT_OPTIONAL = false;

        /// <summary>
        /// Name of argument
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Whether or not the argument is optional
        /// </summary>
        internal bool Optional { get; set; }

        /// <summary>
        /// Default value of argument
        /// </summary>
        internal T DefaultValue { get; set; }

        /// <summary>
        /// Indicates whether the default value is specified or not
        /// </summary>
        internal bool DefaultValueIsSet { get; set; }

        /// <summary>
        /// List of actions, which are invoked when argument value appears in
        /// args string. It will be invoked after value is parsed and tesed with
        /// defined conditions.
        /// </summary>
        /// <remarks>
        /// Action signature: void function(T value){ ... }
        /// </remarks>
        internal List<Action<T>> Actions { get; set; }

        /// <summary>
        /// List of conditions which are checked when argument value appears in
        /// input arguments list
        /// </summary>
        /// <remarks>
        /// Default value will not be checked.
        /// Condition signature: bool function(T value){ ... }
        /// </remarks>
        internal List<Func<T, bool>> Conditions { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ArgumentsLibrary.Argument`1" /> class.
        /// </summary>
        internal Argument() {
            Name = DEFAULT_NAME;
            Optional = DEFAULT_OPTIONAL;
            DefaultValue = default( T );
            DefaultValueIsSet = false;
            Actions = new List<Action<T>>();
            Conditions = new List<Func<T, bool>>();
        }

        /// <summary>
        /// Parse the argument value from string with converters.
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="converter">Type converters</param>
        internal T Parse( string value, Converter converter ) {
            return converter.Convert<T>( value );
        }

        /// <summary>
        /// Invokes the actions.
        /// </summary>
        /// <param name="value">Argument value</param>
        internal void InvokeActions( T value ) {
            Actions.ForEach( action => action.Invoke( value ) );
        }

        /// <summary>
        /// Asserts the conditions.
        /// </summary>
        /// <param name="value">Argument value</param>
        internal void AssertConditions( T value ) {
            if ( !Conditions.All( condition => condition.Invoke( value ) ) ) {
                throw new ArgumentOutOfRangeException( "value",
                    "Argument does not satisfy required conditions" );
            }
        }

    }

}