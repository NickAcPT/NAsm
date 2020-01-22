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
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Tree
{
	/// <summary>
	///     A node that represents an opened package with its name and the module that can access it.
	/// </summary>
	/// <author>Remi Forax</author>
	public class ModuleOpenNode
    {
	    /// <summary>
	    ///     The access flag of the opened package, valid values are among
	    ///     <c>ACC_SYNTHETIC</c>
	    ///     and
	    ///     <c>ACC_MANDATED</c>
	    ///     .
	    /// </summary>
	    public AccessFlags access;

	    /// <summary>
	    ///     The fully qualified names (using dots) of the modules that can use deep reflection to the
	    ///     classes of the open package, or
	    ///     <literal>null</literal>
	    ///     .
	    /// </summary>
	    public IList<string> modules;

        /// <summary>The internal name of the opened package.</summary>
        public string packaze;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ModuleOpenNode" />
        ///     .
        /// </summary>
        /// <param name="packaze">the internal name of the opened package.</param>
        /// <param name="access">
        ///     the access flag of the opened package, valid values are among
        ///     <c>ACC_SYNTHETIC</c>
        ///     and
        ///     <c>ACC_MANDATED</c>
        ///     .
        /// </param>
        /// <param name="modules">
        ///     the fully qualified names (using dots) of the modules that can use deep
        ///     reflection to the classes of the open package, or
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public ModuleOpenNode(string packaze, AccessFlags access, IList<string> modules)
        {
            this.packaze = packaze;
            this.access = access;
            this.modules = modules;
        }

        /// <summary>Makes the given module visitor visit this opened package.</summary>
        /// <param name="moduleVisitor">a module visitor.</param>
        public virtual void Accept(ModuleVisitor moduleVisitor)
        {
            moduleVisitor.VisitOpen(packaze, access, modules == null
                ? null
                : Collections.ToArray
                    (modules, new string[0]));
        }
    }
}