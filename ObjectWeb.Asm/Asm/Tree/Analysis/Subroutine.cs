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

using System.Collections.Generic;

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>A method subroutine (corresponds to a JSR instruction).</summary>
    /// <author>Eric Bruneton</author>
    internal sealed class Subroutine
    {
        /// <summary>The JSR instructions that jump to this subroutine.</summary>
        internal readonly IList<JumpInsnNode> callers;

        /// <summary>The local variables that are read or written by this subroutine.</summary>
        /// <remarks>
        ///     The local variables that are read or written by this subroutine. The i-th element is true if
        ///     and only if the local variable at index i is read or written by this subroutine.
        /// </remarks>
        internal readonly bool[] localsUsed;

        /// <summary>The start of this subroutine.</summary>
        internal readonly LabelNode start;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Subroutine" />
        ///     .
        /// </summary>
        /// <param name="start">the start of this subroutine.</param>
        /// <param name="maxLocals">
        ///     the local variables that are read or written by this subroutine.
        /// </param>
        /// <param name="caller">a JSR instruction that jump to this subroutine.</param>
        internal Subroutine(LabelNode start, int maxLocals, JumpInsnNode caller)
        {
            this.start = start;
            localsUsed = new bool[maxLocals];
            callers = new List<JumpInsnNode>();
            callers.Add(caller);
        }

        /// <summary>
        ///     Constructs a copy of the given
        ///     <see cref="Subroutine" />
        ///     .
        /// </summary>
        /// <param name="subroutine">the subroutine to copy.</param>
        internal Subroutine(Subroutine subroutine)
        {
            start = subroutine.start;
            localsUsed = (bool[]) subroutine.localsUsed.Clone();
            callers = new List<JumpInsnNode>(subroutine.callers);
        }

        /// <summary>Merges the given subroutine into this subroutine.</summary>
        /// <remarks>
        ///     Merges the given subroutine into this subroutine. The local variables read or written by the
        ///     given subroutine are marked as read or written by this one, and the callers of the given
        ///     subroutine are added as callers of this one (if both have the same start).
        /// </remarks>
        /// <param name="subroutine">
        ///     another subroutine. This subroutine is left unchanged by this method.
        /// </param>
        /// <returns>whether this subroutine has been modified by this method.</returns>
        public bool Merge(Subroutine subroutine)
        {
            var changed = false;
            for (var i = 0; i < localsUsed.Length; ++i)
                if (subroutine.localsUsed[i] && !localsUsed[i])
                {
                    localsUsed[i] = true;
                    changed = true;
                }

            if (subroutine.start == start)
                for (var i = 0; i < subroutine.callers.Count; ++i)
                {
                    var caller = subroutine.callers[i];
                    if (!callers.Contains(caller))
                    {
                        callers.Add(caller);
                        changed = true;
                    }
                }

            return changed;
        }
    }
}