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
using System.Collections;
using System.Collections.Generic;

namespace ObjectWeb.Asm.Tree
{
    /// <summary>
    ///     A doubly linked list of
    ///     <see cref="AbstractInsnNode" />
    ///     objects.
    ///     <i>
    ///         This implementation is not thread
    ///         safe
    ///     </i>
    ///     .
    /// </summary>
    public class InsnList : IEnumerable<AbstractInsnNode>
    {
        /// <summary>A cache of the instructions of this list.</summary>
        /// <remarks>
        ///     A cache of the instructions of this list. This cache is used to improve the performance of the
        ///     <see cref="Get(int)" />
        ///     method.
        /// </remarks>
        internal AbstractInsnNode[] cache;

        /// <summary>The first instruction in this list.</summary>
        /// <remarks>
        ///     The first instruction in this list. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AbstractInsnNode firstInsn;

        /// <summary>The last instruction in this list.</summary>
        /// <remarks>
        ///     The last instruction in this list. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AbstractInsnNode lastInsn;

        private int size;

        /// <summary>The number of instructions in this list.</summary>
        private int size__;

        public IEnumerator<AbstractInsnNode> GetEnumerator()
        {
            return (IEnumerator<AbstractInsnNode>) cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Returns the number of instructions in this list.</summary>
        /// <returns>the number of instructions in this list.</returns>
        public virtual int Size()
        {
            return size__;
        }

        /// <summary>Returns the first instruction in this list.</summary>
        /// <returns>
        ///     the first instruction in this list, or
        ///     <literal>null</literal>
        ///     if the list is empty.
        /// </returns>
        public virtual AbstractInsnNode GetFirst()
        {
            return firstInsn;
        }

        /// <summary>Returns the last instruction in this list.</summary>
        /// <returns>
        ///     the last instruction in this list, or
        ///     <literal>null</literal>
        ///     if the list is empty.
        /// </returns>
        public virtual AbstractInsnNode GetLast()
        {
            return lastInsn;
        }

        /// <summary>Returns the instruction whose index is given.</summary>
        /// <remarks>
        ///     Returns the instruction whose index is given. This method builds a cache of the instructions in
        ///     this list to avoid scanning the whole list each time it is called. Once the cache is built,
        ///     this method runs in constant time. This cache is invalidated by all the methods that modify the
        ///     list.
        /// </remarks>
        /// <param name="index">the index of the instruction that must be returned.</param>
        /// <returns>the instruction whose index is given.</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     if (index &lt; 0 || index &gt;= size()).
        /// </exception>
        public virtual AbstractInsnNode Get(int index)
        {
            if (index < 0 || index >= size__) throw new IndexOutOfRangeException();
            if (cache == null) cache = ToArray();
            return cache[index];
        }

        /// <summary>
        ///     Returns
        ///     <literal>true</literal>
        ///     if the given instruction belongs to this list. This method always scans
        ///     the instructions of this list until it finds the given instruction or reaches the end of the
        ///     list.
        /// </summary>
        /// <param name="insnNode">an instruction.</param>
        /// <returns>
        ///     <literal>true</literal>
        ///     if the given instruction belongs to this list.
        /// </returns>
        public virtual bool Contains(AbstractInsnNode insnNode)
        {
            var currentInsn = firstInsn;
            while (currentInsn != null && currentInsn != insnNode) currentInsn = currentInsn.nextInsn;
            return currentInsn != null;
        }

        /// <summary>Returns the index of the given instruction in this list.</summary>
        /// <remarks>
        ///     Returns the index of the given instruction in this list. This method builds a cache of the
        ///     instruction indexes to avoid scanning the whole list each time it is called. Once the cache is
        ///     built, this method run in constant time. The cache is invalidated by all the methods that
        ///     modify the list.
        /// </remarks>
        /// <param name="insnNode">an instruction <i>of this list</i>.</param>
        /// <returns>
        ///     the index of the given instruction in this list.
        ///     <i>
        ///         The result of this method is
        ///         undefined if the given instruction does not belong to this list
        ///     </i>
        ///     . Use
        ///     <see cref="Contains(AbstractInsnNode)"></see>
        ///     to test if an instruction belongs to an instruction list or not.
        /// </returns>
        public virtual int IndexOf(AbstractInsnNode insnNode)
        {
            if (cache == null) cache = ToArray();
            return insnNode.index;
        }

        /// <summary>Makes the given visitor visit all the instructions in this list.</summary>
        /// <param name="methodVisitor">the method visitor that must visit the instructions.</param>
        public virtual void Accept(MethodVisitor methodVisitor)
        {
            var currentInsn = firstInsn;
            while (currentInsn != null)
            {
                currentInsn.Accept(methodVisitor);
                currentInsn = currentInsn.nextInsn;
            }
        }

        /// <summary>Returns an iterator over the instructions in this list.</summary>
        /// <param name="index">index of instruction for the iterator to start at.</param>
        /// <returns>an iterator over the instructions in this list.</returns>
        /// <summary>Returns an array containing all the instructions in this list.</summary>
        /// <returns>an array containing all the instructions in this list.</returns>
        public virtual AbstractInsnNode[] ToArray()
        {
            var currentInsnIndex = 0;
            var currentInsn = firstInsn;
            var insnNodeArray = new AbstractInsnNode[size__];
            while (currentInsn != null)
            {
                insnNodeArray[currentInsnIndex] = currentInsn;
                currentInsn.index = currentInsnIndex++;
                currentInsn = currentInsn.nextInsn;
            }

            return insnNodeArray;
        }

        /// <summary>Replaces an instruction of this list with another instruction.</summary>
        /// <param name="oldInsnNode">an instruction <i>of this list</i>.</param>
        /// <param name="newInsnNode">
        ///     another instruction,
        ///     <i>
        ///         which must not belong to any
        ///         <see cref="InsnList" />
        ///     </i>
        ///     .
        /// </param>
        public virtual void Set(AbstractInsnNode oldInsnNode, AbstractInsnNode newInsnNode
        )
        {
            var nextInsn = oldInsnNode.nextInsn;
            newInsnNode.nextInsn = nextInsn;
            if (nextInsn != null)
                nextInsn.previousInsn = newInsnNode;
            else
                lastInsn = newInsnNode;
            var previousInsn = oldInsnNode.previousInsn;
            newInsnNode.previousInsn = previousInsn;
            if (previousInsn != null)
                previousInsn.nextInsn = newInsnNode;
            else
                firstInsn = newInsnNode;
            if (cache != null)
            {
                var index = oldInsnNode.index;
                cache[index] = newInsnNode;
                newInsnNode.index = index;
            }
            else
            {
                newInsnNode.index = 0;
            }

            // newInsnNode now belongs to an InsnList.
            oldInsnNode.index = -1;
            // oldInsnNode no longer belongs to an InsnList.
            oldInsnNode.previousInsn = null;
            oldInsnNode.nextInsn = null;
        }

        /// <summary>Adds the given instruction to the end of this list.</summary>
        /// <param name="insnNode">
        ///     an instruction,
        ///     <i>
        ///         which must not belong to any
        ///         <see cref="InsnList" />
        ///     </i>
        ///     .
        /// </param>
        public virtual void Add(AbstractInsnNode insnNode)
        {
            ++size__;
            if (lastInsn == null)
            {
                firstInsn = insnNode;
                lastInsn = insnNode;
            }
            else
            {
                lastInsn.nextInsn = insnNode;
                insnNode.previousInsn = lastInsn;
            }

            lastInsn = insnNode;
            cache = null;
            insnNode.index = 0;
        }

        // insnNode now belongs to an InsnList.
        /// <summary>Adds the given instructions to the end of this list.</summary>
        /// <param name="insnList">
        ///     an instruction list, which is cleared during the process. This list must be
        ///     different from 'this'.
        /// </param>
        public virtual void Add(InsnList insnList)
        {
            if (insnList.Size() == 0) return;
            size__ += insnList.Size();
            if (lastInsn == null)
            {
                firstInsn = insnList.firstInsn;
                lastInsn = insnList.lastInsn;
            }
            else
            {
                var firstInsnListElement = insnList.firstInsn;
                lastInsn.nextInsn = firstInsnListElement;
                firstInsnListElement.previousInsn = lastInsn;
                lastInsn = insnList.lastInsn;
            }

            cache = null;
            insnList.RemoveAll(false);
        }

        /// <summary>Inserts the given instruction at the beginning of this list.</summary>
        /// <param name="insnNode">
        ///     an instruction,
        ///     <i>
        ///         which must not belong to any
        ///         <see cref="InsnList" />
        ///     </i>
        ///     .
        /// </param>
        public virtual void Insert(AbstractInsnNode insnNode)
        {
            ++size__;
            if (firstInsn == null)
            {
                firstInsn = insnNode;
                lastInsn = insnNode;
            }
            else
            {
                firstInsn.previousInsn = insnNode;
                insnNode.nextInsn = firstInsn;
            }

            firstInsn = insnNode;
            cache = null;
            insnNode.index = 0;
        }

        // insnNode now belongs to an InsnList.
        /// <summary>Inserts the given instructions at the beginning of this list.</summary>
        /// <param name="insnList">
        ///     an instruction list, which is cleared during the process. This list must be
        ///     different from 'this'.
        /// </param>
        public virtual void Insert(InsnList insnList)
        {
            if (insnList.size == 0) return;
            size__ += insnList.size;
            if (firstInsn == null)
            {
                firstInsn = insnList.firstInsn;
                lastInsn = insnList.lastInsn;
            }
            else
            {
                var lastInsnListElement = insnList.lastInsn;
                firstInsn.previousInsn = lastInsnListElement;
                lastInsnListElement.nextInsn = firstInsn;
                firstInsn = insnList.firstInsn;
            }

            cache = null;
            insnList.RemoveAll(false);
        }

        /// <summary>Inserts the given instruction after the specified instruction.</summary>
        /// <param name="previousInsn">
        ///     an instruction <i>of this list</i> after which insnNode must be inserted.
        /// </param>
        /// <param name="insnNode">
        ///     the instruction to be inserted,
        ///     <i>
        ///         which must not belong to any
        ///         <see cref="InsnList" />
        ///     </i>
        ///     .
        /// </param>
        public virtual void Insert(AbstractInsnNode previousInsn, AbstractInsnNode insnNode
        )
        {
            ++size__;
            var nextInsn = previousInsn.nextInsn;
            if (nextInsn == null)
                lastInsn = insnNode;
            else
                nextInsn.previousInsn = insnNode;
            previousInsn.nextInsn = insnNode;
            insnNode.nextInsn = nextInsn;
            insnNode.previousInsn = previousInsn;
            cache = null;
            insnNode.index = 0;
        }

        // insnNode now belongs to an InsnList.
        /// <summary>Inserts the given instructions after the specified instruction.</summary>
        /// <param name="previousInsn">
        ///     an instruction <i>of this list</i> after which the instructions must be
        ///     inserted.
        /// </param>
        /// <param name="insnList">
        ///     the instruction list to be inserted, which is cleared during the process. This
        ///     list must be different from 'this'.
        /// </param>
        public virtual void Insert(AbstractInsnNode previousInsn, InsnList insnList)
        {
            if (insnList.size == 0) return;
            size__ += insnList.size;
            var firstInsnListElement = insnList.firstInsn;
            var lastInsnListElement = insnList.lastInsn;
            var nextInsn = previousInsn.nextInsn;
            if (nextInsn == null)
                lastInsn = lastInsnListElement;
            else
                nextInsn.previousInsn = lastInsnListElement;
            previousInsn.nextInsn = firstInsnListElement;
            lastInsnListElement.nextInsn = nextInsn;
            firstInsnListElement.previousInsn = previousInsn;
            cache = null;
            insnList.RemoveAll(false);
        }

        /// <summary>Inserts the given instruction before the specified instruction.</summary>
        /// <param name="nextInsn">
        ///     an instruction <i>of this list</i> before which insnNode must be inserted.
        /// </param>
        /// <param name="insnNode">
        ///     the instruction to be inserted,
        ///     <i>
        ///         which must not belong to any
        ///         <see cref="InsnList" />
        ///     </i>
        ///     .
        /// </param>
        public virtual void InsertBefore(AbstractInsnNode nextInsn, AbstractInsnNode insnNode
        )
        {
            ++size__;
            var previousInsn = nextInsn.previousInsn;
            if (previousInsn == null)
                firstInsn = insnNode;
            else
                previousInsn.nextInsn = insnNode;
            nextInsn.previousInsn = insnNode;
            insnNode.nextInsn = nextInsn;
            insnNode.previousInsn = previousInsn;
            cache = null;
            insnNode.index = 0;
        }

        // insnNode now belongs to an InsnList.
        /// <summary>Inserts the given instructions before the specified instruction.</summary>
        /// <param name="nextInsn">
        ///     an instruction <i>of this list</i> before which the instructions must be
        ///     inserted.
        /// </param>
        /// <param name="insnList">
        ///     the instruction list to be inserted, which is cleared during the process. This
        ///     list must be different from 'this'.
        /// </param>
        public virtual void InsertBefore(AbstractInsnNode nextInsn, InsnList insnList)
        {
            if (insnList.size == 0) return;
            size__ += insnList.size;
            var firstInsnListElement = insnList.firstInsn;
            var lastInsnListElement = insnList.lastInsn;
            var previousInsn = nextInsn.previousInsn;
            if (previousInsn == null)
                firstInsn = firstInsnListElement;
            else
                previousInsn.nextInsn = firstInsnListElement;
            nextInsn.previousInsn = lastInsnListElement;
            lastInsnListElement.nextInsn = nextInsn;
            firstInsnListElement.previousInsn = previousInsn;
            cache = null;
            insnList.RemoveAll(false);
        }

        /// <summary>Removes the given instruction from this list.</summary>
        /// <param name="insnNode">the instruction <i>of this list</i> that must be removed.</param>
        public virtual void Remove(AbstractInsnNode insnNode)
        {
            --size__;
            var nextInsn = insnNode.nextInsn;
            var previousInsn = insnNode.previousInsn;
            if (nextInsn == null)
            {
                if (previousInsn == null)
                {
                    firstInsn = null;
                    lastInsn = null;
                }
                else
                {
                    previousInsn.nextInsn = null;
                    lastInsn = previousInsn;
                }
            }
            else if (previousInsn == null)
            {
                firstInsn = nextInsn;
                nextInsn.previousInsn = null;
            }
            else
            {
                previousInsn.nextInsn = nextInsn;
                nextInsn.previousInsn = previousInsn;
            }

            cache = null;
            insnNode.index = -1;
            // insnNode no longer belongs to an InsnList.
            insnNode.previousInsn = null;
            insnNode.nextInsn = null;
        }

        /// <summary>Removes all the instructions of this list.</summary>
        /// <param name="mark">
        ///     if the instructions must be marked as no longer belonging to any
        ///     <see cref="InsnList" />
        ///     .
        /// </param>
        internal virtual void RemoveAll(bool mark)
        {
            if (mark)
            {
                var currentInsn = firstInsn;
                while (currentInsn != null)
                {
                    var next = currentInsn.nextInsn;
                    currentInsn.index = -1;
                    // currentInsn no longer belongs to an InsnList.
                    currentInsn.previousInsn = null;
                    currentInsn.nextInsn = null;
                    currentInsn = next;
                }
            }

            size__ = 0;
            firstInsn = null;
            lastInsn = null;
            cache = null;
        }

        /// <summary>Removes all the instructions of this list.</summary>
        public virtual void Clear()
        {
            RemoveAll(false);
        }

        /// <summary>Resets all the labels in the instruction list.</summary>
        /// <remarks>
        ///     Resets all the labels in the instruction list. This method should be called before reusing an
        ///     instruction list between several <code>ClassWriter</code>s.
        /// </remarks>
        public virtual void ResetLabels()
        {
            var currentInsn = firstInsn;
            while (currentInsn != null)
            {
                if (currentInsn is LabelNode) ((LabelNode) currentInsn).ResetLabel();
                currentInsn = currentInsn.nextInsn;
            }
        }
    }
}