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

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     An
    ///     <see cref="AnnotationVisitor" />
    ///     that checks that its methods are properly used.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class CheckAnnotationAdapter : AnnotationVisitor
    {
        /// <summary>Whether the values of the visited annotation are named.</summary>
        /// <remarks>
        ///     Whether the values of the visited annotation are named. AnnotationVisitor instances used for
        ///     annotation default and annotation arrays use unnamed values.
        /// </remarks>
        private readonly bool useNamedValue;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitEnd()" />
        ///     method has been called.
        /// </summary>
        private bool visitEndCalled;

        public CheckAnnotationAdapter(AnnotationVisitor annotationVisitor)
            : this(annotationVisitor, true)
        {
        }

        internal CheckAnnotationAdapter(AnnotationVisitor annotationVisitor, bool useNamedValues
        )
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, annotationVisitor)
        {
            /* latest api = */
            useNamedValue = useNamedValues;
        }

        public override void Visit(string name, object value)
        {
            CheckVisitEndNotCalled();
            CheckName(name);
            if (!(value is byte || value is bool || value is char || value is short || value
                      is int || value is long || value is float || value is double || value is string
                  || value is Type || value is byte[] || value is bool[] || value is char[] || value
                      is short[] || value is int[] || value is long[] || value is float[] || value is
                      double[]))
                throw new ArgumentException("Invalid annotation value");
            if (value is Type && ((Type) value).GetSort() == Type.Method)
                throw new ArgumentException("Invalid annotation value");
            base.Visit(name, value);
        }

        public override void VisitEnum(string name, string descriptor, string value)
        {
            CheckVisitEndNotCalled();
            CheckName(name);
            // Annotations can only appear in V1_5 or more classes.
            CheckMethodAdapter.CheckDescriptor(OpcodesConstants.V1_5, descriptor, false);
            if (value == null) throw new ArgumentException("Invalid enum value");
            base.VisitEnum(name, descriptor, value);
        }

        public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
        {
            CheckVisitEndNotCalled();
            CheckName(name);
            // Annotations can only appear in V1_5 or more classes.
            CheckMethodAdapter.CheckDescriptor(OpcodesConstants.V1_5, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitAnnotation(name, descriptor));
        }

        public override AnnotationVisitor VisitArray(string name)
        {
            CheckVisitEndNotCalled();
            CheckName(name);
            return new CheckAnnotationAdapter(base.VisitArray(name), false);
        }

        public override void VisitEnd()
        {
            CheckVisitEndNotCalled();
            visitEndCalled = true;
            base.VisitEnd();
        }

        private void CheckName(string name)
        {
            if (useNamedValue && name == null) throw new ArgumentException("Annotation value name must not be null");
        }

        private void CheckVisitEndNotCalled()
        {
            if (visitEndCalled)
                throw new InvalidOperationException("Cannot call a visit method after visitEnd has been called"
                );
        }
    }
}