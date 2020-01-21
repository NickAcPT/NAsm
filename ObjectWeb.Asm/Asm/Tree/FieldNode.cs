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
    /// <summary>A node that represents a field.</summary>
    /// <author>Eric Bruneton</author>
    public class FieldNode : FieldVisitor
    {
        /// <summary>
        ///     The field's access flags (see
        ///     <see cref="Opcodes" />
        ///     ). This field also indicates if
        ///     the field is synthetic and/or deprecated.
        /// </summary>
        public int access;

        /// <summary>The non standard attributes of this field.</summary>
        /// <remarks>
        ///     The non standard attributes of this field. * May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<Attribute> attrs;

        /// <summary>
        ///     The field's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </summary>
        public string desc;

        /// <summary>The runtime invisible annotations of this field.</summary>
        /// <remarks>
        ///     The runtime invisible annotations of this field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<AnnotationNode> invisibleAnnotations;

        /// <summary>The runtime invisible type annotations of this field.</summary>
        /// <remarks>
        ///     The runtime invisible type annotations of this field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<TypeAnnotationNode> invisibleTypeAnnotations;

        /// <summary>The field's name.</summary>
        public string name;

        /// <summary>The field's signature.</summary>
        /// <remarks>
        ///     The field's signature. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string signature;

        /// <summary>The field's initial value.</summary>
        /// <remarks>
        ///     The field's initial value. This field, which may be
        ///     <literal>null</literal>
        ///     if the field does not have
        ///     an initial value, must be an
        ///     <see cref="int" />
        ///     , a
        ///     <see cref="float" />
        ///     , a
        ///     <see cref="long" />
        ///     , a
        ///     <see cref="double" />
        ///     or a
        ///     <see cref="string" />
        ///     .
        /// </remarks>
        public object value;

        /// <summary>The runtime visible annotations of this field.</summary>
        /// <remarks>
        ///     The runtime visible annotations of this field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<AnnotationNode> visibleAnnotations;

        /// <summary>The runtime visible type annotations of this field.</summary>
        /// <remarks>
        ///     The runtime visible type annotations of this field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<TypeAnnotationNode> visibleTypeAnnotations;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="FieldNode" />
        ///     . <i>Subclasses must not use this constructor</i>. Instead,
        ///     they must use the
        ///     <see cref="FieldNode(int, int, string, string, string, object)" />
        ///     version.
        /// </summary>
        /// <param name="access">
        ///     the field's access flags (see
        ///     <see cref="Opcodes" />
        ///     ). This parameter
        ///     also indicates if the field is synthetic and/or deprecated.
        /// </param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">
        ///     the field's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">the field's signature.</param>
        /// <param name="value">
        ///     the field's initial value. This parameter, which may be
        ///     <literal>null</literal>
        ///     if the
        ///     field does not have an initial value, must be an
        ///     <see cref="int" />
        ///     , a
        ///     <see cref="float" />
        ///     , a
        ///     <see cref="long" />
        ///     , a
        ///     <see cref="double" />
        ///     or a
        ///     <see cref="string" />
        ///     .
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public FieldNode(int access, string name, string descriptor, string signature, object
            value)
            : this(VisitorAsmApiVersion.Asm7, access, name, descriptor, signature, value)
        {
            /* latest api = */
            if (GetType() != typeof(FieldNode)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="FieldNode" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of
        ///     <see cref="Opcodes.Asm4" />
        ///     or
        ///     <see cref="Opcodes.Asm5" />
        ///     .
        /// </param>
        /// <param name="access">
        ///     the field's access flags (see
        ///     <see cref="Opcodes" />
        ///     ). This parameter
        ///     also indicates if the field is synthetic and/or deprecated.
        /// </param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">
        ///     the field's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">the field's signature.</param>
        /// <param name="value">
        ///     the field's initial value. This parameter, which may be
        ///     <literal>null</literal>
        ///     if the
        ///     field does not have an initial value, must be an
        ///     <see cref="int" />
        ///     , a
        ///     <see cref="float" />
        ///     , a
        ///     <see cref="long" />
        ///     , a
        ///     <see cref="double" />
        ///     or a
        ///     <see cref="string" />
        ///     .
        /// </param>
        public FieldNode(VisitorAsmApiVersion api, int access, string name, string descriptor, string signature
            , object value)
            : base(api)
        {
            this.access = access;
            this.name = name;
            desc = descriptor;
            this.signature = signature;
            this.value = value;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            var annotation = new AnnotationNode(descriptor);
            if (visible)
                visibleAnnotations = Util.Add(visibleAnnotations, annotation);
            else
                invisibleAnnotations = Util.Add(invisibleAnnotations, annotation);
            return annotation;
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor
            );
            if (visible)
                visibleTypeAnnotations = Util.Add(visibleTypeAnnotations, typeAnnotation);
            else
                invisibleTypeAnnotations = Util.Add(invisibleTypeAnnotations, typeAnnotation);
            return typeAnnotation;
        }

        public override void VisitAttribute(Attribute attribute)
        {
            attrs = Util.Add(attrs, attribute);
        }

        public override void VisitEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Accept methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Checks that this field node is compatible with the given ASM API version.
        /// </summary>
        /// <remarks>
        ///     Checks that this field node is compatible with the given ASM API version. This method checks
        ///     that this node, and all its children recursively, do not contain elements that were introduced
        ///     in more recent versions of the ASM API than the given version.
        /// </remarks>
        /// <param name="api">
        ///     an ASM API version. Must be one of
        ///     <see cref="Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Opcodes.Asm6" />
        ///     or
        ///     <see cref="Opcodes.Asm7" />
        ///     .
        /// </param>
        public virtual void Check(VisitorAsmApiVersion api)
        {
            if (api == VisitorAsmApiVersion.Asm4)
            {
                if (visibleTypeAnnotations != null && !(visibleTypeAnnotations.Count == 0))
                    throw new UnsupportedClassVersionException();
                if (invisibleTypeAnnotations != null && !(invisibleTypeAnnotations.Count == 0))
                    throw new UnsupportedClassVersionException();
            }
        }

        /// <summary>Makes the given class visitor visit this field.</summary>
        /// <param name="classVisitor">a class visitor.</param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            var fieldVisitor = classVisitor.VisitField(access, name, desc, signature
                , value);
            if (fieldVisitor == null) return;
            // Visit the annotations.
            if (visibleAnnotations != null)
                for (int i = 0, n = visibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = visibleAnnotations[i];
                    annotation.Accept(fieldVisitor.VisitAnnotation(annotation.desc, true));
                }

            if (invisibleAnnotations != null)
                for (int i = 0, n = invisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = invisibleAnnotations[i];
                    annotation.Accept(fieldVisitor.VisitAnnotation(annotation.desc, false));
                }

            if (visibleTypeAnnotations != null)
                for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = visibleTypeAnnotations[i];
                    typeAnnotation.Accept(fieldVisitor.VisitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation
                        .typePath, typeAnnotation.desc, true));
                }

            if (invisibleTypeAnnotations != null)
                for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = invisibleTypeAnnotations[i];
                    typeAnnotation.Accept(fieldVisitor.VisitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation
                        .typePath, typeAnnotation.desc, false));
                }

            // Visit the non standard attributes.
            if (attrs != null)
                for (int i = 0, n = attrs.Count; i < n; ++i)
                    fieldVisitor.VisitAttribute(attrs[i]);
            fieldVisitor.VisitEnd();
        }
    }
}