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
	/// <summary>A visitor to visit a record component.</summary>
	/// <remarks>
	///     A visitor to visit a record component. The methods of this class must be called in the following
	///     order: (
	///     <c>visitAnnotation</c>
	///     |
	///     <c>visitTypeAnnotation</c>
	///     |
	///     <c>visitAttribute</c>
	///     )*
	///     <c>visitEnd</c>
	///     .
	/// </remarks>
	/// <author>Remi Forax</author>
	/// <author>Eric Bruneton</author>
	//  [Obsolete(@"this API is experimental.")]
    public abstract class RecordComponentVisitor
    {
	    /// <summary>The ASM API version implemented by this visitor.</summary>
	    /// <remarks>
	    ///     The ASM API version implemented by this visitor. The value of this field must be
	    ///     <see cref="Opcodes.Asm8_Experimental" />
	    ///     .
	    /// </remarks>
	    protected internal readonly VisitorAsmApiVersion api;

	    /// <summary>The record visitor to which this visitor must delegate method calls.</summary>
	    /// <remarks>
	    ///     The record visitor to which this visitor must delegate method calls. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </remarks>
	    internal RecordComponentVisitor delegate_;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="RecordComponentVisitor" />
	    ///     .
	    /// </summary>
	    /// <param name="api">
	    ///     the ASM API version implemented by this visitor. Must be
	    ///     <see cref="Opcodes.Asm8_Experimental" />
	    ///     .
	    /// </param>
	    //  [Obsolete(@"this API is experimental.")]
        public RecordComponentVisitor(VisitorAsmApiVersion api)
            : this(api, null)
        {
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="RecordComponentVisitor" />
	    ///     .
	    /// </summary>
	    /// <param name="api">
	    ///     the ASM API version implemented by this visitor. Must be
	    ///     <see cref="Opcodes.Asm8_Experimental" />
	    ///     .
	    /// </param>
	    /// <param name="recordComponentVisitor">
	    ///     the record component visitor to which this visitor must delegate
	    ///     method calls. May be null.
	    /// </param>
	    //  [Obsolete(@"this API is experimental.")]
        public RecordComponentVisitor(VisitorAsmApiVersion api, RecordComponentVisitor recordComponentVisitor
        )
        {
            /*package-private*/
            if (api != VisitorAsmApiVersion.Asm7 && api != VisitorAsmApiVersion.Asm6 && api != VisitorAsmApiVersion
                    .Asm5 && api != VisitorAsmApiVersion.Asm4 && api != VisitorAsmApiVersion.Asm8Experimental)
                throw new ArgumentException("Unsupported api " + api);
            if (api == VisitorAsmApiVersion.Asm8Experimental) Constants.CheckAsm8Experimental(this);
            this.api = api;
            delegate_ = recordComponentVisitor;
        }

	    /// <summary>The record visitor to which this visitor must delegate method calls.</summary>
	    /// <remarks>
	    ///     The record visitor to which this visitor must delegate method calls. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </remarks>
	    /// <returns>
	    ///     the record visitor to which this visitor must delegate method calls or
	    ///     <literal>null</literal>
	    ///     .
	    /// </returns>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual RecordComponentVisitor GetDelegateExperimental()
        {
            return delegate_;
        }

	    /// <summary>Visits an annotation of the record component.</summary>
	    /// <param name="descriptor">the class descriptor of the annotation class.</param>
	    /// <param name="visible">
	    ///     <literal>true</literal>
	    ///     if the annotation is visible at runtime.
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit the annotation values, or
	    ///     <literal>null</literal>
	    ///     if this visitor is not
	    ///     interested in visiting this annotation.
	    /// </returns>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual AnnotationVisitor VisitAnnotationExperimental(string descriptor, bool
            visible)
        {
            if (delegate_ != null) return delegate_.VisitAnnotationExperimental(descriptor, visible);
            return null;
        }

	    /// <summary>Visits an annotation on a type in the record component signature.</summary>
	    /// <param name="typeRef">
	    ///     a reference to the annotated type. The sort of this type reference must be
	    ///     <see cref="TypeReference.Class_Type_Parameter" />
	    ///     ,
	    ///     <see cref="TypeReference.Class_Type_Parameter_Bound" />
	    ///     or
	    ///     <see cref="TypeReference.Class_Extends" />
	    ///     . See
	    ///     <see cref="TypeReference" />
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
	    /// <param name="visible">
	    ///     <literal>true</literal>
	    ///     if the annotation is visible at runtime.
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit the annotation values, or
	    ///     <literal>null</literal>
	    ///     if this visitor is not
	    ///     interested in visiting this annotation.
	    /// </returns>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual AnnotationVisitor VisitTypeAnnotationExperimental(int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            if (delegate_ != null)
                return delegate_.VisitTypeAnnotationExperimental(typeRef, typePath, descriptor, visible
                );
            return null;
        }

	    /// <summary>Visits a non standard attribute of the record component.</summary>
	    /// <param name="attribute">an attribute.</param>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual void VisitAttributeExperimental(Attribute attribute)
        {
            if (delegate_ != null) delegate_.VisitAttributeExperimental(attribute);
        }

	    /// <summary>Visits the end of the record component.</summary>
	    /// <remarks>
	    ///     Visits the end of the record component. This method, which is the last one to be called, is
	    ///     used to inform the visitor that everything have been visited.
	    /// </remarks>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual void VisitEndExperimental()
        {
            if (delegate_ != null) delegate_.VisitEndExperimental();
        }
    }
}