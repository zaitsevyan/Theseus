using System;
using System.Collections.Generic;

namespace ArgumentsLibrary {

    /// <summary>
    /// Converter helps with conversion of strings to desired types.
    /// </summary>
    /// <example>
    /// <code>
    /// var converter = new Converter();<br />
    /// converter.RegisterTypeConverter&lt;int&gt;(int.Parse);<br />
    /// int number = converter.Convert&lt;int&gt;("12345");<br />
    /// </code>
    /// </example>
    internal class Converter {

        /// <summary>
        /// Dictionary to store converter functions
        /// </summary>
        private Dictionary<Type, object> TypeConverters { get; set; }

        /// <summary>
        /// Constructor function, initiates the type convertors storage
        /// </summary>
        internal Converter() {
            TypeConverters = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Converts a given string value to desired type using defined
        /// type converters.
        /// </summary>
        /// <typeparam name="T">Type to be converted into</typeparam>
        /// <param name="value">String value to be converted</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when input value is null
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the converter for
        /// desired type is not defined
        /// </exception>
        /// <returns>Converted value</returns>
        internal T Convert<T>( string value ) {
            if ( value == null ) {
                throw new ArgumentNullException( "value" );
            }
            if ( !TypeConverters.ContainsKey( typeof ( T ) ) ) {
                throw new InvalidOperationException(
                    String.Format( "Converter for type {0} is not defined",
                        typeof ( T ) ) );
            }
            try {
                return ( ( Func<string, T> ) TypeConverters[ typeof ( T ) ] )
                        .Invoke( value );
            }
            catch ( Exception ) {
                throw new FormatException(
                    String.Format(
                        "Cannot convert input input value to type {0}",
                        typeof ( T ) ) );
            }
        }

        /// <summary>
        /// Registers functions that convert string to a desired type.
        /// </summary>
        /// <typeparam name="T">Target converted type</typeparam>
        /// <param name="converterFunc">
        /// Function to convert string to type T
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the converter function is
        /// null
        /// </exception>
        internal void RegisterTypeConverter<T>( Func<string, T> converterFunc ) {
            if ( converterFunc == null ) {
                throw new ArgumentNullException( "converterFunc" );
            }
            TypeConverters.Add( typeof ( T ), converterFunc );
        }

    }

}