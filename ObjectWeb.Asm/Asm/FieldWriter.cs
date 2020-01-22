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

namespace ObjectWeb.Asm
{
    /// <summary>
    ///     A
    ///     <see cref="FieldVisitor" />
    ///     that generates a corresponding 'field_info' structure, as defined in the
    ///     Java Virtual Machine Specification (JVMS).
    /// </summary>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.5">
    ///         JVMS
    ///         *     4.5
    ///     </a>
    /// </seealso>
    /// <author>Eric Bruneton</author>
    internal sealed class FieldWriter : FieldVisitor
    {
        /// <summary>The access_flags field of the field_info JVMS structure.</summary>
        /// <remarks>
        ///     The access_flags field of the field_info JVMS structure. This field can contain ASM specific
        ///     access flags, such as
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     , which are removed when generating the
        ///     ClassFile structure.
        /// </remarks>
        private readonly AccessFlags accessFlags;

        /// <summary>
        ///     The constantvalue_index field of the ConstantValue attribute of this field_info, or 0 if there
        ///     is no ConstantValue attribute.
        /// </summary>
        private readonly int constantValueIndex;

        /// <summary>The descriptor_index field of the field_info JVMS structure.</summary>
        private readonly int descriptorIndex;

        /// <summary>The name_index field of the field_info JVMS structure.</summary>
        private readonly int nameIndex;

        /// <summary>
        ///     The signature_index field of the Signature attribute of this field_info, or 0 if there is no
        ///     Signature attribute.
        /// </summary>
        private readonly int signatureIndex;

        /// <summary>Where the constants used in this FieldWriter must be stored.</summary>
        private readonly SymbolTable symbolTable;

        /// <summary>The first non standard attribute of this field.</summary>
        /// <remarks>
        ///     The first non standard attribute of this field. The next ones can be accessed with the
        ///     <see cref="Attribute.nextAttribute" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        ///     <p>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in
        ///         <see cref="VisitAttribute(Attribute)" />
        ///         . The
        ///         <see cref="PutFieldInfo(ByteVector)" />
        ///         method writes the attributes in the order defined by this list, i.e. in the
        ///         reverse order specified by the user.
        /// </remarks>
        private Attribute firstAttribute;

        /// <summary>The last runtime invisible annotation of this field.</summary>
        /// <remarks>
        ///     The last runtime invisible annotation of this field. The previous ones can be accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleAnnotation;

        /// <summary>The last runtime invisible type annotation of this field.</summary>
        /// <remarks>
        ///     The last runtime invisible type annotation of this field. The previous ones can be accessed
        ///     with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

        /// <summary>The last runtime visible annotation of this field.</summary>
        /// <remarks>
        ///     The last runtime visible annotation of this field. The previous ones can be accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleAnnotation;

