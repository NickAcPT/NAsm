// ASM: a very small and fast Java bytecode manipulation framework
// Copyright (c) 2000-2011 INRIA, France Telecom
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>
    ///     A
    ///     <see cref="Value" />
    ///     that is represented with its type in a seven types type system. This type system
    ///     distinguishes the UNINITIALZED, INT, FLOAT, LONG, DOUBLE, REFERENCE and RETURNADDRESS types.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class BasicValue : Value
    {
        /// <summary>An uninitialized value.</summary>
        public static readonly BasicValue Uninitialized_Value = new BasicValue(null);

        /// <summary>A byte, boolean, char, short, or int value.</summary>
        public static readonly BasicValue Int_Value = new BasicValue(Type.Int_Type);

        /// <summary>A float value.</summary>
        public static readonly BasicValue Float_Value = new BasicValue(Type.Float_Type);

        /// <summary>A long value.</summary>
        public static readonly BasicValue Long_Value = new BasicValue(Type.Long_Type);

        /// <summary>A double value.</summary>
        public static readonly BasicValue Double_Value = new BasicValue(Type.Double_Type);

        /// <summary>An object or array reference value.</summary>
        public static readonly BasicValue Reference_Value = new BasicValue(Type.GetObjectType
            ("java/lang/Object"));

        /// <summary>A return address value (produced by a jsr instruction).</summary>
        public static readonly BasicValue Returnaddress_Value = new BasicValue(Type.Void_Type
        );

        /// <summary>
        ///     The
        ///     <see cref="Type" />
        ///     of this value, or
        ///     <literal>null</literal>
        ///     for uninitialized values.
        /// </summary>
        private readonly Type type;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="BasicValue" />
        ///     of the given type.
        /// </summary>
        /// <param name="type">the value type.</param>
        public BasicValue(Type type)
        {
            this.type = type;
        }

        public virtual int GetSize()
        {
            return type == Type.Long_Type || type == Type.Double_Type ? 2 : 1;
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     of this value.
        /// </summary>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     of this value.
        /// </returns>
        public virtual Type GetType()
        {
            return type;
        }

        /// <summary>Returns whether this value corresponds to an object or array reference.</summary>
        /// <returns>whether this value corresponds to an object or array reference.</returns>
        public virtual bool IsReference()
        {
            return type != null && (type.GetSort() == Type.Object || type.GetSort() == Type.Array
                   );
        }

        public override bool Equals(object value)
        {
            if (value == this) return true;

            if (value is BasicValue)
            {
                if (type == null)
                    return ((BasicValue) value).type == null;
                return type.Equals(((BasicValue) value).type);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return type == null ? 0 : type.GetHashCode();
        }

        public override string ToString()
        {
            if (this == Uninitialized_Value)
                return ".";
            if (this == Returnaddress_Value)
                return "A";
            if (this == Reference_Value)
                return "R";
            return type.GetDescriptor();
        }
    }
}