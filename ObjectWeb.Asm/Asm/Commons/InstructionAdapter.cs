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
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     providing a more detailed API to generate and transform instructions.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class InstructionAdapter : MethodVisitor
    {
        /// <summary>The type of the java.lang.Object class.</summary>
        public static readonly Type Object_Type = Type.GetType("Ljava/lang/Object;");

        /// <summary>
        ///     Constructs a new
        ///     <see cref="InstructionAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="InstructionAdapter(int, MethodVisitor)" />
        ///     version.
        /// </summary>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public InstructionAdapter(MethodVisitor methodVisitor)
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, methodVisitor)
        {
            /* latest api = */
            if (GetType() != typeof(InstructionAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="InstructionAdapter" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of
        ///     <see cref="Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Opcodes.Asm6" />
        ///     or
        ///     <see cref="Opcodes.Asm7" />
        ///     .
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        protected internal InstructionAdapter(VisitorAsmApiVersion api, MethodVisitor methodVisitor)
            : base(api, methodVisitor)
        {
        }

        public override void VisitInsn(int opcode)
        {
            switch (opcode)
            {
                case OpcodesConstants.Nop:
                {
                    Nop();
                    break;
                }

                case OpcodesConstants.Aconst_Null:
                {
                    Aconst(null);
                    break;
                }

                case OpcodesConstants.Iconst_M1:
                case OpcodesConstants.Iconst_0:
                case OpcodesConstants.Iconst_1:
                case OpcodesConstants.Iconst_2:
                case OpcodesConstants.Iconst_3:
                case OpcodesConstants.Iconst_4:
                case OpcodesConstants.Iconst_5:
                {
                    Iconst(opcode - OpcodesConstants.Iconst_0);
                    break;
                }

                case OpcodesConstants.Lconst_0:
                case OpcodesConstants.Lconst_1:
                {
                    Lconst(opcode - OpcodesConstants.Lconst_0);
                    break;
                }

                case OpcodesConstants.Fconst_0:
                case OpcodesConstants.Fconst_1:
                case OpcodesConstants.Fconst_2:
                {
                    Fconst(opcode - OpcodesConstants.Fconst_0);
                    break;
                }

                case OpcodesConstants.Dconst_0:
                case OpcodesConstants.Dconst_1:
                {
                    Dconst(opcode - OpcodesConstants.Dconst_0);
                    break;
                }

                case OpcodesConstants.Iaload:
                {
                    Aload(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Laload:
                {
                    Aload(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Faload:
                {
                    Aload(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Daload:
                {
                    Aload(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Aaload:
                {
                    Aload(Object_Type);
                    break;
                }

                case OpcodesConstants.Baload:
                {
                    Aload(Type.Byte_Type);
                    break;
                }

                case OpcodesConstants.Caload:
                {
                    Aload(Type.Char_Type);
                    break;
                }

                case OpcodesConstants.Saload:
                {
                    Aload(Type.Short_Type);
                    break;
                }

                case OpcodesConstants.Iastore:
                {
                    Astore(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lastore:
                {
                    Astore(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fastore:
                {
                    Astore(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dastore:
                {
                    Astore(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Aastore:
                {
                    Astore(Object_Type);
                    break;
                }

                case OpcodesConstants.Bastore:
                {
                    Astore(Type.Byte_Type);
                    break;
                }

                case OpcodesConstants.Castore:
                {
                    Astore(Type.Char_Type);
                    break;
                }

                case OpcodesConstants.Sastore:
                {
                    Astore(Type.Short_Type);
                    break;
                }

                case OpcodesConstants.Pop:
                {
                    Pop();
                    break;
                }

                case OpcodesConstants.Pop2:
                {
                    Pop2();
                    break;
                }

                case OpcodesConstants.Dup:
                {
                    Dup();
                    break;
                }

                case OpcodesConstants.Dup_X1:
                {
                    DupX1();
                    break;
                }

                case OpcodesConstants.Dup_X2:
                {
                    DupX2();
                    break;
                }

                case OpcodesConstants.Dup2:
                {
                    Dup2();
                    break;
                }

                case OpcodesConstants.Dup2_X1:
                {
                    Dup2X1();
                    break;
                }

                case OpcodesConstants.Dup2_X2:
                {
                    Dup2X2();
                    break;
                }

                case OpcodesConstants.Swap:
                {
                    Swap();
                    break;
                }

                case OpcodesConstants.Iadd:
                {
                    Add(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Ladd:
                {
                    Add(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fadd:
                {
                    Add(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dadd:
                {
                    Add(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Isub:
                {
                    Sub(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lsub:
                {
                    Sub(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fsub:
                {
                    Sub(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dsub:
                {
                    Sub(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Imul:
                {
                    Mul(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lmul:
                {
                    Mul(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fmul:
                {
                    Mul(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dmul:
                {
                    Mul(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Idiv:
                {
                    Div(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Ldiv:
                {
                    Div(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fdiv:
                {
                    Div(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Ddiv:
                {
                    Div(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Irem:
                {
                    Rem(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lrem:
                {
                    Rem(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Frem:
                {
                    Rem(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Drem:
                {
                    Rem(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Ineg:
                {
                    Neg(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lneg:
                {
                    Neg(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fneg:
                {
                    Neg(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dneg:
                {
                    Neg(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Ishl:
                {
                    Shl(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lshl:
                {
                    Shl(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Ishr:
                {
                    Shr(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lshr:
                {
                    Shr(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Iushr:
                {
                    Ushr(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lushr:
                {
                    Ushr(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Iand:
                {
                    And(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Land:
                {
                    And(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Ior:
                {
                    Or(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lor:
                {
                    Or(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Ixor:
                {
                    Xor(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lxor:
                {
                    Xor(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.I2l:
                {
                    Cast(Type.Int_Type, Type.Long_Type);
                    break;
                }

                case OpcodesConstants.I2f:
                {
                    Cast(Type.Int_Type, Type.Float_Type);
                    break;
                }

                case OpcodesConstants.I2d:
                {
                    Cast(Type.Int_Type, Type.Double_Type);
                    break;
                }

                case OpcodesConstants.L2i:
                {
                    Cast(Type.Long_Type, Type.Int_Type);
                    break;
                }

                case OpcodesConstants.L2f:
                {
                    Cast(Type.Long_Type, Type.Float_Type);
                    break;
                }

                case OpcodesConstants.L2d:
                {
                    Cast(Type.Long_Type, Type.Double_Type);
                    break;
                }

                case OpcodesConstants.F2i:
                {
                    Cast(Type.Float_Type, Type.Int_Type);
                    break;
                }

                case OpcodesConstants.F2l:
                {
                    Cast(Type.Float_Type, Type.Long_Type);
                    break;
                }

                case OpcodesConstants.F2d:
                {
                    Cast(Type.Float_Type, Type.Double_Type);
                    break;
                }

                case OpcodesConstants.D2i:
                {
                    Cast(Type.Double_Type, Type.Int_Type);
                    break;
                }

                case OpcodesConstants.D2l:
                {
                    Cast(Type.Double_Type, Type.Long_Type);
                    break;
                }

                case OpcodesConstants.D2f:
                {
                    Cast(Type.Double_Type, Type.Float_Type);
                    break;
                }

                case OpcodesConstants.I2b:
                {
                    Cast(Type.Int_Type, Type.Byte_Type);
                    break;
                }

                case OpcodesConstants.I2c:
                {
                    Cast(Type.Int_Type, Type.Char_Type);
                    break;
                }

                case OpcodesConstants.I2s:
                {
                    Cast(Type.Int_Type, Type.Short_Type);
                    break;
                }

                case OpcodesConstants.Lcmp:
                {
                    Lcmp();
                    break;
                }

                case OpcodesConstants.Fcmpl:
                {
                    Cmpl(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Fcmpg:
                {
                    Cmpg(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dcmpl:
                {
                    Cmpl(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Dcmpg:
                {
                    Cmpg(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Ireturn:
                {
                    Areturn(Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lreturn:
                {
                    Areturn(Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Freturn:
                {
                    Areturn(Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dreturn:
                {
                    Areturn(Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Areturn:
                {
                    Areturn(Object_Type);
                    break;
                }

                case OpcodesConstants.Return:
                {
                    Areturn(Type.Void_Type);
                    break;
                }

                case OpcodesConstants.Arraylength:
                {
                    Arraylength();
                    break;
                }

                case OpcodesConstants.Athrow:
                {
                    Athrow();
                    break;
                }

                case OpcodesConstants.Monitorenter:
                {
                    Monitorenter();
                    break;
                }

                case OpcodesConstants.Monitorexit:
                {
                    Monitorexit();
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            switch (opcode)
            {
                case OpcodesConstants.Bipush:
                {
                    Iconst(operand);
                    break;
                }

                case OpcodesConstants.Sipush:
                {
                    Iconst(operand);
                    break;
                }

                case OpcodesConstants.Newarray:
                {
                    switch (operand)
                    {
                        case OpcodesConstants.T_Boolean:
                        {
                            Newarray(Type.Boolean_Type);
                            break;
                        }

                        case OpcodesConstants.T_Char:
                        {
                            Newarray(Type.Char_Type);
                            break;
                        }

                        case OpcodesConstants.T_Byte:
                        {
                            Newarray(Type.Byte_Type);
                            break;
                        }

                        case OpcodesConstants.T_Short:
                        {
                            Newarray(Type.Short_Type);
                            break;
                        }

                        case OpcodesConstants.T_Int:
                        {
                            Newarray(Type.Int_Type);
                            break;
                        }

                        case OpcodesConstants.T_Float:
                        {
                            Newarray(Type.Float_Type);
                            break;
                        }

                        case OpcodesConstants.T_Long:
                        {
                            Newarray(Type.Long_Type);
                            break;
                        }

                        case OpcodesConstants.T_Double:
                        {
                            Newarray(Type.Double_Type);
                            break;
                        }

                        default:
                        {
                            throw new ArgumentException();
                        }
                    }

                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            switch (opcode)
            {
                case OpcodesConstants.Iload:
                {
                    Load(var, Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lload:
                {
                    Load(var, Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fload:
                {
                    Load(var, Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dload:
                {
                    Load(var, Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Aload:
                {
                    Load(var, Object_Type);
                    break;
                }

                case OpcodesConstants.Istore:
                {
                    Store(var, Type.Int_Type);
                    break;
                }

                case OpcodesConstants.Lstore:
                {
                    Store(var, Type.Long_Type);
                    break;
                }

                case OpcodesConstants.Fstore:
                {
                    Store(var, Type.Float_Type);
                    break;
                }

                case OpcodesConstants.Dstore:
                {
                    Store(var, Type.Double_Type);
                    break;
                }

                case OpcodesConstants.Astore:
                {
                    Store(var, Object_Type);
                    break;
                }

                case OpcodesConstants.Ret:
                {
                    Ret(var);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            var objectType = Type.GetObjectType(type);
            switch (opcode)
            {
                case OpcodesConstants.New:
                {
                    Anew(objectType);
                    break;
                }

                case OpcodesConstants.Anewarray:
                {
                    Newarray(objectType);
                    break;
                }

                case OpcodesConstants.Checkcast:
                {
                    Checkcast(objectType);
                    break;
                }

                case OpcodesConstants.Instanceof:
                {
                    InstanceOf(objectType);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            switch (opcode)
            {
                case OpcodesConstants.Getstatic:
                {
                    Getstatic(owner, name, descriptor);
                    break;
                }

                case OpcodesConstants.Putstatic:
                {
                    Putstatic(owner, name, descriptor);
                    break;
                }

                case OpcodesConstants.Getfield:
                {
                    Getfield(owner, name, descriptor);
                    break;
                }

                case OpcodesConstants.Putfield:
                {
                    Putfield(owner, name, descriptor);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
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
            switch (opcode)
            {
                case OpcodesConstants.Invokespecial:
                {
                    Invokespecial(owner, name, descriptor, isInterface);
                    break;
                }

                case OpcodesConstants.Invokevirtual:
                {
                    Invokevirtual(owner, name, descriptor, isInterface);
                    break;
                }

                case OpcodesConstants.Invokestatic:
                {
                    Invokestatic(owner, name, descriptor, isInterface);
                    break;
                }

                case OpcodesConstants.Invokeinterface:
                {
                    Invokeinterface(owner, name, descriptor);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            Invokedynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            switch (opcode)
            {
                case OpcodesConstants.Ifeq:
                {
                    Ifeq(label);
                    break;
                }

                case OpcodesConstants.Ifne:
                {
                    Ifne(label);
                    break;
                }

                case OpcodesConstants.Iflt:
                {
                    Iflt(label);
                    break;
                }

                case OpcodesConstants.Ifge:
                {
                    Ifge(label);
                    break;
                }

                case OpcodesConstants.Ifgt:
                {
                    Ifgt(label);
                    break;
                }

                case OpcodesConstants.Ifle:
                {
                    Ifle(label);
                    break;
                }

                case OpcodesConstants.If_Icmpeq:
                {
                    Ificmpeq(label);
                    break;
                }

                case OpcodesConstants.If_Icmpne:
                {
                    Ificmpne(label);
                    break;
                }

                case OpcodesConstants.If_Icmplt:
                {
                    Ificmplt(label);
                    break;
                }

                case OpcodesConstants.If_Icmpge:
                {
                    Ificmpge(label);
                    break;
                }

                case OpcodesConstants.If_Icmpgt:
                {
                    Ificmpgt(label);
                    break;
                }

                case OpcodesConstants.If_Icmple:
                {
                    Ificmple(label);
                    break;
                }

                case OpcodesConstants.If_Acmpeq:
                {
                    Ifacmpeq(label);
                    break;
                }

                case OpcodesConstants.If_Acmpne:
                {
                    Ifacmpne(label);
                    break;
                }

                case OpcodesConstants.Goto:
                {
                    GoTo(label);
                    break;
                }

                case OpcodesConstants.Jsr:
                {
                    Jsr(label);
                    break;
                }

                case OpcodesConstants.Ifnull:
                {
                    Ifnull(label);
                    break;
                }

                case OpcodesConstants.Ifnonnull:
                {
                    Ifnonnull(label);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void VisitLabel(Label label)
        {
            Mark(label);
        }

        public override void VisitLdcInsn(object value)
        {
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5 && (value is Handle || value is Type && ((Type) value
                                                ).GetSort() == Type.Method))
                throw new NotSupportedException("This feature requires ASM5");
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7 && value is ConstantDynamic)
                throw new NotSupportedException("This feature requires ASM7");
            if (value is int)
                Iconst((int) value);
            else if (value is byte)
                Iconst((byte) value);
            else if (value is char)
                Iconst((char) value);
            else if (value is short)
                Iconst((short) value);
            else if (value is bool)
                Iconst((bool) value ? 1 : 0);
            else if (value is float)
                Fconst((float) value);
            else if (value is long)
                Lconst((long) value);
            else if (value is double)
                Dconst((double) value);
            else if (value is string)
                Aconst(value);
            else if (value is Type)
                Tconst((Type) value);
            else if (value is Handle)
                Hconst((Handle) value);
            else if (value is ConstantDynamic)
                Cconst((ConstantDynamic) value);
            else
                throw new ArgumentException();
        }

        public override void VisitIincInsn(int var, int increment)
        {
            Iinc(var, increment);
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            Tableswitch(min, max, dflt, labels);
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            Lookupswitch(dflt, keys, labels);
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            Multianewarray(descriptor, numDimensions);
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates a nop instruction.</summary>
        public virtual void Nop()
        {
            mv.VisitInsn(OpcodesConstants.Nop);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">
        ///     the constant to be pushed on the stack. This parameter must be an
        ///     <see cref="int" />
        ///     ,
        ///     a
        ///     <see cref="float" />
        ///     , a
        ///     <see cref="long" />
        ///     , a
        ///     <see cref="double" />
        ///     , a
        ///     <see cref="string" />
        ///     , a
        ///     <see cref="Type" />
        ///     of
        ///     OBJECT or ARRAY sort for
        ///     <c>.class</c>
        ///     constants, for classes whose version is 49, a
        ///     <see cref="Type" />
        ///     of METHOD sort for MethodType, a
        ///     <see cref="Handle" />
        ///     for MethodHandle constants,
        ///     for classes whose version is 51 or a
        ///     <see cref="ConstantDynamic" />
        ///     for a constant dynamic for
        ///     classes whose version is 55.
        /// </param>
        public virtual void Aconst(object value)
        {
            if (value == null)
                mv.VisitInsn(OpcodesConstants.Aconst_Null);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="intValue">the constant to be pushed on the stack.</param>
        public virtual void Iconst(int intValue)
        {
            if (intValue >= -1 && intValue <= 5)
                mv.VisitInsn(OpcodesConstants.Iconst_0 + intValue);
            else if (intValue >= byte.MinValue && intValue <= byte.MaxValue)
                mv.VisitIntInsn(OpcodesConstants.Bipush, intValue);
            else if (intValue >= short.MinValue && intValue <= short.MaxValue)
                mv.VisitIntInsn(OpcodesConstants.Sipush, intValue);
            else
                mv.VisitLdcInsn(intValue);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="longValue">the constant to be pushed on the stack.</param>
        public virtual void Lconst(long longValue)
        {
            if (longValue == 0L || longValue == 1L)
                mv.VisitInsn(OpcodesConstants.Lconst_0 + (int) longValue);
            else
                mv.VisitLdcInsn(longValue);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="floatValue">the constant to be pushed on the stack.</param>
        public virtual void Fconst(float floatValue)
        {
            var bits = Runtime.FloatToIntBits(floatValue);
            if (bits == 0L || bits == 0x3F800000 || bits == 0x40000000)
                // 0..2
                mv.VisitInsn(OpcodesConstants.Fconst_0 + (int) floatValue);
            else
                mv.VisitLdcInsn(floatValue);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="doubleValue">the constant to be pushed on the stack.</param>
        public virtual void Dconst(double doubleValue)
        {
            var bits = BitConverter.DoubleToInt64Bits(doubleValue);
            if (bits == 0L || bits == 0x3FF0000000000000L)
                // +0.0d and 1.0d
                mv.VisitInsn(OpcodesConstants.Dconst_0 + (int) doubleValue);
            else
                mv.VisitLdcInsn(doubleValue);
        }

        /// <summary>Generates the instruction to push the given type on the stack.</summary>
        /// <param name="type">the type to be pushed on the stack.</param>
        public virtual void Tconst(Type type)
        {
            mv.VisitLdcInsn(type);
        }

        /// <summary>Generates the instruction to push the given handle on the stack.</summary>
        /// <param name="handle">the handle to be pushed on the stack.</param>
        public virtual void Hconst(Handle handle)
        {
            mv.VisitLdcInsn(handle);
        }

        /// <summary>
        ///     Generates the instruction to push the given constant dynamic on the stack.
        /// </summary>
        /// <param name="constantDynamic">the constant dynamic to be pushed on the stack.</param>
        public virtual void Cconst(ConstantDynamic constantDynamic)
        {
            mv.VisitLdcInsn(constantDynamic);
        }

        public virtual void Load(int var, Type type)
        {
            mv.VisitVarInsn(type.GetOpcode(OpcodesConstants.Iload), var);
        }

        public virtual void Aload(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iaload));
        }

        public virtual void Store(int var, Type type)
        {
            mv.VisitVarInsn(type.GetOpcode(OpcodesConstants.Istore), var);
        }

        public virtual void Astore(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iastore));
        }

        public virtual void Pop()
        {
            mv.VisitInsn(OpcodesConstants.Pop);
        }

        public virtual void Pop2()
        {
            mv.VisitInsn(OpcodesConstants.Pop2);
        }

        public virtual void Dup()
        {
            mv.VisitInsn(OpcodesConstants.Dup);
        }

        public virtual void Dup2()
        {
            mv.VisitInsn(OpcodesConstants.Dup2);
        }

        public virtual void DupX1()
        {
            mv.VisitInsn(OpcodesConstants.Dup_X1);
        }

        public virtual void DupX2()
        {
            mv.VisitInsn(OpcodesConstants.Dup_X2);
        }

        public virtual void Dup2X1()
        {
            mv.VisitInsn(OpcodesConstants.Dup2_X1);
        }

        public virtual void Dup2X2()
        {
            mv.VisitInsn(OpcodesConstants.Dup2_X2);
        }

        public virtual void Swap()
        {
            mv.VisitInsn(OpcodesConstants.Swap);
        }

        public virtual void Add(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iadd));
        }

        public virtual void Sub(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Isub));
        }

        public virtual void Mul(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Imul));
        }

        public virtual void Div(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Idiv));
        }

        public virtual void Rem(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Irem));
        }

        public virtual void Neg(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Ineg));
        }

        public virtual void Shl(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Ishl));
        }

        public virtual void Shr(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Ishr));
        }

        public virtual void Ushr(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iushr));
        }

        public virtual void And(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iand));
        }

        public virtual void Or(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Ior));
        }

        public virtual void Xor(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Ixor));
        }

        public virtual void Iinc(int var, int increment)
        {
            mv.VisitIincInsn(var, increment);
        }

        /// <summary>
        ///     Generates the instruction to cast from the first given type to the other.
        /// </summary>
        /// <param name="from">a Type.</param>
        /// <param name="to">a Type.</param>
        public virtual void Cast(Type from, Type to)
        {
            Cast(mv, from, to);
        }

        /// <summary>
        ///     Generates the instruction to cast from the first given type to the other.
        /// </summary>
        /// <param name="methodVisitor">
        ///     the method visitor to use to generate the instruction.
        /// </param>
        /// <param name="from">a Type.</param>
        /// <param name="to">a Type.</param>
        internal static void Cast(MethodVisitor methodVisitor, Type from, Type to)
        {
            if (from != to)
            {
                if (from == Type.Double_Type)
                {
                    if (to == Type.Float_Type)
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.D2f);
                    }
                    else if (to == Type.Long_Type)
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.D2l);
                    }
                    else
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.D2i);
                        Cast(methodVisitor, Type.Int_Type, to);
                    }
                }
                else if (from == Type.Float_Type)
                {
                    if (to == Type.Double_Type)
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.F2d);
                    }
                    else if (to == Type.Long_Type)
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.F2l);
                    }
                    else
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.F2i);
                        Cast(methodVisitor, Type.Int_Type, to);
                    }
                }
                else if (from == Type.Long_Type)
                {
                    if (to == Type.Double_Type)
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.L2d);
                    }
                    else if (to == Type.Float_Type)
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.L2f);
                    }
                    else
                    {
                        methodVisitor.VisitInsn(OpcodesConstants.L2i);
                        Cast(methodVisitor, Type.Int_Type, to);
                    }
                }
                else if (to == Type.Byte_Type)
                {
                    methodVisitor.VisitInsn(OpcodesConstants.I2b);
                }
                else if (to == Type.Char_Type)
                {
                    methodVisitor.VisitInsn(OpcodesConstants.I2c);
                }
                else if (to == Type.Double_Type)
                {
                    methodVisitor.VisitInsn(OpcodesConstants.I2d);
                }
                else if (to == Type.Float_Type)
                {
                    methodVisitor.VisitInsn(OpcodesConstants.I2f);
                }
                else if (to == Type.Long_Type)
                {
                    methodVisitor.VisitInsn(OpcodesConstants.I2l);
                }
                else if (to == Type.Short_Type)
                {
                    methodVisitor.VisitInsn(OpcodesConstants.I2s);
                }
            }
        }

        public virtual void Lcmp()
        {
            mv.VisitInsn(OpcodesConstants.Lcmp);
        }

        public virtual void Cmpl(Type type)
        {
            mv.VisitInsn(type == Type.Float_Type ? OpcodesConstants.Fcmpl : OpcodesConstants.Dcmpl);
        }

        public virtual void Cmpg(Type type)
        {
            mv.VisitInsn(type == Type.Float_Type ? OpcodesConstants.Fcmpg : OpcodesConstants.Dcmpg);
        }

        public virtual void Ifeq(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifeq, label);
        }

        public virtual void Ifne(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifne, label);
        }

        public virtual void Iflt(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Iflt, label);
        }

        public virtual void Ifge(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifge, label);
        }

        public virtual void Ifgt(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifgt, label);
        }

        public virtual void Ifle(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifle, label);
        }

        public virtual void Ificmpeq(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Icmpeq, label);
        }

        public virtual void Ificmpne(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Icmpne, label);
        }

        public virtual void Ificmplt(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Icmplt, label);
        }

        public virtual void Ificmpge(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Icmpge, label);
        }

        public virtual void Ificmpgt(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Icmpgt, label);
        }

        public virtual void Ificmple(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Icmple, label);
        }

        public virtual void Ifacmpeq(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Acmpeq, label);
        }

        public virtual void Ifacmpne(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.If_Acmpne, label);
        }

        public virtual void GoTo(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Goto, label);
        }

        public virtual void Jsr(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Jsr, label);
        }

        public virtual void Ret(int var)
        {
            mv.VisitVarInsn(OpcodesConstants.Ret, var);
        }

        public virtual void Tableswitch(int min, int max, Label dflt, params Label[] labels
        )
        {
            mv.VisitTableSwitchInsn(min, max, dflt, labels);
        }

        public virtual void Lookupswitch(Label dflt, int[] keys, Label[] labels)
        {
            mv.VisitLookupSwitchInsn(dflt, keys, labels);
        }

        public virtual void Areturn(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Ireturn));
        }

        public virtual void Getstatic(string owner, string name, string descriptor)
        {
            mv.VisitFieldInsn(OpcodesConstants.Getstatic, owner, name, descriptor);
        }

        public virtual void Putstatic(string owner, string name, string descriptor)
        {
            mv.VisitFieldInsn(OpcodesConstants.Putstatic, owner, name, descriptor);
        }

        public virtual void Getfield(string owner, string name, string descriptor)
        {
            mv.VisitFieldInsn(OpcodesConstants.Getfield, owner, name, descriptor);
        }

        public virtual void Putfield(string owner, string name, string descriptor)
        {
            mv.VisitFieldInsn(OpcodesConstants.Putfield, owner, name, descriptor);
        }

        /// <summary>Deprecated.</summary>
        /// <param name="owner">the internal name of the method's owner class.</param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        [Obsolete(@"use Invokevirtual(string, string, string, bool) instead."
        )]
        public virtual void Invokevirtual(string owner, string name, string descriptor)
        {
            if (api >= ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5)
            {
                Invokevirtual(owner, name, descriptor, false);
                return;
            }

            mv.VisitMethodInsn(OpcodesConstants.Invokevirtual, owner, name, descriptor);
        }

        /// <summary>Generates the instruction to call the given virtual method.</summary>
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
        public virtual void Invokevirtual(string owner, string name, string descriptor, bool
            isInterface)
        {
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5)
            {
                if (isInterface) throw new ArgumentException("INVOKEVIRTUAL on interfaces require ASM 5");
                Invokevirtual(owner, name, descriptor);
                return;
            }

            mv.VisitMethodInsn(OpcodesConstants.Invokevirtual, owner, name, descriptor, isInterface
            );
        }

        /// <summary>Deprecated.</summary>
        /// <param name="owner">the internal name of the method's owner class.</param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        [Obsolete(@"use Invokespecial(string, string, string, bool) instead."
        )]
        public virtual void Invokespecial(string owner, string name, string descriptor)
        {
            if (api >= ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5)
            {
                Invokespecial(owner, name, descriptor, false);
                return;
            }

            mv.VisitMethodInsn(OpcodesConstants.Invokespecial, owner, name, descriptor, false
            );
        }

        /// <summary>Generates the instruction to call the given special method.</summary>
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
        public virtual void Invokespecial(string owner, string name, string descriptor, bool
            isInterface)
        {
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5)
            {
                if (isInterface) throw new ArgumentException("INVOKESPECIAL on interfaces require ASM 5");
                Invokespecial(owner, name, descriptor);
                return;
            }

            mv.VisitMethodInsn(OpcodesConstants.Invokespecial, owner, name, descriptor, isInterface
            );
        }

        /// <summary>Deprecated.</summary>
        /// <param name="owner">the internal name of the method's owner class.</param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        [Obsolete(@"use Invokestatic(string, string, string, bool) instead."
        )]
        public virtual void Invokestatic(string owner, string name, string descriptor)
        {
            if (api >= ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5)
            {
                Invokestatic(owner, name, descriptor, false);
                return;
            }

            mv.VisitMethodInsn(OpcodesConstants.Invokestatic, owner, name, descriptor, false);
        }

        /// <summary>Generates the instruction to call the given static method.</summary>
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
        public virtual void Invokestatic(string owner, string name, string descriptor, bool
            isInterface)
        {
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5)
            {
                if (isInterface) throw new ArgumentException("INVOKESTATIC on interfaces require ASM 5");
                Invokestatic(owner, name, descriptor);
                return;
            }

            mv.VisitMethodInsn(OpcodesConstants.Invokestatic, owner, name, descriptor, isInterface
            );
        }

        /// <summary>Generates the instruction to call the given interface method.</summary>
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
        public virtual void Invokeinterface(string owner, string name, string descriptor)
        {
            mv.VisitMethodInsn(OpcodesConstants.Invokeinterface, owner, name, descriptor, true
            );
        }

        /// <summary>Generates the instruction to call the given dynamic method.</summary>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="bootstrapMethodHandle">the bootstrap method.</param>
        /// <param name="bootstrapMethodArguments">
        ///     the bootstrap method constant arguments. Each argument must be
        ///     an
        ///     <see cref="int" />
        ///     ,
        ///     <see cref="float" />
        ///     ,
        ///     <see cref="long" />
        ///     ,
        ///     <see cref="double" />
        ///     ,
        ///     <see cref="string" />
        ///     ,
        ///     <see cref="Type" />
        ///     ,
        ///     <see cref="Handle" />
        ///     or
        ///     <see cref="ConstantDynamic" />
        ///     value. This method is allowed to modify
        ///     the content of the array so a caller should expect that this array may change.
        /// </param>
        public virtual void Invokedynamic(string name, string descriptor, Handle bootstrapMethodHandle
            , object[] bootstrapMethodArguments)
        {
            mv.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
        }

        public virtual void Anew(Type type)
        {
            mv.VisitTypeInsn(OpcodesConstants.New, type.GetInternalName());
        }

        /// <summary>
        ///     Generates the instruction to create and push on the stack an array of the given type.
        /// </summary>
        /// <param name="type">an array Type.</param>
        public virtual void Newarray(Type type)
        {
            Newarray(mv, type);
        }

        /// <summary>
        ///     Generates the instruction to create and push on the stack an array of the given type.
        /// </summary>
        /// <param name="methodVisitor">
        ///     the method visitor to use to generate the instruction.
        /// </param>
        /// <param name="type">an array Type.</param>
        internal static void Newarray(MethodVisitor methodVisitor, Type type)
        {
            int arrayType;
            switch (type.GetSort())
            {
                case Type.Boolean:
                {
                    arrayType = OpcodesConstants.T_Boolean;
                    break;
                }

                case Type.Char:
                {
                    arrayType = OpcodesConstants.T_Char;
                    break;
                }

                case Type.Byte:
                {
                    arrayType = OpcodesConstants.T_Byte;
                    break;
                }

                case Type.Short:
                {
                    arrayType = OpcodesConstants.T_Short;
                    break;
                }

                case Type.Int:
                {
                    arrayType = OpcodesConstants.T_Int;
                    break;
                }

                case Type.Float:
                {
                    arrayType = OpcodesConstants.T_Float;
                    break;
                }

                case Type.Long:
                {
                    arrayType = OpcodesConstants.T_Long;
                    break;
                }

                case Type.Double:
                {
                    arrayType = OpcodesConstants.T_Double;
                    break;
                }

                default:
                {
                    methodVisitor.VisitTypeInsn(OpcodesConstants.Anewarray, type.GetInternalName());
                    return;
                }
            }

            methodVisitor.VisitIntInsn(OpcodesConstants.Newarray, arrayType);
        }

        public virtual void Arraylength()
        {
            mv.VisitInsn(OpcodesConstants.Arraylength);
        }

        public virtual void Athrow()
        {
            mv.VisitInsn(OpcodesConstants.Athrow);
        }

        public virtual void Checkcast(Type type)
        {
            mv.VisitTypeInsn(OpcodesConstants.Checkcast, type.GetInternalName());
        }

        public virtual void InstanceOf(Type type)
        {
            mv.VisitTypeInsn(OpcodesConstants.Instanceof, type.GetInternalName());
        }

        public virtual void Monitorenter()
        {
            mv.VisitInsn(OpcodesConstants.Monitorenter);
        }

        public virtual void Monitorexit()
        {
            mv.VisitInsn(OpcodesConstants.Monitorexit);
        }

        public virtual void Multianewarray(string descriptor, int numDimensions)
        {
            mv.VisitMultiANewArrayInsn(descriptor, numDimensions);
        }

        public virtual void Ifnull(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifnull, label);
        }

        public virtual void Ifnonnull(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifnonnull, label);
        }

        public virtual void Mark(Label label)
        {
            mv.VisitLabel(label);
        }
    }
}