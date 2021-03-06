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

namespace ObjectWeb.Asm.Tree
{
    /// <summary>A node that represents a local variable declaration.</summary>
    /// <author>Eric Bruneton</author>
    public class LocalVariableNode
    {
        /// <summary>The type descriptor of this local variable.</summary>
        public string desc;

        /// <summary>
        ///     The last instruction corresponding to the scope of this local variable (exclusive).
        /// </summary>
        public LabelNode end;

        /// <summary>The local variable's index.</summary>
        public int index;

        /// <summary>The name of a local variable.</summary>
        public string name;

        /// <summary>The signature of this local variable.</summary>
        /// <remarks>
        ///     The signature of this local variable. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string signature;

        /// <summary>
        ///     The first instruction corresponding to the scope of this local variable (inclusive).
        /// </summary>
        public LabelNode start;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="LocalVariableNode" />
        ///     .
        /// </summary>
        /// <param name="name">the name of a local variable.</param>
        /// <param name="descriptor">the type descriptor of this local variable.</param>
        /// <param name="signature">
        ///     the signature of this local variable. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="start">
        ///     the first instruction corresponding to the scope of this local variable
        ///     (inclusive).
        /// </param>
        /// <param name="end">
        ///     the last instruction corresponding to the scope of this local variable (exclusive).
        /// </param>
        /// <param name="index">the local variable's index.</param>
        public LocalVariableNode(string name, string descriptor, string signature, LabelNode
            start, LabelNode end, int index)
        {
            this.name = name;
            desc = descriptor;
            this.signature = signature;
            this.start = start;
            this.end = end;
            this.index = index;
        }

        /// <summary>Makes the given visitor visit this local variable declaration.</summary>
        /// <param name="methodVisitor">a method visitor.</param>
        public virtual void Accept(MethodVisitor methodVisitor)
        {
            methodVisitor.VisitLocalVariable(name, desc, signature, start.GetLabel(), end.GetLabel
                (), index);
        }
    }
}