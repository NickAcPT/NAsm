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

namespace ObjectWeb.Asm
{
	/// <summary>A visitor to visit a Java annotation.</summary>
	/// <remarks>
	///     A visitor to visit a Java annotation. The methods of this class must be called in the following
	///     order: (
	///     <c>visit</c>
	///     |
	///     <c>visitEnum</c>
	///     |
	///     <c>visitAnnotation</c>
	///     |
	///     <c>visitArray</c>
	///     )
	///     <c>visitEnd</c>
	///     .
	/// </remarks>
	/// <author>Eric Bruneton</author>
	/// <author>Eugene Kuleshov</author>
	public abstract class AnnotationVisitor
    {
	    /// <summary>The ASM API version implemented by this visitor.</summary>
	    /// <remarks>
	    ///     The ASM API version implemented by this visitor. The value of this field must be one of
	    ///     <see cref="Opcodes.Asm4" />
	    ///     ,
	    ///     <see cref="Opcodes.Asm5" />
	    ///     ,
	    ///     <see cref="Opcodes.Asm6" />
	    ///     or
	    ///     <see cref="Opcodes.Asm7" />
	    ///     .
	    /// </remarks>
	    protected internal readonly VisitorAsmApiVersion api;

	    /// <summary>
	    ///     The annotation visitor to which this visitor must delegate method calls.
	    /// </summary>
	    /// <remarks>
	    ///     The annotation visitor to which this visitor must delegate method calls. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </remarks>
	    protected internal AnnotationVisitor av;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="AnnotationVisitor" />
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
	    public AnnotationVisitor(VisitorAsmApiVersion api)
            : this(api, null)
        {
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="AnnotationVisitor" />
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
	    /// <param name="annotationVisitor">
	    ///     the annotation visitor to which this visitor must delegate method
	    ///     calls. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    public AnnotationVisitor(VisitorAsmApiVersion api, AnnotationVisitor annotationVisitor)
        {
            if (api != VisitorAsmApiVersion.Asm7 && api != VisitorAsmApiVersion.Asm6 && api != VisitorAsmApiVersion
                    .Asm5 && api != VisitorAsmApiVersion.Asm4 && api != VisitorAsmApiVersion.Asm8Experimental)
                throw new ArgumentException("Unsupported api " + api);
            if (api == VisitorAsmApiVersion.Asm8Experimental) Constants.CheckAsm8Experimental(this);
            this.api = api;
            av = annotationVisitor;
        }

	    /// <summary>Visits a primitive value of the annotation.</summary>
	    /// <param name="name">the value name.</param>
	    /// <param name="value">
	    ///     the actual value, whose type must be
	    ///     <see cref="byte" />
	    ///     ,
	    ///     <see cref="bool" />
	    ///     ,
	    ///     <see cref="char" />
	    ///     ,
	    ///     <see cref="short" />
	    ///     ,
	    ///     <see cref="int" />
	    ///     ,
	    ///     <see cref="long" />
	    ///     ,
	    ///     <see cref="float" />
	    ///     ,
	    ///     <see cref="double" />
	    ///     ,
	    ///     <see cref="string" />
	    ///     or
	    ///     <see cref="Type" />
	    ///     of
	    ///     <see cref="Type.Object" />
	    ///     or
	    ///     <see cref="Type.Array" />
	    ///     sort. This
	    ///     value can also be an array of byte, boolean, short, char, int, long, float or double values
	    ///     (this is equivalent to using
	    ///     <see cref="VisitArray(string)" />
	    ///     and visiting each array element in turn,
	    ///     but is more convenient).
	    /// </param>
	    public virtual void Visit(string name, object value)
        {
            if (av != null) av.Visit(name, value);
        }

	    /// <summary>Visits an enumeration value of the annotation.</summary>
	    /// <param name="name">the value name.</param>
	    /// <param name="descriptor">the class descriptor of the enumeration class.</param>
	    /// <param name="value">the actual enumeration value.</param>
	    public virtual void VisitEnum(string name, string descriptor, string value)
        {
            if (av != null) av.VisitEnum(name, descriptor, value);
        }

	    /// <summary>Visits a nested annotation value of the annotation.</summary>
	    /// <param name="name">the value name.</param>
	    /// <param name="descriptor">the class descriptor of the nested annotation class.</param>
	    /// <returns>
	    ///     a visitor to visit the actual nested annotation value, or
	    ///     <literal>null</literal>
	    ///     if this
	    ///     visitor is not interested in visiting this nested annotation.
	    ///     <i>
	    ///         The nested annotation
	    ///         value must be fully visited before calling other methods on this annotation visitor
	    ///     </i>
	    ///     .
	    /// </returns>
	    public virtual AnnotationVisitor VisitAnnotation(string name, string descriptor)
        {
            if (av != null) return av.VisitAnnotation(name, descriptor);
            return null;
        }

	    /// <summary>Visits an array value of the annotation.</summary>
	    /// <remarks>
	    ///     Visits an array value of the annotation. Note that arrays of primitive types (such as byte,
	    ///     boolean, short, char, int, long, float or double) can be passed as value to
	    ///     <see cref="Visit(string, object)">visit</see>
	    ///     . This is what
	    ///     <see cref="ClassReader" />
	    ///     does.
	    /// </remarks>
	    /// <param name="name">the value name.</param>
	    /// <returns>
	    ///     a visitor to visit the actual array value elements, or
	    ///     <literal>null</literal>
	    ///     if this visitor
	    ///     is not interested in visiting these values. The 'name' parameters passed to the methods of
	    ///     this visitor are ignored.
	    ///     <i>
	    ///         All the array values must be visited before calling other
	    ///         methods on this annotation visitor
	    ///     </i>
	    ///     .
	    /// </returns>
	    public virtual AnnotationVisitor VisitArray(string name)
        {
            if (av != null) return av.VisitArray(name);
            return null;
        }

        /// <summary>Visits the end of the annotation.</summary>
        public virtual void VisitEnd()
        {
            if (av != null) av.VisitEnd();
        }
    }
}