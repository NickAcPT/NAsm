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
using System.Text;

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>A symbolic execution stack frame.</summary>
    /// <remarks>
    ///     A symbolic execution stack frame. A stack frame contains a set of local variable slots, and an
    ///     operand stack. Warning: long and double values are represented with <i>two</i> slots in local
    ///     variables, and with <i>one</i> slot in the operand stack.
    /// </remarks>
    /// <?/>
    /// <author>Eric Bruneton</author>
    public class Frame<V>
        where V : Value
    {
        /// <summary>The number of local variables of this frame.</summary>
        private readonly int numLocals;

        /// <summary>The local variables and the operand stack of this frame.</summary>
        /// <remarks>
        ///     The local variables and the operand stack of this frame. The first
        ///     <see cref="Frame{V}.numLocals" />
        ///     elements
        ///     correspond to the local variables. The following
        ///     <see cref="Frame{V}.numStack" />
        ///     elements correspond to the
        ///     operand stack.
        /// </remarks>
        private readonly V[] values;

        /// <summary>The number of elements in the operand stack.</summary>
        private int numStack;

        /// <summary>
        ///     The expected return type of the analyzed method, or
        ///     <literal>null</literal>
        ///     if the method returns void.
        /// </summary>
        private V returnValue;

        /// <summary>Constructs a new frame with the given size.</summary>
        /// <param name="numLocals">the maximum number of local variables of the frame.</param>
        /// <param name="numStack">the maximum stack size of the frame.</param>
        public Frame(int numLocals, int numStack)
        {
            values = new V[numLocals + numStack];
            this.numLocals = numLocals;
        }

        /// <summary>Constructs a copy of the given Frame.</summary>
        /// <param name="frame">a frame.</param>
        public Frame(Frame<V> frame)
            : this(frame.numLocals, frame.values.Length - frame.numLocals)
        {
            Init(frame);
        }

        // NOPMD(ConstructorCallsOverridableMethod): can't fix for backward compatibility.
        /// <summary>Copies the state of the given frame into this frame.</summary>
        /// <param name="frame">a frame.</param>
        /// <returns>this frame.</returns>
        public virtual Frame<V> Init<_T0>(Frame<_T0> frame)
            where _T0 : V, Value
        {
            returnValue = frame.returnValue;
            Array.Copy(frame.values, 0, values, 0, values.Length);
            numStack = frame.numStack;
            return this;
        }

        /// <summary>
        ///     Initializes a frame corresponding to the target or to the successor of a jump instruction.
        /// </summary>
        /// <remarks>
        ///     Initializes a frame corresponding to the target or to the successor of a jump instruction. This
        ///     method is called by
        ///     <see cref="Analyzer{V}.Analyze" />
        ///     while
        ///     interpreting jump instructions. It is called once for each possible target of the jump
        ///     instruction, and once for its successor instruction (except for GOTO and JSR), before the frame
        ///     is merged with the existing frame at this location. The default implementation of this method
        ///     does nothing.
        ///     <p>
        ///         Overriding this method and changing the frame values allows implementing branch-sensitive
        ///         analyses.
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the jump instruction. Can be IFEQ, IFNE, IFLT, IFGE, IFGT, IFLE,
        ///     IF_ICMPEQ, IF_ICMPNE, IF_ICMPLT, IF_ICMPGE, IF_ICMPGT, IF_ICMPLE, IF_ACMPEQ, IF_ACMPNE,
        ///     GOTO, JSR, IFNULL, IFNONNULL, TABLESWITCH or LOOKUPSWITCH.
        /// </param>
        /// <param name="target">
        ///     a target of the jump instruction this frame corresponds to, or
        ///     <literal>null</literal>
        ///     if
        ///     this frame corresponds to the successor of the jump instruction (i.e. the next instruction
        ///     in the instructions sequence).
        /// </param>
        public virtual void InitJumpTarget(int opcode, LabelNode target)
        {
        }

        // Does nothing by default.
        /// <summary>Sets the expected return type of the analyzed method.</summary>
        /// <param name="v">
        ///     the expected return type of the analyzed method, or
        ///     <literal>null</literal>
        ///     if the method
        ///     returns void.
        /// </param>
        public virtual void SetReturn(V v)
        {
            returnValue = v;
        }

        /// <summary>Returns the maximum number of local variables of this frame.</summary>
        /// <returns>the maximum number of local variables of this frame.</returns>
        public virtual int GetLocals()
        {
            return numLocals;
        }

        /// <summary>Returns the maximum stack size of this frame.</summary>
        /// <returns>the maximum stack size of this frame.</returns>
        public virtual int GetMaxStackSize()
        {
            return values.Length - numLocals;
        }

        /// <summary>Returns the value of the given local variable.</summary>
        /// <param name="index">a local variable index.</param>
        /// <returns>the value of the given local variable.</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the variable does not exist.
        /// </exception>
        public virtual V GetLocal(int index)
        {
            if (index >= numLocals)
                throw new IndexOutOfRangeException("Trying to get an inexistant local variable "
                                                   + index);
            return values[index];
        }

        /// <summary>Sets the value of the given local variable.</summary>
        /// <param name="index">a local variable index.</param>
        /// <param name="value">the new value of this local variable.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the variable does not exist.
        /// </exception>
        public virtual void SetLocal(int index, V value)
        {
            if (index >= numLocals)
                throw new IndexOutOfRangeException("Trying to set an inexistant local variable "
                                                   + index);
            values[index] = value;
        }

        /// <summary>Returns the number of values in the operand stack of this frame.</summary>
        /// <remarks>
        ///     Returns the number of values in the operand stack of this frame. Long and double values are
        ///     treated as single values.
        /// </remarks>
        /// <returns>the number of values in the operand stack of this frame.</returns>
        public virtual int GetStackSize()
        {
            return numStack;
        }

        /// <summary>Returns the value of the given operand stack slot.</summary>
        /// <param name="index">the index of an operand stack slot.</param>
        /// <returns>the value of the given operand stack slot.</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the operand stack slot does not exist.
        /// </exception>
        public virtual V GetStack(int index)
        {
            return values[numLocals + index];
        }

        /// <summary>Sets the value of the given stack slot.</summary>
        /// <param name="index">the index of an operand stack slot.</param>
        /// <param name="value">the new value of the stack slot.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if the stack slot does not exist.
        /// </exception>
        public virtual void SetStack(int index, V value)
        {
            values[numLocals + index] = value;
        }

        /// <summary>Clears the operand stack of this frame.</summary>
        public virtual void ClearStack()
        {
            numStack = 0;
        }

        /// <summary>Pops a value from the operand stack of this frame.</summary>
        /// <returns>the value that has been popped from the stack.</returns>
        /// <exception cref="System.IndexOutOfRangeException">if the operand stack is empty.</exception>
        public virtual V Pop()
        {
            if (numStack == 0) throw new IndexOutOfRangeException("Cannot pop operand off an empty stack.");
            return values[numLocals + --numStack];
        }

        /// <summary>Pushes a value into the operand stack of this frame.</summary>
        /// <param name="value">the value that must be pushed into the stack.</param>
        /// <exception cref="System.IndexOutOfRangeException">if the operand stack is full.</exception>
        public virtual void Push(V value)
        {
            if (numLocals + numStack >= values.Length)
                throw new IndexOutOfRangeException("Insufficient maximum stack size.");
            values[numLocals + numStack++] = value;
        }

        /// <summary>
        ///     Simulates the execution of the given instruction on this execution stack frame.
        /// </summary>
        /// <param name="insn">the instruction to execute.</param>
        /// <param name="interpreter">
        ///     the interpreter to use to compute values from other values.
        /// </param>
        /// <exception cref="AnalyzerException">
        ///     if the instruction cannot be executed on this execution frame (e.g. a
        ///     POP on an empty operand stack).
        /// </exception>
        /// <exception cref="AnalyzerException" />
        public virtual void Execute(AbstractInsnNode insn, Interpreter<V> interpreter)
        {
            V value1;
            V value2;
            V value3;
            V value4;
            int var;
            switch (insn.GetOpcode())
            {
                case OpcodesConstants.Nop:
                {
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
                case OpcodesConstants.Lconst_0:
                case OpcodesConstants.Lconst_1:
                case OpcodesConstants.Fconst_0:
                case OpcodesConstants.Fconst_1:
                case OpcodesConstants.Fconst_2:
                case OpcodesConstants.Dconst_0:
                case OpcodesConstants.Dconst_1:
                case OpcodesConstants.Bipush:
                case OpcodesConstants.Sipush:
                case OpcodesConstants.Ldc:
                {
                    Push(interpreter.NewOperation(insn));
                    break;
                }

                case OpcodesConstants.Iload:
                case OpcodesConstants.Lload:
                case OpcodesConstants.Fload:
                case OpcodesConstants.Dload:
                case OpcodesConstants.Aload:
                {
                    Push(interpreter.CopyOperation(insn, GetLocal(((VarInsnNode) insn).var)));
                    break;
                }

                case OpcodesConstants.Istore:
                case OpcodesConstants.Lstore:
                case OpcodesConstants.Fstore:
                case OpcodesConstants.Dstore:
                case OpcodesConstants.Astore:
                {
                    value1 = interpreter.CopyOperation(insn, Pop());
                    var = ((VarInsnNode) insn).var;
                    SetLocal(var, value1);
                    if (value1.GetSize() == 2) SetLocal(var + 1, interpreter.NewEmptyValue(var + 1));
                    if (var > 0)
                    {
                        Value local = GetLocal(var - 1);
                        if (local != null && local.GetSize() == 2)
                            SetLocal(var - 1, interpreter.NewEmptyValue(var - 1));
                    }

                    break;
                }

                case OpcodesConstants.Iastore:
                case OpcodesConstants.Lastore:
                case OpcodesConstants.Fastore:
                case OpcodesConstants.Dastore:
                case OpcodesConstants.Aastore:
                case OpcodesConstants.Bastore:
                case OpcodesConstants.Castore:
                case OpcodesConstants.Sastore:
                {
                    value3 = Pop();
                    value2 = Pop();
                    value1 = Pop();
                    interpreter.TernaryOperation(insn, value1, value2, value3);
                    break;
                }

                case OpcodesConstants.Pop:
                {
                    if (Pop().GetSize() == 2) throw new AnalyzerException(insn, "Illegal use of POP");
                    break;
                }

                case OpcodesConstants.Pop2:
                {
                    if (Pop().GetSize() == 1 && Pop().GetSize() != 1)
                        throw new AnalyzerException(insn, "Illegal use of POP2");
                    break;
                }

                case OpcodesConstants.Dup:
                {
                    value1 = Pop();
                    if (value1.GetSize() != 1) throw new AnalyzerException(insn, "Illegal use of DUP");
                    Push(value1);
                    Push(interpreter.CopyOperation(insn, value1));
                    break;
                }

                case OpcodesConstants.Dup_X1:
                {
                    value1 = Pop();
                    value2 = Pop();
                    if (value1.GetSize() != 1 || value2.GetSize() != 1)
                        throw new AnalyzerException(insn, "Illegal use of DUP_X1");
                    Push(interpreter.CopyOperation(insn, value1));
                    Push(value2);
                    Push(value1);
                    break;
                }

                case OpcodesConstants.Dup_X2:
                {
                    value1 = Pop();
                    if (value1.GetSize() == 1 && ExecuteDupX2(insn, value1, interpreter)) break;
                    throw new AnalyzerException(insn, "Illegal use of DUP_X2");
                }

                case OpcodesConstants.Dup2:
                {
                    value1 = Pop();
                    if (value1.GetSize() == 1)
                    {
                        value2 = Pop();
                        if (value2.GetSize() == 1)
                        {
                            Push(value2);
                            Push(value1);
                            Push(interpreter.CopyOperation(insn, value2));
                            Push(interpreter.CopyOperation(insn, value1));
                            break;
                        }
                    }
                    else
                    {
                        Push(value1);
                        Push(interpreter.CopyOperation(insn, value1));
                        break;
                    }

                    throw new AnalyzerException(insn, "Illegal use of DUP2");
                }

                case OpcodesConstants.Dup2_X1:
                {
                    value1 = Pop();
                    if (value1.GetSize() == 1)
                    {
                        value2 = Pop();
                        if (value2.GetSize() == 1)
                        {
                            value3 = Pop();
                            if (value3.GetSize() == 1)
                            {
                                Push(interpreter.CopyOperation(insn, value2));
                                Push(interpreter.CopyOperation(insn, value1));
                                Push(value3);
                                Push(value2);
                                Push(value1);
                                break;
                            }
                        }
                    }
                    else
                    {
                        value2 = Pop();
                        if (value2.GetSize() == 1)
                        {
                            Push(interpreter.CopyOperation(insn, value1));
                            Push(value2);
                            Push(value1);
                            break;
                        }
                    }

                    throw new AnalyzerException(insn, "Illegal use of DUP2_X1");
                }

                case OpcodesConstants.Dup2_X2:
                {
                    value1 = Pop();
                    if (value1.GetSize() == 1)
                    {
                        value2 = Pop();
                        if (value2.GetSize() == 1)
                        {
                            value3 = Pop();
                            if (value3.GetSize() == 1)
                            {
                                value4 = Pop();
                                if (value4.GetSize() == 1)
                                {
                                    Push(interpreter.CopyOperation(insn, value2));
                                    Push(interpreter.CopyOperation(insn, value1));
                                    Push(value4);
                                    Push(value3);
                                    Push(value2);
                                    Push(value1);
                                    break;
                                }
                            }
                            else
                            {
                                Push(interpreter.CopyOperation(insn, value2));
                                Push(interpreter.CopyOperation(insn, value1));
                                Push(value3);
                                Push(value2);
                                Push(value1);
                                break;
                            }
                        }
                    }
                    else if (ExecuteDupX2(insn, value1, interpreter))
                    {
                        break;
                    }

                    throw new AnalyzerException(insn, "Illegal use of DUP2_X2");
                }

                case OpcodesConstants.Swap:
                {
                    value2 = Pop();
                    value1 = Pop();
                    if (value1.GetSize() != 1 || value2.GetSize() != 1)
                        throw new AnalyzerException(insn, "Illegal use of SWAP");
                    Push(interpreter.CopyOperation(insn, value2));
                    Push(interpreter.CopyOperation(insn, value1));
                    break;
                }

                case OpcodesConstants.Iaload:
                case OpcodesConstants.Laload:
                case OpcodesConstants.Faload:
                case OpcodesConstants.Daload:
                case OpcodesConstants.Aaload:
                case OpcodesConstants.Baload:
                case OpcodesConstants.Caload:
                case OpcodesConstants.Saload:
                case OpcodesConstants.Iadd:
                case OpcodesConstants.Ladd:
                case OpcodesConstants.Fadd:
                case OpcodesConstants.Dadd:
                case OpcodesConstants.Isub:
                case OpcodesConstants.Lsub:
                case OpcodesConstants.Fsub:
                case OpcodesConstants.Dsub:
                case OpcodesConstants.Imul:
                case OpcodesConstants.Lmul:
                case OpcodesConstants.Fmul:
                case OpcodesConstants.Dmul:
                case OpcodesConstants.Idiv:
                case OpcodesConstants.Ldiv:
                case OpcodesConstants.Fdiv:
                case OpcodesConstants.Ddiv:
                case OpcodesConstants.Irem:
                case OpcodesConstants.Lrem:
                case OpcodesConstants.Frem:
                case OpcodesConstants.Drem:
                case OpcodesConstants.Ishl:
                case OpcodesConstants.Lshl:
                case OpcodesConstants.Ishr:
                case OpcodesConstants.Lshr:
                case OpcodesConstants.Iushr:
                case OpcodesConstants.Lushr:
                case OpcodesConstants.Iand:
                case OpcodesConstants.Land:
                case OpcodesConstants.Ior:
                case OpcodesConstants.Lor:
                case OpcodesConstants.Ixor:
                case OpcodesConstants.Lxor:
                case OpcodesConstants.Lcmp:
                case OpcodesConstants.Fcmpl:
                case OpcodesConstants.Fcmpg:
                case OpcodesConstants.Dcmpl:
                case OpcodesConstants.Dcmpg:
                {
                    value2 = Pop();
                    value1 = Pop();
                    Push(interpreter.BinaryOperation(insn, value1, value2));
                    break;
                }

                case OpcodesConstants.Ineg:
                case OpcodesConstants.Lneg:
                case OpcodesConstants.Fneg:
                case OpcodesConstants.Dneg:
                {
                    Push(interpreter.UnaryOperation(insn, Pop()));
                    break;
                }

                case OpcodesConstants.Iinc:
                {
                    var = ((IincInsnNode) insn).var;
                    SetLocal(var, interpreter.UnaryOperation(insn, GetLocal(var)));
                    break;
                }

                case OpcodesConstants.I2l:
                case OpcodesConstants.I2f:
                case OpcodesConstants.I2d:
                case OpcodesConstants.L2i:
                case OpcodesConstants.L2f:
                case OpcodesConstants.L2d:
                case OpcodesConstants.F2i:
                case OpcodesConstants.F2l:
                case OpcodesConstants.F2d:
                case OpcodesConstants.D2i:
                case OpcodesConstants.D2l:
                case OpcodesConstants.D2f:
                case OpcodesConstants.I2b:
                case OpcodesConstants.I2c:
                case OpcodesConstants.I2s:
                {
                    Push(interpreter.UnaryOperation(insn, Pop()));
                    break;
                }

                case OpcodesConstants.Ifeq:
                case OpcodesConstants.Ifne:
                case OpcodesConstants.Iflt:
                case OpcodesConstants.Ifge:
                case OpcodesConstants.Ifgt:
                case OpcodesConstants.Ifle:
                {
                    interpreter.UnaryOperation(insn, Pop());
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
                case OpcodesConstants.Putfield:
                {
                    value2 = Pop();
                    value1 = Pop();
                    interpreter.BinaryOperation(insn, value1, value2);
                    break;
                }

                case OpcodesConstants.Goto:
                {
                    break;
                }

                case OpcodesConstants.Jsr:
                {
                    Push(interpreter.NewOperation(insn));
                    break;
                }

                case OpcodesConstants.Ret:
                {
                    break;
                }

                case OpcodesConstants.Tableswitch:
                case OpcodesConstants.Lookupswitch:
                {
                    interpreter.UnaryOperation(insn, Pop());
                    break;
                }

                case OpcodesConstants.Ireturn:
                case OpcodesConstants.Lreturn:
                case OpcodesConstants.Freturn:
                case OpcodesConstants.Dreturn:
                case OpcodesConstants.Areturn:
                {
                    value1 = Pop();
                    interpreter.UnaryOperation(insn, value1);
                    interpreter.ReturnOperation(insn, value1, returnValue);
                    break;
                }

                case OpcodesConstants.Return:
                {
                    if (returnValue != null) throw new AnalyzerException(insn, "Incompatible return type");
                    break;
                }

                case OpcodesConstants.Getstatic:
                {
                    Push(interpreter.NewOperation(insn));
                    break;
                }

                case OpcodesConstants.Putstatic:
                {
                    interpreter.UnaryOperation(insn, Pop());
                    break;
                }

                case OpcodesConstants.Getfield:
                {
                    Push(interpreter.UnaryOperation(insn, Pop()));
                    break;
                }

                case OpcodesConstants.Invokevirtual:
                case OpcodesConstants.Invokespecial:
                case OpcodesConstants.Invokestatic:
                case OpcodesConstants.Invokeinterface:
                {
                    ExecuteInvokeInsn(insn, ((MethodInsnNode) insn).desc, interpreter);
                    break;
                }

                case OpcodesConstants.Invokedynamic:
                {
                    ExecuteInvokeInsn(insn, ((InvokeDynamicInsnNode) insn).desc, interpreter);
                    break;
                }

                case OpcodesConstants.New:
                {
                    Push(interpreter.NewOperation(insn));
                    break;
                }

                case OpcodesConstants.Newarray:
                case OpcodesConstants.Anewarray:
                case OpcodesConstants.Arraylength:
                {
                    Push(interpreter.UnaryOperation(insn, Pop()));
                    break;
                }

                case OpcodesConstants.Athrow:
                {
                    interpreter.UnaryOperation(insn, Pop());
                    break;
                }

                case OpcodesConstants.Checkcast:
                case OpcodesConstants.Instanceof:
                {
                    Push(interpreter.UnaryOperation(insn, Pop()));
                    break;
                }

                case OpcodesConstants.Monitorenter:
                case OpcodesConstants.Monitorexit:
                {
                    interpreter.UnaryOperation(insn, Pop());
                    break;
                }

                case OpcodesConstants.Multianewarray:
                {
                    IList<V> valueList = new List<V>();
                    for (var i = ((MultiANewArrayInsnNode) insn).dims; i > 0; --i) valueList.Add(0, Pop());
                    Push(interpreter.NaryOperation(insn, valueList));
                    break;
                }

                case OpcodesConstants.Ifnull:
                case OpcodesConstants.Ifnonnull:
                {
                    interpreter.UnaryOperation(insn, Pop());
                    break;
                }

                default:
                {
                    throw new AnalyzerException(insn, "Illegal opcode " + insn.GetOpcode());
                }
            }
        }

        /// <exception cref="AnalyzerException" />
        private bool ExecuteDupX2(AbstractInsnNode insn, V value1, Interpreter<V> interpreter
        )
        {
            var value2 = Pop();
            if (value2.GetSize() == 1)
            {
                var value3 = Pop();
                if (value3.GetSize() == 1)
                {
                    Push(interpreter.CopyOperation(insn, value1));
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    return true;
                }
            }
            else
            {
                Push(interpreter.CopyOperation(insn, value1));
                Push(value2);
                Push(value1);
                return true;
            }

            return false;
        }

        /// <exception cref="AnalyzerException" />
        private void ExecuteInvokeInsn(AbstractInsnNode insn, string methodDescriptor, Interpreter
            <V> interpreter)
        {
            var valueList = new List<V>();
            for (var i = Type.GetArgumentTypes(methodDescriptor).Length; i > 0; --i) valueList.Add(0, Pop());
            if (insn.GetOpcode() != OpcodesConstants.Invokestatic && insn.GetOpcode() != OpcodesConstants
                    .Invokedynamic)
                valueList.Add(0, Pop());
            if (Type.GetReturnType(methodDescriptor) == Type.Void_Type)
                interpreter.NaryOperation(insn, valueList);
            else
                Push(interpreter.NaryOperation(insn, valueList));
        }

        /// <summary>Merges the given frame into this frame.</summary>
        /// <param name="frame">a frame. This frame is left unchanged by this method.</param>
        /// <param name="interpreter">the interpreter used to merge values.</param>
        /// <returns>
        ///     <literal>true</literal>
        ///     if this frame has been changed as a result of the merge operation, or
        ///     <literal>false</literal>
        ///     otherwise.
        /// </returns>
        /// <exception cref="AnalyzerException">if the frames have incompatible sizes.</exception>
        /// <exception cref="AnalyzerException" />
        public virtual bool Merge<_T0>(Frame<_T0> frame, Interpreter<V> interpreter)
            where _T0 : V, Value
        {
            if (numStack != frame.numStack) throw new AnalyzerException(null, "Incompatible stack heights");
            var changed = false;
            for (var i = 0; i < numLocals + numStack; ++i)
            {
                var v = interpreter.Merge(values[i], frame.values[i]);
                if (!v.Equals(values[i]))
                {
                    values[i] = v;
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>Merges the given frame into this frame (case of a subroutine).</summary>
        /// <remarks>
        ///     Merges the given frame into this frame (case of a subroutine). The operand stacks are not
        ///     merged, and only the local variables that have not been used by the subroutine are merged.
        /// </remarks>
        /// <param name="frame">a frame. This frame is left unchanged by this method.</param>
        /// <param name="localsUsed">
        ///     the local variables that are read or written by the subroutine. The i-th
        ///     element is true if and only if the local variable at index i is read or written by the
        ///     subroutine.
        /// </param>
        /// <returns>
        ///     <literal>true</literal>
        ///     if this frame has been changed as a result of the merge operation, or
        ///     <literal>false</literal>
        ///     otherwise.
        /// </returns>
        public virtual bool Merge<_T0>(Frame<_T0> frame, bool[] localsUsed)
            where _T0 : V, Value
        {
            var changed = false;
            for (var i = 0; i < numLocals; ++i)
                if (!localsUsed[i] && !values[i].Equals(frame.values[i]))
                {
                    values[i] = frame.values[i];
                    changed = true;
                }

            return changed;
        }

        /// <summary>Returns a string representation of this frame.</summary>
        /// <returns>a string representation of this frame.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < GetLocals(); ++i) stringBuilder.Append(GetLocal(i));
            stringBuilder.Append(' ');
            for (var i = 0; i < GetStackSize(); ++i) stringBuilder.Append(GetStack(i));
            return stringBuilder.ToString();
        }
    }
}