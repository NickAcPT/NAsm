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
using System.Linq;
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>
    ///     An
    ///     <see cref="Interpreter{V}" />
    ///     for
    ///     <see cref="SourceValue" />
    ///     values.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class SourceInterpreter : Interpreter<SourceValue>, Opcodes
    {
        /// <summary>
        ///     Constructs a new
        ///     <see cref="SourceInterpreter" />
        ///     for the latest ASM API version.
        ///     <i>
        ///         Subclasses must
        ///         not use this constructor
        ///     </i>
        ///     . Instead, they must use the
        ///     <see cref="SourceInterpreter(int)" />
        ///     version.
        /// </summary>
        public SourceInterpreter()
            : base(VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            if (GetType() != typeof(SourceInterpreter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SourceInterpreter" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this interpreter. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        protected internal SourceInterpreter(VisitorAsmApiVersion api)
            : base(api)
        {
        }

        public override SourceValue NewValue(Type type)
        {
            if (type == Type.Void_Type) return null;
            return new SourceValue(type == null ? 1 : type.GetSize());
        }

        public override SourceValue NewOperation(AbstractInsnNode insn)
        {
            int size;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Lconst_0:
                case OpcodesConstants.Lconst_1:
                case OpcodesConstants.Dconst_0:
                case OpcodesConstants.Dconst_1:
                {
                    size = 2;
                    break;
                }

                case OpcodesConstants.Ldc:
                {
                    var value = ((LdcInsnNode) insn).cst;
                    size = value is long || value is double ? 2 : 1;
                    break;
                }

                case OpcodesConstants.Getstatic:
                {
                    size = Type.GetType(((FieldInsnNode) insn).desc).GetSize();
                    break;
                }

                default:
                {
                    size = 1;
                    break;
                }
            }

            return new SourceValue(size, insn);
        }

        public override SourceValue CopyOperation(AbstractInsnNode insn, SourceValue value
        )
        {
            return new SourceValue(value.GetSize(), insn);
        }

        public override SourceValue UnaryOperation(AbstractInsnNode insn, SourceValue value
        )
        {
            int size;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Lneg:
                case OpcodesConstants.Dneg:
                case OpcodesConstants.I2l:
                case OpcodesConstants.I2d:
                case OpcodesConstants.L2d:
                case OpcodesConstants.F2l:
                case OpcodesConstants.F2d:
                case OpcodesConstants.D2l:
                {
                    size = 2;
                    break;
                }

                case OpcodesConstants.Getfield:
                {
                    size = Type.GetType(((FieldInsnNode) insn).desc).GetSize();
                    break;
                }

                default:
                {
                    size = 1;
                    break;
                }
            }

            return new SourceValue(size, insn);
        }

        public override SourceValue BinaryOperation(AbstractInsnNode insn, SourceValue value1
            , SourceValue value2)
        {
            int size;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Laload:
                case OpcodesConstants.Daload:
                case OpcodesConstants.Ladd:
                case OpcodesConstants.Dadd:
                case OpcodesConstants.Lsub:
                case OpcodesConstants.Dsub:
                case OpcodesConstants.Lmul:
                case OpcodesConstants.Dmul:
                case OpcodesConstants.Ldiv:
                case OpcodesConstants.Ddiv:
                case OpcodesConstants.Lrem:
                case OpcodesConstants.Drem:
                case OpcodesConstants.Lshl:
                case OpcodesConstants.Lshr:
                case OpcodesConstants.Lushr:
                case OpcodesConstants.Land:
                case OpcodesConstants.Lor:
                case OpcodesConstants.Lxor:
                {
                    size = 2;
                    break;
                }

                default:
                {
                    size = 1;
                    break;
                }
            }

            return new SourceValue(size, insn);
        }

        public override SourceValue TernaryOperation(AbstractInsnNode insn, SourceValue value1
            , SourceValue value2, SourceValue value3)
        {
            return new SourceValue(1, insn);
        }

        public override SourceValue NaryOperation<_T0>(AbstractInsnNode insn, IList<_T0>
            values)
        {
            int size;
            var opcode = insn.GetOpcode();
            if (opcode == OpcodesConstants.Multianewarray)
                size = 1;
            else if (opcode == OpcodesConstants.Invokedynamic)
                size = Type.GetReturnType(((InvokeDynamicInsnNode) insn).desc).GetSize();
            else
                size = Type.GetReturnType(((MethodInsnNode) insn).desc).GetSize();
            return new SourceValue(size, insn);
        }

        public override void ReturnOperation(AbstractInsnNode insn, SourceValue value, SourceValue
            expected)
        {
        }

        // Nothing to do.
        public override SourceValue Merge(SourceValue value1, SourceValue value2)
        {
            if (Runtime.InstanceOf(value1.insns, typeof(SmallSet<>)) && Runtime.InstanceOf
                    (value2.insns, typeof(SmallSet<>)))
            {
                var setUnion = ((SmallSet<AbstractInsnNode>) value1.insns).Union
                    ((SmallSet<AbstractInsnNode>) value2.insns).ToHashSet();
                if (setUnion == value1.insns && value1.size == value2.size)
                    return value1;
                return new SourceValue(Math.Min(value1.size, value2.size), setUnion);
            }

            if (value1.size != value2.size || !ContainsAll(value1.insns, value2.insns))
            {
                var setUnion = new HashSet<AbstractInsnNode>();
                Collections.AddAll(setUnion, value1.insns);
                Collections.AddAll(setUnion, value2.insns);
                return new SourceValue(Math.Min(value1.size, value2.size), setUnion);
            }

            return value1;
        }

        private static bool ContainsAll<E>(HashSet<E> self, HashSet<E> other)
        {
            if (self.Count < other.Count) return false;
            return other.All(self.Contains);
        }
    }
}