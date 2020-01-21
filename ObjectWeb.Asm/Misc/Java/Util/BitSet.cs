/*
* Copyright (c) 1995, 2014, Oracle and/or its affiliates. All rights reserved.
* ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*/

using System;
using System.Diagnostics;
using System.Text;
using ObjectWeb.Misc.Java.IO;
using ObjectWeb.Misc.Java.Nio;

namespace ObjectWeb.Misc.Java.Util
{
    /// <summary>This class implements a vector of bits that grows as needed.</summary>
    /// <remarks>
    ///     This class implements a vector of bits that grows as needed. Each
    ///     component of the bit set has a
    ///     <c>boolean</c>
    ///     value. The
    ///     bits of a
    ///     <c>BitSet</c>
    ///     are indexed by nonnegative integers.
    ///     Individual indexed bits can be examined, set, or cleared. One
    ///     <c>BitSet</c>
    ///     may be used to modify the contents of another
    ///     <c>BitSet</c>
    ///     through logical AND, logical inclusive OR, and
    ///     logical exclusive OR operations.
    ///     <p>
    ///         By default, all bits in the set initially have the value
    ///         <see langword="false" />
    ///         .
    ///         <p>
    ///             Every bit set has a current size, which is the number of bits
    ///             of space currently in use by the bit set. Note that the size is
    ///             related to the implementation of a bit set, so it may change with
    ///             implementation. The length of a bit set relates to logical length
    ///             of a bit set and is defined independently of implementation.
    ///             <p>
    ///                 Unless otherwise noted, passing a null parameter to any of the
    ///                 methods in a
    ///                 <c>BitSet</c>
    ///                 will result in a
    ///                 <c>NullPointerException</c>
    ///                 .
    ///                 <p>
    ///                     A
    ///                     <c>BitSet</c>
    ///                     is not safe for multithreaded use without
    ///                     external synchronization.
    /// </remarks>
    /// <author>Arthur van Hoff</author>
    /// <author>Michael McCloskey</author>
    /// <author>Martin Buchholz</author>
    /// <since>JDK1.0</since>
    [Serializable]
    public class BitSet : ICloneable
    {
        private const int Address_Bits_Per_Word = 6;

        private const int Bits_Per_Word = 1 << Address_Bits_Per_Word;

        private const int Bit_Index_Mask = Bits_Per_Word - 1;

        private const long Word_Mask = unchecked((long) 0xffffffffffffffffL);

        private const long serialVersionUID = 7997698588986878753L;

        /// <serialField>
        ///     bits long[]
        ///     The bits in this BitSet.  The ith bit is stored in bits[i/64] at
        ///     bit position i % 64 (where bit position 0 refers to the least
        ///     significant bit and 63 refers to the most significant bit).
        /// </serialField>
        private static readonly ObjectStreamField[] serialPersistentFields =
            {new ObjectStreamField("bits", typeof(long[]))};

        /// <summary>Whether the size of "words" is user-specified.</summary>
        /// <remarks>
        ///     Whether the size of "words" is user-specified.  If so, we assume
        ///     the user knows what he's doing and try harder to preserve it.
        /// </remarks>
        [NonSerialized] private bool sizeIsSticky;

        /// <summary>The internal field corresponding to the serialField "bits".</summary>
        private long[] words;

        /// <summary>The number of words in the logical size of this BitSet.</summary>
        [NonSerialized] private int wordsInUse;

        /// <summary>Creates a new bit set.</summary>
        /// <remarks>
        ///     Creates a new bit set. All bits are initially
        ///     <see langword="false" />
        ///     .
        /// </remarks>
        public BitSet()
        {
            // The new logical size
            InitWords(Bits_Per_Word);
            sizeIsSticky = false;
        }

        /// <summary>
        ///     Creates a bit set whose initial size is large enough to explicitly
        ///     represent bits with indices in the range
        ///     <c>0</c>
        ///     through
        ///     <c>nbits-1</c>
        ///     . All bits are initially
        ///     <see langword="false" />
        ///     .
        /// </summary>
        /// <param name="nbits">the initial size of the bit set</param>
        /// <exception cref="Java.Lang.NegativeArraySizeException">
        ///     if the specified initial size
        ///     is negative
        /// </exception>
        public BitSet(int nbits)
        {
            // nbits can't be negative; size 0 is OK
            if (nbits < 0) throw new NegativeArraySizeException("nbits < 0: " + nbits);
            InitWords(nbits);
            sizeIsSticky = true;
        }

        /// <summary>Creates a bit set using words as the internal representation.</summary>
        /// <remarks>
        ///     Creates a bit set using words as the internal representation.
        ///     The last word (if there is one) must be non-zero.
        /// </remarks>
        private BitSet(long[] words)
        {
            this.words = words;
            wordsInUse = words.Length;
            CheckInvariants();
        }

        /// <summary>
        ///     Cloning this
        ///     <c>BitSet</c>
        ///     produces a new
        ///     <c>BitSet</c>
        ///     that is equal to it.
        ///     The clone of the bit set is another bit set that has exactly the
        ///     same bits set to
        ///     <see langword="true" />
        ///     as this bit set.
        /// </summary>
        /// <returns>a clone of this bit set</returns>
        /// <seealso cref="Size()" />
        public virtual object Clone()
        {
            if (!sizeIsSticky) TrimToSize();
            var result = (BitSet) MemberwiseClone();
            result.words = (long[]) words.Clone();
            result.CheckInvariants();
            return result;
        }

        /*
        * BitSets are packed into arrays of "words."  Currently a word is
        * a long, which consists of 64 bits, requiring 6 address bits.
        * The choice of word size is determined purely by performance concerns.
        */
        /* Used to shift left or right for a partial word mask */
        /* use serialVersionUID from JDK 1.0.2 for interoperability */
        /// <summary>Given a bit index, return word index containing it.</summary>
        private static int WordIndex(int bitIndex)
        {
            return bitIndex >> Address_Bits_Per_Word;
        }

