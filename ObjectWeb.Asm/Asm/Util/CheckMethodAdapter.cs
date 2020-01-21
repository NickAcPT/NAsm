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
using System.IO;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Asm.Tree;
using ObjectWeb.Asm.Tree.Analysis;
using ObjectWeb.Misc.Java.Lang;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="Org.Objectweb.Asm.MethodVisitor" />
    ///     that checks that its methods are properly used. More precisely this
    ///     method adapter checks each instruction individually, i.e., each visit method checks some
    ///     preconditions based <i>only</i> on its arguments - such as the fact that the given opcode is
    ///     correct for a given visit method. This adapter can also perform some basic data flow checks (more
    ///     precisely those that can be performed without the full class hierarchy - see
    ///     <see cref="Org.Objectweb.Asm.Tree.Analysis.BasicVerifier" />
    ///     ). For instance in a method whose signature is
    ///     <c>void m ()</c>
    ///     , the invalid instruction IRETURN, or the invalid sequence IADD L2I will be
    ///     detected if the data flow checks are enabled. These checks are enabled by using the
    ///     <see
    ///         cref="CheckMethodAdapter(int, string, string, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
    /// 	" />
    ///     constructor. They are not performed if
    ///     any other constructor is used.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class CheckMethodAdapter : MethodVisitor
    {
        private const string Invalid = "Invalid ";

        private const string Invalid_Descriptor = "Invalid descriptor: ";

        private const string Invalid_Type_Reference = "Invalid type reference sort 0x";

        private const string Invalid_Local_Variable_Index = "Invalid local variable index";

        private const string Must_Not_Be_Null_Or_Empty = " (must not be null or empty)";

        private const string Start_Label = "start label";

        private const string End_Label = "end label";

        /// <summary>The method to use to visit each instruction.</summary>
        /// <remarks>
        ///     The method to use to visit each instruction. Only generic methods are represented here.
        /// </remarks>
        private static readonly Method[] Opcode_Methods =
        {
            Method.Visit_Insn, Method.Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Int_Insn, Method.Visit_Int_Insn, null, null, null, Method
                .Visit_Var_Insn,
            Method.Visit_Var_Insn, Method
                .Visit_Var_Insn,
            Method.Visit_Var_Insn, Method
                .Visit_Var_Insn,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Var_Insn, Method.Visit_Var_Insn, Method.Visit_Var_Insn,
            Method.Visit_Var_Insn, Method.Visit_Var_Insn, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, Method.Visit_Insn,
            Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            null, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn,
            Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn,
            Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn,
            Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Jump_Insn,
            Method.Visit_Jump_Insn, Method.Visit_Jump_Insn, Method.Visit_Var_Insn, null, null, Method.Visit_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, Method.Visit_Field_Insn, Method.Visit_Field_Insn, Method.Visit_Field_Insn,
            Method.Visit_Field_Insn, Method.Visit_Method_Insn, Method.Visit_Method_Insn, Method.Visit_Method_Insn,
            Method.Visit_Method_Insn, null, Method.Visit_Type_Insn, Method.Visit_Int_Insn, Method.Visit_Type_Insn,
            Method.Visit_Insn, Method.Visit_Insn, Method
                .Visit_Type_Insn,
            Method.Visit_Type_Insn, Method
                .Visit_Insn,
            Method.Visit_Insn, null, null, Method
                .Visit_Jump_Insn,
            Method.Visit_Jump_Insn
        };

        /// <summary>The access flags of the visited method.</summary>
        private readonly int access;

        /// <summary>The exception handler ranges.</summary>
        /// <remarks>
        ///     The exception handler ranges. Each pair of list element contains the start and end labels of an
        ///     exception handler block.
        /// </remarks>
        private readonly IList<Label> handlers;

        /// <summary>The index of the instruction designated by each visited label.</summary>
        private readonly IDictionary<Label, int> labelInsnIndices;

        /// <summary>The labels referenced by the visited method.</summary>
        private readonly HashSet<Label> referencedLabels;

        /// <summary>The number of visited instructions so far.</summary>
        private int insnCount;

        /// <summary>
        ///     The number of method parameters that can have runtime invisible annotations.
        /// </summary>
        /// <remarks>
        ///     The number of method parameters that can have runtime invisible annotations. 0 means that all
        ///     the parameters from the method descriptor can have annotations.
        /// </remarks>
        private int invisibleAnnotableParameterCount;

        /// <summary>
        ///     The index of the instruction corresponding to the last visited stack map frame.
        /// </summary>
        private int lastFrameInsnIndex = -1;

        /// <summary>The number of visited frames in compressed form.</summary>
        private int numCompressedFrames;

        /// <summary>The number of visited frames in expanded form.</summary>
        private int numExpandedFrames;

        /// <summary>The class version number.</summary>
        public int version;

        /// <summary>
        ///     The number of method parameters that can have runtime visible annotations.
        /// </summary>
        /// <remarks>
        ///     The number of method parameters that can have runtime visible annotations. 0 means that all the
        ///     parameters from the method descriptor can have annotations.
        /// </remarks>
        private int visibleAnnotableParameterCount;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitCode()" />
        ///     method has been called.
        /// </summary>
        private bool visitCodeCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitEnd()" />
        ///     method has been called.
        /// </summary>
        private bool visitEndCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitMaxs(int, int)" />
        ///     method has been called.
        /// </summary>
        private bool visitMaxCalled;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckMethodAdapter" />
        ///     object. This method adapter will not perform any
        ///     data flow check (see
        ///     <see
        ///         cref="CheckMethodAdapter(int, string, string, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
        /// 	" />
        ///     ).
        ///     <i>Subclasses must not use this constructor</i>. Instead, they must use the
        ///     <see
        ///         cref="CheckMethodAdapter(int, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="methodvisitor">
        ///     the method visitor to which this adapter must delegate calls.
        /// </param>
        public CheckMethodAdapter(MethodVisitor methodvisitor)
            : this(methodvisitor, new Dictionary<Label, int>())
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckMethodAdapter" />
        ///     object. This method adapter will not perform any
        ///     data flow check (see
        ///     <see
        ///         cref="CheckMethodAdapter(int, string, string, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
        /// 	" />
        ///     ).
        ///     <i>Subclasses must not use this constructor</i>. Instead, they must use the
        ///     <see
        ///         cref="CheckMethodAdapter(int, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="labelInsnIndices">
        ///     the index of the instruction designated by each visited label so far
        ///     (in other methods). This map is updated with the labels from the visited method.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public CheckMethodAdapter(MethodVisitor methodVisitor, IDictionary<Label, int> labelInsnIndices
        )
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, methodVisitor, labelInsnIndices)
        {
            // NOP
            // ACONST_NULL
            // ICONST_M1
            // ICONST_0
            // ICONST_1
            // ICONST_2
            // ICONST_3
            // ICONST_4
            // ICONST_5
            // LCONST_0
            // LCONST_1
            // FCONST_0
            // FCONST_1
            // FCONST_2
            // DCONST_0
            // DCONST_1
            // BIPUSH
            // SIPUSH
            // LDC
            // LDC_W
            // LDC2_W
            // ILOAD
            // LLOAD
            // FLOAD
            // DLOAD
            // ALOAD
            // ILOAD_0
            // ILOAD_1
            // ILOAD_2
            // ILOAD_3
            // LLOAD_0
            // LLOAD_1
            // LLOAD_2
            // LLOAD_3
            // FLOAD_0
            // FLOAD_1
            // FLOAD_2
            // FLOAD_3
            // DLOAD_0
            // DLOAD_1
            // DLOAD_2
            // DLOAD_3
            // ALOAD_0
            // ALOAD_1
            // ALOAD_2
            // ALOAD_3
            // IALOAD
            // LALOAD
            // FALOAD
            // DALOAD
            // AALOAD
            // BALOAD
            // CALOAD
            // SALOAD
            // ISTORE
            // LSTORE
            // FSTORE
            // DSTORE
            // ASTORE
            // ISTORE_0
            // ISTORE_1
            // ISTORE_2
            // ISTORE_3
            // LSTORE_0
            // LSTORE_1
            // LSTORE_2
            // LSTORE_3
            // FSTORE_0
            // FSTORE_1
            // FSTORE_2
            // FSTORE_3
            // DSTORE_0
            // DSTORE_1
            // DSTORE_2
            // DSTORE_3
            // ASTORE_0
            // ASTORE_1
            // ASTORE_2
            // ASTORE_3
            // IASTORE
            // LASTORE
            // FASTORE
            // DASTORE
            // AASTORE
            // BASTORE
            // CASTORE
            // SASTORE
            // POP
            // POP2
            // DUP
            // DUP_X1
            // DUP_X2
            // DUP2
            // DUP2_X1
            // DUP2_X2
            // SWAP
            // IADD
            // LADD
            // FADD
            // DADD
            // ISUB
            // LSUB
            // FSUB
            // DSUB
            // IMUL
            // LMUL
            // FMUL
            // DMUL
            // IDIV
            // LDIV
            // FDIV
            // DDIV
            // IREM
            // LREM
            // FREM
            // DREM
            // INEG
            // LNEG
            // FNEG
            // DNEG
            // ISHL
            // LSHL
            // ISHR
            // LSHR
            // IUSHR
            // LUSHR
            // IAND
            // LAND
            // IOR
            // LOR
            // IXOR
            // LXOR
            // IINC
            // I2L
            // I2F
            // I2D
            // L2I
            // L2F
            // L2D
            // F2I
            // F2L
            // F2D
            // D2I
            // D2L
            // D2F
            // I2B
            // I2C
            // I2S
            // LCMP
            // FCMPL
            // FCMPG
            // DCMPL
            // DCMPG
            // IFEQ
            // IFNE
            // IFLT
            // IFGE
            // IFGT
            // IFLE
            // IF_ICMPEQ
            // IF_ICMPNE
            // IF_ICMPLT
            // IF_ICMPGE
            // IF_ICMPGT
            // IF_ICMPLE
            // IF_ACMPEQ
            // IF_ACMPNE
            // GOTO
            // JSR
            // RET
            // TABLESWITCH
            // LOOKUPSWITCH
            // IRETURN
            // LRETURN
            // FRETURN
            // DRETURN
            // ARETURN
            // RETURN
            // GETSTATIC
            // PUTSTATIC
            // GETFIELD
            // PUTFIELD
            // INVOKEVIRTUAL
            // INVOKESPECIAL
            // INVOKESTATIC
            // INVOKEINTERFACE
            // INVOKEDYNAMIC
            // NEW
            // NEWARRAY
            // ANEWARRAY
            // ARRAYLENGTH
            // ATHROW
            // CHECKCAST
            // INSTANCEOF
            // MONITORENTER
            // MONITOREXIT
            // WIDE
            // MULTIANEWARRAY
            // IFNULL
            // IFNONNULL
            /* latest api = */
            if (GetType() != typeof(CheckMethodAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckMethodAdapter" />
        ///     object. This method adapter will not perform any
        ///     data flow check (see
        ///     <see
        ///         cref="CheckMethodAdapter(int, string, string, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
        /// 	" />
        ///     ).
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this CheckMethodAdapter. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="labelInsnIndices">
        ///     the index of the instruction designated by each visited label so far
        ///     (in other methods). This map is updated with the labels from the visited method.
        /// </param>
        protected internal CheckMethodAdapter(VisitorAsmApiVersion api, MethodVisitor methodVisitor, IDictionary
            <Label, int> labelInsnIndices)
            : base(api, methodVisitor)
        {
            this.labelInsnIndices = labelInsnIndices;
            referencedLabels = new HashSet<Label>();
            handlers = new List<Label>();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckMethodAdapter" />
        ///     object. This method adapter will perform basic data
        ///     flow checks. For instance in a method whose signature is
        ///     <c>void m ()</c>
        ///     , the invalid
        ///     instruction IRETURN, or the invalid sequence IADD L2I will be detected.
        ///     <i>
        ///         Subclasses must not
        ///         use this constructor
        ///     </i>
        ///     . Instead, they must use the
        ///     <see
        ///         cref="CheckMethodAdapter(int, int, string, string, Org.Objectweb.Asm.MethodVisitor, System.Collections.Generic.IDictionary{K, V})
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="access">the method's access flags.</param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="labelInsnIndices">
        ///     the index of the instruction designated by each visited label so far
        ///     (in other methods). This map is updated with the labels from the visited method.
        /// </param>
        public CheckMethodAdapter(int access, string name, string descriptor, MethodVisitor
            methodVisitor, IDictionary<Label, int> labelInsnIndices)
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, access, name, descriptor, methodVisitor, labelInsnIndices
            )
        {
            /* latest api = */
            if (GetType() != typeof(CheckMethodAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckMethodAdapter" />
        ///     object. This method adapter will perform basic data
        ///     flow checks. For instance in a method whose signature is
        ///     <c>void m ()</c>
        ///     , the invalid
        ///     instruction IRETURN, or the invalid sequence IADD L2I will be detected.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this CheckMethodAdapter. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        /// <param name="access">the method's access flags.</param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="labelInsnIndices">
        ///     the index of the instruction designated by each visited label so far
        ///     (in other methods). This map is updated with the labels from the visited method.
        /// </param>
        protected internal CheckMethodAdapter(VisitorAsmApiVersion api, int access, string name, string descriptor
            , MethodVisitor methodVisitor, IDictionary<Label, int> labelInsnIndices)
            : this(api, new _MethodNode_445(methodVisitor, api, access, name, descriptor, null
                , null), labelInsnIndices)
        {
            this.access = access;
        }

        public override void VisitParameter(string name, int access)
        {
            if (name != null) CheckUnqualifiedName(version, name, "name");
            CheckClassAdapter.CheckAccess(access, OpcodesConstants.Acc_Final + OpcodesConstants
                                                      .Acc_Mandated + OpcodesConstants.Acc_Synthetic);
            base.VisitParameter(name, access);
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            CheckVisitEndNotCalled();
            CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitAnnotation(descriptor, visible));
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            CheckVisitEndNotCalled();
            var sort = new TypeReference(typeRef).GetSort();
            if (sort != TypeReference.Method_Type_Parameter && sort != TypeReference.Method_Type_Parameter_Bound
                                                            && sort != TypeReference.Method_Return &&
                                                            sort != TypeReference.Method_Receiver
                                                            && sort != TypeReference.Method_Formal_Parameter &&
                                                            sort != TypeReference.Throws)
                throw new ArgumentException(Invalid_Type_Reference + sort.ToString("x8"));
            CheckClassAdapter.CheckTypeRef(typeRef);
            CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitTypeAnnotation(typeRef, typePath, descriptor
                , visible));
        }

        public override AnnotationVisitor VisitAnnotationDefault()
        {
            CheckVisitEndNotCalled();
            return new CheckAnnotationAdapter(base.VisitAnnotationDefault(), false);
        }

        public override void VisitAnnotableParameterCount(int parameterCount, bool visible
        )
        {
            CheckVisitEndNotCalled();
            if (visible)
                visibleAnnotableParameterCount = parameterCount;
            else
                invisibleAnnotableParameterCount = parameterCount;
            base.VisitAnnotableParameterCount(parameterCount, visible);
        }

        public override AnnotationVisitor VisitParameterAnnotation(int parameter, string
            descriptor, bool visible)
        {
            CheckVisitEndNotCalled();
            if (visible && visibleAnnotableParameterCount > 0 && parameter >= visibleAnnotableParameterCount ||
                !visible && invisibleAnnotableParameterCount > 0 && parameter >= invisibleAnnotableParameterCount)
                throw new ArgumentException("Invalid parameter index");
            CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitParameterAnnotation(parameter, descriptor
                , visible));
        }

        public override void VisitAttribute(Attribute attribute)
        {
            CheckVisitEndNotCalled();
            if (attribute == null) throw new ArgumentException("Invalid attribute (must not be null)");
            base.VisitAttribute(attribute);
        }

        public override void VisitCode()
        {
            if ((access & OpcodesConstants.Acc_Abstract) != 0)
                throw new NotSupportedException("Abstract methods cannot have code");
            visitCodeCalled = true;
            base.VisitCode();
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            if (insnCount == lastFrameInsnIndex)
                throw new InvalidOperationException("At most one frame can be visited at a given code location."
                );
            lastFrameInsnIndex = insnCount;
            int maxNumLocal;
            int maxNumStack;
            switch (type)
            {
                case OpcodesConstants.F_New:
                case OpcodesConstants.F_Full:
                {
                    maxNumLocal = int.MaxValue;
                    maxNumStack = int.MaxValue;
                    break;
                }

                case OpcodesConstants.F_Same:
                {
                    maxNumLocal = 0;
                    maxNumStack = 0;
                    break;
                }

                case OpcodesConstants.F_Same1:
                {
                    maxNumLocal = 0;
                    maxNumStack = 1;
                    break;
                }

                case OpcodesConstants.F_Append:
                case OpcodesConstants.F_Chop:
                {
                    maxNumLocal = 3;
                    maxNumStack = 0;
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid frame type " + type);
                }
            }

            if (numLocal > maxNumLocal)
                throw new ArgumentException("Invalid numLocal=" + numLocal + " for frame type " +
                                            type);
            if (numStack > maxNumStack)
                throw new ArgumentException("Invalid numStack=" + numStack + " for frame type " +
                                            type);
            if (type != OpcodesConstants.F_Chop)
            {
                if (numLocal > 0 && (local == null || local.Length < numLocal))
                    throw new ArgumentException("Array local[] is shorter than numLocal");
                for (var i = 0; i < numLocal; ++i) CheckFrameValue(local[i]);
            }

            if (numStack > 0 && (stack == null || stack.Length < numStack))
                throw new ArgumentException("Array stack[] is shorter than numStack");
            for (var i = 0; i < numStack; ++i) CheckFrameValue(stack[i]);
            if (type == OpcodesConstants.F_New)
                ++numExpandedFrames;
            else
                ++numCompressedFrames;
            if (numExpandedFrames > 0 && numCompressedFrames > 0)
                throw new ArgumentException("Expanded and compressed frames must not be mixed.");
            base.VisitFrame(type, numLocal, local, numStack, stack);
        }

        public override void VisitInsn(int opcode)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Insn);
            base.VisitInsn(opcode);
            ++insnCount;
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Int_Insn);
            switch (opcode)
            {
                case OpcodesConstants.Bipush:
                {
                    CheckSignedByte(operand, "Invalid operand");
                    break;
                }

                case OpcodesConstants.Sipush:
                {
                    CheckSignedShort(operand, "Invalid operand");
                    break;
                }

                case OpcodesConstants.Newarray:
                {
                    if (operand < OpcodesConstants.T_Boolean || operand > OpcodesConstants.T_Long)
                        throw new ArgumentException("Invalid operand (must be an array type code T_...): "
                                                    + operand);
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }

            base.VisitIntInsn(opcode, operand);
            ++insnCount;
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Var_Insn);
            CheckUnsignedShort(var, Invalid_Local_Variable_Index);
            base.VisitVarInsn(opcode, var);
            ++insnCount;
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Type_Insn);
            CheckInternalName(version, type, "type");
            if (opcode == OpcodesConstants.New && type[0] == '[')
                throw new ArgumentException("NEW cannot be used to create arrays: " + type);
            base.VisitTypeInsn(opcode, type);
            ++insnCount;
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Field_Insn);
            CheckInternalName(version, owner, "owner");
            CheckUnqualifiedName(version, name, "name");
            CheckDescriptor(version, descriptor, false);
            base.VisitFieldInsn(opcode, owner, name, descriptor);
            ++insnCount;
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name
            , string descriptor, bool isInterface)
        {
            if (api < ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm5 && (opcodeAndSource & OpcodesConstants.Source_Deprecated
                ) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            var opcode = opcodeAndSource & ~OpcodesConstants.Source_Mask;
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Method_Insn);
            if (opcode != OpcodesConstants.Invokespecial || !"<init>".Equals(name))
                CheckMethodIdentifier(version, name, "name");
            CheckInternalName(version, owner, "owner");
            CheckMethodDescriptor(version, descriptor);
            if (opcode == OpcodesConstants.Invokevirtual && isInterface)
                throw new ArgumentException("INVOKEVIRTUAL can't be used with interfaces");
            if (opcode == OpcodesConstants.Invokeinterface && !isInterface)
                throw new ArgumentException("INVOKEINTERFACE can't be used with classes");
            if (opcode == OpcodesConstants.Invokespecial && isInterface && (version & 0xFFFF) < OpcodesConstants.V1_8)
                throw new ArgumentException("INVOKESPECIAL can't be used with interfaces prior to Java 8"
                );
            base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
            ++insnCount;
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckMethodIdentifier(version, name, "name");
            CheckMethodDescriptor(version, descriptor);
            if (bootstrapMethodHandle.GetTag() != OpcodesConstants.H_Invokestatic && bootstrapMethodHandle
                    .GetTag() != OpcodesConstants.H_Newinvokespecial)
                throw new ArgumentException("invalid handle tag " + bootstrapMethodHandle.GetTag(
                                            ));
            foreach (var bootstrapMethodArgument in bootstrapMethodArguments) CheckLdcConstant(bootstrapMethodArgument);
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
            ++insnCount;
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckOpcodeMethod(opcode, Method.Visit_Jump_Insn);
            CheckLabel(label, false, "label");
            base.VisitJumpInsn(opcode, label);
            referencedLabels.Add(label);
            ++insnCount;
        }

        public override void VisitLabel(Label label)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckLabel(label, false, "label");
            if (labelInsnIndices.GetOrNullable(label) != null) throw new ArgumentException("Already visited label");
            Collections.Put(labelInsnIndices, label, insnCount);
            base.VisitLabel(label);
        }

        public override void VisitLdcInsn(object value)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckLdcConstant(value);
            base.VisitLdcInsn(value);
            ++insnCount;
        }

        public override void VisitIincInsn(int var, int increment)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckUnsignedShort(var, Invalid_Local_Variable_Index);
            CheckSignedShort(increment, "Invalid increment");
            base.VisitIincInsn(var, increment);
            ++insnCount;
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            if (max < min)
                throw new ArgumentException("Max = " + max + " must be greater than or equal to min = "
                                            + min);
            CheckLabel(dflt, false, "default label");
            if (labels == null || labels.Length != max - min + 1)
                throw new ArgumentException("There must be max - min + 1 labels");
            for (var i = 0; i < labels.Length; ++i) CheckLabel(labels[i], false, "label at index " + i);
            base.VisitTableSwitchInsn(min, max, dflt, labels);
            foreach (var label in labels) referencedLabels.Add(label);
            ++insnCount;
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            CheckVisitMaxsNotCalled();
            CheckVisitCodeCalled();
            CheckLabel(dflt, false, "default label");
            if (keys == null || labels == null || keys.Length != labels.Length)
                throw new ArgumentException("There must be the same number of keys and labels");
            for (var i = 0; i < labels.Length; ++i) CheckLabel(labels[i], false, "label at index " + i);
            base.VisitLookupSwitchInsn(dflt, keys, labels);
            referencedLabels.Add(dflt);
            foreach (var label in labels) referencedLabels.Add(label);
            ++insnCount;
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckDescriptor(version, descriptor, false);
            if (descriptor[0] != '[')
                throw new ArgumentException("Invalid descriptor (must be an array type descriptor): "
                                            + descriptor);
            if (numDimensions < 1)
                throw new ArgumentException("Invalid dimensions (must be greater than 0): " + numDimensions
                );
            if (numDimensions > descriptor.LastIndexOf('[') + 1)
                throw new ArgumentException("Invalid dimensions (must not be greater than numDimensions(descriptor)): "
                                            + numDimensions);
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
            ++insnCount;
        }

        public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            var sort = new TypeReference(typeRef).GetSort();
            if (sort != TypeReference.Instanceof && sort != TypeReference.New && sort != TypeReference
                    .Constructor_Reference && sort != TypeReference.Method_Reference && sort != TypeReference
                    .Cast && sort != TypeReference.Constructor_Invocation_Type_Argument && sort != TypeReference
                    .Method_Invocation_Type_Argument && sort != TypeReference.Constructor_Reference_Type_Argument
                && sort != TypeReference.Method_Reference_Type_Argument)
                throw new ArgumentException(Invalid_Type_Reference + sort.ToString("x8"));
            CheckClassAdapter.CheckTypeRef(typeRef);
            CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitInsnAnnotation(typeRef, typePath, descriptor
                , visible));
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckLabel(start, false, Start_Label);
            CheckLabel(end, false, End_Label);
            CheckLabel(handler, false, "handler label");
            if (labelInsnIndices.GetOrNullable(start) != null || labelInsnIndices.GetOrNullable
                    (end) != null || labelInsnIndices.GetOrNullable(handler) != null)
                throw new InvalidOperationException("Try catch blocks must be visited before their labels"
                );
            if (type != null) CheckInternalName(version, type, "type");
            base.VisitTryCatchBlock(start, end, handler, type);
            handlers.Add(start);
            handlers.Add(end);
        }

        public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            var sort = new TypeReference(typeRef).GetSort();
            if (sort != TypeReference.Exception_Parameter)
                throw new ArgumentException(Invalid_Type_Reference + sort.ToString("x8"));
            CheckClassAdapter.CheckTypeRef(typeRef);
            CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitTryCatchAnnotation(typeRef, typePath,
                descriptor, visible));
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckUnqualifiedName(version, name, "name");
            CheckDescriptor(version, descriptor, false);
            if (signature != null) CheckClassAdapter.CheckFieldSignature(signature);
            CheckLabel(start, true, Start_Label);
            CheckLabel(end, true, End_Label);
            CheckUnsignedShort(index, Invalid_Local_Variable_Index);
            var startInsnIndex = labelInsnIndices.GetOrNullable(start);
            var endInsnIndex = labelInsnIndices.GetOrNullable(end);
            if (endInsnIndex.Value < startInsnIndex.Value)
                throw new ArgumentException("Invalid start and end labels (end must be greater than start)"
                );
            base.VisitLocalVariable(name, descriptor, signature, start, end, index);
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath
                typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible
        )
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            var sort = new TypeReference(typeRef).GetSort();
            if (sort != TypeReference.Local_Variable && sort != TypeReference.Resource_Variable)
                throw new ArgumentException(Invalid_Type_Reference + sort.ToString("x8"));
            CheckClassAdapter.CheckTypeRef(typeRef);
            CheckDescriptor(version, descriptor, false);
            if (start == null || end == null || index == null || end.Length != start.Length ||
                index.Length != start.Length)
                throw new ArgumentException(
                    "Invalid start, end and index arrays (must be non null and of identical length"
                );
            for (var i = 0; i < start.Length; ++i)
            {
                CheckLabel(start[i], true, Start_Label);
                CheckLabel(end[i], true, End_Label);
                CheckUnsignedShort(index[i], Invalid_Local_Variable_Index);
                var startInsnIndex = labelInsnIndices.GetOrNullable(start[i]);
                var endInsnIndex = labelInsnIndices.GetOrNullable(end[i]);
                if (endInsnIndex.Value < startInsnIndex.Value)
                    throw new ArgumentException("Invalid start and end labels (end must be greater than start)"
                    );
            }

            return base.VisitLocalVariableAnnotation(typeRef, typePath, start, end, index, descriptor
                , visible);
        }

        public override void VisitLineNumber(int line, Label start)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            CheckUnsignedShort(line, "Invalid line number");
            CheckLabel(start, true, Start_Label);
            base.VisitLineNumber(line, start);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            CheckVisitCodeCalled();
            CheckVisitMaxsNotCalled();
            visitMaxCalled = true;
            foreach (var l in referencedLabels)
                if (labelInsnIndices.GetOrNullable(l) == null)
                    throw new InvalidOperationException("Undefined label used");
            for (var i = 0; i < handlers.Count; i += 2)
            {
                var startInsnIndex = labelInsnIndices.GetOrNullable(handlers[i]);
                var endInsnIndex = labelInsnIndices.GetOrNullable(handlers[i + 1]);
                if (startInsnIndex == null || endInsnIndex == null)
                    throw new InvalidOperationException("Undefined try catch block labels");
                if (endInsnIndex <= startInsnIndex)
                    throw new InvalidOperationException("Emty try catch block handler range");
            }

            CheckUnsignedShort(maxStack, "Invalid max stack");
            CheckUnsignedShort(maxLocals, "Invalid max locals");
            base.VisitMaxs(maxStack, maxLocals);
        }

        public override void VisitEnd()
        {
            CheckVisitEndNotCalled();
            visitEndCalled = true;
            base.VisitEnd();
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Checks that the
        ///     <see cref="VisitCode()" />
        ///     method has been called.
        /// </summary>
        private void CheckVisitCodeCalled()
        {
            if (!visitCodeCalled)
                throw new InvalidOperationException("Cannot visit instructions before visitCode has been called."
                );
        }

        /// <summary>
        ///     Checks that the
        ///     <see cref="VisitMaxs(int, int)" />
        ///     method has not been called.
        /// </summary>
        private void CheckVisitMaxsNotCalled()
        {
            if (visitMaxCalled)
                throw new InvalidOperationException("Cannot visit instructions after visitMaxs has been called."
                );
        }

        /// <summary>
        ///     Checks that the
        ///     <see cref="VisitEnd()" />
        ///     method has not been called.
        /// </summary>
        private void CheckVisitEndNotCalled()
        {
            if (visitEndCalled)
                throw new InvalidOperationException("Cannot visit elements after visitEnd has been called."
                );
        }

        /// <summary>Checks a stack frame value.</summary>
        /// <param name="value">the value to be checked.</param>
        private void CheckFrameValue(object value)
        {
            if (value == (object) OpcodesConstants.Top || value == (object) OpcodesConstants.Integer || value ==
                (object) OpcodesConstants.Float || value == (object) OpcodesConstants.Long || value ==
                (object) OpcodesConstants
                    .Double || value == (object) OpcodesConstants.Null ||
                value == (object) OpcodesConstants.Uninitialized_This)
                return;
            if (value is string)
                CheckInternalName(version, (string) value, "Invalid stack frame value");
            else if (value is Label)
                referencedLabels.Add((Label) value);
            else
                throw new ArgumentException("Invalid stack frame value: " + value);
        }

        /// <summary>
        ///     Checks that the method to visit the given opcode is equal to the given method.
        /// </summary>
        /// <param name="opcode">the opcode to be checked.</param>
        /// <param name="method">the expected visit method.</param>
        private static void CheckOpcodeMethod(int opcode, Method method
        )
        {
            if (opcode < OpcodesConstants.Nop || opcode > OpcodesConstants.Ifnonnull || Opcode_Methods
                    [opcode] != method)
                throw new ArgumentException("Invalid opcode: " + opcode);
        }

        /// <summary>Checks that the given value is a signed byte.</summary>
        /// <param name="value">the value to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        private static void CheckSignedByte(int value, string message)
        {
            if (value < byte.MinValue || value > byte.MaxValue)
                throw new ArgumentException(message + " (must be a signed byte): " + value);
        }

        /// <summary>Checks that the given value is a signed short.</summary>
        /// <param name="value">the value to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        private static void CheckSignedShort(int value, string message)
        {
            if (value < short.MinValue || value > short.MaxValue)
                throw new ArgumentException(message + " (must be a signed short): " + value);
        }

        /// <summary>Checks that the given value is an unsigned short.</summary>
        /// <param name="value">the value to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        private static void CheckUnsignedShort(int value, string message)
        {
            if (value < 0 || value > 65535)
                throw new ArgumentException(message + " (must be an unsigned short): " + value);
        }

        /// <summary>
        ///     Checks that the given value is an
        ///     <see cref="int" />
        ///     ,
        ///     <see cref="float" />
        ///     ,
        ///     <see cref="long" />
        ///     ,
        ///     <see cref="double" />
        ///     or
        ///     <see cref="string" />
        ///     value.
        /// </summary>
        /// <param name="value">the value to be checked.</param>
        internal static void CheckConstant(object value)
        {
            if (!(value is int) && !(value is float) && !(value is long) && !(value is double
                    ) && !(value is string))
                throw new ArgumentException("Invalid constant: " + value);
        }

        /// <summary>Checks that the given value is a valid operand for the LDC instruction.</summary>
        /// <param name="value">the value to be checked.</param>
        private void CheckLdcConstant(object value)
        {
            if (value is Type)
            {
                var sort = ((Type) value).GetSort();
                if (sort != Type.Object && sort != Type.Array && sort != Type.Method)
                    throw new ArgumentException("Illegal LDC constant value");
                if (sort != Type.Method && (version & 0xFFFF) < OpcodesConstants
                        .V1_5)
                    throw new ArgumentException("ldc of a constant class requires at least version 1.5"
                    );
                if (sort == Type.Method && (version & 0xFFFF) < OpcodesConstants
                        .V1_7)
                    throw new ArgumentException("ldc of a method type requires at least version 1.7");
            }
            else if (value is Handle)
            {
                if ((version & 0xFFFF) < OpcodesConstants.V1_7)
                    throw new ArgumentException("ldc of a Handle requires at least version 1.7");
                var handle = (Handle) value;
                var tag = handle.GetTag();
                if (tag < OpcodesConstants.H_Getfield || tag > OpcodesConstants.H_Invokeinterface)
                    throw new ArgumentException("invalid handle tag " + tag);
                CheckInternalName(version, handle.GetOwner(), "handle owner");
                if (tag <= OpcodesConstants.H_Putstatic)
                    CheckDescriptor(version, handle.GetDesc(), false);
                else
                    CheckMethodDescriptor(version, handle.GetDesc());
                var handleName = handle.GetName();
                if (!("<init>".Equals(handleName) && tag == OpcodesConstants.H_Newinvokespecial))
                    CheckMethodIdentifier(version, handleName, "handle name");
            }
            else if (value is ConstantDynamic)
            {
                if ((version & 0xFFFF) < OpcodesConstants.V11)
                    throw new ArgumentException("ldc of a ConstantDynamic requires at least version 11"
                    );
                var constantDynamic = (ConstantDynamic) value;
                CheckMethodIdentifier(version, constantDynamic.GetName(), "constant dynamic name"
                );
                CheckDescriptor(version, constantDynamic.GetDescriptor(), false);
                CheckLdcConstant(constantDynamic.GetBootstrapMethod());
                var bootstrapMethodArgumentCount = constantDynamic.GetBootstrapMethodArgumentCount
                    ();
                for (var i = 0; i < bootstrapMethodArgumentCount; ++i)
                    CheckLdcConstant(constantDynamic.GetBootstrapMethodArgument(i));
            }
            else
            {
                CheckConstant(value);
            }
        }

        /// <summary>Checks that the given string is a valid unqualified name.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="name">the string to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        internal static void CheckUnqualifiedName(int version, string name, string message
        )
        {
            CheckIdentifier(version, name, 0, -1, message);
        }

        /// <summary>Checks that the given substring is a valid Java identifier.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="name">the string to be checked.</param>
        /// <param name="startPos">
        ///     the index of the first character of the identifier (inclusive).
        /// </param>
        /// <param name="endPos">
        ///     the index of the last character of the identifier (exclusive). -1 is equivalent
        ///     to
        ///     <c>name.length()</c>
        ///     if name is not
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="message">the message to use in case of error.</param>
        internal static void CheckIdentifier(int version, string name, int startPos, int
            endPos, string message)
        {
            if (name == null || (endPos == -1 ? name.Length <= startPos : endPos <= startPos))
                throw new ArgumentException(Invalid + message + Must_Not_Be_Null_Or_Empty);
            var max = endPos == -1 ? name.Length : endPos;
            if ((version & 0xFFFF) >= OpcodesConstants.V1_5)
                for (var i = startPos; i < max; i = name.OffsetByCodePoints(i, 1))
                    if (".;[/".IndexOf(name.CodePointAt(i)) != -1)
                        throw new ArgumentException(Invalid + message + " (must not contain . ; [ or /): "
                                                    + name);
            /*
            for (int i = startPos; i < max; i = name.OffsetByCodePoints(i, 1))
            {
                if (i == startPos ? !char.IsJavaIdentifierStart(name.CodePointAt(i)) : !char.IsJavaIdentifierPart
                    (name.CodePointAt(i)))
                {
                    throw new ArgumentException(Invalid + message + " (must be a valid Java identifier): "
                         + name);
                }
            }
        */
        }

        /// <summary>Checks that the given string is a valid Java identifier.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="name">the string to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        internal static void CheckMethodIdentifier(int version, string name, string message
        )
        {
            if (name == null || name.Length == 0)
                throw new ArgumentException(Invalid + message + Must_Not_Be_Null_Or_Empty);
            if ((version & 0xFFFF) >= OpcodesConstants.V1_5)
                for (var i = 0; i < name.Length; i = name.OffsetByCodePoints(i, 1))
                    if (".;[/<>".IndexOf(name.CodePointAt(i)) != -1)
                        throw new ArgumentException(Invalid + message + " (must be a valid unqualified name): "
                                                    + name);
            /*
            for (int i = 0; i < name.Length; i = name.OffsetByCodePoints(i, 1))
            {
                if (i == 0 ? !char.IsJavaIdentifierStart(name.CodePointAt(i)) : !char.IsJavaIdentifierPart
                    (name.CodePointAt(i)))
                {
                    throw new ArgumentException(Invalid + message + " (must be a '<init>', '<clinit>' or a valid Java identifier): "
                         + name);
                }
            }
        */
        }

        /// <summary>
        ///     Checks that the given string is a valid internal class name or array type descriptor.
        /// </summary>
        /// <param name="version">the class version.</param>
        /// <param name="name">the string to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        internal static void CheckInternalName(int version, string name, string message)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentException(Invalid + message + Must_Not_Be_Null_Or_Empty);
            if (name[0] == '[')
                CheckDescriptor(version, name, false);
            else
                CheckInternalClassName(version, name, message);
        }

        /// <summary>Checks that the given string is a valid internal class name.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="name">the string to be checked.</param>
        /// <param name="message">the message to use in case of error.</param>
        private static void CheckInternalClassName(int version, string name, string message
        )
        {
            try
            {
                var startIndex = 0;
                int slashIndex;
                while ((slashIndex = name.IndexOf('/', startIndex + 1)) != -1)
                {
                    CheckIdentifier(version, name, startIndex, slashIndex, null);
                    startIndex = slashIndex + 1;
                }

                CheckIdentifier(version, name, startIndex, name.Length, null);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(Invalid + message + " (must be an internal class name): "
                                            + name, e);
            }
        }

        /// <summary>Checks that the given string is a valid type descriptor.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="descriptor">the string to be checked.</param>
        /// <param name="canBeVoid">
        ///     <literal>true</literal>
        ///     if
        ///     <c>V</c>
        ///     can be considered valid.
        /// </param>
        internal static void CheckDescriptor(int version, string descriptor, bool canBeVoid
        )
        {
            var endPos = CheckDescriptor(version, descriptor, 0, canBeVoid);
            if (endPos != descriptor.Length) throw new ArgumentException(Invalid_Descriptor + descriptor);
        }

        /// <summary>Checks that a the given substring is a valid type descriptor.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="descriptor">the string to be checked.</param>
        /// <param name="startPos">
        ///     the index of the first character of the type descriptor (inclusive).
        /// </param>
        /// <param name="canBeVoid">
        ///     whether
        ///     <c>V</c>
        ///     can be considered valid.
        /// </param>
        /// <returns>the index of the last character of the type descriptor, plus one.</returns>
        private static int CheckDescriptor(int version, string descriptor, int startPos,
            bool canBeVoid)
        {
            if (descriptor == null || startPos >= descriptor.Length)
                throw new ArgumentException("Invalid type descriptor (must not be null or empty)"
                );
            switch (descriptor[startPos])
            {
                case 'V':
                {
                    if (canBeVoid)
                        return startPos + 1;
                    throw new ArgumentException(Invalid_Descriptor + descriptor);
                    goto case 'Z';
                }

                case 'Z':
                case 'C':
                case 'B':
                case 'S':
                case 'I':
                case 'F':
                case 'J':
                case 'D':
                {
                    return startPos + 1;
                }

                case '[':
                {
                    var pos = startPos + 1;
                    while (pos < descriptor.Length && descriptor[pos] == '[') ++pos;
                    if (pos < descriptor.Length)
                        return CheckDescriptor(version, descriptor, pos, false);
                    throw new ArgumentException(Invalid_Descriptor + descriptor);
                    goto case 'L';
                }

                case 'L':
                {
                    var endPos = descriptor.IndexOf(';', startPos);
                    if (startPos == -1 || endPos - startPos < 2)
                        throw new ArgumentException(Invalid_Descriptor + descriptor);
                    try
                    {
                        CheckInternalClassName(version, Runtime.Substring(descriptor, startPos +
                                                                                      1, endPos), null);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException(Invalid_Descriptor + descriptor, e);
                    }

                    return endPos + 1;
                }

                default:
                {
                    throw new ArgumentException(Invalid_Descriptor + descriptor);
                }
            }
        }

        /// <summary>Checks that the given string is a valid method descriptor.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="descriptor">the string to be checked.</param>
        internal static void CheckMethodDescriptor(int version, string descriptor)
        {
            if (descriptor == null || descriptor.Length == 0)
                throw new ArgumentException("Invalid method descriptor (must not be null or empty)"
                );
            if (descriptor[0] != '(' || descriptor.Length < 3)
                throw new ArgumentException(Invalid_Descriptor + descriptor);
            var pos = 1;
            if (descriptor[pos] != ')')
                do
                {
                    if (descriptor[pos] == 'V') throw new ArgumentException(Invalid_Descriptor + descriptor);
                    pos = CheckDescriptor(version, descriptor, pos, false);
                } while (pos < descriptor.Length && descriptor[pos] != ')');

            pos = CheckDescriptor(version, descriptor, pos + 1, true);
            if (pos != descriptor.Length) throw new ArgumentException(Invalid_Descriptor + descriptor);
        }

        /// <summary>Checks that the given label is not null.</summary>
        /// <remarks>
        ///     Checks that the given label is not null. This method can also check that the label has been
        ///     visited.
        /// </remarks>
        /// <param name="label">the label to be checked.</param>
        /// <param name="checkVisited">whether to check that the label has been visited.</param>
        /// <param name="message">the message to use in case of error.</param>
        private void CheckLabel(Label label, bool checkVisited, string message)
        {
            if (label == null) throw new ArgumentException(Invalid + message + " (must not be null)");
            if (checkVisited && labelInsnIndices.GetOrNullable(label) == null)
                throw new ArgumentException(Invalid + message + " (must be visited first)");
        }

        /// <summary>The 'generic' instruction visit methods (i.e.</summary>
        /// <remarks>
        ///     The 'generic' instruction visit methods (i.e. those that take an opcode argument).
        /// </remarks>
        [Serializable]
        private sealed class Method : EnumBase
        {
            public static readonly Method Visit_Insn = new Method
                (0, "VISIT_INSN");

            public static readonly Method Visit_Int_Insn = new Method
                (1, "VISIT_INT_INSN");

            public static readonly Method Visit_Var_Insn = new Method
                (2, "VISIT_VAR_INSN");

            public static readonly Method Visit_Type_Insn = new Method
                (3, "VISIT_TYPE_INSN");

            public static readonly Method Visit_Field_Insn = new Method
                (4, "VISIT_FIELD_INSN");

            public static readonly Method Visit_Method_Insn = new Method
                (5, "VISIT_METHOD_INSN");

            public static readonly Method Visit_Jump_Insn = new Method
                (6, "VISIT_JUMP_INSN");

            static Method()
            {
                RegisterValues<Method>(Values());
            }

            private Method(int ordinal, string name)
                : base(ordinal, name)
            {
            }

            public static Method[] Values()
            {
                return new[]
                {
                    Visit_Insn, Visit_Int_Insn, Visit_Var_Insn, Visit_Type_Insn, Visit_Field_Insn, Visit_Method_Insn,
                    Visit_Jump_Insn
                };
            }
        }

        private sealed class _MethodNode_445 : MethodNode
        {
            private readonly MethodVisitor methodVisitor;

            public _MethodNode_445(MethodVisitor methodVisitor, VisitorAsmApiVersion baseArg1, int baseArg2, string
                baseArg3, string baseArg4, string baseArg5, string[] baseArg6)
                : base(baseArg1, baseArg2, baseArg3, baseArg4, baseArg5, baseArg6)
            {
                this.methodVisitor = methodVisitor;
            }

            public override void VisitEnd()
            {
                var analyzer = new Analyzer<BasicValue>(new BasicVerifier());
                try
                {
                    analyzer.Analyze("dummy", this);
                }
                catch (IndexOutOfRangeException e)
                {
                    if (maxLocals == 0 && maxStack == 0)
                        throw new ArgumentException(
                            "Data flow checking option requires valid, non zero maxLocals and maxStack."
                            , e);
                    ThrowError(analyzer, e);
                }
                catch (AnalyzerException e)
                {
                    ThrowError(analyzer, e);
                }

                if (methodVisitor != null) Accept(methodVisitor);
            }

            private void ThrowError(Analyzer<BasicValue> analyzer, Exception e)
            {
                var stringWriter = new StringWriter();
                var printWriter = Console.Error;
                CheckClassAdapter.PrintAnalyzerResult(this, analyzer, printWriter);
                printWriter.Close();
                throw new ArgumentException(e.Message + ' ' + stringWriter, e);
            }
        }
    }
}