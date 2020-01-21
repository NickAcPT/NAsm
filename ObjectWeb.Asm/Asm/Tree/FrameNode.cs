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

using System;
using System.Collections.Generic;

namespace ObjectWeb.Asm.Tree
{
    /// <summary>A node that represents a stack map frame.</summary>
    /// <remarks>
    ///     A node that represents a stack map frame. These nodes are pseudo instruction nodes in order to be
    ///     inserted in an instruction list. In fact these nodes must(*) be inserted <i>just before</i> any
    ///     instruction node <b>i</b> that follows an unconditionnal branch instruction such as GOTO or
    ///     THROW, that is the target of a jump instruction, or that starts an exception handler block. The
    ///     stack map frame types must describe the values of the local variables and of the operand stack
    ///     elements <i>just before</i> <b>i</b> is executed. <br />
    ///     <br />
    ///     (*) this is mandatory only for classes whose version is greater than or equal to
    ///     <see cref="Org.Objectweb.Asm.Opcodes.V1_6" />
    ///     .
    /// </remarks>
    /// <author>Eric Bruneton</author>
    public class FrameNode : AbstractInsnNode
    {
        /// <summary>The types of the local variables of this stack map frame.</summary>
        /// <remarks>
        ///     The types of the local variables of this stack map frame. Elements of this list can be Integer,
        ///     String or LabelNode objects (for primitive, reference and uninitialized types respectively -
        ///     see
        ///     <see cref="MethodVisitor" />
        ///     ).
        /// </remarks>
        public List<object> local;

        /// <summary>The types of the operand stack elements of this stack map frame.</summary>
        /// <remarks>
        ///     The types of the operand stack elements of this stack map frame. Elements of this list can be
        ///     Integer, String or LabelNode objects (for primitive, reference and uninitialized types
        ///     respectively - see
        ///     <see cref="MethodVisitor" />
        ///     ).
        /// </remarks>
        public List<object> stack;

        /// <summary>The type of this frame.</summary>
        /// <remarks>
        ///     The type of this frame. Must be
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_New" />
        ///     for expanded frames, or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Full" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Append" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Chop" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Same" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Append" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Same1" />
        ///     for compressed frames.
        /// </remarks>
        public int type;

        private FrameNode()
            : base(-1)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="FrameNode" />
        ///     .
        /// </summary>
        /// <param name="type">
        ///     the type of this frame. Must be
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_New" />
        ///     for expanded frames, or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Full" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Append" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Chop" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Same" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Append" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Same1" />
        ///     for compressed frames.
        /// </param>
        /// <param name="numLocal">number of local variables of this stack map frame.</param>
        /// <param name="local">
        ///     the types of the local variables of this stack map frame. Elements of this list
        ///     can be Integer, String or LabelNode objects (for primitive, reference and uninitialized
        ///     types respectively - see
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor" />
        ///     ).
        /// </param>
        /// <param name="numStack">number of operand stack elements of this stack map frame.</param>
        /// <param name="stack">
        ///     the types of the operand stack elements of this stack map frame. Elements of this
        ///     list can be Integer, String or LabelNode objects (for primitive, reference and
        ///     uninitialized types respectively - see
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor" />
        ///     ).
        /// </param>
        public FrameNode(int type, int numLocal, object[] local, int numStack, object[] stack
        )
            : base(-1)
        {
            this.type = type;
            switch (type)
            {
                case OpcodesConstants.F_New:
                case OpcodesConstants.F_Full:
                {
                    this.local = Util.AsArrayList(numLocal, local);
                    this.stack = Util.AsArrayList(numStack, stack);
                    break;
                }

                case OpcodesConstants.F_Append:
                {
                    this.local = Util.AsArrayList(numLocal, local);
                    break;
                }

                case OpcodesConstants.F_Chop:
                {
                    this.local = Util.AsObjectArrayList(numLocal);
                    break;
                }

                case OpcodesConstants.F_Same:
                {
                    break;
                }

                case OpcodesConstants.F_Same1:
                {
                    this.stack = Util.AsArrayList(1, stack);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override int GetType()
        {
            return Frame;
        }

        public override void Accept(MethodVisitor methodVisitor)
        {
            switch (type)
            {
                case OpcodesConstants.F_New:
                case OpcodesConstants.F_Full:
                {
                    methodVisitor.VisitFrame(type, local.Count, AsArray(local), stack.Count, AsArray(
                        stack));
                    break;
                }

                case OpcodesConstants.F_Append:
                {
                    methodVisitor.VisitFrame(type, local.Count, AsArray(local), 0, null);
                    break;
                }

                case OpcodesConstants.F_Chop:
                {
                    methodVisitor.VisitFrame(type, local.Count, null, 0, null);
                    break;
                }

                case OpcodesConstants.F_Same:
                {
                    methodVisitor.VisitFrame(type, 0, null, 0, null);
                    break;
                }

                case OpcodesConstants.F_Same1:
                {
                    methodVisitor.VisitFrame(type, 0, null, 1, AsArray(stack));
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels
        )
        {
            var clone = new FrameNode();
            clone.type = type;
            if (local != null)
            {
                clone.local = new List<object>();
                for (int i = 0, n = local.Count; i < n; ++i)
                {
                    var localElement = local[i];
                    if (localElement is LabelNode element) localElement = clonedLabels.GetOrNull(element);
                    clone.local.Add(localElement);
                }
            }

            if (stack != null)
            {
                clone.stack = new List<object>();
                for (int i = 0, n = stack.Count; i < n; ++i)
                {
                    var stackElement = stack[i];
                    if (stackElement is LabelNode element) stackElement = clonedLabels.GetOrNull(element);
                    clone.stack.Add(stackElement);
                }
            }

            return clone;
        }

        private static object[] AsArray(List<object> list)
        {
            var array = new object[list.Count];
            for (int i = 0, n = array.Length; i < n; ++i)
            {
                var o = list[i];
                if (o is LabelNode) o = ((LabelNode) o).GetLabel();
                array[i] = o;
            }

            return array;
        }
    }
}