        /// <summary>Every public method must preserve these invariants.</summary>
        private void CheckInvariants()
        {
            Debug.Assert(wordsInUse == 0 || words[wordsInUse - 1] != 0);
            Debug.Assert(wordsInUse >= 0 && wordsInUse <= words.Length);
            Debug.Assert(wordsInUse == words.Length || words[wordsInUse]
                         == 0);
        }

        /// <summary>Sets the field wordsInUse to the logical size in words of the bit set.</summary>
        /// <remarks>
        ///     Sets the field wordsInUse to the logical size in words of the bit set.
        ///     WARNING:This method assumes that the number of words actually in use is
        ///     less than or equal to the current value of wordsInUse!
        /// </remarks>
        private void RecalculateWordsInUse()
        {
            // Traverse the bitset until a used word is found
            int i;
            for (i = wordsInUse - 1; i >= 0; i--)
                if (words[i] != 0)
                    break;
            wordsInUse = i + 1;
        }

        private void InitWords(int nbits)
        {
            words = new long[WordIndex(nbits - 1) + 1];
        }

        /// <summary>Returns a new bit set containing all the bits in the given long array.</summary>
        /// <remarks>
        ///     Returns a new bit set containing all the bits in the given long array.
        ///     <p>
        ///         More precisely,
        ///         <br />
        ///         <c>BitSet.valueOf(longs).get(n) == ((longs[n/64] & (1L&lt;&lt;(n%64))) != 0)</c>
        ///         <br />for all
        ///         <c>n &lt; 64 * longs.length</c>
        ///         .
        ///         <p>
        ///             This method is equivalent to
        ///             <c>BitSet.valueOf(LongBuffer.wrap(longs))</c>
        ///             .
        /// </remarks>
        /// <param name="longs">
        ///     a long array containing a little-endian representation
        ///     of a sequence of bits to be used as the initial bits of the
        ///     new bit set
        /// </param>
        /// <returns>
        ///     a
        ///     <c>BitSet</c>
        ///     containing all the bits in the long array
        /// </returns>
        /// <since>1.7</since>
        public static BitSet ValueOf(long[] longs)
        {
            int n;
            for (n = longs.Length; n > 0 && longs[n - 1] == 0; n--)
            {
            }

            return new BitSet(Arrays.CopyOf(longs, n));
        }

        /// <summary>
        ///     Returns a new bit set containing all the bits in the given long
        ///     buffer between its position and limit.
        /// </summary>
        /// <remarks>
        ///     Returns a new bit set containing all the bits in the given long
        ///     buffer between its position and limit.
        ///     <p>
        ///         More precisely,
        ///         <br />
        ///         <c>
        ///             BitSet.valueOf(lb).get(n) == ((lb.get(lb.position()+n/64) & (1L&lt;&lt;(n%64))) != 0)
        ///         </c>
        ///         <br />for all
        ///         <c>n &lt; 64 * lb.remaining()</c>
        ///         .
        ///         <p>
        ///             The long buffer is not modified by this method, and no
        ///             reference to the buffer is retained by the bit set.
        /// </remarks>
        /// <param name="lb">
        ///     a long buffer containing a little-endian representation
        ///     of a sequence of bits between its position and limit, to be
        ///     used as the initial bits of the new bit set
        /// </param>
        /// <returns>
        ///     a
        ///     <c>BitSet</c>
        ///     containing all the bits in the buffer in the
        ///     specified range
        /// </returns>
        /// <since>1.7</since>
        public static BitSet ValueOf(LongBuffer lb)
        {
            lb = lb.Slice();
            int n;
            for (n = lb.Remaining(); n > 0 && lb.Get(n - 1) == 0; n--)
            {
            }

            var words = new long[n];
            lb.Get(words);
            return new BitSet(words);
        }

        /// <summary>Returns a new bit set containing all the bits in the given byte array.</summary>
        /// <remarks>
        ///     Returns a new bit set containing all the bits in the given byte array.
        ///     <p>
        ///         More precisely,
        ///         <br />
        ///         <c>BitSet.valueOf(bytes).get(n) == ((bytes[n/8] & (1&lt;&lt;(n%8))) != 0)</c>
        ///         <br />for all
        ///         <c>n &lt;  8 * bytes.length</c>
        ///         .
        ///         <p>
        ///             This method is equivalent to
        ///             <c>BitSet.valueOf(ByteBuffer.wrap(bytes))</c>
        ///             .
        /// </remarks>
        /// <param name="bytes">
        ///     a byte array containing a little-endian
        ///     representation of a sequence of bits to be used as the
        ///     initial bits of the new bit set
        /// </param>
        /// <returns>
        ///     a
        ///     <c>BitSet</c>
        ///     containing all the bits in the byte array
        /// </returns>
        /// <since>1.7</since>
        [Obsolete("Not Implemented", true)]
        public static BitSet ValueOf(byte[] bytes)
        {
            return ValueOf(ByteBuffer.Wrap(bytes));
        }

        /// <summary>
        ///     Returns a new bit set containing all the bits in the given byte
        ///     buffer between its position and limit.
        /// </summary>
        /// <remarks>
        ///     Returns a new bit set containing all the bits in the given byte
        ///     buffer between its position and limit.
        ///     <p>
        ///         More precisely,
        ///         <br />
        ///         <c>
        ///             BitSet.valueOf(bb).get(n) == ((bb.get(bb.position()+n/8) & (1&lt;&lt;(n%8))) != 0)
        ///         </c>
        ///         <br />for all
        ///         <c>n &lt; 8 * bb.remaining()</c>
        ///         .
        ///         <p>
        ///             The byte buffer is not modified by this method, and no
        ///             reference to the buffer is retained by the bit set.
        /// </remarks>
        /// <param name="bb">
        ///     a byte buffer containing a little-endian representation
        ///     of a sequence of bits between its position and limit, to be
        ///     used as the initial bits of the new bit set
        /// </param>
        /// <returns>
        ///     a
        ///     <c>BitSet</c>
        ///     containing all the bits in the buffer in the
        ///     specified range
        /// </returns>
        /// <since>1.7</since>
        public static BitSet ValueOf(ByteBuffer bb)
        {
            bb = bb.Slice().Order(ByteOrder.Little_Endian);
            int n;
            for (n = bb.Remaining(); n > 0 && bb.Get(n - 1) == 0; n--)
            {
            }

            var words = new long[(n + 7) / 8];
            bb.Limit(n);
            var i = 0;
            while (bb.Remaining() >= 8) words[i++] = bb.GetLong();
            for (int remaining = bb.Remaining(), j = 0; j < remaining; j++) words[i] |= (bb.Get() & 0xffL) << (8 * j);
            return new BitSet(words);
        }

