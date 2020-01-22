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

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="Printer" />
    ///     that prints the ASM code to generate the classes if visits.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class ASMifier : Printer
    {
        /// <summary>The help message shown when command line arguments are incorrect.</summary>
        private const string Usage = "Prints the ASM code to generate the given class.\n"
                                     + "Usage: ASMifier [-debug] <fully qualified class name or class file name>";

        /// <summary>A pseudo access flag used to distinguish class access flags.</summary>
        private const ObjectWeb.Asm.Enums.AccessFlags access_Class = (AccessFlags) 0x40000;

        /// <summary>A pseudo access flag used to distinguish field access flags.</summary>
        private const ObjectWeb.Asm.Enums.AccessFlags access_Field = (AccessFlags) 0x80000;

        /// <summary>A pseudo access flag used to distinguish inner class flags.</summary>
        private const ObjectWeb.Asm.Enums.AccessFlags access_Inner = (AccessFlags) 0x100000;

        /// <summary>
        ///     A pseudo access flag used to distinguish module requires / exports flags.
        /// </summary>
        private const ObjectWeb.Asm.Enums.AccessFlags access_Module = (AccessFlags) 0x200000;

        private const string Annotation_Visitor = "annotationVisitor";

        private const string Annotation_Visitor0 = "annotationVisitor0 = ";

        private const string Comma = "\", \"";

        private const string End_Array = " });\n";

        private const string End_Parameters = ");\n\n";

        private const string New_Object_Array = ", new Object[] {";

        private const string Visit_End = ".visitEnd();\n";

        private static readonly IList<string> Frame_Types =
            Arrays.AsList("Opcodes.TOP", "Opcodes.INTEGER", "Opcodes.FLOAT", "Opcodes.DOUBLE"
                , "Opcodes.LONG", "Opcodes.NULL", "Opcodes.UNINITIALIZED_THIS");

        private static readonly IDictionary<int, string> Class_Versions;

        /// <summary>The identifier of the annotation visitor variable in the produced code.</summary>
        protected internal readonly int id;

        /// <summary>The name of the visitor variable in the produced code.</summary>
        protected internal readonly string name;

        /// <summary>The name of the Label variables in the produced code.</summary>
        protected internal IDictionary<Label, string> labelNames;

        static ASMifier()
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            var classVersions = new Dictionary<int, string>();
            Collections.Put(classVersions, OpcodesConstants.V1_1, "V1_1");
            Collections.Put(classVersions, OpcodesConstants.V1_2, "V1_2");
            Collections.Put(classVersions, OpcodesConstants.V1_3, "V1_3");
            Collections.Put(classVersions, OpcodesConstants.V1_4, "V1_4");
            Collections.Put(classVersions, OpcodesConstants.V1_5, "V1_5");
            Collections.Put(classVersions, OpcodesConstants.V1_6, "V1_6");
            Collections.Put(classVersions, OpcodesConstants.V1_7, "V1_7");
            Collections.Put(classVersions, OpcodesConstants.V1_8, "V1_8");
            Collections.Put(classVersions, OpcodesConstants.V9, "V9");
            Collections.Put(classVersions, OpcodesConstants.V10, "V10");
            Collections.Put(classVersions, OpcodesConstants.V11, "V11");
            Collections.Put(classVersions, OpcodesConstants.V12, "V12");
            Collections.Put(classVersions, OpcodesConstants.V13, "V13");
            Collections.Put(classVersions, OpcodesConstants.V14, "V14");
            Class_Versions = classVersions;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ASMifier" />
        ///     . <i>Subclasses must not use this constructor</i>. Instead,
        ///     they must use the
        ///     <see cref="ASMifier(int, string, int)" />
        ///     version.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public ASMifier()
            : this(VisitorAsmApiVersion.Asm7, "classWriter", 0)
        {
            /* latest api = */
            if (GetType() != typeof(ASMifier)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ASMifier" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this class. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        /// <param name="visitorVariableName">
        ///     the name of the visitor variable in the produced code.
        /// </param>
        /// <param name="annotationVisitorId">
        ///     identifier of the annotation visitor variable in the produced code.
        /// </param>
        protected internal ASMifier(VisitorAsmApiVersion api, string visitorVariableName, int annotationVisitorId
        )
            : base(api)
        {
            name = visitorVariableName;
            id = annotationVisitorId;
        }

        /// <summary>
        ///     Prints the ASM source code to generate the given class to the standard output.
        /// </summary>
        /// <remarks>
        ///     Prints the ASM source code to generate the given class to the standard output.
        ///     <p>Usage: ASMifier [-debug] &lt;binary class name or class file name&gt;
        /// </remarks>
        /// <param name="args">the command line arguments.</param>
        /// <exception cref="System.IO.IOException">
        ///     if the class cannot be found, or if an IOException occurs.
        /// </exception>
        public static void Main(string[] args)
        {
            Main(args, Console.Out, Console.Out);
        }

        /// <summary>
        ///     Prints the ASM source code to generate the given class to the given output.
        /// </summary>
        /// <remarks>
        ///     Prints the ASM source code to generate the given class to the given output.
        ///     <p>Usage: ASMifier [-debug] &lt;binary class name or class file name&gt;
        /// </remarks>
        /// <param name="args">the command line arguments.</param>
        /// <param name="output">where to print the result.</param>
        /// <param name="logger">where to log errors.</param>
        /// <exception cref="System.IO.IOException">
        ///     if the class cannot be found, or if an IOException occurs.
        /// </exception>
        internal static void Main(string[] args, TextWriter output, TextWriter logger)
        {
            Main(args, Usage, new ASMifier(), output, logger);
        }

        // -----------------------------------------------------------------------------------------------
        // Classes
        // -----------------------------------------------------------------------------------------------
        public override void Visit(int version, ObjectWeb.Asm.Enums.AccessFlags access, string name, string signature
            , string superName, string[] interfaces)
        {
            string simpleName;
            if (name == null)
            {
                simpleName = "module-info";
            }
            else
            {
                var lastSlashIndex = name.LastIndexOf('/');
                if (lastSlashIndex == -1)
                {
                    simpleName = name;
                }
                else
                {
                    text.Add("package asm." + Runtime.Substring(name, 0, lastSlashIndex).Replace
                                 ('/', '.') + ";\n");
                    simpleName = Runtime.Substring(name, lastSlashIndex + 1).ReplaceAll("[-\\(\\)]"
                        , "_");
                }
            }

            text.Add("import org.objectweb.asm.AnnotationVisitor;\n");
            text.Add("import org.objectweb.asm.Attribute;\n");
            text.Add("import org.objectweb.asm.ClassReader;\n");
            text.Add("import org.objectweb.asm.ClassWriter;\n");
            text.Add("import org.objectweb.asm.ConstantDynamic;\n");
            text.Add("import org.objectweb.asm.FieldVisitor;\n");
            text.Add("import org.objectweb.asm.Handle;\n");
            text.Add("import org.objectweb.asm.Label;\n");
            text.Add("import org.objectweb.asm.MethodVisitor;\n");
            text.Add("import org.objectweb.asm.Opcodes;\n");
            text.Add("import org.objectweb.asm.RecordComponentVisitor;\n");
            text.Add("import org.objectweb.asm.Type;\n");
            text.Add("import org.objectweb.asm.TypePath;\n");
            text.Add("public class " + simpleName + "Dump implements Opcodes {\n\n");
            text.Add("public static byte[] dump () throws Exception {\n\n");
            text.Add("ClassWriter classWriter = new ClassWriter(0);\n");
            text.Add("FieldVisitor fieldVisitor;\n");
            text.Add("RecordComponentVisitor recordComponentVisitor;\n");
            text.Add("MethodVisitor methodVisitor;\n");
            text.Add("AnnotationVisitor annotationVisitor0;\n\n");
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visit(");
            var versionString = Class_Versions.GetOrNull(version);
            if (versionString != null)
                stringBuilder.Append(versionString);
            else
                stringBuilder.Append(version);
            stringBuilder.Append(", ");
            AppendAccessFlags(access | access_Class);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(signature);
            stringBuilder.Append(", ");
            AppendConstant(superName);
            stringBuilder.Append(", ");
            if (interfaces != null && interfaces.Length > 0)
            {
                stringBuilder.Append("new String[] {");
                for (var i = 0; i < interfaces.Length; ++i)
                {
                    stringBuilder.Append(i == 0 ? " " : ", ");
                    AppendConstant(interfaces[i]);
                }

                stringBuilder.Append(" }");
            }
            else
            {
                stringBuilder.Append("null");
            }

            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitSource(string file, string debug)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visitSource(");
            AppendConstant(file);
            stringBuilder.Append(", ");
            AppendConstant(debug);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitModule(string name, AccessFlags flags, string version)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("ModuleVisitor moduleVisitor = classWriter.visitModule(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendAccessFlags(flags | access_Module);
            stringBuilder.Append(", ");
            AppendConstant(version);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier("moduleVisitor", 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override void VisitNestHost(string nestHost)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visitNestHost(");
            AppendConstant(nestHost);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitOuterClass(string owner, string name, string descriptor
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visitOuterClass(");
            AppendConstant(owner);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitClassAnnotation(string descriptor, bool visible)
        {
            return VisitAnnotation(descriptor, visible);
        }

        public override Printer VisitClassTypeAnnotation(int typeRef, TypePath typePath,
            string descriptor, bool visible)
        {
            return VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        public override void VisitClassAttribute(Attribute attribute)
        {
            VisitAttribute(attribute);
        }

        public override void VisitNestMember(string nestMember)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visitNestMember(");
            AppendConstant(nestMember);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitPermittedSubtypeExperimental(string visitPermittedSubtype
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visitPermittedSubtypeExperimental(");
            AppendConstant(visitPermittedSubtype);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitInnerClass(string name, string outerName, string innerName
            , ObjectWeb.Asm.Enums.AccessFlags access)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("classWriter.visitInnerClass(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(outerName);
            stringBuilder.Append(", ");
            AppendConstant(innerName);
            stringBuilder.Append(", ");
            AppendAccessFlags(access | access_Inner);
            stringBuilder.Append(End_Parameters);
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitRecordComponentExperimental(ObjectWeb.Asm.Enums.AccessFlags access, string name,
            string descriptor, string signature)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n");
            stringBuilder.Append("recordComponentVisitor = classWriter.visitRecordComponentExperimental("
            );
            AppendAccessFlags(access | access_Field);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            AppendConstant(signature);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier("recordComponentVisitor", 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override Printer VisitField(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor, string
            signature, object value)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n");
            stringBuilder.Append("fieldVisitor = classWriter.visitField(");
            AppendAccessFlags(access | access_Field);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            AppendConstant(signature);
            stringBuilder.Append(", ");
            AppendConstant(value);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier("fieldVisitor", 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override Printer VisitMethod(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor, string
            signature, string[] exceptions)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n");
            stringBuilder.Append("methodVisitor = classWriter.visitMethod(");
            AppendAccessFlags(access);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            AppendConstant(signature);
            stringBuilder.Append(", ");
            if (exceptions != null && exceptions.Length > 0)
            {
                stringBuilder.Append("new String[] {");
                for (var i = 0; i < exceptions.Length; ++i)
                {
                    stringBuilder.Append(i == 0 ? " " : ", ");
                    AppendConstant(exceptions[i]);
                }

                stringBuilder.Append(" }");
            }
            else
            {
                stringBuilder.Append("null");
            }

            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier("methodVisitor", 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override void VisitClassEnd()
        {
            text.Add("classWriter.visitEnd();\n\n");
            text.Add("return classWriter.toByteArray();\n");
            text.Add("}\n");
            text.Add("}\n");
        }

        // -----------------------------------------------------------------------------------------------
        // Modules
        // -----------------------------------------------------------------------------------------------
        public override void VisitMainClass(string mainClass)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("moduleVisitor.visitMainClass(");
            AppendConstant(mainClass);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitPackage(string packaze)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("moduleVisitor.visitPackage(");
            AppendConstant(packaze);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitRequire(string module, ObjectWeb.Asm.Enums.AccessFlags access, string version)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("moduleVisitor.visitRequire(");
            AppendConstant(module);
            stringBuilder.Append(", ");
            AppendAccessFlags(access | access_Module);
            stringBuilder.Append(", ");
            AppendConstant(version);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitExport(string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params string[] modules
        )
        {
            VisitExportOrOpen("moduleVisitor.visitExport(", packaze, access, modules);
        }

        public override void VisitOpen(string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params string[] modules
        )
        {
            VisitExportOrOpen("moduleVisitor.visitOpen(", packaze, access, modules);
        }

        private void VisitExportOrOpen(string visitMethod, string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params
            string[] modules)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(visitMethod);
            AppendConstant(packaze);
            stringBuilder.Append(", ");
            AppendAccessFlags(access | access_Module);
            if (modules != null && modules.Length > 0)
            {
                stringBuilder.Append(", new String[] {");
                for (var i = 0; i < modules.Length; ++i)
                {
                    stringBuilder.Append(i == 0 ? " " : ", ");
                    AppendConstant(modules[i]);
                }

                stringBuilder.Append(" }");
            }

            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitUse(string service)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("moduleVisitor.visitUse(");
            AppendConstant(service);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitProvide(string service, params string[] providers)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("moduleVisitor.visitProvide(");
            AppendConstant(service);
            stringBuilder.Append(",  new String[] {");
            for (var i = 0; i < providers.Length; ++i)
            {
                stringBuilder.Append(i == 0 ? " " : ", ");
                AppendConstant(providers[i]);
            }

            stringBuilder.Append(End_Array);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitModuleEnd()
        {
            text.Add("moduleVisitor.visitEnd();\n");
        }

        // -----------------------------------------------------------------------------------------------
        // Annotations
        // -----------------------------------------------------------------------------------------------
        // DontCheck(OverloadMethodsDeclarationOrder): overloads are semantically different.
        public override void Visit(string name, object value)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(Annotation_Visitor).Append(id).Append(".visit(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(value);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitEnum(string name, string descriptor, string value)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(Annotation_Visitor).Append(id).Append(".visitEnum(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            AppendConstant(value);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitAnnotation(string name, string descriptor)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append("AnnotationVisitor annotationVisitor").Append(
                id + 1).Append(" = annotationVisitor");
            stringBuilder.Append(id).Append(".visitAnnotation(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, id + 1);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override Printer VisitArray(string name)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n");
            stringBuilder.Append("AnnotationVisitor annotationVisitor").Append(id + 1).Append
                (" = annotationVisitor");
            stringBuilder.Append(id).Append(".visitArray(");
            AppendConstant(name);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, id + 1);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override void VisitAnnotationEnd()
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(Annotation_Visitor).Append(id).Append(Visit_End);
            text.Add(stringBuilder.ToString());
        }

        // -----------------------------------------------------------------------------------------------
        // Record components
        // -----------------------------------------------------------------------------------------------
        public override Printer VisitRecordComponentAnnotationExperimental(string descriptor
            , bool visible)
        {
            // TODO Use this call when not experimental anymore
            // return visitAnnotation(descriptor, visible);
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append(Annotation_Visitor0).Append(name).Append(".visitAnnotationExperimental("
            );
            AppendConstant(descriptor);
            stringBuilder.Append(", ").Append(visible).Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override Printer VisitRecordComponentTypeAnnotationExperimental(int typeRef
            , TypePath typePath, string descriptor, bool visible)
        {
            // TODO Use this call when not experimental anymore
            // return visitTypeAnnotation(typeRef, typePath, descriptor, visible);
            return VisitTypeAnnotation("visitTypeAnnotationExperimental", typeRef, typePath,
                descriptor, visible);
        }

        public override void VisitRecordComponentAttributeExperimental(Attribute attribute
        )
        {
            // TODO Use this call when not experimental anymore
            // visitAttribute(attribute);
            stringBuilder.Length = 0;
            stringBuilder.Append("// ATTRIBUTE ").Append(attribute.type).Append('\n');
            if (attribute is ASMifierSupport)
            {
                if (labelNames == null) labelNames = new Dictionary<Label, string>();
                stringBuilder.Append("{\n");
                ((ASMifierSupport) attribute).Asmify(stringBuilder, "attribute", labelNames);
                stringBuilder.Append(name).Append(".visitAttributeExperimental(attribute);\n");
                stringBuilder.Append("}\n");
            }

            text.Add(stringBuilder.ToString());
        }

        public override void VisitRecordComponentEndExperimental()
        {
            stringBuilder.Length = 0;
            // TODO Use this call when not experimental anymore
            // stringBuilder.append(name).append(VISIT_END);
            stringBuilder.Append(name).Append(".visitEndExperimental();\n");
            text.Add(stringBuilder.ToString());
        }

        // -----------------------------------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------------------------------
        public override Printer VisitFieldAnnotation(string descriptor, bool visible)
        {
            return VisitAnnotation(descriptor, visible);
        }

        public override Printer VisitFieldTypeAnnotation(int typeRef, TypePath typePath,
            string descriptor, bool visible)
        {
            return VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        public override void VisitFieldAttribute(Attribute attribute)
        {
            VisitAttribute(attribute);
        }

        public override void VisitFieldEnd()
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(Visit_End);
            text.Add(stringBuilder.ToString());
        }

        // -----------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------
        public override void VisitParameter(string parameterName, ObjectWeb.Asm.Enums.AccessFlags access)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitParameter(");
            AppendString(stringBuilder, parameterName);
            stringBuilder.Append(", ");
            AppendAccessFlags(access);
            text.Add(stringBuilder.Append(");\n").ToString());
        }

        public override Printer VisitAnnotationDefault()
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append(Annotation_Visitor0).Append(name).Append(".visitAnnotationDefault();\n"
            );
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override Printer VisitMethodAnnotation(string descriptor, bool visible)
        {
            return VisitAnnotation(descriptor, visible);
        }

        public override Printer VisitMethodTypeAnnotation(int typeRef, TypePath typePath,
            string descriptor, bool visible)
        {
            return VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        public override Printer VisitAnnotableParameterCount(int parameterCount, bool visible
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitAnnotableParameterCount(").Append(parameterCount
            ).Append(", ").Append(visible).Append(");\n");
            text.Add(stringBuilder.ToString());
            return this;
        }

        public override Printer VisitParameterAnnotation(int parameter, string descriptor
            , bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append(Annotation_Visitor0).Append(name).Append(".visitParameterAnnotation("
            ).Append(parameter).Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ").Append(visible).Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override void VisitMethodAttribute(Attribute attribute)
        {
            VisitAttribute(attribute);
        }

        public override void VisitCode()
        {
            text.Add(name + ".visitCode();\n");
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            stringBuilder.Length = 0;
            switch (type)
            {
                case OpcodesConstants.F_New:
                case OpcodesConstants.F_Full:
                {
                    DeclareFrameTypes(numLocal, local);
                    DeclareFrameTypes(numStack, stack);
                    if (type == OpcodesConstants.F_New)
                        stringBuilder.Append(name).Append(".visitFrame(Opcodes.F_NEW, ");
                    else
                        stringBuilder.Append(name).Append(".visitFrame(Opcodes.F_FULL, ");
                    stringBuilder.Append(numLocal).Append(New_Object_Array);
                    AppendFrameTypes(numLocal, local);
                    stringBuilder.Append("}, ").Append(numStack).Append(New_Object_Array);
                    AppendFrameTypes(numStack, stack);
                    stringBuilder.Append('}');
                    break;
                }

                case OpcodesConstants.F_Append:
                {
                    DeclareFrameTypes(numLocal, local);
                    stringBuilder.Append(name).Append(".visitFrame(Opcodes.F_APPEND,").Append(numLocal
                    ).Append(New_Object_Array);
                    AppendFrameTypes(numLocal, local);
                    stringBuilder.Append("}, 0, null");
                    break;
                }

                case OpcodesConstants.F_Chop:
                {
                    stringBuilder.Append(name).Append(".visitFrame(Opcodes.F_CHOP,").Append(numLocal)
                        .Append(", null, 0, null");
                    break;
                }

                case OpcodesConstants.F_Same:
                {
                    stringBuilder.Append(name).Append(".visitFrame(Opcodes.F_SAME, 0, null, 0, null");
                    break;
                }

                case OpcodesConstants.F_Same1:
                {
                    DeclareFrameTypes(1, stack);
                    stringBuilder.Append(name).Append(".visitFrame(Opcodes.F_SAME1, 0, null, 1, new Object[] {"
                    );
                    AppendFrameTypes(1, stack);
                    stringBuilder.Append('}');
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }

            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitInsn(int opcode)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitInsn(").Append(Opcodes[opcode]).Append(");\n"
            );
            text.Add(stringBuilder.ToString());
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitIntInsn(").Append(Opcodes[opcode]).Append
                (", ").Append(opcode == OpcodesConstants.Newarray ? Types[operand] : operand.ToString()).Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitVarInsn(").Append(Opcodes[opcode]).Append
                (", ").Append(var).Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitTypeInsn(").Append(Opcodes[opcode]).Append
                (", ");
            AppendConstant(type);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(this.name).Append(".visitFieldInsn(").Append(Opcodes[opcode]
            ).Append(", ");
            AppendConstant(owner);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMethodInsn(int opcode, string owner, string name, string
            descriptor, bool isInterface)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(this.name).Append(".visitMethodInsn(").Append(Opcodes[opcode
            ]).Append(", ");
            AppendConstant(owner);
            stringBuilder.Append(", ");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            stringBuilder.Append(isInterface ? "true" : "false");
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(this.name).Append(".visitInvokeDynamicInsn(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            AppendConstant(bootstrapMethodHandle);
            stringBuilder.Append(", new Object[]{");
            for (var i = 0; i < bootstrapMethodArguments.Length; ++i)
            {
                AppendConstant(bootstrapMethodArguments[i]);
                if (i != bootstrapMethodArguments.Length - 1) stringBuilder.Append(", ");
            }

            stringBuilder.Append("});\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            stringBuilder.Length = 0;
            DeclareLabel(label);
            stringBuilder.Append(name).Append(".visitJumpInsn(").Append(Opcodes[opcode]).Append
                (", ");
            AppendLabel(label);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitLabel(Label label)
        {
            stringBuilder.Length = 0;
            DeclareLabel(label);
            stringBuilder.Append(name).Append(".visitLabel(");
            AppendLabel(label);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitLdcInsn(object value)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitLdcInsn(");
            AppendConstant(value);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitIincInsn(int var, int increment)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitIincInsn(").Append(var).Append(", ").Append
                (increment).Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            stringBuilder.Length = 0;
            foreach (var label in labels) DeclareLabel(label);
            DeclareLabel(dflt);
            stringBuilder.Append(name).Append(".visitTableSwitchInsn(").Append(min).Append(", "
            ).Append(max).Append(", ");
            AppendLabel(dflt);
            stringBuilder.Append(", new Label[] {");
            for (var i = 0; i < labels.Length; ++i)
            {
                stringBuilder.Append(i == 0 ? " " : ", ");
                AppendLabel(labels[i]);
            }

            stringBuilder.Append(End_Array);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            stringBuilder.Length = 0;
            foreach (var label in labels) DeclareLabel(label);
            DeclareLabel(dflt);
            stringBuilder.Append(name).Append(".visitLookupSwitchInsn(");
            AppendLabel(dflt);
            stringBuilder.Append(", new int[] {");
            for (var i = 0; i < keys.Length; ++i) stringBuilder.Append(i == 0 ? " " : ", ").Append(keys[i]);
            stringBuilder.Append(" }, new Label[] {");
            for (var i = 0; i < labels.Length; ++i)
            {
                stringBuilder.Append(i == 0 ? " " : ", ");
                AppendLabel(labels[i]);
            }

            stringBuilder.Append(End_Array);
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitMultiANewArrayInsn(");
            AppendConstant(descriptor);
            stringBuilder.Append(", ").Append(numDimensions).Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitInsnAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            return VisitTypeAnnotation("visitInsnAnnotation", typeRef, typePath, descriptor,
                visible);
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type)
        {
            stringBuilder.Length = 0;
            DeclareLabel(start);
            DeclareLabel(end);
            DeclareLabel(handler);
            stringBuilder.Append(name).Append(".visitTryCatchBlock(");
            AppendLabel(start);
            stringBuilder.Append(", ");
            AppendLabel(end);
            stringBuilder.Append(", ");
            AppendLabel(handler);
            stringBuilder.Append(", ");
            AppendConstant(type);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitTryCatchAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            return VisitTypeAnnotation("visitTryCatchAnnotation", typeRef, typePath, descriptor
                , visible);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(this.name).Append(".visitLocalVariable(");
            AppendConstant(name);
            stringBuilder.Append(", ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ");
            AppendConstant(signature);
            stringBuilder.Append(", ");
            AppendLabel(start);
            stringBuilder.Append(", ");
            AppendLabel(end);
            stringBuilder.Append(", ").Append(index).Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitLocalVariableAnnotation(int typeRef, TypePath typePath
            , Label[] start, Label[] end, int[] index, string descriptor, bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append(Annotation_Visitor0).Append(name).Append(".visitLocalVariableAnnotation("
            ).Append(typeRef);
            if (typePath == null)
                stringBuilder.Append(", null, ");
            else
                stringBuilder.Append(", TypePath.fromString(\"").Append(typePath).Append("\"), ");
            stringBuilder.Append("new Label[] {");
            for (var i = 0; i < start.Length; ++i)
            {
                stringBuilder.Append(i == 0 ? " " : ", ");
                AppendLabel(start[i]);
            }

            stringBuilder.Append(" }, new Label[] {");
            for (var i = 0; i < end.Length; ++i)
            {
                stringBuilder.Append(i == 0 ? " " : ", ");
                AppendLabel(end[i]);
            }

            stringBuilder.Append(" }, new int[] {");
            for (var i = 0; i < index.Length; ++i) stringBuilder.Append(i == 0 ? " " : ", ").Append(index[i]);
            stringBuilder.Append(" }, ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ").Append(visible).Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        public override void VisitLineNumber(int line, Label start)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitLineNumber(").Append(line).Append(", ");
            AppendLabel(start);
            stringBuilder.Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(".visitMaxs(").Append(maxStack).Append(", ").Append
                (maxLocals).Append(");\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMethodEnd()
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(name).Append(Visit_End);
            text.Add(stringBuilder.ToString());
        }

        // -----------------------------------------------------------------------------------------------
        // Common methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Visits a class, field or method annotation.</summary>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>
        ///     a new
        ///     <see cref="ASMifier" />
        ///     to visit the annotation values.
        /// </returns>
        public virtual ASMifier VisitAnnotation(string descriptor, bool visible)
        {
            // DontCheck(OverloadMethodsDeclarationOrder): overloads are semantically different.
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append(Annotation_Visitor0).Append(name).Append(".visitAnnotation("
            );
            AppendConstant(descriptor);
            stringBuilder.Append(", ").Append(visible).Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        /// <summary>Visits a class, field or method type annotation.</summary>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="TypeReference.Field" />
        ///     . See
        ///     <see cref="TypeReference" />
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
        ///     a new
        ///     <see cref="ASMifier" />
        ///     to visit the annotation values.
        /// </returns>
        public virtual ASMifier VisitTypeAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            return VisitTypeAnnotation("visitTypeAnnotation", typeRef, typePath, descriptor,
                visible);
        }

        /// <summary>
        ///     Visits a class, field, method, instruction or try catch block type annotation.
        /// </summary>
        /// <param name="method">the name of the visit method for this type of annotation.</param>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <see cref="TypeReference.Field" />
        ///     . See
        ///     <see cref="TypeReference" />
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
        ///     a new
        ///     <see cref="ASMifier" />
        ///     to visit the annotation values.
        /// </returns>
        public virtual ASMifier VisitTypeAnnotation(string method, int typeRef, TypePath
            typePath, string descriptor, bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("{\n").Append(Annotation_Visitor0).Append(name).Append(".").Append(method).Append("(")
                .Append(typeRef);
            if (typePath == null)
                stringBuilder.Append(", null, ");
            else
                stringBuilder.Append(", TypePath.fromString(\"").Append(typePath).Append("\"), ");
            AppendConstant(descriptor);
            stringBuilder.Append(", ").Append(visible).Append(");\n");
            text.Add(stringBuilder.ToString());
            var asmifier = CreateASMifier(Annotation_Visitor, 0);
            text.Add(asmifier.GetText());
            text.Add("}\n");
            return asmifier;
        }

        /// <summary>Visit a class, field or method attribute.</summary>
        /// <param name="attribute">an attribute.</param>
        public virtual void VisitAttribute(Attribute attribute)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("// ATTRIBUTE ").Append(attribute.type).Append('\n');
            if (attribute is ASMifierSupport)
            {
                if (labelNames == null) labelNames = new Dictionary<Label, string>();
                stringBuilder.Append("{\n");
                ((ASMifierSupport) attribute).Asmify(stringBuilder, "attribute", labelNames);
                stringBuilder.Append(name).Append(".visitAttribute(attribute);\n");
                stringBuilder.Append("}\n");
            }

            text.Add(stringBuilder.ToString());
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Constructs a new
        ///     <see cref="ASMifier" />
        ///     .
        /// </summary>
        /// <param name="visitorVariableName">
        ///     the name of the visitor variable in the produced code.
        /// </param>
        /// <param name="annotationVisitorId">
        ///     identifier of the annotation visitor variable in the produced code.
        /// </param>
        /// <returns>
        ///     a new
        ///     <see cref="ASMifier" />
        ///     .
        /// </returns>
        protected internal virtual ASMifier CreateASMifier(string visitorVariableName, int
            annotationVisitorId)
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            return new ASMifier(api, visitorVariableName, annotationVisitorId);
        }

        /// <summary>
        ///     Appends a string representation of the given access flags to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="accessFlags">some access flags.</param>
        private void AppendAccessFlags(ObjectWeb.Asm.Enums.AccessFlags accessFlags)
        {
            var isEmpty = true;
            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Public))
            {
                stringBuilder.Append("ACC_PUBLIC");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Private))
            {
                stringBuilder.Append("ACC_PRIVATE");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Protected))
            {
                stringBuilder.Append("ACC_PROTECTED");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Final))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                if ((accessFlags & access_Module) == 0)
                    stringBuilder.Append("ACC_FINAL");
                else
                    stringBuilder.Append("ACC_TRANSITIVE");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Static))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_STATIC");
                isEmpty = false;
            }

            if ((accessFlags & (ObjectWeb.Asm.Enums.AccessFlags.Synchronized | ObjectWeb.Asm.Enums.AccessFlags.Super
                                                                  | ObjectWeb.Asm.Enums.AccessFlags.Transitive)) != 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                if ((accessFlags & access_Class) == 0)
                {
                    if ((accessFlags & access_Module) == 0)
                        stringBuilder.Append("ACC_SYNCHRONIZED");
                    else
                        stringBuilder.Append("ACC_TRANSITIVE");
                }
                else
                {
                    stringBuilder.Append("ACC_SUPER");
                }

                isEmpty = false;
            }

            if ((accessFlags & (ObjectWeb.Asm.Enums.AccessFlags.Volatile | ObjectWeb.Asm.Enums.AccessFlags.Bridge |
                                ObjectWeb.Asm.Enums.AccessFlags.Static_Phase)) != 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                if ((accessFlags & access_Field) == 0)
                {
                    if ((accessFlags & access_Module) == 0)
                        stringBuilder.Append("ACC_BRIDGE");
                    else
                        stringBuilder.Append("ACC_STATIC_PHASE");
                }
                else
                {
                    stringBuilder.Append("ACC_VOLATILE");
                }

                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Varargs) && (accessFlags & (access_Class
                                                                                     | access_Field)) == 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_VARARGS");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Transient) && (accessFlags & access_Field
                ) != 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_TRANSIENT");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Native) && (accessFlags & (access_Class
                                                                                    | access_Field)) == 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_NATIVE");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Enum) && (accessFlags & (access_Class
                                                                                  | access_Field | access_Inner)) != 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_ENUM");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Annotation) && (accessFlags & (access_Class
                                                                                        | access_Inner)) != 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_ANNOTATION");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Abstract))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_ABSTRACT");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Interface))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_INTERFACE");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Strict))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_STRICT");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Synthetic))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_SYNTHETIC");
                isEmpty = false;
            }

            if (accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Deprecated))
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                stringBuilder.Append("ACC_DEPRECATED");
                isEmpty = false;
            }

            if ((accessFlags & (ObjectWeb.Asm.Enums.AccessFlags.Mandated | ObjectWeb.Asm.Enums.AccessFlags.Module))
                != 0)
            {
                if (!isEmpty) stringBuilder.Append(" | ");
                if ((accessFlags & access_Class) == 0)
                    stringBuilder.Append("ACC_MANDATED");
                else
                    stringBuilder.Append("ACC_MODULE");
                isEmpty = false;
            }

            if (isEmpty) stringBuilder.Append('0');
        }

        /// <summary>
        ///     Appends a string representation of the given constant to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="value">
        ///     a
        ///     <see cref="string" />
        ///     ,
        ///     <see cref="Type" />
        ///     ,
        ///     <see cref="Handle" />
        ///     ,
        ///     <see cref="byte" />
        ///     ,
        ///     <see cref="short" />
        ///     ,
        ///     <see cref="char" />
        ///     ,
        ///     <see cref="int" />
        ///     ,
        ///     <see cref="float" />
        ///     ,
        ///     <see cref="long" />
        ///     or
        ///     <see cref="double" />
        ///     object,
        ///     or an array of primitive values. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        protected internal virtual void AppendConstant(object value)
        {
            if (value == null)
            {
                stringBuilder.Append("null");
            }
            else if (value is string)
            {
                AppendString(stringBuilder, (string) value);
            }
            else if (value is Type)
            {
                stringBuilder.Append("Type.getType(\"");
                stringBuilder.Append(((Type) value).GetDescriptor());
                stringBuilder.Append("\")");
            }
            else if (value is Handle)
            {
                stringBuilder.Append("new Handle(");
                var handle = (Handle) value;
                stringBuilder.Append("Opcodes.").Append(Handle_Tag[handle.GetTag()]).Append(", \""
                );
                stringBuilder.Append(handle.GetOwner()).Append(Comma);
                stringBuilder.Append(handle.GetName()).Append(Comma);
                stringBuilder.Append(handle.GetDesc()).Append("\", ");
                stringBuilder.Append(handle.IsInterface()).Append(")");
            }
            else if (value is ConstantDynamic)
            {
                stringBuilder.Append("new ConstantDynamic(\"");
                var constantDynamic = (ConstantDynamic) value;
                stringBuilder.Append(constantDynamic.GetName()).Append(Comma);
                stringBuilder.Append(constantDynamic.GetDescriptor()).Append("\", ");
                AppendConstant(constantDynamic.GetBootstrapMethod());
                stringBuilder.Append(New_Object_Array);
                var bootstrapMethodArgumentCount = constantDynamic.GetBootstrapMethodArgumentCount
                    ();
                for (var i = 0; i < bootstrapMethodArgumentCount; ++i)
                {
                    AppendConstant(constantDynamic.GetBootstrapMethodArgument(i));
                    if (i != bootstrapMethodArgumentCount - 1) stringBuilder.Append(", ");
                }

                stringBuilder.Append("})");
            }
            else if (value is byte)
            {
                stringBuilder.Append("new Byte((byte)").Append(value).Append(')');
            }
            else if (value is bool)
            {
                stringBuilder.Append((bool) value ? "Boolean.TRUE" : "Boolean.FALSE");
            }
            else if (value is short)
            {
                stringBuilder.Append("new Short((short)").Append(value).Append(')');
            }
            else if (value is char)
            {
                stringBuilder.Append("new Character((char)").Append((int) (char) value).Append(')'
                );
            }
            else if (value is int)
            {
                stringBuilder.Append("new Integer(").Append(value).Append(')');
            }
            else if (value is float)
            {
                stringBuilder.Append("new Float(\"").Append(value).Append("\")");
            }
            else if (value is long)
            {
                stringBuilder.Append("new Long(").Append(value).Append("L)");
            }
            else if (value is double)
            {
                stringBuilder.Append("new Double(\"").Append(value).Append("\")");
            }
            else if (value is byte[])
            {
                var byteArray = (byte[]) value;
                stringBuilder.Append("new byte[] {");
                for (var i = 0; i < byteArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append(byteArray[i]);
                stringBuilder.Append('}');
            }
            else if (value is bool[])
            {
                var booleanArray = (bool[]) value;
                stringBuilder.Append("new boolean[] {");
                for (var i = 0; i < booleanArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append(booleanArray[i]);
                stringBuilder.Append('}');
            }
            else if (value is short[])
            {
                var shortArray = (short[]) value;
                stringBuilder.Append("new short[] {");
                for (var i = 0; i < shortArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append("(short)").Append(shortArray
                        [i]);
                stringBuilder.Append('}');
            }
            else if (value is char[])
            {
                var charArray = (char[]) value;
                stringBuilder.Append("new char[] {");
                for (var i = 0; i < charArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append("(char)").Append((int) charArray
                        [i]);
                stringBuilder.Append('}');
            }
            else if (value is int[])
            {
                var intArray = (int[]) value;
                stringBuilder.Append("new int[] {");
                for (var i = 0; i < intArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append(intArray[i]);
                stringBuilder.Append('}');
            }
            else if (value is long[])
            {
                var longArray = (long[]) value;
                stringBuilder.Append("new long[] {");
                for (var i = 0; i < longArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append(longArray[i]).Append('L'
                    );
                stringBuilder.Append('}');
            }
            else if (value is float[])
            {
                var floatArray = (float[]) value;
                stringBuilder.Append("new float[] {");
                for (var i = 0; i < floatArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append(floatArray[i]).Append('f'
                    );
                stringBuilder.Append('}');
            }
            else if (value is double[])
            {
                var doubleArray = (double[]) value;
                stringBuilder.Append("new double[] {");
                for (var i = 0; i < doubleArray.Length; i++)
                    stringBuilder.Append(i == 0 ? string.Empty : ",").Append(doubleArray[i]).Append('d'
                    );
                stringBuilder.Append('}');
            }
        }

        /// <summary>
        ///     Calls
        ///     <see cref="DeclareLabel(Org.Objectweb.Asm.Label)" />
        ///     for each label in the given stack map frame types.
        /// </summary>
        /// <param name="numTypes">the number of stack map frame types in 'frameTypes'.</param>
        /// <param name="frameTypes">
        ///     an array of stack map frame types, in the format described in
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitFrame(int, int, object[], int, object[])
        /// 	" />
        ///     .
        /// </param>
        private void DeclareFrameTypes(int numTypes, object[] frameTypes)
        {
            for (var i = 0; i < numTypes; ++i)
                if (frameTypes[i] is Label)
                    DeclareLabel((Label) frameTypes[i]);
        }

        /// <summary>
        ///     Appends the given stack map frame types to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="numTypes">the number of stack map frame types in 'frameTypes'.</param>
        /// <param name="frameTypes">
        ///     an array of stack map frame types, in the format described in
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitFrame(int, int, object[], int, object[])
        /// 	" />
        ///     .
        /// </param>
        private void AppendFrameTypes(int numTypes, object[] frameTypes)
        {
            for (var i = 0; i < numTypes; ++i)
            {
                if (i > 0) stringBuilder.Append(", ");
                if (frameTypes[i] is string)
                    AppendConstant(frameTypes[i]);
                else if (frameTypes[i] is int)
                    stringBuilder.Append(Frame_Types[(int) frameTypes[i]]);
                else
                    AppendLabel((Label) frameTypes[i]);
            }
        }

        /// <summary>
        ///     Appends a declaration of the given label to
        ///     <see cref="Printer.stringBuilder" />
        ///     . This declaration is of the
        ///     form "Label labelXXX = new Label();". Does nothing if the given label has already been
        ///     declared.
        /// </summary>
        /// <param name="label">a label.</param>
        protected internal virtual void DeclareLabel(Label label)
        {
            if (labelNames == null) labelNames = new Dictionary<Label, string>();
            var labelName = labelNames.GetOrNull(label);
            if (labelName == null)
            {
                labelName = "label" + labelNames.Count;
                Collections.Put(labelNames, label, labelName);
                stringBuilder.Append("Label ").Append(labelName).Append(" = new Label();\n");
            }
        }

        /// <summary>
        ///     Appends the name of the given label to
        ///     <see cref="Printer.stringBuilder" />
        ///     . The given label <i>must</i>
        ///     already have a name. One way to ensure this is to always call
        ///     <see cref="DeclareLabel" />
        ///     before
        ///     calling this method.
        /// </summary>
        /// <param name="label">a label.</param>
        protected internal virtual void AppendLabel(Label label)
        {
            stringBuilder.Append(labelNames.GetOrNull(label));
        }
    }
}