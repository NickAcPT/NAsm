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

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>A semantic bytecode analyzer.</summary>
    /// <remarks>
    ///     A semantic bytecode analyzer.
    ///     <i>
    ///         This class does not fully check that JSR and RET instructions
    ///         are valid.
    ///     </i>
    /// </remarks>
    /// <?/>
    /// <author>Eric Bruneton</author>
    public class Analyzer<V> : Opcodes
        where V : Value
    {
        /// <summary>
        ///     The interpreter to use to symbolically interpret the bytecode instructions.
        /// </summary>
        private readonly Interpreter<V> interpreter;

        /// <summary>
        ///     The execution stack frames of the currently analyzed method (one per instruction index).
        /// </summary>
        private Frame<V>[] frames;

        /// <summary>
        ///     The exception handlers of the currently analyzed method (one list per instruction index).
        /// </summary>
        private IList<TryCatchBlockNode>[] handlers;

        /// <summary>
        ///     The instructions that remain to process (one boolean per instruction index).
        /// </summary>
        private bool[] inInstructionsToProcess;

        /// <summary>The instructions of the currently analyzed method.</summary>
        private InsnList insnList;

        /// <summary>
        ///     The size of
        ///     <see cref="Analyzer{V}.insnList" />
        ///     .
        /// </summary>
        private int insnListSize;

        /// <summary>
        ///     The indices of the instructions that remain to process in the currently analyzed method.
        /// </summary>
        private int[] instructionsToProcess;

        /// <summary>
        ///     The number of instructions that remain to process in the currently analyzed method.
        /// </summary>
        private int numInstructionsToProcess;

        /// <summary>
        ///     The subroutines of the currently analyzed method (one per instruction index).
        /// </summary>
        private Subroutine[] subroutines;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Analyzer{V}" />
        ///     .
        /// </summary>
        /// <param name="interpreter">
        ///     the interpreter to use to symbolically interpret the bytecode instructions.
        /// </param>
        public Analyzer(Interpreter<V> interpreter)
        {
            this.interpreter = interpreter;
        }

        /// <summary>Analyzes the given method.</summary>
        /// <param name="owner">the internal name of the class to which 'method' belongs.</param>
        /// <param name="method">the method to be analyzed.</param>
        /// <returns>
        ///     the symbolic state of the execution stack frame at each bytecode instruction of the
        ///     method. The size of the returned array is equal to the number of instructions (and labels)
        ///     of the method. A given frame is
        ///     <literal>null</literal>
        ///     if and only if the corresponding
        ///     instruction cannot be reached (dead code).
        /// </returns>
        /// <exception cref="AnalyzerException">if a problem occurs during the analysis.</exception>
        /// <exception cref="AnalyzerException" />
        public virtual Frame<V>[] Analyze(string owner, MethodNode method)
        {
            if ((method.access & (OpcodesConstants.Acc_Abstract | OpcodesConstants.Acc_Native
                 )) != 0)
            {
                frames = new Frame<V>[0];
                return frames;
            }

            insnList = method.instructions;
            insnListSize = insnList.Size();
            handlers = (IList<TryCatchBlockNode>[]) new IList<object>[insnListSize];
            frames = new Frame<V>[insnListSize];
            subroutines = new Subroutine[insnListSize];
            inInstructionsToProcess = new bool[insnListSize];
            instructionsToProcess = new int[insnListSize];
            numInstructionsToProcess = 0;
            // For each exception handler, and each instruction within its range, record in 'handlers' the
            // fact that execution can flow from this instruction to the exception handler.
            for (var i = 0; i < method.tryCatchBlocks.Count; ++i)
            {
                var tryCatchBlock = method.tryCatchBlocks[i];
                var startIndex = insnList.IndexOf(tryCatchBlock.start);
                var endIndex = insnList.IndexOf(tryCatchBlock.end);
                for (var j = startIndex; j < endIndex; ++j)
                {
                    var insnHandlers = handlers[j];
                    if (insnHandlers == null)
                    {
                        insnHandlers = new List<TryCatchBlockNode>();
                        handlers[j] = insnHandlers;
                    }

                    insnHandlers.Add(tryCatchBlock);
                }
            }

            // For each instruction, compute the subroutine to which it belongs.
            // Follow the main 'subroutine', and collect the jsr instructions to nested subroutines.
            var main = new Subroutine(null, method.maxLocals, null);
            IList<AbstractInsnNode> jsrInsns = new List<AbstractInsnNode>();
            FindSubroutine(0, main, jsrInsns);
            // Follow the nested subroutines, and collect their own nested subroutines, until all
            // subroutines are found.
            IDictionary<LabelNode, Subroutine> jsrSubroutines = new Dictionary<LabelNode, Subroutine
            >();
            while (!(jsrInsns.Count == 0))
            {
                var jsrInsn = (JumpInsnNode) jsrInsns.RemoveAtReturningValue(0);
                var subroutine = jsrSubroutines.GetOrNull(jsrInsn.label);
                if (subroutine == null)
                {
                    subroutine = new Subroutine(jsrInsn.label, method.maxLocals, jsrInsn);
                    Collections.Put(jsrSubroutines, jsrInsn.label, subroutine);
                    FindSubroutine(insnList.IndexOf(jsrInsn.label), subroutine, jsrInsns);
                }
                else
                {
                    subroutine.callers.Add(jsrInsn);
                }
            }

            // Clear the main 'subroutine', which is not a real subroutine (and was used only as an
            // intermediate step above to find the real ones).
            for (var i = 0; i < insnListSize; ++i)
                if (subroutines[i] != null && subroutines[i].start == null)
                    subroutines[i] = null;
            // Initializes the data structures for the control flow analysis.
            var currentFrame = ComputeInitialFrame(owner, method);
            Merge(0, currentFrame, null);
            Init(owner, method);
            // Control flow analysis.
            while (numInstructionsToProcess > 0)
            {
                // Get and remove one instruction from the list of instructions to process.
                var insnIndex = instructionsToProcess[--numInstructionsToProcess];
                var oldFrame = frames[insnIndex];
                var subroutine = subroutines[insnIndex];
                inInstructionsToProcess[insnIndex] = false;
                // Simulate the execution of this instruction.
                AbstractInsnNode insnNode = null;
                try
                {
                    insnNode = method.instructions.Get(insnIndex);
                    var insnOpcode = insnNode.GetOpcode();
                    var insnType = insnNode.GetType();
                    if (insnType == AbstractInsnNode.Label || insnType == AbstractInsnNode.Line || insnType
                        == AbstractInsnNode.Frame)
                    {
                        Merge(insnIndex + 1, oldFrame, subroutine);
                        NewControlFlowEdge(insnIndex, insnIndex + 1);
                    }
                    else
                    {
                        currentFrame.Init(oldFrame).Execute(insnNode, interpreter);
                        subroutine = subroutine == null ? null : new Subroutine(subroutine);
                        if (insnNode is JumpInsnNode)
                        {
                            var jumpInsn = (JumpInsnNode) insnNode;
                            if (insnOpcode != OpcodesConstants.Goto && insnOpcode != OpcodesConstants.Jsr)
                            {
                                currentFrame.InitJumpTarget(insnOpcode, null);
                                /* target = */
                                Merge(insnIndex + 1, currentFrame, subroutine);
                                NewControlFlowEdge(insnIndex, insnIndex + 1);
                            }

                            var jumpInsnIndex = insnList.IndexOf(jumpInsn.label);
                            currentFrame.InitJumpTarget(insnOpcode, jumpInsn.label);
                            if (insnOpcode == OpcodesConstants.Jsr)
                                Merge(jumpInsnIndex, currentFrame, new Subroutine(jumpInsn.label, method.maxLocals
                                    , jumpInsn));
                            else
                                Merge(jumpInsnIndex, currentFrame, subroutine);
                            NewControlFlowEdge(insnIndex, jumpInsnIndex);
                        }
                        else if (insnNode is LookupSwitchInsnNode)
                        {
                            var lookupSwitchInsn = (LookupSwitchInsnNode) insnNode;
                            var targetInsnIndex = insnList.IndexOf(lookupSwitchInsn.dflt);
                            currentFrame.InitJumpTarget(insnOpcode, lookupSwitchInsn.dflt);
                            Merge(targetInsnIndex, currentFrame, subroutine);
                            NewControlFlowEdge(insnIndex, targetInsnIndex);
                            for (var i = 0; i < lookupSwitchInsn.labels.Count; ++i)
                            {
                                var label = lookupSwitchInsn.labels[i];
                                targetInsnIndex = insnList.IndexOf(label);
                                currentFrame.InitJumpTarget(insnOpcode, label);
                                Merge(targetInsnIndex, currentFrame, subroutine);
                                NewControlFlowEdge(insnIndex, targetInsnIndex);
                            }
                        }
                        else if (insnNode is TableSwitchInsnNode)
                        {
                            var tableSwitchInsn = (TableSwitchInsnNode) insnNode;
                            var targetInsnIndex = insnList.IndexOf(tableSwitchInsn.dflt);
                            currentFrame.InitJumpTarget(insnOpcode, tableSwitchInsn.dflt);
                            Merge(targetInsnIndex, currentFrame, subroutine);
                            NewControlFlowEdge(insnIndex, targetInsnIndex);
                            for (var i = 0; i < tableSwitchInsn.labels.Count; ++i)
                            {
                                var label = tableSwitchInsn.labels[i];
                                currentFrame.InitJumpTarget(insnOpcode, label);
                                targetInsnIndex = insnList.IndexOf(label);
                                Merge(targetInsnIndex, currentFrame, subroutine);
                                NewControlFlowEdge(insnIndex, targetInsnIndex);
                            }
                        }
                        else if (insnOpcode == OpcodesConstants.Ret)
                        {
                            if (subroutine == null)
                                throw new AnalyzerException(insnNode, "RET instruction outside of a subroutine");
                            for (var i = 0; i < subroutine.callers.Count; ++i)
                            {
                                var caller = subroutine.callers[i];
                                var jsrInsnIndex = insnList.IndexOf(caller);
                                if (frames[jsrInsnIndex] != null)
                                {
                                    Merge(jsrInsnIndex + 1, frames[jsrInsnIndex], currentFrame, subroutines[jsrInsnIndex
                                    ], subroutine.localsUsed);
                                    NewControlFlowEdge(insnIndex, jsrInsnIndex + 1);
                                }
                            }
                        }
                        else if (insnOpcode != OpcodesConstants.Athrow &&
                                 (insnOpcode < OpcodesConstants.Ireturn || insnOpcode > OpcodesConstants.Return))
                        {
                            if (subroutine != null)
                            {
                                if (insnNode is VarInsnNode)
                                {
                                    var var = ((VarInsnNode) insnNode).var;
                                    subroutine.localsUsed[var] = true;
                                    if (insnOpcode == OpcodesConstants.Lload || insnOpcode == OpcodesConstants.Dload
                                                                             || insnOpcode == OpcodesConstants.Lstore ||
                                                                             insnOpcode == OpcodesConstants.Dstore)
                                        subroutine.localsUsed[var + 1] = true;
                                }
                                else if (insnNode is IincInsnNode)
                                {
                                    var var = ((IincInsnNode) insnNode).var;
                                    subroutine.localsUsed[var] = true;
                                }
                            }

                            Merge(insnIndex + 1, currentFrame, subroutine);
                            NewControlFlowEdge(insnIndex, insnIndex + 1);
                        }
                    }

                    var insnHandlers = handlers[insnIndex];
                    if (insnHandlers != null)
                        foreach (var tryCatchBlock in insnHandlers)
                        {
                            Type catchType;
                            if (tryCatchBlock.type == null)
                                catchType = Type.GetObjectType("java/lang/Throwable");
                            else
                                catchType = Type.GetObjectType(tryCatchBlock.type);
                            if (NewControlFlowExceptionEdge(insnIndex, tryCatchBlock))
                            {
                                var handler = NewFrame(oldFrame);
                                handler.ClearStack();
                                handler.Push(interpreter.NewExceptionValue(tryCatchBlock, handler, catchType));
                                Merge(insnList.IndexOf(tryCatchBlock.handler), handler, subroutine);
                            }
                        }
                }
                catch (AnalyzerException e)
                {
                    throw new AnalyzerException(e.node, "Error at instruction " + insnIndex + ": " +
                                                        e.Message, e);
                }
                catch (Exception e)
                {
                    // DontCheck(IllegalCatch): can't be fixed, for backward compatibility.
                    throw new AnalyzerException(insnNode, "Error at instruction " + insnIndex + ": "
                                                          + e.Message, e);
                }
            }

            return frames;
        }

        /// <summary>
        ///     Follows the control flow graph of the currently analyzed method, starting at the given
        ///     instruction index, and stores a copy of the given subroutine in
        ///     <see cref="Analyzer{V}.subroutines" />
        ///     for each
        ///     encountered instruction. Jumps to nested subroutines are <i>not</i> followed: instead, the
        ///     corresponding instructions are put in the given list.
        /// </summary>
        /// <param name="insnIndex">an instruction index.</param>
        /// <param name="subroutine">a subroutine.</param>
        /// <param name="jsrInsns">
        ///     where the jsr instructions for nested subroutines must be put.
        /// </param>
        /// <exception cref="AnalyzerException">
        ///     if the control flow graph can fall off the end of the code.
        /// </exception>
        /// <exception cref="AnalyzerException" />
        private void FindSubroutine(int insnIndex, Subroutine subroutine, IList<AbstractInsnNode
        > jsrInsns)
        {
            var instructionIndicesToProcess = new List<int>();
            instructionIndicesToProcess.Add(insnIndex);
            while (!(instructionIndicesToProcess.Count == 0))
            {
                var currentInsnIndex = instructionIndicesToProcess.RemoveAtReturningValue(instructionIndicesToProcess
                                                                                              .Count - 1);
                if (currentInsnIndex < 0 || currentInsnIndex >= insnListSize)
                    throw new AnalyzerException(null, "Execution can fall off the end of the code");
                if (subroutines[currentInsnIndex] != null) continue;
                subroutines[currentInsnIndex] = new Subroutine(subroutine);
                var currentInsn = insnList.Get(currentInsnIndex);
                // Push the normal successors of currentInsn onto instructionIndicesToProcess.
                if (currentInsn is JumpInsnNode)
                {
                    if (currentInsn.GetOpcode() == OpcodesConstants.Jsr)
                    {
                        // Do not follow a jsr, it leads to another subroutine!
                        jsrInsns.Add(currentInsn);
                    }
                    else
                    {
                        var jumpInsn = (JumpInsnNode) currentInsn;
                        instructionIndicesToProcess.Add(insnList.IndexOf(jumpInsn.label));
                    }
                }
                else if (currentInsn is TableSwitchInsnNode)
                {
                    var tableSwitchInsn = (TableSwitchInsnNode) currentInsn;
                    FindSubroutine(insnList.IndexOf(tableSwitchInsn.dflt), subroutine, jsrInsns);
                    for (var i = tableSwitchInsn.labels.Count - 1; i >= 0; --i)
                    {
                        var labelNode = tableSwitchInsn.labels[i];
                        instructionIndicesToProcess.Add(insnList.IndexOf(labelNode));
                    }
                }
                else if (currentInsn is LookupSwitchInsnNode)
                {
                    var lookupSwitchInsn = (LookupSwitchInsnNode) currentInsn;
                    FindSubroutine(insnList.IndexOf(lookupSwitchInsn.dflt), subroutine, jsrInsns);
                    for (var i = lookupSwitchInsn.labels.Count - 1; i >= 0; --i)
                    {
                        var labelNode = lookupSwitchInsn.labels[i];
                        instructionIndicesToProcess.Add(insnList.IndexOf(labelNode));
                    }
                }

                // Push the exception handler successors of currentInsn onto instructionIndicesToProcess.
                var insnHandlers = handlers[currentInsnIndex];
                if (insnHandlers != null)
                    foreach (var tryCatchBlock in insnHandlers)
                        instructionIndicesToProcess.Add(insnList.IndexOf(tryCatchBlock.handler));
                switch (currentInsn.GetOpcode())
                {
                    case OpcodesConstants.Goto:
                    case OpcodesConstants.Ret:
                    case OpcodesConstants.Tableswitch:
                    case OpcodesConstants.Lookupswitch:
                    case OpcodesConstants.Ireturn:
                    case OpcodesConstants.Lreturn:
                    case OpcodesConstants.Freturn:
                    case OpcodesConstants.Dreturn:
                    case OpcodesConstants.Areturn:
                    case OpcodesConstants.Return:
                    case OpcodesConstants.Athrow:
                    {
                        // Push the next instruction, if the control flow can go from currentInsn to the next.
                        break;
                    }

                    default:
                    {
                        instructionIndicesToProcess.Add(currentInsnIndex + 1);
                        break;
                    }
                }
            }
        }

        /// <summary>Computes the initial execution stack frame of the given method.</summary>
        /// <param name="owner">the internal name of the class to which 'method' belongs.</param>
        /// <param name="method">the method to be analyzed.</param>
        /// <returns>the initial execution stack frame of the 'method'.</returns>
        private Frame<V> ComputeInitialFrame(string owner, MethodNode method)
        {
            var frame = NewFrame(method.maxLocals, method.maxStack);
            var currentLocal = 0;
            var isInstanceMethod = (method.access & OpcodesConstants.Acc_Static) == 0;
            if (isInstanceMethod)
            {
                var ownerType = Type.GetObjectType(owner);
                frame.SetLocal(currentLocal, interpreter.NewParameterValue(isInstanceMethod, currentLocal
                    , ownerType));
                currentLocal++;
            }

            var argumentTypes = Type.GetArgumentTypes(method.desc);
            foreach (var argumentType in argumentTypes)
            {
                frame.SetLocal(currentLocal, interpreter.NewParameterValue(isInstanceMethod, currentLocal
                    , argumentType));
                currentLocal++;
                if (argumentType.GetSize() == 2)
                {
                    frame.SetLocal(currentLocal, interpreter.NewEmptyValue(currentLocal));
                    currentLocal++;
                }
            }

            while (currentLocal < method.maxLocals)
            {
                frame.SetLocal(currentLocal, interpreter.NewEmptyValue(currentLocal));
                currentLocal++;
            }

            frame.SetReturn(interpreter.NewReturnTypeValue(Type.GetReturnType(method.desc)));
            return frame;
        }

        /// <summary>
        ///     Returns the symbolic execution stack frame for each instruction of the last analyzed method.
        /// </summary>
        /// <returns>
        ///     the symbolic state of the execution stack frame at each bytecode instruction of the
        ///     method. The size of the returned array is equal to the number of instructions (and labels)
        ///     of the method. A given frame is
        ///     <literal>null</literal>
        ///     if the corresponding instruction cannot be
        ///     reached, or if an error occurred during the analysis of the method.
        /// </returns>
        public virtual Frame<V>[] GetFrames()
        {
            return frames;
        }

        /// <summary>Returns the exception handlers for the given instruction.</summary>
        /// <param name="insnIndex">the index of an instruction of the last analyzed method.</param>
        /// <returns>
        ///     a list of
        ///     <see cref="TryCatchBlockNode" />
        ///     objects.
        /// </returns>
        public virtual IList<TryCatchBlockNode> GetHandlers(int insnIndex)
        {
            return handlers[insnIndex];
        }

        /// <summary>Initializes this analyzer.</summary>
        /// <remarks>
        ///     Initializes this analyzer. This method is called just before the execution of control flow
        ///     analysis loop in #analyze. The default implementation of this method does nothing.
        /// </remarks>
        /// <param name="owner">the internal name of the class to which the method belongs.</param>
        /// <param name="method">the method to be analyzed.</param>
        /// <exception cref="AnalyzerException">if a problem occurs.</exception>
        /// <exception cref="AnalyzerException" />
        protected internal virtual void Init(string owner, MethodNode method)
        {
        }

        // Nothing to do.
        /// <summary>Constructs a new frame with the given size.</summary>
        /// <param name="numLocals">the maximum number of local variables of the frame.</param>
        /// <param name="numStack">the maximum stack size of the frame.</param>
        /// <returns>the created frame.</returns>
        protected internal virtual Frame<V> NewFrame(int numLocals, int numStack)
        {
            return new Frame<V>(numLocals, numStack);
        }

        /// <summary>Constructs a copy of the given frame.</summary>
        /// <param name="frame">a frame.</param>
        /// <returns>the created frame.</returns>
        protected internal virtual Frame<V> NewFrame(Frame<V> frame)
        {
            return new Frame<V>(frame);
        }

        /// <summary>Creates a control flow graph edge.</summary>
        /// <remarks>
        ///     Creates a control flow graph edge. The default implementation of this method does nothing. It
        ///     can be overridden in order to construct the control flow graph of a method (this method is
        ///     called by the
        ///     <see cref="Analyze" />
        ///     method during its visit of the method's code).
        /// </remarks>
        /// <param name="insnIndex">an instruction index.</param>
        /// <param name="successorIndex">index of a successor instruction.</param>
        protected internal virtual void NewControlFlowEdge(int insnIndex, int successorIndex
        )
        {
        }

        // Nothing to do.
        /// <summary>
        ///     Creates a control flow graph edge corresponding to an exception handler.
        /// </summary>
        /// <remarks>
        ///     Creates a control flow graph edge corresponding to an exception handler. The default
        ///     implementation of this method does nothing. It can be overridden in order to construct the
        ///     control flow graph of a method (this method is called by the
        ///     <see cref="Analyze" />
        ///     method during its
        ///     visit of the method's code).
        /// </remarks>
        /// <param name="insnIndex">an instruction index.</param>
        /// <param name="successorIndex">index of a successor instruction.</param>
        /// <returns>
        ///     true if this edge must be considered in the data flow analysis performed by this
        ///     analyzer, or false otherwise. The default implementation of this method always returns
        ///     true.
        /// </returns>
        protected internal virtual bool NewControlFlowExceptionEdge(int insnIndex, int successorIndex
        )
        {
            return true;
        }

        /// <summary>
        ///     Creates a control flow graph edge corresponding to an exception handler.
        /// </summary>
        /// <remarks>
        ///     Creates a control flow graph edge corresponding to an exception handler. The default
        ///     implementation of this method delegates to
        ///     <see cref="Analyzer{V}.NewControlFlowExceptionEdge(int, int)" />
        ///     . It
        ///     can be overridden in order to construct the control flow graph of a method (this method is
        ///     called by the
        ///     <see cref="Analyze" />
        ///     method during its visit of the method's code).
        /// </remarks>
        /// <param name="insnIndex">an instruction index.</param>
        /// <param name="tryCatchBlock">TryCatchBlockNode corresponding to this edge.</param>
        /// <returns>
        ///     true if this edge must be considered in the data flow analysis performed by this
        ///     analyzer, or false otherwise. The default implementation of this method delegates to
        ///     <see cref="Analyzer{V}.NewControlFlowExceptionEdge(int, int)" />
        ///     .
        /// </returns>
        protected internal virtual bool NewControlFlowExceptionEdge(int insnIndex, TryCatchBlockNode
            tryCatchBlock)
        {
            return NewControlFlowExceptionEdge(insnIndex, insnList.IndexOf(tryCatchBlock.handler
            ));
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Merges the given frame and subroutine into the frame and subroutines at the given instruction
        ///     index.
        /// </summary>
        /// <remarks>
        ///     Merges the given frame and subroutine into the frame and subroutines at the given instruction
        ///     index. If the frame or the subroutine at the given instruction index changes as a result of
        ///     this merge, the instruction index is added to the list of instructions to process (if it is not
        ///     already the case).
        /// </remarks>
        /// <param name="insnIndex">an instruction index.</param>
        /// <param name="frame">a frame. This frame is left unchanged by this method.</param>
        /// <param name="subroutine">
        ///     a subroutine. This subroutine is left unchanged by this method.
        /// </param>
        /// <exception cref="AnalyzerException">if the frames have incompatible sizes.</exception>
        /// <exception cref="AnalyzerException" />
        private void Merge(int insnIndex, Frame<V> frame, Subroutine subroutine)
        {
            bool changed;
            var oldFrame = frames[insnIndex];
            if (oldFrame == null)
            {
                frames[insnIndex] = NewFrame(frame);
                changed = true;
            }
            else
            {
                changed = oldFrame.Merge(frame, interpreter);
            }

            var oldSubroutine = subroutines[insnIndex];
            if (oldSubroutine == null)
            {
                if (subroutine != null)
                {
                    subroutines[insnIndex] = new Subroutine(subroutine);
                    changed = true;
                }
            }
            else if (subroutine != null)
            {
                changed |= oldSubroutine.Merge(subroutine);
            }

            if (changed && !inInstructionsToProcess[insnIndex])
            {
                inInstructionsToProcess[insnIndex] = true;
                instructionsToProcess[numInstructionsToProcess++] = insnIndex;
            }
        }

        /// <summary>
        ///     Merges the given frame and subroutine into the frame and subroutines at the given instruction
        ///     index (case of a RET instruction).
        /// </summary>
        /// <remarks>
        ///     Merges the given frame and subroutine into the frame and subroutines at the given instruction
        ///     index (case of a RET instruction). If the frame or the subroutine at the given instruction
        ///     index changes as a result of this merge, the instruction index is added to the list of
        ///     instructions to process (if it is not already the case).
        /// </remarks>
        /// <param name="insnIndex">
        ///     the index of an instruction immediately following a jsr instruction.
        /// </param>
        /// <param name="frameBeforeJsr">
        ///     the execution stack frame before the jsr instruction. This frame is
        ///     merged into 'frameAfterRet'.
        /// </param>
        /// <param name="frameAfterRet">
        ///     the execution stack frame after a ret instruction of the subroutine. This
        ///     frame is merged into the frame at 'insnIndex' (after it has itself been merge with
        ///     'frameBeforeJsr').
        /// </param>
        /// <param name="subroutineBeforeJsr">
        ///     if the jsr is itself part of a subroutine (case of nested
        ///     subroutine), the subroutine it belongs to.
        /// </param>
        /// <param name="localsUsed">the local variables read or written in the subroutine.</param>
        /// <exception cref="AnalyzerException">if the frames have incompatible sizes.</exception>
        /// <exception cref="AnalyzerException" />
        private void Merge(int insnIndex, Frame<V> frameBeforeJsr, Frame<V> frameAfterRet
            , Subroutine subroutineBeforeJsr, bool[] localsUsed)
        {
            frameAfterRet.Merge(frameBeforeJsr, localsUsed);
            bool changed;
            var oldFrame = frames[insnIndex];
            if (oldFrame == null)
            {
                frames[insnIndex] = NewFrame(frameAfterRet);
                changed = true;
            }
            else
            {
                changed = oldFrame.Merge(frameAfterRet, interpreter);
            }

            var oldSubroutine = subroutines[insnIndex];
            if (oldSubroutine != null && subroutineBeforeJsr != null)
                changed |= oldSubroutine.Merge(subroutineBeforeJsr);
            if (changed && !inInstructionsToProcess[insnIndex])
            {
                inInstructionsToProcess[insnIndex] = true;
                instructionsToProcess[numInstructionsToProcess++] = insnIndex;
            }
        }
    }
}