        /*
        /// <summary>Returns a new byte array containing all the bits in this bit set.</summary>
        /// <remarks>
        /// Returns a new byte array containing all the bits in this bit set.
        /// <p>More precisely, if
        /// <br />
        /// <c>byte[] bytes = s.toByteArray();</c>
        /// <br />then
        /// <c>bytes.length == (s.length()+7)/8</c>
        /// and
        /// <br />
        /// <c>s.get(n) == ((bytes[n/8] & (1&lt;&lt;(n%8))) != 0)</c>
        /// <br />for all
        /// <c>n &lt; 8 * bytes.length</c>
        /// .
        /// </remarks>
        /// <returns>
        /// a byte array containing a little-endian representation
        /// of all the bits in this bit set
        /// </returns>
        /// <since>1.7</since>
        public virtual byte[] ToByteArray()
        {
            int n = wordsInUse;
            if (n == 0)
            {
                return new byte[0];
            }
            int len = 8 * (n - 1);
            for (long x = words[n - 1]; x != 0; x = (long)(((ulong)x) >> 8))
            {
                len++;
            }
            byte[] bytes = new byte[len];
            ByteBuffer bb = LongBuffer.Wrap(bytes.Select(c => (long)c).ToArray()).Order(ByteOrder.Little_Endian);
            for (int i = 0; i < n - 1; i++)
            {
                bb.PutLong(words[i]);
            }
            for (long x = words[n - 1]; x != 0; x = (long)(((ulong)x) >> 8))
            {
                bb.Put(unchecked((byte)(x & unchecked((int)(0xff)))));
            }
            return bytes;
        }
        */

        /// <summary>Returns a new long array containing all the bits in this bit set.</summary>
        /// <remarks>
        ///     Returns a new long array containing all the bits in this bit set.
        ///     <p>
        ///         More precisely, if
        ///         <br />
        ///         <c>long[] longs = s.toLongArray();</c>
        ///         <br />then
        ///         <c>longs.length == (s.length()+63)/64</c>
        ///         and
        ///         <br />
        ///         <c>s.get(n) == ((longs[n/64] & (1L&lt;&lt;(n%64))) != 0)</c>
        ///         <br />for all
        ///         <c>n &lt; 64 * longs.length</c>
        ///         .
        /// </remarks>
        /// <returns>
        ///     a long array containing a little-endian representation
        ///     of all the bits in this bit set
        /// </returns>
        /// <since>1.7</since>
        public virtual long[] ToLongArray()
        {
            return Arrays.CopyOf(words, wordsInUse);
        }

        /// <summary>Ensures that the BitSet can hold enough words.</summary>
        /// <param name="wordsRequired">the minimum acceptable number of words.</param>
        private void EnsureCapacity(int wordsRequired)
        {
            if (words.Length < wordsRequired)
            {
                // Allocate larger of doubled size or required size
                var request = Math.Max(2 * words.Length, wordsRequired);
                words = Arrays.CopyOf(words, request);
                sizeIsSticky = false;
            }
        }

        /// <summary>
        ///     Ensures that the BitSet can accommodate a given wordIndex,
        ///     temporarily violating the invariants.
        /// </summary>
        /// <remarks>
        ///     Ensures that the BitSet can accommodate a given wordIndex,
        ///     temporarily violating the invariants.  The caller must
        ///     restore the invariants before returning to the user,
        ///     possibly using recalculateWordsInUse().
        /// </remarks>
        /// <param name="wordIndex">the index to be accommodated.</param>
        private void ExpandTo(int wordIndex)
        {
            var wordsRequired = wordIndex + 1;
            if (wordsInUse < wordsRequired)
            {
                EnsureCapacity(wordsRequired);
                wordsInUse = wordsRequired;
            }
        }

        /// <summary>Checks that fromIndex ...</summary>
        /// <remarks>Checks that fromIndex ... toIndex is a valid range of bit indices.</remarks>
        private static void CheckRange(int fromIndex, int toIndex)
        {
            if (fromIndex < 0) throw new IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
            if (toIndex < 0) throw new IndexOutOfRangeException("toIndex < 0: " + toIndex);
            if (fromIndex > toIndex)
                throw new IndexOutOfRangeException("fromIndex: " + fromIndex + " > toIndex: " + toIndex
                );
        }

