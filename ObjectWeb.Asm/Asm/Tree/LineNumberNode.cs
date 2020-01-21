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

namespace ObjectWeb.Asm.Tree
{
	/// <summary>A node that represents a line number declaration.</summary>
	/// <remarks>
	///     A node that represents a line number declaration. These nodes are pseudo instruction nodes in
	///     order to be inserted in an instruction list.
	/// </remarks>
	/// <author>Eric Bruneton</author>
	public class LineNumberNode : AbstractInsnNode
    {
	    /// <summary>A line number.</summary>
	    /// <remarks>
	    ///     A line number. This number refers to the source file from which the class was compiled.
	    /// </remarks>
	    public int line;

        /// <summary>The first instruction corresponding to this line number.</summary>
        public LabelNode start;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="LineNumberNode" />
        ///     .
        /// </summary>
        /// <param name="line">
        ///     a line number. This number refers to the source file from which the class was
        ///     compiled.
        /// </param>
        /// <param name="start">the first instruction corresponding to this line number.</param>
        public LineNumberNode(int line, LabelNode start)
            : base(-1)
        {
            this.line = line;
            this.start = start;
        }

        public override int GetType()
        {
            return Line;
        }

        public override void Accept(MethodVisitor methodVisitor)
        {
            methodVisitor.VisitLineNumber(line, start.GetLabel());
        }

        public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels
        )
        {
            return new LineNumberNode(line, Clone(start, clonedLabels));
        }
    }
}