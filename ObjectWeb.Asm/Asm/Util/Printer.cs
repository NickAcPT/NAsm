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
using System.IO;
using System.Text;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Misc.Java.Nio;

namespace ObjectWeb.Asm.Util
{
    /// <summary>An abstract converter from visit events to text.</summary>
    /// <author>Eric Bruneton</author>
    public abstract class Printer
    {
        /// <summary>
        ///     Message of the UnsupportedOperationException thrown by methods which must be overridden.
        /// </summary>
        private const string Unsupported_Operation = "Must be overridden";

        /// <summary>The names of the Java Virtual Machine opcodes.</summary>
        public static readonly string[] Opcodes =
        {
            "NOP", "ACONST_NULL", "ICONST_M1", "ICONST_0", "ICONST_1", "ICONST_2", "ICONST_3", "ICONST_4", "ICONST_5",
            "LCONST_0", "LCONST_1", "FCONST_0", "FCONST_1", "FCONST_2", "DCONST_0", "DCONST_1", "BIPUSH", "SIPUSH",
            "LDC", "LDC_W", "LDC2_W", "ILOAD", "LLOAD", "FLOAD", "DLOAD", "ALOAD", "ILOAD_0", "ILOAD_1", "ILOAD_2",
            "ILOAD_3", "LLOAD_0", "LLOAD_1", "LLOAD_2", "LLOAD_3", "FLOAD_0", "FLOAD_1", "FLOAD_2", "FLOAD_3",
            "DLOAD_0", "DLOAD_1", "DLOAD_2", "DLOAD_3", "ALOAD_0", "ALOAD_1", "ALOAD_2", "ALOAD_3", "IALOAD", "LALOAD",
            "FALOAD", "DALOAD", "AALOAD", "BALOAD", "CALOAD", "SALOAD", "ISTORE", "LSTORE", "FSTORE", "DSTORE",
            "ASTORE", "ISTORE_0", "ISTORE_1", "ISTORE_2", "ISTORE_3", "LSTORE_0", "LSTORE_1", "LSTORE_2", "LSTORE_3",
            "FSTORE_0", "FSTORE_1", "FSTORE_2", "FSTORE_3", "DSTORE_0", "DSTORE_1", "DSTORE_2", "DSTORE_3", "ASTORE_0",
            "ASTORE_1", "ASTORE_2", "ASTORE_3", "IASTORE", "LASTORE", "FASTORE", "DASTORE", "AASTORE", "BASTORE",
            "CASTORE", "SASTORE", "POP", "POP2", "DUP", "DUP_X1", "DUP_X2", "DUP2", "DUP2_X1", "DUP2_X2", "SWAP",
            "IADD", "LADD", "FADD", "DADD", "ISUB", "LSUB", "FSUB", "DSUB", "IMUL", "LMUL", "FMUL", "DMUL", "IDIV",
            "LDIV", "FDIV", "DDIV", "IREM", "LREM", "FREM", "DREM", "INEG", "LNEG", "FNEG", "DNEG", "ISHL", "LSHL",
            "ISHR", "LSHR", "IUSHR", "LUSHR", "IAND", "LAND", "IOR", "LOR", "IXOR", "LXOR", "IINC", "I2L", "I2F", "I2D",
            "L2I", "L2F", "L2D", "F2I", "F2L", "F2D", "D2I", "D2L", "D2F", "I2B", "I2C", "I2S", "LCMP", "FCMPL",
            "FCMPG", "DCMPL", "DCMPG", "IFEQ", "IFNE", "IFLT", "IFGE", "IFGT", "IFLE", "IF_ICMPEQ", "IF_ICMPNE",
            "IF_ICMPLT", "IF_ICMPGE", "IF_ICMPGT", "IF_ICMPLE",
            "IF_ACMPEQ", "IF_ACMPNE", "GOTO", "JSR", "RET", "TABLESWITCH", "LOOKUPSWITCH", "IRETURN", "LRETURN",
            "FRETURN", "DRETURN", "ARETURN", "RETURN", "GETSTATIC", "PUTSTATIC", "GETFIELD", "PUTFIELD",
            "INVOKEVIRTUAL", "INVOKESPECIAL", "INVOKESTATIC", "INVOKEINTERFACE", "INVOKEDYNAMIC", "NEW", "NEWARRAY",
            "ANEWARRAY", "ARRAYLENGTH", "ATHROW", "CHECKCAST", "INSTANCEOF", "MONITORENTER", "MONITOREXIT", "WIDE",
            "MULTIANEWARRAY", "IFNULL", "IFNONNULL"
        };

        /// <summary>
        ///     The names of the
        ///     <c>operand</c>
        ///     values of the
        ///     <see cref="MethodVisitor.VisitIntInsn" />
        ///     method when
        ///     <c>opcode</c>
        ///     is
        ///     <c>NEWARRAY</c>
        ///     .
        /// </summary>
        public static readonly string[] Types =
        {
            string.Empty, string.Empty, string.Empty, string.Empty, "T_BOOLEAN", "T_CHAR", "T_FLOAT", "T_DOUBLE",
            "T_BYTE", "T_SHORT", "T_INT", "T_LONG"
        };

        /// <summary>
        ///     The names of the
        ///     <c>tag</c>
        ///     field values for
        ///     <see cref="Handle" />
        ///     .
        /// </summary>
        public static readonly string[] Handle_Tag =
        {
            string.Empty, "H_GETFIELD", "H_GETSTATIC", "H_PUTFIELD", "H_PUTSTATIC", "H_INVOKEVIRTUAL", "H_INVOKESTATIC",
            "H_INVOKESPECIAL", "H_NEWINVOKESPECIAL", "H_INVOKEINTERFACE"
        };

        /// <summary>The ASM API version implemented by this class.</summary>
        /// <remarks>
        ///     The ASM API version implemented by this class. The value of this field must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </remarks>
        protected internal readonly VisitorAsmApiVersion api;

        /// <summary>The builder used to build strings in the various visit methods.</summary>
        protected internal readonly StringBuilder stringBuilder;

