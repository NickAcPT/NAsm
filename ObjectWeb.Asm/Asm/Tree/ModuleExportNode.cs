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

namespace ObjectWeb.Asm.Tree
{
	/// <summary>
	///     A node that represents an exported package with its name and the module that can access to it.
	/// </summary>
	/// <author>Remi Forax</author>
	public class ModuleExportNode
    {
	    /// <summary>
	    ///     The access flags (see
	    ///     <see cref="Opcodes" />
	    ///     ). Valid values are
	    ///     <c>ACC_SYNTHETIC</c>
	    ///     and
	    ///     <c>ACC_MANDATED</c>
	    ///     .
	    /// </summary>
	    public int access;

	    /// <summary>
	    ///     The list of modules that can access this exported package, specified with fully qualified names
	    ///     (using dots).
	    /// </summary>
	    /// <remarks>
	    ///     The list of modules that can access this exported package, specified with fully qualified names
	    ///     (using dots). May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </remarks>
	    public IList<string> modules;

        /// <summary>The internal name of the exported package.</summary>
        public string packaze;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ModuleExportNode" />
        ///     .
        /// </summary>
        /// <param name="packaze">the internal name of the exported package.</param>
        /// <param name="access">
        ///     the package access flags, one or more of
        ///     <c>ACC_SYNTHETIC</c>
        ///     and
        ///     <c>ACC_MANDATED</c>
        ///     .
        /// </param>
        /// <param name="modules">
        ///     a list of modules that can access this exported package, specified with fully
        ///     qualified names (using dots).
        /// </param>
        public ModuleExportNode(string packaze, int access, IList<string> modules)
        {
            this.packaze = packaze;
            this.access = access;
            this.modules = modules;
        }

        /// <summary>Makes the given module visitor visit this export declaration.</summary>
        /// <param name="moduleVisitor">a module visitor.</param>
        public virtual void Accept(ModuleVisitor moduleVisitor)
        {
            moduleVisitor.VisitExport(packaze, access, modules == null
                ? null
                : Collections.ToArray
                    (modules, new string[0]));
        }
    }
}