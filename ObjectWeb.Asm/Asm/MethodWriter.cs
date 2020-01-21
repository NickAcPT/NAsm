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

namespace ObjectWeb.Asm
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     that generates a corresponding 'method_info' structure, as defined in the
    ///     Java Virtual Machine Specification (JVMS).
    /// </summary>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.6">
    ///         JVMS
    ///         *     4.6
    ///     </a>
    /// </seealso>
    /// <author>Eric Bruneton</author>
    /// <author>Eugene Kuleshov</author>
    internal sealed class MethodWriter : MethodVisitor
    {
        /// <summary>Indicates that nothing must be computed.</summary>
        internal const int Compute_Nothing = 0;

        /// <summary>
        ///     Indicates that the maximum stack size and the maximum number of local variables must be
        ///     computed, from scratch.
        /// </summary>
        internal const int Compute_Max_Stack_And_Local = 1;

        /// <summary>
        ///     Indicates that the maximum stack size and the maximum number of local variables must be
        ///     computed, from the existing stack map frames.
        /// </summary>
        /// <remarks>
        ///     Indicates that the maximum stack size and the maximum number of local variables must be
        ///     computed, from the existing stack map frames. This can be done more efficiently than with the
        ///     control flow graph algorithm used for
        ///     <see cref="Compute_Max_Stack_And_Local" />
        ///     , by using a linear
        ///     scan of the bytecode instructions.
        /// </remarks>
        internal const int Compute_Max_Stack_And_Local_From_Frames = 2;

        /// <summary>Indicates that the stack map frames of type F_INSERT must be computed.</summary>
        /// <remarks>
        ///     Indicates that the stack map frames of type F_INSERT must be computed. The other frames are not
        ///     computed. They should all be of type F_NEW and should be sufficient to compute the content of
        ///     the F_INSERT frames, together with the bytecode instructions between a F_NEW and a F_INSERT
        ///     frame - and without any knowledge of the type hierarchy (by definition of F_INSERT).
        /// </remarks>
        internal const int Compute_Inserted_Frames = 3;

        /// <summary>Indicates that all the stack map frames must be computed.</summary>
        /// <remarks>
        ///     Indicates that all the stack map frames must be computed. In this case the maximum stack size
        ///     and the maximum number of local variables is also computed.
        /// </remarks>
        internal const int Compute_All_Frames = 4;

        /// <summary>
        ///     Indicates that
        ///     <see cref="Stack_Size_Delta" />
        ///     is not applicable (not constant or never used).
        /// </summary>
        private const int Na = 0;

        /// <summary>The stack size variation corresponding to each JVM opcode.</summary>
        /// <remarks>
        ///     The stack size variation corresponding to each JVM opcode. The stack size variation for opcode
        ///     'o' is given by the array element at index 'o'.
        /// </remarks>
        /// <seealso>
        ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html">JVMS 6</a>
        /// </seealso>
        private static readonly int[] Stack_Size_Delta =
        {
            0, 1, 1, 1, 1, 1, 1,
            1, 1, 2, 2, 1, 1, 1, 2, 2, 1, 1, 1, Na, Na, 1, 2, 1, 2, 1, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na,
            Na, Na, Na, Na, Na, Na, Na, Na, -1, 0, -1, 0, -1, -1,
            -1, -1, -1, -2, -1, -2, -1, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na,
            Na, Na, Na, Na, Na, Na, Na, -3, -4, -3, -4, -3, -3, -3, -3, -1, -2, 1, 1, 1, 2,
            2, 2, 0, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2,
            -1, -2, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -2, -1, -2, -1, -2, 0, 1, 0, 1,
            -1, -1, 0, 0, 1, 1, -1, 0, -1, 0, 0, 0, -3, -1, -1, -3, -3, -1, -1, -1, -1, -1,
            -1, -2, -2, -2, -2, -2, -2, -2, -2, 0, 1, 0, -1, -1, -1, -2, -1, -2, -1, 0, Na,
            Na, Na, Na, Na, Na, Na, Na, Na, 1, 0, 0, 0, Na, 0, 0, -1, -1, Na, Na, -1, -1, Na, Na
        };

        /// <summary>The access_flags field of the method_info JVMS structure.</summary>
        /// <remarks>
        ///     The access_flags field of the method_info JVMS structure. This field can contain ASM specific
        ///     access flags, such as
        ///     <see cref="Opcodes.Acc_Deprecated" />
        ///     , which are removed when generating the
        ///     ClassFile structure.
        /// </remarks>
        private readonly int accessFlags;

        /// <summary>The 'code' field of the Code attribute.</summary>
        private readonly ByteVector code = new ByteVector();

        /// <summary>Indicates what must be computed.</summary>
        /// <remarks>
        ///     Indicates what must be computed. Must be one of
        ///     <see cref="Compute_All_Frames" />
        ///     ,
        ///     <see cref="Compute_Inserted_Frames" />
        ///     ,
        ///     <see cref="Compute_Max_Stack_And_Local" />
        ///     or
        ///     <see cref="Compute_Nothing" />
        ///     .
        /// </remarks>
        private readonly int compute;

        /// <summary>The descriptor of this method.</summary>
        private readonly string descriptor;

        /// <summary>The descriptor_index field of the method_info JVMS structure.</summary>
        private readonly int descriptorIndex;

        /// <summary>
        ///     The exception_index_table array of the Exceptions attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private readonly int[] exceptionIndexTable;

        /// <summary>The first basic block of the method.</summary>
        /// <remarks>
        ///     The first basic block of the method. The next ones (in bytecode offset order) can be accessed
        ///     with the
        ///     <see cref="Label.nextBasicBlock" />
        ///     field.
        /// </remarks>
        private readonly Label firstBasicBlock;

        /// <summary>The name of this method.</summary>
        private readonly string name;

        /// <summary>The name_index field of the method_info JVMS structure.</summary>
        private readonly int nameIndex;

        /// <summary>The number_of_exceptions field of the Exceptions attribute.</summary>
        private readonly int numberOfExceptions;

        /// <summary>The signature_index field of the Signature attribute.</summary>
        private readonly int signatureIndex;

        /// <summary>Where the constants used in this MethodWriter must be stored.</summary>
        private readonly SymbolTable symbolTable;

        /// <summary>The current basic block, i.e.</summary>
        /// <remarks>
        ///     The current basic block, i.e. the basic block of the last visited instruction. When
        ///     <see cref="compute" />
        ///     is equal to
        ///     <see cref="Compute_Max_Stack_And_Local" />
        ///     or
        ///     <see cref="Compute_All_Frames" />
        ///     , this
        ///     field is
        ///     <literal>null</literal>
        ///     for unreachable code. When
        ///     <see cref="compute" />
        ///     is equal to
        ///     <see cref="Compute_Max_Stack_And_Local_From_Frames" />
        ///     or
        ///     <see cref="Compute_Inserted_Frames" />
        ///     , this field stays
        ///     unchanged throughout the whole method (i.e. the whole code is seen as a single basic block;
        ///     indeed, the existing frames are sufficient by hypothesis to compute any intermediate frame -
        ///     and the maximum stack size as well - without using any control flow graph).
        /// </remarks>
        private Label currentBasicBlock;

        /// <summary>The current stack map frame.</summary>
        /// <remarks>
        ///     The current stack map frame. The first element contains the bytecode offset of the instruction
        ///     to which the frame corresponds, the second element is the number of locals and the third one is
        ///     the number of stack elements. The local variables start at index 3 and are followed by the
        ///     operand stack elements. In summary frame[0] = offset, frame[1] = numLocal, frame[2] = numStack.
        ///     Local variables and operand stack entries contain abstract types, as defined in
        ///     <see cref="Frame" />
        ///     ,
        ///     but restricted to
        ///     <see cref="Frame#CONSTANT_KIND" />
        ///     ,
        ///     <see cref="Frame#REFERENCE_KIND" />
        ///     or
        ///     <see cref="Frame#UNINITIALIZED_KIND" />
        ///     abstract types. Long and double types use only one array entry.
        /// </remarks>
        private int[] currentFrame;

        /// <summary>The number of local variables in the last visited stack map frame.</summary>
        private int currentLocals;

        /// <summary>
        ///     The default_value field of the AnnotationDefault attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector defaultValue;

        /// <summary>The first non standard attribute of this method.</summary>
        /// <remarks>
        ///     The first non standard attribute of this method. The next ones can be accessed with the
        ///     <see cref="Attribute.nextAttribute" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        ///     <p>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in
        ///         <see cref="VisitAttribute(Attribute)" />
        ///         . The
        ///         <see cref="PutMethodInfo(ByteVector)" />
        ///         method writes the attributes in the order defined by this list, i.e. in the
        ///         reverse order specified by the user.
        /// </remarks>
        private Attribute firstAttribute;

        /// <summary>The first non standard attribute of the Code attribute.</summary>
        /// <remarks>
        ///     The first non standard attribute of the Code attribute. The next ones can be accessed with the
        ///     <see cref="Attribute.nextAttribute" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        ///     <p>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in
        ///         <see cref="VisitAttribute(Attribute)" />
        ///         . The
        ///         <see cref="PutMethodInfo(ByteVector)" />
        ///         method writes the attributes in the order defined by this list, i.e. in the
        ///         reverse order specified by the user.
        /// </remarks>
        private Attribute firstCodeAttribute;

        /// <summary>
        ///     The first element in the exception handler list (used to generate the exception_table of the
        ///     Code attribute).
        /// </summary>
        /// <remarks>
        ///     The first element in the exception handler list (used to generate the exception_table of the
        ///     Code attribute). The next ones can be accessed with the
        ///     <see cref="Handler.nextHandler" />
        ///     field. May
        ///     be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private Handler firstHandler;

        /// <summary>Whether the bytecode of this method contains ASM specific instructions.</summary>
        private bool hasAsmInstructions__;

        /// <summary>Whether this method contains subroutines.</summary>
        private bool hasSubroutines;

        /// <summary>
        ///     The number of method parameters that can have runtime visible annotations, or 0.
        /// </summary>
        private int invisibleAnnotableParameterCount;

        /// <summary>The last basic block of the method (in bytecode offset order).</summary>
        /// <remarks>
        ///     The last basic block of the method (in bytecode offset order). This field is updated each time
        ///     a basic block is encountered, and is used to append it at the end of the basic block list.
        /// </remarks>
        private Label lastBasicBlock;

        /// <summary>The start offset of the last visited instruction.</summary>
        /// <remarks>
        ///     The start offset of the last visited instruction. Used to set the offset field of type
        ///     annotations of type 'offset_target' (see &lt;a
        ///     href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.1"&gt;JVMS
        ///     4.7.20.1</a>).
        /// </remarks>
        private int lastBytecodeOffset;

        /// <summary>The last runtime invisible type annotation of the Code attribute.</summary>
        /// <remarks>
        ///     The last runtime invisible type annotation of the Code attribute. The previous ones can be
        ///     accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastCodeRuntimeInvisibleTypeAnnotation;

        /// <summary>The last runtime visible type annotation of the Code attribute.</summary>
        /// <remarks>
        ///     The last runtime visible type annotation of the Code attribute. The previous ones can be
        ///     accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastCodeRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The last element in the exception handler list (used to generate the exception_table of the
        ///     Code attribute).
        /// </summary>
        /// <remarks>
        ///     The last element in the exception handler list (used to generate the exception_table of the
        ///     Code attribute). The next ones can be accessed with the
        ///     <see cref="Handler.nextHandler" />
        ///     field. May
        ///     be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private Handler lastHandler;

        /// <summary>The last runtime invisible annotation of this method.</summary>
        /// <remarks>
        ///     The last runtime invisible annotation of this method. The previous ones can be accessed with
        ///     the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleAnnotation;

        /// <summary>The runtime invisible parameter annotations of this method.</summary>
        /// <remarks>
        ///     The runtime invisible parameter annotations of this method. Each array element contains the
        ///     last annotation of a parameter (which can be
        ///     <literal>null</literal>
        ///     - the previous ones can be
        ///     accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field). May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter[] lastRuntimeInvisibleParameterAnnotations;

        /// <summary>The last runtime invisible type annotation of this method.</summary>
        /// <remarks>
        ///     The last runtime invisible type annotation of this method. The previous ones can be accessed
        ///     with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

        /// <summary>The last runtime visible annotation of this method.</summary>
        /// <remarks>
        ///     The last runtime visible annotation of this method. The previous ones can be accessed with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleAnnotation;

        /// <summary>The runtime visible parameter annotations of this method.</summary>
        /// <remarks>
        ///     The runtime visible parameter annotations of this method. Each array element contains the last
        ///     annotation of a parameter (which can be
        ///     <literal>null</literal>
        ///     - the previous ones can be accessed
        ///     with the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field). May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter[] lastRuntimeVisibleParameterAnnotations;

        /// <summary>The last runtime visible type annotation of this method.</summary>
        /// <remarks>
        ///     The last runtime visible type annotation of this method. The previous ones can be accessed with
        ///     the
        ///     <see cref="AnnotationWriter#previousAnnotation" />
        ///     field. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The line_number_table array of the LineNumberTable code attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector lineNumberTable;

        /// <summary>
        ///     The line_number_table_length field of the LineNumberTable code attribute.
        /// </summary>
        private int lineNumberTableLength;

        /// <summary>
        ///     The local_variable_table array of the LocalVariableTable code attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector localVariableTable;

        /// <summary>
        ///     The local_variable_table_length field of the LocalVariableTable code attribute.
        /// </summary>
        private int localVariableTableLength;

        /// <summary>
        ///     The local_variable_type_table array of the LocalVariableTypeTable code attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector localVariableTypeTable;

        /// <summary>
        ///     The local_variable_type_table_length field of the LocalVariableTypeTable code attribute.
        /// </summary>
        private int localVariableTypeTableLength;

        /// <summary>The max_locals field of the Code attribute.</summary>
        private int maxLocals;

        /// <summary>The maximum relative stack size after the last visited instruction.</summary>
        /// <remarks>
        ///     The maximum relative stack size after the last visited instruction. This size is relative to
        ///     the beginning of
        ///     <see cref="currentBasicBlock" />
        ///     , i.e. the true maximum stack size after the last
        ///     visited instruction is equal to the
        ///     <see cref="Label.inputStackSize" />
        ///     of the current basic block
        ///     plus
        ///     <see cref="maxRelativeStackSize" />
        ///     .When
        ///     <see cref="compute" />
        ///     is equal to
        ///     <see cref="Compute_Max_Stack_And_Local_From_Frames" />
        ///     ,
        ///     <see cref="currentBasicBlock" />
        ///     is always the start of
        ///     the method, so this relative size is also equal to the absolute maximum stack size after the
        ///     last visited instruction.
        /// </remarks>
        private int maxRelativeStackSize;

        /// <summary>The max_stack field of the Code attribute.</summary>
        private int maxStack;

        /// <summary>
        ///     The 'parameters' array of the MethodParameters attribute, or
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        private ByteVector parameters;

        /// <summary>The parameters_count field of the MethodParameters attribute.</summary>
        private int parametersCount;

        /// <summary>
        ///     The last frame that was written in
        ///     <see cref="stackMapTableEntries" />
        ///     . This field has the same
        ///     format as
        ///     <see cref="currentFrame" />
        ///     .
        /// </summary>
        private int[] previousFrame;

        /// <summary>
        ///     The bytecode offset of the last frame that was written in
        ///     <see cref="stackMapTableEntries" />
        ///     .
        /// </summary>
        private int previousFrameOffset;

        /// <summary>The relative stack size after the last visited instruction.</summary>
        /// <remarks>
        ///     The relative stack size after the last visited instruction. This size is relative to the
        ///     beginning of
        ///     <see cref="currentBasicBlock" />
        ///     , i.e. the true stack size after the last visited
        ///     instruction is equal to the
        ///     <see cref="Label.inputStackSize" />
        ///     of the current basic block plus
        ///     <see cref="relativeStackSize" />
        ///     . When
        ///     <see cref="compute" />
        ///     is equal to
        ///     <see cref="Compute_Max_Stack_And_Local_From_Frames" />
        ///     ,
        ///     <see cref="currentBasicBlock" />
        ///     is always the start of
        ///     the method, so this relative size is also equal to the absolute stack size after the last
        ///     visited instruction.
        /// </remarks>
        private int relativeStackSize;

        /// <summary>
        ///     The length in bytes in
        ///     <see cref="SymbolTable.GetSource()" />
        ///     which must be copied to get the
        ///     method_info for this method (excluding its first 6 bytes for access_flags, name_index and
        ///     descriptor_index).
        /// </summary>
        private int sourceLength;

        /// <summary>
        ///     The offset in bytes in
        ///     <see cref="SymbolTable.GetSource()" />
        ///     from which the method_info for this method
        ///     (excluding its first 6 bytes) must be copied, or 0.
        /// </summary>
        private int sourceOffset;

        /// <summary>The 'entries' array of the StackMapTable code attribute.</summary>
        private ByteVector stackMapTableEntries;

        /// <summary>The number_of_entries field of the StackMapTable code attribute.</summary>
        private int stackMapTableNumberOfEntries;

        /// <summary>
        ///     The number of method parameters that can have runtime visible annotations, or 0.
        /// </summary>
        private int visibleAnnotableParameterCount;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="MethodWriter" />
        ///     .
        /// </summary>
        /// <param name="symbolTable">
        ///     where the constants used in this AnnotationWriter must be stored.
        /// </param>
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the method's signature. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="exceptions">
        ///     the internal names of the method's exceptions. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="compute">indicates what must be computed (see #compute).</param>
        internal MethodWriter(SymbolTable symbolTable, int access, string name, string descriptor
            , string signature, string[] exceptions, int compute)
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7)
        {
            // nop = 0 (0x0)
            // aconst_null = 1 (0x1)
            // iconst_m1 = 2 (0x2)
            // iconst_0 = 3 (0x3)
            // iconst_1 = 4 (0x4)
            // iconst_2 = 5 (0x5)
            // iconst_3 = 6 (0x6)
            // iconst_4 = 7 (0x7)
            // iconst_5 = 8 (0x8)
            // lconst_0 = 9 (0x9)
            // lconst_1 = 10 (0xa)
            // fconst_0 = 11 (0xb)
            // fconst_1 = 12 (0xc)
            // fconst_2 = 13 (0xd)
            // dconst_0 = 14 (0xe)
            // dconst_1 = 15 (0xf)
            // bipush = 16 (0x10)
            // sipush = 17 (0x11)
            // ldc = 18 (0x12)
            // ldc_w = 19 (0x13)
            // ldc2_w = 20 (0x14)
            // iload = 21 (0x15)
            // lload = 22 (0x16)
            // fload = 23 (0x17)
            // dload = 24 (0x18)
            // aload = 25 (0x19)
            // iload_0 = 26 (0x1a)
            // iload_1 = 27 (0x1b)
            // iload_2 = 28 (0x1c)
            // iload_3 = 29 (0x1d)
            // lload_0 = 30 (0x1e)
            // lload_1 = 31 (0x1f)
            // lload_2 = 32 (0x20)
            // lload_3 = 33 (0x21)
            // fload_0 = 34 (0x22)
            // fload_1 = 35 (0x23)
            // fload_2 = 36 (0x24)
            // fload_3 = 37 (0x25)
            // dload_0 = 38 (0x26)
            // dload_1 = 39 (0x27)
            // dload_2 = 40 (0x28)
            // dload_3 = 41 (0x29)
            // aload_0 = 42 (0x2a)
            // aload_1 = 43 (0x2b)
            // aload_2 = 44 (0x2c)
            // aload_3 = 45 (0x2d)
            // iaload = 46 (0x2e)
            // laload = 47 (0x2f)
            // faload = 48 (0x30)
            // daload = 49 (0x31)
            // aaload = 50 (0x32)
            // baload = 51 (0x33)
            // caload = 52 (0x34)
            // saload = 53 (0x35)
            // istore = 54 (0x36)
            // lstore = 55 (0x37)
            // fstore = 56 (0x38)
            // dstore = 57 (0x39)
            // astore = 58 (0x3a)
            // istore_0 = 59 (0x3b)
            // istore_1 = 60 (0x3c)
            // istore_2 = 61 (0x3d)
            // istore_3 = 62 (0x3e)
            // lstore_0 = 63 (0x3f)
            // lstore_1 = 64 (0x40)
            // lstore_2 = 65 (0x41)
            // lstore_3 = 66 (0x42)
            // fstore_0 = 67 (0x43)
            // fstore_1 = 68 (0x44)
            // fstore_2 = 69 (0x45)
            // fstore_3 = 70 (0x46)
            // dstore_0 = 71 (0x47)
            // dstore_1 = 72 (0x48)
            // dstore_2 = 73 (0x49)
            // dstore_3 = 74 (0x4a)
            // astore_0 = 75 (0x4b)
            // astore_1 = 76 (0x4c)
            // astore_2 = 77 (0x4d)
            // astore_3 = 78 (0x4e)
            // iastore = 79 (0x4f)
            // lastore = 80 (0x50)
            // fastore = 81 (0x51)
            // dastore = 82 (0x52)
            // aastore = 83 (0x53)
            // bastore = 84 (0x54)
            // castore = 85 (0x55)
            // sastore = 86 (0x56)
            // pop = 87 (0x57)
            // pop2 = 88 (0x58)
            // dup = 89 (0x59)
            // dup_x1 = 90 (0x5a)
            // dup_x2 = 91 (0x5b)
            // dup2 = 92 (0x5c)
            // dup2_x1 = 93 (0x5d)
            // dup2_x2 = 94 (0x5e)
            // swap = 95 (0x5f)
            // iadd = 96 (0x60)
            // ladd = 97 (0x61)
            // fadd = 98 (0x62)
            // dadd = 99 (0x63)
            // isub = 100 (0x64)
            // lsub = 101 (0x65)
            // fsub = 102 (0x66)
            // dsub = 103 (0x67)
            // imul = 104 (0x68)
            // lmul = 105 (0x69)
            // fmul = 106 (0x6a)
            // dmul = 107 (0x6b)
            // idiv = 108 (0x6c)
            // ldiv = 109 (0x6d)
            // fdiv = 110 (0x6e)
            // ddiv = 111 (0x6f)
            // irem = 112 (0x70)
            // lrem = 113 (0x71)
            // frem = 114 (0x72)
            // drem = 115 (0x73)
            // ineg = 116 (0x74)
            // lneg = 117 (0x75)
            // fneg = 118 (0x76)
            // dneg = 119 (0x77)
            // ishl = 120 (0x78)
            // lshl = 121 (0x79)
            // ishr = 122 (0x7a)
            // lshr = 123 (0x7b)
            // iushr = 124 (0x7c)
            // lushr = 125 (0x7d)
            // iand = 126 (0x7e)
            // land = 127 (0x7f)
            // ior = 128 (0x80)
            // lor = 129 (0x81)
            // ixor = 130 (0x82)
            // lxor = 131 (0x83)
            // iinc = 132 (0x84)
            // i2l = 133 (0x85)
            // i2f = 134 (0x86)
            // i2d = 135 (0x87)
            // l2i = 136 (0x88)
            // l2f = 137 (0x89)
            // l2d = 138 (0x8a)
            // f2i = 139 (0x8b)
            // f2l = 140 (0x8c)
            // f2d = 141 (0x8d)
            // d2i = 142 (0x8e)
            // d2l = 143 (0x8f)
            // d2f = 144 (0x90)
            // i2b = 145 (0x91)
            // i2c = 146 (0x92)
            // i2s = 147 (0x93)
            // lcmp = 148 (0x94)
            // fcmpl = 149 (0x95)
            // fcmpg = 150 (0x96)
            // dcmpl = 151 (0x97)
            // dcmpg = 152 (0x98)
            // ifeq = 153 (0x99)
            // ifne = 154 (0x9a)
            // iflt = 155 (0x9b)
            // ifge = 156 (0x9c)
            // ifgt = 157 (0x9d)
            // ifle = 158 (0x9e)
            // if_icmpeq = 159 (0x9f)
            // if_icmpne = 160 (0xa0)
            // if_icmplt = 161 (0xa1)
            // if_icmpge = 162 (0xa2)
            // if_icmpgt = 163 (0xa3)
            // if_icmple = 164 (0xa4)
            // if_acmpeq = 165 (0xa5)
            // if_acmpne = 166 (0xa6)
            // goto = 167 (0xa7)
            // jsr = 168 (0xa8)
            // ret = 169 (0xa9)
            // tableswitch = 170 (0xaa)
            // lookupswitch = 171 (0xab)
            // ireturn = 172 (0xac)
            // lreturn = 173 (0xad)
            // freturn = 174 (0xae)
            // dreturn = 175 (0xaf)
            // areturn = 176 (0xb0)
            // return = 177 (0xb1)
            // getstatic = 178 (0xb2)
            // putstatic = 179 (0xb3)
            // getfield = 180 (0xb4)
            // putfield = 181 (0xb5)
            // invokevirtual = 182 (0xb6)
            // invokespecial = 183 (0xb7)
            // invokestatic = 184 (0xb8)
            // invokeinterface = 185 (0xb9)
            // invokedynamic = 186 (0xba)
            // new = 187 (0xbb)
            // newarray = 188 (0xbc)
            // anewarray = 189 (0xbd)
            // arraylength = 190 (0xbe)
            // athrow = 191 (0xbf)
            // checkcast = 192 (0xc0)
            // instanceof = 193 (0xc1)
            // monitorenter = 194 (0xc2)
            // monitorexit = 195 (0xc3)
            // wide = 196 (0xc4)
            // multianewarray = 197 (0xc5)
            // ifnull = 198 (0xc6)
            // ifnonnull = 199 (0xc7)
            // goto_w = 200 (0xc8)
            // jsr_w = 201 (0xc9)
            // Note: fields are ordered as in the method_info structure, and those related to attributes are
            // ordered as in Section 4.7 of the JVMS.
            // Code attribute fields and sub attributes:
            // Other method_info attributes:
            // -----------------------------------------------------------------------------------------------
            // Fields used to compute the maximum stack size and number of locals, and the stack map frames
            // -----------------------------------------------------------------------------------------------
            // -----------------------------------------------------------------------------------------------
            // Other miscellaneous status fields
            // -----------------------------------------------------------------------------------------------
            // -----------------------------------------------------------------------------------------------
            // Constructor and accessors
            // -----------------------------------------------------------------------------------------------
            /* latest api = */
            this.symbolTable = symbolTable;
            accessFlags = "<init>".Equals(name) ? access | Constants.Acc_Constructor : access;
            nameIndex = symbolTable.AddConstantUtf8(name);
            this.name = name;
            descriptorIndex = symbolTable.AddConstantUtf8(descriptor);
            this.descriptor = descriptor;
            signatureIndex = signature == null
                ? 0
                : symbolTable.AddConstantUtf8(signature
                );
            if (exceptions != null && exceptions.Length > 0)
            {
                numberOfExceptions = exceptions.Length;
                exceptionIndexTable = new int[numberOfExceptions];
                for (var i = 0; i < numberOfExceptions; ++i)
                    exceptionIndexTable[i] = symbolTable.AddConstantClass(exceptions[i]).index;
            }
            else
            {
                numberOfExceptions = 0;
                exceptionIndexTable = null;
            }

            this.compute = compute;
            if (compute != Compute_Nothing)
            {
                // Update maxLocals and currentLocals.
                var argumentsSize = Type.GetArgumentsAndReturnSizes(descriptor) >> 2;
                if ((access & OpcodesConstants.Acc_Static) != 0) --argumentsSize;
                maxLocals = argumentsSize;
                currentLocals = argumentsSize;
                // Create and visit the label for the first basic block.
                firstBasicBlock = new Label();
                VisitLabel(firstBasicBlock);
            }
        }

        internal bool HasFrames()
        {
            return stackMapTableNumberOfEntries > 0;
        }

        internal bool HasAsmInstructions()
        {
            return hasAsmInstructions__;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the MethodVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override void VisitParameter(string name, int access)
        {
            if (parameters == null) parameters = new ByteVector();
            ++parametersCount;
            parameters.PutShort(name == null ? 0 : symbolTable.AddConstantUtf8(name)).PutShort
                (access);
        }

        public override AnnotationVisitor VisitAnnotationDefault()
        {
            defaultValue = new ByteVector();
            return new AnnotationWriter(symbolTable, false, defaultValue, null);
        }

        /* useNamedValues = */
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

        public override void VisitAnnotableParameterCount(int parameterCount, bool visible
        )
        {
            if (visible)
                visibleAnnotableParameterCount = parameterCount;
            else
                invisibleAnnotableParameterCount = parameterCount;
        }

        public override AnnotationVisitor VisitParameterAnnotation(int parameter, string
            annotationDescriptor, bool visible)
        {
            if (visible)
            {
                if (lastRuntimeVisibleParameterAnnotations == null)
                    lastRuntimeVisibleParameterAnnotations = new AnnotationWriter[Type.GetArgumentTypes
                        (descriptor).Length];
                return lastRuntimeVisibleParameterAnnotations[parameter] = AnnotationWriter.Create
                (symbolTable, annotationDescriptor, lastRuntimeVisibleParameterAnnotations[parameter
                ]);
            }

            if (lastRuntimeInvisibleParameterAnnotations == null)
                lastRuntimeInvisibleParameterAnnotations = new AnnotationWriter[Type.GetArgumentTypes
                    (descriptor).Length];
            return lastRuntimeInvisibleParameterAnnotations[parameter] = AnnotationWriter.Create
            (symbolTable, annotationDescriptor, lastRuntimeInvisibleParameterAnnotations[parameter
            ]);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            // Store the attributes in the <i>reverse</i> order of their visit by this method.
            if (attribute.IsCodeAttribute())
            {
                attribute.nextAttribute = firstCodeAttribute;
                firstCodeAttribute = attribute;
            }
            else
            {
                attribute.nextAttribute = firstAttribute;
                firstAttribute = attribute;
            }
        }

        public override void VisitCode()
        {
        }

        // Nothing to do.
        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            if (compute == Compute_All_Frames) return;
            if (compute == Compute_Inserted_Frames)
            {
                if (currentBasicBlock.frame == null)
                {
                    // This should happen only once, for the implicit first frame (which is explicitly visited
                    // in ClassReader if the EXPAND_ASM_INSNS option is used - and COMPUTE_INSERTED_FRAMES
                    // can't be set if EXPAND_ASM_INSNS is not used).
                    currentBasicBlock.frame = new CurrentFrame(currentBasicBlock);
                    currentBasicBlock.frame.SetInputFrameFromDescriptor(symbolTable, accessFlags, descriptor
                        , numLocal);
                    currentBasicBlock.frame.Accept(this);
                }
                else
                {
                    if (type == OpcodesConstants.F_New)
                        currentBasicBlock.frame.SetInputFrameFromApiFormat(symbolTable, numLocal, local,
                            numStack, stack);
                    // If type is not F_NEW then it is F_INSERT by hypothesis, and currentBlock.frame contains
                    // the stack map frame at the current instruction, computed from the last F_NEW frame and
                    // the bytecode instructions in between (via calls to CurrentFrame#execute).
                    currentBasicBlock.frame.Accept(this);
                }
            }
            else if (type == OpcodesConstants.F_New)
            {
                if (previousFrame == null)
                {
                    var argumentsSize = Type.GetArgumentsAndReturnSizes(descriptor) >> 2;
                    var implicitFirstFrame = new Frame(new Label());
                    implicitFirstFrame.SetInputFrameFromDescriptor(symbolTable, accessFlags, descriptor
                        , argumentsSize);
                    implicitFirstFrame.Accept(this);
                }

                currentLocals = numLocal;
                var frameIndex = VisitFrameStart(code.length, numLocal, numStack);
                for (var i = 0; i < numLocal; ++i)
                    currentFrame[frameIndex++] = Frame.GetAbstractTypeFromApiFormat(symbolTable, local
                        [i]);
                for (var i = 0; i < numStack; ++i)
                    currentFrame[frameIndex++] = Frame.GetAbstractTypeFromApiFormat(symbolTable, stack
                        [i]);
                VisitFrameEnd();
            }
            else
            {
                if (symbolTable.GetMajorVersion() < OpcodesConstants.V1_6)
                    throw new ArgumentException("Class versions V1_5 or less must use F_NEW frames.");
                int offsetDelta;
                if (stackMapTableEntries == null)
                {
                    stackMapTableEntries = new ByteVector();
                    offsetDelta = code.length;
                }
                else
                {
                    offsetDelta = code.length - previousFrameOffset - 1;
                    if (offsetDelta < 0)
                    {
                        if (type == OpcodesConstants.F_Same)
                            return;
                        throw new InvalidOperationException();
                    }
                }

                switch (type)
                {
                    case OpcodesConstants.F_Full:
                    {
                        currentLocals = numLocal;
                        stackMapTableEntries.PutByte(Frame.Full_Frame).PutShort(offsetDelta).PutShort(numLocal
                        );
                        for (var i = 0; i < numLocal; ++i) PutFrameType(local[i]);
                        stackMapTableEntries.PutShort(numStack);
                        for (var i = 0; i < numStack; ++i) PutFrameType(stack[i]);
                        break;
                    }

                    case OpcodesConstants.F_Append:
                    {
                        currentLocals += numLocal;
                        stackMapTableEntries.PutByte(Frame.Same_Frame_Extended + numLocal).PutShort(offsetDelta
                        );
                        for (var i = 0; i < numLocal; ++i) PutFrameType(local[i]);
                        break;
                    }

                    case OpcodesConstants.F_Chop:
                    {
                        currentLocals -= numLocal;
                        stackMapTableEntries.PutByte(Frame.Same_Frame_Extended - numLocal).PutShort(offsetDelta
                        );
                        break;
                    }

                    case OpcodesConstants.F_Same:
                    {
                        if (offsetDelta < 64)
                            stackMapTableEntries.PutByte(offsetDelta);
                        else
                            stackMapTableEntries.PutByte(Frame.Same_Frame_Extended).PutShort(offsetDelta);
                        break;
                    }

                    case OpcodesConstants.F_Same1:
                    {
                        if (offsetDelta < 64)
                            stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame + offsetDelta);
                        else
                            stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame_Extended).PutShort
                                (offsetDelta);
                        PutFrameType(stack[0]);
                        break;
                    }

                    default:
                    {
                        throw new ArgumentException();
                    }
                }

                previousFrameOffset = code.length;
                ++stackMapTableNumberOfEntries;
            }

            if (compute == Compute_Max_Stack_And_Local_From_Frames)
            {
                relativeStackSize = numStack;
                for (var i = 0; i < numStack; ++i)
                    if (stack[i] == (object) OpcodesConstants.Long || stack[i] == (object) OpcodesConstants.Double)
                        relativeStackSize++;
                if (relativeStackSize > maxRelativeStackSize) maxRelativeStackSize = relativeStackSize;
            }

            maxStack = Math.Max(maxStack, numStack);
            maxLocals = Math.Max(maxLocals, currentLocals);
        }

        public override void VisitInsn(int opcode)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            code.PutByte(opcode);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(opcode, 0, null, null);
                }
                else
                {
                    var size = relativeStackSize + Stack_Size_Delta[opcode];
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }

                if (opcode >= OpcodesConstants.Ireturn && opcode <= OpcodesConstants.Return ||
                    opcode == OpcodesConstants.Athrow)
                    EndCurrentBasicBlockWithNoSuccessor();
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            if (opcode == OpcodesConstants.Sipush)
                code.Put12(opcode, operand);
            else
                // BIPUSH or NEWARRAY
                code.Put11(opcode, operand);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(opcode, operand, null, null);
                }
                else if (opcode != OpcodesConstants.Newarray)
                {
                    // The stack size delta is 1 for BIPUSH or SIPUSH, and 0 for NEWARRAY.
                    var size = relativeStackSize + 1;
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            if (var < 4 && opcode != OpcodesConstants.Ret)
            {
                int optimizedOpcode;
                if (opcode < OpcodesConstants.Istore)
                    optimizedOpcode = Constants.Iload_0 + ((opcode - OpcodesConstants.Iload) << 2) +
                                      var;
                else
                    optimizedOpcode = Constants.Istore_0 + ((opcode - OpcodesConstants.Istore) << 2)
                                                         + var;
                code.PutByte(optimizedOpcode);
            }
            else if (var >= 256)
            {
                code.PutByte(Constants.Wide).Put12(opcode, var);
            }
            else
            {
                code.Put11(opcode, var);
            }

            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(opcode, var, null, null);
                }
                else if (opcode == OpcodesConstants.Ret)
                {
                    // No stack size delta.
                    currentBasicBlock.flags |= Label.Flag_Subroutine_End;
                    currentBasicBlock.outputStackSize = (short) relativeStackSize;
                    EndCurrentBasicBlockWithNoSuccessor();
                }
                else
                {
                    // xLOAD or xSTORE
                    var size = relativeStackSize + Stack_Size_Delta[opcode];
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }

            if (compute != Compute_Nothing)
            {
                int currentMaxLocals;
                if (opcode == OpcodesConstants.Lload || opcode == OpcodesConstants.Dload || opcode
                    == OpcodesConstants.Lstore || opcode == OpcodesConstants.Dstore)
                    currentMaxLocals = var + 2;
                else
                    currentMaxLocals = var + 1;
                if (currentMaxLocals > maxLocals) maxLocals = currentMaxLocals;
            }

            if (opcode >= OpcodesConstants.Istore && compute == Compute_All_Frames && firstHandler
                != null)
                // If there are exception handler blocks, each instruction within a handler range is, in
                // theory, a basic block (since execution can jump from this instruction to the exception
                // handler). As a consequence, the local variable types at the beginning of the handler
                // block should be the merge of the local variable types at all the instructions within the
                // handler range. However, instead of creating a basic block for each instruction, we can
                // get the same result in a more efficient way. Namely, by starting a new basic block after
                // each xSTORE instruction, which is what we do here.
                VisitLabel(new Label());
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            var typeSymbol = symbolTable.AddConstantClass(type);
            code.Put12(opcode, typeSymbol.index);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(opcode, lastBytecodeOffset, typeSymbol, symbolTable
                    );
                }
                else if (opcode == OpcodesConstants.New)
                {
                    // The stack size delta is 1 for NEW, and 0 for ANEWARRAY, CHECKCAST, or INSTANCEOF.
                    var size = relativeStackSize + 1;
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            var fieldrefSymbol = symbolTable.AddConstantFieldref(owner, name, descriptor);
            code.Put12(opcode, fieldrefSymbol.index);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(opcode, 0, fieldrefSymbol, symbolTable);
                }
                else
                {
                    int size;
                    var firstDescChar = descriptor[0];
                    switch (opcode)
                    {
                        case OpcodesConstants.Getstatic:
                        {
                            size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? 2 : 1);
                            break;
                        }

                        case OpcodesConstants.Putstatic:
                        {
                            size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J'
                                       ? -2
                                       : -
                                           1);
                            break;
                        }

                        case OpcodesConstants.Getfield:
                        {
                            size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? 1 : 0);
                            break;
                        }

                        case OpcodesConstants.Putfield:
                        default:
                        {
                            size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J'
                                       ? -3
                                       : -
                                           2);
                            break;
                        }
                    }

                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }
        }

        public override void VisitMethodInsn(int opcode, string owner, string name, string
            descriptor, bool isInterface)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            var methodrefSymbol = symbolTable.AddConstantMethodref(owner, name, descriptor
                , isInterface);
            if (opcode == OpcodesConstants.Invokeinterface)
                code.Put12(OpcodesConstants.Invokeinterface, methodrefSymbol.index).Put11(methodrefSymbol
                                                                                              .GetArgumentsAndReturnSizes() >>
                                                                                          2, 0);
            else
                code.Put12(opcode, methodrefSymbol.index);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(opcode, 0, methodrefSymbol, symbolTable);
                }
                else
                {
                    var argumentsAndReturnSize = methodrefSymbol.GetArgumentsAndReturnSizes();
                    var stackSizeDelta = (argumentsAndReturnSize & 3) - (argumentsAndReturnSize >> 2);
                    int size;
                    if (opcode == OpcodesConstants.Invokestatic)
                        size = relativeStackSize + stackSizeDelta + 1;
                    else
                        size = relativeStackSize + stackSizeDelta;
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            var invokeDynamicSymbol = symbolTable.AddConstantInvokeDynamic(name, descriptor
                , bootstrapMethodHandle, bootstrapMethodArguments);
            code.Put12(OpcodesConstants.Invokedynamic, invokeDynamicSymbol.index);
            code.PutShort(0);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(OpcodesConstants.Invokedynamic, 0, invokeDynamicSymbol
                        , symbolTable);
                }
                else
                {
                    var argumentsAndReturnSize = invokeDynamicSymbol.GetArgumentsAndReturnSizes();
                    var stackSizeDelta = (argumentsAndReturnSize & 3) - (argumentsAndReturnSize >> 2)
                                         + 1;
                    var size = relativeStackSize + stackSizeDelta;
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            // Compute the 'base' opcode, i.e. GOTO or JSR if opcode is GOTO_W or JSR_W, otherwise opcode.
            var baseOpcode = opcode >= Constants.Goto_W
                ? opcode - Constants.Wide_Jump_Opcode_Delta
                : opcode;
            var nextInsnIsJumpTarget = false;
            if ((label.flags & Label.Flag_Resolved) != 0 && label.bytecodeOffset - code.length
                < short.MinValue)
            {
                // Case of a backward jump with an offset < -32768. In this case we automatically replace GOTO
                // with GOTO_W, JSR with JSR_W and IFxxx <l> with IFNOTxxx <L> GOTO_W <l> L:..., where
                // IFNOTxxx is the "opposite" opcode of IFxxx (e.g. IFNE for IFEQ) and where <L> designates
                // the instruction just after the GOTO_W.
                if (baseOpcode == OpcodesConstants.Goto)
                {
                    code.PutByte(Constants.Goto_W);
                }
                else if (baseOpcode == OpcodesConstants.Jsr)
                {
                    code.PutByte(Constants.Jsr_W);
                }
                else
                {
                    // Put the "opposite" opcode of baseOpcode. This can be done by flipping the least
                    // significant bit for IFNULL and IFNONNULL, and similarly for IFEQ ... IF_ACMPEQ (with a
                    // pre and post offset by 1). The jump offset is 8 bytes (3 for IFNOTxxx, 5 for GOTO_W).
                    code.PutByte(baseOpcode >= OpcodesConstants.Ifnull
                        ? baseOpcode ^ 1
                        : ((baseOpcode
                            + 1) ^ 1) - 1);
                    code.PutShort(8);
                    // Here we could put a GOTO_W in theory, but if ASM specific instructions are used in this
                    // method or another one, and if the class has frames, we will need to insert a frame after
                    // this GOTO_W during the additional ClassReader -> ClassWriter round trip to remove the ASM
                    // specific instructions. To not miss this additional frame, we need to use an ASM_GOTO_W
                    // here, which has the unfortunate effect of forcing this additional round trip (which in
                    // some case would not have been really necessary, but we can't know this at this point).
                    code.PutByte(Constants.Asm_Goto_W);
                    hasAsmInstructions__ = true;
                    // The instruction after the GOTO_W becomes the target of the IFNOT instruction.
                    nextInsnIsJumpTarget = true;
                }

                label.Put(code, code.length - 1, true);
            }
            else if (baseOpcode != opcode)
            {
                // Case of a GOTO_W or JSR_W specified by the user (normally ClassReader when used to remove
                // ASM specific instructions). In this case we keep the original instruction.
                code.PutByte(opcode);
                label.Put(code, code.length - 1, true);
            }
            else
            {
                // Case of a jump with an offset >= -32768, or of a jump with an unknown offset. In these
                // cases we store the offset in 2 bytes (which will be increased via a ClassReader ->
                // ClassWriter round trip if it turns out that 2 bytes are not sufficient).
                code.PutByte(baseOpcode);
                label.Put(code, code.length - 1, false);
            }

            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                Label nextBasicBlock = null;
                if (compute == Compute_All_Frames)
                {
                    currentBasicBlock.frame.Execute(baseOpcode, 0, null, null);
                    // Record the fact that 'label' is the target of a jump instruction.
                    label.GetCanonicalInstance().flags |= Label.Flag_Jump_Target;
                    // Add 'label' as a successor of the current basic block.
                    AddSuccessorToCurrentBasicBlock(Edge.Jump, label);
                    if (baseOpcode != OpcodesConstants.Goto)
                        // The next instruction starts a new basic block (except for GOTO: by default the code
                        // following a goto is unreachable - unless there is an explicit label for it - and we
                        // should not compute stack frame types for its instructions).
                        nextBasicBlock = new Label();
                }
                else if (compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(baseOpcode, 0, null, null);
                }
                else if (compute == Compute_Max_Stack_And_Local_From_Frames)
                {
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    relativeStackSize += Stack_Size_Delta[baseOpcode];
                }
                else if (baseOpcode == OpcodesConstants.Jsr)
                {
                    // Record the fact that 'label' designates a subroutine, if not already done.
                    if ((label.flags & Label.Flag_Subroutine_Start) == 0)
                    {
                        label.flags |= Label.Flag_Subroutine_Start;
                        hasSubroutines = true;
                    }

                    currentBasicBlock.flags |= Label.Flag_Subroutine_Caller;
                    // Note that, by construction in this method, a block which calls a subroutine has at
                    // least two successors in the control flow graph: the first one (added below) leads to
                    // the instruction after the JSR, while the second one (added here) leads to the JSR
                    // target. Note that the first successor is virtual (it does not correspond to a possible
                    // execution path): it is only used to compute the successors of the basic blocks ending
                    // with a ret, in {@link Label#addSubroutineRetSuccessors}.
                    AddSuccessorToCurrentBasicBlock(relativeStackSize + 1, label);
                    // The instruction after the JSR starts a new basic block.
                    nextBasicBlock = new Label();
                }
                else
                {
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    relativeStackSize += Stack_Size_Delta[baseOpcode];
                    AddSuccessorToCurrentBasicBlock(relativeStackSize, label);
                }

                // If the next instruction starts a new basic block, call visitLabel to add the label of this
                // instruction as a successor of the current block, and to start a new basic block.
                if (nextBasicBlock != null)
                {
                    if (nextInsnIsJumpTarget) nextBasicBlock.flags |= Label.Flag_Jump_Target;
                    VisitLabel(nextBasicBlock);
                }

                if (baseOpcode == OpcodesConstants.Goto) EndCurrentBasicBlockWithNoSuccessor();
            }
        }

        public override void VisitLabel(Label label)
        {
            // Resolve the forward references to this label, if any.
            hasAsmInstructions__ |= label.Resolve(code.data, code.length);
            // visitLabel starts a new basic block (except for debug only labels), so we need to update the
            // previous and current block references and list of successors.
            if ((label.flags & Label.Flag_Debug_Only) != 0) return;
            if (compute == Compute_All_Frames)
            {
                if (currentBasicBlock != null)
                {
                    if (label.bytecodeOffset == currentBasicBlock.bytecodeOffset)
                    {
                        // We use {@link Label#getCanonicalInstance} to store the state of a basic block in only
                        // one place, but this does not work for labels which have not been visited yet.
                        // Therefore, when we detect here two labels having the same bytecode offset, we need to
                        // - consolidate the state scattered in these two instances into the canonical instance:
                        currentBasicBlock.flags |= label.flags & Label.Flag_Jump_Target;
                        // - make sure the two instances share the same Frame instance (the implementation of
                        // {@link Label#getCanonicalInstance} relies on this property; here label.frame should be
                        // null):
                        label.frame = currentBasicBlock.frame;
                        // - and make sure to NOT assign 'label' into 'currentBasicBlock' or 'lastBasicBlock', so
                        // that they still refer to the canonical instance for this bytecode offset.
                        return;
                    }

                    // End the current basic block (with one new successor).
                    AddSuccessorToCurrentBasicBlock(Edge.Jump, label);
                }

                // Append 'label' at the end of the basic block list.
                if (lastBasicBlock != null)
                {
                    if (label.bytecodeOffset == lastBasicBlock.bytecodeOffset)
                    {
                        // Same comment as above.
                        lastBasicBlock.flags |= label.flags & Label.Flag_Jump_Target;
                        // Here label.frame should be null.
                        label.frame = lastBasicBlock.frame;
                        currentBasicBlock = lastBasicBlock;
                        return;
                    }

                    lastBasicBlock.nextBasicBlock = label;
                }

                lastBasicBlock = label;
                // Make it the new current basic block.
                currentBasicBlock = label;
                // Here label.frame should be null.
                label.frame = new Frame(label);
            }
            else if (compute == Compute_Inserted_Frames)
            {
                if (currentBasicBlock == null)
                    // This case should happen only once, for the visitLabel call in the constructor. Indeed, if
                    // compute is equal to COMPUTE_INSERTED_FRAMES, currentBasicBlock stays unchanged.
                    currentBasicBlock = label;
                else
                    // Update the frame owner so that a correct frame offset is computed in Frame.accept().
                    currentBasicBlock.frame.owner = label;
            }
            else if (compute == Compute_Max_Stack_And_Local)
            {
                if (currentBasicBlock != null)
                {
                    // End the current basic block (with one new successor).
                    currentBasicBlock.outputStackMax = (short) maxRelativeStackSize;
                    AddSuccessorToCurrentBasicBlock(relativeStackSize, label);
                }

                // Start a new current basic block, and reset the current and maximum relative stack sizes.
                currentBasicBlock = label;
                relativeStackSize = 0;
                maxRelativeStackSize = 0;
                // Append the new basic block at the end of the basic block list.
                if (lastBasicBlock != null) lastBasicBlock.nextBasicBlock = label;
                lastBasicBlock = label;
            }
            else if (compute == Compute_Max_Stack_And_Local_From_Frames && currentBasicBlock
                     == null)
            {
                // This case should happen only once, for the visitLabel call in the constructor. Indeed, if
                // compute is equal to COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES, currentBasicBlock stays
                // unchanged.
                currentBasicBlock = label;
            }
        }

        public override void VisitLdcInsn(object value)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            var constantSymbol = symbolTable.AddConstant(value);
            var constantIndex = constantSymbol.index;
            char firstDescriptorChar;
            var isLongOrDouble = constantSymbol.tag == Symbol.Constant_Long_Tag || constantSymbol
                                     .tag == Symbol.Constant_Double_Tag ||
                                 constantSymbol.tag == Symbol.Constant_Dynamic_Tag
                                 && ((firstDescriptorChar = constantSymbol.value[0]) == 'J' || firstDescriptorChar
                                     == 'D');
            if (isLongOrDouble)
                code.Put12(Constants.Ldc2_W, constantIndex);
            else if (constantIndex >= 256)
                code.Put12(Constants.Ldc_W, constantIndex);
            else
                code.Put11(OpcodesConstants.Ldc, constantIndex);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                {
                    currentBasicBlock.frame.Execute(OpcodesConstants.Ldc, 0, constantSymbol, symbolTable
                    );
                }
                else
                {
                    var size = relativeStackSize + (isLongOrDouble ? 2 : 1);
                    if (size > maxRelativeStackSize) maxRelativeStackSize = size;
                    relativeStackSize = size;
                }
            }
        }

        public override void VisitIincInsn(int var, int increment)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            if (var > 255 || increment > 127 || increment < -128)
                code.PutByte(Constants.Wide).Put12(OpcodesConstants.Iinc, var).PutShort(increment
                );
            else
                code.PutByte(OpcodesConstants.Iinc).Put11(var, increment);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null && (compute == Compute_All_Frames || compute == Compute_Inserted_Frames
                ))
                currentBasicBlock.frame.Execute(OpcodesConstants.Iinc, var, null, null);
            if (compute != Compute_Nothing)
            {
                var currentMaxLocals = var + 1;
                if (currentMaxLocals > maxLocals) maxLocals = currentMaxLocals;
            }
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            code.PutByte(OpcodesConstants.Tableswitch).PutByteArray(null, 0, (4 - code.length
                                                                              % 4) % 4);
            dflt.Put(code, lastBytecodeOffset, true);
            code.PutInt(min).PutInt(max);
            foreach (var label in labels) label.Put(code, lastBytecodeOffset, true);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            VisitSwitchInsn(dflt, labels);
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            code.PutByte(OpcodesConstants.Lookupswitch).PutByteArray(null, 0, (4 - code.length
                                                                               % 4) % 4);
            dflt.Put(code, lastBytecodeOffset, true);
            code.PutInt(labels.Length);
            for (var i = 0; i < labels.Length; ++i)
            {
                code.PutInt(keys[i]);
                labels[i].Put(code, lastBytecodeOffset, true);
            }

            // If needed, update the maximum stack size and number of locals, and stack map frames.
            VisitSwitchInsn(dflt, labels);
        }

        private void VisitSwitchInsn(Label dflt, Label[] labels)
        {
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames)
                {
                    currentBasicBlock.frame.Execute(OpcodesConstants.Lookupswitch, 0, null, null);
                    // Add all the labels as successors of the current basic block.
                    AddSuccessorToCurrentBasicBlock(Edge.Jump, dflt);
                    dflt.GetCanonicalInstance().flags |= Label.Flag_Jump_Target;
                    foreach (var label in labels)
                    {
                        AddSuccessorToCurrentBasicBlock(Edge.Jump, label);
                        label.GetCanonicalInstance().flags |= Label.Flag_Jump_Target;
                    }
                }
                else if (compute == Compute_Max_Stack_And_Local)
                {
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    --relativeStackSize;
                    // Add all the labels as successors of the current basic block.
                    AddSuccessorToCurrentBasicBlock(relativeStackSize, dflt);
                    foreach (var label in labels) AddSuccessorToCurrentBasicBlock(relativeStackSize, label);
                }

                // End the current basic block.
                EndCurrentBasicBlockWithNoSuccessor();
            }
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            lastBytecodeOffset = code.length;
            // Add the instruction to the bytecode of the method.
            var descSymbol = symbolTable.AddConstantClass(descriptor);
            code.Put12(OpcodesConstants.Multianewarray, descSymbol.index).PutByte(numDimensions
            );
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (currentBasicBlock != null)
            {
                if (compute == Compute_All_Frames || compute == Compute_Inserted_Frames)
                    currentBasicBlock.frame.Execute(OpcodesConstants.Multianewarray, numDimensions, descSymbol
                        , symbolTable);
                else
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    relativeStackSize += 1 - numDimensions;
            }
        }

        public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            if (visible)
                return lastCodeRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(symbolTable
                    , (typeRef & unchecked((int) 0xFF0000FF)) | (lastBytecodeOffset << 8), typePath
                    , descriptor, lastCodeRuntimeVisibleTypeAnnotation);
            return lastCodeRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(symbolTable
                , (typeRef & unchecked((int) 0xFF0000FF)) | (lastBytecodeOffset << 8), typePath
                , descriptor, lastCodeRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type)
        {
            var newHandler = new Handler(start, end, handler,
                type != null ? symbolTable.AddConstantClass(type).index : 0, type);
            if (firstHandler == null)
                firstHandler = newHandler;
            else
                lastHandler.nextHandler = newHandler;
            lastHandler = newHandler;
        }

        public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            if (visible)
                return lastCodeRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(symbolTable
                    , typeRef, typePath, descriptor, lastCodeRuntimeVisibleTypeAnnotation);
            return lastCodeRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(symbolTable
                , typeRef, typePath, descriptor, lastCodeRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            if (signature != null)
            {
                if (localVariableTypeTable == null) localVariableTypeTable = new ByteVector();
                ++localVariableTypeTableLength;
                localVariableTypeTable.PutShort(start.bytecodeOffset).PutShort(end.bytecodeOffset
                                                                               - start.bytecodeOffset)
                    .PutShort(symbolTable.AddConstantUtf8(name)).PutShort(symbolTable
                        .AddConstantUtf8(signature)).PutShort(index);
            }

            if (localVariableTable == null) localVariableTable = new ByteVector();
            ++localVariableTableLength;
            localVariableTable.PutShort(start.bytecodeOffset).PutShort(end.bytecodeOffset - start
                                                                           .bytecodeOffset)
                .PutShort(symbolTable.AddConstantUtf8(name)).PutShort(symbolTable
                    .AddConstantUtf8(descriptor)).PutShort(index);
            if (compute != Compute_Nothing)
            {
                var firstDescChar = descriptor[0];
                var currentMaxLocals = index + (firstDescChar == 'J' || firstDescChar == 'D' ? 2 : 1);
                if (currentMaxLocals > maxLocals) maxLocals = currentMaxLocals;
            }
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath
                typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible
        )
        {
            // Create a ByteVector to hold a 'type_annotation' JVMS structure.
            // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.
            var typeAnnotation = new ByteVector();
            // Write target_type, target_info, and target_path.
            typeAnnotation.PutByte((int) ((uint) typeRef >> 24)).PutShort(start.Length);
            for (var i = 0; i < start.Length; ++i)
                typeAnnotation.PutShort(start[i].bytecodeOffset).PutShort(end[i].bytecodeOffset -
                                                                          start[i].bytecodeOffset).PutShort(index[i]);
            TypePath.Put(typePath, typeAnnotation);
            // Write type_index and reserve space for num_element_value_pairs.
            typeAnnotation.PutShort(symbolTable.AddConstantUtf8(descriptor)).PutShort(0);
            if (visible)
                return lastCodeRuntimeVisibleTypeAnnotation = new AnnotationWriter(symbolTable, true
                    , typeAnnotation, lastCodeRuntimeVisibleTypeAnnotation);
            return lastCodeRuntimeInvisibleTypeAnnotation = new AnnotationWriter(symbolTable,
                true, typeAnnotation, lastCodeRuntimeInvisibleTypeAnnotation);
        }

        /* useNamedValues = */
        public override void VisitLineNumber(int line, Label start)
        {
            if (lineNumberTable == null) lineNumberTable = new ByteVector();
            ++lineNumberTableLength;
            lineNumberTable.PutShort(start.bytecodeOffset);
            lineNumberTable.PutShort(line);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            if (compute == Compute_All_Frames)
            {
                ComputeAllFrames();
            }
            else if (compute == Compute_Max_Stack_And_Local)
            {
                ComputeMaxStackAndLocal();
            }
            else if (compute == Compute_Max_Stack_And_Local_From_Frames)
            {
                this.maxStack = maxRelativeStackSize;
            }
            else
            {
                this.maxStack = maxStack;
                this.maxLocals = maxLocals;
            }
        }

        /// <summary>Computes all the stack map frames of the method, from scratch.</summary>
        private void ComputeAllFrames()
        {
            // Complete the control flow graph with exception handler blocks.
            var handler = firstHandler;
            while (handler != null)
            {
                var catchTypeDescriptor = handler.catchTypeDescriptor == null
                    ? "java/lang/Throwable"
                    : handler.catchTypeDescriptor;
                var catchType = Frame.GetAbstractTypeFromInternalName(symbolTable, catchTypeDescriptor
                );
                // Mark handlerBlock as an exception handler.
                var handlerBlock = handler.handlerPc.GetCanonicalInstance();
                handlerBlock.flags |= Label.Flag_Jump_Target;
                // Add handlerBlock as a successor of all the basic blocks in the exception handler range.
                var handlerRangeBlock = handler.startPc.GetCanonicalInstance();
                var handlerRangeEnd = handler.endPc.GetCanonicalInstance();
                while (handlerRangeBlock != handlerRangeEnd)
                {
                    handlerRangeBlock.outgoingEdges = new Edge(catchType, handlerBlock, handlerRangeBlock
                        .outgoingEdges);
                    handlerRangeBlock = handlerRangeBlock.nextBasicBlock;
                }

                handler = handler.nextHandler;
            }

            // Create and visit the first (implicit) frame.
            var firstFrame = firstBasicBlock.frame;
            firstFrame.SetInputFrameFromDescriptor(symbolTable, accessFlags, descriptor, maxLocals);
            firstFrame.Accept(this);
            // Fix point algorithm: add the first basic block to a list of blocks to process (i.e. blocks
            // whose stack map frame has changed) and, while there are blocks to process, remove one from
            // the list and update the stack map frames of its successor blocks in the control flow graph
            // (which might change them, in which case these blocks must be processed too, and are thus
            // added to the list of blocks to process). Also compute the maximum stack size of the method,
            // as a by-product.
            var listOfBlocksToProcess = firstBasicBlock;
            listOfBlocksToProcess.nextListElement = Label.Empty_List;
            var maxStackSize = 0;
            while (listOfBlocksToProcess != Label.Empty_List)
            {
                // Remove a basic block from the list of blocks to process.
                var basicBlock = listOfBlocksToProcess;
                listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
                basicBlock.nextListElement = null;
                // By definition, basicBlock is reachable.
                basicBlock.flags |= Label.Flag_Reachable;
                // Update the (absolute) maximum stack size.
                var maxBlockStackSize = basicBlock.frame.GetInputStackSize() + basicBlock.outputStackMax;
                if (maxBlockStackSize > maxStackSize) maxStackSize = maxBlockStackSize;
                // Update the successor blocks of basicBlock in the control flow graph.
                var outgoingEdge = basicBlock.outgoingEdges;
                while (outgoingEdge != null)
                {
                    var successorBlock = outgoingEdge.successor.GetCanonicalInstance();
                    var successorBlockChanged = basicBlock.frame.Merge(symbolTable, successorBlock.frame
                        , outgoingEdge.info);
                    if (successorBlockChanged && successorBlock.nextListElement == null)
                    {
                        // If successorBlock has changed it must be processed. Thus, if it is not already in the
                        // list of blocks to process, add it to this list.
                        successorBlock.nextListElement = listOfBlocksToProcess;
                        listOfBlocksToProcess = successorBlock;
                    }

                    outgoingEdge = outgoingEdge.nextEdge;
                }
            }

            // Loop over all the basic blocks and visit the stack map frames that must be stored in the
            // StackMapTable attribute. Also replace unreachable code with NOP* ATHROW, and remove it from
            // exception handler ranges.
            var basicBlock_1 = firstBasicBlock;
            while (basicBlock_1 != null)
            {
                if ((basicBlock_1.flags & (Label.Flag_Jump_Target | Label.Flag_Reachable)) == (Label
                                                                                                   .Flag_Jump_Target |
                                                                                               Label.Flag_Reachable))
                    basicBlock_1.frame.Accept(this);
                if ((basicBlock_1.flags & Label.Flag_Reachable) == 0)
                {
                    // Find the start and end bytecode offsets of this unreachable block.
                    var nextBasicBlock = basicBlock_1.nextBasicBlock;
                    var startOffset = basicBlock_1.bytecodeOffset;
                    var endOffset = (nextBasicBlock == null ? code.length : nextBasicBlock.bytecodeOffset
                                    ) - 1;
                    if (endOffset >= startOffset)
                    {
                        // Replace its instructions with NOP ... NOP ATHROW.
                        for (var i = startOffset; i < endOffset; ++i) code.data[i] = OpcodesConstants.Nop;
                        code.data[endOffset] = OpcodesConstants.Athrow;
                        // Emit a frame for this unreachable block, with no local and a Throwable on the stack
                        // (so that the ATHROW could consume this Throwable if it were reachable).
                        var frameIndex = VisitFrameStart(startOffset, 0, 1);
                        /* numLocal = */
                        /* numStack = */
                        currentFrame[frameIndex] = Frame.GetAbstractTypeFromInternalName(symbolTable,
                            "java/lang/Throwable"
                        );
                        VisitFrameEnd();
                        // Remove this unreachable basic block from the exception handler ranges.
                        firstHandler = Handler.RemoveRange(firstHandler, basicBlock_1, nextBasicBlock);
                        // The maximum stack size is now at least one, because of the Throwable declared above.
                        maxStackSize = Math.Max(maxStackSize, 1);
                    }
                }

                basicBlock_1 = basicBlock_1.nextBasicBlock;
            }

            maxStack = maxStackSize;
        }

        /// <summary>Computes the maximum stack size of the method.</summary>
        private void ComputeMaxStackAndLocal()
        {
            // Complete the control flow graph with exception handler blocks.
            var handler = firstHandler;
            while (handler != null)
            {
                var handlerBlock = handler.handlerPc;
                var handlerRangeBlock = handler.startPc;
                var handlerRangeEnd = handler.endPc;
                // Add handlerBlock as a successor of all the basic blocks in the exception handler range.
                while (handlerRangeBlock != handlerRangeEnd)
                {
                    if ((handlerRangeBlock.flags & Label.Flag_Subroutine_Caller) == 0)
                        handlerRangeBlock.outgoingEdges = new Edge(Edge.Exception, handlerBlock, handlerRangeBlock
                            .outgoingEdges);
                    else
                        // If handlerRangeBlock is a JSR block, add handlerBlock after the first two outgoing
                        // edges to preserve the hypothesis about JSR block successors order (see
                        // {@link #visitJumpInsn}).
                        handlerRangeBlock.outgoingEdges.nextEdge.nextEdge = new Edge(Edge.Exception, handlerBlock
                            , handlerRangeBlock.outgoingEdges.nextEdge.nextEdge);
                    handlerRangeBlock = handlerRangeBlock.nextBasicBlock;
                }

                handler = handler.nextHandler;
            }

            // Complete the control flow graph with the successor blocks of subroutines, if needed.
            if (hasSubroutines)
            {
                // First step: find the subroutines. This step determines, for each basic block, to which
                // subroutine(s) it belongs. Start with the main "subroutine":
                short numSubroutines = 1;
                firstBasicBlock.MarkSubroutine(numSubroutines);
                // Then, mark the subroutines called by the main subroutine, then the subroutines called by
                // those called by the main subroutine, etc.
                for (short currentSubroutine = 1; currentSubroutine <= numSubroutines; ++currentSubroutine)
                {
                    var basicBlock = firstBasicBlock;
                    while (basicBlock != null)
                    {
                        if ((basicBlock.flags & Label.Flag_Subroutine_Caller) != 0 && basicBlock.subroutineId
                            == currentSubroutine)
                        {
                            var jsrTarget = basicBlock.outgoingEdges.nextEdge.successor;
                            if (jsrTarget.subroutineId == 0)
                                // If this subroutine has not been marked yet, find its basic blocks.
                                jsrTarget.MarkSubroutine(++numSubroutines);
                        }

                        basicBlock = basicBlock.nextBasicBlock;
                    }
                }

                // Second step: find the successors in the control flow graph of each subroutine basic block
                // 'r' ending with a RET instruction. These successors are the virtual successors of the basic
                // blocks ending with JSR instructions (see {@link #visitJumpInsn)} that can reach 'r'.
                var basicBlock_1 = firstBasicBlock;
                while (basicBlock_1 != null)
                {
                    if ((basicBlock_1.flags & Label.Flag_Subroutine_Caller) != 0)
                    {
                        // By construction, jsr targets are stored in the second outgoing edge of basic blocks
                        // that ends with a jsr instruction (see {@link #FLAG_SUBROUTINE_CALLER}).
                        var subroutine = basicBlock_1.outgoingEdges.nextEdge.successor;
                        subroutine.AddSubroutineRetSuccessors(basicBlock_1);
                    }

                    basicBlock_1 = basicBlock_1.nextBasicBlock;
                }
            }

            // Data flow algorithm: put the first basic block in a list of blocks to process (i.e. blocks
            // whose input stack size has changed) and, while there are blocks to process, remove one
            // from the list, update the input stack size of its successor blocks in the control flow
            // graph, and add these blocks to the list of blocks to process (if not already done).
            var listOfBlocksToProcess = firstBasicBlock;
            listOfBlocksToProcess.nextListElement = Label.Empty_List;
            var maxStackSize = maxStack;
            while (listOfBlocksToProcess != Label.Empty_List)
            {
                // Remove a basic block from the list of blocks to process. Note that we don't reset
                // basicBlock.nextListElement to null on purpose, to make sure we don't reprocess already
                // processed basic blocks.
                var basicBlock = listOfBlocksToProcess;
                listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
                // Compute the (absolute) input stack size and maximum stack size of this block.
                int inputStackTop = basicBlock.inputStackSize;
                var maxBlockStackSize = inputStackTop + basicBlock.outputStackMax;
                // Update the absolute maximum stack size of the method.
                if (maxBlockStackSize > maxStackSize) maxStackSize = maxBlockStackSize;
                // Update the input stack size of the successor blocks of basicBlock in the control flow
                // graph, and add these blocks to the list of blocks to process, if not already done.
                var outgoingEdge = basicBlock.outgoingEdges;
                if ((basicBlock.flags & Label.Flag_Subroutine_Caller) != 0)
                    // Ignore the first outgoing edge of the basic blocks ending with a jsr: these are virtual
                    // edges which lead to the instruction just after the jsr, and do not correspond to a
                    // possible execution path (see {@link #visitJumpInsn} and
                    // {@link Label#FLAG_SUBROUTINE_CALLER}).
                    outgoingEdge = outgoingEdge.nextEdge;
                while (outgoingEdge != null)
                {
                    var successorBlock = outgoingEdge.successor;
                    if (successorBlock.nextListElement == null)
                    {
                        successorBlock.inputStackSize = (short) (outgoingEdge.info == Edge.Exception
                            ? 1
                            : inputStackTop + outgoingEdge.info);
                        successorBlock.nextListElement = listOfBlocksToProcess;
                        listOfBlocksToProcess = successorBlock;
                    }

                    outgoingEdge = outgoingEdge.nextEdge;
                }
            }

            maxStack = maxStackSize;
        }

        public override void VisitEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Utility methods: control flow analysis algorithm
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds a successor to
        ///     <see cref="currentBasicBlock" />
        ///     in the control flow graph.
        /// </summary>
        /// <param name="info">information about the control flow edge to be added.</param>
        /// <param name="successor">
        ///     the successor block to be added to the current basic block.
        /// </param>
        private void AddSuccessorToCurrentBasicBlock(int info, Label successor)
        {
            currentBasicBlock.outgoingEdges = new Edge(info, successor, currentBasicBlock.outgoingEdges
            );
        }

        /// <summary>Ends the current basic block.</summary>
        /// <remarks>
        ///     Ends the current basic block. This method must be used in the case where the current basic
        ///     block does not have any successor.
        ///     <p>
        ///         WARNING: this method must be called after the currently visited instruction has been put in
        ///         <see cref="code" />
        ///         (if frames are computed, this method inserts a new Label to start a new basic
        ///         block after the current instruction).
        /// </remarks>
        private void EndCurrentBasicBlockWithNoSuccessor()
        {
            if (compute == Compute_All_Frames)
            {
                var nextBasicBlock = new Label();
                nextBasicBlock.frame = new Frame(nextBasicBlock);
                nextBasicBlock.Resolve(code.data, code.length);
                lastBasicBlock.nextBasicBlock = nextBasicBlock;
                lastBasicBlock = nextBasicBlock;
                currentBasicBlock = null;
            }
            else if (compute == Compute_Max_Stack_And_Local)
            {
                currentBasicBlock.outputStackMax = (short) maxRelativeStackSize;
                currentBasicBlock = null;
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods: stack map frames
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Starts the visit of a new stack map frame, stored in
        ///     <see cref="currentFrame" />
        ///     .
        /// </summary>
        /// <param name="offset">
        ///     the bytecode offset of the instruction to which the frame corresponds.
        /// </param>
        /// <param name="numLocal">the number of local variables in the frame.</param>
        /// <param name="numStack">the number of stack elements in the frame.</param>
        /// <returns>the index of the next element to be written in this frame.</returns>
        internal int VisitFrameStart(int offset, int numLocal, int numStack)
        {
            var frameLength = 3 + numLocal + numStack;
            if (currentFrame == null || currentFrame.Length < frameLength) currentFrame = new int[frameLength];
            currentFrame[0] = offset;
            currentFrame[1] = numLocal;
            currentFrame[2] = numStack;
            return 3;
        }

        /// <summary>
        ///     Sets an abstract type in
        ///     <see cref="currentFrame" />
        ///     .
        /// </summary>
        /// <param name="frameIndex">
        ///     the index of the element to be set in
        ///     <see cref="currentFrame" />
        ///     .
        /// </param>
        /// <param name="abstractType">an abstract type.</param>
        internal void VisitAbstractType(int frameIndex, int abstractType)
        {
            currentFrame[frameIndex] = abstractType;
        }

        /// <summary>
        ///     Ends the visit of
        ///     <see cref="currentFrame" />
        ///     by writing it in the StackMapTable entries and by
        ///     updating the StackMapTable number_of_entries (except if the current frame is the first one,
        ///     which is implicit in StackMapTable). Then resets
        ///     <see cref="currentFrame" />
        ///     to
        ///     <literal>null</literal>
        ///     .
        /// </summary>
        internal void VisitFrameEnd()
        {
            if (previousFrame != null)
            {
                if (stackMapTableEntries == null) stackMapTableEntries = new ByteVector();
                PutFrame();
                ++stackMapTableNumberOfEntries;
            }

            previousFrame = currentFrame;
            currentFrame = null;
        }

        /// <summary>
        ///     Compresses and writes
        ///     <see cref="currentFrame" />
        ///     in a new StackMapTable entry.
        /// </summary>
        private void PutFrame()
        {
            var numLocal = currentFrame[1];
            var numStack = currentFrame[2];
            if (symbolTable.GetMajorVersion() < OpcodesConstants.V1_6)
            {
                // Generate a StackMap attribute entry, which are always uncompressed.
                stackMapTableEntries.PutShort(currentFrame[0]).PutShort(numLocal);
                PutAbstractTypes(3, 3 + numLocal);
                stackMapTableEntries.PutShort(numStack);
                PutAbstractTypes(3 + numLocal, 3 + numLocal + numStack);
                return;
            }

            var offsetDelta = stackMapTableNumberOfEntries == 0
                ? currentFrame[0]
                : currentFrame
                      [0] - previousFrame[0] - 1;
            var previousNumlocal = previousFrame[1];
            var numLocalDelta = numLocal - previousNumlocal;
            var type = Frame.Full_Frame;
            if (numStack == 0)
                switch (numLocalDelta)
                {
                    case -3:
                    case -2:
                    case -1:
                    {
                        type = Frame.Chop_Frame;
                        break;
                    }

                    case 0:
                    {
                        type = offsetDelta < 64 ? Frame.Same_Frame : Frame.Same_Frame_Extended;
                        break;
                    }

                    case 1:
                    case 2:
                    case 3:
                    {
                        type = Frame.Append_Frame;
                        break;
                    }
                }
            else if (numLocalDelta == 0 && numStack == 1)
                type = offsetDelta < 63
                    ? Frame.Same_Locals_1_Stack_Item_Frame
                    : Frame.Same_Locals_1_Stack_Item_Frame_Extended;

            if (type != Frame.Full_Frame)
            {
                // Verify if locals are the same as in the previous frame.
                var frameIndex = 3;
                for (var i = 0; i < previousNumlocal && i < numLocal; i++)
                {
                    if (currentFrame[frameIndex] != previousFrame[frameIndex])
                    {
                        type = Frame.Full_Frame;
                        break;
                    }

                    frameIndex++;
                }
            }

            switch (type)
            {
                case Frame.Same_Frame:
                {
                    stackMapTableEntries.PutByte(offsetDelta);
                    break;
                }

                case Frame.Same_Locals_1_Stack_Item_Frame:
                {
                    stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame + offsetDelta);
                    PutAbstractTypes(3 + numLocal, 4 + numLocal);
                    break;
                }

                case Frame.Same_Locals_1_Stack_Item_Frame_Extended:
                {
                    stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame_Extended).PutShort
                        (offsetDelta);
                    PutAbstractTypes(3 + numLocal, 4 + numLocal);
                    break;
                }

                case Frame.Same_Frame_Extended:
                {
                    stackMapTableEntries.PutByte(Frame.Same_Frame_Extended).PutShort(offsetDelta);
                    break;
                }

                case Frame.Chop_Frame:
                {
                    stackMapTableEntries.PutByte(Frame.Same_Frame_Extended + numLocalDelta).PutShort(
                        offsetDelta);
                    break;
                }

                case Frame.Append_Frame:
                {
                    stackMapTableEntries.PutByte(Frame.Same_Frame_Extended + numLocalDelta).PutShort(
                        offsetDelta);
                    PutAbstractTypes(3 + previousNumlocal, 3 + numLocal);
                    break;
                }

                case Frame.Full_Frame:
                default:
                {
                    stackMapTableEntries.PutByte(Frame.Full_Frame).PutShort(offsetDelta).PutShort(numLocal
                    );
                    PutAbstractTypes(3, 3 + numLocal);
                    stackMapTableEntries.PutShort(numStack);
                    PutAbstractTypes(3 + numLocal, 3 + numLocal + numStack);
                    break;
                }
            }
        }

        /// <summary>
        ///     Puts some abstract types of
        ///     <see cref="currentFrame" />
        ///     in
        ///     <see cref="stackMapTableEntries" />
        ///     , using the
        ///     JVMS verification_type_info format used in StackMapTable attributes.
        /// </summary>
        /// <param name="start">
        ///     index of the first type in
        ///     <see cref="currentFrame" />
        ///     to write.
        /// </param>
        /// <param name="end">
        ///     index of last type in
        ///     <see cref="currentFrame" />
        ///     to write (exclusive).
        /// </param>
        private void PutAbstractTypes(int start, int end)
        {
            for (var i = start; i < end; ++i) Frame.PutAbstractType(symbolTable, currentFrame[i], stackMapTableEntries);
        }

        /// <summary>
        ///     Puts the given public API frame element type in
        ///     <see cref="stackMapTableEntries" />
        ///     , using the JVMS
        ///     verification_type_info format used in StackMapTable attributes.
        /// </summary>
        /// <param name="type">
        ///     a frame element type described using the same format as in
        ///     <see cref="MethodVisitor.VisitFrame(int, int, object[], int, object[])" />
        ///     , i.e. either
        ///     <see cref="Opcodes.Top" />
        ///     ,
        ///     <see cref="Opcodes.Integer" />
        ///     ,
        ///     <see cref="Opcodes.Float" />
        ///     ,
        ///     <see cref="Opcodes.Long" />
        ///     ,
        ///     <see cref="Opcodes.Double" />
        ///     ,
        ///     <see cref="Opcodes.Null" />
        ///     , or
        ///     <see cref="Opcodes.Uninitialized_This" />
        ///     , or the internal name of a class, or a Label designating
        ///     a NEW instruction (for uninitialized types).
        /// </param>
        private void PutFrameType(object type)
        {
            if (type is int)
                stackMapTableEntries.PutByte((int) type);
            else if (type is string)
                stackMapTableEntries.PutByte(Frame.Item_Object).PutShort(symbolTable.AddConstantClass
                    ((string) type).index);
            else
                stackMapTableEntries.PutByte(Frame.Item_Uninitialized).PutShort(((Label) type).bytecodeOffset
                );
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns whether the attributes of this method can be copied from the attributes of the given
        ///     method (assuming there is no method visitor between the given ClassReader and this
        ///     MethodWriter).
        /// </summary>
        /// <remarks>
        ///     Returns whether the attributes of this method can be copied from the attributes of the given
        ///     method (assuming there is no method visitor between the given ClassReader and this
        ///     MethodWriter). This method should only be called just after this MethodWriter has been created,
        ///     and before any content is visited. It returns true if the attributes corresponding to the
        ///     constructor arguments (at most a Signature, an Exception, a Deprecated and a Synthetic
        ///     attribute) are the same as the corresponding attributes in the given method.
        /// </remarks>
        /// <param name="source">
        ///     the source ClassReader from which the attributes of this method might be copied.
        /// </param>
        /// <param name="hasSyntheticAttribute">
        ///     whether the method_info JVMS structure from which the attributes
        ///     of this method might be copied contains a Synthetic attribute.
        /// </param>
        /// <param name="hasDeprecatedAttribute">
        ///     whether the method_info JVMS structure from which the attributes
        ///     of this method might be copied contains a Deprecated attribute.
        /// </param>
        /// <param name="descriptorIndex">
        ///     the descriptor_index field of the method_info JVMS structure from which
        ///     the attributes of this method might be copied.
        /// </param>
        /// <param name="signatureIndex">
        ///     the constant pool index contained in the Signature attribute of the
        ///     method_info JVMS structure from which the attributes of this method might be copied, or 0.
        /// </param>
        /// <param name="exceptionsOffset">
        ///     the offset in 'source.b' of the Exceptions attribute of the method_info
        ///     JVMS structure from which the attributes of this method might be copied, or 0.
        /// </param>
        /// <returns>
        ///     whether the attributes of this method can be copied from the attributes of the
        ///     method_info JVMS structure in 'source.b', between 'methodInfoOffset' and 'methodInfoOffset'
        ///     + 'methodInfoLength'.
        /// </returns>
        internal bool CanCopyMethodAttributes(ClassReader source, bool hasSyntheticAttribute
            , bool hasDeprecatedAttribute, int descriptorIndex, int signatureIndex, int exceptionsOffset
        )
        {
            // If the method descriptor has changed, with more locals than the max_locals field of the
            // original Code attribute, if any, then the original method attributes can't be copied. A
            // conservative check on the descriptor changes alone ensures this (being more precise is not
            // worth the additional complexity, because these cases should be rare -- if a transform changes
            // a method descriptor, most of the time it needs to change the method's code too).
            if (source != symbolTable.GetSource() || descriptorIndex != this.descriptorIndex
                                                  || signatureIndex != this.signatureIndex || hasDeprecatedAttribute !=
                                                  ((accessFlags
                                                    & OpcodesConstants.Acc_Deprecated) != 0))
                return false;
            var needSyntheticAttribute = symbolTable.GetMajorVersion() < OpcodesConstants.V1_5
                                         && (accessFlags & OpcodesConstants.Acc_Synthetic) != 0;
            if (hasSyntheticAttribute != needSyntheticAttribute) return false;
            if (exceptionsOffset == 0)
            {
                if (numberOfExceptions != 0) return false;
            }
            else if (source.ReadUnsignedShort(exceptionsOffset) == numberOfExceptions)
            {
                var currentExceptionOffset = exceptionsOffset + 2;
                for (var i = 0; i < numberOfExceptions; ++i)
                {
                    if (source.ReadUnsignedShort(currentExceptionOffset) != exceptionIndexTable[i]) return false;
                    currentExceptionOffset += 2;
                }
            }

            return true;
        }

        /// <summary>
        ///     Sets the source from which the attributes of this method will be copied.
        /// </summary>
        /// <param name="methodInfoOffset">
        ///     the offset in 'symbolTable.getSource()' of the method_info JVMS
        ///     structure from which the attributes of this method will be copied.
        /// </param>
        /// <param name="methodInfoLength">
        ///     the length in 'symbolTable.getSource()' of the method_info JVMS
        ///     structure from which the attributes of this method will be copied.
        /// </param>
        internal void SetMethodAttributesSource(int methodInfoOffset, int methodInfoLength
        )
        {
            // Don't copy the attributes yet, instead store their location in the source class reader so
            // they can be copied later, in {@link #putMethodInfo}. Note that we skip the 6 header bytes
            // of the method_info JVMS structure.
            sourceOffset = methodInfoOffset + 6;
            sourceLength = methodInfoLength - 6;
        }

        /// <summary>
        ///     Returns the size of the method_info JVMS structure generated by this MethodWriter.
        /// </summary>
        /// <remarks>
        ///     Returns the size of the method_info JVMS structure generated by this MethodWriter. Also add the
        ///     names of the attributes of this method in the constant pool.
        /// </remarks>
        /// <returns>the size in bytes of the method_info JVMS structure.</returns>
        internal int ComputeMethodInfoSize()
        {
            // If this method_info must be copied from an existing one, the size computation is trivial.
            if (sourceOffset != 0)
                // sourceLength excludes the first 6 bytes for access_flags, name_index and descriptor_index.
                return 6 + sourceLength;
            // 2 bytes each for access_flags, name_index, descriptor_index and attributes_count.
            var size = 8;
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            if (code.length > 0)
            {
                if (code.length > 65535)
                    throw new MethodTooLargeException(symbolTable.GetClassName(), name, descriptor, code
                        .length);
                symbolTable.AddConstantUtf8(Constants.Code);
                // The Code attribute has 6 header bytes, plus 2, 2, 4 and 2 bytes respectively for max_stack,
                // max_locals, code_length and attributes_count, plus the bytecode and the exception table.
                size += 16 + code.length + Handler.GetExceptionTableSize(firstHandler);
                if (stackMapTableEntries != null)
                {
                    var useStackMapTable = symbolTable.GetMajorVersion() >= OpcodesConstants.V1_6;
                    symbolTable.AddConstantUtf8(useStackMapTable ? Constants.Stack_Map_Table : "StackMap"
                    );
                    // 6 header bytes and 2 bytes for number_of_entries.
                    size += 8 + stackMapTableEntries.length;
                }

                if (lineNumberTable != null)
                {
                    symbolTable.AddConstantUtf8(Constants.Line_Number_Table);
                    // 6 header bytes and 2 bytes for line_number_table_length.
                    size += 8 + lineNumberTable.length;
                }

                if (localVariableTable != null)
                {
                    symbolTable.AddConstantUtf8(Constants.Local_Variable_Table);
                    // 6 header bytes and 2 bytes for local_variable_table_length.
                    size += 8 + localVariableTable.length;
                }

                if (localVariableTypeTable != null)
                {
                    symbolTable.AddConstantUtf8(Constants.Local_Variable_Type_Table);
                    // 6 header bytes and 2 bytes for local_variable_type_table_length.
                    size += 8 + localVariableTypeTable.length;
                }

                if (lastCodeRuntimeVisibleTypeAnnotation != null)
                    size += lastCodeRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(
                        Constants.Runtime_Visible_Type_Annotations
                    );
                if (lastCodeRuntimeInvisibleTypeAnnotation != null)
                    size += lastCodeRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(
                        Constants.Runtime_Invisible_Type_Annotations
                    );
                if (firstCodeAttribute != null)
                    size += firstCodeAttribute.ComputeAttributesSize(symbolTable, code.data, code.length
                        , maxStack, maxLocals);
            }

            if (numberOfExceptions > 0)
            {
                symbolTable.AddConstantUtf8(Constants.Exceptions);
                size += 8 + 2 * numberOfExceptions;
            }

            size += Attribute.ComputeAttributesSize(symbolTable, accessFlags, signatureIndex);
            size += AnnotationWriter.ComputeAnnotationsSize(lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation);
            if (lastRuntimeVisibleParameterAnnotations != null)
                size += AnnotationWriter.ComputeParameterAnnotationsSize(Constants.Runtime_Visible_Parameter_Annotations
                    , lastRuntimeVisibleParameterAnnotations,
                    visibleAnnotableParameterCount == 0
                        ? lastRuntimeVisibleParameterAnnotations.Length
                        : visibleAnnotableParameterCount);
            if (lastRuntimeInvisibleParameterAnnotations != null)
                size += AnnotationWriter.ComputeParameterAnnotationsSize(
                    Constants.Runtime_Invisible_Parameter_Annotations
                    , lastRuntimeInvisibleParameterAnnotations, invisibleAnnotableParameterCount ==
                                                                0
                        ? lastRuntimeInvisibleParameterAnnotations.Length
                        : invisibleAnnotableParameterCount
                );
            if (defaultValue != null)
            {
                symbolTable.AddConstantUtf8(Constants.Annotation_Default);
                size += 6 + defaultValue.length;
            }

            if (parameters != null)
            {
                symbolTable.AddConstantUtf8(Constants.Method_Parameters);
                // 6 header bytes and 1 byte for parameters_count.
                size += 7 + parameters.length;
            }

            if (firstAttribute != null) size += firstAttribute.ComputeAttributesSize(symbolTable);
            return size;
        }

        /// <summary>
        ///     Puts the content of the method_info JVMS structure generated by this MethodWriter into the
        ///     given ByteVector.
        /// </summary>
        /// <param name="output">where the method_info structure must be put.</param>
        internal void PutMethodInfo(ByteVector output)
        {
            var useSyntheticAttribute = symbolTable.GetMajorVersion() < OpcodesConstants.V1_5;
            var mask = useSyntheticAttribute ? OpcodesConstants.Acc_Synthetic : 0;
            output.PutShort(accessFlags & ~mask).PutShort(nameIndex).PutShort(descriptorIndex
            );
            // If this method_info must be copied from an existing one, copy it now and return early.
            if (sourceOffset != 0)
            {
                output.PutByteArray(symbolTable.GetSource().classFileBuffer, sourceOffset, sourceLength
                );
                return;
            }

            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributeCount = 0;
            if (code.length > 0) ++attributeCount;
            if (numberOfExceptions > 0) ++attributeCount;
            if ((accessFlags & OpcodesConstants.Acc_Synthetic) != 0 && useSyntheticAttribute) ++attributeCount;
            if (signatureIndex != 0) ++attributeCount;
            if ((accessFlags & OpcodesConstants.Acc_Deprecated) != 0) ++attributeCount;
            if (lastRuntimeVisibleAnnotation != null) ++attributeCount;
            if (lastRuntimeInvisibleAnnotation != null) ++attributeCount;
            if (lastRuntimeVisibleParameterAnnotations != null) ++attributeCount;
            if (lastRuntimeInvisibleParameterAnnotations != null) ++attributeCount;
            if (lastRuntimeVisibleTypeAnnotation != null) ++attributeCount;
            if (lastRuntimeInvisibleTypeAnnotation != null) ++attributeCount;
            if (defaultValue != null) ++attributeCount;
            if (parameters != null) ++attributeCount;
            if (firstAttribute != null) attributeCount += firstAttribute.GetAttributeCount();
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            output.PutShort(attributeCount);
            if (code.length > 0)
            {
                // 2, 2, 4 and 2 bytes respectively for max_stack, max_locals, code_length and
                // attributes_count, plus the bytecode and the exception table.
                var size = 10 + code.length + Handler.GetExceptionTableSize(firstHandler);
                var codeAttributeCount = 0;
                if (stackMapTableEntries != null)
                {
                    // 6 header bytes and 2 bytes for number_of_entries.
                    size += 8 + stackMapTableEntries.length;
                    ++codeAttributeCount;
                }

                if (lineNumberTable != null)
                {
                    // 6 header bytes and 2 bytes for line_number_table_length.
                    size += 8 + lineNumberTable.length;
                    ++codeAttributeCount;
                }

                if (localVariableTable != null)
                {
                    // 6 header bytes and 2 bytes for local_variable_table_length.
                    size += 8 + localVariableTable.length;
                    ++codeAttributeCount;
                }

                if (localVariableTypeTable != null)
                {
                    // 6 header bytes and 2 bytes for local_variable_type_table_length.
                    size += 8 + localVariableTypeTable.length;
                    ++codeAttributeCount;
                }

                if (lastCodeRuntimeVisibleTypeAnnotation != null)
                {
                    size += lastCodeRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(
                        Constants.Runtime_Visible_Type_Annotations
                    );
                    ++codeAttributeCount;
                }

                if (lastCodeRuntimeInvisibleTypeAnnotation != null)
                {
                    size += lastCodeRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(
                        Constants.Runtime_Invisible_Type_Annotations
                    );
                    ++codeAttributeCount;
                }

                if (firstCodeAttribute != null)
                {
                    size += firstCodeAttribute.ComputeAttributesSize(symbolTable, code.data, code.length
                        , maxStack, maxLocals);
                    codeAttributeCount += firstCodeAttribute.GetAttributeCount();
                }

                output.PutShort(symbolTable.AddConstantUtf8(Constants.Code)).PutInt(size).PutShort
                    (maxStack).PutShort(maxLocals).PutInt(code.length).PutByteArray(code.data, 0, code
                    .length);
                Handler.PutExceptionTable(firstHandler, output);
                output.PutShort(codeAttributeCount);
                if (stackMapTableEntries != null)
                {
                    var useStackMapTable = symbolTable.GetMajorVersion() >= OpcodesConstants.V1_6;
                    output.PutShort(symbolTable.AddConstantUtf8(useStackMapTable
                        ? Constants.Stack_Map_Table
                        : "StackMap")).PutInt(2 + stackMapTableEntries.length).PutShort(stackMapTableNumberOfEntries
                    ).PutByteArray(stackMapTableEntries.data, 0, stackMapTableEntries.length);
                }

                if (lineNumberTable != null)
                    output.PutShort(symbolTable.AddConstantUtf8(Constants.Line_Number_Table)).PutInt(
                        2 + lineNumberTable.length).PutShort(lineNumberTableLength).PutByteArray(lineNumberTable
                        .data, 0, lineNumberTable.length);
                if (localVariableTable != null)
                    output.PutShort(symbolTable.AddConstantUtf8(Constants.Local_Variable_Table)).PutInt
                        (2 + localVariableTable.length).PutShort(localVariableTableLength).PutByteArray(
                        localVariableTable.data, 0, localVariableTable.length);
                if (localVariableTypeTable != null)
                    output.PutShort(symbolTable.AddConstantUtf8(Constants.Local_Variable_Type_Table))
                        .PutInt(2 + localVariableTypeTable.length).PutShort(localVariableTypeTableLength
                        ).PutByteArray(localVariableTypeTable.data, 0, localVariableTypeTable.length);
                if (lastCodeRuntimeVisibleTypeAnnotation != null)
                    lastCodeRuntimeVisibleTypeAnnotation.PutAnnotations(symbolTable.AddConstantUtf8(Constants
                        .Runtime_Visible_Type_Annotations), output);
                if (lastCodeRuntimeInvisibleTypeAnnotation != null)
                    lastCodeRuntimeInvisibleTypeAnnotation.PutAnnotations(symbolTable.AddConstantUtf8
                        (Constants.Runtime_Invisible_Type_Annotations), output);
                if (firstCodeAttribute != null)
                    firstCodeAttribute.PutAttributes(symbolTable, code.data, code.length, maxStack, maxLocals
                        , output);
            }

            if (numberOfExceptions > 0)
            {
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Exceptions)).PutInt(2 + 2 *
                                                                                          numberOfExceptions)
                    .PutShort(numberOfExceptions);
                foreach (var exceptionIndex in exceptionIndexTable) output.PutShort(exceptionIndex);
            }

            Attribute.PutAttributes(symbolTable, accessFlags, signatureIndex, output);
            AnnotationWriter.PutAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation
                , lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, output);
            if (lastRuntimeVisibleParameterAnnotations != null)
                AnnotationWriter.PutParameterAnnotations(symbolTable.AddConstantUtf8(
                        Constants.Runtime_Visible_Parameter_Annotations
                    ), lastRuntimeVisibleParameterAnnotations,
                    visibleAnnotableParameterCount == 0
                        ? lastRuntimeVisibleParameterAnnotations.Length
                        : visibleAnnotableParameterCount,
                    output);
            if (lastRuntimeInvisibleParameterAnnotations != null)
                AnnotationWriter.PutParameterAnnotations(symbolTable.AddConstantUtf8(
                        Constants.Runtime_Invisible_Parameter_Annotations
                    ), lastRuntimeInvisibleParameterAnnotations, invisibleAnnotableParameterCount ==
                                                                 0
                        ? lastRuntimeInvisibleParameterAnnotations.Length
                        : invisibleAnnotableParameterCount
                    , output);
            if (defaultValue != null)
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Annotation_Default)).PutInt
                    (defaultValue.length).PutByteArray(defaultValue.data, 0, defaultValue.length);
            if (parameters != null)
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Method_Parameters)).PutInt(
                    1 + parameters.length).PutByte(parametersCount).PutByteArray(parameters.data, 0,
                    parameters.length);
            if (firstAttribute != null) firstAttribute.PutAttributes(symbolTable, output);
        }

        /// <summary>
        ///     Collects the attributes of this method into the given set of attribute prototypes.
        /// </summary>
        /// <param name="attributePrototypes">a set of attribute prototypes.</param>
        internal void CollectAttributePrototypes(Attribute.Set attributePrototypes)
        {
            attributePrototypes.AddAttributes(firstAttribute);
            attributePrototypes.AddAttributes(firstCodeAttribute);
        }
    }
}