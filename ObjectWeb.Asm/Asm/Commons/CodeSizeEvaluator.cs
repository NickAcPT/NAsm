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

using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     that approximates the size of the methods it visits.
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    public class CodeSizeEvaluator : MethodVisitor, Opcodes
    {
        /// <summary>The maximum size in bytes of the visited method.</summary>
        private int maxSize;

        /// <summary>The minimum size in bytes of the visited method.</summary>
        private int minSize;

        public CodeSizeEvaluator(MethodVisitor methodVisitor)
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, methodVisitor)
        {
        }

        protected internal CodeSizeEvaluator(VisitorAsmApiVersion api, MethodVisitor methodVisitor)
            : base(api, methodVisitor)
        {
        }

        /* latest api = */

        public virtual int MinSize
        {
            get { return minSize; }
        }

        public virtual int MaxSize
        {
            get { return maxSize; }
        }

        public override void VisitInsn(int opcode)
        {
            minSize += 1;
            maxSize += 1;
            base.VisitInsn(opcode);
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            if (opcode == OpcodesConstants.Sipush)
            {
                minSize += 3;
                maxSize += 3;
            }
            else
            {
                minSize += 2;
                maxSize += 2;
            }

            base.VisitIntInsn(opcode, operand);
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            if (var < 4 && opcode != OpcodesConstants.Ret)
            {
                minSize += 1;
                maxSize += 1;
            }
            else if (var >= 256)
            {
                minSize += 4;
                maxSize += 4;
            }
            else
            {
                minSize += 2;
                maxSize += 2;
            }

            base.VisitVarInsn(opcode, var);
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            minSize += 3;
            maxSize += 3;
            base.VisitTypeInsn(opcode, type);
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            minSize += 3;
            maxSize += 3;
            base.VisitFieldInsn(opcode, owner, name, descriptor);
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name
            , string descriptor, bool isInterface)
        {
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5 && (opcodeAndSource & OpcodesConstants.Source_Deprecated
                ) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            var opcode = opcodeAndSource & ~OpcodesConstants.Source_Mask;
            if (opcode == OpcodesConstants.Invokeinterface)
            {
                minSize += 5;
                maxSize += 5;
            }
            else
            {
                minSize += 3;
                maxSize += 3;
            }

            base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            minSize += 5;
            maxSize += 5;
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            minSize += 3;
            if (opcode == OpcodesConstants.Goto || opcode == OpcodesConstants.Jsr)
                maxSize += 5;
            else
                maxSize += 8;
            base.VisitJumpInsn(opcode, label);
        }

        public override void VisitLdcInsn(object value)
        {
            if (value is long || value is double || value is ConstantDynamic && ((ConstantDynamic
                    ) value).GetSize() == 2)
            {
                minSize += 3;
                maxSize += 3;
            }
            else
            {
                minSize += 2;
                maxSize += 3;
            }

            base.VisitLdcInsn(value);
        }

        public override void VisitIincInsn(int var, int increment)
        {
            if (var > 255 || increment > 127 || increment < -128)
            {
                minSize += 6;
                maxSize += 6;
            }
            else
            {
                minSize += 3;
                maxSize += 3;
            }

            base.VisitIincInsn(var, increment);
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            minSize += 13 + labels.Length * 4;
            maxSize += 16 + labels.Length * 4;
            base.VisitTableSwitchInsn(min, max, dflt, labels);
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            minSize += 9 + keys.Length * 8;
            maxSize += 12 + keys.Length * 8;
            base.VisitLookupSwitchInsn(dflt, keys, labels);
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            minSize += 4;
            maxSize += 4;
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
        }
    }
}