        /// <summary>The text to be printed.</summary>
        /// <remarks>
        ///     The text to be printed. Since the code of methods is not necessarily visited in sequential
        ///     order, one method after the other, but can be interlaced (some instructions from method one,
        ///     then some instructions from method two, then some instructions from method one again...), it is
        ///     not possible to print the visited instructions directly to a sequential stream. A class is
        ///     therefore printed in a two steps process: a string tree is constructed during the visit, and
        ///     printed to a sequential stream at the end of the visit. This string tree is stored in this
        ///     field, as a string list that can contain other string lists, which can themselves contain other
        ///     string lists, and so on.
        /// </remarks>
        public readonly IList<object> text;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Printer" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this printer. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        protected internal Printer(VisitorAsmApiVersion api)
        {
            // 0 (0x0)
            // 1 (0x1)
            // 2 (0x2)
            // 3 (0x3)
            // 4 (0x4)
            // 5 (0x5)
            // 6 (0x6)
            // 7 (0x7)
            // 8 (0x8)
            // 9 (0x9)
            // 10 (0xa)
            // 11 (0xb)
            // 12 (0xc)
            // 13 (0xd)
            // 14 (0xe)
            // 15 (0xf)
            // 16 (0x10)
            // 17 (0x11)
            // 18 (0x12)
            // 19 (0x13)
            // 20 (0x14)
            // 21 (0x15)
            // 22 (0x16)
            // 23 (0x17)
            // 24 (0x18)
            // 25 (0x19)
            // 26 (0x1a)
            // 27 (0x1b)
            // 28 (0x1c)
            // 29 (0x1d)
            // 30 (0x1e)
            // 31 (0x1f)
            // 32 (0x20)
            // 33 (0x21)
            // 34 (0x22)
            // 35 (0x23)
            // 36 (0x24)
            // 37 (0x25)
            // 38 (0x26)
            // 39 (0x27)
            // 40 (0x28)
            // 41 (0x29)
            // 42 (0x2a)
            // 43 (0x2b)
            // 44 (0x2c)
            // 45 (0x2d)
            // 46 (0x2e)
            // 47 (0x2f)
            // 48 (0x30)
            // 49 (0x31)
            // 50 (0x32)
            // 51 (0x33)
            // 52 (0x34)
            // 53 (0x35)
            // 54 (0x36)
            // 55 (0x37)
            // 56 (0x38)
            // 57 (0x39)
            // 58 (0x3a)
            // 59 (0x3b)
            // 60 (0x3c)
            // 61 (0x3d)
            // 62 (0x3e)
            // 63 (0x3f)
            // 64 (0x40)
            // 65 (0x41)
            // 66 (0x42)
            // 67 (0x43)
            // 68 (0x44)
            // 69 (0x45)
            // 70 (0x46)
            // 71 (0x47)
            // 72 (0x48)
            // 73 (0x49)
            // 74 (0x4a)
            // 75 (0x4b)
            // 76 (0x4c)
            // 77 (0x4d)
            // 78 (0x4e)
            // 79 (0x4f)
            // 80 (0x50)
            // 81 (0x51)
            // 82 (0x52)
            // 83 (0x53)
            // 84 (0x54)
            // 85 (0x55)
            // 86 (0x56)
            // 87 (0x57)
            // 88 (0x58)
            // 89 (0x59)
            // 90 (0x5a)
            // 91 (0x5b)
            // 92 (0x5c)
            // 93 (0x5d)
            // 94 (0x5e)
            // 95 (0x5f)
            // 96 (0x60)
            // 97 (0x61)
            // 98 (0x62)
            // 99 (0x63)
            // 100 (0x64)
            // 101 (0x65)
            // 102 (0x66)
            // 103 (0x67)
            // 104 (0x68)
            // 105 (0x69)
            // 106 (0x6a)
            // 107 (0x6b)
            // 108 (0x6c)
            // 109 (0x6d)
            // 110 (0x6e)
            // 111 (0x6f)
            // 112 (0x70)
            // 113 (0x71)
            // 114 (0x72)
            // 115 (0x73)
            // 116 (0x74)
            // 117 (0x75)
            // 118 (0x76)
            // 119 (0x77)
            // 120 (0x78)
            // 121 (0x79)
            // 122 (0x7a)
            // 123 (0x7b)
            // 124 (0x7c)
            // 125 (0x7d)
            // 126 (0x7e)
            // 127 (0x7f)
            // 128 (0x80)
            // 129 (0x81)
            // 130 (0x82)
            // 131 (0x83)
            // 132 (0x84)
            // 133 (0x85)
            // 134 (0x86)
            // 135 (0x87)
            // 136 (0x88)
            // 137 (0x89)
            // 138 (0x8a)
            // 139 (0x8b)
            // 140 (0x8c)
            // 141 (0x8d)
            // 142 (0x8e)
            // 143 (0x8f)
            // 144 (0x90)
            // 145 (0x91)
            // 146 (0x92)
            // 147 (0x93)
            // 148 (0x94)
            // 149 (0x95)
            // 150 (0x96)
            // 151 (0x97)
            // 152 (0x98)
            // 153 (0x99)
            // 154 (0x9a)
            // 155 (0x9b)
            // 156 (0x9c)
            // 157 (0x9d)
            // 158 (0x9e)
            // 159 (0x9f)
            // 160 (0xa0)
            // 161 (0xa1)
            // 162 (0xa2)
            // 163 (0xa3)
            // 164 (0xa4)
            // 165 (0xa5)
            // 166 (0xa6)
            // 167 (0xa7)
            // 168 (0xa8)
            // 169 (0xa9)
            // 170 (0xaa)
            // 171 (0xab)
            // 172 (0xac)
            // 173 (0xad)
            // 174 (0xae)
            // 175 (0xaf)
            // 176 (0xb0)
            // 177 (0xb1)
            // 178 (0xb2)
            // 179 (0xb3)
            // 180 (0xb4)
            // 181 (0xb5)
            // 182 (0xb6)
            // 183 (0xb7)
            // 184 (0xb8)
            // 185 (0xb9)
            // 186 (0xba)
            // 187 (0xbb)
            // 188 (0xbc)
            // 189 (0xbd)
            // 190 (0xbe)
            // 191 (0xbf)
            // 192 (0xc0)
            // 193 (0xc1)
            // 194 (0xc2)
            // 195 (0xc3)
            // 196 (0xc4)
            // 197 (0xc5)
            // 198 (0xc6)
            // 199 (0xc7)
            // -----------------------------------------------------------------------------------------------
            // Constructor
            // -----------------------------------------------------------------------------------------------
            this.api = api;
            stringBuilder = new StringBuilder();
            text = new List<object>();
        }

        // -----------------------------------------------------------------------------------------------
        // Classes
        // -----------------------------------------------------------------------------------------------
        /// <summary>Class header.</summary>
        /// <remarks>
        ///     Class header. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.Visit(int, int, string, string, string, string[])
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="version">
        ///     the class version. The minor version is stored in the 16 most significant bits,
        ///     and the major version in the 16 least significant bits.
        /// </param>
        /// <param name="access">
        ///     the class's access flags (see
        ///     <see cref="Org.Objectweb.Asm.Opcodes" />
        ///     ). This parameter also indicates if
        ///     the class is deprecated.
        /// </param>
        /// <param name="name">
        ///     the internal name of the class (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the signature of this class. May be
        ///     <literal>null</literal>
        ///     if the class is not a
        ///     generic one, and does not extend or implement generic classes or interfaces.
        /// </param>
        /// <param name="superName">
        ///     the internal of name of the super class (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ). For interfaces, the super class is
        ///     <see cref="object" />
        ///     . May be
        ///     <literal>null</literal>
        ///     , but only for the
        ///     <see cref="object" />
        ///     class.
        /// </param>
        /// <param name="interfaces">
        ///     the internal names of the class's interfaces (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ). May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public abstract void Visit(int version, AccessFlags access, string name, string signature
            , string superName, string[] interfaces);

