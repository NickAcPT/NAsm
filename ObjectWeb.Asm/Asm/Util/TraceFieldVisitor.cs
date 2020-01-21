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

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="FieldVisitor" />
    ///     that prints the fields it visits with a
    ///     <see cref="Printer" />
    ///     .
    /// </summary>
    /// <author>Eric Bruneton</author>
    public sealed class TraceFieldVisitor : FieldVisitor
    {
        /// <summary>The printer to convert the visited field into text.</summary>
        public readonly Printer p;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceFieldVisitor" />
        ///     .
        /// </summary>
        /// <param name="printer">the printer to convert the visited field into text.</param>
        public TraceFieldVisitor(Printer printer)
            : this(null, printer)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceFieldVisitor" />
        ///     .
        /// </summary>
        /// <param name="fieldVisitor">
        ///     the field visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printer">the printer to convert the visited field into text.</param>
        public TraceFieldVisitor(FieldVisitor fieldVisitor, Printer printer)
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, fieldVisitor)
        {
            // DontCheck(MemberName): can't be renamed (for backward binary compatibility).
            /* latest api = */
            p = printer;
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            var annotationPrinter = p.VisitFieldAnnotation(descriptor, visible);
            return new TraceAnnotationVisitor(base.VisitAnnotation(descriptor, visible), annotationPrinter
            );
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var annotationPrinter = p.VisitFieldTypeAnnotation(typeRef, typePath, descriptor
                , visible);
            return new TraceAnnotationVisitor(base.VisitTypeAnnotation(typeRef, typePath, descriptor
                , visible), annotationPrinter);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            p.VisitFieldAttribute(attribute);
            base.VisitAttribute(attribute);
        }

        public override void VisitEnd()
        {
            p.VisitFieldEnd();
            base.VisitEnd();
        }
    }
}