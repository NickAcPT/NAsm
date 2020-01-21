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

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>
    ///     An exception thrown if a problem occurs during the analysis of a method.
    /// </summary>
    /// <author>Bing Ran</author>
    /// <author>Eric Bruneton</author>
    [Serializable]
    public class AnalyzerException : Exception
    {
        private const long serialVersionUID = 3154190448018943333L;

        /// <summary>The bytecode instruction where the analysis failed.</summary>
        [NonSerialized] public readonly AbstractInsnNode node;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnalyzerException" />
        ///     .
        /// </summary>
        /// <param name="insn">the bytecode instruction where the analysis failed.</param>
        /// <param name="message">the reason why the analysis failed.</param>
        public AnalyzerException(AbstractInsnNode insn, string message)
            : base(message)
        {
            node = insn;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnalyzerException" />
        ///     .
        /// </summary>
        /// <param name="insn">the bytecode instruction where the analysis failed.</param>
        /// <param name="message">the reason why the analysis failed.</param>
        /// <param name="cause">the cause of the failure.</param>
        public AnalyzerException(AbstractInsnNode insn, string message, Exception cause)
            : base(message, cause)
        {
            node = insn;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnalyzerException" />
        ///     .
        /// </summary>
        /// <param name="insn">the bytecode instruction where the analysis failed.</param>
        /// <param name="message">the reason why the analysis failed.</param>
        /// <param name="expected">an expected value.</param>
        /// <param name="actual">the actual value, different from the expected one.</param>
        public AnalyzerException(AbstractInsnNode insn, string message, object expected,
            Value actual)
            : base((message == null ? "Expected " : message + ": expected ") + expected + ", but found "
                   + actual)
        {
            node = insn;
        }
    }
}