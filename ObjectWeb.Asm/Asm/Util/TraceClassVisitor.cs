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

using System.IO;
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="ClassVisitor" />
    ///     that prints the classes it visits with a
    ///     <see cref="Printer" />
    ///     . This class
    ///     visitor can be used in the middle of a class visitor chain to trace the class that is visited at
    ///     a given point in this chain. This may be useful for debugging purposes.
    ///     <p>
    ///         When used with a
    ///         <see cref="Textifier" />
    ///         , the trace printed when visiting the
    ///         <c>Hello</c>
    ///         class is
    ///         the following:
    ///         <pre>
    ///             // class version 49.0 (49) // access flags 0x21 public class Hello {
    ///             // compiled from: Hello.java
    ///             // access flags 0x1
    ///             public &lt;init&gt; ()V
    ///             ALOAD 0
    ///             INVOKESPECIAL java/lang/Object &lt;init&gt; ()V
    ///             RETURN
    ///             MAXSTACK = 1 MAXLOCALS = 1
    ///             // access flags 0x9
    ///             public static main ([Ljava/lang/String;)V
    ///             GETSTATIC java/lang/System out Ljava/io/PrintStream;
    ///             LDC &quot;hello&quot;
    ///             INVOKEVIRTUAL java/io/PrintStream println (Ljava/lang/String;)V
    ///             RETURN
    ///             MAXSTACK = 2 MAXLOCALS = 1
    ///             }
    ///         </pre>
    ///         <p>
    ///             where
    ///             <c>Hello</c>
    ///             is defined by:
    ///             <pre>
    ///                 public class Hello {
    ///                 public static void main(String[] args) {
    ///                 System.out.println(&quot;hello&quot;);
    ///                 }
    ///                 }
    ///             </pre>
    /// </summary>
    /// <author>Eric Bruneton</author>
    /// <author>Eugene Kuleshov</author>
    public sealed class TraceClassVisitor : ClassVisitor
    {
        /// <summary>The printer to convert the visited class into text.</summary>
        public readonly Printer p;

        /// <summary>The print writer to be used to print the class.</summary>
        /// <remarks>
        ///     The print writer to be used to print the class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private readonly TextWriter printWriter;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceClassVisitor" />
        ///     .
        /// </summary>
        /// <param name="printWriter">
        ///     the print writer to be used to print the class. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public TraceClassVisitor(TextWriter printWriter)
            : this(null, printWriter)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceClassVisitor" />
        ///     .
        /// </summary>
        /// <param name="classVisitor">
        ///     the class visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printWriter">
        ///     the print writer to be used to print the class. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public TraceClassVisitor(ClassVisitor classVisitor, TextWriter printWriter)
            : this(classVisitor, new Textifier(), printWriter)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceClassVisitor" />
        ///     .
        /// </summary>
        /// <param name="classVisitor">
        ///     the class visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printer">the printer to convert the visited class into text.</param>
        /// <param name="printWriter">
        ///     the print writer to be used to print the class. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public TraceClassVisitor(ClassVisitor classVisitor, Printer printer, TextWriter
            printWriter)
            : base(VisitorAsmApiVersion.Asm8Experimental, classVisitor)
        {
            // DontCheck(MemberName): can't be renamed (for backward binary compatibility).
            /* latest api = */
            this.printWriter = printWriter;
            p = printer;
        }

        public override void Visit(int version, AccessFlags access, string name, string signature
            , string superName, string[] interfaces)
        {
            p.Visit(version, access, name, signature, superName, interfaces);
            base.Visit(version, access, name, signature, superName, interfaces);
        }

        public override void VisitSource(string file, string debug)
        {
            p.VisitSource(file, debug);
            base.VisitSource(file, debug);
        }

        public override ModuleVisitor VisitModule(string name, AccessFlags flags, string version)
        {
            var modulePrinter = p.VisitModule(name, flags, version);
            return new TraceModuleVisitor(base.VisitModule(name, flags, version), modulePrinter
            );
        }

        public override void VisitNestHost(string nestHost)
        {
            p.VisitNestHost(nestHost);
            base.VisitNestHost(nestHost);
        }

        public override void VisitOuterClass(string owner, string name, string descriptor
        )
        {
            p.VisitOuterClass(owner, name, descriptor);
            base.VisitOuterClass(owner, name, descriptor);
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            var annotationPrinter = p.VisitClassAnnotation(descriptor, visible);
            return new TraceAnnotationVisitor(base.VisitAnnotation(descriptor, visible), annotationPrinter
            );
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var annotationPrinter = p.VisitClassTypeAnnotation(typeRef, typePath, descriptor
                , visible);
            return new TraceAnnotationVisitor(base.VisitTypeAnnotation(typeRef, typePath, descriptor
                , visible), annotationPrinter);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            p.VisitClassAttribute(attribute);
            base.VisitAttribute(attribute);
        }

        public override void VisitNestMember(string nestMember)
        {
            p.VisitNestMember(nestMember);
            base.VisitNestMember(nestMember);
        }

        public override void VisitPermittedSubtypeExperimental(string permittedSubtype)
        {
            p.VisitPermittedSubtypeExperimental(permittedSubtype);
            base.VisitPermittedSubtypeExperimental(permittedSubtype);
        }

        public override void VisitInnerClass(string name, string outerName, string innerName
            , AccessFlags access)
        {
            p.VisitInnerClass(name, outerName, innerName, access);
            base.VisitInnerClass(name, outerName, innerName, access);
        }

        public override RecordComponentVisitor VisitRecordComponentExperimental(AccessFlags access
            , string name, string descriptor, string signature)
        {
            var recordComponentPrinter = p.VisitRecordComponentExperimental(access, name,
                descriptor, signature);
            return new TraceRecordComponentVisitor(base.VisitRecordComponentExperimental(access
                , name, descriptor, signature), recordComponentPrinter);
        }

        public override FieldVisitor VisitField(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor
            , string signature, object value)
        {
            var fieldPrinter = p.VisitField(access, name, descriptor, signature, value);
            return new TraceFieldVisitor(base.VisitField(access, name, descriptor, signature,
                value), fieldPrinter);
        }

        public override MethodVisitor VisitMethod(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor
            , string signature, string[] exceptions)
        {
            var methodPrinter = p.VisitMethod(access, name, descriptor, signature, exceptions
            );
            return new TraceMethodVisitor(base.VisitMethod(access, name, descriptor, signature
                , exceptions), methodPrinter);
        }

        public override void VisitEnd()
        {
            p.VisitClassEnd();
            if (printWriter != null)
            {
                p.Print(printWriter);
                printWriter.Flush();
            }

            base.VisitEnd();
        }
    }
}