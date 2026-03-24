using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Common.Maths
{
    /// <summary>
    /// Represents a half-precision floating point number. 
    /// </summary>
    /// <remarks>
    /// Note:
    ///     Half is not fast enought and precision is also very bad, 
    ///     so is should not be used for matemathical computation (use Single instead).
    ///     The main advantage of Half type is lower memory cost: two bytes per number. 
    ///     Half is typically used in graphical applications.
    ///     
    /// Note: 
    ///     All functions, where is used conversion half->float/float->half, 
    ///     are approx. ten times slower than float->double/double->float, i.e. ~3ns on 2GHz CPU.
    ///
    /// References:
    ///     - Fast Half Float Conversions, Jeroen van der Zijp, link: http://www.fox-toolkit.org/ftp/fasthalffloatconversion.pdf
    ///     - IEEE 754 revision, link: http://grouper.ieee.org/groups/754/
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct hfloat : IComparable, IFormattable, IConvertible, IComparable<hfloat>, IEquatable<hfloat>
    {
        /// <summary>
        /// Internal representation of the half-precision floating-point number.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [FieldOffset(0)] internal ushort value;

        #region Constants
        /// <summary>
        /// Represents the smallest positive System.Half value greater than zero. This field is constant.
        /// </summary>
        public static readonly hfloat Epsilon = ToHalf(0x0001);
        /// <summary>
        /// Represents the largest possible value of System.Half. This field is constant.
        /// </summary>
        public static readonly hfloat MaxValue = ToHalf(0x7bff);
        /// <summary>
        /// Represents the smallest possible value of System.Half. This field is constant.
        /// </summary>
        public static readonly hfloat MinValue = ToHalf(0xfbff);
        /// <summary>
        /// Represents not a number (NaN). This field is constant.
        /// </summary>
        public static readonly hfloat NaN = ToHalf(0xfe00);
        /// <summary>
        /// Represents negative infinity. This field is constant.
        /// </summary>
        public static readonly hfloat NegativeInfinity = ToHalf(0xfc00);
        /// <summary>
        /// Represents positive infinity. This field is constant.
        /// </summary>
        public static readonly hfloat PositiveInfinity = ToHalf(0x7c00);
        #endregion

        #region Constructors
        public hfloat(BinaryReader reader) { value = reader.ReadUInt16(); }
        public void Write(BinaryWriter writer) => writer.Write(value);
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified single-precision floating-point number.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(float value) { this = HalfHelper.SingleToHalf(value); }
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified 32-bit signed integer.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(int value) : this((float)value) { }
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified 64-bit signed integer.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(long value) : this((float)value) { }
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified double-precision floating-point number.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(double value) : this((float)value) { }
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified decimal number.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(decimal value) : this((float)value) { }
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(uint value) : this((float)value) { }
        /// <summary>
        /// Initializes a new instance of System.Half to the value of the specified 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The value to represent as a System.Half.</param>
        public hfloat(ulong value) : this((float)value) { }
        #endregion

        #region Numeric operators

        /// <summary>
        /// Returns the result of multiplying the specified System.Half value by negative one.
        /// </summary>
        /// <param name="half">A System.Half.</param>
        /// <returns>A System.Half with the value of half, but the opposite sign. -or- Zero, if half is zero.</returns>
        public static hfloat Negate(hfloat half) => -half;
        /// <summary>
        /// Adds two specified System.Half values.
        /// </summary>
        /// <returns>A System.Half value that is the sum of half1 and half2.</returns>
        public static hfloat Add(hfloat half1, hfloat half2) => half1 + half2;
        /// <summary>
        /// Subtracts one specified System.Half value from another.
        /// </summary>
        /// <returns>The System.Half result of subtracting half2 from half1.</returns>
        public static hfloat Subtract(hfloat half1, hfloat half2) => half1 - half2;
        /// <summary>
        /// Multiplies two specified System.Half values.
        /// </summary>
        /// <returns>A System.Half that is the result of multiplying half1 and half2.</returns>
        public static hfloat Multiply(hfloat half1, hfloat half2) => half1 * half2;
        /// <summary>
        /// Divides two specified System.Half values.
        /// </summary>
        /// <returns>The System.Half that is the result of dividing half1 by half2.</returns>
        /// <exception cref="System.DivideByZeroException">half2 is zero.</exception>
        public static hfloat Divide(hfloat half1, hfloat half2) => half1 / half2;

        /// <summary>
        /// Returns the value of the System.Half operand (the sign of the operand is unchanged).
        /// </summary>
        /// <returns>The value of the operand, half.</returns>
        public static hfloat operator +(hfloat half) => half;
        /// <summary>
        /// Negates the value of the specified System.Half operand.
        /// </summary>
        /// <returns>The result of half multiplied by negative one (-1).</returns>
        public static hfloat operator -(hfloat half) => HalfHelper.Negate(half);
        /// <summary>
        /// Increments the System.Half operand by 1.
        /// </summary>
        /// <returns>The value of half incremented by 1.</returns>
        public static hfloat operator ++(hfloat half) => (hfloat)(half + 1f);
        /// <summary>
        /// Decrements the System.Half operand by one.
        /// </summary>
        /// <returns>The value of half decremented by 1.</returns>
        public static hfloat operator --(hfloat half) => (hfloat)(half - 1f);
        /// <summary>
        /// Adds two specified System.Half values.
        /// </summary>
        /// <returns>The System.Half result of adding half1 and half2.</returns>
        public static hfloat operator +(hfloat half1, hfloat half2) => (hfloat)((float)half1 + (float)half2);
        /// <summary>
        /// Subtracts two specified System.Half values.
        /// </summary>    
        public static hfloat operator -(hfloat half1, hfloat half2) => (hfloat)((float)half1 - (float)half2);
        /// <summary>
        /// Multiplies two specified System.Half values.
        /// </summary>
        public static hfloat operator *(hfloat half1, hfloat half2) => (hfloat)((float)half1 * (float)half2);
        /// <summary>
        /// Divides two specified System.Half values.
        /// </summary>
        public static hfloat operator /(hfloat half1, hfloat half2) => (hfloat)((float)half1 / (float)half2);
        /// <summary>
        /// Returns a value indicating whether two instances of System.Half are equal.
        /// </summary>
        public static bool operator ==(hfloat half1, hfloat half2) => !IsNaN(half1) && (half1.value == half2.value);
        /// <summary>
        /// Returns a value indicating whether two instances of System.Half are not equal.
        /// </summary>
        public static bool operator !=(hfloat half1, hfloat half2) => !(half1.value == half2.value);
        /// <summary>
        /// Returns a value indicating whether a specified System.Half is less than another specified System.Half.
        /// </summary>
        public static bool operator <(hfloat half1, hfloat half2) => (float)half1 < (float)half2;
        /// <summary>
        /// Returns a value indicating whether a specified System.Half is greater than another specified System.Half.
        /// </summary>
        public static bool operator >(hfloat half1, hfloat half2) => (float)half1 > (float)half2;
        /// <summary>
        /// Returns a value indicating whether a specified System.Half is less than or equal to another specified System.Half.
        /// </summary>
        public static bool operator <=(hfloat half1, hfloat half2) => (half1 == half2) || (half1 < half2);
        /// <summary>
        /// Returns a value indicating whether a specified System.Half is greater than or equal to another specified System.Half.
        /// </summary>
        public static bool operator >=(hfloat half1, hfloat half2) => (half1 == half2) || (half1 > half2);
        #endregion

        #region Type casting operators
        /// <summary>
        /// Converts an 8-bit unsigned integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(byte value) => new hfloat((float)value);
        /// <summary>
        /// Converts a 16-bit signed integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(short value) => new hfloat((float)value);
        /// <summary>
        /// Converts a Unicode character to a System.Half.
        /// </summary>
        public static implicit operator hfloat(char value) => new hfloat((float)value);
        /// <summary>
        /// Converts a 32-bit signed integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(int value) => new hfloat((float)value);
        /// <summary>
        /// Converts a 64-bit signed integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(long value) => new hfloat((float)value);
        /// <summary>
        /// Converts a single-precision floating-point number to a System.Half.
        /// </summary>
        public static explicit operator hfloat(float value) => new hfloat((float)value);
        /// <summary>
        /// Converts a double-precision floating-point number to a System.Half.
        /// </summary>
        public static explicit operator hfloat(double value) => new hfloat((float)value);
        /// <summary>
        /// Converts a decimal number to a System.Half.
        /// </summary>
        public static explicit operator hfloat(decimal value) => new hfloat((float)value);
        /// <summary>
        /// Converts a System.Half to an 8-bit unsigned integer.
        /// </summary>
        public static explicit operator byte(hfloat value) => (byte)(float)value;
        /// <summary>
        /// Converts a System.Half to a Unicode character.
        /// </summary>
        public static explicit operator char(hfloat value) => (char)(float)value;
        /// <summary>
        /// Converts a System.Half to a 16-bit signed integer.
        /// </summary>
        public static explicit operator short(hfloat value) => (short)(float)value;
        /// <summary>
        /// Converts a System.Half to a 32-bit signed integer.
        /// </summary>
        public static explicit operator int(hfloat value) => (int)(float)value;
        /// <summary>
        /// Converts a System.Half to a 64-bit signed integer.
        /// </summary>
        public static explicit operator long(hfloat value) => (long)(float)value;
        /// <summary>
        /// Converts a System.Half to a single-precision floating-point number.
        /// </summary>
        public static implicit operator float(hfloat value) => (float)HalfHelper.HalfToSingle(value);
        /// <summary>
        /// Converts a System.Half to a double-precision floating-point number.
        /// </summary>
        public static implicit operator double(hfloat value) => (double)(float)value;
        /// <summary>
        /// Converts a System.Half to a decimal number.
        /// </summary>
        public static explicit operator decimal(hfloat value) => (decimal)(float)value;
        /// <summary>
        /// Converts an 8-bit signed integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(sbyte value) => new hfloat((float)value);
        /// <summary>
        /// Converts a 16-bit unsigned integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(ushort value) => new hfloat((float)value);
        /// <summary>
        /// Converts a 32-bit unsigned integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(uint value) => new hfloat((float)value);
        /// <summary>
        /// Converts a 64-bit unsigned integer to a System.Half.
        /// </summary>
        public static implicit operator hfloat(ulong value) => new hfloat((float)value);
        /// <summary>
        /// Converts a System.Half to an 8-bit signed integer.
        /// </summary>
        public static explicit operator sbyte(hfloat value) => (sbyte)(float)value;
        /// <summary>
        /// Converts a System.Half to a 16-bit unsigned integer.
        /// </summary>
        public static explicit operator ushort(hfloat value) => (ushort)(float)value;
        /// <summary>
        /// Converts a System.Half to a 32-bit unsigned integer.
        /// </summary>
        public static explicit operator uint(hfloat value) => (uint)(float)value;
        /// <summary>
        /// Converts a System.Half to a 64-bit unsigned integer.
        /// </summary>
        public static explicit operator ulong(hfloat value) => (ulong)(float)value;
        #endregion

        /// <summary>
        /// Compares this instance to a specified System.Half object.
        /// </summary>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value.
        /// Return Value Meaning Less than zero This instance is less than value. Zero
        /// This instance is equal to value. Greater than zero This instance is greater than value.
        /// </returns>
        public int CompareTo(hfloat other)
        {
            if (this < other) return -1;
            else if (this > other) return 1;
            else if (this != other)
            {
                if (!IsNaN(this)) return 1;
                else if (!IsNaN(other)) return -1;
            }
            return 0;
        }
        /// <summary>
        /// Compares this instance to a specified System.Object.
        /// </summary>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value.
        /// Return Value Meaning Less than zero This instance is less than value. Zero
        /// This instance is equal to value. Greater than zero This instance is greater
        /// than value. -or- value is null.
        /// </returns>
        /// <exception cref="System.ArgumentException">value is not a System.Half</exception>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            else
            {
                if (obj is hfloat half) return CompareTo(half);
                else throw new ArgumentException("Object must be of type Half.");
            }
        }
        /// <summary>
        /// Returns a value indicating whether this instance and a specified System.Half object represent the same value.
        /// </summary>
        public bool Equals(hfloat other) => (other == this) || (IsNaN(other) && IsNaN(this));

        /// <summary>
        /// Returns a value indicating whether this instance and a specified System.Object
        /// represent the same type and value.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is hfloat half)
                return (half == this) || (IsNaN(half) && IsNaN(this));
            return false;
        }
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Returns the System.TypeCode for value type System.Half.
        /// </summary>
        /// <returns>The enumerated constant (TypeCode)255.</returns>
        public TypeCode GetTypeCode() => (TypeCode)255;

        #region BitConverter & Math methods for Half
        /// <summary>
        /// Returns the specified half-precision floating point value as bytes[2]
        /// </summary>
        public static byte[] GetBytes(hfloat value) => BitConverter.GetBytes(value.value);
        /// <summary>
        /// Converts the value of a specified instance of System.Half to its equivalent binary representation.
        /// </summary>
        /// <param name="value">A System.Half value.</param>
        /// <returns>A 16-bit unsigned integer that contain the binary representation of value.</returns>        
        public static ushort GetBits(hfloat value) => value.value;
        /// <summary>
        /// Returns a half-precision floating point number converted from two bytes
        /// at a specified position in a byte array.
        /// </summary>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A half-precision floating point number formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="System.ArgumentException">
        /// startIndex is greater than or equal to the length of value minus 1, and is
        /// less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public static hfloat ToHalf(byte[] value, int startIndex) => ToHalf((ushort)BitConverter.ToInt16(value, startIndex));

        /// <summary>
        /// Returns a half-precision floating point number converted from its binary representation.
        /// </summary>
        /// <returns>A half-precision floating point number formed by its binary representation.</returns>
        public static hfloat ToHalf(ushort bits) => new hfloat { value = bits };

        /// <summary>
        /// Returns a floating point number converted from its half representation passed as int16
        /// </summary>
        public static float ToFloat(ushort bits) => HalfHelper.HalfToSingle(bits);

        /// <summary>
        /// Returns a value indicating the sign of a half-precision floating-point number.
        /// </summary>
        /// <returns>
        /// A number indicating the sign of value. Number Description -1 value is less
        /// than zero. 0 value is equal to zero. 1 value is greater than zero.
        /// </returns>
        /// <exception cref="System.ArithmeticException">value is equal to System.Half.NaN.</exception>
        public static int Sign(hfloat value)
        {
            if (value < 0) return -1;
            else if (value > 0) return 1;
            else if (value != 0) throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
            return 0;
        }
        /// <summary>
        /// Returns the absolute value of a half-precision floating-point number.
        /// </summary>
        /// <param name="value">A number in the range System.Half.MinValue ≤ value ≤ System.Half.MaxValue.</param>
        /// <returns>A half-precision floating-point number, x, such that 0 ≤ x ≤System.Half.MaxValue.</returns>
        public static hfloat Abs(hfloat value) => HalfHelper.Abs(value);
        /// <summary>
        /// Returns the larger of two half-precision floating-point numbers.
        /// </summary>
        /// <returns>
        /// Parameter value1 or value2, whichever is larger. If value1, or value2, or both val1
        /// and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        /// </returns>
        public static hfloat Max(hfloat value1, hfloat value2) => (value1 < value2) ? value2 : value1;

        /// <summary>
        /// Returns the smaller of two half-precision floating-point numbers.
        /// </summary>
        /// <returns>
        /// Parameter value1 or value2, whichever is smaller. If value1, or value2, or both val1
        /// and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        /// </returns>
        public static hfloat Min(hfloat value1, hfloat value2) => (value1 < value2) ? value1 : value2;
        #endregion

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to not a number (System.Half.NaN).
        /// </summary>
        /// <returns>true if value evaluates to not a number (System.Half.NaN); otherwise, false.</returns>
        public static bool IsNaN(hfloat half) => HalfHelper.IsNaN(half);
        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to negative or positive infinity.
        /// </summary>
        /// <returns>true if half evaluates to System.Half.PositiveInfinity or System.Half.NegativeInfinity; otherwise, false.</returns>
        public static bool IsInfinity(hfloat half) => HalfHelper.IsInfinity(half);
        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to negative infinity.
        /// </summary>
        /// <returns>true if half evaluates to System.Half.NegativeInfinity; otherwise, false.</returns>
        public static bool IsNegativeInfinity(hfloat half) => HalfHelper.IsNegativeInfinity(half);
        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to positive infinity.
        /// </summary>
        /// <returns>true if half evaluates to System.Half.PositiveInfinity; otherwise, false.</returns>
        public static bool IsPositiveInfinity(hfloat half) => HalfHelper.IsPositiveInfinity(half);


        #region String operations (Parse and ToString)
        /// <summary>
        /// Converts the string representation of a number to its System.Half equivalent.
        /// </summary>
        /// <returns>The System.Half number equivalent to the number contained in value.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.FormatException">value is not in the correct format.</exception>
        /// <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        public static hfloat Parse(string value) => (hfloat)float.Parse(value, CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts the string representation of a number to its System.Half equivalent 
        /// using the specified culture-specific format information.
        /// </summary>
        /// <param name="provider">An System.IFormatProvider that supplies culture-specific parsing information about value.</param>
        /// <returns>The System.Half number equivalent to the number contained in s as specified by provider.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.FormatException">value is not in the correct format.</exception>
        /// <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        public static hfloat Parse(string value, IFormatProvider provider) => (hfloat)float.Parse(value, provider);

        /// <summary>
        /// Converts the string representation of a number in a specified style to its System.Half equivalent.
        /// </summary>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates
        /// the style elements that can be present in value. A typical value to specify is
        /// System.Globalization.NumberStyles.Number.
        /// </param>
        /// <returns>The System.Half number equivalent to the number contained in s as specified by style.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is the
        /// System.Globalization.NumberStyles.AllowHexSpecifier value.
        /// </exception>
        /// <exception cref="System.FormatException">value is not in the correct format.</exception>
        /// <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        public static hfloat Parse(string value, NumberStyles style) => (hfloat)float.Parse(value, style, CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts the string representation of a number to its System.Half equivalent 
        /// using the specified style and culture-specific format.
        /// </summary>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates
        /// the style elements that can be present in value. A typical value to specify is 
        /// System.Globalization.NumberStyles.Number.
        /// </param>
        /// <param name="provider">An System.IFormatProvider object that supplies culture-specific information about the format of value.</param>
        /// <returns>The System.Half number equivalent to the number contained in s as specified by style and provider.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is the
        /// System.Globalization.NumberStyles.AllowHexSpecifier value.
        /// </exception>
        /// <exception cref="System.FormatException">value is not in the correct format.</exception>
        /// <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        public static hfloat Parse(string value, NumberStyles style, IFormatProvider provider) => (hfloat)float.Parse(value, style, provider);

        /// <summary>
        /// Converts the string representation of a number to its System.Half equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="result">
        /// When this method returns, contains the System.Half number that is equivalent
        /// to the numeric value contained in value, if the conversion succeeded, or is zero
        /// if the conversion failed. The conversion fails if the s parameter is null,
        /// is not a number in a valid format, or represents a number less than System.Half.MinValue
        /// or greater than System.Half.MaxValue. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string value, out hfloat result)
        {
            if (float.TryParse(value, out var f))
            {
                result = (hfloat)f;
                return true;
            }
            result = new hfloat();
            return false;
        }
        /// <summary>
        /// Converts the string representation of a number to its System.Half equivalent
        /// using the specified style and culture-specific format. A return value indicates
        /// whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates
        /// the permitted format of value. A typical value to specify is System.Globalization.NumberStyles.Number.
        /// </param>
        /// <param name="provider">An System.IFormatProvider object that supplies culture-specific parsing information about value.</param>
        /// <param name="result">
        /// When this method returns, contains the System.Half number that is equivalent
        /// to the numeric value contained in value, if the conversion succeeded, or is zero
        /// if the conversion failed. The conversion fails if the s parameter is null,
        /// is not in a format compliant with style, or represents a number less than
        /// System.Half.MinValue or greater than System.Half.MaxValue. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="System.ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style 
        /// is the System.Globalization.NumberStyles.AllowHexSpecifier value.
        /// </exception>
        public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out hfloat result)
        {
            if (float.TryParse(value, style, provider, out var f))
            {
                result = (hfloat)f;
                return true;
            }
            else result = new hfloat();
            return false;
        }
        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        public override string ToString() => ((float)this).ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation
        /// using the specified culture-specific format information.
        /// </summary>
        /// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of the value of this instance as specified by provider.</returns>
        public string ToString(IFormatProvider formatProvider) => ((float)this).ToString(formatProvider);

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation, using the specified format.
        /// </summary>
        public string ToString(string format) => ((float)this).ToString(format, CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation 
        /// using the specified format and culture-specific format information.
        /// </summary>
        /// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of the value of this instance as specified by format and provider.</returns>
        /// <exception cref="System.FormatException">format is invalid.</exception>
        public string ToString(string format, IFormatProvider formatProvider) => ((float)this).ToString(format, formatProvider);
        #endregion

        #region IConvertible Members
        float IConvertible.ToSingle(IFormatProvider provider)
            => (float)this;
        TypeCode IConvertible.GetTypeCode()
            => GetTypeCode();
        bool IConvertible.ToBoolean(IFormatProvider provider)
            => Convert.ToBoolean((float)this);

        byte IConvertible.ToByte(IFormatProvider provider)
            => Convert.ToByte((float)this);

        char IConvertible.ToChar(IFormatProvider provider)
            => throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "Char"));

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
            => throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "DateTime"));

        decimal IConvertible.ToDecimal(IFormatProvider provider)
            => Convert.ToDecimal((float)this);

        double IConvertible.ToDouble(IFormatProvider provider)
            => Convert.ToDouble((float)this);

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16((float)this);
        }
        int IConvertible.ToInt32(IFormatProvider provider)
            => Convert.ToInt32((float)this);

        long IConvertible.ToInt64(IFormatProvider provider)
            => Convert.ToInt64((float)this);

        sbyte IConvertible.ToSByte(IFormatProvider provider)
            => Convert.ToSByte((float)this);

        string IConvertible.ToString(IFormatProvider provider)
            => Convert.ToString((float)this, CultureInfo.InvariantCulture);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
            => (((float)this) as IConvertible).ToType(conversionType, provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider)
            => Convert.ToUInt16((float)this);

        uint IConvertible.ToUInt32(IFormatProvider provider)
            => Convert.ToUInt32((float)this);

        ulong IConvertible.ToUInt64(IFormatProvider provider)
            => Convert.ToUInt64((float)this);
        #endregion
    }


    /// <summary>
    /// Helper class for Half conversions and some low level operations.
    /// This class is internally used in the Half class.
    /// </summary>
    /// <remarks>
    /// References:
    ///     - Fast Half Float Conversions, Jeroen van der Zijp, link: http://www.fox-toolkit.org/ftp/fasthalffloatconversion.pdf
    /// </remarks>
    [ComVisible(false)]
    internal static class HalfHelper
    {
        static uint[] mantissaTable = GenerateMantissaTable();
        static uint[] exponentTable = GenerateExponentTable();
        static ushort[] offsetTable = GenerateOffsetTable();
        static ushort[] baseTable = GenerateBaseTable();
        static sbyte[] shiftTable = GenerateShiftTable();

        // Transforms the subnormal representation to a normalized one. 
        static uint ConvertMantissa(int i)
        {
            uint m = (uint)(i << 13); // Zero pad mantissa bits
            uint e = 0; // Zero exponent

            // While not normalized
            while ((m & 0x00800000) == 0)
            {
                e -= 0x00800000; // Decrement exponent (1<<23)
                m <<= 1; // Shift mantissa                
            }
            m &= unchecked((uint)~0x00800000); // Clear leading 1 bit
            e += 0x38800000; // Adjust bias ((127-14)<<23)
            return m | e; // Return combined number
        }

        static uint[] GenerateMantissaTable()
        {
            uint[] mantissaTable = new uint[2048];
            mantissaTable[0] = 0;
            for (int i = 1; i < 1024; i++)
            {
                mantissaTable[i] = ConvertMantissa(i);
            }
            for (int i = 1024; i < 2048; i++)
            {
                mantissaTable[i] = (uint)(0x38000000 + ((i - 1024) << 13));
            }

            return mantissaTable;
        }
        static uint[] GenerateExponentTable()
        {
            uint[] exponentTable = new uint[64];
            exponentTable[0] = 0;
            for (int i = 1; i < 31; i++)
            {
                exponentTable[i] = (uint)(i << 23);
            }
            exponentTable[31] = 0x47800000;
            exponentTable[32] = 0x80000000;
            for (int i = 33; i < 63; i++)
            {
                exponentTable[i] = (uint)(0x80000000 + ((i - 32) << 23));
            }
            exponentTable[63] = 0xc7800000;

            return exponentTable;
        }
        static ushort[] GenerateOffsetTable()
        {
            ushort[] offsetTable = new ushort[64];
            offsetTable[0] = 0;
            for (int i = 1; i < 32; i++)
            {
                offsetTable[i] = 1024;
            }
            offsetTable[32] = 0;
            for (int i = 33; i < 64; i++)
            {
                offsetTable[i] = 1024;
            }

            return offsetTable;
        }
        static ushort[] GenerateBaseTable()
        {
            ushort[] baseTable = new ushort[512];
            for (int i = 0; i < 256; ++i)
            {
                sbyte e = (sbyte)(127 - i);
                if (e > 24)
                { // Very small numbers map to zero
                    baseTable[i | 0x000] = 0x0000;
                    baseTable[i | 0x100] = 0x8000;
                }
                else if (e > 14)
                { // Small numbers map to denorms
                    baseTable[i | 0x000] = (ushort)(0x0400 >> (18 + e));
                    baseTable[i | 0x100] = (ushort)((0x0400 >> (18 + e)) | 0x8000);
                }
                else if (e >= -15)
                { // Normal numbers just lose precision
                    baseTable[i | 0x000] = (ushort)((15 - e) << 10);
                    baseTable[i | 0x100] = (ushort)(((15 - e) << 10) | 0x8000);
                }
                else if (e > -128)
                { // Large numbers map to Infinity
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
                else
                { // Infinity and NaN's stay Infinity and NaN's
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
            }

            return baseTable;
        }
        static sbyte[] GenerateShiftTable()
        {
            sbyte[] shiftTable = new sbyte[512];
            for (int i = 0; i < 256; ++i)
            {
                sbyte e = (sbyte)(127 - i);
                if (e > 24)
                { // Very small numbers map to zero
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                }
                else if (e > 14)
                { // Small numbers map to denorms
                    shiftTable[i | 0x000] = (sbyte)(e - 1);
                    shiftTable[i | 0x100] = (sbyte)(e - 1);
                }
                else if (e >= -15)
                { // Normal numbers just lose precision
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
                else if (e > -128)
                { // Large numbers map to Infinity
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                }
                else
                { // Infinity and NaN's stay Infinity and NaN's
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
            }

            return shiftTable;
        }

        public static unsafe float HalfToSingle(hfloat half)
            => HalfToSingle(half.value);

        public static unsafe float HalfToSingle(ushort halfvalue)
        {
            uint result = mantissaTable[offsetTable[halfvalue >> 10] + (halfvalue & 0x3ff)] + exponentTable[halfvalue >> 10];
            return *((float*)&result);
        }

        public static unsafe hfloat SingleToHalf(float single)
        {
            uint value = *((uint*)&single);
            ushort result = (ushort)(baseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> shiftTable[value >> 23]));
            return hfloat.ToHalf(result);
        }

        public static hfloat Negate(hfloat half)
            => hfloat.ToHalf((ushort)(half.value ^ 0x8000));

        public static hfloat Abs(hfloat half)
            => hfloat.ToHalf((ushort)(half.value & 0x7fff));

        public static bool IsNaN(hfloat half)
            => ((half.value & 0x7fff) > 0x7c00);

        public static bool IsInfinity(hfloat half)
            => ((half.value & 0x7fff) == 0x7c00);

        public static bool IsPositiveInfinity(hfloat half)
            => (half.value == 0x7c00);

        public static bool IsNegativeInfinity(hfloat half)
            => (half.value == 0xfc00);
    }

    /// <summary>
    /// Half Vector4
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct VectorHalf4
    {
        public hfloat hx;
        public hfloat hy;
        public hfloat hz;
        public hfloat hw;

        public float x
        {
            get => (float)hx;
            set => hx = new hfloat(value);
        }
        public float y
        {
            get => (float)hy;
            set => hy = new hfloat(value);
        }
        public float z
        {
            get => (float)hz;
            set => hz = new hfloat(value);
        }
        public float w
        {
            get => (float)hw;
            set => hw = new hfloat(value);
        }

        /// <summary>
        /// </summary>
        public VectorHalf4(double X, double Y, double Z, double W)
        {
            hx = new hfloat(X);
            hy = new hfloat(Y);
            hz = new hfloat(Z);
            hw = new hfloat(W);
        }
        /// <summary>
        /// Passing double or float value need a normalization
        /// </summary>
        public VectorHalf4(float X, float Y, float Z, float W)
        {
            hx = new hfloat(X);
            hy = new hfloat(Y);
            hz = new hfloat(Z);
            hw = new hfloat(W);
        }
        public float this[int i]
        {
#if UNSAFE
            get
            {
                unsafe
                {
                    fixed (Half* pX = &this.hx)
                    {
                        return *(pX + i);
                    }
                }
            }
             
            set
            {
                unsafe
                {
                    fixed (Half* pX = &this.hx)
                    {
                        *(pX + i) = HalfHelper.SingleToHalf(value);
                    }
                }
            }
#else
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                    default: throw new ArgumentException("i must be 0,1,2,3");
                }
            }
            set
            {
                switch (i)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;
                    default: throw new ArgumentException("i must be 0,1,2,3");
                }
            }
#endif
        }

        public override string ToString()
            => (string.Format("{0,4} {1,4} {2,4} {3,4}", x, y, z, w));


        public static readonly VectorHalf4 Zero = new VectorHalf4 { hx = 0, hy = 0, hz = 0, hw = 0 };
        public static readonly VectorHalf4 UnitX = new VectorHalf4 { hx = new hfloat(1.0f), hy = 0, hz = 0, hw = new hfloat(1.0f) };
        public static readonly VectorHalf4 UnitY = new VectorHalf4 { hx = 0, hy = new hfloat(1.0f), hz = 0, hw = new hfloat(1.0f) };
        public static readonly VectorHalf4 UnitZ = new VectorHalf4 { hx = 0, hy = 0, hz = new hfloat(1.0f), hw = new hfloat(1.0f) };

        /// <summary>
        /// Transform the vector using matrix (= transform * vector), the product are from normal vector and rotation part of transform
        /// </summary>
        public static VectorHalf4 TransformCoordinate(VectorHalf4 point, Matrix4x4f transform)
        {
            Vector3f vector = (Vector3f)point;
            float inverseW = 1.0f / (transform.m30 * vector.x + transform.m31 * vector.y + transform.m32 * vector.z + transform.m33);
            float xf = ((transform.m00 * vector.x) + (transform.m01 * vector.y) + (transform.m02 * vector.z) + transform.m03) * inverseW;
            float yf = ((transform.m10 * vector.x) + (transform.m11 * vector.y) + (transform.m12 * vector.z) + transform.m13) * inverseW;
            float zf = ((transform.m20 * vector.x) + (transform.m21 * vector.y) + (transform.m22 * vector.z) + transform.m23) * inverseW;

            return new VectorHalf4(xf, yf, zf, inverseW);

        }
        public static VectorHalf4 TransformCoordinate(VectorHalf4 point, Matrix3x3f rotation)
        {
            Vector3f vector = (Vector3f)point;
            float xf = rotation.m00 * vector.x + rotation.m01 * vector.y + rotation.m02 * vector.z;
            float yf = rotation.m10 * vector.x + rotation.m11 * vector.y + rotation.m12 * vector.z;
            float zf = rotation.m20 * vector.x + rotation.m21 * vector.y + rotation.m22 * vector.z;
            return new VectorHalf4(xf, yf, zf, point.w);
        }

        public static implicit operator Vector3f(VectorHalf4 vector)
            => new Vector3f(vector.x, vector.y, vector.z);

        public static implicit operator VectorHalf4(Vector3f vector)
            => new VectorHalf4(vector.x, vector.y, vector.z, 1f);

        public static implicit operator Vector4f(VectorHalf4 vector)
            => new Vector4f(vector.x, vector.y, vector.z, vector.w);

        public static implicit operator VectorHalf4(Vector4f vector)
            => new VectorHalf4(vector.x, vector.y, vector.z, vector.w);
    }

    /// <summary>
    /// Half Vector3
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct VectorHalf3
    {
        public hfloat hx;
        public hfloat hy;
        public hfloat hz;
        public float x
        {
            get => (float)hx;
            set => hx = new hfloat(value);
        }
        public float y
        {
            get => (float)hy;
            set => hy = new hfloat(value);
        }
        public float z
        {
            get => (float)hz;
            set => hz = new hfloat(value);
        }

        public VectorHalf3(BinaryReader reader)
        {
            hx = new hfloat(reader);
            hy = new hfloat(reader);
            hz = new hfloat(reader);
        }

        public void Write(BinaryWriter writer)
        {
            hx.Write(writer);
            hy.Write(writer);
            hz.Write(writer);
        }

        /// <summary>
        /// Passing double or float value need a normalization
        /// </summary>
        public VectorHalf3(double x, double y, double z)
        {
            hx = new hfloat(x);
            hy = new hfloat(y);
            hz = new hfloat(z);
        }

        public VectorHalf3(Vector3f vector)
            : this(vector.x, vector.y, vector.z) { }

        /// <summary>
        /// Passing double or float value need a normalization
        /// </summary>
        public VectorHalf3(float x, float y, float z)
        {
            hx = new hfloat(x);
            hy = new hfloat(y);
            hz = new hfloat(z);
        }


        public float this[int i]
        {
#if UNSAFE
            get
            {
                unsafe
                {
                    fixed (Half* pX = &this.hx)
                    {
                        return *(pX + i);
                    }
                }
            }
             
            set
            {
                unsafe
                {
                    fixed (Half* pX = &this.hx)
                    {
                        *(pX + i) = HalfHelper.SingleToHalf(value);
                    }
                }
            }
#else
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default: throw new ArgumentException("i must be 0,1");
                }
            }
            set
            {
                switch (i)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new ArgumentException("i must be 0,1");
                }
            }
