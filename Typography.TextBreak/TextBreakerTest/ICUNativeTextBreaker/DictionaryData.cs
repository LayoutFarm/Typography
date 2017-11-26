// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License
/*
 *******************************************************************************
 * Copyright (C) 2014, International Business Machines Corporation and         *
 * others. All Rights Reserved.                                                *
 *******************************************************************************
*/
using System;
using System.IO;

//test only1 
#if TEST_ICU
namespace LayoutFarm.TextBreaker.ICU
{
    public static class DictionaryData
    {

        // Magic numbers to authenticate the data file

        const byte MAGIC1 = 0xda;
        const byte MAGIC2 = 0x27;
        //File format authentication values

        const byte CHAR_SET_ = 0;
        const byte CHAR_SIZE_ = 2;
        const int DATA_FORMAT_ID = 0x44696374;
        const int IX_COUNT = 8;
        const int IX_STRING_TRIE_OFFSET = 0;

        const int TRIE_TYPE_BYTES = 0;
        const int TRIE_TYPE_UCHARS = 1;
        const int TRIE_TYPE_MASK = 7;
        const int TRIE_HAS_VALUES = 8;
        const int TRANSFORM_NONE = 0;
        const int TRANSFORM_TYPE_OFFSET = 0x1000000;
        const int TRANSFORM_TYPE_MASK = 0x7f000000;
        internal const int TRANSFORM_OFFSET_MASK = 0x1fffff;


