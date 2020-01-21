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

namespace ObjectWeb.Asm.Tree
{
	/// <summary>A node that represents a type annotation.</summary>
	/// <author>Eric Bruneton</author>
	public class TypeAnnotationNode : AnnotationNode
    {
	    /// <summary>
	    ///     The path to the annotated type argument, wildcard bound, array element type, or static outer
	    ///     type within the referenced type.
	    /// </summary>
	    /// <remarks>
	    ///     The path to the annotated type argument, wildcard bound, array element type, or static outer
	    ///     type within the referenced type. May be
	    ///     <literal>null</literal>
	    ///     if the annotation targets 'typeRef' as
	    ///     a whole.
	    /// </remarks>
	    public TypePath typePath;

	    /// <summary>A reference to the annotated type.</summary>
	    /// <remarks>
	    ///     A reference to the annotated type. See
	    ///     <see cref="TypeReference" />
	    ///     .
	    /// </remarks>
	    public int typeRef;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="AnnotationNode" />
	    ///     . <i>Subclasses must not use this constructor</i>.
	    ///     Instead, they must use the
	    ///     <see cref="TypeAnnotationNode(int, int, Org.Objectweb.Asm.TypePath, string)" />
	    ///     version.
	    /// </summary>
	    /// <param name="typeRef">
	    ///     a reference to the annotated type. See
	    ///     <see cref="Org.Objectweb.Asm.TypeReference" />
	    ///     .
	    /// </param>
	    /// <param name="typePath">
	    ///     the path to the annotated type argument, wildcard bound, array element type, or
	    ///     static inner type within 'typeRef'. May be
	    ///     <literal>null</literal>
	    ///     if the annotation targets
	    ///     'typeRef' as a whole.
	    /// </param>
	    /// <param name="descriptor">the class descriptor of the annotation class.</param>
	    /// <exception cref="System.InvalidOperationException">
	    ///     If a subclass calls this constructor.
	    /// </exception>
	    public TypeAnnotationNode(int typeRef, TypePath typePath, string descriptor)
            : this(VisitorAsmApiVersion.Asm7, typeRef, typePath, descriptor)
        {
            /* latest api = */
            if (GetType() != typeof(TypeAnnotationNode)) throw new InvalidOperationException();
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="AnnotationNode" />
	    ///     .
	    /// </summary>
	    /// <param name="api">
	    ///     the ASM API version implemented by this visitor. Must be one of
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
	    ///     ,
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
	    ///     ,
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
	    ///     or
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
	    ///     .
	    /// </param>
	    /// <param name="typeRef">
	    ///     a reference to the annotated type. See
	    ///     <see cref="Org.Objectweb.Asm.TypeReference" />
	    ///     .
	    /// </param>
	    /// <param name="typePath">
	    ///     the path to the annotated type argument, wildcard bound, array element type, or
	    ///     static inner type within 'typeRef'. May be
	    ///     <literal>null</literal>
	    ///     if the annotation targets
	    ///     'typeRef' as a whole.
	    /// </param>
	    /// <param name="descriptor">the class descriptor of the annotation class.</param>
	    public TypeAnnotationNode(VisitorAsmApiVersion api, int typeRef, TypePath typePath, string descriptor
        )
            : base(api, descriptor)
        {
            this.typeRef = typeRef;
            this.typePath = typePath;
        }
    }
}