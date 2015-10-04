using System;

namespace ArgumentsLibrary {

    /// <summary>
    /// Simple structure to hold a name and type of an option signature
    /// </summary>
    internal class OptionAlias : IEquatable<OptionAlias> {

        /// <summary>
        /// Definition of a prefix for a short option, i.e. -o
        /// </summary>
        private const string SHORT_OPTION_PREFIX = "-";

        /// <summary>
        /// Definition of a prefix for a long option, i.e. --option
        /// </summary>
        private const string LONG_OPTION_PREFIX = "--";

        /// <summary>
        /// Gets the alias string
        /// </summary>
        /// <value>The alias string</value>
        internal string Alias { get; private set; }

        /// <summary>
        /// Gets the alis type
        /// </summary>
        /// <value>The alias type; see <see cref="OptionType" /> class</value>
        internal OptionType Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentsLibrary.OptionAlias" />
        /// class.
        /// </summary>
        /// <param name="alias">Alias string</param>
        /// <param name="type">Alias type</param>
        internal OptionAlias( string alias, OptionType type ) {
            if ( alias == null ) {
                throw new ArgumentNullException( "alias" );
            }
            if ( alias.Equals( string.Empty ) ) {
                throw new ArgumentException(
                    "Argument alias cannot be an empty string" );
            }
            Alias = alias;
            Type = type;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ArgumentsLibrary.OptionAlias" />
        /// is equal to the current <see cref="ArgumentsLibrary.OptionAlias" />
        /// </summary>
        /// <param name="other">
        /// The <see cref="ArgumentsLibrary.OptionAlias" /> to compare
        /// with the current <see cref="ArgumentsLibrary.OptionAlias" />
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="ArgumentsLibrary.OptionAlias" />
        /// is equal to the current <see cref="ArgumentsLibrary.OptionAlias" />;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals( OptionAlias other ) {
            return String.Equals(other.Alias, Alias, StringComparison.InvariantCulture) && other.Type == Type;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is
        /// equal to the current <see cref="ArgumentsLibrary.OptionAlias" />
        /// </summary>
        /// <param name="obj">
        /// The <see cref="System.Object" /> to compare with the current
        /// <see cref="ArgumentsLibrary.OptionAlias" />.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object" /> is equal
        /// to the current <see cref="ArgumentsLibrary.OptionAlias" />;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( object obj ) {
            return obj is OptionAlias && Equals( ( OptionAlias ) obj );
        }

        /// <summary>
        /// Serves as a hash function for the
        /// <see cref="ArgumentsLibrary.OptionAlias" /> object
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing
        /// algorithms and data structures such as a hash table.
        /// </returns>
        public override int GetHashCode() {
            // If hash is equal (in Dictionary.ContainsKey), than Equals() is used
            return Alias.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents the current
        /// <see cref="ArgumentsLibrary.OptionAlias" />
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents the current
        /// <see cref="ArgumentsLibrary.OptionAlias" />
        /// </returns>
        public override string ToString() {
            if ( Type == OptionType.Short ) {
                return SHORT_OPTION_PREFIX + Alias;
            }
            else {
                return LONG_OPTION_PREFIX + Alias;
            }
        }

    }

}