#endif
        }


        public override string ToString()
            => string.Format("{0,4} {1,4} {2,4}", x, y, z);

        public static readonly VectorHalf3 Zero = new VectorHalf3 { hx = 0, hy = 0, hz = 0 };
        public static readonly VectorHalf3 UnitX = new VectorHalf3 { hx = new hfloat(1.0f), hy = 0, hz = 0 };
        public static readonly VectorHalf3 UnitY = new VectorHalf3 { hx = 0, hy = new hfloat(1.0f), hz = 0 };
        public static readonly VectorHalf3 UnitZ = new VectorHalf3 { hx = 0, hy = 0, hz = new hfloat(1.0f) };

        public static implicit operator Vector3f(VectorHalf3 vector)
            => new Vector3f(vector.x, vector.y, vector.z);

        public static implicit operator VectorHalf3(Vector3f vector)
            => new VectorHalf3(vector.x, vector.y, vector.z);
    }

    /// <summary>
    /// Half Vector2
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct VectorHalf2
    {
        public hfloat hx;
        public hfloat hy;

        public float x
        {
            get => (float)hx;
            set => hx = new hfloat(value);
        }
        public float y
        {
            get => (float)hy;
            set => hy = new hfloat(value);
        }


        public VectorHalf2(BinaryReader reader)
        {
            hx = new hfloat(reader);
            hy = new hfloat(reader);
        }

        public void Write(BinaryWriter writer)
        {
            hx.Write(writer);
            hy.Write(writer);
        }
        /// <summary>
        /// Passing double or float value need a normalization
        /// </summary>
        public VectorHalf2(double x, double y)
        {
            hx = new hfloat(x);
            hy = new hfloat(y);
        }
        /// <summary>
        /// Passing double or float value need a normalization
        /// </summary>
        public VectorHalf2(float x, float y)
        {
            hx = new hfloat(x);
            hy = new hfloat(y);
        }
        public float this[int i]
        {
#if UNSAFE
            get
            {
                unsafe
                {
                    fixed (Half* pX = &this.hx)
                    {
                        return *(pX + i);
                    }
                }
            }
             
            set
            {
                unsafe
                {
                    fixed (Half* pX = &this.hx)
                    {
                        *(pX + i) = HalfHelper.SingleToHalf(value);
                    }
                }
            }
#else
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return y;
                    default: throw new ArgumentException("i must be 0,1");
                }
            }
            set
            {
                switch (i)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default: throw new ArgumentException("i must be 0,1");
                }
            }
#endif
        }

        public override string ToString()
            => string.Format("<{0,4} {1,4}>", x, y);

        public static readonly VectorHalf2 Zero = new VectorHalf2 { hx = 0, hy = 0 };
        public static readonly VectorHalf2 UnitX = new VectorHalf2 { hx = new hfloat(1.0f), hy = 0 };
        public static readonly VectorHalf2 UnitY = new VectorHalf2 { hx = 0, hy = new hfloat(1.0f) };

        public static implicit operator Vector2f(VectorHalf2 vector)
            => new Vector2f(vector.x, vector.y);

        public static implicit operator VectorHalf2(Vector2f vector)
            => new VectorHalf2(vector.x, vector.y);
    }
}
