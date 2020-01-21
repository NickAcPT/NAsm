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

namespace ObjectWeb.Asm.Util
{
	/// <summary>
	///     A
	///     <see cref="RecordComponentVisitor" />
	///     that checks that its methods are properly used.
	/// </summary>
	/// <author>Eric Bruneton</author>
	/// <author>Remi Forax</author>
	public class CheckRecordComponentAdapter : RecordComponentVisitor
    {
	    /// <summary>
	    ///     Whether the
	    ///     <see cref="VisitEndExperimental()" />
	    ///     method has been called.
	    /// </summary>
	    private bool visitEndCalled;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="CheckRecordComponentAdapter" />
	    ///     .
	    ///     <i>
	    ///         Subclasses must not use this
	    ///         constructor
	    ///     </i>
	    ///     . Instead, they must use the
	    ///     <see cref="CheckRecordComponentAdapter(int, Org.Objectweb.Asm.RecordComponentVisitor)
	    /// 	" />
	    ///     version.
	    /// </summary>
	    /// <param name="recordComponentVisitor">
	    ///     the record component visitor to which this adapter must delegate
	    ///     calls.
	    /// </param>
	    /// <exception cref="System.InvalidOperationException">
	    ///     If a subclass calls this constructor.
	    /// </exception>
	    public CheckRecordComponentAdapter(RecordComponentVisitor recordComponentVisitor)
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm8Experimental, recordComponentVisitor)
        {
            // TODO: add 'latest api =' comment when no longer experimental.
            if (GetType() != typeof(CheckRecordComponentAdapter)) throw new InvalidOperationException();
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="CheckRecordComponentAdapter" />
	    ///     .
	    /// </summary>
	    /// <param name="api">
	    ///     the ASM API version implemented by this visitor. Must be
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm8_Experimental" />
	    ///     .
	    /// </param>
	    /// <param name="recordComponentVisitor">
	    ///     the record component visitor to which this adapter must delegate
	    ///     calls.
	    /// </param>
	    protected internal CheckRecordComponentAdapter(VisitorAsmApiVersion api, RecordComponentVisitor recordComponentVisitor
        )
            : base(api, recordComponentVisitor)
        {
        }

        public override AnnotationVisitor VisitAnnotationExperimental(string descriptor,
            bool visible)
        {
            CheckVisitEndNotCalled();
            // Annotations can only appear in V1_5 or more classes.
            CheckMethodAdapter.CheckDescriptor(OpcodesConstants.V1_5, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitAnnotationExperimental(descriptor, visible
            ));
        }

        public override AnnotationVisitor VisitTypeAnnotationExperimental(int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            CheckVisitEndNotCalled();
            var sort = new TypeReference(typeRef).GetSort();
            if (sort != TypeReference.Field)
                throw new ArgumentException("Invalid type reference sort 0x" + sort.ToString("x8"));
            CheckClassAdapter.CheckTypeRef(typeRef);
            CheckMethodAdapter.CheckDescriptor(OpcodesConstants.V1_5, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitTypeAnnotationExperimental(typeRef, typePath
                , descriptor, visible));
        }

        public override void VisitAttributeExperimental(Attribute attribute)
        {
            CheckVisitEndNotCalled();
            if (attribute == null) throw new ArgumentException("Invalid attribute (must not be null)");
            base.VisitAttributeExperimental(attribute);
        }

        public override void VisitEndExperimental()
        {
            CheckVisitEndNotCalled();
            visitEndCalled = true;
            base.VisitEndExperimental();
        }

        private void CheckVisitEndNotCalled()
        {
            if (visitEndCalled)
                throw new InvalidOperationException("Cannot call a visit method after visitEnd has been called"
                );
        }
    }
}