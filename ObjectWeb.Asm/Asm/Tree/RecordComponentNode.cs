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
using System.Collections.Generic;
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Tree
{
    /// <summary>A node that represents a record component.</summary>
    /// <author>Remi Forax</author>
    // [Obsolete(@"this API is experimental.")]
    public class RecordComponentNode : RecordComponentVisitor
    {
        /// <summary>
        ///     The record component access flags (see
        ///     <see cref="Opcodes" />
        ///     ). The only valid value
        ///     is
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     .
        /// </summary>
        // [Obsolete(@"this API is experimental.")]
        public int accessExperimental;

        /// <summary>The non standard attributes of this record component.</summary>
        /// <remarks>
        ///     The non standard attributes of this record component. * May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        // [Obsolete(@"this API is experimental.")]
        public List<Attribute> attrsExperimental;

        /// <summary>
        ///     The record component descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </summary>
        // [Obsolete(@"this API is experimental.")]
        public string descriptorExperimental;

        /// <summary>The runtime invisible annotations of this record component.</summary>
        /// <remarks>
        ///     The runtime invisible annotations of this record component. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        // [Obsolete(@"this API is experimental.")]
        public List<AnnotationNode> invisibleAnnotationsExperimental;

        /// <summary>The runtime invisible type annotations of this record component.</summary>
        /// <remarks>
        ///     The runtime invisible type annotations of this record component. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        // [Obsolete(@"this API is experimental.")]
        public List<TypeAnnotationNode> invisibleTypeAnnotationsExperimental;

        /// <summary>The record component name.</summary>
        // [Obsolete(@"this API is experimental.")]
        public string nameExperimental;

        /// <summary>The record component signature.</summary>
        /// <remarks>
        ///     The record component signature. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        // [Obsolete(@"this API is experimental.")]
        public string signatureExperimental;

        /// <summary>The runtime visible annotations of this record component.</summary>
        /// <remarks>
        ///     The runtime visible annotations of this record component. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        // [Obsolete(@"this API is experimental.")]
        public List<AnnotationNode> visibleAnnotationsExperimental;

        /// <summary>The runtime visible type annotations of this record component.</summary>
        /// <remarks>
        ///     The runtime visible type annotations of this record component. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        // [Obsolete(@"this API is experimental.")]
        public List<TypeAnnotationNode> visibleTypeAnnotationsExperimental;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="RecordComponentNode" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="RecordComponentNode(int, int, string, string, string)" />
        ///     version.
        /// </summary>
        /// <param name="access">
        ///     the record component access flags (see
        ///     <see cref="Opcodes" />
        ///     ). The
        ///     only valid value is
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     .
        /// </param>
        /// <param name="name">the record component name.</param>
        /// <param name="descriptor">
        ///     the record component descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">the record component signature.</param>
        /// <exception cref="InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        // [Obsolete(@"this API is experimental.")]
        public RecordComponentNode(int access, string name, string descriptor, string signature
        )
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, access, name, descriptor, signature)
        {
            /* latest api = */
            if (GetType() != typeof(RecordComponentNode)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="RecordComponentNode" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be
        ///     <see cref="Opcodes.Asm8_Experimental" />
        ///     .
        /// </param>
        /// <param name="access">
        ///     the record component access flags (see
        ///     <see cref="Opcodes" />
        ///     ). The
        ///     only valid value is
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     .
        /// </param>
        /// <param name="name">the record component name.</param>
        /// <param name="descriptor">
        ///     the record component descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">the record component signature.</param>
        // [Obsolete(@"this API is experimental.")]
        public RecordComponentNode(VisitorAsmApiVersion api, int access, string name, string descriptor, string
            signature)
            : base(api)
        {
            accessExperimental = access;
            nameExperimental = name;
            descriptorExperimental = descriptor;
            signatureExperimental = signature;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override AnnotationVisitor VisitAnnotationExperimental(string descriptor,
            bool visible)
        {
            var annotation = new AnnotationNode(descriptor);
            if (visible)
                visibleAnnotationsExperimental = Util.Add(visibleAnnotationsExperimental, annotation
                );
            else
                invisibleAnnotationsExperimental = Util.Add(invisibleAnnotationsExperimental, annotation
                );
            return annotation;
        }

        public override AnnotationVisitor VisitTypeAnnotationExperimental(int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor
            );
            if (visible)
                visibleTypeAnnotationsExperimental = Util.Add(visibleTypeAnnotationsExperimental,
                    typeAnnotation);
            else
                invisibleTypeAnnotationsExperimental = Util.Add(invisibleTypeAnnotationsExperimental
                    , typeAnnotation);
            return typeAnnotation;
        }

        public override void VisitAttributeExperimental(Attribute attribute)
        {
            attrsExperimental = Util.Add(attrsExperimental, attribute);
        }

        public override void VisitEndExperimental()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Accept methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Checks that this record component node is compatible with the given ASM API version.
        /// </summary>
        /// <remarks>
        ///     Checks that this record component node is compatible with the given ASM API version. This
        ///     method checks that this node, and all its children recursively, do not contain elements that
        ///     were introduced in more recent versions of the ASM API than the given version.
        /// </remarks>
        /// <param name="api">
        ///     an ASM API version. Must be
        ///     <see cref="Opcodes.Asm8_Experimental" />
        ///     .
        /// </param>
        // [Obsolete(@"this API is experimental.")]
        public virtual void CheckExperimental(VisitorAsmApiVersion api)
        {
            if (api != ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm8Experimental) throw new UnsupportedClassVersionException();
        }

        /// <summary>Makes the given class visitor visit this record component.</summary>
        /// <param name="classVisitor">a class visitor.</param>
        // [Obsolete(@"this API is experimental.")]
        public virtual void AcceptExperimental(ClassVisitor classVisitor)
        {
            var recordComponentVisitor = classVisitor.VisitRecordComponentExperimental
            (accessExperimental, nameExperimental, descriptorExperimental, signatureExperimental
            );
            if (recordComponentVisitor == null) return;
            // Visit the annotations.
            if (visibleAnnotationsExperimental != null)
                for (int i = 0, n = visibleAnnotationsExperimental.Count; i < n; ++i)
                {
                    var annotation = visibleAnnotationsExperimental[i];
                    annotation.Accept(recordComponentVisitor.VisitAnnotationExperimental(annotation.desc
                        , true));
                }

            if (invisibleAnnotationsExperimental != null)
                for (int i = 0, n = invisibleAnnotationsExperimental.Count; i < n; ++i)
                {
                    var annotation = invisibleAnnotationsExperimental[i];
                    annotation.Accept(recordComponentVisitor.VisitAnnotationExperimental(annotation.desc
                        , false));
                }

            if (visibleTypeAnnotationsExperimental != null)
                for (int i = 0, n = visibleTypeAnnotationsExperimental.Count; i < n; ++i)
                {
                    var typeAnnotation = visibleTypeAnnotationsExperimental[i];
                    typeAnnotation.Accept(recordComponentVisitor.VisitTypeAnnotationExperimental(typeAnnotation
                        .typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
                }

            if (invisibleTypeAnnotationsExperimental != null)
                for (int i = 0, n = invisibleTypeAnnotationsExperimental.Count; i < n; ++i)
                {
                    var typeAnnotation = invisibleTypeAnnotationsExperimental[i];
                    typeAnnotation.Accept(recordComponentVisitor.VisitTypeAnnotationExperimental(typeAnnotation
                        .typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
                }

            // Visit the non standard attributes.
            if (attrsExperimental != null)
                for (int i = 0, n = attrsExperimental.Count; i < n; ++i)
                    recordComponentVisitor.VisitAttributeExperimental(attrsExperimental[i]);
            recordComponentVisitor.VisitEndExperimental();
        }
    }
}