        /// <summary>The last runtime visible type annotation of this field.</summary>
        /// <remarks>
        ///     The last runtime visible type annotation of this field. The previous ones can be accessed with
        ///     the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="FieldWriter" />
        ///     .
        /// </summary>
        /// <param name="symbolTable">
        ///     where the constants used in this FieldWriter must be stored.
        /// </param>
        /// <param name="access">
        ///     the field's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">
        ///     the field's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the field's signature. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="constantValue">
        ///     the field's constant value. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        internal FieldWriter(SymbolTable symbolTable, AccessFlags access, string name, string descriptor
            , string signature, object constantValue)
            : base(VisitorAsmApiVersion.Asm7)
        {
            // Note: fields are ordered as in the field_info structure, and those related to attributes are
            // ordered as in Section 4.7 of the JVMS.
            // -----------------------------------------------------------------------------------------------
            // Constructor
            // -----------------------------------------------------------------------------------------------
            /* latest api = */
            this.symbolTable = symbolTable;
            accessFlags = access;
            nameIndex = symbolTable.AddConstantUtf8(name);
            descriptorIndex = symbolTable.AddConstantUtf8(descriptor);
            if (signature != null) signatureIndex = symbolTable.AddConstantUtf8(signature);
            if (constantValue != null) constantValueIndex = symbolTable.AddConstant(constantValue).index;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            if (visible)
                return lastRuntimeVisibleAnnotation = AnnotationWriter.Create(symbolTable, descriptor
                    , lastRuntimeVisibleAnnotation);
            return lastRuntimeInvisibleAnnotation = AnnotationWriter.Create(symbolTable, descriptor
                , lastRuntimeInvisibleAnnotation);
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            if (visible)
                return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(symbolTable, typeRef
                    , typePath, descriptor, lastRuntimeVisibleTypeAnnotation);
            return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(symbolTable,
                typeRef, typePath, descriptor, lastRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            // Store the attributes in the <i>reverse</i> order of their visit by this method.
            attribute.nextAttribute = firstAttribute;
            firstAttribute = attribute;
        }

        public override void VisitEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the size of the field_info JVMS structure generated by this FieldWriter.
        /// </summary>
        /// <remarks>
        ///     Returns the size of the field_info JVMS structure generated by this FieldWriter. Also adds the
        ///     names of the attributes of this field in the constant pool.
        /// </remarks>
        /// <returns>the size in bytes of the field_info JVMS structure.</returns>
        internal int ComputeFieldInfoSize()
        {
            // The access_flags, name_index, descriptor_index and attributes_count fields use 8 bytes.
            var size = 8;
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            if (constantValueIndex != 0)
            {
                // ConstantValue attributes always use 8 bytes.
                symbolTable.AddConstantUtf8(Constants.Constant_Value);
                size += 8;
            }

            size += Attribute.ComputeAttributesSize(symbolTable, accessFlags, signatureIndex);
            size += AnnotationWriter.ComputeAnnotationsSize(lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation);
            if (firstAttribute != null) size += firstAttribute.ComputeAttributesSize(symbolTable);
            return size;
        }

        /// <summary>
        ///     Puts the content of the field_info JVMS structure generated by this FieldWriter into the given
        ///     ByteVector.
        /// </summary>
        /// <param name="output">where the field_info structure must be put.</param>
        internal void PutFieldInfo(ByteVector output)
        {
            var useSyntheticAttribute = symbolTable.GetMajorVersion() < OpcodesConstants.V1_5;
            // Put the access_flags, name_index and descriptor_index fields.
            var mask = useSyntheticAttribute ? AccessFlags.Synthetic : 0;
            output.PutShort((int) (accessFlags & ~mask)).PutShort(nameIndex).PutShort(descriptorIndex
            );
            // Compute and put the attributes_count field.
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributesCount = 0;
            if (constantValueIndex != 0) ++attributesCount;
            if (accessFlags.HasFlagFast(AccessFlags.Synthetic) && useSyntheticAttribute) ++attributesCount;
            if (signatureIndex != 0) ++attributesCount;
            if (accessFlags.HasFlagFast(AccessFlags.Deprecated)) ++attributesCount;
            if (lastRuntimeVisibleAnnotation != null) ++attributesCount;
            if (lastRuntimeInvisibleAnnotation != null) ++attributesCount;
            if (lastRuntimeVisibleTypeAnnotation != null) ++attributesCount;
            if (lastRuntimeInvisibleTypeAnnotation != null) ++attributesCount;
            if (firstAttribute != null) attributesCount += firstAttribute.GetAttributeCount();
            output.PutShort(attributesCount);
            // Put the field_info attributes.
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            if (constantValueIndex != 0)
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Constant_Value)).PutInt(2)
                    .PutShort(constantValueIndex);
            Attribute.PutAttributes(symbolTable, accessFlags, signatureIndex, output);
            AnnotationWriter.PutAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, output);
            if (firstAttribute != null) firstAttribute.PutAttributes(symbolTable, output);
        }

        /// <summary>
        ///     Collects the attributes of this field into the given set of attribute prototypes.
        /// </summary>
        /// <param name="attributePrototypes">a set of attribute prototypes.</param>
        internal void CollectAttributePrototypes(Attribute.Set attributePrototypes)
        {
            attributePrototypes.AddAttributes(firstAttribute);
        }
    }
}