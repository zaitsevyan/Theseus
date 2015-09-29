using System;
using System.Linq;
using ArgumentsLibrary.Exceptions;

namespace ArgumentsLibrary.Builders {

    /// <summary>
    /// Fluent interface provider for building Arguments
    /// </summary>
    /// <typeparam name="T">Type of the Argument</typeparam>
    public class ArgumentBuilder<T> {
        #region Internals

        /// <summary>
        /// Argument instance to build with this builder
        /// </summary>
        internal Argument<T> Argument { get; private set; }

        internal ArgumentBuilder( Action<dynamic, Type> registerArgumentAction ) {
            if ( registerArgumentAction == null ) {
                throw new ArgumentNullException( "registerArgumentAction" );
            }

            Argument = new Argument<T>();
            registerArgumentAction( Argument, typeof ( T ) );
        }

        #endregion

        #region API

        /// <summary>
        /// Sets an indicator whether the Argument is optional or not.
        /// </summary>
        /// <param name="flag">True if optional, False otherwise</param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> SetName( string name ) {
            if ( name == null ) {
                throw new SetupException( "Argument name cannot be null" );
            }
            Argument.Name = name;
            return this;
        }

        /// <summary>
        /// Sets an indicator whether the Argument is optional or not.
        /// </summary>
        /// <param name="flag">True if optional, False otherwise</param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> SetOptional( bool flag = true ) {
            Argument.Optional = flag;
            return this;
        }

        /// <summary>
        /// Specifies the default value for the Argument
        /// </summary>
        /// <param name="value">Default value of type T</param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> WithDefaultValue( T value ) {
            if ( value == null ) {
                throw new SetupException(
                    "Argument default value cannot be null" );
            }
            Argument.DefaultValue = value;
            Argument.DefaultValueIsSet = true;
            return this;
        }

        /// <summary>
        /// Specifies a condition that the Argument value must satisfy.
        /// Multiple conditions may be defined; if so, the Argument value
        /// must satisfy all conditions.
        /// </summary>
        /// <param name="conditionFunc">Condition function (predicate)</param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> WithCondition( Func<T, bool> conditionFunc ) {
            if ( conditionFunc == null ) {
                throw new SetupException(
                    "Argument condition function cannot be null" );
            }
            Argument.Conditions.Add( conditionFunc );
            return this;
        }

        /// <summary>
        /// Specifies a list of possible values of the Argument
        /// Only one set of values should be defined. If there are more sets
        /// defined, the Argument value will be accepted only if it is
        /// present in all enumerations.
        /// </summary>
        /// <param name="valuesEnumeration">
        /// Enumeration of possible values
        /// </param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> WithEnumeratedValue(
            params T[] valuesEnumeration ) {
            if ( valuesEnumeration == null ) {
                throw new SetupException(
                    "Argument values enumeration cannot be null" );
            }
            return WithCondition( valuesEnumeration.Contains );
        }

        /// <summary>
        /// Specifies an Action to perform when the Argument value is detected
        /// among console input parameters.
        /// </summary>
        /// <param name="action">
        /// Action to be performed when the Argument value
        /// is detected among input parameters
        /// </param>
        /// <returns>ArgumentBuilder{T} fluent interface</returns>
        public ArgumentBuilder<T> WithAction( Action<T> action ) {
            if ( action == null ) {
                throw new SetupException( "Argument action cannot be null" );
            }
            Argument.Actions.Add( action );
            return this;
        }

        #endregion
    }

}