        /// <summary>Class source.</summary>
        /// <remarks>
        ///     Class source. See
        ///     <see cref="ClassVisitor.VisitSource" />
        ///     .
        /// </remarks>
        /// <param name="source">
        ///     the name of the source file from which the class was compiled. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="debug">
        ///     additional debug information to compute the correspondence between source and
        ///     compiled elements of the class. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public abstract void VisitSource(string source, string debug);

        /// <summary>Module.</summary>
        /// <remarks>
        ///     Module. See
        ///     <see cref="ClassVisitor.VisitModule" />
        ///     .
        /// </remarks>
        /// <param name="name">the fully qualified name (using dots) of the module.</param>
        /// <param name="access">
        ///     the module access flags, among
        ///     <c>ACC_OPEN</c>
        ///     ,
        ///     <c>ACC_SYNTHETIC</c>
        ///     and
        ///     <c>ACC_MANDATED</c>
        ///     .
        /// </param>
        /// <param name="version">
        ///     the module version, or
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitModule(string name, AccessFlags access, string version)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Visits the nest host class of the class.</summary>
        /// <remarks>
        ///     Visits the nest host class of the class. A nest is a set of classes of the same package that
        ///     share access to their private members. One of these classes, called the host, lists the other
        ///     members of the nest, which in turn should link to the host of their nest. This method must be
        ///     called only once and only if the visited class is a non-host member of a nest. A class is
        ///     implicitly its own nest, so it's invalid to call this method with the visited class name as
        ///     argument.
        /// </remarks>
        /// <param name="nestHost">the internal name of the host class of the nest.</param>
        public virtual void VisitNestHost(string nestHost)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Class outer class.</summary>
        /// <remarks>
        ///     Class outer class. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitOuterClass(string, string, string)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="owner">internal name of the enclosing class of the class.</param>
        /// <param name="name">
        ///     the name of the method that contains the class, or
        ///     <literal>null</literal>
        ///     if the class is
        ///     not enclosed in a method of its enclosing class.
        /// </param>
        /// <param name="descriptor">
        ///     the descriptor of the method that contains the class, or
        ///     <literal>null</literal>
        ///     if
        ///     the class is not enclosed in a method of its enclosing class.
        /// </param>
        public abstract void VisitOuterClass(string owner, string name, string descriptor
        );

        /// <summary>Class annotation.</summary>
        /// <remarks>
        ///     Class annotation. See
        ///     <see cref="ClassVisitor.VisitAnnotation" />
        ///     .
        /// </remarks>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitClassAnnotation(string descriptor, bool visible);

        /// <summary>Class type annotation.</summary>
        /// <remarks>
        ///     Class type annotation. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitTypeAnnotation(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Class_Type_Parameter" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Class_Type_Parameter_Bound" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Class_Extends" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitClassTypeAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Class attribute.</summary>
        /// <remarks>
        ///     Class attribute. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitAttribute(Org.Objectweb.Asm.Attribute)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="attribute">an attribute.</param>
        public abstract void VisitClassAttribute(Attribute attribute);

        /// <summary>Visits a member of the nest.</summary>
        /// <remarks>
        ///     Visits a member of the nest. A nest is a set of classes of the same package that share access
        ///     to their private members. One of these classes, called the host, lists the other members of the
        ///     nest, which in turn should link to the host of their nest. This method must be called only if
        ///     the visited class is the host of a nest. A nest host is implicitly a member of its own nest, so
        ///     it's invalid to call this method with the visited class name as argument.
        /// </remarks>
        /// <param name="nestMember">the internal name of a nest member.</param>
        public virtual void VisitNestMember(string nestMember)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>
        ///     <b>Experimental, use at your own risk.
        /// </summary>
        /// <remarks>
        ///     <b>
        ///         Experimental, use at your own risk. This method will be renamed when it becomes stable, this
        ///         will break existing code using it
        ///     </b>
        ///     .
        ///     <p>
        ///         Visits a permitted subtypes. A permitted subtypes is one of the allowed subtypes of the
        ///         current class. See
        ///         <see cref="Org.Objectweb.Asm.ClassVisitor.VisitPermittedSubtypeExperimental(string)
        /// 	" />
        ///         .
        /// </remarks>
        /// <param name="permittedSubtype">the internal name of a permitted subtype.</param>
        //  [Obsolete(@"this API is experimental.")]
        public virtual void VisitPermittedSubtypeExperimental(string permittedSubtype)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Class inner name.</summary>
        /// <remarks>
        ///     Class inner name. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitInnerClass(string, string, string, int)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="name">
        ///     the internal name of an inner class (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="outerName">
        ///     the internal name of the class to which the inner class belongs (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ). May be
        ///     <literal>null</literal>
        ///     for not member classes.
        /// </param>
        /// <param name="innerName">
        ///     the (simple) name of the inner class inside its enclosing class. May be
        ///     <literal>null</literal>
        ///     for anonymous inner classes.
        /// </param>
        /// <param name="access">
        ///     the access flags of the inner class as originally declared in the enclosing
        ///     class.
        /// </param>
        public abstract void VisitInnerClass(string name, string outerName, string innerName
            , AccessFlags access);

