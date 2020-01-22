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
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm
{
    /// <summary>
    ///     A
    ///     <see cref="ClassVisitor" />
    ///     that generates a corresponding ClassFile structure, as defined in the Java
    ///     Virtual Machine Specification (JVMS). It can be used alone, to generate a Java class "from
    ///     scratch", or with one or more
    ///     <see cref="ClassReader" />
    ///     and adapter
    ///     <see cref="ClassVisitor" />
    ///     to generate a
    ///     modified class from one or more existing Java classes.
    /// </summary>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html">JVMS 4</a>
    /// </seealso>
    /// <author>Eric Bruneton</author>
    public class ClassWriter : ClassVisitor
    {
        /// <summary>
        ///     A flag to automatically compute the maximum stack size and the maximum number of local
        ///     variables of methods.
        /// </summary>
        /// <remarks>
        ///     A flag to automatically compute the maximum stack size and the maximum number of local
        ///     variables of methods. If this flag is set, then the arguments of the
        ///     <see cref="MethodVisitor.VisitMaxs(int, int)" />
        ///     method of the
        ///     <see cref="MethodVisitor" />
        ///     returned by the
        ///     <see cref="VisitMethod(int, string, string, string, string[])" />
        ///     method will be ignored, and computed automatically from the signature and the
        ///     bytecode of each method.
        ///     <p>
        ///         <b>Note:</b> for classes whose version is
        ///         <see cref="Opcodes.V1_7" />
        ///         of more, this option requires
        ///         valid stack map frames. The maximum stack size is then computed from these frames, and from the
        ///         bytecode instructions in between. If stack map frames are not present or must be recomputed,
        ///         used
        ///         <see cref="Compute_Frames" />
        ///         instead.
        /// </remarks>
        /// <seealso cref="ClassWriter(int)" />
        public const int Compute_Maxs = 1;

        /// <summary>
        ///     A flag to automatically compute the stack map frames of methods from scratch.
        /// </summary>
        /// <remarks>
        ///     A flag to automatically compute the stack map frames of methods from scratch. If this flag is
        ///     set, then the calls to the
        ///     <see cref="MethodVisitor.VisitFrame(int, int, object[], int, object[])" />
        ///     method are ignored, and the stack
        ///     map frames are recomputed from the methods bytecode. The arguments of the
        ///     <see cref="MethodVisitor.VisitMaxs(int, int)" />
        ///     method are also ignored and recomputed from the bytecode. In other
        ///     words,
        ///     <see cref="Compute_Frames" />
        ///     implies
        ///     <see cref="Compute_Maxs" />
        ///     .
        /// </remarks>
        /// <seealso cref="ClassWriter(int)" />
        public const int Compute_Frames = 2;

        /// <summary>
        ///     The symbol table for this class (contains the constant_pool and the BootstrapMethods).
        /// </summary>
        private readonly SymbolTable symbolTable;

        /// <summary>The access_flags field of the JVMS ClassFile structure.</summary>
        /// <remarks>
        ///     The access_flags field of the JVMS ClassFile structure. This field can contain ASM specific
        ///     access flags, such as
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     , which are removed when generating the
        ///     ClassFile structure.
        /// </remarks>
        private ObjectWeb.Asm.Enums.AccessFlags accessFlags;

        /// <summary>
        ///     Indicates what must be automatically computed in
        ///     <see cref="MethodWriter" />
        ///     . Must be one of
        ///     <see cref="MethodWriter.Compute_Nothing" />
        ///     ,
        ///     <see cref="MethodWriter.Compute_Max_Stack_And_Local" />
        ///     ,
        ///     <see cref="MethodWriter.Compute_Inserted_Frames" />
        ///     , or
        ///     <see cref="MethodWriter.Compute_All_Frames" />
        ///     .
        /// </summary>
        private int compute;

        /// <summary>
        ///     The debug_extension field of the SourceDebugExtension attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector debugExtension;

        /// <summary>The class_index field of the EnclosingMethod attribute, or 0.</summary>
        private int enclosingClassIndex;

        /// <summary>The method_index field of the EnclosingMethod attribute.</summary>
        private int enclosingMethodIndex;

        /// <summary>The first non standard attribute of this class.</summary>
        /// <remarks>
        ///     The first non standard attribute of this class. The next ones can be accessed with the
        ///     <see cref="Attribute.nextAttribute" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        ///     <p>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in
        ///         <see cref="VisitAttribute(Attribute)" />
        ///         . The
        ///         <see cref="ToByteArray()" />
        ///         method writes the attributes in the order defined by this list, i.e. in the
        ///         reverse order specified by the user.
        /// </remarks>
        private Attribute firstAttribute;

        /// <summary>
        ///     The fields of this class, stored in a linked list of
        ///     <see cref="FieldWriter" />
        ///     linked via their
        ///     <see cref="FieldVisitor.fv" />
        ///     field. This field stores the first element of this list.
        /// </summary>
        private FieldWriter firstField;

        /// <summary>
        ///     The methods of this class, stored in a linked list of
        ///     <see cref="MethodWriter" />
        ///     linked via their
        ///     <see cref="MethodVisitor.mv" />
        ///     field. This field stores the first element of this list.
        /// </summary>
        private MethodWriter firstMethod;

        /// <summary>
        ///     The record components of this class, stored in a linked list of
        ///     <see cref="RecordComponentWriter" />
        ///     linked via their
        ///     <see cref="RecordComponentVisitor.delegate_" />
        ///     field. This field stores the first
        ///     element of this list.
        /// </summary>
        private RecordComponentWriter firstRecordComponent;

        /// <summary>
        ///     The 'classes' array of the InnerClasses attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector innerClasses;

        /// <summary>The interface_count field of the JVMS ClassFile structure.</summary>
        private int interfaceCount;

        /// <summary>The 'interfaces' array of the JVMS ClassFile structure.</summary>
        private int[] interfaces;

        /// <summary>
        ///     The fields of this class, stored in a linked list of
        ///     <see cref="FieldWriter" />
        ///     linked via their
        ///     <see cref="FieldVisitor.fv" />
        ///     field. This field stores the last element of this list.
        /// </summary>
        private FieldWriter lastField;

        /// <summary>
        ///     The methods of this class, stored in a linked list of
        ///     <see cref="MethodWriter" />
        ///     linked via their
        ///     <see cref="MethodVisitor.mv" />
        ///     field. This field stores the last element of this list.
        /// </summary>
        private MethodWriter lastMethod;

        /// <summary>
        ///     The record components of this class, stored in a linked list of
        ///     <see cref="RecordComponentWriter" />
        ///     linked via their
        ///     <see cref="RecordComponentVisitor.delegate_" />
        ///     field. This field stores the last
        ///     element of this list.
        /// </summary>
        private RecordComponentWriter lastRecordComponent;

        /// <summary>The last runtime invisible annotation of this class.</summary>
        /// <remarks>
        ///     The last runtime invisible annotation of this class. The previous ones can be accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleAnnotation;

        /// <summary>The last runtime invisible type annotation of this class.</summary>
        /// <remarks>
        ///     The last runtime invisible type annotation of this class. The previous ones can be accessed
        ///     with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

        /// <summary>The last runtime visible annotation of this class.</summary>
        /// <remarks>
        ///     The last runtime visible annotation of this class. The previous ones can be accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleAnnotation;

        /// <summary>The last runtime visible type annotation of this class.</summary>
        /// <remarks>
        ///     The last runtime visible type annotation of this class. The previous ones can be accessed with
        ///     the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The Module attribute of this class, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ModuleWriter moduleWriter;

        /// <summary>The host_class_index field of the NestHost attribute, or 0.</summary>
        private int nestHostClassIndex;

        /// <summary>
        ///     The 'classes' array of the NestMembers attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector nestMemberClasses;

        /// <summary>The number_of_classes field of the InnerClasses attribute, or 0.</summary>
        private int numberOfInnerClasses;

        /// <summary>The number_of_classes field of the NestMembers attribute, or 0.</summary>
        private int numberOfNestMemberClasses;

        /// <summary>The number_of_classes field of the PermittedSubtypes attribute, or 0.</summary>
        private int numberOfPermittedSubtypeClasses;

        /// <summary>
        ///     The 'classes' array of the PermittedSubtypes attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector permittedSubtypeClasses;

        /// <summary>The signature_index field of the Signature attribute, or 0.</summary>
        private int signatureIndex;

        /// <summary>The source_file_index field of the SourceFile attribute, or 0.</summary>
        private int sourceFileIndex;

        /// <summary>The super_class field of the JVMS ClassFile structure.</summary>
        private int superClass;

        /// <summary>The this_class field of the JVMS ClassFile structure.</summary>
        private int thisClass;

        /// <summary>
        ///     The minor_version and major_version fields of the JVMS ClassFile structure.
        /// </summary>
        /// <remarks>
        ///     The minor_version and major_version fields of the JVMS ClassFile structure. minor_version is
        ///     stored in the 16 most significant bits, and major_version in the 16 least significant bits.
        /// </remarks>
        private int version;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ClassWriter" />
        ///     object.
        /// </summary>
        /// <param name="flags">
        ///     option flags that can be used to modify the default behavior of this class. Must
        ///     be zero or more of
        ///     <see cref="Compute_Maxs" />
        ///     and
        ///     <see cref="Compute_Frames" />
        ///     .
        /// </param>
        public ClassWriter(int flags)
            : this(null, flags)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ClassWriter" />
        ///     object and enables optimizations for "mostly add" bytecode
        ///     transformations. These optimizations are the following:
        ///     <ul>
        ///         <li>
        ///             The constant pool and bootstrap methods from the original class are copied as is in the
        ///             new class, which saves time. New constant pool entries and new bootstrap methods will be
        ///             added at the end if necessary, but unused constant pool entries or bootstrap methods
        ///             <i>won't be removed</i>.
        ///             <li>
        ///                 Methods that are not transformed are copied as is in the new class, directly from the
        ///                 original class bytecode (i.e. without emitting visit events for all the method
        ///                 instructions), which saves a <i>lot</i> of time. Untransformed methods are detected by
        ///                 the fact that the
        ///                 <see cref="ClassReader" />
        ///                 receives
        ///                 <see cref="MethodVisitor" />
        ///                 objects that come
        ///                 from a
        ///                 <see cref="ClassWriter" />
        ///                 (and not from any other
        ///                 <see cref="ClassVisitor" />
        ///                 instance).
        ///     </ul>
        /// </summary>
        /// <param name="classReader">
        ///     the
        ///     <see cref="ClassReader" />
        ///     used to read the original class. It will be used to
        ///     copy the entire constant pool and bootstrap methods from the original class and also to
        ///     copy other fragments of original bytecode where applicable.
        /// </param>
        /// <param name="flags">
        ///     option flags that can be used to modify the default behavior of this class.Must be
        ///     zero or more of
        ///     <see cref="Compute_Maxs" />
        ///     and
        ///     <see cref="Compute_Frames" />
        ///     .
        ///     <i>
        ///         These option flags do
        ///         not affect methods that are copied as is in the new class. This means that neither the
        ///         maximum stack size nor the stack frames will be computed for these methods
        ///     </i>
        ///     .
        /// </param>
        public ClassWriter(ClassReader classReader, int flags)
            : base(VisitorAsmApiVersion.Asm7)
        {
            // Note: fields are ordered as in the ClassFile structure, and those related to attributes are
            // ordered as in Section 4.7 of the JVMS.
            // -----------------------------------------------------------------------------------------------
            // Constructor
            // -----------------------------------------------------------------------------------------------
            /* latest api = */
            symbolTable = classReader == null
                ? new SymbolTable(this)
                : new SymbolTable(this,
                    classReader);
            if ((flags & Compute_Frames) != 0)
                compute = MethodWriter.Compute_All_Frames;
            else if ((flags & Compute_Maxs) != 0)
                compute = MethodWriter.Compute_Max_Stack_And_Local;
            else
                compute = MethodWriter.Compute_Nothing;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the ClassVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public sealed override void Visit(int version, AccessFlags access, string name, string signature
            , string superName, string[] interfaces)
        {
            this.version = version;
            accessFlags = access;
            thisClass = symbolTable.SetMajorVersionAndClassName(version & 0xFFFF, name);
            if (signature != null) signatureIndex = symbolTable.AddConstantUtf8(signature);
            superClass = superName == null
                ? 0
                : symbolTable.AddConstantClass(superName)
                    .index;
            if (interfaces != null && interfaces.Length > 0)
            {
                interfaceCount = interfaces.Length;
                this.interfaces = new int[interfaceCount];
                for (var i = 0; i < interfaceCount; ++i)
                    this.interfaces[i] = symbolTable.AddConstantClass(interfaces[i]).index;
            }

            if (compute == MethodWriter.Compute_Max_Stack_And_Local && (version & 0xFFFF) >= OpcodesConstants.V1_7)
                compute = MethodWriter.Compute_Max_Stack_And_Local_From_Frames;
        }

        public sealed override void VisitSource(string file, string debug)
        {
            if (file != null) sourceFileIndex = symbolTable.AddConstantUtf8(file);
            if (debug != null) debugExtension = new ByteVector().EncodeUtf8(debug, 0, int.MaxValue);
        }

        public sealed override ModuleVisitor VisitModule(string name, AccessFlags access, string version)
        {
            return moduleWriter = new ModuleWriter(symbolTable, symbolTable.AddConstantModule
                    (name).index, access, version == null ? 0 : symbolTable.AddConstantUtf8(version)
            );
        }

        public sealed override void VisitNestHost(string nestHost)
        {
            nestHostClassIndex = symbolTable.AddConstantClass(nestHost).index;
        }

        public sealed override void VisitOuterClass(string owner, string name, string descriptor
        )
        {
            enclosingClassIndex = symbolTable.AddConstantClass(owner).index;
            if (name != null && descriptor != null)
                enclosingMethodIndex = symbolTable.AddConstantNameAndType(name, descriptor);
        }

        public sealed override AnnotationVisitor VisitAnnotation(string descriptor, bool
            visible)
        {
            if (visible)
                return lastRuntimeVisibleAnnotation = AnnotationWriter.Create(symbolTable, descriptor
                    , lastRuntimeVisibleAnnotation);
            return lastRuntimeInvisibleAnnotation = AnnotationWriter.Create(symbolTable, descriptor
                , lastRuntimeInvisibleAnnotation);
        }

        public sealed override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            if (visible)
                return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(symbolTable, typeRef
                    , typePath, descriptor, lastRuntimeVisibleTypeAnnotation);
            return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(symbolTable,
                typeRef, typePath, descriptor, lastRuntimeInvisibleTypeAnnotation);
        }

        public sealed override void VisitAttribute(Attribute attribute)
        {
            // Store the attributes in the <i>reverse</i> order of their visit by this method.
            attribute.nextAttribute = firstAttribute;
            firstAttribute = attribute;
        }

        public sealed override void VisitNestMember(string nestMember)
        {
            if (nestMemberClasses == null) nestMemberClasses = new ByteVector();
            ++numberOfNestMemberClasses;
            nestMemberClasses.PutShort(symbolTable.AddConstantClass(nestMember).index);
        }

        public sealed override void VisitPermittedSubtypeExperimental(string permittedSubtype
        )
        {
            if (permittedSubtypeClasses == null) permittedSubtypeClasses = new ByteVector();
            ++numberOfPermittedSubtypeClasses;
            permittedSubtypeClasses.PutShort(symbolTable.AddConstantClass(permittedSubtype).index
            );
        }

        public sealed override void VisitInnerClass(string name, string outerName, string innerName, AccessFlags access)
        {
            if (innerClasses == null) innerClasses = new ByteVector();
            // Section 4.7.6 of the JVMS states "Every CONSTANT_Class_info entry in the constant_pool table
            // which represents a class or interface C that is not a package member must have exactly one
            // corresponding entry in the classes array". To avoid duplicates we keep track in the info
            // field of the Symbol of each CONSTANT_Class_info entry C whether an inner class entry has
            // already been added for C. If so, we store the index of this inner class entry (plus one) in
            // the info field. This trick allows duplicate detection in O(1) time.
            var nameSymbol = symbolTable.AddConstantClass(name);
            if (nameSymbol.info == 0)
            {
                ++numberOfInnerClasses;
                innerClasses.PutShort(nameSymbol.index);
                innerClasses.PutShort(outerName == null
                    ? 0
                    : symbolTable.AddConstantClass(outerName
                    ).index);
                innerClasses.PutShort(innerName == null
                    ? 0
                    : symbolTable.AddConstantUtf8(innerName
                    ));
                innerClasses.PutShort((int) access);
                nameSymbol.info = numberOfInnerClasses;
            }
        }

        // Else, compare the inner classes entry nameSymbol.info - 1 with the arguments of this method
        // and throw an exception if there is a difference?
        public sealed override RecordComponentVisitor VisitRecordComponentExperimental(AccessFlags access, string name,
            string descriptor, string signature)
        {
            var recordComponentWriter = new RecordComponentWriter(symbolTable
                , access, name, descriptor, signature);
            if (firstRecordComponent == null)
                firstRecordComponent = recordComponentWriter;
            else
                lastRecordComponent.delegate_ = recordComponentWriter;
            return lastRecordComponent = recordComponentWriter;
        }

        public sealed override FieldVisitor VisitField(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor
            , string signature, object value)
        {
            var fieldWriter = new FieldWriter(symbolTable, access, name, descriptor,
                signature, value);
            if (firstField == null)
                firstField = fieldWriter;
            else
                lastField.fv = fieldWriter;
            return lastField = fieldWriter;
        }

        public sealed override MethodVisitor VisitMethod(ObjectWeb.Asm.Enums.AccessFlags access, string name, string
            descriptor, string signature, string[] exceptions)
        {
            var methodWriter = new MethodWriter(symbolTable, access, name, descriptor
                , signature, exceptions, compute);
            if (firstMethod == null)
                firstMethod = methodWriter;
            else
                lastMethod.mv = methodWriter;
            return lastMethod = methodWriter;
        }

        public sealed override void VisitEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Other public methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the content of the class file that was built by this ClassWriter.
        /// </summary>
        /// <returns>
        ///     the binary content of the JVMS ClassFile structure that was built by this ClassWriter.
        /// </returns>
        /// <exception cref="ClassTooLargeException">
        ///     if the constant pool of the class is too large.
        /// </exception>
        /// <exception cref="MethodTooLargeException">
        ///     if the Code attribute of a method is too large.
        /// </exception>
        public virtual byte[] ToByteArray()
        {
            // First step: compute the size in bytes of the ClassFile structure.
            // The magic field uses 4 bytes, 10 mandatory fields (minor_version, major_version,
            // constant_pool_count, access_flags, this_class, super_class, interfaces_count, fields_count,
            // methods_count and attributes_count) use 2 bytes each, and each interface uses 2 bytes too.
            var size = 24 + 2 * interfaceCount;
            var fieldsCount = 0;
            var fieldWriter = firstField;
            while (fieldWriter != null)
            {
                ++fieldsCount;
                size += fieldWriter.ComputeFieldInfoSize();
                fieldWriter = (FieldWriter) fieldWriter.fv;
            }

            var methodsCount = 0;
            var methodWriter = firstMethod;
            while (methodWriter != null)
            {
                ++methodsCount;
                size += methodWriter.ComputeMethodInfoSize();
                methodWriter = (MethodWriter) methodWriter.mv;
            }

            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributesCount = 0;
            if (innerClasses != null)
            {
                ++attributesCount;
                size += 8 + innerClasses.length;
                symbolTable.AddConstantUtf8(Constants.Inner_Classes);
            }

            if (enclosingClassIndex != 0)
            {
                ++attributesCount;
                size += 10;
                symbolTable.AddConstantUtf8(Constants.Enclosing_Method);
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Synthetic) && (version & 0xFFFF) < OpcodesConstants.V1_5)
            {
                ++attributesCount;
                size += 6;
                symbolTable.AddConstantUtf8(Constants.Synthetic);
            }

            if (signatureIndex != 0)
            {
                ++attributesCount;
                size += 8;
                symbolTable.AddConstantUtf8(Constants.Signature);
            }

            if (sourceFileIndex != 0)
            {
                ++attributesCount;
                size += 8;
                symbolTable.AddConstantUtf8(Constants.Source_File);
            }

            if (debugExtension != null)
            {
                ++attributesCount;
                size += 6 + debugExtension.length;
                symbolTable.AddConstantUtf8(Constants.Source_Debug_Extension);
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Deprecated))
            {
                ++attributesCount;
                size += 6;
                symbolTable.AddConstantUtf8(Constants.Deprecated);
            }

            if (lastRuntimeVisibleAnnotation != null)
            {
                ++attributesCount;
                size += lastRuntimeVisibleAnnotation.ComputeAnnotationsSize(Constants.Runtime_Visible_Annotations
                );
            }

            if (lastRuntimeInvisibleAnnotation != null)
            {
                ++attributesCount;
                size += lastRuntimeInvisibleAnnotation.ComputeAnnotationsSize(Constants.Runtime_Invisible_Annotations
                );
            }

            if (lastRuntimeVisibleTypeAnnotation != null)
            {
                ++attributesCount;
                size += lastRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(
                    Constants.Runtime_Visible_Type_Annotations
                );
            }

            if (lastRuntimeInvisibleTypeAnnotation != null)
            {
                ++attributesCount;
                size += lastRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(
                    Constants.Runtime_Invisible_Type_Annotations
                );
            }

            if (symbolTable.ComputeBootstrapMethodsSize() > 0)
            {
                ++attributesCount;
                size += symbolTable.ComputeBootstrapMethodsSize();
            }

            if (moduleWriter != null)
            {
                attributesCount += moduleWriter.GetAttributeCount();
                size += moduleWriter.ComputeAttributesSize();
            }

            if (nestHostClassIndex != 0)
            {
                ++attributesCount;
                size += 8;
                symbolTable.AddConstantUtf8(Constants.Nest_Host);
            }

            if (nestMemberClasses != null)
            {
                ++attributesCount;
                size += 8 + nestMemberClasses.length;
                symbolTable.AddConstantUtf8(Constants.Nest_Members);
            }

            if (permittedSubtypeClasses != null)
            {
                ++attributesCount;
                size += 8 + permittedSubtypeClasses.length;
                symbolTable.AddConstantUtf8(Constants.Permitted_Subtypes);
            }

            var recordComponentCount = 0;
            var recordSize = 0;
            if (firstRecordComponent != null)
            {
                var recordComponentWriter = firstRecordComponent;
                while (recordComponentWriter != null)
                {
                    ++recordComponentCount;
                    recordSize += recordComponentWriter.ComputeRecordComponentInfoSize();
                    recordComponentWriter = (RecordComponentWriter) recordComponentWriter.delegate_;
                }

                ++attributesCount;
                size += 8 + recordSize;
                symbolTable.AddConstantUtf8(Constants.Record);
            }

            if (firstAttribute != null)
            {
                attributesCount += firstAttribute.GetAttributeCount();
                size += firstAttribute.ComputeAttributesSize(symbolTable);
            }

            // IMPORTANT: this must be the last part of the ClassFile size computation, because the previous
            // statements can add attribute names to the constant pool, thereby changing its size!
            size += symbolTable.GetConstantPoolLength();
            var constantPoolCount = symbolTable.GetConstantPoolCount();
            if (constantPoolCount > 0xFFFF)
                throw new ClassTooLargeException(symbolTable.GetClassName(), constantPoolCount);
            // Second step: allocate a ByteVector of the correct size (in order to avoid any array copy in
            // dynamic resizes) and fill it with the ClassFile content.
            var result = new ByteVector(size);
            result.PutInt(unchecked((int) 0xCAFEBABE)).PutInt(version);
            symbolTable.PutConstantPool(result);
            var mask = (version & 0xFFFF) < OpcodesConstants.V1_5
                ? ObjectWeb.Asm.Enums.AccessFlags.Synthetic
                : AccessFlags.None;
            result.PutShort((int) (accessFlags & ~mask)).PutShort(thisClass).PutShort(superClass);
            result.PutShort(interfaceCount);
            for (var i = 0; i < interfaceCount; ++i) result.PutShort(interfaces[i]);
            result.PutShort(fieldsCount);
            fieldWriter = firstField;
            while (fieldWriter != null)
            {
                fieldWriter.PutFieldInfo(result);
                fieldWriter = (FieldWriter) fieldWriter.fv;
            }

            result.PutShort(methodsCount);
            var hasFrames = false;
            var hasAsmInstructions = false;
            methodWriter = firstMethod;
            while (methodWriter != null)
            {
                hasFrames |= methodWriter.HasFrames();
                hasAsmInstructions |= methodWriter.HasAsmInstructions();
                methodWriter.PutMethodInfo(result);
                methodWriter = (MethodWriter) methodWriter.mv;
            }

            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            result.PutShort(attributesCount);
            if (innerClasses != null)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Inner_Classes)).PutInt(innerClasses
                                                                                                 .length + 2)
                    .PutShort(numberOfInnerClasses).PutByteArray(innerClasses.data, 0, innerClasses
                        .length);
            if (enclosingClassIndex != 0)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Enclosing_Method)).PutInt(4
                ).PutShort(enclosingClassIndex).PutShort(enclosingMethodIndex);
            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Synthetic) && (version & 0xFFFF) < OpcodesConstants.V1_5)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Synthetic)).PutInt(0);
            if (signatureIndex != 0)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Signature)).PutInt(2).PutShort
                    (signatureIndex);
            if (sourceFileIndex != 0)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Source_File)).PutInt(2).PutShort
                    (sourceFileIndex);
            if (debugExtension != null)
            {
                var length = debugExtension.length;
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Source_Debug_Extension)).PutInt
                    (length).PutByteArray(debugExtension.data, 0, length);
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Deprecated))
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Deprecated)).PutInt(0);
            AnnotationWriter.PutAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, result);
            symbolTable.PutBootstrapMethods(result);
            if (moduleWriter != null) moduleWriter.PutAttributes(result);
            if (nestHostClassIndex != 0)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Nest_Host)).PutInt(2).PutShort
                    (nestHostClassIndex);
            if (nestMemberClasses != null)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Nest_Members)).PutInt(nestMemberClasses
                                                                                                .length + 2)
                    .PutShort(numberOfNestMemberClasses)
                    .PutByteArray(nestMemberClasses.data, 0, nestMemberClasses.length);
            if (permittedSubtypeClasses != null)
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Permitted_Subtypes)).PutInt
                    (permittedSubtypeClasses.length + 2).PutShort(numberOfPermittedSubtypeClasses).PutByteArray
                    (permittedSubtypeClasses.data, 0, permittedSubtypeClasses.length);
            if (firstRecordComponent != null)
            {
                result.PutShort(symbolTable.AddConstantUtf8(Constants.Record)).PutInt(recordSize
                                                                                      + 2).PutShort(
                    recordComponentCount);
                var recordComponentWriter = firstRecordComponent;
                while (recordComponentWriter != null)
                {
                    recordComponentWriter.PutRecordComponentInfo(result);
                    recordComponentWriter = (RecordComponentWriter) recordComponentWriter.delegate_;
                }
            }

            if (firstAttribute != null) firstAttribute.PutAttributes(symbolTable, result);
            // Third step: replace the ASM specific instructions, if any.
            if (hasAsmInstructions)
                return ReplaceAsmInstructions(result.data, hasFrames);
            return result.data;
        }

        /// <summary>
        ///     Returns the equivalent of the given class file, with the ASM specific instructions replaced
        ///     with standard ones.
        /// </summary>
        /// <remarks>
        ///     Returns the equivalent of the given class file, with the ASM specific instructions replaced
        ///     with standard ones. This is done with a ClassReader -&gt; ClassWriter round trip.
        /// </remarks>
        /// <param name="classFile">
        ///     a class file containing ASM specific instructions, generated by this
        ///     ClassWriter.
        /// </param>
        /// <param name="hasFrames">
        ///     whether there is at least one stack map frames in 'classFile'.
        /// </param>
        /// <returns>
        ///     an equivalent of 'classFile', with the ASM specific instructions replaced with standard
        ///     ones.
        /// </returns>
        private byte[] ReplaceAsmInstructions(byte[] classFile, bool hasFrames)
        {
            var attributes = GetAttributePrototypes();
            firstField = null;
            lastField = null;
            firstMethod = null;
            lastMethod = null;
            lastRuntimeVisibleAnnotation = null;
            lastRuntimeInvisibleAnnotation = null;
            lastRuntimeVisibleTypeAnnotation = null;
            lastRuntimeInvisibleTypeAnnotation = null;
            moduleWriter = null;
            nestHostClassIndex = 0;
            numberOfNestMemberClasses = 0;
            nestMemberClasses = null;
            numberOfPermittedSubtypeClasses = 0;
            permittedSubtypeClasses = null;
            firstRecordComponent = null;
            lastRecordComponent = null;
            firstAttribute = null;
            compute = hasFrames ? MethodWriter.Compute_Inserted_Frames : MethodWriter.Compute_Nothing;
            new ClassReader(classFile, 0, false).Accept(this, attributes, (hasFrames
                                                                              ? ParsingOptions.ExpandFrames
                                                                              : 0) | ParsingOptions.ExpandAsmInsns);
            /* checkClassVersion = */
            return ToByteArray();
        }

        /// <summary>
        ///     Returns the prototypes of the attributes used by this class, its fields and its methods.
        /// </summary>
        /// <returns>
        ///     the prototypes of the attributes used by this class, its fields and its methods.
        /// </returns>
        private Attribute[] GetAttributePrototypes()
        {
            var attributePrototypes = new Attribute.Set();
            attributePrototypes.AddAttributes(firstAttribute);
            var fieldWriter = firstField;
            while (fieldWriter != null)
            {
                fieldWriter.CollectAttributePrototypes(attributePrototypes);
                fieldWriter = (FieldWriter) fieldWriter.fv;
            }

            var methodWriter = firstMethod;
            while (methodWriter != null)
            {
                methodWriter.CollectAttributePrototypes(attributePrototypes);
                methodWriter = (MethodWriter) methodWriter.mv;
            }

            var recordComponentWriter = firstRecordComponent;
            while (recordComponentWriter != null)
            {
                recordComponentWriter.CollectAttributePrototypes(attributePrototypes);
                recordComponentWriter = (RecordComponentWriter) recordComponentWriter.delegate_;
            }

            return attributePrototypes.ToArray();
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods: constant pool management for Attribute sub classes
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds a number or string constant to the constant pool of the class being build.
        /// </summary>
        /// <remarks>
        ///     Adds a number or string constant to the constant pool of the class being build. Does nothing if
        ///     the constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="value">
        ///     the value of the constant to be added to the constant pool. This parameter must be
        ///     an
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
        /// <returns>
        ///     the index of a new or already existing constant item with the given value.
        /// </returns>
        public virtual int NewConst(object value)
        {
            return symbolTable.AddConstant(value).index;
        }

        /// <summary>Adds an UTF8 string to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds an UTF8 string to the constant pool of the class being build. Does nothing if the constant
        ///     pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub
        ///         classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="value">the String value.</param>
        /// <returns>the index of a new or already existing UTF8 item.</returns>
        public virtual int NewUTF8(string value)
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            return symbolTable.AddConstantUtf8(value);
        }

        /// <summary>Adds a class reference to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a class reference to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="value">the internal name of the class.</param>
        /// <returns>the index of a new or already existing class reference item.</returns>
        public virtual int NewClass(string value)
        {
            return symbolTable.AddConstantClass(value).index;
        }

        /// <summary>
        ///     Adds a method type reference to the constant pool of the class being build.
        /// </summary>
        /// <remarks>
        ///     Adds a method type reference to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="methodDescriptor">method descriptor of the method type.</param>
        /// <returns>the index of a new or already existing method type reference item.</returns>
        public virtual int NewMethodType(string methodDescriptor)
        {
            return symbolTable.AddConstantMethodType(methodDescriptor).index;
        }

        /// <summary>Adds a module reference to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a module reference to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="moduleName">name of the module.</param>
        /// <returns>the index of a new or already existing module reference item.</returns>
        public virtual int NewModule(string moduleName)
        {
            return symbolTable.AddConstantModule(moduleName).index;
        }

        /// <summary>Adds a package reference to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a package reference to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="packageName">name of the package in its internal form.</param>
        /// <returns>the index of a new or already existing module reference item.</returns>
        public virtual int NewPackage(string packageName)
        {
            return symbolTable.AddConstantPackage(packageName).index;
        }

        /// <summary>Adds a handle to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a handle to the constant pool of the class being build. Does nothing if the constant pool
        ///     already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes,
        ///         and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="tag">
        ///     the kind of this handle. Must be
        ///     <see cref="Opcodes.H_Getfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Getstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Putfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Putstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokevirtual" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokestatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokespecial" />
        ///     ,
        ///     <see cref="Opcodes.H_Newinvokespecial" />
        ///     or
        ///     <see cref="Opcodes.H_Invokeinterface" />
        ///     .
        /// </param>
        /// <param name="owner">the internal name of the field or method owner class.</param>
        /// <param name="name">the name of the field or method.</param>
        /// <param name="descriptor">the descriptor of the field or method.</param>
        /// <returns>the index of a new or already existing method type reference item.</returns>
        [Obsolete(@"this method is superseded by NewHandle(int, string, string, string, bool) ."
        )]
        public virtual int NewHandle(int tag, string owner, string name, string descriptor
        )
        {
            return NewHandle(tag, owner, name, descriptor, tag == OpcodesConstants.H_Invokeinterface
            );
        }

        /// <summary>Adds a handle to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a handle to the constant pool of the class being build. Does nothing if the constant pool
        ///     already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes,
        ///         and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="tag">
        ///     the kind of this handle. Must be
        ///     <see cref="Opcodes.H_Getfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Getstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Putfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Putstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokevirtual" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokestatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokespecial" />
        ///     ,
        ///     <see cref="Opcodes.H_Newinvokespecial" />
        ///     or
        ///     <see cref="Opcodes.H_Invokeinterface" />
        ///     .
        /// </param>
        /// <param name="owner">the internal name of the field or method owner class.</param>
        /// <param name="name">the name of the field or method.</param>
        /// <param name="descriptor">the descriptor of the field or method.</param>
        /// <param name="isInterface">true if the owner is an interface.</param>
        /// <returns>the index of a new or already existing method type reference item.</returns>
        public virtual int NewHandle(int tag, string owner, string name, string descriptor
            , bool isInterface)
        {
            return symbolTable.AddConstantMethodHandle(tag, owner, name, descriptor, isInterface
            ).index;
        }

        /// <summary>
        ///     Adds a dynamic constant reference to the constant pool of the class being build.
        /// </summary>
        /// <remarks>
        ///     Adds a dynamic constant reference to the constant pool of the class being build. Does nothing
        ///     if the constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="name">name of the invoked method.</param>
        /// <param name="descriptor">field descriptor of the constant type.</param>
        /// <param name="bootstrapMethodHandle">the bootstrap method.</param>
        /// <param name="bootstrapMethodArguments">the bootstrap method constant arguments.</param>
        /// <returns>the index of a new or already existing dynamic constant reference item.</returns>
        public virtual int NewConstantDynamic(string name, string descriptor, Handle bootstrapMethodHandle
            , params object[] bootstrapMethodArguments)
        {
            return symbolTable.AddConstantDynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            ).index;
        }

        /// <summary>
        ///     Adds an invokedynamic reference to the constant pool of the class being build.
        /// </summary>
        /// <remarks>
        ///     Adds an invokedynamic reference to the constant pool of the class being build. Does nothing if
        ///     the constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="name">name of the invoked method.</param>
        /// <param name="descriptor">descriptor of the invoke method.</param>
        /// <param name="bootstrapMethodHandle">the bootstrap method.</param>
        /// <param name="bootstrapMethodArguments">the bootstrap method constant arguments.</param>
        /// <returns>the index of a new or already existing invokedynamic reference item.</returns>
        public virtual int NewInvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle
            , params object[] bootstrapMethodArguments)
        {
            return symbolTable.AddConstantInvokeDynamic(name, descriptor, bootstrapMethodHandle
                , bootstrapMethodArguments).index;
        }

        /// <summary>Adds a field reference to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a field reference to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="owner">the internal name of the field's owner class.</param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">the field's descriptor.</param>
        /// <returns>the index of a new or already existing field reference item.</returns>
        public virtual int NewField(string owner, string name, string descriptor)
        {
            return symbolTable.AddConstantFieldref(owner, name, descriptor).index;
        }

        /// <summary>Adds a method reference to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a method reference to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="owner">the internal name of the method's owner class.</param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">the method's descriptor.</param>
        /// <param name="isInterface">
        ///     <literal>true</literal>
        ///     if
        ///     <paramref name="owner" />
        ///     is an interface.
        /// </param>
        /// <returns>the index of a new or already existing method reference item.</returns>
        public virtual int NewMethod(string owner, string name, string descriptor, bool isInterface
        )
        {
            return symbolTable.AddConstantMethodref(owner, name, descriptor, isInterface).index;
        }

        /// <summary>Adds a name and type to the constant pool of the class being build.</summary>
        /// <remarks>
        ///     Adds a name and type to the constant pool of the class being build. Does nothing if the
        ///     constant pool already contains a similar item.
        ///     <i>
        ///         This method is intended for
        ///         <see cref="Attribute" />
        ///         sub classes, and is normally not needed by class generators or adapters.
        ///     </i>
        /// </remarks>
        /// <param name="name">a name.</param>
        /// <param name="descriptor">a type descriptor.</param>
        /// <returns>the index of a new or already existing name and type item.</returns>
        public virtual int NewNameType(string name, string descriptor)
        {
            return symbolTable.AddConstantNameAndType(name, descriptor);
        }

        // -----------------------------------------------------------------------------------------------
        // Default method to compute common super classes when computing stack map frames
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns the common super type of the two given types.</summary>
        /// <remarks>
        ///     Returns the common super type of the two given types. The default implementation of this method
        ///     <i>loads</i> the two given classes and uses the java.lang.Class methods to find the common
        ///     super class. It can be overridden to compute this common super type in other ways, in
        ///     particular without actually loading any class, or to take into account the class that is
        ///     currently being generated by this ClassWriter, which can of course not be loaded since it is
        ///     under construction.
        /// </remarks>
        /// <param name="type1">the internal name of a class.</param>
        /// <param name="type2">the internal name of another class.</param>
        /// <returns>the internal name of the common super class of the two given classes.</returns>
        protected internal virtual string GetCommonSuperClass(string type1, string type2)
        {
            var class1 = global::System.Type.GetType(type1.Replace('/', '.'));

            var class2 = global::System.Type.GetType(type2.Replace('/', '.'));
            if (Runtime.IsAssignableFrom(class1, class2)) return type1;
            if (Runtime.IsAssignableFrom(class2, class1)) return type2;
            if (class1.IsInterface || class2.IsInterface) return "java/lang/Object";

            do
            {
                class1 = class1.BaseType;
            } while (!Runtime.IsAssignableFrom(class1, class2));

            return class1.FullName.Replace('.', '/');
        }
    }

    public class TypeNotPresentException : Exception
    {
        public TypeNotPresentException(string type, TypeLoadException exception)
        {
            Type = type;
            Exception = exception;
        }

        public string Type { get; }
        public TypeLoadException Exception { get; }
    }
}