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

using System.Collections.Generic;

namespace ObjectWeb.Asm.Tree.Analysis
{
	/// <summary>
	///     A
	///     <see cref="Value" />
	///     which keeps track of the bytecode instructions that can produce it.
	/// </summary>
	/// <author>Eric Bruneton</author>
	public class SourceValue : Value
    {
	    /// <summary>The instructions that can produce this value.</summary>
	    /// <remarks>
	    ///     The instructions that can produce this value. For example, for the Java code below, the
	    ///     instructions that can produce the value of
	    ///     <c>i</c>
	    ///     at line 5 are the two ISTORE instructions
	    ///     at line 1 and 3:
	    ///     <pre>
	    ///         1: i = 0;
	    ///         2: if (...) {
	    ///         3:   i = 1;
	    ///         4: }
	    ///         5: return i;
	    ///     </pre>
	    /// </remarks>
	    public readonly HashSet<AbstractInsnNode> insns;

	    /// <summary>The size of this value, in 32 bits words.</summary>
	    /// <remarks>
	    ///     The size of this value, in 32 bits words. This size is 1 for byte, boolean, char, short, int,
	    ///     float, object and array types, and 2 for long and double.
	    /// </remarks>
	    public readonly int size;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="SourceValue" />
	    ///     .
	    /// </summary>
	    /// <param name="size">
	    ///     the size of this value, in 32 bits words. This size is 1 for byte, boolean, char,
	    ///     short, int, float, object and array types, and 2 for long and double.
	    /// </param>
	    public SourceValue(int size)
            : this(size, new SmallSet<AbstractInsnNode>())
        {
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="SourceValue" />
	    ///     .
	    /// </summary>
	    /// <param name="size">
	    ///     the size of this value, in 32 bits words. This size is 1 for byte, boolean, char,
	    ///     short, int, float, object and array types, and 2 for long and double.
	    /// </param>
	    /// <param name="insnNode">an instruction that can produce this value.</param>
	    public SourceValue(int size, AbstractInsnNode insnNode)
        {
            this.size = size;
            insns = new SmallSet<AbstractInsnNode>(insnNode);
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="SourceValue" />
	    ///     .
	    /// </summary>
	    /// <param name="size">
	    ///     the size of this value, in 32 bits words. This size is 1 for byte, boolean, char,
	    ///     short, int, float, object and array types, and 2 for long and double.
	    /// </param>
	    /// <param name="insnSet">the instructions that can produce this value.</param>
	    public SourceValue(int size, HashSet<AbstractInsnNode> insnSet)
        {
            this.size = size;
            insns = insnSet;
        }

	    /// <summary>Returns the size of this value.</summary>
	    /// <returns>
	    ///     the size of this value, in 32 bits words. This size is 1 for byte, boolean, char,
	    ///     short, int, float, object and array types, and 2 for long and double.
	    /// </returns>
	    public virtual int GetSize()
        {
            return size;
        }

        public override bool Equals(object value)
        {
            if (!(value is SourceValue)) return false;
            var sourceValue = (SourceValue) value;
            return size == sourceValue.size && insns.Equals(sourceValue.insns);
        }

        public override int GetHashCode()
        {
            return insns.GetHashCode();
        }
    }
}