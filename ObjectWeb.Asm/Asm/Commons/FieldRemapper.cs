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
    ///     A
    ///     <see cref="FieldVisitor" />
    ///     that remaps types with a
    ///     <see cref="Remapper" />
    ///     .
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    public class FieldRemapper : FieldVisitor
    {
        /// <summary>The remapper used to remap the types in the visited field.</summary>
        protected internal readonly Remapper remapper;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="FieldRemapper" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="FieldRemapper(int,ObjectWeb.Asm.FieldVisitor,ObjectWeb.Asm.Commons.Remapper)" />
        ///     version.
        /// </summary>
        /// <param name="fieldVisitor">the field visitor this remapper must deleted to.</param>
        /// <param name="remapper">
        ///     the remapper to use to remap the types in the visited field.
        /// </param>
        public FieldRemapper(FieldVisitor fieldVisitor, Remapper remapper)
            : this(VisitorAsmApiVersion.Asm7, fieldVisitor, remapper)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="FieldRemapper" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this remapper. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     .
        /// </param>
        /// <param name="fieldVisitor">the field visitor this remapper must deleted to.</param>
        /// <param name="remapper">
        ///     the remapper to use to remap the types in the visited field.
        /// </param>
        protected internal FieldRemapper(VisitorAsmApiVersion api, FieldVisitor fieldVisitor, Remapper remapper
        )
            : base(api, fieldVisitor)
        {
            /* latest api = */
            this.remapper = remapper;
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            var annotationVisitor = base.VisitAnnotation(remapper.MapDesc(descriptor
            ), visible);
            return annotationVisitor == null
                ? null
                : CreateAnnotationRemapper(annotationVisitor
                );
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var annotationVisitor = base.VisitTypeAnnotation(typeRef, typePath,
                remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? null
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