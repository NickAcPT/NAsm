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
    /// <summary>
    ///     An
    ///     <see cref="AbstractInsnNode" />
    ///     that encapsulates a
    ///     <see cref="Label" />
    ///     .
    /// </summary>
    public class LabelNode : AbstractInsnNode
    {
        private Label value;

        public LabelNode()
            : base(-1)
        {
        }

        public LabelNode(Label label)
            : base(-1)
        {
            value = label;
        }

        public override int GetType()
        {
            return Label;
        }

        /// <summary>Returns the label encapsulated by this node.</summary>
        /// <remarks>
        ///     Returns the label encapsulated by this node. A new label is created and associated with this
        ///     node if it was created without an encapsulated label.
        /// </remarks>
        /// <returns>the label encapsulated by this node.</returns>
        public virtual Label GetLabel()
        {
            if (value == null) value = new Label();
            return value;
        }

        public override void Accept(MethodVisitor methodVisitor)
        {
            methodVisitor.VisitLabel(GetLabel());
        }

        public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels
        )
        {
            return clonedLabels.GetOrNull(this);
        }

        public virtual void ResetLabel()
        {
            value = null;
        }
    }
}