        /// <summary>Visits a record component of the class.</summary>
        /// <remarks>
        ///     Visits a record component of the class. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitRecordComponentExperimental(int, string, string, string)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="access">
        ///     the record component access flags, the only possible value is
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Acc_Deprecated" />
        ///     .
        /// </param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">
        ///     the record component descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the record component signature. May be
        ///     <literal>null</literal>
        ///     if the record component
        ///     type does not use generic types.
        /// </param>
        /// <returns>
        ///     a visitor to visit this record component annotations and attributes, or
        ///     <literal>null</literal>
        ///     if this class visitor is not interested in visiting these annotations and attributes.
        /// </returns>
        //  [Obsolete(@"this API is experimental.")]
        public virtual Printer VisitRecordComponentExperimental(AccessFlags access, string name,
            string descriptor, string signature)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Class field.</summary>
        /// <remarks>
        ///     Class field. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitField(int, string, string, string, object)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="access">
        ///     the field's access flags (see
        ///     <see cref="Org.Objectweb.Asm.Opcodes" />
        ///     ). This parameter also indicates if
        ///     the field is synthetic and/or deprecated.
        /// </param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">
        ///     the field's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the field's signature. May be
        ///     <literal>null</literal>
        ///     if the field's type does not use
        ///     generic types.
        /// </param>
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
        ///     (for
        ///     <c>int</c>
        ///     ,
        ///     <c>float</c>
        ///     ,
        ///     <c>long</c>
        ///     or
        ///     <c>String</c>
        ///     fields respectively).
        ///     <i>
        ///         This parameter is only used for static
        ///         fields
        ///     </i>
        ///     . Its value is ignored for non static fields, which must be initialized through
        ///     bytecode instructions in constructors or methods.
        /// </param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitField(AccessFlags access, string name, string descriptor, string
            signature, object value);

        /// <summary>Class method.</summary>
        /// <remarks>
        ///     Class method. See
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitMethod(int, string, string, string, string[])
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Org.Objectweb.Asm.Opcodes" />
        ///     ). This parameter also indicates if
        ///     the method is synthetic and/or deprecated.
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="signature">
        ///     the method's signature. May be
        ///     <literal>null</literal>
        ///     if the method parameters,
        ///     return type and exceptions do not use generic types.
        /// </param>
        /// <param name="exceptions">
        ///     the internal names of the method's exception classes (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ). May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitMethod(AccessFlags access, string name, string descriptor, string
            signature, string[] exceptions);

        /// <summary>Class end.</summary>
        /// <remarks>
        ///     Class end. See
        ///     <see cref="ClassVisitor.VisitEnd" />
        ///     .
        /// </remarks>
        public abstract void VisitClassEnd();

        // -----------------------------------------------------------------------------------------------
        // Modules
        // -----------------------------------------------------------------------------------------------
        /// <summary>Module main class.</summary>
        /// <remarks>
        ///     Module main class. See
        ///     <see cref="ModuleVisitor.VisitMainClass" />
        ///     .
        /// </remarks>
        /// <param name="mainClass">
        ///     the internal name of the main class of the current module.
        /// </param>
        public virtual void VisitMainClass(string mainClass)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module package.</summary>
        /// <remarks>
        ///     Module package. See
        ///     <see cref="ModuleVisitor.VisitPackage" />
        ///     .
        /// </remarks>
        /// <param name="packaze">the internal name of a package.</param>
        public virtual void VisitPackage(string packaze)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module require.</summary>
        /// <remarks>
        ///     Module require. See
        ///     <see cref="ModuleVisitor.VisitRequire" />
        ///     .
        /// </remarks>
        /// <param name="module">the fully qualified name (using dots) of the dependence.</param>
        /// <param name="access">
        ///     the access flag of the dependence among
        ///     <c>ACC_TRANSITIVE</c>
        ///     ,
        ///     <c>ACC_STATIC_PHASE</c>
        ///     ,
        ///     <c>ACC_SYNTHETIC</c>
        ///     and
        ///     <c>ACC_MANDATED</c>
        ///     .
        /// </param>
        /// <param name="version">
        ///     the module version at compile time, or
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public virtual void VisitRequire(string module, AccessFlags access, string version)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module export.</summary>
        /// <remarks>
        ///     Module export. See
        ///     <see cref="ModuleVisitor.VisitExport" />
        ///     .
        /// </remarks>
        /// <param name="packaze">the internal name of the exported package.</param>
        /// <param name="access">
        ///     the access flag of the exported package, valid values are among
        ///     <c>ACC_SYNTHETIC</c>
        ///     and
        ///     <c>ACC_MANDATED</c>
        ///     .
        /// </param>
        /// <param name="modules">
        ///     the fully qualified names (using dots) of the modules that can access the public
        ///     classes of the exported package, or
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public virtual void VisitExport(string packaze, AccessFlags access, params string[] modules
        )
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module open.</summary>
        /// <remarks>
        ///     Module open. See
        ///     <see cref="ModuleVisitor.VisitOpen" />
        ///     .
        /// </remarks>
        /// <param name="packaze">the internal name of the opened package.</param>
        /// <param name="access">
        ///     the access flag of the opened package, valid values are among
        ///     <c>ACC_SYNTHETIC</c>
        ///     and
        ///     <c>ACC_MANDATED</c>
        ///     .
        /// </param>
        /// <param name="modules">
        ///     the fully qualified names (using dots) of the modules that can use deep
        ///     reflection to the classes of the open package, or
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public virtual void VisitOpen(string packaze, AccessFlags access, params string[] modules
        )
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module use.</summary>
        /// <remarks>
        ///     Module use. See
        ///     <see cref="ModuleVisitor.VisitUse" />
        ///     .
        /// </remarks>
        /// <param name="service">the internal name of the service.</param>
        public virtual void VisitUse(string service)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module provide.</summary>
        /// <remarks>
        ///     Module provide. See
        ///     <see cref="ModuleVisitor.VisitProvide" />
        ///     .
        /// </remarks>
        /// <param name="service">the internal name of the service.</param>
        /// <param name="providers">
        ///     the internal names of the implementations of the service (there is at least
        ///     one provider).
        /// </param>
        public virtual void VisitProvide(string service, params string[] providers)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Module end.</summary>
        /// <remarks>
        ///     Module end. See
        ///     <see cref="ModuleVisitor.VisitEnd" />
        ///     .
        /// </remarks>
        public virtual void VisitModuleEnd()
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        // -----------------------------------------------------------------------------------------------
        // Annotations
        // -----------------------------------------------------------------------------------------------
        /// <summary>Annotation value.</summary>
        /// <remarks>
        ///     Annotation value. See
        ///     <see cref="AnnotationVisitor.Visit" />
        ///     .
        /// </remarks>
        /// <param name="name">the value name.</param>
        /// <param name="value">
        ///     the actual value, whose type must be
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
        ///     <see cref="Type" />
        ///     of
        ///     <see cref="Type.Object" />
        ///     or
        ///     <see cref="Type.Array" />
        ///     sort. This value can also be an array of byte,
        ///     boolean, short, char, int, long, float or double values (this is equivalent to using
        ///     <see cref="VisitArray(string)" />
        ///     and visiting each array element in turn, but is more convenient).
        /// </param>
        public abstract void Visit(string name, object value);

        // DontCheck(OverloadMethodsDeclarationOrder): overloads are semantically different.
        /// <summary>Annotation enum value.</summary>
        /// <remarks>
        ///     Annotation enum value. See
        ///     <see cref="AnnotationVisitor.VisitEnum" />
        ///     .
        /// </remarks>
        /// <param name="name">the value name.</param>
        /// <param name="descriptor">the class descriptor of the enumeration class.</param>
        /// <param name="value">the actual enumeration value.</param>
        public abstract void VisitEnum(string name, string descriptor, string value);

        /// <summary>Nested annotation value.</summary>
        /// <remarks>
        ///     Nested annotation value. See
        ///     <see cref="AnnotationVisitor.VisitAnnotation" />
        ///     .
        /// </remarks>
        /// <param name="name">the value name.</param>
        /// <param name="descriptor">the class descriptor of the nested annotation class.</param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitAnnotation(string name, string descriptor);

        /// <summary>Annotation array value.</summary>
        /// <remarks>
        ///     Annotation array value. See
        ///     <see cref="AnnotationVisitor.VisitArray" />
        ///     .
        /// </remarks>
        /// <param name="name">the value name.</param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitArray(string name);

        /// <summary>Annotation end.</summary>
        /// <remarks>
        ///     Annotation end. See
        ///     <see cref="AnnotationVisitor.VisitEnd" />
        ///     .
        /// </remarks>
        public abstract void VisitAnnotationEnd();

        // -----------------------------------------------------------------------------------------------
        // Record components
        // -----------------------------------------------------------------------------------------------
        /// <summary>Visits an annotation of the record component.</summary>
        /// <remarks>
        ///     Visits an annotation of the record component. See
        ///     <see cref="Org.Objectweb.Asm.RecordComponentVisitor.VisitAnnotationExperimental(string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>
        ///     a visitor to visit the annotation values, or
        ///     <literal>null</literal>
        ///     if this visitor is not
        ///     interested in visiting this annotation.
        /// </returns>
        //  [Obsolete(@"this API is experimental.")]
        public virtual Printer VisitRecordComponentAnnotationExperimental(string descriptor
            , bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Visits an annotation on a type in the record component signature.</summary>
        /// <remarks>
        ///     Visits an annotation on a type in the record component signature. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.RecordComponentVisitor.VisitTypeAnnotationExperimental(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Class_Type_Parameter" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Class_Type_Parameter_Bound" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Class_Extends" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>
        ///     a visitor to visit the annotation values, or
        ///     <literal>null</literal>
        ///     if this visitor is not
        ///     interested in visiting this annotation.
        /// </returns>
        //  [Obsolete(@"this API is experimental.")]
        public virtual Printer VisitRecordComponentTypeAnnotationExperimental(int typeRef
            , TypePath typePath, string descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Visits a non standard attribute of the record component.</summary>
        /// <remarks>
        ///     Visits a non standard attribute of the record component. See
        ///     <see cref="Org.Objectweb.Asm.RecordComponentVisitor.VisitAttributeExperimental(Org.Objectweb.Asm.Attribute)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="attribute">an attribute.</param>
        //  [Obsolete(@"this API is experimental.")]
        public virtual void VisitRecordComponentAttributeExperimental(Attribute attribute
        )
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Visits the end of the record component.</summary>
        /// <remarks>
        ///     Visits the end of the record component. See
        ///     <see cref="RecordComponentVisitor.VisitEndExperimental" />
        ///     . This method, which is the last
        ///     one to be called, is used to inform the visitor that everything have been visited.
        /// </remarks>
        //  [Obsolete(@"this API is experimental.")]
        public virtual void VisitRecordComponentEndExperimental()
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        // -----------------------------------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------------------------------
        /// <summary>Field annotation.</summary>
        /// <remarks>
        ///     Field annotation. See
        ///     <see cref="FieldVisitor.VisitAnnotation" />
        ///     .
        /// </remarks>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitFieldAnnotation(string descriptor, bool visible);

        /// <summary>Field type annotation.</summary>
        /// <remarks>
        ///     Field type annotation. See
        ///     <see cref="Org.Objectweb.Asm.FieldVisitor.VisitTypeAnnotation(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Field" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitFieldTypeAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Field attribute.</summary>
        /// <remarks>
        ///     Field attribute. See
        ///     <see cref="Org.Objectweb.Asm.FieldVisitor.VisitAttribute(Org.Objectweb.Asm.Attribute)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="attribute">an attribute.</param>
        public abstract void VisitFieldAttribute(Attribute attribute);

        /// <summary>Field end.</summary>
        /// <remarks>
        ///     Field end. See
        ///     <see cref="FieldVisitor.VisitEnd" />
        ///     .
        /// </remarks>
        public abstract void VisitFieldEnd();

        // -----------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Method parameter.</summary>
        /// <remarks>
        ///     Method parameter. See
        ///     <see cref="MethodVisitor.VisitParameter" />
        ///     .
        /// </remarks>
        /// <param name="name">
        ///     parameter name or
        ///     <literal>null</literal>
        ///     if none is provided.
        /// </param>
        /// <param name="access">
        ///     the parameter's access flags, only
        ///     <c>ACC_FINAL</c>
        ///     ,
        ///     <c>ACC_SYNTHETIC</c>
        ///     or/and
        ///     <c>ACC_MANDATED</c>
        ///     are allowed (see
        ///     <see cref="Asm.Opcodes" />
        ///     ).
        /// </param>
        public virtual void VisitParameter(string name, AccessFlags access)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Method default annotation.</summary>
        /// <remarks>
        ///     Method default annotation. See
        ///     <see cref="MethodVisitor.VisitAnnotationDefault" />
        ///     .
        /// </remarks>
        /// <returns>the printer.</returns>
        public abstract Printer VisitAnnotationDefault();

        /// <summary>Method annotation.</summary>
        /// <remarks>
        ///     Method annotation. See
        ///     <see cref="MethodVisitor.VisitAnnotation" />
        ///     .
        /// </remarks>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitMethodAnnotation(string descriptor, bool visible);

        /// <summary>Method type annotation.</summary>
        /// <remarks>
        ///     Method type annotation. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitTypeAnnotation(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Type_Parameter" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Type_Parameter_Bound" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Return" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Receiver" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Formal_Parameter" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Throws" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitMethodTypeAnnotation(int typeRef, TypePath typePath,
            string descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Number of method parameters that can have annotations.</summary>
        /// <remarks>
        ///     Number of method parameters that can have annotations. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitAnnotableParameterCount(int, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="parameterCount">
        ///     the number of method parameters than can have annotations. This number
        ///     must be less or equal than the number of parameter types in the method descriptor. It can
        ///     be strictly less when a method has synthetic parameters and when these parameters are
        ///     ignored when computing parameter indices for the purpose of parameter annotations (see
        ///     https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.18).
        /// </param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     to define the number of method parameters that can have
        ///     annotations visible at runtime,
        ///     <literal>false</literal>
        ///     to define the number of method parameters
        ///     that can have annotations invisible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitAnnotableParameterCount(int parameterCount, bool visible
        )
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Method parameter annotation.</summary>
        /// <remarks>
        ///     Method parameter annotation. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitParameterAnnotation(int, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="parameter">
        ///     the parameter index. This index must be strictly smaller than the number of
        ///     parameters in the method descriptor, and strictly smaller than the parameter count
        ///     specified in
        ///     <see cref="VisitAnnotableParameterCount(int, bool)" />
        ///     . Important note:
        ///     <i>
        ///         a parameter index i
        ///         is not required to correspond to the i'th parameter descriptor in the method
        ///         descriptor
        ///     </i>
        ///     , in particular in case of synthetic parameters (see
        ///     https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.18).
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public abstract Printer VisitParameterAnnotation(int parameter, string descriptor
            , bool visible);

        /// <summary>Method attribute.</summary>
        /// <remarks>
        ///     Method attribute. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitAttribute(Org.Objectweb.Asm.Attribute)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="attribute">an attribute.</param>
        public abstract void VisitMethodAttribute(Attribute attribute);

        /// <summary>Method start.</summary>
        /// <remarks>
        ///     Method start. See
        ///     <see cref="MethodVisitor.VisitCode" />
        ///     .
        /// </remarks>
        public abstract void VisitCode();

        /// <summary>Method stack frame.</summary>
        /// <remarks>
        ///     Method stack frame. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitFrame(int, int, object[], int, object[])
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="type">
        ///     the type of this stack map frame. Must be
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_New" />
        ///     for expanded
        ///     frames, or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Full" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Append" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Chop" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Same" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Append" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.F_Same1" />
        ///     for compressed frames.
        /// </param>
        /// <param name="numLocal">the number of local variables in the visited frame.</param>
        /// <param name="local">
        ///     the local variable types in this frame. This array must not be modified. Primitive
        ///     types are represented by
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Top" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Integer" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Float" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Long" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Double" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Null" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Uninitialized_This" />
        ///     (long and double are represented by a single element).
        ///     Reference types are represented by String objects (representing internal names), and
        ///     uninitialized types by Label objects (this label designates the NEW instruction that
        ///     created this uninitialized value).
        /// </param>
        /// <param name="numStack">
        ///     the number of operand stack elements in the visited frame.
        /// </param>
        /// <param name="stack">
        ///     the operand stack types in this frame. This array must not be modified. Its
        ///     content has the same format as the "local" array.
        /// </param>
        public abstract void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="MethodVisitor.VisitInsn" />
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the instruction to be visited. This opcode is either NOP,
        ///     ACONST_NULL, ICONST_M1, ICONST_0, ICONST_1, ICONST_2, ICONST_3, ICONST_4, ICONST_5,
        ///     LCONST_0, LCONST_1, FCONST_0, FCONST_1, FCONST_2, DCONST_0, DCONST_1, IALOAD, LALOAD,
        ///     FALOAD, DALOAD, AALOAD, BALOAD, CALOAD, SALOAD, IASTORE, LASTORE, FASTORE, DASTORE,
        ///     AASTORE, BASTORE, CASTORE, SASTORE, POP, POP2, DUP, DUP_X1, DUP_X2, DUP2, DUP2_X1, DUP2_X2,
        ///     SWAP, IADD, LADD, FADD, DADD, ISUB, LSUB, FSUB, DSUB, IMUL, LMUL, FMUL, DMUL, IDIV, LDIV,
        ///     FDIV, DDIV, IREM, LREM, FREM, DREM, INEG, LNEG, FNEG, DNEG, ISHL, LSHL, ISHR, LSHR, IUSHR,
        ///     LUSHR, IAND, LAND, IOR, LOR, IXOR, LXOR, I2L, I2F, I2D, L2I, L2F, L2D, F2I, F2L, F2D, D2I,
        ///     D2L, D2F, I2B, I2C, I2S, LCMP, FCMPL, FCMPG, DCMPL, DCMPG, IRETURN, LRETURN, FRETURN,
        ///     DRETURN, ARETURN, RETURN, ARRAYLENGTH, ATHROW, MONITORENTER, or MONITOREXIT.
        /// </param>
        public abstract void VisitInsn(int opcode);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitIntInsn(int, int)" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the instruction to be visited. This opcode is either BIPUSH, SIPUSH
        ///     or NEWARRAY.
        /// </param>
        /// <param name="operand">
        ///     the operand of the instruction to be visited.<br />
        ///     When opcode is BIPUSH, operand value should be between Byte.MIN_VALUE and Byte.MAX_VALUE.
        ///     <br />
        ///     When opcode is SIPUSH, operand value should be between Short.MIN_VALUE and Short.MAX_VALUE.
        ///     <br />
        ///     When opcode is NEWARRAY, operand value should be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Boolean" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Char" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Float" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Double" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Byte" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Short" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Int" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.T_Long" />
        ///     .
        /// </param>
        public abstract void VisitIntInsn(int opcode, int operand);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="MethodVisitor.VisitVarInsn" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the local variable instruction to be visited. This opcode is either
        ///     ILOAD, LLOAD, FLOAD, DLOAD, ALOAD, ISTORE, LSTORE, FSTORE, DSTORE, ASTORE or RET.
        /// </param>
        /// <param name="var">
        ///     the operand of the instruction to be visited. This operand is the index of a local
        ///     variable.
        /// </param>
        public abstract void VisitVarInsn(int opcode, int var);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="MethodVisitor.VisitTypeInsn" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be visited. This opcode is either NEW,
        ///     ANEWARRAY, CHECKCAST or INSTANCEOF.
        /// </param>
        /// <param name="type">
        ///     the operand of the instruction to be visited. This operand must be the internal
        ///     name of an object or array class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        /// </param>
        public abstract void VisitTypeInsn(int opcode, string type);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitFieldInsn(int, string, string, string)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be visited. This opcode is either
        ///     GETSTATIC, PUTSTATIC, GETFIELD or PUTFIELD.
        /// </param>
        /// <param name="owner">
        ///     the internal name of the field's owner class (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="name">the field's name.</param>
        /// <param name="descriptor">
        ///     the field's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        public abstract void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitMethodInsn(int, string, string, string)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be visited. This opcode is either
        ///     INVOKEVIRTUAL, INVOKESPECIAL, INVOKESTATIC or INVOKEINTERFACE.
        /// </param>
        /// <param name="owner">
        ///     the internal name of the method's owner class (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        [Obsolete(@"use VisitMethodInsn(int, string, string, string, bool) instead."
        )]
        public virtual void VisitMethodInsn(int opcode, string owner, string name, string
            descriptor)
        {
            // This method was abstract before ASM5, and was therefore always overridden (without any
            // call to 'super'). Thus, at this point we necessarily have api >= ASM5, and we must then
            // redirect the method call to the ASM5 visitMethodInsn() method.
            VisitMethodInsn(opcode, owner, name, descriptor, opcode == OpcodesConstants.Invokeinterface
            );
        }

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitMethodInsn(int, string, string, string)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be visited. This opcode is either
        ///     INVOKEVIRTUAL, INVOKESPECIAL, INVOKESTATIC or INVOKEINTERFACE.
        /// </param>
        /// <param name="owner">
        ///     the internal name of the method's owner class (see
        ///     <see cref="Org.Objectweb.Asm.Type.GetInternalName()" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="isInterface">if the method's owner class is an interface.</param>
        public virtual void VisitMethodInsn(int opcode, string owner, string name, string
            descriptor, bool isInterface)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitInvokeDynamicInsn(string, string, Org.Objectweb.Asm.Handle, object[])
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="bootstrapMethodHandle">the bootstrap method.</param>
        /// <param name="bootstrapMethodArguments">
        ///     the bootstrap method constant arguments. Each argument must be
        ///     an
        ///     <see cref="int" />
        ///     ,
        ///     <see cref="float" />
        ///     ,
        ///     <see cref="long" />
        ///     ,
        ///     <see cref="double" />
        ///     ,
        ///     <see cref="string" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Handle" />
        ///     value. This method is allowed to modify the
        ///     content of the array so a caller should expect that this array may change.
        /// </param>
        public abstract void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments);

        /// <summary>Method jump instruction.</summary>
        /// <remarks>
        ///     Method jump instruction. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitJumpInsn(int, Org.Objectweb.Asm.Label)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="opcode">
        ///     the opcode of the type instruction to be visited. This opcode is either IFEQ,
        ///     IFNE, IFLT, IFGE, IFGT, IFLE, IF_ICMPEQ, IF_ICMPNE, IF_ICMPLT, IF_ICMPGE, IF_ICMPGT,
        ///     IF_ICMPLE, IF_ACMPEQ, IF_ACMPNE, GOTO, JSR, IFNULL or IFNONNULL.
        /// </param>
        /// <param name="label">
        ///     the operand of the instruction to be visited. This operand is a label that
        ///     designates the instruction to which the jump instruction may jump.
        /// </param>
        public abstract void VisitJumpInsn(int opcode, Label label);

        /// <summary>Method label.</summary>
        /// <remarks>
        ///     Method label. See
        ///     <see cref="MethodVisitor.VisitLabel" />
        ///     .
        /// </remarks>
        /// <param name="label">
        ///     a
        ///     <see cref="Label" />
        ///     object.
        /// </param>
        public abstract void VisitLabel(Label label);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="MethodVisitor.VisitLdcInsn" />
        ///     .
        /// </remarks>
        /// <param name="value">
        ///     the constant to be loaded on the stack. This parameter must be a non null
        ///     <see cref="int" />
        ///     , a
        ///     <see cref="float" />
        ///     , a
        ///     <see cref="long" />
        ///     , a
        ///     <see cref="double" />
        ///     , a
        ///     <see cref="string" />
        ///     , a
        ///     <see cref="Type" />
        ///     of OBJECT or ARRAY sort for
        ///     <c>.class</c>
        ///     constants, for classes whose version is
        ///     49, a
        ///     <see cref="Type" />
        ///     of METHOD sort for MethodType, a
        ///     <see cref="Handle" />
        ///     for MethodHandle
        ///     constants, for classes whose version is 51 or a
        ///     <see cref="ConstantDynamic" />
        ///     for a constant
        ///     dynamic for classes whose version is 55.
        /// </param>
        public abstract void VisitLdcInsn(object value);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="MethodVisitor.VisitIincInsn" />
        ///     .
        /// </remarks>
        /// <param name="var">index of the local variable to be incremented.</param>
        /// <param name="increment">amount to increment the local variable by.</param>
        public abstract void VisitIincInsn(int var, int increment);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitTableSwitchInsn(int, int, Org.Objectweb.Asm.Label, Org.Objectweb.Asm.Label[])
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="min">the minimum key value.</param>
        /// <param name="max">the maximum key value.</param>
        /// <param name="dflt">beginning of the default handler block.</param>
        /// <param name="labels">
        ///     beginnings of the handler blocks.
        ///     <c>labels[i]</c>
        ///     is the beginning of the
        ///     handler block for the
        ///     <c>min + i</c>
        ///     key.
        /// </param>
        public abstract void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels);

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitLookupSwitchInsn(Org.Objectweb.Asm.Label, int[], Org.Objectweb.Asm.Label[])
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="dflt">beginning of the default handler block.</param>
        /// <param name="keys">the values of the keys.</param>
        /// <param name="labels">
        ///     beginnings of the handler blocks.
        ///     <c>labels[i]</c>
        ///     is the beginning of the
        ///     handler block for the
        ///     <c>keys[i]</c>
        ///     key.
        /// </param>
        public abstract void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        );

        /// <summary>Method instruction.</summary>
        /// <remarks>
        ///     Method instruction. See
        ///     <see cref="MethodVisitor.VisitMultiANewArrayInsn" />
        ///     .
        /// </remarks>
        /// <param name="descriptor">
        ///     an array type descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="numDimensions">the number of dimensions of the array to allocate.</param>
        public abstract void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        );

        /// <summary>Instruction type annotation.</summary>
        /// <remarks>
        ///     Instruction type annotation. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitInsnAnnotation(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Instanceof" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.New" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Constructor_Reference" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Reference" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Cast" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Constructor_Invocation_Type_Argument" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Invocation_Type_Argument" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Constructor_Reference_Type_Argument" />
        ///     , or
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Method_Reference_Type_Argument" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitInsnAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Method exception handler.</summary>
        /// <remarks>
        ///     Method exception handler. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitTryCatchBlock(Org.Objectweb.Asm.Label, Org.Objectweb.Asm.Label, Org.Objectweb.Asm.Label, string)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="start">the beginning of the exception handler's scope (inclusive).</param>
        /// <param name="end">the end of the exception handler's scope (exclusive).</param>
        /// <param name="handler">the beginning of the exception handler's code.</param>
        /// <param name="type">
        ///     the internal name of the type of exceptions handled by the handler, or
        ///     <literal>null</literal>
        ///     to catch any exceptions (for "finally" blocks).
        /// </param>
        public abstract void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type);

        /// <summary>Try catch block type annotation.</summary>
        /// <remarks>
        ///     Try catch block type annotation. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitTryCatchAnnotation(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Exception_Parameter" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitTryCatchAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Method debug info.</summary>
        /// <remarks>
        ///     Method debug info. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitLocalVariable(string, string, string, Org.Objectweb.Asm.Label, Org.Objectweb.Asm.Label, int)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="name">the name of a local variable.</param>
        /// <param name="descriptor">the type descriptor of this local variable.</param>
        /// <param name="signature">
        ///     the type signature of this local variable. May be
        ///     <literal>null</literal>
        ///     if the local
        ///     variable type does not use generic types.
        /// </param>
        /// <param name="start">
        ///     the first instruction corresponding to the scope of this local variable
        ///     (inclusive).
        /// </param>
        /// <param name="end">
        ///     the last instruction corresponding to the scope of this local variable (exclusive).
        /// </param>
        /// <param name="index">the local variable's index.</param>
        public abstract void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index);

        /// <summary>Local variable type annotation.</summary>
        /// <remarks>
        ///     Local variable type annotation. See
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitTryCatchAnnotation(int, Org.Objectweb.Asm.TypePath, string, bool)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Local_Variable" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.TypeReference.Resource_Variable" />
        ///     . See
        ///     <see cref="Org.Objectweb.Asm.TypeReference" />
        ///     .
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be
        ///     <literal>null</literal>
        ///     if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="start">
        ///     the fist instructions corresponding to the continuous ranges that make the scope
        ///     of this local variable (inclusive).
        /// </param>
        /// <param name="end">
        ///     the last instructions corresponding to the continuous ranges that make the scope of
        ///     this local variable (exclusive). This array must have the same size as the 'start' array.
        /// </param>
        /// <param name="index">
        ///     the local variable's index in each range. This array must have the same size as
        ///     the 'start' array.
        /// </param>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>the printer.</returns>
        public virtual Printer VisitLocalVariableAnnotation(int typeRef, TypePath typePath
            , Label[] start, Label[] end, int[] index, string descriptor, bool visible)
        {
            throw new NotSupportedException(Unsupported_Operation);
        }

        /// <summary>Method debug info.</summary>
        /// <remarks>
        ///     Method debug info. See
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitLineNumber(int, Org.Objectweb.Asm.Label)
        /// 	" />
        ///     .
        /// </remarks>
        /// <param name="line">
        ///     a line number. This number refers to the source file from which the class was
        ///     compiled.
        /// </param>
        /// <param name="start">the first instruction corresponding to this line number.</param>
        public abstract void VisitLineNumber(int line, Label start);

        /// <summary>Method max stack and max locals.</summary>
        /// <remarks>
        ///     Method max stack and max locals. See
        ///     <see cref="MethodVisitor.VisitMaxs" />
        ///     .
        /// </remarks>
        /// <param name="maxStack">maximum stack size of the method.</param>
        /// <param name="maxLocals">maximum number of local variables for the method.</param>
        public abstract void VisitMaxs(int maxStack, int maxLocals);

        /// <summary>Method end.</summary>
        /// <remarks>
        ///     Method end. See
        ///     <see cref="MethodVisitor.VisitEnd" />
        ///     .
        /// </remarks>
        public abstract void VisitMethodEnd();

        // -----------------------------------------------------------------------------------------------
        // Print and utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns the text constructed by this visitor.</summary>
        /// <returns>
        ///     the text constructed by this visitor. See
        ///     <see cref="text" />
        ///     .
        /// </returns>
        public virtual IList<object> GetText()
        {
            return text;
        }

        /// <summary>Prints the text constructed by this visitor.</summary>
        /// <param name="printWriter">the print writer to be used.</param>
        public virtual void Print(TextWriter printWriter)
        {
            PrintList(printWriter, text);
        }

        /// <summary>Prints the given string tree.</summary>
        /// <param name="printWriter">the writer to be used to print the tree.</param>
        /// <param name="list">
        ///     a string tree, i.e., a string list that can contain other string lists, and so on
        ///     recursively.
        /// </param>
        internal static void PrintList<_T0>(TextWriter printWriter, IList<_T0> list)
        {
            foreach (var o in list)
                if (o is IList)
                    PrintList(printWriter, (IList<_T0>) o);
                else
                    printWriter.Write(o.ToString());
        }

        /// <summary>Appends a quoted string to the given string builder.</summary>
        /// <param name="stringBuilder">the buffer where the string must be added.</param>
        /// <param name="string">the string to be added.</param>
        public static void AppendString(StringBuilder stringBuilder, string @string)
        {
            stringBuilder.Append('\"');
            for (var i = 0; i < @string.Length; ++i)
            {
                var c = @string[i];
                if (c == '\n')
                {
                    stringBuilder.Append("\\n");
                }
                else if (c == '\r')
                {
                    stringBuilder.Append("\\r");
                }
                else if (c == '\\')
                {
                    stringBuilder.Append("\\\\");
                }
                else if (c == '"')
                {
                    stringBuilder.Append("\\\"");
                }
                else if (c < 0x20 || c > 0x7f)
                {
                    stringBuilder.Append("\\u");
                    if (c < 0x10)
                        stringBuilder.Append("000");
                    else if (c < 0x100)
                        stringBuilder.Append("00");
                    else if (c < 0x1000) stringBuilder.Append('0');

                    stringBuilder.Append(((int) c).ToString("x8"));
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            stringBuilder.Append('\"');
        }

        /// <summary>Prints a the given class to the given output.</summary>
        /// <remarks>
        ///     Prints a the given class to the given output.
        ///     <p>Command line arguments: [-debug] &lt;binary class name or class file name &gt;
        /// </remarks>
        /// <param name="args">the command line arguments.</param>
        /// <param name="usage">
        ///     the help message to show when command line arguments are incorrect.
        /// </param>
        /// <param name="printer">the printer to convert the class into text.</param>
        /// <param name="output">where to print the result.</param>
        /// <param name="logger">where to log errors.</param>
        /// <exception cref="System.IO.IOException">
        ///     if the class cannot be found, or if an IOException occurs.
        /// </exception>
        internal static void Main(string[] args, string usage, Printer printer, TextWriter
            output, TextWriter logger)
        {
            if (args.Length < 1 || args.Length > 2 || args[0].Equals("-debug") && args.Length
                != 2)
            {
                logger.WriteLine(usage);
                return;
            }

            var traceClassVisitor = new TraceClassVisitor(null, printer, output
            );
            string className;
            ParsingOptions parsingOptions;
            if (args[0].Equals("-debug"))
            {
                className = args[1];
                parsingOptions = ParsingOptions.SkipDebug;
            }
            else
            {
                className = args[0];
                parsingOptions = 0;
            }

            if (className.EndsWith(".class") || className.IndexOf('\\') != -1 || className.IndexOf
                    ('/') != -1)
            {
                var inputStream = new MemoryStream(File.ReadAllBytes(className)).ToInputStream();
                // NOPMD(AvoidFileStream): can't fix for 1.5 compatibility
                new ClassReader(inputStream).Accept(traceClassVisitor, parsingOptions);
            }
        }
    }
}