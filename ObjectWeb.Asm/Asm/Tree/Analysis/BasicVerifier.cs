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
using ObjectWeb.Asm.Enums;
using ObjectWeb.Misc.Java.Lang;

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>
    ///     An extended
    ///     <see cref="BasicInterpreter" />
    ///     that checks that bytecode instructions are correctly used.
    /// </summary>
    /// <author>Eric Bruneton</author>
    /// <author>Bing Ran</author>
    public class BasicVerifier : BasicInterpreter
    {
        /// <summary>
        ///     Constructs a new
        ///     <see cref="BasicVerifier" />
        ///     for the latest ASM API version.
        ///     <i>
        ///         Subclasses must not
        ///         use this constructor
        ///     </i>
        ///     . Instead, they must use the
        ///     <see cref="BasicVerifier(int)" />
        ///     version.
        /// </summary>
        public BasicVerifier()
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            if (GetType() != typeof(BasicVerifier)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="BasicVerifier" />
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
        protected internal BasicVerifier(VisitorAsmApiVersion api)
            : base(api)
        {
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue CopyOperation(AbstractInsnNode insn, BasicValue value)
        {
            Value expected;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Iload:
                case OpcodesConstants.Istore:
                {
                    expected = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Fload:
                case OpcodesConstants.Fstore:
                {
                    expected = BasicValue.Float_Value;
                    break;
                }

                case OpcodesConstants.Lload:
                case OpcodesConstants.Lstore:
                {
                    expected = BasicValue.Long_Value;
                    break;
                }

                case OpcodesConstants.Dload:
                case OpcodesConstants.Dstore:
                {
                    expected = BasicValue.Double_Value;
                    break;
                }

                case OpcodesConstants.Aload:
                {
                    if (!value.IsReference()) throw new AnalyzerException(insn, null, "an object reference", value);
                    return value;
                }

                case OpcodesConstants.Astore:
                {
                    if (!value.IsReference() && !BasicValue.Returnaddress_Value.Equals(value))
                        throw new AnalyzerException(insn, null, "an object reference or a return address"
                            , value);
                    return value;
                }

                default:
                {
                    return value;
                }
            }

            if (!expected.Equals(value)) throw new AnalyzerException(insn, null, expected, value);
            return value;
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue UnaryOperation(AbstractInsnNode insn, BasicValue value
        )
        {
            BasicValue expected;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Ineg:
                case OpcodesConstants.Iinc:
                case OpcodesConstants.I2f:
                case OpcodesConstants.I2l:
                case OpcodesConstants.I2d:
                case OpcodesConstants.I2b:
                case OpcodesConstants.I2c:
                case OpcodesConstants.I2s:
                case OpcodesConstants.Ifeq:
                case OpcodesConstants.Ifne:
                case OpcodesConstants.Iflt:
                case OpcodesConstants.Ifge:
                case OpcodesConstants.Ifgt:
                case OpcodesConstants.Ifle:
                case OpcodesConstants.Tableswitch:
                case OpcodesConstants.Lookupswitch:
                case OpcodesConstants.Ireturn:
                case OpcodesConstants.Newarray:
                case OpcodesConstants.Anewarray:
                {
                    expected = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Fneg:
                case OpcodesConstants.F2i:
                case OpcodesConstants.F2l:
                case OpcodesConstants.F2d:
                case OpcodesConstants.Freturn:
                {
                    expected = BasicValue.Float_Value;
                    break;
                }

                case OpcodesConstants.Lneg:
                case OpcodesConstants.L2i:
                case OpcodesConstants.L2f:
                case OpcodesConstants.L2d:
                case OpcodesConstants.Lreturn:
                {
                    expected = BasicValue.Long_Value;
                    break;
                }

                case OpcodesConstants.Dneg:
                case OpcodesConstants.D2i:
                case OpcodesConstants.D2f:
                case OpcodesConstants.D2l:
                case OpcodesConstants.Dreturn:
                {
                    expected = BasicValue.Double_Value;
                    break;
                }

                case OpcodesConstants.Getfield:
                {
                    expected = NewValue(Type.GetObjectType(((FieldInsnNode) insn).owner));
                    break;
                }

                case OpcodesConstants.Arraylength:
                {
                    if (!IsArrayValue(value)) throw new AnalyzerException(insn, null, "an array reference", value);
                    return base.UnaryOperation(insn, value);
                }

                case OpcodesConstants.Checkcast:
                case OpcodesConstants.Areturn:
                case OpcodesConstants.Athrow:
                case OpcodesConstants.Instanceof:
                case OpcodesConstants.Monitorenter:
                case OpcodesConstants.Monitorexit:
                case OpcodesConstants.Ifnull:
                case OpcodesConstants.Ifnonnull:
                {
                    if (!value.IsReference()) throw new AnalyzerException(insn, null, "an object reference", value);
                    return base.UnaryOperation(insn, value);
                }

                case OpcodesConstants.Putstatic:
                {
                    expected = NewValue(Type.GetType(((FieldInsnNode) insn).desc));
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }

            if (!IsSubTypeOf(value, expected)) throw new AnalyzerException(insn, null, expected, value);
            return base.UnaryOperation(insn, value);
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue BinaryOperation(AbstractInsnNode insn, BasicValue value1
            , BasicValue value2)
        {
            BasicValue expected1;
            BasicValue expected2;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Iaload:
                {
                    expected1 = NewValue(Type.GetType("[I"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Baload:
                {
                    if (IsSubTypeOf(value1, NewValue(Type.GetType("[Z"))))
                        expected1 = NewValue(Type.GetType("[Z"));
                    else
                        expected1 = NewValue(Type.GetType("[B"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Caload:
                {
                    expected1 = NewValue(Type.GetType("[C"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Saload:
                {
                    expected1 = NewValue(Type.GetType("[S"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Laload:
                {
                    expected1 = NewValue(Type.GetType("[J"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Faload:
                {
                    expected1 = NewValue(Type.GetType("[F"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Daload:
                {
                    expected1 = NewValue(Type.GetType("[D"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Aaload:
                {
                    expected1 = NewValue(Type.GetType("[Ljava/lang/Object;"));
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Iadd:
                case OpcodesConstants.Isub:
                case OpcodesConstants.Imul:
                case OpcodesConstants.Idiv:
                case OpcodesConstants.Irem:
                case OpcodesConstants.Ishl:
                case OpcodesConstants.Ishr:
                case OpcodesConstants.Iushr:
                case OpcodesConstants.Iand:
                case OpcodesConstants.Ior:
                case OpcodesConstants.Ixor:
                case OpcodesConstants.If_Icmpeq:
                case OpcodesConstants.If_Icmpne:
                case OpcodesConstants.If_Icmplt:
                case OpcodesConstants.If_Icmpge:
                case OpcodesConstants.If_Icmpgt:
                case OpcodesConstants.If_Icmple:
                {
                    expected1 = BasicValue.Int_Value;
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Fadd:
                case OpcodesConstants.Fsub:
                case OpcodesConstants.Fmul:
                case OpcodesConstants.Fdiv:
                case OpcodesConstants.Frem:
                case OpcodesConstants.Fcmpl:
                case OpcodesConstants.Fcmpg:
                {
                    expected1 = BasicValue.Float_Value;
                    expected2 = BasicValue.Float_Value;
                    break;
                }

                case OpcodesConstants.Ladd:
                case OpcodesConstants.Lsub:
                case OpcodesConstants.Lmul:
                case OpcodesConstants.Ldiv:
                case OpcodesConstants.Lrem:
                case OpcodesConstants.Land:
                case OpcodesConstants.Lor:
                case OpcodesConstants.Lxor:
                case OpcodesConstants.Lcmp:
                {
                    expected1 = BasicValue.Long_Value;
                    expected2 = BasicValue.Long_Value;
                    break;
                }

                case OpcodesConstants.Lshl:
                case OpcodesConstants.Lshr:
                case OpcodesConstants.Lushr:
                {
                    expected1 = BasicValue.Long_Value;
                    expected2 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Dadd:
                case OpcodesConstants.Dsub:
                case OpcodesConstants.Dmul:
                case OpcodesConstants.Ddiv:
                case OpcodesConstants.Drem:
                case OpcodesConstants.Dcmpl:
                case OpcodesConstants.Dcmpg:
                {
                    expected1 = BasicValue.Double_Value;
                    expected2 = BasicValue.Double_Value;
                    break;
                }

                case OpcodesConstants.If_Acmpeq:
                case OpcodesConstants.If_Acmpne:
                {
                    expected1 = BasicValue.Reference_Value;
                    expected2 = BasicValue.Reference_Value;
                    break;
                }

                case OpcodesConstants.Putfield:
                {
                    var fieldInsn = (FieldInsnNode) insn;
                    expected1 = NewValue(Type.GetObjectType(fieldInsn.owner));
                    expected2 = NewValue(Type.GetType(fieldInsn.desc));
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }

            if (!IsSubTypeOf(value1, expected1))
                throw new AnalyzerException(insn, "First argument", expected1, value1);
            if (!IsSubTypeOf(value2, expected2))
                throw new AnalyzerException(insn, "Second argument", expected2, value2);
            if (insn.GetOpcode() == OpcodesConstants.Aaload)
                return GetElementValue(value1);
            return base.BinaryOperation(insn, value1, value2);
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue TernaryOperation(AbstractInsnNode insn, BasicValue value1
            , BasicValue value2, BasicValue value3)
        {
            BasicValue expected1;
            BasicValue expected3;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Iastore:
                {
                    expected1 = NewValue(Type.GetType("[I"));
                    expected3 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Bastore:
                {
                    if (IsSubTypeOf(value1, NewValue(Type.GetType("[Z"))))
                        expected1 = NewValue(Type.GetType("[Z"));
                    else
                        expected1 = NewValue(Type.GetType("[B"));
                    expected3 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Castore:
                {
                    expected1 = NewValue(Type.GetType("[C"));
                    expected3 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Sastore:
                {
                    expected1 = NewValue(Type.GetType("[S"));
                    expected3 = BasicValue.Int_Value;
                    break;
                }

                case OpcodesConstants.Lastore:
                {
                    expected1 = NewValue(Type.GetType("[J"));
                    expected3 = BasicValue.Long_Value;
                    break;
                }

                case OpcodesConstants.Fastore:
                {
                    expected1 = NewValue(Type.GetType("[F"));
                    expected3 = BasicValue.Float_Value;
                    break;
                }

                case OpcodesConstants.Dastore:
                {
                    expected1 = NewValue(Type.GetType("[D"));
                    expected3 = BasicValue.Double_Value;
                    break;
                }

                case OpcodesConstants.Aastore:
                {
                    expected1 = value1;
                    expected3 = BasicValue.Reference_Value;
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }

            if (!IsSubTypeOf(value1, expected1))
                throw new AnalyzerException(insn, "First argument", "a " + expected1 + " array reference"
                    , value1);
            if (!BasicValue.Int_Value.Equals(value2))
                throw new AnalyzerException(insn, "Second argument", BasicValue.Int_Value, value2
                );
            if (!IsSubTypeOf(value3, expected3)) throw new AnalyzerException(insn, "Third argument", expected3, value3);
            return null;
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue NaryOperation<_T0>(AbstractInsnNode insn, IList<_T0> values
        )
        {
            var opcode = insn.GetOpcode();
            if (opcode == OpcodesConstants.Multianewarray)
            {
                foreach (var value in values)
                    if (!BasicValue.Int_Value.Equals(value))
                        throw new AnalyzerException(insn, null, BasicValue.Int_Value, value);
            }
            else
            {
                var i = 0;
                var j = 0;
                if (opcode != OpcodesConstants.Invokestatic && opcode != OpcodesConstants.Invokedynamic)
                {
                    var owner = Type.GetObjectType(((MethodInsnNode) insn).owner);
                    if (!IsSubTypeOf(values[i++], NewValue(owner)))
                        throw new AnalyzerException(insn, "Method owner", NewValue(owner), values[0]);
                }

                var methodDescriptor = opcode == OpcodesConstants.Invokedynamic
                    ? ((InvokeDynamicInsnNode
                        ) insn).desc
                    : ((MethodInsnNode) insn).desc;
                var args = Type.GetArgumentTypes(methodDescriptor);
                while (i < values.Count)
                {
                    var expected = NewValue(args[j++]);
                    var actual = values[i++];
                    if (!IsSubTypeOf(actual, expected))
                        throw new AnalyzerException(insn, "Argument " + j, expected, actual);
                }
            }

            return base.NaryOperation(insn, values);
        }

        /// <exception cref="AnalyzerException" />
        public override void ReturnOperation(AbstractInsnNode insn, BasicValue value, BasicValue
            expected)
        {
            if (!IsSubTypeOf(value, expected))
                throw new AnalyzerException(insn, "Incompatible return type", expected, value);
        }

        /// <summary>Returns whether the given value corresponds to an array reference.</summary>
        /// <param name="value">a value.</param>
        /// <returns>whether 'value' corresponds to an array reference.</returns>
        protected internal virtual bool IsArrayValue(BasicValue value)
        {
            return value.IsReference();
        }

        /// <summary>
        ///     Returns the value corresponding to the type of the elements of the given array reference value.
        /// </summary>
        /// <param name="objectArrayValue">
        ///     a value corresponding to array of object (or array) references.
        /// </param>
        /// <returns>
        ///     the value corresponding to the type of the elements of 'objectArrayValue'.
        /// </returns>
        /// <exception cref="AnalyzerException">
        ///     if objectArrayValue does not correspond to an array type.
        /// </exception>
        /// <exception cref="AnalyzerException" />
        protected internal virtual BasicValue GetElementValue(BasicValue objectArrayValue
        )
        {
            return BasicValue.Reference_Value;
        }

        /// <summary>
        ///     Returns whether the type corresponding to the first argument is a subtype of the type
        ///     corresponding to the second argument.
        /// </summary>
        /// <param name="value">a value.</param>
        /// <param name="expected">another value.</param>
        /// <returns>
        ///     whether the type corresponding to 'value' is a subtype of the type corresponding to
        ///     'expected'.
        /// </returns>
        protected internal virtual bool IsSubTypeOf(BasicValue value, BasicValue expected
        )
        {
            return value.Equals(expected);
        }
    }
}