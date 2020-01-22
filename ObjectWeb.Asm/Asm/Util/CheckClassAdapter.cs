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
using System.Text;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Asm.Tree;
using ObjectWeb.Asm.Tree.Analysis;
using ObjectWeb.Misc.Java.Lang;
using ObjectWeb.Misc.Java.Nio;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="ClassVisitor" />
    ///     that checks that its methods are properly used. More precisely this class
    ///     adapter checks each method call individually, based <i>only</i> on its arguments, but does
    ///     <i>not</i> check the <i>sequence</i> of method calls. For example, the invalid sequence
    ///     <c>visitField(ACC_PUBLIC, "i", "I", null)</c>
    ///     <c>visitField(ACC_PUBLIC, "i", "D", null)</c>
    ///     will
    ///     <i>not</i> be detected by this class adapter.
    ///     <p>
    ///         <code>CheckClassAdapter</code> can be also used to verify bytecode transformations in order to
    ///         make sure that the transformed bytecode is sane. For example:
    ///         <pre>
    ///             InputStream inputStream = ...; // get bytes for the source class
    ///             ClassReader classReader = new ClassReader(inputStream);
    ///             ClassWriter classWriter = new ClassWriter(classReader, ClassWriter.COMPUTE_MAXS);
    ///             ClassVisitor classVisitor = new <b>MyClassAdapter</b>(new CheckClassAdapter(classWriter, true));
    ///             classReader.accept(classVisitor, 0);
    ///             StringWriter stringWriter = new StringWriter();
    ///             PrintWriter printWriter = new PrintWriter(stringWriter);
    ///             CheckClassAdapter.verify(new ClassReader(classWriter.toByteArray()), false, printWriter);
    ///             assertTrue(stringWriter.toString().isEmpty());
    ///         </pre>
    ///         <p>
    ///             The above code pass the transformed bytecode through a <code>CheckClassAdapter</code>, with
    ///             data flow checks enabled. These checks are not exactly the same as the JVM verification, but
    ///             provide some basic type checking for each method instruction. If the bytecode has errors, the
    ///             output text shows the erroneous instruction number, and a dump of the failed method with
    ///             information about the type of the local variables and of the operand stack slots for each
    ///             instruction. For example (format is - insnNumber locals : stack):
    ///             <pre>
    ///                 org.objectweb.asm.tree.analysis.AnalyzerException: Error at instruction 71: Expected I, but found .
    ///                 at org.objectweb.asm.tree.analysis.Analyzer.analyze(Analyzer.java:...)
    ///                 at org.objectweb.asm.util.CheckClassAdapter.verify(CheckClassAdapter.java:...)
    ///                 ...
    ///                 remove()V
    ///                 00000 LinkedBlockingQueue$Itr . . . . . . . .  : ICONST_0
    ///                 00001 LinkedBlockingQueue$Itr . . . . . . . .  : I ISTORE 2
    ///                 00001 LinkedBlockingQueue$Itr <b>.</b> I . . . . . .  :
    ///                 ...
    ///                 00071 LinkedBlockingQueue$Itr <b>.</b> I . . . . . .  : ILOAD 1
    ///                 00072 <b>?</b> INVOKESPECIAL java/lang/Integer.&lt;init&gt; (I)V
    ///                 ...
    ///             </pre>
    ///             <p>
    ///                 The above output shows that the local variable 1, loaded by the <code>ILOAD 1</code>
    ///                 instruction at position <code>00071</code> is not initialized, whereas the local variable 2 is
    ///                 initialized and contains an int value.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class CheckClassAdapter : ClassVisitor
    {
        /// <summary>The help message shown when command line arguments are incorrect.</summary>
        private const string Usage = "Verifies the given class.\n" +
                                     "Usage: CheckClassAdapter <fully qualified class name or class file name>";

        private const string Error_At = ": error at index ";

        /// <summary>Whether the bytecode must be checked with a BasicVerifier.</summary>
        private readonly bool checkDataFlow;

        /// <summary>The index of the instruction designated by each visited label so far.</summary>
        private readonly IDictionary<Label, int> labelInsnIndices;

        /// <summary>The common package of all the nest members.</summary>
        /// <remarks>
        ///     The common package of all the nest members. Not
        ///     <literal>null</literal>
        ///     if the visitNestMember method
        ///     has been called.
        /// </remarks>
        private string nestMemberPackageName;

        /// <summary>The class version number.</summary>
        private int version;

        /// <summary>
        ///     Whether the
        ///     <see cref="Visit" />
        ///     method has been called.
        /// </summary>
        private bool visitCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitEnd()" />
        ///     method has been called.
        /// </summary>
        private bool visitEndCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitModule" />
        ///     method has been called.
        /// </summary>
        private bool visitModuleCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitNestHost(string)" />
        ///     method has been called.
        /// </summary>
        private bool visitNestHostCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitOuterClass(string, string, string)" />
        ///     method has been called.
        /// </summary>
        private bool visitOuterClassCalled;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitSource(string, string)" />
        ///     method has been called.
        /// </summary>
        private bool visitSourceCalled;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckClassAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="CheckClassAdapter(int,ObjectWeb.Asm.ClassVisitor,bool)" />
        ///     version.
        /// </summary>
        /// <param name="classVisitor">
        ///     the class visitor to which this adapter must delegate calls.
        /// </param>
        public CheckClassAdapter(ClassVisitor classVisitor)
            : this(classVisitor, true)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckClassAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="CheckClassAdapter(int, Org.Objectweb.Asm.ClassVisitor, bool)" />
        ///     version.
        /// </summary>
        /// <param name="classVisitor">
        ///     the class visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="checkDataFlow">
        ///     whether to perform basic data flow checks. This option requires valid
        ///     maxLocals and maxStack values.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public CheckClassAdapter(ClassVisitor classVisitor, bool checkDataFlow)
            : this(VisitorAsmApiVersion.Asm7, classVisitor, checkDataFlow)
        {
            // -----------------------------------------------------------------------------------------------
            // Constructors
            // -----------------------------------------------------------------------------------------------
            /* latest api = */
            if (GetType() != typeof(CheckClassAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckClassAdapter" />
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
        /// <param name="classVisitor">
        ///     the class visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="checkDataFlow">
        ///     <literal>true</literal>
        ///     to perform basic data flow checks, or
        ///     <literal>false</literal>
        ///     to
        ///     not perform any data flow check (see
        ///     <see cref="CheckMethodAdapter" />
        ///     ). This option requires
        ///     valid maxLocals and maxStack values.
        /// </param>
        protected internal CheckClassAdapter(VisitorAsmApiVersion api, ClassVisitor classVisitor, bool checkDataFlow
        )
            : base(api, classVisitor)
        {
            labelInsnIndices = new Dictionary<Label, int>();
            this.checkDataFlow = checkDataFlow;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the ClassVisitor interface
        // -----------------------------------------------------------------------------------------------
        public override void Visit(int version, AccessFlags access, string name, string signature
            , string superName, string[] interfaces)
        {
            if (visitCalled) throw new InvalidOperationException("visit must be called only once");
            visitCalled = true;
            CheckState();
            CheckAccess(access, AccessFlags.Public | AccessFlags.Final | AccessFlags
                                    .Super | AccessFlags.Interface | AccessFlags.Abstract |
                                AccessFlags
                                    .Synthetic | AccessFlags.Annotation | AccessFlags.Enum |
                                AccessFlags
                                    .Deprecated | AccessFlags.Module);
            if (name == null) throw new ArgumentException("Illegal class name (null)");
            if (!name.EndsWith("package-info") && !name.EndsWith("module-info"))
                CheckMethodAdapter.CheckInternalName(version, name, "class name");
            if ("java/lang/Object".Equals(name))
            {
                if (superName != null)
                    throw new ArgumentException("The super class name of the Object class must be 'null'"
                    );
            }
            else if (name.EndsWith("module-info"))
            {
                if (superName != null)
                    throw new ArgumentException("The super class name of a module-info class must be 'null'"
                    );
            }
            else
            {
                CheckMethodAdapter.CheckInternalName(version, superName, "super class name");
            }

            if (signature != null) CheckClassSignature(signature);
            if (access.HasFlagFast(AccessFlags.Interface) && !"java/lang/Object".Equals(
                    superName))
                throw new ArgumentException("The super class name of interfaces must be 'java/lang/Object'"
                );
            if (interfaces != null)
                for (var i = 0; i < interfaces.Length; ++i)
                    CheckMethodAdapter.CheckInternalName(version, interfaces[i], "interface name at index "
                                                                                 + i);
            this.version = version;
            base.Visit(version, access, name, signature, superName, interfaces);
        }

        public override void VisitSource(string file, string debug)
        {
            CheckState();
            if (visitSourceCalled) throw new InvalidOperationException("visitSource can be called only once.");
            visitSourceCalled = true;
            base.VisitSource(file, debug);
        }

        public override ModuleVisitor VisitModule(string name, AccessFlags access, string version)
        {
            CheckState();
            if (visitModuleCalled) throw new InvalidOperationException("visitModule can be called only once.");
            visitModuleCalled = true;
            CheckFullyQualifiedName(this.version, name, "module name");
            CheckAccess(access, AccessFlags.Open | AccessFlags.Synthetic |
                                AccessFlags.Mandated);
            var checkModuleAdapter = new CheckModuleAdapter(api, base.VisitModule
                    (name, access, version), access.HasFlagFast(AccessFlags.Open));
            checkModuleAdapter.classVersion = this.version;
            return checkModuleAdapter;
        }

        public override void VisitNestHost(string nestHost)
        {
            CheckState();
            CheckMethodAdapter.CheckInternalName(version, nestHost, "nestHost");
            if (visitNestHostCalled) throw new InvalidOperationException("visitNestHost can be called only once.");
            if (nestMemberPackageName != null)
                throw new InvalidOperationException("visitNestHost and visitNestMember are mutually exclusive."
                );
            visitNestHostCalled = true;
            base.VisitNestHost(nestHost);
        }

        public override void VisitNestMember(string nestMember)
        {
            CheckState();
            CheckMethodAdapter.CheckInternalName(version, nestMember, "nestMember");
            if (visitNestHostCalled)
                throw new InvalidOperationException("visitMemberOfNest and visitNestHost are mutually exclusive."
                );
            var packageName = PackageName(nestMember);
            if (nestMemberPackageName == null)
                nestMemberPackageName = packageName;
            else if (!nestMemberPackageName.Equals(packageName))
                throw new InvalidOperationException("nest member " + nestMember + " should be in the package "
                                                    + nestMemberPackageName);
            base.VisitNestMember(nestMember);
        }

        public override void VisitPermittedSubtypeExperimental(string permittedSubtype)
        {
            CheckState();
            CheckMethodAdapter.CheckInternalName(version, permittedSubtype, "permittedSubtype"
            );
            base.VisitPermittedSubtypeExperimental(permittedSubtype);
        }

        public override void VisitOuterClass(string owner, string name, string descriptor
        )
        {
            CheckState();
            if (visitOuterClassCalled) throw new InvalidOperationException("visitOuterClass can be called only once.");
            visitOuterClassCalled = true;
            if (owner == null) throw new ArgumentException("Illegal outer class owner");
            if (descriptor != null) CheckMethodAdapter.CheckMethodDescriptor(version, descriptor);
            base.VisitOuterClass(owner, name, descriptor);
        }

        public override void VisitInnerClass(string name, string outerName, string innerName
            , AccessFlags access)
        {
            CheckState();
            CheckMethodAdapter.CheckInternalName(version, name, "class name");
            if (outerName != null) CheckMethodAdapter.CheckInternalName(version, outerName, "outer class name");
            if (innerName != null)
            {
                var startIndex = 0;
                while (startIndex < innerName.Length && char.IsDigit(innerName[startIndex])) startIndex++;
                if (startIndex == 0 || startIndex < innerName.Length)
                    CheckMethodAdapter.CheckIdentifier(version, innerName, startIndex, -1, "inner class name"
                    );
            }

            CheckAccess(access, AccessFlags.Public | AccessFlags.Private |
                                AccessFlags.Protected | AccessFlags.Static |
                                AccessFlags.Final | AccessFlags.Interface |
                                AccessFlags.Abstract | AccessFlags
                                    .Synthetic | AccessFlags.Annotation | AccessFlags.Enum);
            base.VisitInnerClass(name, outerName, innerName, access);
        }

        public override RecordComponentVisitor VisitRecordComponentExperimental(AccessFlags access
            , string name, string descriptor, string signature)
        {
            CheckState();
            CheckAccess(access, AccessFlags.Deprecated);
            CheckMethodAdapter.CheckUnqualifiedName(version, name, "record component name");
            CheckMethodAdapter.CheckDescriptor(version, descriptor, false);
            /* canBeVoid = */
            if (signature != null) CheckFieldSignature(signature);
            return new CheckRecordComponentAdapter(api, base.VisitRecordComponentExperimental
                (access, name, descriptor, signature));
        }

        public override FieldVisitor VisitField(AccessFlags access, string name, string descriptor
            , string signature, object value)
        {
            CheckState();
            CheckAccess(access, AccessFlags.Public | AccessFlags.Private |
                                AccessFlags.Protected | AccessFlags.Static |
                                AccessFlags.Final | AccessFlags.Volatile |
                                AccessFlags.Transient | AccessFlags
                                    .Synthetic | AccessFlags.Enum | AccessFlags.Mandated |
                                AccessFlags
                                    .Deprecated);
            CheckMethodAdapter.CheckUnqualifiedName(version, name, "field name");
            CheckMethodAdapter.CheckDescriptor(version, descriptor, false);
            /* canBeVoid = */
            if (signature != null) CheckFieldSignature(signature);
            if (value != null) CheckMethodAdapter.CheckConstant(value);
            return new CheckFieldAdapter(api, base.VisitField(access, name, descriptor, signature
                , value));
        }

        public override MethodVisitor VisitMethod(AccessFlags access, string name, string descriptor
            , string signature, string[] exceptions)
        {
            CheckState();
            CheckAccess(access, AccessFlags.Public | AccessFlags.Private |
                                AccessFlags.Protected | AccessFlags.Static |
                                AccessFlags.Final | AccessFlags.Synchronized |
                                AccessFlags.Bridge | AccessFlags
                                    .Varargs | AccessFlags.Native | AccessFlags.Abstract |
                                AccessFlags
                                    .Strict | AccessFlags.Synthetic | AccessFlags.Mandated |
                                AccessFlags
                                    .Deprecated);
            if (!"<init>".Equals(name) && !"<clinit>".Equals(name))
                CheckMethodAdapter.CheckMethodIdentifier(version, name, "method name");
            CheckMethodAdapter.CheckMethodDescriptor(version, descriptor);
            if (signature != null) CheckMethodSignature(signature);
            if (exceptions != null)
                for (var i = 0; i < exceptions.Length; ++i)
                    CheckMethodAdapter.CheckInternalName(version, exceptions[i], "exception name at index "
                                                                                 + i);
            CheckMethodAdapter checkMethodAdapter;
            if (checkDataFlow)
                checkMethodAdapter = new CheckMethodAdapter(api, access, name, descriptor, base.VisitMethod
                    (access, name, descriptor, signature, exceptions), labelInsnIndices);
            else
                checkMethodAdapter = new CheckMethodAdapter(api, base.VisitMethod(access, name, descriptor
                    , signature, exceptions), labelInsnIndices);
            checkMethodAdapter.version = version;
            return checkMethodAdapter;
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            CheckState();
            CheckMethodAdapter.CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitAnnotation(descriptor, visible));
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            CheckState();
            var sort = new TypeReference(typeRef).GetSort();
            if (sort != TypeReference.Class_Type_Parameter && sort != TypeReference.Class_Type_Parameter_Bound
                                                           && sort != TypeReference.Class_Extends)
                throw new ArgumentException("Invalid type reference sort 0x" + sort.ToString("x8"));
            CheckTypeRef(typeRef);
            CheckMethodAdapter.CheckDescriptor(version, descriptor, false);
            return new CheckAnnotationAdapter(base.VisitTypeAnnotation(typeRef, typePath, descriptor
                , visible));
        }

        public override void VisitAttribute(Attribute attribute)
        {
            CheckState();
            if (attribute == null) throw new ArgumentException("Invalid attribute (must not be null)");
            base.VisitAttribute(attribute);
        }

        public override void VisitEnd()
        {
            CheckState();
            visitEndCalled = true;
            base.VisitEnd();
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Checks that the visit method has been called and that visitEnd has not been called.
        /// </summary>
        private void CheckState()
        {
            if (!visitCalled)
                throw new InvalidOperationException("Cannot visit member before visit has been called."
                );
            if (visitEndCalled)
                throw new InvalidOperationException("Cannot visit member after visitEnd has been called."
                );
        }

        /// <summary>Checks that the given access flags do not contain invalid flags.</summary>
        /// <remarks>
        ///     Checks that the given access flags do not contain invalid flags. This method also checks that
        ///     mutually incompatible flags are not set simultaneously.
        /// </remarks>
        /// <param name="access">the access flags to be checked.</param>
        /// <param name="possibleAccess">the valid access flags.</param>
        internal static void CheckAccess(AccessFlags access, AccessFlags possibleAccess)
        {
            if ((access & ~possibleAccess) != 0) throw new ArgumentException("Invalid access flags: " + access);
            var publicProtectedPrivate = AccessFlags.Public | AccessFlags.Protected
                                                                     | AccessFlags.Private;
            if (Runtime.BitCount((long) (access & publicProtectedPrivate)) > 1)
                throw new ArgumentException("public, protected and private are mutually exclusive: "
                                            + access);
            if (Runtime.BitCount((long) (access & (AccessFlags.Final | AccessFlags.Abstract
                                         ))) > 1)
                throw new ArgumentException("final and abstract are mutually exclusive: " + access
                );
        }

        /// <summary>Checks that the given name is a fully qualified name, using dots.</summary>
        /// <param name="version">the class version.</param>
        /// <param name="name">the name to be checked.</param>
        /// <param name="source">the source of 'name' (e.g 'module' for a module name).</param>
        internal static void CheckFullyQualifiedName(int version, string name, string source
        )
        {
            try
            {
                var startIndex = 0;
                int dotIndex;
                while ((dotIndex = name.IndexOf('.', startIndex + 1)) != -1)
                {
                    CheckMethodAdapter.CheckIdentifier(version, name, startIndex, dotIndex, null);
                    startIndex = dotIndex + 1;
                }

                CheckMethodAdapter.CheckIdentifier(version, name, startIndex, name.Length, null);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Invalid " + source + " (must be a fully qualified name): "
                                            + name, e);
            }
        }

        /// <summary>Checks a class signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        public static void CheckClassSignature(string signature)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // ClassSignature:
            //   [TypeParameters] SuperclassSignature SuperinterfaceSignature*
            // SuperclassSignature:
            //   ClassTypeSignature
            // SuperinterfaceSignature:
            //   ClassTypeSignature
            var pos = 0;
            if (GetChar(signature, 0) == '<') pos = CheckTypeParameters(signature, pos);
            pos = CheckClassTypeSignature(signature, pos);
            while (GetChar(signature, pos) == 'L') pos = CheckClassTypeSignature(signature, pos);
            if (pos != signature.Length) throw new ArgumentException(signature + Error_At + pos);
        }

        /// <summary>Checks a method signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        public static void CheckMethodSignature(string signature)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // MethodSignature:
            //   [TypeParameters] ( JavaTypeSignature* ) Result ThrowsSignature*
            // Result:
            //   JavaTypeSignature
            //   VoidDescriptor
            // ThrowsSignature:
            //   ^ ClassTypeSignature
            //   ^ TypeVariableSignature
            var pos = 0;
            if (GetChar(signature, 0) == '<') pos = CheckTypeParameters(signature, pos);
            pos = CheckChar('(', signature, pos);
            while ("ZCBSIFJDL[T".IndexOf(GetChar(signature, pos)) != -1) pos = CheckJavaTypeSignature(signature, pos);
            pos = CheckChar(')', signature, pos);
            if (GetChar(signature, pos) == 'V')
                ++pos;
            else
                pos = CheckJavaTypeSignature(signature, pos);
            while (GetChar(signature, pos) == '^')
            {
                ++pos;
                if (GetChar(signature, pos) == 'L')
                    pos = CheckClassTypeSignature(signature, pos);
                else
                    pos = CheckTypeVariableSignature(signature, pos);
            }

            if (pos != signature.Length) throw new ArgumentException(signature + Error_At + pos);
        }

        /// <summary>Checks a field signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        public static void CheckFieldSignature(string signature)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // FieldSignature:
            //   ReferenceTypeSignature
            var pos = CheckReferenceTypeSignature(signature, 0);
            if (pos != signature.Length) throw new ArgumentException(signature + Error_At + pos);
        }

        /// <summary>Checks the type parameters of a class or method signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckTypeParameters(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // TypeParameters:
            //   < TypeParameter TypeParameter* >
            var pos = startPos;
            pos = CheckChar('<', signature, pos);
            pos = CheckTypeParameter(signature, pos);
            while (GetChar(signature, pos) != '>') pos = CheckTypeParameter(signature, pos);
            return pos + 1;
        }

        /// <summary>Checks a type parameter of a class or method signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckTypeParameter(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // TypeParameter:
            //   Identifier ClassBound InterfaceBound*
            // ClassBound:
            //   : [ReferenceTypeSignature]
            // InterfaceBound:
            //   : ReferenceTypeSignature
            var pos = startPos;
            pos = CheckSignatureIdentifier(signature, pos);
            pos = CheckChar(':', signature, pos);
            if ("L[T".IndexOf(GetChar(signature, pos)) != -1) pos = CheckReferenceTypeSignature(signature, pos);
            while (GetChar(signature, pos) == ':') pos = CheckReferenceTypeSignature(signature, pos + 1);
            return pos;
        }

        /// <summary>Checks a reference type signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="pos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckReferenceTypeSignature(string signature, int pos)
        {
            switch (GetChar(signature, pos))
            {
                case 'L':
                {
                    // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
                    // ReferenceTypeSignature:
                    //   ClassTypeSignature
                    //   TypeVariableSignature
                    //   ArrayTypeSignature
                    // ArrayTypeSignature:
                    //   [ JavaTypeSignature
                    return CheckClassTypeSignature(signature, pos);
                }

                case '[':
                {
                    return CheckJavaTypeSignature(signature, pos + 1);
                }

                default:
                {
                    return CheckTypeVariableSignature(signature, pos);
                }
            }
        }

        /// <summary>Checks a class type signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckClassTypeSignature(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // ClassTypeSignature:
            //   L [PackageSpecifier] SimpleClassTypeSignature ClassTypeSignatureSuffix* ;
            // PackageSpecifier:
            //   Identifier / PackageSpecifier*
            // SimpleClassTypeSignature:
            //   Identifier [TypeArguments]
            // ClassTypeSignatureSuffix:
            //   . SimpleClassTypeSignature
            var pos = startPos;
            pos = CheckChar('L', signature, pos);
            pos = CheckSignatureIdentifier(signature, pos);
            while (GetChar(signature, pos) == '/') pos = CheckSignatureIdentifier(signature, pos + 1);
            if (GetChar(signature, pos) == '<') pos = CheckTypeArguments(signature, pos);
            while (GetChar(signature, pos) == '.')
            {
                pos = CheckSignatureIdentifier(signature, pos + 1);
                if (GetChar(signature, pos) == '<') pos = CheckTypeArguments(signature, pos);
            }

            return CheckChar(';', signature, pos);
        }

        /// <summary>Checks the type arguments in a class type signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckTypeArguments(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // TypeArguments:
            //   < TypeArgument TypeArgument* >
            var pos = startPos;
            pos = CheckChar('<', signature, pos);
            pos = CheckTypeArgument(signature, pos);
            while (GetChar(signature, pos) != '>') pos = CheckTypeArgument(signature, pos);
            return pos + 1;
        }

        /// <summary>Checks a type argument in a class type signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckTypeArgument(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // TypeArgument:
            //   [WildcardIndicator] ReferenceTypeSignature
            //   *
            // WildcardIndicator:
            //   +
            //   -
            var pos = startPos;
            var c = GetChar(signature, pos);
            if (c == '*')
                return pos + 1;
            if (c == '+' || c == '-') pos++;
            return CheckReferenceTypeSignature(signature, pos);
        }

        /// <summary>Checks a type variable signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckTypeVariableSignature(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // TypeVariableSignature:
            //  T Identifier ;
            var pos = startPos;
            pos = CheckChar('T', signature, pos);
            pos = CheckSignatureIdentifier(signature, pos);
            return CheckChar(';', signature, pos);
        }

        /// <summary>Checks a Java type signature.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckJavaTypeSignature(string signature, int startPos)
        {
            // From https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1:
            // JavaTypeSignature:
            //   ReferenceTypeSignature
            //   BaseType
            // BaseType:
            //   (one of)
            //   B C D F I J S Z
            var pos = startPos;
            switch (GetChar(signature, pos))
            {
                case 'B':
                case 'C':
                case 'D':
                case 'F':
                case 'I':
                case 'J':
                case 'S':
                case 'Z':
                {
                    return pos + 1;
                }

                default:
                {
                    return CheckReferenceTypeSignature(signature, pos);
                }
            }
        }

        /// <summary>Checks an identifier.</summary>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="startPos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckSignatureIdentifier(string signature, int startPos)
        {
            var pos = startPos;
            while (pos < signature.Length && ".;[/<>:".IndexOf(signature[pos]) ==
                   -1)
                pos = signature[pos] + 1;
            if (pos == startPos)
                throw new ArgumentException(signature + ": identifier expected at index " + startPos
                );
            return pos;
        }

        /// <summary>Checks a single character.</summary>
        /// <param name="c">a character.</param>
        /// <param name="signature">a string containing the signature that must be checked.</param>
        /// <param name="pos">index of first character to be checked.</param>
        /// <returns>the index of the first character after the checked part.</returns>
        private static int CheckChar(char c, string signature, int pos)
        {
            if (GetChar(signature, pos) == c) return pos + 1;
            throw new ArgumentException(signature + ": '" + c + "' expected at index " + pos);
        }

        /// <summary>Returns the string character at the given index, or 0.</summary>
        /// <param name="string">a string.</param>
        /// <param name="pos">an index in 'string'.</param>
        /// <returns>the character at the given index, or 0 if there is no such character.</returns>
        private static char GetChar(string @string, int pos)
        {
            return pos < @string.Length ? @string[pos] : (char) 0;
        }

        /// <summary>Checks the reference to a type in a type annotation.</summary>
        /// <param name="typeRef">a reference to an annotated type.</param>
        internal static void CheckTypeRef(int typeRef)
        {
            var mask = 0;
            switch ((int) ((uint) typeRef >> 24))
            {
                case TypeReference.Class_Type_Parameter:
                case TypeReference.Method_Type_Parameter:
                case TypeReference.Method_Formal_Parameter:
                {
                    mask = unchecked((int) 0xFFFF0000);
                    break;
                }

                case TypeReference.Field:
                case TypeReference.Method_Return:
                case TypeReference.Method_Receiver:
                case TypeReference.Local_Variable:
                case TypeReference.Resource_Variable:
                case TypeReference.Instanceof:
                case TypeReference.New:
                case TypeReference.Constructor_Reference:
                case TypeReference.Method_Reference:
                {
                    mask = unchecked((int) 0xFF000000);
                    break;
                }

                case TypeReference.Class_Extends:
                case TypeReference.Class_Type_Parameter_Bound:
                case TypeReference.Method_Type_Parameter_Bound:
                case TypeReference.Throws:
                case TypeReference.Exception_Parameter:
                {
                    mask = unchecked((int) 0xFFFFFF00);
                    break;
                }

                case TypeReference.Cast:
                case TypeReference.Constructor_Invocation_Type_Argument:
                case TypeReference.Method_Invocation_Type_Argument:
                case TypeReference.Constructor_Reference_Type_Argument:
                case TypeReference.Method_Reference_Type_Argument:
                {
                    mask = unchecked((int) 0xFF0000FF);
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }

            if ((typeRef & ~mask) != 0)
                throw new ArgumentException("Invalid type reference 0x" + typeRef.ToString("x8"));
        }

        /// <summary>Returns the package name of an internal name.</summary>
        /// <param name="name">an internal name.</param>
        /// <returns>the package name or "" if there is no package.</returns>
        private static string PackageName(string name)
        {
            var index = name.LastIndexOf('/');
            if (index == -1) return string.Empty;
            return Runtime.Substring(name, 0, index);
        }

        // -----------------------------------------------------------------------------------------------
        // Static verification methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Checks the given class.</summary>
        /// <remarks>
        ///     Checks the given class.
        ///     <p>Usage: CheckClassAdapter &lt;binary class name or class file name&gt;
        /// </remarks>
        /// <param name="args">the command line arguments.</param>
        /// <exception cref="System.IO.IOException">
        ///     if the class cannot be found, or if an IO exception occurs.
        /// </exception>
        public static void Main(string[] args)
        {
            Main(args, Console.Error);
        }

        /// <summary>Checks the given class.</summary>
        /// <param name="args">the command line arguments.</param>
        /// <param name="logger">where to log errors.</param>
        /// <exception cref="System.IO.IOException">
        ///     if the class cannot be found, or if an IO exception occurs.
        /// </exception>
        internal static void Main(string[] args, TextWriter logger)
        {
            if (args.Length != 1)
            {
                logger.WriteLine(Usage);
                return;
            }

            ClassReader classReader = null;
            if (args[0].EndsWith(".class"))
            {
                var inputStream = new MemoryStream(File.ReadAllBytes(args[0])).ToInputStream();
                // NOPMD(AvoidFileStream): can't fix for 1.5 compatibility
                classReader = new ClassReader(inputStream);
            }

            Verify(classReader, false, logger);
        }

        /// <summary>Checks the given class.</summary>
        /// <param name="classReader">the class to be checked.</param>
        /// <param name="printResults">
        ///     whether to print the results of the bytecode verification.
        /// </param>
        /// <param name="printWriter">
        ///     where the results (or the stack trace in case of error) must be printed.
        /// </param>
        public static void Verify(ClassReader classReader, bool printResults, TextWriter
            printWriter)
        {
            Verify(classReader, null, printResults, printWriter);
        }

        /// <summary>Checks the given class.</summary>
        /// <param name="classReader">the class to be checked.</param>
        /// <param name="loader">
        ///     a <code>ClassLoader</code> which will be used to load referenced classes. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printResults">
        ///     whether to print the results of the bytecode verification.
        /// </param>
        /// <param name="printWriter">
        ///     where the results (or the stack trace in case of error) must be printed.
        /// </param>
        public static void Verify(ClassReader classReader, AppDomain loader, bool printResults
            , TextWriter printWriter)
        {
            var classNode = new ClassNode();
            classReader.Accept(new _CheckClassAdapter_1036(VisitorAsmApiVersion.Asm8Experimental
                , classNode, false), ParsingOptions.SkipDebug);
            var syperType = classNode.superName == null
                ? null
                : Type.GetObjectType(classNode
                    .superName);
            IList<MethodNode> methods = classNode.methods;
            IList<Type> interfaces = new List<Type>();
            foreach (var interfaceName in classNode.interfaces) interfaces.Add(Type.GetObjectType(interfaceName));
            foreach (var method in methods)
            {
                var verifier = new SimpleVerifier(Type.GetObjectType(classNode.name),
                    syperType, interfaces, classNode.access.HasFlagFast(AccessFlags.Interface));
                var analyzer = new Analyzer<BasicValue>(verifier);
                if (loader != null) verifier.SetClassLoader(loader);
                try
                {
                    analyzer.Analyze(classNode.name, method);
                }
                catch (AnalyzerException e)
                {
                    printWriter.WriteLine(e);
                }

                if (printResults) PrintAnalyzerResult(method, analyzer, printWriter);
            }

            printWriter.Flush();
        }

        internal static void PrintAnalyzerResult(MethodNode method, Analyzer<BasicValue>
            analyzer, TextWriter printWriter)
        {
            var textifier = new Textifier();
            var traceMethodVisitor = new TraceMethodVisitor(textifier);
            printWriter.WriteLine(method.name + method.desc);
            for (var i = 0; i < method.instructions.Size(); ++i)
            {
                method.instructions.Get(i).Accept(traceMethodVisitor);
                var stringBuilder = new StringBuilder();
                var frame = analyzer.GetFrames()[i];
                if (frame == null)
                {
                    stringBuilder.Append('?');
                }
                else
                {
                    for (var j = 0; j < frame.GetLocals(); ++j)
                        stringBuilder.Append(GetUnqualifiedName(frame.GetLocal(j).ToString())).Append(' '
                        );
                    stringBuilder.Append(" : ");
                    for (var j = 0; j < frame.GetStackSize(); ++j)
                        stringBuilder.Append(GetUnqualifiedName(frame.GetStack(j).ToString())).Append(' '
                        );
                }

                while (stringBuilder.Length < method.maxStack + method.maxLocals + 1) stringBuilder.Append(' ');
                printWriter.Write(Runtime.Substring((i + 100000).ToString(), 1));
                printWriter.Write(" " + stringBuilder + " : " + textifier.text[textifier.text.Count
                                                                               - 1]);
            }

            foreach (var tryCatchBlock in method.tryCatchBlocks)
            {
                tryCatchBlock.Accept(traceMethodVisitor);
                printWriter.Write(" " + textifier.text[textifier.text.Count - 1]);
            }

            printWriter.WriteLine();
        }

        private static string GetUnqualifiedName(string name)
        {
            var lastSlashIndex = name.LastIndexOf('/');
            if (lastSlashIndex == -1) return name;

            var endIndex = name.Length;
            if (name[endIndex - 1] == ';') endIndex--;
            return Runtime.Substring(name, lastSlashIndex + 1, endIndex);
        }

        private sealed class _CheckClassAdapter_1036 : CheckClassAdapter
        {
            public _CheckClassAdapter_1036(VisitorAsmApiVersion baseArg1, ClassVisitor baseArg2, bool baseArg3
            )
                : base(baseArg1, baseArg2, baseArg3)
            {
            }

            public override ModuleVisitor VisitModule(string name, AccessFlags access, string version)
            {
                return base.VisitModule(name, access, version);
            }
        }
    }
}