        /// <summary>
        ///     Sets the bit at the specified index to the complement of its
        ///     current value.
        /// </summary>
        /// <param name="bitIndex">the index of the bit to flip</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        /// <since>1.4</since>
        public virtual void Flip(int bitIndex)
        {
            if (bitIndex < 0) throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            var wordIndex = WordIndex(bitIndex);
            ExpandTo(wordIndex);
            words[wordIndex] ^= 1L << bitIndex;
            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>
        ///     Sets each bit from the specified
        ///     <paramref name="fromIndex" />
        ///     (inclusive) to the
        ///     specified
        ///     <paramref name="toIndex" />
        ///     (exclusive) to the complement of its current
        ///     value.
        /// </summary>
        /// <param name="fromIndex">index of the first bit to flip</param>
        /// <param name="toIndex">index after the last bit to flip</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if
        ///     <paramref name="fromIndex" />
        ///     is negative,
        ///     or
        ///     <paramref name="toIndex" />
        ///     is negative, or
        ///     <paramref name="fromIndex" />
        ///     is
        ///     larger than
        ///     <paramref name="toIndex" />
        /// </exception>
        /// <since>1.4</since>
        public virtual void Flip(int fromIndex, int toIndex)
        {
            CheckRange(fromIndex, toIndex);
            if (fromIndex == toIndex) return;
            var startWordIndex = WordIndex(fromIndex);
            var endWordIndex = WordIndex(toIndex - 1);
            ExpandTo(endWordIndex);
            var firstWordMask = Word_Mask << fromIndex;
            var lastWordMask = (long) (unchecked((ulong) Word_Mask) >> -toIndex);
            if (startWordIndex == endWordIndex)
            {
                // Case 1: One word
                words[startWordIndex] ^= firstWordMask & lastWordMask;
            }
            else
            {
                // Case 2: Multiple words
                // Handle first word
                words[startWordIndex] ^= firstWordMask;
                // Handle intermediate words, if any
                for (var i = startWordIndex + 1; i < endWordIndex; i++) words[i] ^= Word_Mask;
                // Handle last word
                words[endWordIndex] ^= lastWordMask;
            }

            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>
        ///     Sets the bit at the specified index to
        ///     <see langword="true" />
        ///     .
        /// </summary>
        /// <param name="bitIndex">a bit index</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        /// <since>JDK1.0</since>
        public virtual void Set(int bitIndex)
        {
            if (bitIndex < 0) throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            var wordIndex = WordIndex(bitIndex);
            ExpandTo(wordIndex);
            words[wordIndex] |= 1L << bitIndex;
            // Restores invariants
            CheckInvariants();
        }

        /// <summary>Sets the bit at the specified index to the specified value.</summary>
        /// <param name="bitIndex">a bit index</param>
        /// <param name="value">a boolean value to set</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        /// <since>1.4</since>
        public virtual void Set(int bitIndex, bool value)
        {
            if (value)
                Set(bitIndex);
            else
                Clear(bitIndex);
        }

        /// <summary>
        ///     Sets the bits from the specified
        ///     <paramref name="fromIndex" />
        ///     (inclusive) to the
        ///     specified
        ///     <paramref name="toIndex" />
        ///     (exclusive) to
        ///     <see langword="true" />
        ///     .
        /// </summary>
        /// <param name="fromIndex">index of the first bit to be set</param>
        /// <param name="toIndex">index after the last bit to be set</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if
        ///     <paramref name="fromIndex" />
        ///     is negative,
        ///     or
        ///     <paramref name="toIndex" />
        ///     is negative, or
        ///     <paramref name="fromIndex" />
        ///     is
        ///     larger than
        ///     <paramref name="toIndex" />
        /// </exception>
        /// <since>1.4</since>
        public virtual void Set(int fromIndex, int toIndex)
        {
            CheckRange(fromIndex, toIndex);
            if (fromIndex == toIndex) return;
            // Increase capacity if necessary
            var startWordIndex = WordIndex(fromIndex);
            var endWordIndex = WordIndex(toIndex - 1);
            ExpandTo(endWordIndex);
            var firstWordMask = Word_Mask << fromIndex;
            var lastWordMask = (long) (unchecked((ulong) Word_Mask) >> -toIndex);
            if (startWordIndex == endWordIndex)
            {
                // Case 1: One word
                words[startWordIndex] |= firstWordMask & lastWordMask;
            }
            else
            {
                // Case 2: Multiple words
                // Handle first word
                words[startWordIndex] |= firstWordMask;
                // Handle intermediate words, if any
                for (var i = startWordIndex + 1; i < endWordIndex; i++) words[i] = Word_Mask;
                // Handle last word (restores invariants)
                words[endWordIndex] |= lastWordMask;
            }

            CheckInvariants();
        }

        /// <summary>
        ///     Sets the bits from the specified
        ///     <paramref name="fromIndex" />
        ///     (inclusive) to the
        ///     specified
        ///     <paramref name="toIndex" />
        ///     (exclusive) to the specified value.
        /// </summary>
        /// <param name="fromIndex">index of the first bit to be set</param>
        /// <param name="toIndex">index after the last bit to be set</param>
        /// <param name="value">value to set the selected bits to</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if
        ///     <paramref name="fromIndex" />
        ///     is negative,
        ///     or
        ///     <paramref name="toIndex" />
        ///     is negative, or
        ///     <paramref name="fromIndex" />
        ///     is
        ///     larger than
        ///     <paramref name="toIndex" />
        /// </exception>
        /// <since>1.4</since>
        public virtual void Set(int fromIndex, int toIndex, bool value)
        {
            if (value)
                Set(fromIndex, toIndex);
            else
                Clear(fromIndex, toIndex);
        }

        /// <summary>
        ///     Sets the bit specified by the index to
        ///     <see langword="false" />
        ///     .
        /// </summary>
        /// <param name="bitIndex">the index of the bit to be cleared</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        /// <since>JDK1.0</since>
        public virtual void Clear(int bitIndex)
        {
            if (bitIndex < 0) throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            var wordIndex = WordIndex(bitIndex);
            if (wordIndex >= wordsInUse) return;
            words[wordIndex] &= ~(1L << bitIndex);
            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>
        ///     Sets the bits from the specified
        ///     <paramref name="fromIndex" />
        ///     (inclusive) to the
        ///     specified
        ///     <paramref name="toIndex" />
        ///     (exclusive) to
        ///     <see langword="false" />
        ///     .
        /// </summary>
        /// <param name="fromIndex">index of the first bit to be cleared</param>
        /// <param name="toIndex">index after the last bit to be cleared</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if
        ///     <paramref name="fromIndex" />
        ///     is negative,
        ///     or
        ///     <paramref name="toIndex" />
        ///     is negative, or
        ///     <paramref name="fromIndex" />
        ///     is
        ///     larger than
        ///     <paramref name="toIndex" />
        /// </exception>
        /// <since>1.4</since>
        public virtual void Clear(int fromIndex, int toIndex)
        {
            CheckRange(fromIndex, toIndex);
            if (fromIndex == toIndex) return;
            var startWordIndex = WordIndex(fromIndex);
            if (startWordIndex >= wordsInUse) return;
            var endWordIndex = WordIndex(toIndex - 1);
            if (endWordIndex >= wordsInUse)
            {
                toIndex = Length();
                endWordIndex = wordsInUse - 1;
            }

            var firstWordMask = Word_Mask << fromIndex;
            var lastWordMask = (long) (unchecked((ulong) Word_Mask) >> -toIndex);
            if (startWordIndex == endWordIndex)
            {
                // Case 1: One word
                words[startWordIndex] &= ~(firstWordMask & lastWordMask);
            }
            else
            {
                // Case 2: Multiple words
                // Handle first word
                words[startWordIndex] &= ~firstWordMask;
                // Handle intermediate words, if any
                for (var i = startWordIndex + 1; i < endWordIndex; i++) words[i] = 0;
                // Handle last word
                words[endWordIndex] &= ~lastWordMask;
            }

            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>
        ///     Sets all of the bits in this BitSet to
        ///     <see langword="false" />
        ///     .
        /// </summary>
        /// <since>1.4</since>
        public virtual void Clear()
        {
            while (wordsInUse > 0) words[--wordsInUse] = 0;
        }

        /// <summary>Returns the value of the bit with the specified index.</summary>
        /// <remarks>
        ///     Returns the value of the bit with the specified index. The value
        ///     is
        ///     <see langword="true" />
        ///     if the bit with the index
        ///     <paramref name="bitIndex" />
        ///     is currently set in this
        ///     <c>BitSet</c>
        ///     ; otherwise, the result
        ///     is
        ///     <see langword="false" />
        ///     .
        /// </remarks>
        /// <param name="bitIndex">the bit index</param>
        /// <returns>the value of the bit with the specified index</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        public virtual bool Get(int bitIndex)
        {
            if (bitIndex < 0) throw new IndexOutOfRangeException("bitIndex < 0: " + bitIndex);
            CheckInvariants();
            var wordIndex = WordIndex(bitIndex);
            return wordIndex < wordsInUse && (words[wordIndex] & (1L << bitIndex)) != 0;
        }

        /// <summary>
        ///     Returns a new
        ///     <c>BitSet</c>
        ///     composed of bits from this
        ///     <c>BitSet</c>
        ///     from
        ///     <paramref name="fromIndex" />
        ///     (inclusive) to
        ///     <paramref name="toIndex" />
        ///     (exclusive).
        /// </summary>
        /// <param name="fromIndex">index of the first bit to include</param>
        /// <param name="toIndex">index after the last bit to include</param>
        /// <returns>
        ///     a new
        ///     <c>BitSet</c>
        ///     from a range of this
        ///     <c>BitSet</c>
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if
        ///     <paramref name="fromIndex" />
        ///     is negative,
        ///     or
        ///     <paramref name="toIndex" />
        ///     is negative, or
        ///     <paramref name="fromIndex" />
        ///     is
        ///     larger than
        ///     <paramref name="toIndex" />
        /// </exception>
        /// <since>1.4</since>
        public virtual BitSet Get(int fromIndex, int toIndex)
        {
            CheckRange(fromIndex, toIndex);
            CheckInvariants();
            var len = Length();
            // If no set bits in range return empty bitset
            if (len <= fromIndex || fromIndex == toIndex) return new BitSet(0);
            // An optimization
            if (toIndex > len) toIndex = len;
            var result = new BitSet(toIndex - fromIndex);
            var targetWords = WordIndex(toIndex - fromIndex - 1) + 1;
            var sourceIndex = WordIndex(fromIndex);
            var wordAligned = (fromIndex & Bit_Index_Mask) == 0;
            // Process all words but the last word
            for (var i = 0; i < targetWords - 1; i++, sourceIndex++)
                result.words[i] = wordAligned
                    ? words[sourceIndex]
                    : (long) ((ulong) words[sourceIndex
                              ] >> fromIndex) | (words[sourceIndex + 1] << -fromIndex);
            // Process the last word
            var lastWordMask = (long) (unchecked((ulong) Word_Mask) >> -toIndex);
            result.words[targetWords - 1] = ((toIndex - 1) & Bit_Index_Mask) < (fromIndex & Bit_Index_Mask
                                            )
                ? (long) ((ulong) words[sourceIndex] >> fromIndex) | ((words[sourceIndex + 1
                                                                       ] & lastWordMask) << -fromIndex)
                : (long) ((ulong) (words[sourceIndex] & lastWordMask
                          ) >> fromIndex);
            /* straddles source words */
            // Set wordsInUse correctly
            result.wordsInUse = targetWords;
            result.RecalculateWordsInUse();
            result.CheckInvariants();
            return result;
        }

        /// <summary>
        ///     Returns the index of the first bit that is set to
        ///     <see langword="true" />
        ///     that occurs on or after the specified starting index. If no such
        ///     bit exists then
        ///     <c>-1</c>
        ///     is returned.
        ///     <p>
        ///         To iterate over the
        ///         <see langword="true" />
        ///         bits in a
        ///         <c>BitSet</c>
        ///         ,
        ///         use the following loop:
        ///         <pre>
        ///             <c>
        ///                 for (int i = bs.nextSetBit(0); i &gt;= 0; i = bs.nextSetBit(i+1))
        ///                 // operate on index i here
        ///                 if (i == Integer.MAX_VALUE)
        ///                 break; // or (i+1) would overflow
        ///             </c>
        ///             }}
        ///         </pre>
        /// </summary>
        /// <param name="fromIndex">the index to start checking from (inclusive)</param>
        /// <returns>
        ///     the index of the next set bit, or
        ///     <c>-1</c>
        ///     if there
        ///     is no such bit
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        /// <since>1.4</since>
        public virtual int NextSetBit(int fromIndex)
        {
            if (fromIndex < 0) throw new IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
            CheckInvariants();
            var u = WordIndex(fromIndex);
            if (u >= wordsInUse) return -1;
            var word = words[u] & (Word_Mask << fromIndex);
            while (true)
            {
                if (word != 0) return u * Bits_Per_Word + Runtime.NumberOfTrailingZeros(word);
                if (++u == wordsInUse) return -1;
                word = words[u];
            }
        }

        /// <summary>
        ///     Returns the index of the first bit that is set to
        ///     <see langword="false" />
        ///     that occurs on or after the specified starting index.
        /// </summary>
        /// <param name="fromIndex">the index to start checking from (inclusive)</param>
        /// <returns>the index of the next clear bit</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is negative
        /// </exception>
        /// <since>1.4</since>
        public virtual int NextClearBit(int fromIndex)
        {
            // Neither spec nor implementation handle bitsets of maximal length.
            // See 4816253.
            if (fromIndex < 0) throw new IndexOutOfRangeException("fromIndex < 0: " + fromIndex);
            CheckInvariants();
            var u = WordIndex(fromIndex);
            if (u >= wordsInUse) return fromIndex;
            var word = ~words[u] & (Word_Mask << fromIndex);
            while (true)
            {
                if (word != 0) return u * Bits_Per_Word + Runtime.NumberOfTrailingZeros(word);
                if (++u == wordsInUse) return wordsInUse * Bits_Per_Word;
                word = ~words[u];
            }
        }

        /// <summary>
        ///     Returns the index of the nearest bit that is set to
        ///     <see langword="true" />
        ///     that occurs on or before the specified starting index.
        ///     If no such bit exists, or if
        ///     <c>-1</c>
        ///     is given as the
        ///     starting index, then
        ///     <c>-1</c>
        ///     is returned.
        ///     <p>
        ///         To iterate over the
        ///         <see langword="true" />
        ///         bits in a
        ///         <c>BitSet</c>
        ///         ,
        ///         use the following loop:
        ///         <pre>
        ///             <c>
        ///                 for (int i = bs.length(); (i = bs.previousSetBit(i-1)) &gt;= 0; )
        ///                 // operate on index i here
        ///             </c>
        ///         </pre>
        /// </summary>
        /// <param name="fromIndex">the index to start checking from (inclusive)</param>
        /// <returns>
        ///     the index of the previous set bit, or
        ///     <c>-1</c>
        ///     if there
        ///     is no such bit
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is less
        ///     than
        ///     <c>-1</c>
        /// </exception>
        /// <since>1.7</since>
        public virtual int PreviousSetBit(int fromIndex)
        {
            if (fromIndex < 0)
            {
                if (fromIndex == -1) return -1;
                throw new IndexOutOfRangeException("fromIndex < -1: " + fromIndex);
            }

            CheckInvariants();
            var u = WordIndex(fromIndex);
            if (u >= wordsInUse) return Length() - 1;
            var word = words[u] & (long) (unchecked((ulong) Word_Mask) >> -(fromIndex + 1));
            while (true)
            {
                if (word != 0) return (u + 1) * Bits_Per_Word - 1 - Runtime.NumberOfLeadingZeros(word);
                if (u-- == 0) return -1;
                word = words[u];
            }
        }

        /// <summary>
        ///     Returns the index of the nearest bit that is set to
        ///     <see langword="false" />
        ///     that occurs on or before the specified starting index.
        ///     If no such bit exists, or if
        ///     <c>-1</c>
        ///     is given as the
        ///     starting index, then
        ///     <c>-1</c>
        ///     is returned.
        /// </summary>
        /// <param name="fromIndex">the index to start checking from (inclusive)</param>
        /// <returns>
        ///     the index of the previous clear bit, or
        ///     <c>-1</c>
        ///     if there
        ///     is no such bit
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the specified index is less
        ///     than
        ///     <c>-1</c>
        /// </exception>
        /// <since>1.7</since>
        public virtual int PreviousClearBit(int fromIndex)
        {
            if (fromIndex < 0)
            {
                if (fromIndex == -1) return -1;
                throw new IndexOutOfRangeException("fromIndex < -1: " + fromIndex);
            }

            CheckInvariants();
            var u = WordIndex(fromIndex);
            if (u >= wordsInUse) return fromIndex;
            var word = ~words[u] & (long) (unchecked((ulong) Word_Mask) >> -(fromIndex + 1));
            while (true)
            {
                if (word != 0) return (u + 1) * Bits_Per_Word - 1 - Runtime.NumberOfLeadingZeros(word);
                if (u-- == 0) return -1;
                word = ~words[u];
            }
        }

        /// <summary>
        ///     Returns the "logical size" of this
        ///     <c>BitSet</c>
        ///     : the index of
        ///     the highest set bit in the
        ///     <c>BitSet</c>
        ///     plus one. Returns zero
        ///     if the
        ///     <c>BitSet</c>
        ///     contains no set bits.
        /// </summary>
        /// <returns>
        ///     the logical size of this
        ///     <c>BitSet</c>
        /// </returns>
        /// <since>1.2</since>
        public virtual int Length()
        {
            if (wordsInUse == 0) return 0;
            return Bits_Per_Word * (wordsInUse - 1) + (Bits_Per_Word - Runtime.NumberOfLeadingZeros
                                                           (words[wordsInUse - 1]));
        }

        /// <summary>
        ///     Returns true if this
        ///     <c>BitSet</c>
        ///     contains no bits that are set
        ///     to
        ///     <see langword="true" />
        ///     .
        /// </summary>
        /// <returns>
        ///     boolean indicating whether this
        ///     <c>BitSet</c>
        ///     is empty
        /// </returns>
        /// <since>1.4</since>
        public virtual bool IsEmpty()
        {
            return wordsInUse == 0;
        }

        /// <summary>
        ///     Returns true if the specified
        ///     <c>BitSet</c>
        ///     has any bits set to
        ///     <see langword="true" />
        ///     that are also set to
        ///     <see langword="true" />
        ///     in this
        ///     <c>BitSet</c>
        ///     .
        /// </summary>
        /// <param name="set">
        ///     <c>BitSet</c>
        ///     to intersect with
        /// </param>
        /// <returns>
        ///     boolean indicating whether this
        ///     <c>BitSet</c>
        ///     intersects
        ///     the specified
        ///     <c>BitSet</c>
        /// </returns>
        /// <since>1.4</since>
        public virtual bool Intersects(BitSet set)
        {
            for (var i = Math.Min(wordsInUse, set.wordsInUse) - 1; i >= 0; i--)
                if ((words[i] & set.words[i]) != 0)
                    return true;
            return false;
        }

        /// <summary>
        ///     Returns the number of bits set to
        ///     <see langword="true" />
        ///     in this
        ///     <c>BitSet</c>
        ///     .
        /// </summary>
        /// <returns>
        ///     the number of bits set to
        ///     <see langword="true" />
        ///     in this
        ///     <c>BitSet</c>
        /// </returns>
        /// <since>1.4</since>
        public virtual int Cardinality()
        {
            var sum = 0;
            for (var i = 0; i < wordsInUse; i++) sum += Runtime.BitCount(words[i]);
            return sum;
        }

        /// <summary>
        ///     Performs a logical <b>AND</b> of this target bit set with the
        ///     argument bit set.
        /// </summary>
        /// <remarks>
        ///     Performs a logical <b>AND</b> of this target bit set with the
        ///     argument bit set. This bit set is modified so that each bit in it
        ///     has the value
        ///     <see langword="true" />
        ///     if and only if it both initially
        ///     had the value
        ///     <see langword="true" />
        ///     and the corresponding bit in the
        ///     bit set argument also had the value
        ///     <see langword="true" />
        ///     .
        /// </remarks>
        /// <param name="set">a bit set</param>
        public virtual void And(BitSet set)
        {
            if (this == set) return;
            while (wordsInUse > set.wordsInUse) words[--wordsInUse] = 0;
            // Perform logical AND on words in common
            for (var i = 0; i < wordsInUse; i++) words[i] &= set.words[i];
            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>
        ///     Performs a logical <b>OR</b> of this bit set with the bit set
        ///     argument.
        /// </summary>
        /// <remarks>
        ///     Performs a logical <b>OR</b> of this bit set with the bit set
        ///     argument. This bit set is modified so that a bit in it has the
        ///     value
        ///     <see langword="true" />
        ///     if and only if it either already had the
        ///     value
        ///     <see langword="true" />
        ///     or the corresponding bit in the bit set
        ///     argument has the value
        ///     <see langword="true" />
        ///     .
        /// </remarks>
        /// <param name="set">a bit set</param>
        public virtual void Or(BitSet set)
        {
            if (this == set) return;
            var wordsInCommon = Math.Min(wordsInUse, set.wordsInUse);
            if (wordsInUse < set.wordsInUse)
            {
                EnsureCapacity(set.wordsInUse);
                wordsInUse = set.wordsInUse;
            }

            // Perform logical OR on words in common
            for (var i = 0; i < wordsInCommon; i++) words[i] |= set.words[i];
            // Copy any remaining words
            if (wordsInCommon < set.wordsInUse)
                Array.Copy(set.words, wordsInCommon, words, wordsInCommon, wordsInUse - wordsInCommon
                );
            // recalculateWordsInUse() is unnecessary
            CheckInvariants();
        }

        /// <summary>
        ///     Performs a logical <b>XOR</b> of this bit set with the bit set
        ///     argument.
        /// </summary>
        /// <remarks>
        ///     Performs a logical <b>XOR</b> of this bit set with the bit set
        ///     argument. This bit set is modified so that a bit in it has the
        ///     value
        ///     <see langword="true" />
        ///     if and only if one of the following
        ///     statements holds:
        ///     <ul>
        ///         <li>
        ///             The bit initially has the value
        ///             <see langword="true" />
        ///             , and the
        ///             corresponding bit in the argument has the value
        ///             <see langword="false" />
        ///             .
        ///             <li>
        ///                 The bit initially has the value
        ///                 <see langword="false" />
        ///                 , and the
        ///                 corresponding bit in the argument has the value
        ///                 <see langword="true" />
        ///                 .
        ///     </ul>
        /// </remarks>
        /// <param name="set">a bit set</param>
        public virtual void Xor(BitSet set)
        {
            var wordsInCommon = Math.Min(wordsInUse, set.wordsInUse);
            if (wordsInUse < set.wordsInUse)
            {
                EnsureCapacity(set.wordsInUse);
                wordsInUse = set.wordsInUse;
            }

            // Perform logical XOR on words in common
            for (var i = 0; i < wordsInCommon; i++) words[i] ^= set.words[i];
            // Copy any remaining words
            if (wordsInCommon < set.wordsInUse)
                Array.Copy(set.words, wordsInCommon, words, wordsInCommon, set.wordsInUse
                                                                           - wordsInCommon);
            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>
        ///     Clears all of the bits in this
        ///     <c>BitSet</c>
        ///     whose corresponding
        ///     bit is set in the specified
        ///     <c>BitSet</c>
        ///     .
        /// </summary>
        /// <param name="set">
        ///     the
        ///     <c>BitSet</c>
        ///     with which to mask this
        ///     <c>BitSet</c>
        /// </param>
        /// <since>1.2</since>
        public virtual void AndNot(BitSet set)
        {
            // Perform logical (a & !b) on words in common
            for (var i = Math.Min(wordsInUse, set.wordsInUse) - 1; i >= 0; i--) words[i] &= ~set.words[i];
            RecalculateWordsInUse();
            CheckInvariants();
        }

        /// <summary>Returns the hash code value for this bit set.</summary>
        /// <remarks>
        ///     Returns the hash code value for this bit set. The hash code depends
        ///     only on which bits are set within this
        ///     <c>BitSet</c>
        ///     .
        ///     <p>
        ///         The hash code is defined to be the result of the following
        ///         calculation:
        ///         <pre>
        ///             <c>
        ///                 public int hashCode()
        ///                 long h = 1234;
        ///                 long[] words = toLongArray();
        ///                 for (int i = words.length; --i &gt;= 0; )
        ///                 h ^= words[i] * (i + 1);
        ///                 return (int)((h &gt;&gt; 32) ^ h);
        ///             </c>
        ///         </pre>
        ///         Note that the hash code changes if the set of bits is altered.
        /// </remarks>
        /// <returns>the hash code value for this bit set</returns>
        public override int GetHashCode()
        {
            long h = 1234;
            for (var i = wordsInUse; --i >= 0;) h ^= words[i] * (i + 1);
            return (int) ((h >> 32) ^ h);
        }

        /// <summary>
        ///     Returns the number of bits of space actually in use by this
        ///     <c>BitSet</c>
        ///     to represent bit values.
        ///     The maximum element in the set is the size - 1st element.
        /// </summary>
        /// <returns>the number of bits currently in this bit set</returns>
        public virtual int Size()
        {
            return words.Length * Bits_Per_Word;
        }

        /// <summary>Compares this object against the specified object.</summary>
        /// <remarks>
        ///     Compares this object against the specified object.
        ///     The result is
        ///     <see langword="true" />
        ///     if and only if the argument is
        ///     not
        ///     <see langword="null" />
        ///     and is a
        ///     <c>Bitset</c>
        ///     object that has
        ///     exactly the same set of bits set to
        ///     <see langword="true" />
        ///     as this bit
        ///     set. That is, for every nonnegative
        ///     <c>int</c>
        ///     index
        ///     <c>k</c>
        ///     ,
        ///     <pre>((BitSet)obj).get(k) == this.get(k)</pre>
        ///     must be true. The current sizes of the two bit sets are not compared.
        /// </remarks>
        /// <param name="obj">the object to compare with</param>
        /// <returns>
        ///     <see langword="true" />
        ///     if the objects are the same;
        ///     <see langword="false" />
        ///     otherwise
        /// </returns>
        /// <seealso cref="Size()" />
        public override bool Equals(object obj)
        {
            if (!(obj is BitSet)) return false;
            if (this == obj) return true;
            var set = (BitSet) obj;
            CheckInvariants();
            set.CheckInvariants();
            if (wordsInUse != set.wordsInUse) return false;
            // Check words in use by both BitSets
            for (var i = 0; i < wordsInUse; i++)
                if (words[i] != set.words[i])
                    return false;
            return true;
        }

        /// <summary>Attempts to reduce internal storage used for the bits in this bit set.</summary>
        /// <remarks>
        ///     Attempts to reduce internal storage used for the bits in this bit set.
        ///     Calling this method may, but is not required to, affect the value
        ///     returned by a subsequent call to the
        ///     <see cref="Size()" />
        ///     method.
        /// </remarks>
        private void TrimToSize()
        {
            if (wordsInUse != words.Length)
            {
                words = Arrays.CopyOf(words, wordsInUse);
                CheckInvariants();
            }
        }

        /*
        /// <summary>
        /// Save the state of the
        /// <c>BitSet</c>
        /// instance to a stream (i.e.,
        /// serialize it).
        /// </summary>
        /// <exception cref="System.IO.IOException"/>
        private void WriteObject(ObjectOutputStream s)
        {
            CheckInvariants();
            if (!sizeIsSticky)
            {
                TrimToSize();
            }
            ObjectOutputStream.PutField fields = s.PutFields();
            fields.Put("bits", words);
            s.WriteFields();
        }
        */

        /*
        /// <summary>
        /// Reconstitute the
        /// <c>BitSet</c>
        /// instance from a stream (i.e.,
        /// deserialize it).
        /// </summary>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.TypeLoadException"/>
        private void ReadObject(ObjectInputStream s)
        {
            ObjectInputStream.GetField fields = s.ReadFields();
            words = (long[])fields.Get("bits", null);
            // Assume maximum length then find real length
            // because recalculateWordsInUse assumes maintenance
            // or reduction in logical size
            wordsInUse = words.Length;
            RecalculateWordsInUse();
            sizeIsSticky = (words.Length > 0 && words[words.Length - 1] == 0L);
            // heuristic
            CheckInvariants();
        }
        */

        /// <summary>Returns a string representation of this bit set.</summary>
        /// <remarks>
        ///     Returns a string representation of this bit set. For every index
        ///     for which this
        ///     <c>BitSet</c>
        ///     contains a bit in the set
        ///     state, the decimal representation of that index is included in
        ///     the result. Such indices are listed in order from lowest to
        ///     highest, separated by ",&nbsp;" (a comma and a space) and
        ///     surrounded by braces, resulting in the usual mathematical
        ///     notation for a set of integers.
        ///     <p>
        ///         Example:
        ///         <pre>
        ///             BitSet drPepper = new BitSet();
        ///         </pre>
        ///         Now
        ///         <c>drPepper.toString()</c>
        ///         returns "
        ///         <c />
        ///         {}}".
        ///         <pre>
        ///             drPepper.set(2);
        ///         </pre>
        ///         Now
        ///         <c>drPepper.toString()</c>
        ///         returns "
        ///         <c />
        ///         {2}}".
        ///         <pre>
        ///             drPepper.set(4);
        ///             drPepper.set(10);
        ///         </pre>
        ///         Now
        ///         <c>drPepper.toString()</c>
        ///         returns "
        ///         <c />
        ///         {2, 4, 10}}".
        /// </remarks>
        /// <returns>a string representation of this bit set</returns>
        public override string ToString()
        {
            CheckInvariants();
            var numBits = wordsInUse > 128 ? Cardinality() : wordsInUse * Bits_Per_Word;
            var b = new StringBuilder(6 * numBits + 2);
            b.Append('{');
            var i = NextSetBit(0);
            if (i != -1)
            {
                b.Append(i);
                while (true)
                {
                    if (++i < 0) break;
                    if ((i = NextSetBit(i)) < 0) break;
                    var endOfRun = NextClearBit(i);
                    do
                    {
                        b.Append(", ").Append(i);
                    } while (++i != endOfRun);
                }
            }

            b.Append('}');
            return b.ToString();
        }
    }

    internal class NoSuchElementException : Exception
    {
    }

    public class NegativeArraySizeException : Exception
    {
        private readonly string _nbits;

        public NegativeArraySizeException(string nbits)
        {
            _nbits = nbits;
        }
    }
}