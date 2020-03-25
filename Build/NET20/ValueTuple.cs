// https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/ValueTuple.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable SA1141 // explicitly not using tuple syntax in tuple implementation

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Helper so we can call some tuple methods recursively without knowing the underlying types.
    /// </summary>
    internal interface IValueTupleInternal
    {
        int Length { get; }
        int GetHashCode(IEqualityComparer comparer);
        string ToStringEnd();
    }

    /// <summary>
    /// The ValueTuple types (from arity 0 to 8) comprise the runtime implementation that underlies tuples in C# and struct tuples in F#.
    /// Aside from created via language syntax, they are most easily created via the ValueTuple.Create factory methods.
    /// The System.ValueTuple types differ from the System.Tuple types in that:
    /// - they are structs rather than classes,
    /// - they are mutable rather than readonly, and
    /// - their members (such as Item1, Item2, etc) are fields rather than properties.
    /// </summary>
    [Serializable]
    public struct ValueTuple
        : IEquatable<ValueTuple>, IComparable, IComparable<ValueTuple>, IValueTupleInternal
    {
        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ValueTuple"/>.</returns>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple;
        }

        /// <summary>Returns a value indicating whether this instance is equal to a specified value.</summary>
        /// <param name="other">An instance to compare to this instance.</param>
        /// <returns>true if <paramref name="other"/> has the same value as this instance; otherwise, false.</returns>
        public bool Equals(ValueTuple other)
        {
            return true;
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return 0;
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple other)
        {
            return 0;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return 0;
        }

        int IValueTupleInternal.Length => 0;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return 0;
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>()</c>.
        /// </remarks>
        public override string ToString()
        {
            return "()";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return ")";
        }

        /// <summary>Creates a new struct 0-tuple.</summary>
        /// <returns>A 0-tuple.</returns>
        public static ValueTuple Create() =>
            default;

        /// <summary>Creates a new struct 1-tuple, or singleton.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <returns>A 1-tuple (singleton) whose value is (item1).</returns>
        public static ValueTuple<T1> Create<T1>(T1 item1) =>
            new ValueTuple<T1>(item1);

        /// <summary>Creates a new struct 2-tuple, or pair.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <returns>A 2-tuple (pair) whose value is (item1, item2).</returns>
        public static ValueTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) =>
            new ValueTuple<T1, T2>(item1, item2);

        /// <summary>Creates a new struct 3-tuple, or triple.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <returns>A 3-tuple (triple) whose value is (item1, item2, item3).</returns>
        public static ValueTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) =>
            new ValueTuple<T1, T2, T3>(item1, item2, item3);

        /// <summary>Creates a new struct 4-tuple, or quadruple.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <returns>A 4-tuple (quadruple) whose value is (item1, item2, item3, item4).</returns>
        public static ValueTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) =>
            new ValueTuple<T1, T2, T3, T4>(item1, item2, item3, item4);

        /// <summary>Creates a new struct 5-tuple, or quintuple.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <returns>A 5-tuple (quintuple) whose value is (item1, item2, item3, item4, item5).</returns>
        public static ValueTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) =>
            new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);

        /// <summary>Creates a new struct 6-tuple, or sextuple.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <param name="item6">The value of the sixth component of the tuple.</param>
        /// <returns>A 6-tuple (sextuple) whose value is (item1, item2, item3, item4, item5, item6).</returns>
        public static ValueTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) =>
            new ValueTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);

        /// <summary>Creates a new struct 7-tuple, or septuple.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
        /// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <param name="item6">The value of the sixth component of the tuple.</param>
        /// <param name="item7">The value of the seventh component of the tuple.</param>
        /// <returns>A 7-tuple (septuple) whose value is (item1, item2, item3, item4, item5, item6, item7).</returns>
        public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) =>
            new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);

        /// <summary>Creates a new struct 8-tuple, or octuple.</summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
        /// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
        /// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <param name="item6">The value of the sixth component of the tuple.</param>
        /// <param name="item7">The value of the seventh component of the tuple.</param>
        /// <param name="item8">The value of the eighth component of the tuple.</param>
        /// <returns>An 8-tuple (octuple) whose value is (item1, item2, item3, item4, item5, item6, item7, item8).</returns>
        public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8) =>
            new ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>>(item1, item2, item3, item4, item5, item6, item7, ValueTuple.Create(item8));
    }

    /// <summary>Represents a 1-tuple, or singleton, as a value type.</summary>
    /// <typeparam name="T1">The type of the tuple's only component.</typeparam>
    [Serializable]
    public struct ValueTuple<T1>
        : IEquatable<ValueTuple<T1>>, IComparable, IComparable<ValueTuple<T1>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1}"/> instance's first component.
        /// </summary>
        public T1 Item1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        public ValueTuple(T1 item1)
        {
            Item1 = item1;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1> && Equals((ValueTuple<T1>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its field
        /// is equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            var objTuple = (ValueTuple<T1>)other;

            return Comparer<T1>.Default.Compare(Item1, objTuple.Item1);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1> other)
        {
            return Comparer<T1>.Default.Compare(Item1, other.Item1);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Item1?.GetHashCode() ?? 0;
        }

        int IValueTupleInternal.Length => 1;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(Item1!);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1)</c>,
        /// where <c>Item1</c> represents the value of <see cref="Item1"/>. If the field is <see langword="null"/>,
        /// it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a 2-tuple, or pair, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2>
        : IEquatable<ValueTuple<T1, T2>>, IComparable, IComparable<ValueTuple<T1, T2>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2}"/> instance's first component.
        /// </summary>
        public T1 Item1;

        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2}"/> instance's second component.
        /// </summary>
        public T2 Item2;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        public ValueTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        ///
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2> && Equals((ValueTuple<T1, T2>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2}"/> instance is equal to a specified <see cref="ValueTuple{T1, T2}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            return Comparer<T2>.Default.Compare(Item2, other.Item2);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                    Item2?.GetHashCode() ?? 0);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            return HashCode.Combine(comparer.GetHashCode(Item1!),
                                    comparer.GetHashCode(Item2!));
        }

        int IValueTupleInternal.Length => 1;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2)</c>,
        /// where <c>Item1</c> and <c>Item2</c> represent the values of the <see cref="Item1"/>
        /// and <see cref="Item2"/> fields. If either field value is <see langword="null"/>,
        /// it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a 3-tuple, or triple, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2, T3>
        : IEquatable<ValueTuple<T1, T2, T3>>, IComparable, IComparable<ValueTuple<T1, T2, T3>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3}"/> instance's first component.
        /// </summary>
        public T1 Item1;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3}"/> instance's second component.
        /// </summary>
        public T2 Item2;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3}"/> instance's third component.
        /// </summary>
        public T3 Item3;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        public ValueTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2, T3> && Equals((ValueTuple<T1, T2, T3>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2, T3> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2, T3>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2, T3>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2, T3> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            c = Comparer<T2>.Default.Compare(Item2, other.Item2);
            if (c != 0) return c;

            return Comparer<T3>.Default.Compare(Item3, other.Item3);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                    Item2?.GetHashCode() ?? 0,
                                    Item3?.GetHashCode() ?? 0);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            return HashCode.Combine(comparer.GetHashCode(Item1!),
                                    comparer.GetHashCode(Item2!),
                                    comparer.GetHashCode(Item3!));
        }

        int IValueTupleInternal.Length => 2;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2, Item3)</c>.
        /// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a 4-tuple, or quadruple, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2, T3, T4>
        : IEquatable<ValueTuple<T1, T2, T3, T4>>, IComparable, IComparable<ValueTuple<T1, T2, T3, T4>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's first component.
        /// </summary>
        public T1 Item1;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's second component.
        /// </summary>
        public T2 Item2;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's third component.
        /// </summary>
        public T3 Item3;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's fourth component.
        /// </summary>
        public T4 Item4;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component.</param>
        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2, T3, T4> && Equals((ValueTuple<T1, T2, T3, T4>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2, T3, T4> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2, T3, T4>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2, T3, T4>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2, T3, T4> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            c = Comparer<T2>.Default.Compare(Item2, other.Item2);
            if (c != 0) return c;

            c = Comparer<T3>.Default.Compare(Item3, other.Item3);
            if (c != 0) return c;

            return Comparer<T4>.Default.Compare(Item4, other.Item4);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                    Item2?.GetHashCode() ?? 0,
                                    Item3?.GetHashCode() ?? 0,
                                    Item4?.GetHashCode() ?? 0);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            return HashCode.Combine(comparer.GetHashCode(Item1!),
                                    comparer.GetHashCode(Item2!),
                                    comparer.GetHashCode(Item3!),
                                    comparer.GetHashCode(Item4!));
        }

        int IValueTupleInternal.Length => 3;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4)</c>.
        /// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a 5-tuple, or quintuple, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2, T3, T4, T5>
        : IEquatable<ValueTuple<T1, T2, T3, T4, T5>>, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's first component.
        /// </summary>
        public T1 Item1;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's second component.
        /// </summary>
        public T2 Item2;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's third component.
        /// </summary>
        public T3 Item3;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's fourth component.
        /// </summary>
        public T4 Item4;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's fifth component.
        /// </summary>
        public T5 Item5;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component.</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2, T3, T4, T5> && Equals((ValueTuple<T1, T2, T3, T4, T5>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2, T3, T4, T5> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
                && EqualityComparer<T5>.Default.Equals(Item5, other.Item5);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2, T3, T4, T5>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2, T3, T4, T5>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2, T3, T4, T5> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            c = Comparer<T2>.Default.Compare(Item2, other.Item2);
            if (c != 0) return c;

            c = Comparer<T3>.Default.Compare(Item3, other.Item3);
            if (c != 0) return c;

            c = Comparer<T4>.Default.Compare(Item4, other.Item4);
            if (c != 0) return c;

            return Comparer<T5>.Default.Compare(Item5, other.Item5);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                    Item2?.GetHashCode() ?? 0,
                                    Item3?.GetHashCode() ?? 0,
                                    Item4?.GetHashCode() ?? 0,
                                    Item5?.GetHashCode() ?? 0);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            return HashCode.Combine(comparer.GetHashCode(Item1!),
                                    comparer.GetHashCode(Item2!),
                                    comparer.GetHashCode(Item3!),
                                    comparer.GetHashCode(Item4!),
                                    comparer.GetHashCode(Item5!));
        }

        int IValueTupleInternal.Length => 4;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5)</c>.
        /// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a 6-tuple, or sixtuple, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2, T3, T4, T5, T6>
        : IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6>>, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's first component.
        /// </summary>
        public T1 Item1;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's second component.
        /// </summary>
        public T2 Item2;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's third component.
        /// </summary>
        public T3 Item3;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's fourth component.
        /// </summary>
        public T4 Item4;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's fifth component.
        /// </summary>
        public T5 Item5;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's sixth component.
        /// </summary>
        public T6 Item6;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component.</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        /// <param name="item6">The value of the tuple's sixth component.</param>
        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2, T3, T4, T5, T6> && Equals((ValueTuple<T1, T2, T3, T4, T5, T6>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
                && EqualityComparer<T5>.Default.Equals(Item5, other.Item5)
                && EqualityComparer<T6>.Default.Equals(Item6, other.Item6);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2, T3, T4, T5, T6>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2, T3, T4, T5, T6>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            c = Comparer<T2>.Default.Compare(Item2, other.Item2);
            if (c != 0) return c;

            c = Comparer<T3>.Default.Compare(Item3, other.Item3);
            if (c != 0) return c;

            c = Comparer<T4>.Default.Compare(Item4, other.Item4);
            if (c != 0) return c;

            c = Comparer<T5>.Default.Compare(Item5, other.Item5);
            if (c != 0) return c;

            return Comparer<T6>.Default.Compare(Item6, other.Item6);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                    Item2?.GetHashCode() ?? 0,
                                    Item3?.GetHashCode() ?? 0,
                                    Item4?.GetHashCode() ?? 0,
                                    Item5?.GetHashCode() ?? 0,
                                    Item6?.GetHashCode() ?? 0);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            return HashCode.Combine(comparer.GetHashCode(Item1!),
                                    comparer.GetHashCode(Item2!),
                                    comparer.GetHashCode(Item3!),
                                    comparer.GetHashCode(Item4!),
                                    comparer.GetHashCode(Item5!),
                                    comparer.GetHashCode(Item6!));
        }

        int IValueTupleInternal.Length => 6;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6)</c>.
        /// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a 7-tuple, or sentuple, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7>
        : IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>, IValueTupleInternal
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's first component.
        /// </summary>
        public T1 Item1;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's second component.
        /// </summary>
        public T2 Item2;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's third component.
        /// </summary>
        public T3 Item3;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's fourth component.
        /// </summary>
        public T4 Item4;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's fifth component.
        /// </summary>
        public T5 Item5;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's sixth component.
        /// </summary>
        public T6 Item6;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's seventh component.
        /// </summary>
        public T7 Item7;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component.</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        /// <param name="item6">The value of the tuple's sixth component.</param>
        /// <param name="item7">The value of the tuple's seventh component.</param>
        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7> && Equals((ValueTuple<T1, T2, T3, T4, T5, T6, T7>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6, T7> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
                && EqualityComparer<T5>.Default.Equals(Item5, other.Item5)
                && EqualityComparer<T6>.Default.Equals(Item6, other.Item6)
                && EqualityComparer<T7>.Default.Equals(Item7, other.Item7);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2, T3, T4, T5, T6, T7>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2, T3, T4, T5, T6, T7>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6, T7> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            c = Comparer<T2>.Default.Compare(Item2, other.Item2);
            if (c != 0) return c;

            c = Comparer<T3>.Default.Compare(Item3, other.Item3);
            if (c != 0) return c;

            c = Comparer<T4>.Default.Compare(Item4, other.Item4);
            if (c != 0) return c;

            c = Comparer<T5>.Default.Compare(Item5, other.Item5);
            if (c != 0) return c;

            c = Comparer<T6>.Default.Compare(Item6, other.Item6);
            if (c != 0) return c;

            return Comparer<T7>.Default.Compare(Item7, other.Item7);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                    Item2?.GetHashCode() ?? 0,
                                    Item3?.GetHashCode() ?? 0,
                                    Item4?.GetHashCode() ?? 0,
                                    Item5?.GetHashCode() ?? 0,
                                    Item6?.GetHashCode() ?? 0,
                                    Item7?.GetHashCode() ?? 0);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            return HashCode.Combine(comparer.GetHashCode(Item1!),
                                    comparer.GetHashCode(Item2!),
                                    comparer.GetHashCode(Item3!),
                                    comparer.GetHashCode(Item4!),
                                    comparer.GetHashCode(Item5!),
                                    comparer.GetHashCode(Item6!),
                                    comparer.GetHashCode(Item7!));
        }

        int IValueTupleInternal.Length => 7;
        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6, Item7)</c>.
        /// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ", " + Item7?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ", " + Item7?.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents an 8-tuple, or octuple, as a value type.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    /// <typeparam name="TRest">The type of the tuple's eighth component.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
    : IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>, IValueTupleInternal
    where TRest : struct
    {
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's first component.
        /// </summary>
        public T1 Item1;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's second component.
        /// </summary>
        public T2 Item2;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's third component.
        /// </summary>
        public T3 Item3;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's fourth component.
        /// </summary>
        public T4 Item4;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's fifth component.
        /// </summary>
        public T5 Item5;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's sixth component.
        /// </summary>
        public T6 Item6;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's seventh component.
        /// </summary>
        public T7 Item7;
        /// <summary>
        /// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's eighth component.
        /// </summary>
        public TRest Rest;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> value type.
        /// </summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component.</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        /// <param name="item6">The value of the tuple's sixth component.</param>
        /// <param name="item7">The value of the tuple's seventh component.</param>
        /// <param name="rest">The value of the tuple's eight component.</param>
        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
        {
            if (!(rest is IValueTupleInternal))
            {
                throw new ArgumentException("Last argument of an 8-tuple is not a ValueTuple", nameof(rest));
            }

            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Rest = rest;
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
        /// <list type="bullet">
        ///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> value type.</description></item>
        ///     <item><description>Its components are of the same types as those of the current instance.</description></item>
        ///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
        /// </list>
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> && Equals((ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/>
        /// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/>.
        /// </summary>
        /// <param name="other">The tuple to compare with this instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to the specified tuple; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The <paramref name="other"/> parameter is considered to be equal to the current instance if each of its fields
        /// are equal to that of the current instance, using the default comparer for that field's type.
        /// </remarks>
        public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
                && EqualityComparer<T5>.Default.Equals(Item5, other.Item5)
                && EqualityComparer<T6>.Default.Equals(Item6, other.Item6)
                && EqualityComparer<T7>.Default.Equals(Item7, other.Item7)
                && EqualityComparer<TRest>.Default.Equals(Rest, other.Rest);
        }

        int IComparable.CompareTo(object? other)
        {
            if (other == null) return 1;

            if (!(other is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>))
            {
                throw new ArgumentException("Object to compare is not a ValueTuple", nameof(other));
            }

            return CompareTo((ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>)other);
        }

        /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
        /// <param name="other">An instance to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// Returns less than zero if this instance is less than <paramref name="other"/>, zero if this
        /// instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater
        /// than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            c = Comparer<T2>.Default.Compare(Item2, other.Item2);
            if (c != 0) return c;

            c = Comparer<T3>.Default.Compare(Item3, other.Item3);
            if (c != 0) return c;

            c = Comparer<T4>.Default.Compare(Item4, other.Item4);
            if (c != 0) return c;

            c = Comparer<T5>.Default.Compare(Item5, other.Item5);
            if (c != 0) return c;

            c = Comparer<T6>.Default.Compare(Item6, other.Item6);
            if (c != 0) return c;

            c = Comparer<T7>.Default.Compare(Item7, other.Item7);
            if (c != 0) return c;

            return Comparer<TRest>.Default.Compare(Rest, other.Rest);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            // We want to have a limited hash in this case. We'll use the first 7 elements of the tuple
            if (!(Rest is IValueTupleInternal))
            {
                return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                        Item2?.GetHashCode() ?? 0,
                                        Item3?.GetHashCode() ?? 0,
                                        Item4?.GetHashCode() ?? 0,
                                        Item5?.GetHashCode() ?? 0,
                                        Item6?.GetHashCode() ?? 0,
                                        Item7?.GetHashCode() ?? 0);
            }

            int size = ((IValueTupleInternal)Rest).Length;
            int restHashCode = Rest.GetHashCode();
            if (size >= 8)
            {
                return restHashCode;
            }

            // In this case, the rest member has less than 8 elements so we need to combine some of our elements with the elements in rest
            int k = 8 - size;
            switch (k)
            {
                case 1:
                    return HashCode.Combine(Item7?.GetHashCode() ?? 0,
                                            restHashCode);
                case 2:
                    return HashCode.Combine(Item6?.GetHashCode() ?? 0,
                                            Item7?.GetHashCode() ?? 0,
                                            restHashCode);
                case 3:
                    return HashCode.Combine(Item5?.GetHashCode() ?? 0,
                                            Item6?.GetHashCode() ?? 0,
                                            Item7?.GetHashCode() ?? 0,
                                            restHashCode);
                case 4:
                    return HashCode.Combine(Item4?.GetHashCode() ?? 0,
                                            Item5?.GetHashCode() ?? 0,
                                            Item6?.GetHashCode() ?? 0,
                                            Item7?.GetHashCode() ?? 0,
                                            restHashCode);
                case 5:
                    return HashCode.Combine(Item3?.GetHashCode() ?? 0,
                                            Item4?.GetHashCode() ?? 0,
                                            Item5?.GetHashCode() ?? 0,
                                            Item6?.GetHashCode() ?? 0,
                                            Item7?.GetHashCode() ?? 0,
                                            restHashCode);
                case 6:
                    return HashCode.Combine(Item2?.GetHashCode() ?? 0,
                                            Item3?.GetHashCode() ?? 0,
                                            Item4?.GetHashCode() ?? 0,
                                            Item5?.GetHashCode() ?? 0,
                                            Item6?.GetHashCode() ?? 0,
                                            Item7?.GetHashCode() ?? 0,
                                            restHashCode);
                case 7:
                case 8:
                    return HashCode.Combine(Item1?.GetHashCode() ?? 0,
                                            Item2?.GetHashCode() ?? 0,
                                            Item3?.GetHashCode() ?? 0,
                                            Item4?.GetHashCode() ?? 0,
                                            Item5?.GetHashCode() ?? 0,
                                            Item6?.GetHashCode() ?? 0,
                                            Item7?.GetHashCode() ?? 0,
                                            restHashCode);
            }

            Debug.Fail("Missed all cases for computing ValueTuple hash code");
            return -1;
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            // We want to have a limited hash in this case. We'll use the first 7 elements of the tuple
            if (!(Rest is IValueTupleInternal rest))
            {
                return HashCode.Combine(comparer.GetHashCode(Item1!), comparer.GetHashCode(Item2!), comparer.GetHashCode(Item3!),
                                        comparer.GetHashCode(Item4!), comparer.GetHashCode(Item5!), comparer.GetHashCode(Item6!),
                                        comparer.GetHashCode(Item7!));
            }

            int size = rest.Length;
            int restHashCode = rest.GetHashCode(comparer);
            if (size >= 8)
            {
                return restHashCode;
            }

            // In this case, the rest member has less than 8 elements so we need to combine some our elements with the elements in rest
            int k = 8 - size;
            switch (k)
            {
                case 1:
                    return HashCode.Combine(comparer.GetHashCode(Item7!),
                                            restHashCode);
                case 2:
                    return HashCode.Combine(comparer.GetHashCode(Item6!),
                                            comparer.GetHashCode(Item7!),
                                            restHashCode);
                case 3:
                    return HashCode.Combine(comparer.GetHashCode(Item5!),
                                            comparer.GetHashCode(Item6!),
                                            comparer.GetHashCode(Item7!),
                                            restHashCode);
                case 4:
                    return HashCode.Combine(comparer.GetHashCode(Item4!),
                                            comparer.GetHashCode(Item5!),
                                            comparer.GetHashCode(Item6!),
                                            comparer.GetHashCode(Item7!),
                                            restHashCode);
                case 5:
                    return HashCode.Combine(comparer.GetHashCode(Item3!),
                                            comparer.GetHashCode(Item4!),
                                            comparer.GetHashCode(Item5!),
                                            comparer.GetHashCode(Item6!),
                                            comparer.GetHashCode(Item7!),
                                            restHashCode);
                case 6:
                    return HashCode.Combine(comparer.GetHashCode(Item2!),
                                            comparer.GetHashCode(Item3!),
                                            comparer.GetHashCode(Item4!),
                                            comparer.GetHashCode(Item5!),
                                            comparer.GetHashCode(Item6!),
                                            comparer.GetHashCode(Item7!),
                                            restHashCode);
                case 7:
                case 8:
                    return HashCode.Combine(comparer.GetHashCode(Item1!),
                                            comparer.GetHashCode(Item2!),
                                            comparer.GetHashCode(Item3!),
                                            comparer.GetHashCode(Item4!),
                                            comparer.GetHashCode(Item5!),
                                            comparer.GetHashCode(Item6!),
                                            comparer.GetHashCode(Item7!),
                                            restHashCode);
            }

            Debug.Fail("Missed all cases for computing ValueTuple hash code");
            return -1;
        }

        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance.
        /// </summary>
        /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance.</returns>
        /// <remarks>
        /// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6, Item7, Rest)</c>.
        /// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
        /// </remarks>
        public override string ToString()
        {
            if (Rest is IValueTupleInternal)
            {
                return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ", " + Item7?.ToString() + ", " + ((IValueTupleInternal)Rest).ToStringEnd();
            }

            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ", " + Item7?.ToString() + ", " + Rest.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            if (Rest is IValueTupleInternal)
            {
                return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ", " + Item7?.ToString() + ", " + ((IValueTupleInternal)Rest).ToStringEnd();
            }

            return Item1?.ToString() + ", " + Item2?.ToString() + ", " + Item3?.ToString() + ", " + Item4?.ToString() + ", " + Item5?.ToString() + ", " + Item6?.ToString() + ", " + Item7?.ToString() + ", " + Rest.ToString() + ")";
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int IValueTupleInternal.Length => Rest is IValueTupleInternal ? 7 + ((IValueTupleInternal)Rest).Length : 8;
    }
}