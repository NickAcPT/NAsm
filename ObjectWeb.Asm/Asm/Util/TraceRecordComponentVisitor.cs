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
    ///     that prints the record components it visits with a
    ///     <see cref="Printer" />
    ///     .
    /// </summary>
    /// <author>Remi Forax</author>
    [Obsolete(@"This is an experimental API.")]
    public sealed class TraceRecordComponentVisitor : RecordComponentVisitor
    {
        /// <summary>The printer to convert the visited record component into text.</summary>
        [Obsolete(@"This is an experimental API.")]
        public readonly Printer printerExperimental;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceRecordComponentVisitor" />
        ///     .
        /// </summary>
        /// <param name="printer">
        ///     the printer to convert the visited record component into text.
        /// </param>
        [Obsolete(@"This is an experimental API.")]
        public TraceRecordComponentVisitor(Printer printer)
            : this(null, printer)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceRecordComponentVisitor" />
        ///     .
        /// </summary>
        /// <param name="recordComponentVisitor">
        ///     the record component visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printer">
        ///     the printer to convert the visited record component into text.
        /// </param>
        [Obsolete(@"This is an experimental API.")]
        public TraceRecordComponentVisitor(RecordComponentVisitor recordComponentVisitor,
            Printer printer)
            : base(VisitorAsmApiVersion.Asm8Experimental, recordComponentVisitor)
        {
            // TODO: add 'latest api =' comment when no longer experimental.
            printerExperimental = printer;
        }

        public override AnnotationVisitor VisitAnnotationExperimental(string descriptor,
            bool visible)
        {
            var annotationPrinter = printerExperimental.VisitRecordComponentAnnotationExperimental
                (descriptor, visible);
            return new TraceAnnotationVisitor(base.VisitAnnotationExperimental(descriptor, visible
            ), annotationPrinter);
        }

        public override AnnotationVisitor VisitTypeAnnotationExperimental(int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            var annotationPrinter = printerExperimental.VisitRecordComponentTypeAnnotationExperimental
                (typeRef, typePath, descriptor, visible);
            return new TraceAnnotationVisitor(base.VisitTypeAnnotationExperimental(typeRef, typePath
                , descriptor, visible), annotationPrinter);
        }

        public override void VisitAttributeExperimental(Attribute attribute)
        {
            printerExperimental.VisitRecordComponentAttributeExperimental(attribute);
            base.VisitAttributeExperimental(attribute);
        }

        public override void VisitEndExperimental()
        {
            printerExperimental.VisitRecordComponentEndExperimental();
            base.VisitEndExperimental();
        }
    }
}