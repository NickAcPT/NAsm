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
    ///     An
    ///     <see cref="Interpreter{V}" />
    ///     for
    ///     <see cref="BasicValue" />
    ///     values.
    /// </summary>
    /// <author>Eric Bruneton</author>
    /// <author>Bing Ran</author>
    public class BasicInterpreter : Interpreter<BasicValue>, Opcodes
    {
        /// <summary>
        ///     Special type used for the
        ///     <literal>null</literal>
        ///     literal. This is an object reference type with
        ///     descriptor 'Lnull;'.
        /// </summary>
        public static readonly Type Null_Type = Type.GetObjectType("null");

        /// <summary>
        ///     Constructs a new
        ///     <see cref="BasicInterpreter" />
        ///     for the latest ASM API version.
        ///     <i>
        ///         Subclasses must
        ///         not use this constructor
        ///     </i>
        ///     . Instead, they must use the
        ///     <see cref="BasicInterpreter(int)" />
        ///     version.
        /// </summary>
        public BasicInterpreter()
            : base(VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            if (GetType() != typeof(BasicInterpreter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="BasicInterpreter" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this interpreter. Must be one of
        ///     <see cref="Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Opcodes.Asm6" />
        ///     or
        ///     <see cref="Opcodes.Asm7" />
        ///     .
        /// </param>
        protected internal BasicInterpreter(VisitorAsmApiVersion api)
            : base(api)
        {
        }

        public override BasicValue NewValue(Type type)
        {
            if (type == null) return BasicValue.Uninitialized_Value;
            switch (type.GetSort())
            {
                case Type.Void:
                {
                    return null;
                }

                case Type.Boolean:
                case Type.Char:
                case Type.Byte:
                case Type.Short:
                case Type.Int:
                {
                    return BasicValue.Int_Value;
                }

                case Type.Float:
                {
                    return BasicValue.Float_Value;
                }

                case Type.Long:
                {
                    return BasicValue.Long_Value;
                }

                case Type.Double:
                {
                    return BasicValue.Double_Value;
                }

                case Type.Array:
                case Type.Object:
                {
                    return BasicValue.Reference_Value;
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue NewOperation(AbstractInsnNode insn)
        {
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Aconst_Null:
                {
                    return NewValue(Null_Type);
                }

                case OpcodesConstants.Iconst_M1:
                case OpcodesConstants.Iconst_0:
                case OpcodesConstants.Iconst_1:
                case OpcodesConstants.Iconst_2:
                case OpcodesConstants.Iconst_3:
                case OpcodesConstants.Iconst_4:
                case OpcodesConstants.Iconst_5:
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.Lconst_0:
                case OpcodesConstants.Lconst_1:
                {
                    return BasicValue.Long_Value;
                }

                case OpcodesConstants.Fconst_0:
                case OpcodesConstants.Fconst_1:
                case OpcodesConstants.Fconst_2:
                {
                    return BasicValue.Float_Value;
                }

                case OpcodesConstants.Dconst_0:
                case OpcodesConstants.Dconst_1:
                {
                    return BasicValue.Double_Value;
                }

                case OpcodesConstants.Bipush:
                case OpcodesConstants.Sipush:
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.Ldc:
                {
                    var value = ((LdcInsnNode) insn).cst;
                    if (value is int) return BasicValue.Int_Value;

                    if (value is float) return BasicValue.Float_Value;

                    if (value is long) return BasicValue.Long_Value;

                    if (value is double) return BasicValue.Double_Value;

                    if (value is string) return NewValue(Type.GetObjectType("java/lang/String"));

                    if (value is Type)
                    {
                        var sort = ((Type) value).GetSort();
                        if (sort == Type.Object || sort == Type.Array)
                            return NewValue(Type.GetObjectType("java/lang/Class"));
                        if (sort == Type.Method)
                            return NewValue(Type.GetObjectType("java/lang/invoke/MethodType"));
                        throw new AnalyzerException(insn, "Illegal LDC value " + value);
                    }

                    if (value is Handle)
                        return NewValue(Type.GetObjectType("java/lang/invoke/MethodHandle"));
                    if (value is ConstantDynamic)
                        return NewValue(Type.GetType(((ConstantDynamic) value).GetDescriptor()));
                    throw new AnalyzerException(insn, "Illegal LDC value " + value);
                    goto case OpcodesConstants.Jsr;
                }

                case OpcodesConstants.Jsr:
                {
                    return BasicValue.Returnaddress_Value;
                }

                case OpcodesConstants.Getstatic:
                {
                    return NewValue(Type.GetType(((FieldInsnNode) insn).desc));
                }

                case OpcodesConstants.New:
                {
                    return NewValue(Type.GetObjectType(((TypeInsnNode) insn).desc));
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue CopyOperation(AbstractInsnNode insn, BasicValue value)
        {
            return value;
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue UnaryOperation(AbstractInsnNode insn, BasicValue value
        )
        {
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Ineg:
                case OpcodesConstants.Iinc:
                case OpcodesConstants.L2i:
                case OpcodesConstants.F2i:
                case OpcodesConstants.D2i:
                case OpcodesConstants.I2b:
                case OpcodesConstants.I2c:
                case OpcodesConstants.I2s:
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.Fneg:
                case OpcodesConstants.I2f:
                case OpcodesConstants.L2f:
                case OpcodesConstants.D2f:
                {
                    return BasicValue.Float_Value;
                }

                case OpcodesConstants.Lneg:
                case OpcodesConstants.I2l:
                case OpcodesConstants.F2l:
                case OpcodesConstants.D2l:
                {
                    return BasicValue.Long_Value;
                }

                case OpcodesConstants.Dneg:
                case OpcodesConstants.I2d:
                case OpcodesConstants.L2d:
                case OpcodesConstants.F2d:
                {
                    return BasicValue.Double_Value;
                }

                case OpcodesConstants.Ifeq:
                case OpcodesConstants.Ifne:
                case OpcodesConstants.Iflt:
                case OpcodesConstants.Ifge:
                case OpcodesConstants.Ifgt:
                case OpcodesConstants.Ifle:
                case OpcodesConstants.Tableswitch:
                case OpcodesConstants.Lookupswitch:
                case OpcodesConstants.Ireturn:
                case OpcodesConstants.Lreturn:
                case OpcodesConstants.Freturn:
                case OpcodesConstants.Dreturn:
                case OpcodesConstants.Areturn:
                case OpcodesConstants.Putstatic:
                {
                    return null;
                }

                case OpcodesConstants.Getfield:
                {
                    return NewValue(Type.GetType(((FieldInsnNode) insn).desc));
                }

                case OpcodesConstants.Newarray:
                {
                    switch (((IntInsnNode) insn).operand)
                    {
                        case OpcodesConstants.T_Boolean:
                        {
                            return NewValue(Type.GetType("[Z"));
                        }

                        case OpcodesConstants.T_Char:
                        {
                            return NewValue(Type.GetType("[C"));
                        }

                        case OpcodesConstants.T_Byte:
                        {
                            return NewValue(Type.GetType("[B"));
                        }

                        case OpcodesConstants.T_Short:
                        {
                            return NewValue(Type.GetType("[S"));
                        }

                        case OpcodesConstants.T_Int:
                        {
                            return NewValue(Type.GetType("[I"));
                        }

                        case OpcodesConstants.T_Float:
                        {
                            return NewValue(Type.GetType("[F"));
                        }

                        case OpcodesConstants.T_Double:
                        {
                            return NewValue(Type.GetType("[D"));
                        }

                        case OpcodesConstants.T_Long:
                        {
                            return NewValue(Type.GetType("[J"));
                        }
                    }

                    throw new AnalyzerException(insn, "Invalid array type");
                }

                case OpcodesConstants.Anewarray:
                {
                    return NewValue(Type.GetType("[" + Type.GetObjectType(((TypeInsnNode) insn).desc))
                    );
                }

                case OpcodesConstants.Arraylength:
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.Athrow:
                {
                    return null;
                }

                case OpcodesConstants.Checkcast:
                {
                    return NewValue(Type.GetObjectType(((TypeInsnNode) insn).desc));
                }

                case OpcodesConstants.Instanceof:
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.Monitorenter:
                case OpcodesConstants.Monitorexit:
                case OpcodesConstants.Ifnull:
                case OpcodesConstants.Ifnonnull:
                {
                    return null;
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue BinaryOperation(AbstractInsnNode insn, BasicValue value1
            , BasicValue value2)
        {
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Iaload:
                case OpcodesConstants.Baload:
                case OpcodesConstants.Caload:
                case OpcodesConstants.Saload:
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
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.Faload:
                case OpcodesConstants.Fadd:
                case OpcodesConstants.Fsub:
                case OpcodesConstants.Fmul:
                case OpcodesConstants.Fdiv:
                case OpcodesConstants.Frem:
                {
                    return BasicValue.Float_Value;
                }

                case OpcodesConstants.Laload:
                case OpcodesConstants.Ladd:
                case OpcodesConstants.Lsub:
                case OpcodesConstants.Lmul:
                case OpcodesConstants.Ldiv:
                case OpcodesConstants.Lrem:
                case OpcodesConstants.Lshl:
                case OpcodesConstants.Lshr:
                case OpcodesConstants.Lushr:
                case OpcodesConstants.Land:
                case OpcodesConstants.Lor:
                case OpcodesConstants.Lxor:
                {
                    return BasicValue.Long_Value;
                }

                case OpcodesConstants.Daload:
                case OpcodesConstants.Dadd:
                case OpcodesConstants.Dsub:
                case OpcodesConstants.Dmul:
                case OpcodesConstants.Ddiv:
                case OpcodesConstants.Drem:
                {
                    return BasicValue.Double_Value;
                }

                case OpcodesConstants.Aaload:
                {
                    return BasicValue.Reference_Value;
                }

                case OpcodesConstants.Lcmp:
                case OpcodesConstants.Fcmpl:
                case OpcodesConstants.Fcmpg:
                case OpcodesConstants.Dcmpl:
                case OpcodesConstants.Dcmpg:
                {
                    return BasicValue.Int_Value;
                }

                case OpcodesConstants.If_Icmpeq:
                case OpcodesConstants.If_Icmpne:
                case OpcodesConstants.If_Icmplt:
                case OpcodesConstants.If_Icmpge:
                case OpcodesConstants.If_Icmpgt:
                case OpcodesConstants.If_Icmple:
                case OpcodesConstants.If_Acmpeq:
                case OpcodesConstants.If_Acmpne:
                case OpcodesConstants.Putfield:
                {
                    return null;
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue TernaryOperation(AbstractInsnNode insn, BasicValue value1
            , BasicValue value2, BasicValue value3)
        {
            return null;
        }

        /// <exception cref="AnalyzerException" />
        public override BasicValue NaryOperation<_T0>(AbstractInsnNode insn, IList<_T0> values
        )
        {
            var opcode = insn.GetOpcode();
            if (opcode == OpcodesConstants.Multianewarray)
                return NewValue(Type.GetType(((MultiANewArrayInsnNode) insn).desc));
            if (opcode == OpcodesConstants.Invokedynamic)
                return NewValue(Type.GetReturnType(((InvokeDynamicInsnNode) insn).desc));
            return NewValue(Type.GetReturnType(((MethodInsnNode) insn).desc));
        }

        /// <exception cref="AnalyzerException" />
        public override void ReturnOperation(AbstractInsnNode insn, BasicValue value, BasicValue
            expected)
        {
        }

        // Nothing to do.
        public override BasicValue Merge(BasicValue value1, BasicValue value2)
        {
            if (!value1.Equals(value2)) return BasicValue.Uninitialized_Value;
            return value1;
        }
    }
}