        const int IX_RESERVED1_OFFSET = 1;
        const int IX_RESERVED2_OFFSET = 2;
        const int IX_TOTAL_SIZE = 3;
        const int IX_TRIE_TYPE = 4;
        const int IX_TRANSFORM = 5;
        const int IX_RESERVED6 = 6;
        const int IX_RESERVED7 = 7;
        public static DictionaryMatcher LoadData(string filename)
        {
            DictionaryMatcher dicMatcher = null;

            //from ICUBinary.java
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                //file header
                //--------------------------------------------------------------
                BinaryReader reader = new BinaryReader(fs);
                ushort headerSize = reader.ReadUInt16();//0,1 
                byte magic1 = reader.ReadByte();//2
                byte magic2 = reader.ReadByte();//3
                if (magic1 != MAGIC1 && magic2 != MAGIC2)
                {
                    throw new Exception("MAGIC_NUMBER_AUTHENTICATION_FAILED_");
                }
                ushort sizeofUDataInfo = reader.ReadUInt16();//4,5                 
                reader.ReadUInt16(); //6,7
                //
                byte isBigEndian = reader.ReadByte();//8
                byte charsetFamily = reader.ReadByte();//9
                byte sizeofUChar = reader.ReadByte(); //10
                if (isBigEndian < 0 || 1 < isBigEndian ||
                        charsetFamily != CHAR_SET_ || sizeofUChar != CHAR_SIZE_)
                {
                    throw new IOException("HEADER_AUTHENTICATION_FAILED_");
                }
                if (sizeofUDataInfo < 20 || headerSize < (sizeofUDataInfo + 4))
                {
                    throw new IOException("Internal Error: Header size error");
                }
                reader.ReadByte();//11

                byte[] fourBytesBuffer = new byte[4];
                reader.Read(fourBytesBuffer, 0, 4); //12,13,14,15
                int file_dataFormat =
                    (fourBytesBuffer[0] << 24) |
                    (fourBytesBuffer[1] << 16) |
                    (fourBytesBuffer[2] << 8) |
                    (fourBytesBuffer[3]);
                if (file_dataFormat != DATA_FORMAT_ID)
                {
                    throw new IOException("HEADER_AUTHENTICATION_FAILED_");
                }

                //format version
                reader.Read(fourBytesBuffer, 0, 4);//16,17,18,19


                //must = 20 
                //
                //data version
                reader.Read(fourBytesBuffer, 0, 4);
                int data_version =
                    (fourBytesBuffer[0] << 24) |
                    (fourBytesBuffer[1] << 16) |
                    (fourBytesBuffer[2] << 8) |
                    (fourBytesBuffer[3]);
                //-------------------------------------------------------------- 
                reader.BaseStream.Position = headerSize;
                //body
                int[] indexes = new int[IX_COUNT];
                for (int i = 0; i < IX_COUNT; ++i)
                {
                    indexes[i] = reader.ReadInt32();
                }

                int offset = indexes[IX_STRING_TRIE_OFFSET];

                if (offset >= 4 * IX_COUNT)
                {
                    //must true

                }

                if (offset > (4 * IX_COUNT))
                {
                    int diff = offset - (4 * IX_COUNT);
                    //ICUBinary.skipBytes(bytes, diff);
                    throw new NotSupportedException();
                }

                int trieType = indexes[IX_TRIE_TYPE] & TRIE_TYPE_MASK;
                int totalSize = indexes[IX_TOTAL_SIZE] - offset;

                switch (trieType)
                {
                    default: throw new NotSupportedException();
                    case TRIE_TYPE_BYTES:
                        {
                            int transform = indexes[IX_TRANSFORM];
                            byte[] data = new byte[totalSize];
                            reader.Read(data, 0, totalSize);
                            if ((transform & DictionaryData.TRANSFORM_TYPE_MASK) == DictionaryData.TRANSFORM_TYPE_OFFSET)
                            {//must true
                            }
                            BytesDictionaryMatcher byteDicMatcher = new BytesDictionaryMatcher(data, transform);
                            byteDicMatcher.Tx2('ก');
                            dicMatcher = byteDicMatcher;
                        } break;
                    case TRIE_TYPE_UCHARS:
                        {
                            throw new NotSupportedException();
                        } break;

                }
                reader.Close();
            }
            return dicMatcher;
        }
    }

    public class CharacterIterator
    {
        char[] textBuffer;
        int index;
        int length;
        public CharacterIterator(char[] textBuffer)
        {
            this.index = 0;
            this.textBuffer = textBuffer;
            this.length = textBuffer.Length;
        }
        public int Length
        {
            get { return this.length; }
        }
        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }
        public char GetCharAt(int index)
        {
            return textBuffer[index];
        }
        /// <summary>
        /// read from current index
        /// </summary>
        /// <returns></returns>
        public char Read()
        {
            if (index < length)
            {
                return textBuffer[index++];
            }
            else
            {
                return '\0';
            }
        }
    }

    public abstract class DictionaryMatcher
    {
    }

    public struct BytesTrie
    {
        byte[] bytes_;
        int remainingMatchLength_;
        int root_;
        int pos_;
        public BytesTrie(byte[] charBuffer, int offset)
        {
            this.bytes_ = charBuffer;
            this.pos_ = this.root_ = offset;
            this.remainingMatchLength_ = -1;
        }
        public void Reset()
        {
            this.pos_ = this.root_;
            this.remainingMatchLength_ = -1;
        }
        public Result First(int inByte)
        {

            remainingMatchLength_ = -1;
            if (inByte < 0)
            {
                inByte += 0x100;
            }
            return nextImpl(root_, inByte);
        }

        // 10..1f: Linear-match node, match 1..16 bytes and continue reading the next node.
        const int kMinLinearMatch = 0x10;/*package*/
        const int kMaxLinearMatchLength = 0x10;/*package*/
        // 20..ff: Variable-length value node.
        // If odd, the value is final. (Otherwise, intermediate value or jump delta.)
        // Then shift-right by 1 bit.
        // The remaining lead byte value indicates the number of following bytes (0..4)
        // and contains the value's top bits.
        /*package*/
        const int kMinValueLead = kMinLinearMatch + kMaxLinearMatchLength;  // 0x20
        // It is a final value if bit 0 is set.
        const int kValueIsFinal = 1;

        // Compact value: After testing bit 0, shift right by 1 and then use the following thresholds.
        /*package*/
        const int kMinOneByteValueLead = kMinValueLead / 2;  // 0x10
        /*package*/
        const int kMaxOneByteValue = 0x40;  // At least 6 bits in the first byte.

        /*package*/
        const int kMinTwoByteValueLead = kMinOneByteValueLead + kMaxOneByteValue + 1;  // 0x51
        /*package*/
        const int kMaxTwoByteValue = 0x1aff;

        /*package*/
        const int kMinThreeByteValueLead = kMinTwoByteValueLead + (kMaxTwoByteValue >> 8) + 1;  // 0x6c
        /*package*/
        const int kFourByteValueLead = 0x7e;

        // A little more than Unicode code points. (0x11ffff)
        /*package*/
        const int kMaxThreeByteValue = ((kFourByteValueLead - kMinThreeByteValueLead) << 16) - 1;

        /*package*/
        const int kFiveByteValueLead = 0x7f;

        // Compact delta integers.
        /*package*/
        const int kMaxOneByteDelta = 0xbf;
        /*package*/
        const int kMinTwoByteDeltaLead = kMaxOneByteDelta + 1;  // 0xc0
        /*package*/
        const int kMinThreeByteDeltaLead = 0xf0;
        /*package*/
        const int kFourByteDeltaLead = 0xfe;
        /*package*/
        const int kFiveByteDeltaLead = 0xff;

        /*package*/
        const int kMaxTwoByteDelta = ((kMinThreeByteDeltaLead - kMinTwoByteDeltaLead) << 8) - 1;  // 0x2fff
        /*package*/
        const int kMaxThreeByteDelta = ((kFourByteDeltaLead - kMinThreeByteDeltaLead) << 16) - 1;  // 0xdffff
        private Result nextImpl(int pos, int inByte)
        {
            throw new NotSupportedException();
            //for (; ; )
            //{
            //    int node = bytes_[pos++] & 0xff;
            //    if (node < kMinLinearMatch)
            //    {
            //        return branchNext(pos, node, inByte);
            //    }
            //    else if (node < kMinValueLead)
            //    {
            //        // Match the first of length+1 bytes.
            //        int length = node - kMinLinearMatch;  // Actual match length minus 1.
            //        if (inByte == (bytes_[pos++] & 0xff))
            //        {
            //            remainingMatchLength_ = --length;
            //            pos_ = pos;
            //            return (length < 0 && (node = bytes_[pos] & 0xff) >= kMinValueLead) ?
            //                    valueResults_[node & kValueIsFinal] : Result.NO_VALUE;
            //        }
            //        else
            //        {
            //            // No match.
            //            break;
            //        }
            //    }
            //    else if ((node & kValueIsFinal) != 0)
            //    {
            //        // No further matching bytes.
            //        break;
            //    }
            //    else
            //    {
            //        // Skip intermediate value.
            //        pos = skipValue(pos, node);
            //        // The next node must not also be a value node.
            //        assert((bytes_[pos] & 0xff) < kMinValueLead);
            //    }
            //}
            //stop();
            //return Result.NO_MATCH;
        }
        /**
 * Return values for BytesTrie.next(), CharsTrie.next() and similar methods.
 * @stable ICU 4.8
 */
        public enum Result
        {
            /**
             * The input unit(s) did not continue a matching string.
             * Once current()/next() return NO_MATCH,
             * all further calls to current()/next() will also return NO_MATCH,
             * until the trie is reset to its original state or to a saved state.
             * @stable ICU 4.8
             */
            NO_MATCH,
            /**
             * The input unit(s) continued a matching string
             * but there is no value for the string so far.
             * (It is a prefix of a longer string.)
             * @stable ICU 4.8
             */
            NO_VALUE,
            /**
             * The input unit(s) continued a matching string
             * and there is a value for the string so far.
             * This value will be returned by getValue().
             * No further input byte/unit can continue a matching string.
             * @stable ICU 4.8
             */
            FINAL_VALUE,
            /**
             * The input unit(s) continued a matching string
             * and there is a value for the string so far.
             * This value will be returned by getValue().
             * Another input byte/unit can continue a matching string.
             * @stable ICU 4.8
             */
            INTERMEDIATE_VALUE

            //// Note: The following methods assume the particular order
            //// of enum constants, treating the ordinal() values like bit sets.
            //// Do not reorder the enum constants!

            ///**
            // * Same as (result!=NO_MATCH).
            // * @return true if the input bytes/units so far are part of a matching string/byte sequence.
            // * @stable ICU 4.8
            // */
            //public boolean matches() { return this!=NO_MATCH; }

            ///**
            // * Equivalent to (result==INTERMEDIATE_VALUE || result==FINAL_VALUE).
            // * @return true if there is a value for the input bytes/units so far.
            // * @see #getValue
            // * @stable ICU 4.8
            // */
            //public boolean hasValue() { return ordinal()>=2; }

            ///**
            // * Equivalent to (result==NO_VALUE || result==INTERMEDIATE_VALUE).
            // * @return true if another input byte/unit can continue a matching string.
            // * @stable ICU 4.8
            // */
            //public boolean hasNext() { return (ordinal()&1)!=0; }
        }
    }

    public class BytesDictionaryMatcher : DictionaryMatcher
    {
        byte[] _charBuffer;
        int _transform;
        public BytesDictionaryMatcher(byte[] charBuffer, int transform)
        {
            this._charBuffer = charBuffer;
            this._transform = transform;
        }
        int Transform(int c)
        {
            if (c == 0x200D)
            {
                return 0xFF;
            }
            else if (c == 0x200C)
            {
                return 0xFE;
            }

            int delta = c - (_transform & DictionaryData.TRANSFORM_OFFSET_MASK);
            if (delta < 0 || 0xFD < delta)
            {
                return -1;
            }
            return delta;
        }
        public int Tx2(char c)
        {
            return Transform(c);
        }
        public int Match(CharacterIterator text, int maxLength, int[] lengths, int[] count, int limit, int[] values)
        {
            char c = text.Read();
            if (c == '\0')
            {
                return 0;
            }
            Transform(c);
            BytesTrie bt = new BytesTrie(_charBuffer, 0);


            return 0;
        }
        //  @Override
        //public int matches(CharacterIterator text_, int maxLength, int[] lengths, int[] count_, int limit, int[] values) {
        //    UCharacterIterator text = UCharacterIterator.getInstance(text_);
        //    BytesTrie bt = new BytesTrie(characters, 0);
        //    int c = text.nextCodePoint();
        //    if (c == UCharacterIterator.DONE) {
        //        return 0;
        //    }
        //    Result result = bt.first(transform(c));
        //    // TODO: should numChars count Character.charCount() ?
        //    int numChars = 1;
        //    int count = 0;
        //    for (;;) {
        //        if (result.hasValue()) {
        //            if (count < limit) {
        //                if (values != null) {
        //                    values[count] = bt.getValue();
        //                }
        //                lengths[count] = numChars;
        //                count++;
        //            }
        //            if (result == Result.FINAL_VALUE) {
        //                break;
        //            }
        //        } else if (result == Result.NO_MATCH) {
        //            break;
        //        }

        //        if (numChars >= maxLength) {
        //            break;
        //        }

        //        c = text.nextCodePoint();
        //        if (c == UCharacterIterator.DONE) {
        //            break;
        //        }
        //        ++numChars;
        //        result = bt.next(transform(c));
        //    }
        //    count_[0] = count;
        //    return numChars;
        //}
    }
}


#endif