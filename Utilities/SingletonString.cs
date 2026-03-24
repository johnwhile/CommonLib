using System;

namespace Common
{
    /// <summary>
    ///     A singleton string is a string that has a unique instance in memory, 
    ///     This class should mostly be used internally for performance reasons, in scenarios where equals/hashcode
    ///     could be invoked frequently, and the set of strings is limited. Internally, <see cref="SingletonString" />
    ///     string is using the method <see cref="string.Intern" /> and also is precaching the hashcode of the string.
    /// </summary>
    public struct SingletonString : IEquatable<SingletonString>
    {
        private int hashCode;
        private string text;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SingletonString" /> struct.
        /// </summary>
        public SingletonString(string text): this()
        {
            this.text = string.Intern(text);
            hashCode = text != null ? text.GetHashCode() : 0;
        }

        public bool Equals(SingletonString other)
        {
            // Optimized equals, only using references.
            return hashCode == other.hashCode && ReferenceEquals(text, other.text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SingletonString && Equals((SingletonString)obj);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SingletonString left, SingletonString right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SingletonString left, SingletonString right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SingletonString left, string right)
        {
            return string.Equals(left.text, right);
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SingletonString left, string right)
        {
            return !string.Equals(left.text, right);
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="SingletonString" /> to <see cref="string" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(SingletonString value)
        {
            return value.text;
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="string" /> to <see cref="SingletonString" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator SingletonString(string value)
        {
            return new SingletonString(value);
        }

        public override string ToString()
        {
            return text;
        }
    }
}
