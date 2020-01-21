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

namespace ObjectWeb.Asm
{
    internal sealed class RecordComponentWriter : RecordComponentVisitor
    {
        /// <summary>
        ///     The access_flags field can only be
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     .
        /// </summary>
        private readonly int accessFlags;

        /// <summary>The descriptor_index field of the the Record attribute.</summary>
        private readonly int descriptorIndex;

        /// <summary>The name_index field of the Record attribute.</summary>
        private readonly int nameIndex;

        /// <summary>
        ///     The signature_index field of the Signature attribute of this record component, or 0 if there is
        ///     no Signature attribute.
        /// </summary>
        private readonly int signatureIndex;

        /// <summary>Where the constants used in this RecordComponentWriter must be stored.</summary>
        private readonly SymbolTable symbolTable;

        /// <summary>The first non standard attribute of this record component.</summary>
        /// <remarks>
        ///     The first non standard attribute of this record component. The next ones can be accessed with
        ///     the
        ///     <see cref="Attribute.nextAttribute" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        ///     <p>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in
        ///         <see cref="VisitAttributeExperimental(Attribute)" />
        ///         . The
        ///         <see cref="PutRecordComponentInfo(ByteVector)" />
        ///         method
        ///         writes the attributes in the order defined by this list, i.e. in the reverse order specified by
        ///         the user.
        /// </remarks>
        private Attribute firstAttribute;

        /// <summary>The last runtime invisible annotation of this record component.</summary>
        /// <remarks>
        ///     The last runtime invisible annotation of this record component. The previous ones can be
        ///     accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleAnnotation;

        /// <summary>The last runtime invisible type annotation of this record component.</summary>
        /// <remarks>
        ///     The last runtime invisible type annotation of this record component. The previous ones can be
        ///     accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

        /// <summary>The last runtime visible annotation of this record component.</summary>
        /// <remarks>
        ///     The last runtime visible annotation of this record component. The previous ones can be accessed
        ///     with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleAnnotation;

        /// <summary>The last runtime visible type annotation of this record component.</summary>
        /// <remarks>
        ///     The last runtime visible type annotation of this record component. The previous ones can be
        ///     accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="RecordComponentWriter" />
        ///     .
        /// </summary>
        /// <param name="symbolTable">
        ///     where the constants used in this RecordComponentWriter must be stored.
        /// </param>
        /// <param name="accessFlags">
        ///     the record component access flags, only synthetic and/or deprecated.
        /// </param>
        /// <param name="name">the record component name.</param>
        /// <param name="descriptor">
        ///     the record component descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the record component signature. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        internal RecordComponentWriter(SymbolTable symbolTable, int accessFlags, string name
            , string descriptor, string signature)
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7)
        {
            // Note: fields are ordered as in the component_info structure, and those related to attributes
            // are ordered as in Section TODO of the JVMS.
            // The field accessFlag doesn't exist in the component_info structure but is used to carry
            // ACC_DEPRECATED which is represented by an attribute in the structure and as an access flag by
            // ASM.
            /* latest api = */
            this.symbolTable = symbolTable;
            this.accessFlags = accessFlags;
            nameIndex = symbolTable.AddConstantUtf8(name);
            descriptorIndex = symbolTable.AddConstantUtf8(descriptor);
            if (signature != null) signatureIndex = symbolTable.AddConstantUtf8(signature);
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override AnnotationVisitor VisitAnnotationExperimental(string descriptor,
            bool visible)
        {
            if (visible)
                return lastRuntimeVisibleAnnotation = AnnotationWriter.Create(symbolTable, descriptor
                    , lastRuntimeVisibleAnnotation);
            return lastRuntimeInvisibleAnnotation = AnnotationWriter.Create(symbolTable, descriptor
                , lastRuntimeInvisibleAnnotation);
        }

        public override AnnotationVisitor VisitTypeAnnotationExperimental(int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            if (visible)
                return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(symbolTable, typeRef
                    , typePath, descriptor, lastRuntimeVisibleTypeAnnotation);
            return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(symbolTable,
                typeRef, typePath, descriptor, lastRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitAttributeExperimental(Attribute attribute)
        {
            // Store the attributes in the <i>reverse</i> order of their visit by this method.
            attribute.nextAttribute = firstAttribute;
            firstAttribute = attribute;
        }

        public override void VisitEndExperimental()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the size of the record component JVMS structure generated by this
        ///     RecordComponentWriter.
        /// </summary>
        /// <remarks>
        ///     Returns the size of the record component JVMS structure generated by this
        ///     RecordComponentWriter. Also adds the names of the attributes of this record component in the
        ///     constant pool.
        /// </remarks>
        /// <returns>the size in bytes of the record_component_info of the Record attribute.</returns>
        internal int ComputeRecordComponentInfoSize()
        {
            // name_index, descriptor_index and attributes_count fields use 6 bytes.
            var size = 6;
            size += Attribute.ComputeAttributesSize(symbolTable, accessFlags & OpcodesConstants
                                                                     .Acc_Deprecated, signatureIndex);
            size += AnnotationWriter.ComputeAnnotationsSize(lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation);
            if (firstAttribute != null) size += firstAttribute.ComputeAttributesSize(symbolTable);
            return size;
        }

        /// <summary>
        ///     Puts the content of the record component generated by this RecordComponentWriter into the given
        ///     ByteVector.
        /// </summary>
        /// <param name="output">where the record_component_info structure must be put.</param>
        internal void PutRecordComponentInfo(ByteVector output)
        {
            output.PutShort(nameIndex).PutShort(descriptorIndex);
            // Compute and put the attributes_count field.
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributesCount = 0;
            if (signatureIndex != 0) ++attributesCount;
            if ((accessFlags & OpcodesConstants.Acc_Deprecated) != 0) ++attributesCount;
            if (lastRuntimeVisibleAnnotation != null) ++attributesCount;
            if (lastRuntimeInvisibleAnnotation != null) ++attributesCount;
            if (lastRuntimeVisibleTypeAnnotation != null) ++attributesCount;
            if (lastRuntimeInvisibleTypeAnnotation != null) ++attributesCount;
            if (firstAttribute != null) attributesCount += firstAttribute.GetAttributeCount();
            output.PutShort(attributesCount);
            Attribute.PutAttributes(symbolTable, accessFlags, signatureIndex, output);
            AnnotationWriter.PutAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, output);
            if (firstAttribute != null) firstAttribute.PutAttributes(symbolTable, output);
        }

        /// <summary>
        ///     Collects the attributes of this record component into the given set of attribute prototypes.
        /// </summary>
        /// <param name="attributePrototypes">a set of attribute prototypes.</param>
        internal void CollectAttributePrototypes(Attribute.Set attributePrototypes)
        {
            attributePrototypes.AddAttributes(firstAttribute);
        }
    }
}