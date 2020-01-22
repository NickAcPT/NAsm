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

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     to insert before, after and around advices in methods and constructors.
    ///     For constructors, the code keeps track of the elements on the stack in order to detect when the
    ///     super class constructor is called (note that there can be multiple such calls in different
    ///     branches).
    ///     <c>onMethodEnter</c>
    ///     is called after each super class constructor call, because the
    ///     object cannot be used before it is properly initialized.
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    /// <author>Eric Bruneton</author>
    public abstract class AdviceAdapter : GeneratorAdapter, Opcodes
    {
        /// <summary>Prefix of the error message when invalid opcodes are found.</summary>
        private const string Invalid_Opcode = "Invalid opcode ";

        /// <summary>The "uninitialized this" value.</summary>
        private static readonly object Uninitialized_This = new object();

        /// <summary>Any value other than "uninitialized this".</summary>
        private static readonly object Other = new object();

        /// <summary>Whether the visited method is a constructor.</summary>
        private readonly bool isConstructor;

        /// <summary>
        ///     The stack map frames corresponding to the labels of the forward jumps made *before* the super
        ///     class constructor has been called (note that the Java Virtual Machine forbids backward jumps
        ///     before the super class constructor is called).
        /// </summary>
        /// <remarks>
        ///     The stack map frames corresponding to the labels of the forward jumps made *before* the super
        ///     class constructor has been called (note that the Java Virtual Machine forbids backward jumps
        ///     before the super class constructor is called). Note that by definition (cf. the 'before'), when
        ///     we reach a label from this map,
        ///     <see cref="superClassConstructorCalled" />
        ///     must be reset to false.
        ///     This field is only maintained for constructors.
        /// </remarks>
        private IDictionary<Label, IList<object>> forwardJumpStackFrames;

        /// <summary>The access flags of the visited method.</summary>
        protected internal AccessFlags methodAccess;

        /// <summary>The descriptor of the visited method.</summary>
        protected internal string methodDesc;

        /// <summary>
        ///     The values on the current execution stack frame (long and double are represented by two
        ///     elements).
        /// </summary>
        /// <remarks>
        ///     The values on the current execution stack frame (long and double are represented by two
        ///     elements). Each value is either
        ///     <see cref="Uninitialized_This" />
        ///     (for the uninitialized this value),
        ///     or
        ///     <see cref="Other" />
        ///     (for any other value). This field is only maintained for constructors, in
        ///     branches where the super class constructor has not been called yet.
        /// </remarks>
        private IList<object> stackFrame;

        /// <summary>
        ///     Whether the super class constructor has been called (if the visited method is a constructor),
        ///     at the current instruction.
        /// </summary>
        /// <remarks>
        ///     Whether the super class constructor has been called (if the visited method is a constructor),
        ///     at the current instruction. There can be multiple call sites to the super constructor (e.g. for
        ///     Java code such as
        ///     <c>super(expr ? value1 : value2);</c>
        ///     ), in different branches. When scanning
        ///     the bytecode linearly, we can move from one branch where the super constructor has been called
        ///     to another where it has not been called yet. Therefore, this value can change from false to
        ///     true, and vice-versa.
        /// </remarks>
        private bool superClassConstructorCalled;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AdviceAdapter" />
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
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type">Type</see>
        ///     ).
        /// </param>
        protected internal AdviceAdapter(VisitorAsmApiVersion api, MethodVisitor methodVisitor, AccessFlags access
            , string name, string descriptor)
            : base(api, methodVisitor, access, name, descriptor)
        {
            methodAccess = access;
            methodDesc = descriptor;
            isConstructor = "<init>".Equals(name);
        }

        public override void VisitCode()
        {
            base.VisitCode();
            if (isConstructor)
            {
                stackFrame = new List<object>();
                forwardJumpStackFrames = new Dictionary<Label, IList<object>>();
            }
            else
            {
                OnMethodEnter();
            }
        }

        public override void VisitLabel(Label label)
        {
            base.VisitLabel(label);
            if (isConstructor && forwardJumpStackFrames != null)
            {
                var labelStackFrame = forwardJumpStackFrames.GetOrNull(label);
                if (labelStackFrame != null)
                {
                    stackFrame = labelStackFrame;
                    superClassConstructorCalled = false;
                    Collections.Remove(forwardJumpStackFrames, label);
                }
            }
        }

        public override void VisitInsn(int opcode)
        {
            if (isConstructor && !superClassConstructorCalled)
            {
                int stackSize;
                switch (opcode)
                {
                    case OpcodesConstants.Ireturn:
                    case OpcodesConstants.Freturn:
                    case OpcodesConstants.Areturn:
                    case OpcodesConstants.Lreturn:
                    case OpcodesConstants.Dreturn:
                    {
                        throw new ArgumentException("Invalid return in constructor");
                    }

                    case OpcodesConstants.Return:
                    {
                        // empty stack
                        OnMethodExit(opcode);
                        break;
                    }

                    case OpcodesConstants.Athrow:
                    {
                        // 1 before n/a after
                        PopValue();
                        OnMethodExit(opcode);
                        break;
                    }

                    case OpcodesConstants.Nop:
                    case OpcodesConstants.Laload:
                    case OpcodesConstants.Daload:
                    case OpcodesConstants.Lneg:
                    case OpcodesConstants.Dneg:
                    case OpcodesConstants.Fneg:
                    case OpcodesConstants.Ineg:
                    case OpcodesConstants.L2d:
                    case OpcodesConstants.D2l:
                    case OpcodesConstants.F2i:
                    case OpcodesConstants.I2b:
                    case OpcodesConstants.I2c:
                    case OpcodesConstants.I2s:
                    case OpcodesConstants.I2f:
                    case OpcodesConstants.Arraylength:
                    {
                        // remove 2 add 2
                        // remove 2 add 2
                        break;
                    }

                    case OpcodesConstants.Aconst_Null:
                    case OpcodesConstants.Iconst_M1:
                    case OpcodesConstants.Iconst_0:
                    case OpcodesConstants.Iconst_1:
                    case OpcodesConstants.Iconst_2:
                    case OpcodesConstants.Iconst_3:
                    case OpcodesConstants.Iconst_4:
                    case OpcodesConstants.Iconst_5:
                    case OpcodesConstants.Fconst_0:
                    case OpcodesConstants.Fconst_1:
                    case OpcodesConstants.Fconst_2:
                    case OpcodesConstants.F2l:
                    case OpcodesConstants.F2d:
                    case OpcodesConstants.I2l:
                    case OpcodesConstants.I2d:
                    {
                        // 1 before 2 after
                        PushValue(Other);
                        break;
                    }

                    case OpcodesConstants.Lconst_0:
                    case OpcodesConstants.Lconst_1:
                    case OpcodesConstants.Dconst_0:
                    case OpcodesConstants.Dconst_1:
                    {
                        PushValue(Other);
                        PushValue(Other);
                        break;
                    }

                    case OpcodesConstants.Iaload:
                    case OpcodesConstants.Faload:
                    case OpcodesConstants.Aaload:
                    case OpcodesConstants.Baload:
                    case OpcodesConstants.Caload:
                    case OpcodesConstants.Saload:
                    case OpcodesConstants.Pop:
                    case OpcodesConstants.Iadd:
                    case OpcodesConstants.Fadd:
                    case OpcodesConstants.Isub:
                    case OpcodesConstants.Lshl:
                    case OpcodesConstants.Lshr:
                    case OpcodesConstants.Lushr:
                    case OpcodesConstants.L2i:
                    case OpcodesConstants.L2f:
                    case OpcodesConstants.D2i:
                    case OpcodesConstants.D2f:
                    case OpcodesConstants.Fsub:
                    case OpcodesConstants.Fmul:
                    case OpcodesConstants.Fdiv:
                    case OpcodesConstants.Frem:
                    case OpcodesConstants.Fcmpl:
                    case OpcodesConstants.Fcmpg:
                    case OpcodesConstants.Imul:
                    case OpcodesConstants.Idiv:
                    case OpcodesConstants.Irem:
                    case OpcodesConstants.Ishl:
                    case OpcodesConstants.Ishr:
                    case OpcodesConstants.Iushr:
                    case OpcodesConstants.Iand:
                    case OpcodesConstants.Ior:
                    case OpcodesConstants.Ixor:
                    case OpcodesConstants.Monitorenter:
                    case OpcodesConstants.Monitorexit:
                    {
                        // remove 2 add 1
                        // remove 2 add 1
                        // remove 2 add 1
                        // remove 2 add 1
                        // remove 2 add 1
                        // remove 2 add 1
                        // 3 before 2 after
                        // 3 before 2 after
                        // 3 before 2 after
                        // 2 before 1 after
                        // 2 before 1 after
                        // 2 before 1 after
                        // 2 before 1 after
                        // 2 before 1 after
                        // 2 before 1 after
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Pop2:
                    case OpcodesConstants.Lsub:
                    case OpcodesConstants.Lmul:
                    case OpcodesConstants.Ldiv:
                    case OpcodesConstants.Lrem:
                    case OpcodesConstants.Ladd:
                    case OpcodesConstants.Land:
                    case OpcodesConstants.Lor:
                    case OpcodesConstants.Lxor:
                    case OpcodesConstants.Dadd:
                    case OpcodesConstants.Dmul:
                    case OpcodesConstants.Dsub:
                    case OpcodesConstants.Ddiv:
                    case OpcodesConstants.Drem:
                    {
                        PopValue();
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Iastore:
                    case OpcodesConstants.Fastore:
                    case OpcodesConstants.Aastore:
                    case OpcodesConstants.Bastore:
                    case OpcodesConstants.Castore:
                    case OpcodesConstants.Sastore:
                    case OpcodesConstants.Lcmp:
                    case OpcodesConstants.Dcmpl:
                    case OpcodesConstants.Dcmpg:
                    {
                        // 4 before 1 after
                        PopValue();
                        PopValue();
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Lastore:
                    case OpcodesConstants.Dastore:
                    {
                        PopValue();
                        PopValue();
                        PopValue();
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Dup:
                    {
                        PushValue(PeekValue());
                        break;
                    }

                    case OpcodesConstants.Dup_X1:
                    {
                        stackSize = stackFrame.Count;
                        stackFrame.Add(stackSize - 2, stackFrame[stackSize - 1]);
                        break;
                    }

                    case OpcodesConstants.Dup_X2:
                    {
                        stackSize = stackFrame.Count;
                        stackFrame.Add(stackSize - 3, stackFrame[stackSize - 1]);
                        break;
                    }

                    case OpcodesConstants.Dup2:
                    {
                        stackSize = stackFrame.Count;
                        stackFrame.Add(stackSize - 2, stackFrame[stackSize - 1]);
                        stackFrame.Add(stackSize - 2, stackFrame[stackSize - 1]);
                        break;
                    }

                    case OpcodesConstants.Dup2_X1:
                    {
                        stackSize = stackFrame.Count;
                        stackFrame.Add(stackSize - 3, stackFrame[stackSize - 1]);
                        stackFrame.Add(stackSize - 3, stackFrame[stackSize - 1]);
                        break;
                    }

                    case OpcodesConstants.Dup2_X2:
                    {
                        stackSize = stackFrame.Count;
                        stackFrame.Add(stackSize - 4, stackFrame[stackSize - 1]);
                        stackFrame.Add(stackSize - 4, stackFrame[stackSize - 1]);
                        break;
                    }

                    case OpcodesConstants.Swap:
                    {
                        stackSize = stackFrame.Count;
                        stackFrame.Add(stackSize - 2, stackFrame[stackSize - 1]);
                        stackFrame.RemoveAtReturningValue(stackSize);
                        break;
                    }

                    default:
                    {
                        throw new ArgumentException(Invalid_Opcode + opcode);
                    }
                }
            }
            else
            {
                switch (opcode)
                {
                    case OpcodesConstants.Return:
                    case OpcodesConstants.Ireturn:
                    case OpcodesConstants.Freturn:
                    case OpcodesConstants.Areturn:
                    case OpcodesConstants.Lreturn:
                    case OpcodesConstants.Dreturn:
                    case OpcodesConstants.Athrow:
                    {
                        OnMethodExit(opcode);
                        break;
                    }
                }
            }

            base.VisitInsn(opcode);
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            base.VisitVarInsn(opcode, var);
            if (isConstructor && !superClassConstructorCalled)
                switch (opcode)
                {
                    case OpcodesConstants.Iload:
                    case OpcodesConstants.Fload:
                    {
                        PushValue(Other);
                        break;
                    }

                    case OpcodesConstants.Lload:
                    case OpcodesConstants.Dload:
                    {
                        PushValue(Other);
                        PushValue(Other);
                        break;
                    }

                    case OpcodesConstants.Aload:
                    {
                        PushValue(var == 0 ? Uninitialized_This : Other);
                        break;
                    }

                    case OpcodesConstants.Astore:
                    case OpcodesConstants.Istore:
                    case OpcodesConstants.Fstore:
                    {
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Lstore:
                    case OpcodesConstants.Dstore:
                    {
                        PopValue();
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Ret:
                    {
                        break;
                    }

                    default:
                    {
                        throw new ArgumentException(Invalid_Opcode + opcode);
                    }
                }
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            base.VisitFieldInsn(opcode, owner, name, descriptor);
            if (isConstructor && !superClassConstructorCalled)
            {
                var firstDescriptorChar = descriptor[0];
                var longOrDouble = firstDescriptorChar == 'J' || firstDescriptorChar == 'D';
                switch (opcode)
                {
                    case OpcodesConstants.Getstatic:
                    {
                        PushValue(Other);
                        if (longOrDouble) PushValue(Other);
                        break;
                    }

                    case OpcodesConstants.Putstatic:
                    {
                        PopValue();
                        if (longOrDouble) PopValue();
                        break;
                    }

                    case OpcodesConstants.Putfield:
                    {
                        PopValue();
                        PopValue();
                        if (longOrDouble) PopValue();
                        break;
                    }

                    case OpcodesConstants.Getfield:
                    {
                        if (longOrDouble) PushValue(Other);
                        break;
                    }

                    default:
                    {
                        throw new ArgumentException(Invalid_Opcode + opcode);
                    }
                }
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            base.VisitIntInsn(opcode, operand);
            if (isConstructor && !superClassConstructorCalled && opcode != OpcodesConstants.Newarray) PushValue(Other);
        }

        public override void VisitLdcInsn(object value)
        {
            base.VisitLdcInsn(value);
            if (isConstructor && !superClassConstructorCalled)
            {
                PushValue(Other);
                if (value is double || value is long || value is ConstantDynamic && ((ConstantDynamic
                        ) value).GetSize() == 2)
                    PushValue(Other);
            }
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
            if (isConstructor && !superClassConstructorCalled)
            {
                for (var i = 0; i < numDimensions; i++) PopValue();
                PushValue(Other);
            }
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            base.VisitTypeInsn(opcode, type);
            // ANEWARRAY, CHECKCAST or INSTANCEOF don't change stack.
            if (isConstructor && !superClassConstructorCalled && opcode == OpcodesConstants.New) PushValue(Other);
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
            DoVisitMethodInsn(opcode, descriptor);
        }

        private void DoVisitMethodInsn(int opcode, string descriptor)
        {
            if (isConstructor && !superClassConstructorCalled)
            {
                foreach (var argumentType in Type.GetArgumentTypes(descriptor))
                {
                    PopValue();
                    if (argumentType.GetSize() == 2) PopValue();
                }

                switch (opcode)
                {
                    case OpcodesConstants.Invokeinterface:
                    case OpcodesConstants.Invokevirtual:
                    {
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Invokespecial:
                    {
                        var value = PopValue();
                        if (value == Uninitialized_This && !superClassConstructorCalled)
                        {
                            superClassConstructorCalled = true;
                            OnMethodEnter();
                        }

                        break;
                    }
                }

                var returnType = Type.GetReturnType(descriptor);
                if (returnType != Type.Void_Type)
                {
                    PushValue(Other);
                    if (returnType.GetSize() == 2) PushValue(Other);
                }
            }
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
            DoVisitMethodInsn(OpcodesConstants.Invokedynamic, descriptor);
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            base.VisitJumpInsn(opcode, label);
            if (isConstructor && !superClassConstructorCalled)
            {
                switch (opcode)
                {
                    case OpcodesConstants.Ifeq:
                    case OpcodesConstants.Ifne:
                    case OpcodesConstants.Iflt:
                    case OpcodesConstants.Ifge:
                    case OpcodesConstants.Ifgt:
                    case OpcodesConstants.Ifle:
                    case OpcodesConstants.Ifnull:
                    case OpcodesConstants.Ifnonnull:
                    {
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.If_Icmpeq:
                    case OpcodesConstants.If_Icmpne:
                    case OpcodesConstants.If_Icmplt:
                    case OpcodesConstants.If_Icmpge:
                    case OpcodesConstants.If_Icmpgt:
                    case OpcodesConstants.If_Icmple:
                    case OpcodesConstants.If_Acmpeq:
                    case OpcodesConstants.If_Acmpne:
                    {
                        PopValue();
                        PopValue();
                        break;
                    }

                    case OpcodesConstants.Jsr:
                    {
                        PushValue(Other);
                        break;
                    }
                }

                AddForwardJump(label);
            }
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            base.VisitLookupSwitchInsn(dflt, keys, labels);
            if (isConstructor && !superClassConstructorCalled)
            {
                PopValue();
                AddForwardJumps(dflt, labels);
            }
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            base.VisitTableSwitchInsn(min, max, dflt, labels);
            if (isConstructor && !superClassConstructorCalled)
            {
                PopValue();
                AddForwardJumps(dflt, labels);
            }
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type)
        {
            base.VisitTryCatchBlock(start, end, handler, type);
            // By definition of 'forwardJumpStackFrames', 'handler' should be pushed only if there is an
            // instruction between 'start' and 'end' at which the super class constructor is not yet
            // called. Unfortunately, try catch blocks must be visited before their labels, so we have no
            // way to know this at this point. Instead, we suppose that the super class constructor has not
            // been called at the start of *any* exception handler. If this is wrong, normally there should
            // not be a second super class constructor call in the exception handler (an object can't be
            // initialized twice), so this is not issue (in the sense that there is no risk to emit a wrong
            // 'onMethodEnter').
            if (isConstructor && !forwardJumpStackFrames.ContainsKey(handler))
            {
                IList<object> handlerStackFrame = new List<object>();
                handlerStackFrame.Add(Other);
                Collections.Put(forwardJumpStackFrames, handler, handlerStackFrame);
            }
        }

        private void AddForwardJumps(Label dflt, Label[] labels)
        {
            AddForwardJump(dflt);
            foreach (var label in labels) AddForwardJump(label);
        }

        private void AddForwardJump(Label label)
        {
            if (forwardJumpStackFrames.ContainsKey(label)) return;
            Collections.Put(forwardJumpStackFrames, label, new List<object>(stackFrame
            ));
        }

        private object PopValue()
        {
            return stackFrame.RemoveAtReturningValue(stackFrame.Count - 1);
        }

        private object PeekValue()
        {
            return stackFrame[stackFrame.Count - 1];
        }

        private void PushValue(object value)
        {
            stackFrame.Add(value);
        }

        /// <summary>Generates the "before" advice for the visited method.</summary>
        /// <remarks>
        ///     Generates the "before" advice for the visited method. The default implementation of this method
        ///     does nothing. Subclasses can use or change all the local variables, but should not change state
        ///     of the stack. This method is called at the beginning of the method or after super class
        ///     constructor has been called (in constructors).
        /// </remarks>
        protected internal virtual void OnMethodEnter()
        {
        }

        /// <summary>Generates the "after" advice for the visited method.</summary>
        /// <remarks>
        ///     Generates the "after" advice for the visited method. The default implementation of this method
        ///     does nothing. Subclasses can use or change all the local variables, but should not change state
        ///     of the stack. This method is called at the end of the method, just before return and athrow
        ///     instructions. The top element on the stack contains the return value or the exception instance.
        ///     For example:
        ///     <pre>
        ///         public void onMethodExit(final int opcode) {
        ///         if (opcode == RETURN) {
        ///         visitInsn(ACONST_NULL);
        ///         } else if (opcode == ARETURN || opcode == ATHROW) {
        ///         dup();
        ///         } else {
        ///         if (opcode == LRETURN || opcode == DRETURN) {
        ///         dup2();
        ///         } else {
        ///         dup();
        ///         }
        ///         box(Type.getReturnType(this.methodDesc));
        ///         }
        ///         visitIntInsn(SIPUSH, opcode);
        ///         visitMethodInsn(INVOKESTATIC, owner, "onExit", "(Ljava/lang/Object;I)V");
        ///         }
        ///         // An actual call back method.
        ///         public static void onExit(final Object exitValue, final int opcode) {
        ///         ...
        ///         }
        ///     </pre>
        /// </remarks>
        /// <param name="opcode">
        ///     one of
        ///     <see cref="Opcodes.Return" />
        ///     ,
        ///     <see cref="Opcodes.Ireturn" />
        ///     ,
        ///     <see cref="Opcodes.Freturn" />
        ///     ,
        ///     <see cref="Opcodes.Areturn" />
        ///     ,
        ///     <see cref="Opcodes.Lreturn" />
        ///     ,
        ///     <see cref="Opcodes.Dreturn" />
        ///     or
        ///     <see cref="Opcodes.Athrow" />
        ///     .
        /// </param>
        protected internal virtual void OnMethodExit(int opcode)
        {
        }
    }
}