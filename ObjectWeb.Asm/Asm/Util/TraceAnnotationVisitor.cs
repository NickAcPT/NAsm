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
    ///     An
    ///     <see cref="AnnotationVisitor" />
    ///     that prints the annotations it visits with a
    ///     <see cref="Printer" />
    ///     .
    /// </summary>
    /// <author>Eric Bruneton</author>
    public sealed class TraceAnnotationVisitor : AnnotationVisitor
    {
        /// <summary>The printer to convert the visited annotation into text.</summary>
        private readonly Printer printer;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceAnnotationVisitor" />
        ///     .
        /// </summary>
        /// <param name="printer">the printer to convert the visited annotation into text.</param>
        public TraceAnnotationVisitor(Printer printer)
            : this(null, printer)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceAnnotationVisitor" />
        ///     .
        /// </summary>
        /// <param name="annotationVisitor">
        ///     the annotation visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printer">the printer to convert the visited annotation into text.</param>
        public TraceAnnotationVisitor(AnnotationVisitor annotationVisitor, Printer printer
        )
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, annotationVisitor)
        {
            /* latest api = */
            this.printer = printer;
        }

        public override void Visit(string name, object value)
        {
            printer.Visit(name, value);
            base.Visit(name, value);
        }

        public override void VisitEnum(string name, string descriptor, string value)
        {
            printer.VisitEnum(name, descriptor, value);
            base.VisitEnum(name, descriptor, value);
        }

        public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
        {
            var annotationPrinter = printer.VisitAnnotation(name, descriptor);
            return new TraceAnnotationVisitor(base.VisitAnnotation(name, descriptor), annotationPrinter
            );
        }

        public override AnnotationVisitor VisitArray(string name)
        {
            var arrayPrinter = printer.VisitArray(name);
            return new TraceAnnotationVisitor(base.VisitArray(name), arrayPrinter);
        }

        public override void VisitEnd()
        {
            printer.VisitAnnotationEnd();
            base.VisitEnd();
        }
    }
}