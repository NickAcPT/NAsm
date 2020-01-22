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

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     that keeps track of stack map frame changes between
    ///     <see cref="VisitFrame(int, int, object[], int, object[])" />
    ///     calls. This adapter must be used with the
    ///     <see cref="ClassReader.Expand_Frames" />
    ///     option. Each visit<i>X</i> instruction delegates to
    ///     the next visitor in the chain, if any, and then simulates the effect of this instruction on the
    ///     stack map frame, represented by
    ///     <see cref="locals" />
    ///     and
    ///     <see cref="stack" />
    ///     . The next visitor in the chain
    ///     can get the state of the stack map frame <i>before</i> each instruction by reading the value of
    ///     these fields in its visit<i>X</i> methods (this requires a reference to the AnalyzerAdapter that
    ///     is before it in the chain). If this adapter is used with a class that does not contain stack map
    ///     table attributes (i.e., pre Java 6 classes) then this adapter may not be able to compute the
    ///     stack map frame for each instruction. In this case no exception is thrown but the
    ///     <see cref="locals" />
    ///     and
    ///     <see cref="stack" />
    ///     fields will be null for these instructions.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class AnalyzerAdapter : MethodVisitor
    {
        /// <summary>The owner's class name.</summary>
        private readonly string owner;

        /// <summary>The labels that designate the next instruction to be visited.</summary>
        /// <remarks>
        ///     The labels that designate the next instruction to be visited. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private IList<Label> labels;

        /// <summary>The local variable slots for the current execution frame.</summary>
        /// <remarks>
        ///     The local variable slots for the current execution frame. Primitive types are represented by
        ///     <see cref="Opcodes.Top" />
        ///     ,
        ///     <see cref="Opcodes.Integer" />
        ///     ,
        ///     <see cref="Opcodes.Float" />
        ///     ,
        ///     <see cref="Opcodes.Long" />
        ///     ,
        ///     <see cref="double" />
        ///     ,
        ///     <see cref="Opcodes.Null" />
        ///     or
        ///     <see cref="Opcodes.Uninitialized_This" />
        ///     (long and
        ///     double are represented by two elements, the second one being TOP). Reference types are
        ///     represented by String objects (representing internal names), and uninitialized types by Label
        ///     objects (this label designates the NEW instruction that created this uninitialized value). This
        ///     field is
        ///     <literal>null</literal>
        ///     for unreachable instructions.
        /// </remarks>
        public IList<object> locals;

        /// <summary>The maximum number of local variables of this method.</summary>
        private int maxLocals;

        /// <summary>The maximum stack size of this method.</summary>
        private int maxStack;

        /// <summary>The operand stack slots for the current execution frame.</summary>
        /// <remarks>
        ///     The operand stack slots for the current execution frame. Primitive types are represented by
        ///     <see cref="Opcodes.Top" />
        ///     ,
        ///     <see cref="Opcodes.Integer" />
        ///     ,
        ///     <see cref="Opcodes.Float" />
        ///     ,
        ///     <see cref="Opcodes.Long" />
        ///     ,
        ///     <see cref="double" />
        ///     ,
        ///     <see cref="Opcodes.Null" />
        ///     or
        ///     <see cref="Opcodes.Uninitialized_This" />
        ///     (long and
        ///     double are represented by two elements, the second one being TOP). Reference types are
        ///     represented by String objects (representing internal names), and uninitialized types by Label
        ///     objects (this label designates the NEW instruction that created this uninitialized value). This
        ///     field is
        ///     <literal>null</literal>
        ///     for unreachable instructions.
        /// </remarks>
        public IList<object> stack;

        /// <summary>The uninitialized types in the current execution frame.</summary>
        /// <remarks>
        ///     The uninitialized types in the current execution frame. This map associates internal names to
        ///     Label objects. Each label designates a NEW instruction that created the currently uninitialized
        ///     types, and the associated internal name represents the NEW operand, i.e. the final, initialized
        ///     type value.
        /// </remarks>
        public IDictionary<object, object> uninitializedTypes;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnalyzerAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="AnalyzerAdapter(int, string, int, string, string, MethodVisitor)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="owner">the owner's class name.</param>
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public AnalyzerAdapter(string owner, ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor,
            MethodVisitor methodVisitor)
            : this(VisitorAsmApiVersion.Asm7, owner, access, name, descriptor, methodVisitor)
        {
            /* latest api = */
            if (GetType() != typeof(AnalyzerAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnalyzerAdapter" />
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
        /// <param name="owner">the owner's class name.</param>
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        protected internal AnalyzerAdapter(VisitorAsmApiVersion api, string owner, ObjectWeb.Asm.Enums.AccessFlags access, string name
            , string descriptor, MethodVisitor methodVisitor)
            : base(api, methodVisitor)
        {
            this.owner = owner;
            locals = new List<object>();
            stack = new List<object>();
            uninitializedTypes = new Dictionary<object, object>();
            if (access.HasNotFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Static))
            {
                if ("<init>".Equals(name))
                    locals.Add(OpcodesConstants.Uninitialized_This);
                else
                    locals.Add(owner);
            }

            foreach (var argumentType in Type.GetArgumentTypes(descriptor))
                switch (argumentType.GetSort())
                {
                    case Type.Boolean:
                    case Type.Char:
                    case Type.Byte:
                    case Type.Short:
                    case Type.Int:
                    {
                        locals.Add(OpcodesConstants.Integer);
                        break;
                    }

                    case Type.Float:
                    {
                        locals.Add(OpcodesConstants.Float);
                        break;
                    }

                    case Type.Long:
                    {
                        locals.Add(OpcodesConstants.Long);
                        locals.Add(OpcodesConstants.Top);
                        break;
                    }

                    case Type.Double:
                    {
                        locals.Add(OpcodesConstants.Double);
                        locals.Add(OpcodesConstants.Top);
                        break;
                    }

                    case Type.Array:
                    {
                        locals.Add(argumentType.GetDescriptor());
                        break;
                    }

                    case Type.Object:
                    {
                        locals.Add(argumentType.GetInternalName());
                        break;
                    }

                    default:
                    {
                        throw new AssertionError();
                    }
                }

            maxLocals = locals.Count;
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            if (type != OpcodesConstants.F_New)
                // Uncompressed frame.
                throw new ArgumentException(
                    "AnalyzerAdapter only accepts expanded frames (see ClassReader.EXPAND_FRAMES)"
                );
            base.VisitFrame(type, numLocal, local, numStack, stack);
            if (locals != null)
            {
                locals.Clear();
                this.stack.Clear();
            }
            else
            {
                locals = new List<object>();
                this.stack = new List<object>();
            }

            VisitFrameTypes(numLocal, local, locals);
            VisitFrameTypes(numStack, stack, this.stack);
            maxLocals = Math.Max(maxLocals, locals.Count);
            maxStack = Math.Max(maxStack, this.stack.Count);
        }

        private static void VisitFrameTypes(int numTypes, object[] frameTypes, IList<object
        > result)
        {
            for (var i = 0; i < numTypes; ++i)
            {
                var frameType = frameTypes[i];
                result.Add(frameType);
                if (frameType == (object) OpcodesConstants.Long || frameType == (object) OpcodesConstants.Double)
                    result.Add(OpcodesConstants.Top);
            }
        }

        public override void VisitInsn(int opcode)
        {
            base.VisitInsn(opcode);
            Execute(opcode, 0, null);
            if (opcode >= OpcodesConstants.Ireturn && opcode <= OpcodesConstants.Return ||
                opcode == OpcodesConstants.Athrow)
            {
                locals = null;
                stack = null;
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            base.VisitIntInsn(opcode, operand);
            Execute(opcode, operand, null);
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            base.VisitVarInsn(opcode, var);
            var isLongOrDouble = opcode == OpcodesConstants.Lload || opcode == OpcodesConstants
                                     .Dload || opcode == OpcodesConstants.Lstore || opcode == OpcodesConstants.Dstore;
            maxLocals = Math.Max(maxLocals, var + (isLongOrDouble ? 2 : 1));
            Execute(opcode, var, null);
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            if (opcode == OpcodesConstants.New)
            {
                if (labels == null)
                {
                    var label = new Label();
                    labels = new List<Label>(3);
                    labels.Add(label);
                    if (mv != null) mv.VisitLabel(label);
                }

                foreach (var label in labels) Collections.Put(uninitializedTypes, label, type);
            }

            base.VisitTypeInsn(opcode, type);
            Execute(opcode, 0, type);
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            base.VisitFieldInsn(opcode, owner, name, descriptor);
            Execute(opcode, 0, descriptor);
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name
            , string descriptor, bool isInterface)
        {
            if (api < VisitorAsmApiVersion.Asm5 && (opcodeAndSource & OpcodesConstants.Source_Deprecated
                ) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
            var opcode = opcodeAndSource & ~OpcodesConstants.Source_Mask;
            if (locals == null)
            {
                labels = null;
                return;
            }

            Pop(descriptor);
            if (opcode != OpcodesConstants.Invokestatic)
            {
                var value = Pop();
                if (opcode == OpcodesConstants.Invokespecial && name.Equals("<init>"))
                {
                    object initializedValue;
                    if (value == (object) OpcodesConstants.Uninitialized_This)
                        initializedValue = this.owner;
                    else
                        initializedValue = uninitializedTypes.GetOrNull(value);
                    for (var i = 0; i < locals.Count; ++i)
                        if (locals[i] == value)
                            locals[i] = initializedValue;
                    for (var i = 0; i < stack.Count; ++i)
                        if (stack[i] == value)
                            stack[i] = initializedValue;
                }
            }

            PushDescriptor(descriptor);
            labels = null;
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
            if (locals == null)
            {
                labels = null;
                return;
            }

            Pop(descriptor);
            PushDescriptor(descriptor);
            labels = null;
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            base.VisitJumpInsn(opcode, label);
            Execute(opcode, 0, null);
            if (opcode == OpcodesConstants.Goto)
            {
                locals = null;
                stack = null;
            }
        }

        public override void VisitLabel(Label label)
        {
            base.VisitLabel(label);
            if (labels == null) labels = new List<Label>(3);
            labels.Add(label);
        }

        public override void VisitLdcInsn(object value)
        {
            base.VisitLdcInsn(value);
            if (locals == null)
            {
                labels = null;
                return;
            }

            if (value is int)
            {
                Push(OpcodesConstants.Integer);
            }
            else if (value is long)
            {
                Push(OpcodesConstants.Long);
                Push(OpcodesConstants.Top);
            }
            else if (value is float)
            {
                Push(OpcodesConstants.Float);
            }
            else if (value is double)
            {
                Push(OpcodesConstants.Double);
                Push(OpcodesConstants.Top);
            }
            else if (value is string)
            {
                Push("java/lang/String");
            }
            else if (value is Type)
            {
                var sort = ((Type) value).GetSort();
                if (sort == Type.Object || sort == Type.Array)
                    Push("java/lang/Class");
                else if (sort == Type.Method)
                    Push("java/lang/invoke/MethodType");
                else
                    throw new ArgumentException();
            }
            else if (value is Handle)
            {
                Push("java/lang/invoke/MethodHandle");
            }
            else if (value is ConstantDynamic)
            {
                PushDescriptor(((ConstantDynamic) value).GetDescriptor());
            }
            else
            {
                throw new ArgumentException();
            }

            labels = null;
        }

        public override void VisitIincInsn(int var, int increment)
        {
            base.VisitIincInsn(var, increment);
            maxLocals = Math.Max(maxLocals, var + 1);
            Execute(OpcodesConstants.Iinc, var, null);
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            base.VisitTableSwitchInsn(min, max, dflt, labels);
            Execute(OpcodesConstants.Tableswitch, 0, null);
            locals = null;
            stack = null;
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            base.VisitLookupSwitchInsn(dflt, keys, labels);
            Execute(OpcodesConstants.Lookupswitch, 0, null);
            locals = null;
            stack = null;
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
            Execute(OpcodesConstants.Multianewarray, numDimensions, descriptor);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            var firstDescriptorChar = descriptor[0];
            maxLocals = Math.Max(maxLocals, index + (firstDescriptorChar == 'J' || firstDescriptorChar
                                                     == 'D'
                                                ? 2
                                                : 1));
            base.VisitLocalVariable(name, descriptor, signature, start, end, index);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            if (mv != null)
            {
                this.maxStack = Math.Max(this.maxStack, maxStack);
                this.maxLocals = Math.Max(this.maxLocals, maxLocals);
                mv.VisitMaxs(this.maxStack, this.maxLocals);
            }
        }

        // -----------------------------------------------------------------------------------------------
        private object Get(int local)
        {
            maxLocals = Math.Max(maxLocals, local + 1);
            return local < locals.Count ? locals[local] : OpcodesConstants.Top;
        }

        private void Set(int local, object type)
        {
            maxLocals = Math.Max(maxLocals, local + 1);
            while (local >= locals.Count) locals.Add(OpcodesConstants.Top);
            locals[local] = type;
        }

        private void Push(object type)
        {
            stack.Add(type);
            maxStack = Math.Max(maxStack, stack.Count);
        }

        private void PushDescriptor(string fieldOrMethodDescriptor)
        {
            var descriptor = fieldOrMethodDescriptor[0] == '('
                ? Type.GetReturnType(fieldOrMethodDescriptor
                ).GetDescriptor()
                : fieldOrMethodDescriptor;
            switch (descriptor[0])
            {
                case 'V':
                {
                    return;
                }

                case 'Z':
                case 'C':
                case 'B':
                case 'S':
                case 'I':
                {
                    Push(OpcodesConstants.Integer);
                    return;
                }

                case 'F':
                {
                    Push(OpcodesConstants.Float);
                    return;
                }

                case 'J':
                {
                    Push(OpcodesConstants.Long);
                    Push(OpcodesConstants.Top);
                    return;
                }

                case 'D':
                {
                    Push(OpcodesConstants.Double);
                    Push(OpcodesConstants.Top);
                    return;
                }

                case '[':
                {
                    Push(descriptor);
                    break;
                }

                case 'L':
                {
                    Push(Runtime.Substring(descriptor, 1, descriptor.Length - 1));
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        private object Pop()
        {
            return stack.RemoveAtReturningValue(stack.Count - 1);
        }

        private void Pop(int numSlots)
        {
            var size = stack.Count;
            var end = size - numSlots;
            for (var i = size - 1; i >= end; --i) stack.RemoveAtReturningValue(i);
        }

        private void Pop(string descriptor)
        {
            var firstDescriptorChar = descriptor[0];
            if (firstDescriptorChar == '(')
            {
                var numSlots = 0;
                var types = Type.GetArgumentTypes(descriptor);
                foreach (var type in types) numSlots += type.GetSize();
                Pop(numSlots);
            }
            else if (firstDescriptorChar == 'J' || firstDescriptorChar == 'D')
            {
                Pop(2);
            }
            else
            {
                Pop(1);
            }
        }

        private void Execute(int opcode, int intArg, string stringArg)
        {
            if (opcode == OpcodesConstants.Jsr || opcode == OpcodesConstants.Ret)
                throw new ArgumentException("JSR/RET are not supported");
            if (locals == null)
            {
                labels = null;
                return;
            }

            object value1;
            object value2;
            object value3;
            object t4;
            switch (opcode)
            {
                case OpcodesConstants.Nop:
                case OpcodesConstants.Ineg:
                case OpcodesConstants.Lneg:
                case OpcodesConstants.Fneg:
                case OpcodesConstants.Dneg:
                case OpcodesConstants.I2b:
                case OpcodesConstants.I2c:
                case OpcodesConstants.I2s:
                case OpcodesConstants.Goto:
                case OpcodesConstants.Return:
                {
                    break;
                }

                case OpcodesConstants.Aconst_Null:
                {
                    Push(OpcodesConstants.Null);
                    break;
                }

                case OpcodesConstants.Iconst_M1:
                case OpcodesConstants.Iconst_0:
                case OpcodesConstants.Iconst_1:
                case OpcodesConstants.Iconst_2:
                case OpcodesConstants.Iconst_3:
                case OpcodesConstants.Iconst_4:
                case OpcodesConstants.Iconst_5:
                case OpcodesConstants.Bipush:
                case OpcodesConstants.Sipush:
                {
                    Push(OpcodesConstants.Integer);
                    break;
                }

                case OpcodesConstants.Lconst_0:
                case OpcodesConstants.Lconst_1:
                {
                    Push(OpcodesConstants.Long);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Fconst_0:
                case OpcodesConstants.Fconst_1:
                case OpcodesConstants.Fconst_2:
                {
                    Push(OpcodesConstants.Float);
                    break;
                }

                case OpcodesConstants.Dconst_0:
                case OpcodesConstants.Dconst_1:
                {
                    Push(OpcodesConstants.Double);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Iload:
                case OpcodesConstants.Fload:
                case OpcodesConstants.Aload:
                {
                    Push(Get(intArg));
                    break;
                }

                case OpcodesConstants.Lload:
                case OpcodesConstants.Dload:
                {
                    Push(Get(intArg));
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Laload:
                case OpcodesConstants.D2l:
                {
                    Pop(2);
                    Push(OpcodesConstants.Long);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Daload:
                case OpcodesConstants.L2d:
                {
                    Pop(2);
                    Push(OpcodesConstants.Double);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Aaload:
                {
                    Pop(1);
                    value1 = Pop();
                    if (value1 is string)
                        PushDescriptor(Runtime.Substring((string) value1, 1));
                    else if (value1 == (object) OpcodesConstants.Null)
                        Push(value1);
                    else
                        Push("java/lang/Object");
                    break;
                }

                case OpcodesConstants.Istore:
                case OpcodesConstants.Fstore:
                case OpcodesConstants.Astore:
                {
                    value1 = Pop();
                    Set(intArg, value1);
                    if (intArg > 0)
                    {
                        value2 = Get(intArg - 1);
                        if (value2 == (object) OpcodesConstants.Long || value2 == (object) OpcodesConstants.Double)
                            Set(intArg - 1, OpcodesConstants.Top);
                    }

                    break;
                }

                case OpcodesConstants.Lstore:
                case OpcodesConstants.Dstore:
                {
                    Pop(1);
                    value1 = Pop();
                    Set(intArg, value1);
                    Set(intArg + 1, OpcodesConstants.Top);
                    if (intArg > 0)
                    {
                        value2 = Get(intArg - 1);
                        if (value2 == (object) OpcodesConstants.Long || value2 == (object) OpcodesConstants.Double)
                            Set(intArg - 1, OpcodesConstants.Top);
                    }

                    break;
                }

                case OpcodesConstants.Iastore:
                case OpcodesConstants.Bastore:
                case OpcodesConstants.Castore:
                case OpcodesConstants.Sastore:
                case OpcodesConstants.Fastore:
                case OpcodesConstants.Aastore:
                {
                    Pop(3);
                    break;
                }

                case OpcodesConstants.Lastore:
                case OpcodesConstants.Dastore:
                {
                    Pop(4);
                    break;
                }

                case OpcodesConstants.Pop:
                case OpcodesConstants.Ifeq:
                case OpcodesConstants.Ifne:
                case OpcodesConstants.Iflt:
                case OpcodesConstants.Ifge:
                case OpcodesConstants.Ifgt:
                case OpcodesConstants.Ifle:
                case OpcodesConstants.Ireturn:
                case OpcodesConstants.Freturn:
                case OpcodesConstants.Areturn:
                case OpcodesConstants.Tableswitch:
                case OpcodesConstants.Lookupswitch:
                case OpcodesConstants.Athrow:
                case OpcodesConstants.Monitorenter:
                case OpcodesConstants.Monitorexit:
                case OpcodesConstants.Ifnull:
                case OpcodesConstants.Ifnonnull:
                {
                    Pop(1);
                    break;
                }

                case OpcodesConstants.Pop2:
                case OpcodesConstants.If_Icmpeq:
                case OpcodesConstants.If_Icmpne:
                case OpcodesConstants.If_Icmplt:
                case OpcodesConstants.If_Icmpge:
                case OpcodesConstants.If_Icmpgt:
                case OpcodesConstants.If_Icmple:
                case OpcodesConstants.If_Acmpeq:
                case OpcodesConstants.If_Acmpne:
                case OpcodesConstants.Lreturn:
                case OpcodesConstants.Dreturn:
                {
                    Pop(2);
                    break;
                }

                case OpcodesConstants.Dup:
                {
                    value1 = Pop();
                    Push(value1);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Dup_X1:
                {
                    value1 = Pop();
                    value2 = Pop();
                    Push(value1);
                    Push(value2);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Dup_X2:
                {
                    value1 = Pop();
                    value2 = Pop();
                    value3 = Pop();
                    Push(value1);
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Dup2:
                {
                    value1 = Pop();
                    value2 = Pop();
                    Push(value2);
                    Push(value1);
                    Push(value2);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Dup2_X1:
                {
                    value1 = Pop();
                    value2 = Pop();
                    value3 = Pop();
                    Push(value2);
                    Push(value1);
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Dup2_X2:
                {
                    value1 = Pop();
                    value2 = Pop();
                    value3 = Pop();
                    t4 = Pop();
                    Push(value2);
                    Push(value1);
                    Push(t4);
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Swap:
                {
                    value1 = Pop();
                    value2 = Pop();
                    Push(value1);
                    Push(value2);
                    break;
                }

                case OpcodesConstants.Iaload:
                case OpcodesConstants.Baload:
                case OpcodesConstants.Caload:
                case OpcodesConstants.Saload:
                case OpcodesConstants.Iadd:
                case OpcodesConstants.Isub:
                case OpcodesConstants.Imul:
                case OpcodesConstants.Idiv:
                case OpcodesConstants.Irem:
                case OpcodesConstants.Iand:
                case OpcodesConstants.Ior:
                case OpcodesConstants.Ixor:
                case OpcodesConstants.Ishl:
                case OpcodesConstants.Ishr:
                case OpcodesConstants.Iushr:
                case OpcodesConstants.L2i:
                case OpcodesConstants.D2i:
                case OpcodesConstants.Fcmpl:
                case OpcodesConstants.Fcmpg:
                {
                    Pop(2);
                    Push(OpcodesConstants.Integer);
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
                {
                    Pop(4);
                    Push(OpcodesConstants.Long);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Faload:
                case OpcodesConstants.Fadd:
                case OpcodesConstants.Fsub:
                case OpcodesConstants.Fmul:
                case OpcodesConstants.Fdiv:
                case OpcodesConstants.Frem:
                case OpcodesConstants.L2f:
                case OpcodesConstants.D2f:
                {
                    Pop(2);
                    Push(OpcodesConstants.Float);
                    break;
                }

                case OpcodesConstants.Dadd:
                case OpcodesConstants.Dsub:
                case OpcodesConstants.Dmul:
                case OpcodesConstants.Ddiv:
                case OpcodesConstants.Drem:
                {
                    Pop(4);
                    Push(OpcodesConstants.Double);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Lshl:
                case OpcodesConstants.Lshr:
                case OpcodesConstants.Lushr:
                {
                    Pop(3);
                    Push(OpcodesConstants.Long);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.Iinc:
                {
                    Set(intArg, OpcodesConstants.Integer);
                    break;
                }

                case OpcodesConstants.I2l:
                case OpcodesConstants.F2l:
                {
                    Pop(1);
                    Push(OpcodesConstants.Long);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.I2f:
                {
                    Pop(1);
                    Push(OpcodesConstants.Float);
                    break;
                }

                case OpcodesConstants.I2d:
                case OpcodesConstants.F2d:
                {
                    Pop(1);
                    Push(OpcodesConstants.Double);
                    Push(OpcodesConstants.Top);
                    break;
                }

                case OpcodesConstants.F2i:
                case OpcodesConstants.Arraylength:
                case OpcodesConstants.Instanceof:
                {
                    Pop(1);
                    Push(OpcodesConstants.Integer);
                    break;
                }

                case OpcodesConstants.Lcmp:
                case OpcodesConstants.Dcmpl:
                case OpcodesConstants.Dcmpg:
                {
                    Pop(4);
                    Push(OpcodesConstants.Integer);
                    break;
                }

                case OpcodesConstants.Getstatic:
                {
                    PushDescriptor(stringArg);
                    break;
                }

                case OpcodesConstants.Putstatic:
                {
                    Pop(stringArg);
                    break;
                }

                case OpcodesConstants.Getfield:
                {
                    Pop(1);
                    PushDescriptor(stringArg);
                    break;
                }

                case OpcodesConstants.Putfield:
                {
                    Pop(stringArg);
                    Pop();
                    break;
                }

                case OpcodesConstants.New:
                {
                    Push(labels[0]);
                    break;
                }

                case OpcodesConstants.Newarray:
                {
                    Pop();
                    switch (intArg)
                    {
                        case OpcodesConstants.T_Boolean:
                        {
                            PushDescriptor("[Z");
                            break;
                        }

                        case OpcodesConstants.T_Char:
                        {
                            PushDescriptor("[C");
                            break;
                        }

                        case OpcodesConstants.T_Byte:
                        {
                            PushDescriptor("[B");
                            break;
                        }

                        case OpcodesConstants.T_Short:
                        {
                            PushDescriptor("[S");
                            break;
                        }

                        case OpcodesConstants.T_Int:
                        {
                            PushDescriptor("[I");
                            break;
                        }

                        case OpcodesConstants.T_Float:
                        {
                            PushDescriptor("[F");
                            break;
                        }

                        case OpcodesConstants.T_Double:
                        {
                            PushDescriptor("[D");
                            break;
                        }

                        case OpcodesConstants.T_Long:
                        {
                            PushDescriptor("[J");
                            break;
                        }

                        default:
                        {
                            throw new ArgumentException("Invalid array type " + intArg);
                        }
                    }

                    break;
                }

                case OpcodesConstants.Anewarray:
                {
                    Pop();
                    PushDescriptor("[" + Type.GetObjectType(stringArg));
                    break;
                }

                case OpcodesConstants.Checkcast:
                {
                    Pop();
                    PushDescriptor(Type.GetObjectType(stringArg).GetDescriptor());
                    break;
                }

                case OpcodesConstants.Multianewarray:
                {
                    Pop(intArg);
                    PushDescriptor(stringArg);
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid opcode " + opcode);
                }
            }

            labels = null;
        }
    }
}