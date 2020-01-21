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
using System.Collections;
using System.Collections.Generic;
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Tree
{
    /// <summary>A node that represents an annotation.</summary>
    /// <author>Eric Bruneton</author>
    public class AnnotationNode : AnnotationVisitor
    {
        /// <summary>The class descriptor of the annotation class.</summary>
        public string desc;

        /// <summary>The name value pairs of this annotation.</summary>
        /// <remarks>
        ///     The name value pairs of this annotation. Each name value pair is stored as two consecutive
        ///     elements in the list. The name is a
        ///     <see cref="string" />
        ///     , and the value may be a
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
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     , or a two elements String
        ///     array (for enumeration values), an
        ///     <see cref="AnnotationNode" />
        ///     , or a
        ///     <see cref="Sharpen.System.Collections.IList{E}" />
        ///     of values of one
        ///     of the preceding types. The list may be
        ///     <literal>null</literal>
        ///     if there is no name value pair.
        /// </remarks>
        public IList<object> values;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnnotationNode" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="AnnotationNode(int, string)" />
        ///     version.
        /// </summary>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public AnnotationNode(string descriptor)
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, descriptor)
        {
            /* latest api = */
            if (GetType() != typeof(AnnotationNode)) throw new InvalidOperationException();
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
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        public AnnotationNode(VisitorAsmApiVersion api, string descriptor)
            : base(api)
        {
            desc = descriptor;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnnotationNode" />
        ///     to visit an array value.
        /// </summary>
        /// <param name="values">where the visited values must be stored.</param>
        internal AnnotationNode(IList<object> values)
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            this.values = values;
        }

        // ------------------------------------------------------------------------
        // Implementation of the AnnotationVisitor abstract class
        // ------------------------------------------------------------------------
        public override void Visit(string name, object value)
        {
            if (values == null) values = new List<object>(desc != null ? 2 : 1);
            if (desc != null) values.Add(name);
            if (value is byte[])
                values.Add(Util.AsArrayList((byte[]) value));
            else if (value is bool[])
                values.Add(Util.AsArrayList((bool[]) value));
            else if (value is short[])
                values.Add(Util.AsArrayList((short[]) value));
            else if (value is char[])
                values.Add(Util.AsArrayList((char[]) value));
            else if (value is int[])
                values.Add(Util.AsArrayList((int[]) value));
            else if (value is long[])
                values.Add(Util.AsArrayList((long[]) value));
            else if (value is float[])
                values.Add(Util.AsArrayList((float[]) value));
            else if (value is double[])
                values.Add(Util.AsArrayList((double[]) value));
            else
                values.Add(value);
        }

        public override void VisitEnum(string name, string descriptor, string value)
        {
            if (values == null) values = new List<object>(desc != null ? 2 : 1);
            if (desc != null) values.Add(name);
            values.Add(new[] {descriptor, value});
        }

        public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
        {
            if (values == null) values = new List<object>(desc != null ? 2 : 1);
            if (desc != null) values.Add(name);
            var annotation = new AnnotationNode(descriptor);
            values.Add(annotation);
            return annotation;
        }

        public override AnnotationVisitor VisitArray(string name)
        {
            if (values == null) values = new List<object>(desc != null ? 2 : 1);
            if (desc != null) values.Add(name);
            IList<object> array = new List<object>();
            values.Add(array);
            return new AnnotationNode(array);
        }

        public override void VisitEnd()
        {
        }

        // Nothing to do.
        // ------------------------------------------------------------------------
        // Accept methods
        // ------------------------------------------------------------------------
        /// <summary>
        ///     Checks that this annotation node is compatible with the given ASM API version.
        /// </summary>
        /// <remarks>
        ///     Checks that this annotation node is compatible with the given ASM API version. This method
        ///     checks that this node, and all its children recursively, do not contain elements that were
        ///     introduced in more recent versions of the ASM API than the given version.
        /// </remarks>
        /// <param name="api">
        ///     an ASM API version. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        public virtual void Check(VisitorAsmApiVersion api)
        {
        }

        // nothing to do
        /// <summary>Makes the given visitor visit this annotation.</summary>
        /// <param name="annotationVisitor">
        ///     an annotation visitor. Maybe
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public virtual void Accept(AnnotationVisitor annotationVisitor)
        {
            if (annotationVisitor != null)
            {
                if (values != null)
                    for (int i = 0, n = values.Count; i < n; i += 2)
                    {
                        var name = (string) values[i];
                        var value = values[i + 1];
                        Accept(annotationVisitor, name, value);
                    }

                annotationVisitor.VisitEnd();
            }
        }

        /// <summary>Makes the given visitor visit a given annotation value.</summary>
        /// <param name="annotationVisitor">
        ///     an annotation visitor. Maybe
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="name">the value name.</param>
        /// <param name="value">the actual value.</param>
        internal static void Accept(AnnotationVisitor annotationVisitor, string name, object
            value)
        {
            if (annotationVisitor != null)
            {
                if (value is string[])
                {
                    var typeValue = (string[]) value;
                    annotationVisitor.VisitEnum(name, typeValue[0], typeValue[1]);
                }
                else if (value is AnnotationNode)
                {
                    var annotationValue = (AnnotationNode) value;
                    annotationValue.Accept(annotationVisitor.VisitAnnotation(name, annotationValue.desc
                    ));
                }
                else if (value is IList)
                {
                    var arrayAnnotationVisitor = annotationVisitor.VisitArray(name);
                    if (arrayAnnotationVisitor != null)
                    {
                        var arrayValue = (IList<object>) value;
                        for (int i = 0, n = arrayValue.Count; i < n; ++i)
                            Accept(arrayAnnotationVisitor, null, arrayValue[i]);
                        arrayAnnotationVisitor.VisitEnd();
                    }
                }
                else
                {
                    annotationVisitor.Visit(name, value);
                }
            }
        }
    }
}