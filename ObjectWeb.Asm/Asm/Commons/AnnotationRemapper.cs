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

using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     An
    ///     <see cref="AnnotationVisitor" />
    ///     that remaps types with a
    ///     <see cref="Remapper" />
    ///     .
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    public class AnnotationRemapper : AnnotationVisitor
    {
        /// <summary>The remapper used to remap the types in the visited annotation.</summary>
        protected internal readonly Remapper remapper;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnnotationRemapper" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="AnnotationRemapper(VisitorAsmApiVersion, AnnotationVisitor, Remapper)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="annotationVisitor">
        ///     the annotation visitor this remapper must deleted to.
        /// </param>
        /// <param name="remapper">
        ///     the remapper to use to remap the types in the visited annotation.
        /// </param>
        public AnnotationRemapper(AnnotationVisitor annotationVisitor, Remapper remapper)
            : this(VisitorAsmApiVersion.Asm7, annotationVisitor, remapper)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="AnnotationRemapper" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this remapper. Must be one of
        ///     <see cref="Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Opcodes.Asm5" />
        ///     or
        ///     <see cref="Opcodes.Asm6" />
        ///     .
        /// </param>
        /// <param name="annotationVisitor">
        ///     the annotation visitor this remapper must deleted to.
        /// </param>
        /// <param name="remapper">
        ///     the remapper to use to remap the types in the visited annotation.
        /// </param>
        protected internal AnnotationRemapper(VisitorAsmApiVersion api, AnnotationVisitor annotationVisitor
            , Remapper remapper)
            : base(api, annotationVisitor)
        {
            /* latest api = */
            this.remapper = remapper;
        }

        public override void Visit(string name, object value)
        {
            base.Visit(name, remapper.MapValue(value));
        }

        public override void VisitEnum(string name, string descriptor, string value)
        {
            base.VisitEnum(name, remapper.MapDesc(descriptor), value);
        }

        public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
        {
            var annotationVisitor = base.VisitAnnotation(name, remapper.MapDesc
                (descriptor));
            if (annotationVisitor == null)
                return null;
            return annotationVisitor == av
                ? this
                : CreateAnnotationRemapper(annotationVisitor
                );
        }

        public override AnnotationVisitor VisitArray(string name)
        {
            var annotationVisitor = base.VisitArray(name);
            if (annotationVisitor == null)
                return null;
            return annotationVisitor == av
                ? this
                : CreateAnnotationRemapper(annotationVisitor
                );
        }

        /// <summary>Constructs a new remapper for annotations.</summary>
        /// <remarks>
        ///     Constructs a new remapper for annotations. The default implementation of this method returns a
        ///     new
        ///     <see cref="AnnotationRemapper" />
        ///     .
        /// </remarks>
        /// <param name="annotationVisitor">
        ///     the AnnotationVisitor the remapper must delegate to.
        /// </param>
        /// <returns>the newly created remapper.</returns>
        protected internal virtual AnnotationVisitor CreateAnnotationRemapper(AnnotationVisitor
            annotationVisitor)
        {
            return new AnnotationRemapper(api, annotationVisitor, remapper);
        }
    }
}