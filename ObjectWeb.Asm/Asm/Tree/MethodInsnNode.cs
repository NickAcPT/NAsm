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
    /// <summary>A node that represents a method instruction.</summary>
    /// <remarks>
    ///     A node that represents a method instruction. A method instruction is an instruction that invokes
    ///     a method.
    /// </remarks>
    /// <author>Eric Bruneton</author>
    public class MethodInsnNode : AbstractInsnNode
    {
        /// <summary>
        ///     The method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </summary>
        public string desc;

        /// <summary>Whether the method's owner class if an interface.</summary>
        public bool itf;

        /// <summary>The method's name.</summary>
        public string name;

        /// <summary>
        ///     The internal name of the method's owner class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        ///     <p>
        ///         For methods of arrays, e.g.,
        ///         <c>clone()</c>
        ///         , the array type descriptor.
        /// </summary>
        public string owner;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="MethodInsnNode" />
        ///     .
        /// </summary>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be constructed. This opcode must be
        ///     INVOKEVIRTUAL, INVOKESPECIAL, INVOKESTATIC or INVOKEINTERFACE.
        /// </param>
        /// <param name="owner">
        ///     the internal name of the method's owner class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        public MethodInsnNode(int opcode, string owner, string name, string descriptor)
            : this(opcode, owner, name, descriptor, opcode == OpcodesConstants.Invokeinterface
            )
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="MethodInsnNode" />
        ///     .
        /// </summary>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be constructed. This opcode must be
        ///     INVOKEVIRTUAL, INVOKESPECIAL, INVOKESTATIC or INVOKEINTERFACE.
        /// </param>
        /// <param name="owner">
        ///     the internal name of the method's owner class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="isInterface">if the method's owner class is an interface.</param>
        public MethodInsnNode(int opcode, string owner, string name, string descriptor, bool
            isInterface)
            : base(opcode)
        {
            this.owner = owner;
            this.name = name;
            desc = descriptor;
            itf = isInterface;
        }

        /// <summary>Sets the opcode of this instruction.</summary>
        /// <param name="opcode">
        ///     the new instruction opcode. This opcode must be INVOKEVIRTUAL, INVOKESPECIAL,
        ///     INVOKESTATIC or INVOKEINTERFACE.
        /// </param>
        public virtual void SetOpcode(int opcode)
        {
            this.opcode = opcode;
        }

        public override int GetType()
        {
            return Method_Insn;
        }

        public override void Accept(MethodVisitor methodVisitor)
        {
            methodVisitor.VisitMethodInsn(opcode, owner, name, desc, itf);
            AcceptAnnotations(methodVisitor);
        }

        public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels
        )
        {
            return new MethodInsnNode(opcode, owner, name, desc, itf).CloneAnnotations(this);
        }
    }
}