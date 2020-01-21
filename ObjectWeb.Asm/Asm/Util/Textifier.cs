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
using ObjectWeb.Asm.Signature;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="Printer" />
    ///     that prints a disassembled view of the classes it visits.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class Textifier : Printer
    {
        /// <summary>The help message shown when command line arguments are incorrect.</summary>
        private const string Usage = "Prints a disassembled view of the given class.\n" +
                                     "Usage: Textifier [-debug] <fully qualified class name or class file name>";

        /// <summary>The type of internal names.</summary>
        /// <remarks>
        ///     The type of internal names. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Internal_Name = 0;

        /// <summary>The type of field descriptors.</summary>
        /// <remarks>
        ///     The type of field descriptors. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Field_Descriptor = 1;

        /// <summary>The type of field signatures.</summary>
        /// <remarks>
        ///     The type of field signatures. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Field_Signature = 2;

        /// <summary>The type of method descriptors.</summary>
        /// <remarks>
        ///     The type of method descriptors. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Method_Descriptor = 3;

        /// <summary>The type of method signatures.</summary>
        /// <remarks>
        ///     The type of method signatures. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Method_Signature = 4;

        /// <summary>The type of class signatures.</summary>
        /// <remarks>
        ///     The type of class signatures. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Class_Signature = 5;

        /// <summary>The type of method handle descriptors.</summary>
        /// <remarks>
        ///     The type of method handle descriptors. See
        ///     <see cref="AppendDescriptor(int, string)" />
        ///     .
        /// </remarks>
        public const int Handle_Descriptor = 9;

        private const string Class_Suffix = ".class";

        private const string Deprecated = "// DEPRECATED\n";

        private const string Invisible = " // invisible\n";

        private static readonly IList<string> Frame_Types =
            Arrays.AsList("T", "I", "F", "D", "J", "N", "U");

        /// <summary>The access flags of the visited class.</summary>
        private int access;

        /// <summary>The names of the labels.</summary>
        protected internal IDictionary<Label, string> labelNames;

        /// <summary>The indentation of labels.</summary>
        protected internal string ltab = "   ";

        /// <summary>The number of annotation values visited so far.</summary>
        private int numAnnotationValues;

        /// <summary>The indentation of class members at depth level 1 (e.g.</summary>
        /// <remarks>
        ///     The indentation of class members at depth level 1 (e.g. fields, methods).
        /// </remarks>
        protected internal string tab = "  ";

        /// <summary>The indentation of class elements at depth level 2 (e.g.</summary>
        /// <remarks>
        ///     The indentation of class elements at depth level 2 (e.g. bytecode instructions in methods).
        /// </remarks>
        protected internal string tab2 = "    ";

        /// <summary>The indentation of class elements at depth level 3 (e.g.</summary>
        /// <remarks>
        ///     The indentation of class elements at depth level 3 (e.g. switch cases in methods).
        /// </remarks>
        protected internal string tab3 = "      ";

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Textifier" />
        ///     . <i>Subclasses must not use this constructor</i>. Instead,
        ///     they must use the
        ///     <see cref="Textifier(int)" />
        ///     version.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public Textifier()
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            if (GetType() != typeof(Textifier)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Textifier" />
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
        protected internal Textifier(VisitorAsmApiVersion api)
            : base(api)
        {
        }

        /// <summary>Prints a disassembled view of the given class to the standard output.</summary>
        /// <remarks>
        ///     Prints a disassembled view of the given class to the standard output.
        ///     <p>Usage: Textifier [-debug] &lt;binary class name or class file name &gt;
        /// </remarks>
        /// <param name="args">the command line arguments.</param>
        /// <exception cref="IOException">
        ///     if the class cannot be found, or if an IOException occurs.
        /// </exception>
        public static void Main(string[] args)
        {
            Main(args, Console.Out, Console.Error);
        }

        /// <summary>Prints a disassembled view of the given class to the given output.</summary>
        /// <remarks>
        ///     Prints a disassembled view of the given class to the given output.
        ///     <p>Usage: Textifier [-debug] &lt;binary class name or class file name &gt;
        /// </remarks>
        /// <param name="args">the command line arguments.</param>
        /// <param name="output">where to print the result.</param>
        /// <param name="logger">where to log errors.</param>
        /// <exception cref="IOException">
        ///     if the class cannot be found, or if an IOException occurs.
        /// </exception>
        internal static void Main(string[] args, TextWriter output, TextWriter logger)
        {
            Main(args, Usage, new Textifier(), output, logger);
        }

        // -----------------------------------------------------------------------------------------------
        // Classes
        // -----------------------------------------------------------------------------------------------
        public override void Visit(int version, int access, string name, string signature
            , string superName, string[] interfaces)
        {
            if ((access & OpcodesConstants.Acc_Module) != 0)
                // Modules are printed in visitModule.
                return;
            this.access = access;
            var majorVersion = version & 0xFFFF;
            var minorVersion = (int) ((uint) version >> 16);
            stringBuilder.Length = 0;
            stringBuilder.Append("// class version ").Append(majorVersion).Append('.').Append
                (minorVersion).Append(" (").Append(version).Append(")\n");
            if ((access & OpcodesConstants.Acc_Deprecated) != 0) stringBuilder.Append(Deprecated);
            AppendRawAccess(access);
            AppendDescriptor(Class_Signature, signature);
            if (signature != null) AppendJavaDeclaration(name, signature);
            AppendAccess(access & ~(OpcodesConstants.Acc_Super | OpcodesConstants.Acc_Module)
            );
            if ((access & OpcodesConstants.Acc_Annotation) != 0)
                stringBuilder.Append("@interface ");
            else if ((access & OpcodesConstants.Acc_Interface) != 0)
                stringBuilder.Append("interface ");
            else if ((access & OpcodesConstants.Acc_Enum) == 0) stringBuilder.Append("class ");
            AppendDescriptor(Internal_Name, name);
            if (superName != null && !"java/lang/Object".Equals(superName))
            {
                stringBuilder.Append(" extends ");
                AppendDescriptor(Internal_Name, superName);
            }

            if (interfaces != null && interfaces.Length > 0)
            {
                stringBuilder.Append(" implements ");
                for (var i = 0; i < interfaces.Length; ++i)
                {
                    AppendDescriptor(Internal_Name, interfaces[i]);
                    if (i != interfaces.Length - 1) stringBuilder.Append(' ');
                }
            }

            stringBuilder.Append(" {\n\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitSource(string file, string debug)
        {
            stringBuilder.Length = 0;
            if (file != null) stringBuilder.Append(tab).Append("// compiled from: ").Append(file).Append('\n');
            if (debug != null) stringBuilder.Append(tab).Append("// debug info: ").Append(debug).Append('\n');
            if (stringBuilder.Length > 0) text.Add(stringBuilder.ToString());
        }

        public override Printer VisitModule(string name, int access, string version)
        {
            stringBuilder.Length = 0;
            if ((access & OpcodesConstants.Acc_Open) != 0) stringBuilder.Append("open ");
            stringBuilder.Append("module ").Append(name).Append(" { ").Append(version == null
                ? string.Empty
                : "// " + version).Append("\n\n");
            text.Add(stringBuilder.ToString());
            return AddNewTextifier(null);
        }

        public override void VisitNestHost(string nestHost)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("NESTHOST ");
            AppendDescriptor(Internal_Name, nestHost);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitOuterClass(string owner, string name, string descriptor
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("OUTERCLASS ");
            AppendDescriptor(Internal_Name, owner);
            stringBuilder.Append(' ');
            if (name != null) stringBuilder.Append(name).Append(' ');
            AppendDescriptor(Method_Descriptor, descriptor);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitClassAnnotation(string descriptor, bool visible)
        {
            text.Add("\n");
            return VisitAnnotation(descriptor, visible);
        }

        public override Printer VisitClassTypeAnnotation(int typeRef, TypePath typePath,
            string descriptor, bool visible)
        {
            text.Add("\n");
            return VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        public override void VisitClassAttribute(Attribute attribute)
        {
            text.Add("\n");
            VisitAttribute(attribute);
        }

        public override void VisitNestMember(string nestMember)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("NESTMEMBER ");
            AppendDescriptor(Internal_Name, nestMember);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitPermittedSubtypeExperimental(string permittedSubtype)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("PERMITTEDSUBTYPE ");
            AppendDescriptor(Internal_Name, permittedSubtype);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitInnerClass(string name, string outerName, string innerName
            , int access)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab);
            AppendRawAccess(access & ~OpcodesConstants.Acc_Super);
            stringBuilder.Append(tab);
            AppendAccess(access);
            stringBuilder.Append("INNERCLASS ");
            AppendDescriptor(Internal_Name, name);
            stringBuilder.Append(' ');
            AppendDescriptor(Internal_Name, outerName);
            stringBuilder.Append(' ');
            AppendDescriptor(Internal_Name, innerName);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitRecordComponentExperimental(int access, string name,
            string descriptor, string signature)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append('\n');
            if ((access & OpcodesConstants.Acc_Deprecated) != 0) stringBuilder.Append(tab).Append(Deprecated);
            stringBuilder.Append(tab);
            AppendRawAccess(access);
            if (signature != null)
            {
                stringBuilder.Append(tab);
                AppendDescriptor(Field_Signature, signature);
                stringBuilder.Append(tab);
                AppendJavaDeclaration(name, signature);
            }

            stringBuilder.Append(tab);
            AppendAccess(access);
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append(' ').Append(name);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
            return AddNewTextifier(null);
        }

        public override Printer VisitField(int access, string name, string descriptor, string
            signature, object value)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append('\n');
            if ((access & OpcodesConstants.Acc_Deprecated) != 0) stringBuilder.Append(tab).Append(Deprecated);
            stringBuilder.Append(tab);
            AppendRawAccess(access);
            if (signature != null)
            {
                stringBuilder.Append(tab);
                AppendDescriptor(Field_Signature, signature);
                stringBuilder.Append(tab);
                AppendJavaDeclaration(name, signature);
            }

            stringBuilder.Append(tab);
            AppendAccess(access);
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append(' ').Append(name);
            if (value != null)
            {
                stringBuilder.Append(" = ");
                if (value is string)
                    stringBuilder.Append('\"').Append(value).Append('\"');
                else
                    stringBuilder.Append(value);
            }

            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
            return AddNewTextifier(null);
        }

        public override Printer VisitMethod(int access, string name, string descriptor, string
            signature, string[] exceptions)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append('\n');
            if ((access & OpcodesConstants.Acc_Deprecated) != 0) stringBuilder.Append(tab).Append(Deprecated);
            stringBuilder.Append(tab);
            AppendRawAccess(access);
            if (signature != null)
            {
                stringBuilder.Append(tab);
                AppendDescriptor(Method_Signature, signature);
                stringBuilder.Append(tab);
                AppendJavaDeclaration(name, signature);
            }

            stringBuilder.Append(tab);
            AppendAccess(access & ~(OpcodesConstants.Acc_Volatile | OpcodesConstants.Acc_Transient
                             ));
            if ((access & OpcodesConstants.Acc_Native) != 0) stringBuilder.Append("native ");
            if ((access & OpcodesConstants.Acc_Varargs) != 0) stringBuilder.Append("varargs ");
            if ((access & OpcodesConstants.Acc_Bridge) != 0) stringBuilder.Append("bridge ");
            if ((this.access & OpcodesConstants.Acc_Interface) != 0 && (access & (OpcodesConstants
                                                                                      .Acc_Abstract |
                                                                                  OpcodesConstants.Acc_Static)) == 0)
                stringBuilder.Append("default ");
            stringBuilder.Append(name);
            AppendDescriptor(Method_Descriptor, descriptor);
            if (exceptions != null && exceptions.Length > 0)
            {
                stringBuilder.Append(" throws ");
                foreach (var exception in exceptions)
                {
                    AppendDescriptor(Internal_Name, exception);
                    stringBuilder.Append(' ');
                }
            }

            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
            return AddNewTextifier(null);
        }

        public override void VisitClassEnd()
        {
            text.Add("}\n");
        }

        // -----------------------------------------------------------------------------------------------
        // Modules
        // -----------------------------------------------------------------------------------------------
        public override void VisitMainClass(string mainClass)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("  // main class ").Append(mainClass).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitPackage(string packaze)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("  // package ").Append(packaze).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitRequire(string require, int access, string version)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("requires ");
            if ((access & OpcodesConstants.Acc_Transitive) != 0) stringBuilder.Append("transitive ");
            if ((access & OpcodesConstants.Acc_Static_Phase) != 0) stringBuilder.Append("static ");
            stringBuilder.Append(require).Append(';');
            AppendRawAccess(access);
            if (version != null) stringBuilder.Append("  // version ").Append(version).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitExport(string packaze, int access, params string[] modules
        )
        {
            VisitExportOrOpen("exports ", packaze, access, modules);
        }

        public override void VisitOpen(string packaze, int access, params string[] modules
        )
        {
            VisitExportOrOpen("opens ", packaze, access, modules);
        }

        private void VisitExportOrOpen(string method, string packaze, int access, params
            string[] modules)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append(method);
            stringBuilder.Append(packaze);
            if (modules != null && modules.Length > 0)
                stringBuilder.Append(" to");
            else
                stringBuilder.Append(';');
            AppendRawAccess(access);
            if (modules != null && modules.Length > 0)
                for (var i = 0; i < modules.Length; ++i)
                {
                    stringBuilder.Append(tab2).Append(modules[i]);
                    stringBuilder.Append(i != modules.Length - 1 ? ",\n" : ";\n");
                }

            text.Add(stringBuilder.ToString());
        }

        public override void VisitUse(string use)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("uses ");
            AppendDescriptor(Internal_Name, use);
            stringBuilder.Append(";\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitProvide(string provide, params string[] providers)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("provides ");
            AppendDescriptor(Internal_Name, provide);
            stringBuilder.Append(" with\n");
            for (var i = 0; i < providers.Length; ++i)
            {
                stringBuilder.Append(tab2);
                AppendDescriptor(Internal_Name, providers[i]);
                stringBuilder.Append(i != providers.Length - 1 ? ",\n" : ";\n");
            }

            text.Add(stringBuilder.ToString());
        }

        public override void VisitModuleEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Annotations
        // -----------------------------------------------------------------------------------------------
        // DontCheck(OverloadMethodsDeclarationOrder): overloads are semantically different.
        public override void Visit(string name, object value)
        {
            VisitAnnotationValue(name);
            if (value is string)
            {
                VisitString((string) value);
            }
            else if (value is Type)
            {
                VisitType((Type) value);
            }
            else if (value is byte)
            {
                VisitByte((byte) value);
            }
            else if (value is bool)
            {
                VisitBoolean((bool) value);
            }
            else if (value is short)
            {
                VisitShort((short) value);
            }
            else if (value is char)
            {
                VisitChar((char) value);
            }
            else if (value is int)
            {
                VisitInt((int) value);
            }
            else if (value is float)
            {
                VisitFloat((float) value);
            }
            else if (value is long)
            {
                VisitLong((long) value);
            }
            else if (value is double)
            {
                VisitDouble((double) value);
            }
            else if (value.GetType().IsArray)
            {
                stringBuilder.Append('{');
                if (value is byte[])
                {
                    var byteArray = (byte[]) value;
                    for (var i = 0; i < byteArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitByte(byteArray[i]);
                    }
                }
                else if (value is bool[])
                {
                    var booleanArray = (bool[]) value;
                    for (var i = 0; i < booleanArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitBoolean(booleanArray[i]);
                    }
                }
                else if (value is short[])
                {
                    var shortArray = (short[]) value;
                    for (var i = 0; i < shortArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitShort(shortArray[i]);
                    }
                }
                else if (value is char[])
                {
                    var charArray = (char[]) value;
                    for (var i = 0; i < charArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitChar(charArray[i]);
                    }
                }
                else if (value is int[])
                {
                    var intArray = (int[]) value;
                    for (var i = 0; i < intArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitInt(intArray[i]);
                    }
                }
                else if (value is long[])
                {
                    var longArray = (long[]) value;
                    for (var i = 0; i < longArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitLong(longArray[i]);
                    }
                }
                else if (value is float[])
                {
                    var floatArray = (float[]) value;
                    for (var i = 0; i < floatArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitFloat(floatArray[i]);
                    }
                }
                else if (value is double[])
                {
                    var doubleArray = (double[]) value;
                    for (var i = 0; i < doubleArray.Length; i++)
                    {
                        MaybeAppendComma(i);
                        VisitDouble(doubleArray[i]);
                    }
                }

                stringBuilder.Append('}');
            }

            text.Add(stringBuilder.ToString());
        }

        private void VisitInt(int value)
        {
            stringBuilder.Append(value);
        }

        private void VisitLong(long value)
        {
            stringBuilder.Append(value).Append('L');
        }

        private void VisitFloat(float value)
        {
            stringBuilder.Append(value).Append('F');
        }

        private void VisitDouble(double value)
        {
            stringBuilder.Append(value).Append('D');
        }

        private void VisitChar(char value)
        {
            stringBuilder.Append("(char)").Append((int) value);
        }

        private void VisitShort(short value)
        {
            stringBuilder.Append("(short)").Append(value);
        }

        private void VisitByte(byte value)
        {
            stringBuilder.Append("(byte)").Append(value);
        }

        private void VisitBoolean(bool value)
        {
            stringBuilder.Append(value);
        }

        private void VisitString(string value)
        {
            AppendString(stringBuilder, value);
        }

        private void VisitType(Type value)
        {
            stringBuilder.Append(value.GetClassName()).Append(Class_Suffix);
        }

        public override void VisitEnum(string name, string descriptor, string value)
        {
            VisitAnnotationValue(name);
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('.').Append(value);
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitAnnotation(string name, string descriptor)
        {
            VisitAnnotationValue(name);
            stringBuilder.Append('@');
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('(');
            text.Add(stringBuilder.ToString());
            return AddNewTextifier(")");
        }

        public override Printer VisitArray(string name)
        {
            VisitAnnotationValue(name);
            stringBuilder.Append('{');
            text.Add(stringBuilder.ToString());
            return AddNewTextifier("}");
        }

        public override void VisitAnnotationEnd()
        {
        }

        // Nothing to do.
        private void VisitAnnotationValue(string name)
        {
            stringBuilder.Length = 0;
            MaybeAppendComma(numAnnotationValues++);
            if (name != null) stringBuilder.Append(name).Append('=');
        }

        // -----------------------------------------------------------------------------------------------
        // Record components
        // -----------------------------------------------------------------------------------------------
        public override Printer VisitRecordComponentAnnotationExperimental(string descriptor
            , bool visible)
        {
            return VisitAnnotation(descriptor, visible);
        }

        public override Printer VisitRecordComponentTypeAnnotationExperimental(int typeRef
            , TypePath typePath, string descriptor, bool visible)
        {
            return VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        public override void VisitRecordComponentAttributeExperimental(Attribute attribute
        )
        {
            VisitAttribute(attribute);
        }

        public override void VisitRecordComponentEndExperimental()
        {
        }

        // Nothing to do.
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
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------
        public override void VisitParameter(string name, int access)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("// parameter ");
            AppendAccess(access);
            stringBuilder.Append(' ').Append(name == null ? "<no name>" : name).Append('\n'
            );
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitAnnotationDefault()
        {
            text.Add(tab2 + "default=");
            return AddNewTextifier("\n");
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
            stringBuilder.Append(tab2).Append("// annotable parameter count: ");
            stringBuilder.Append(parameterCount);
            stringBuilder.Append(visible ? " (visible)\n" : " (invisible)\n");
            text.Add(stringBuilder.ToString());
            return this;
        }

        public override Printer VisitParameterAnnotation(int parameter, string descriptor
            , bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append('@');
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('(');
            text.Add(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append(visible ? ") // parameter " : ") // invisible, parameter ").Append(parameter)
                .Append('\n');
            return AddNewTextifier(stringBuilder.ToString());
        }

        public override void VisitMethodAttribute(Attribute attribute)
        {
            VisitAttribute(attribute);
        }

        public override void VisitCode()
        {
        }

        // Nothing to do.
        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(ltab);
            stringBuilder.Append("FRAME ");
            switch (type)
            {
                case OpcodesConstants.F_New:
                case OpcodesConstants.F_Full:
                {
                    stringBuilder.Append("FULL [");
                    AppendFrameTypes(numLocal, local);
                    stringBuilder.Append("] [");
                    AppendFrameTypes(numStack, stack);
                    stringBuilder.Append(']');
                    break;
                }

                case OpcodesConstants.F_Append:
                {
                    stringBuilder.Append("APPEND [");
                    AppendFrameTypes(numLocal, local);
                    stringBuilder.Append(']');
                    break;
                }

                case OpcodesConstants.F_Chop:
                {
                    stringBuilder.Append("CHOP ").Append(numLocal);
                    break;
                }

                case OpcodesConstants.F_Same:
                {
                    stringBuilder.Append("SAME");
                    break;
                }

                case OpcodesConstants.F_Same1:
                {
                    stringBuilder.Append("SAME1 ");
                    AppendFrameTypes(1, stack);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }

            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitInsn(int opcode)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append(' ').Append(opcode == OpcodesConstants
                                                                                      .Newarray
                ? Types[operand]
                : operand.ToString()).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append(' ').Append(var).Append
                ('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append(' ');
            AppendDescriptor(Internal_Name, type);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append(' ');
            AppendDescriptor(Internal_Name, owner);
            stringBuilder.Append('.').Append(name).Append(" : ");
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMethodInsn(int opcode, string owner, string name, string
            descriptor, bool isInterface)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append(' ');
            AppendDescriptor(Internal_Name, owner);
            stringBuilder.Append('.').Append(name).Append(' ');
            AppendDescriptor(Method_Descriptor, descriptor);
            if (isInterface) stringBuilder.Append(" (itf)");
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("INVOKEDYNAMIC").Append(' ');
            stringBuilder.Append(name);
            AppendDescriptor(Method_Descriptor, descriptor);
            stringBuilder.Append(" [");
            stringBuilder.Append('\n');
            stringBuilder.Append(tab3);
            AppendHandle(bootstrapMethodHandle);
            stringBuilder.Append('\n');
            stringBuilder.Append(tab3).Append("// arguments:");
            if (bootstrapMethodArguments.Length == 0)
            {
                stringBuilder.Append(" none");
            }
            else
            {
                stringBuilder.Append('\n');
                foreach (var value in bootstrapMethodArguments)
                {
                    stringBuilder.Append(tab3);
                    if (value is string)
                    {
                        AppendString(stringBuilder, (string) value);
                    }
                    else if (value is Type)
                    {
                        var type = (Type) value;
                        if (type.GetSort() == Type.Method)
                            AppendDescriptor(Method_Descriptor, type.GetDescriptor());
                        else
                            VisitType(type);
                    }
                    else if (value is Handle)
                    {
                        AppendHandle((Handle) value);
                    }
                    else
                    {
                        stringBuilder.Append(value);
                    }

                    stringBuilder.Append(", \n");
                }

                stringBuilder.Length = stringBuilder.Length - 3;
            }

            stringBuilder.Append('\n');
            stringBuilder.Append(tab2).Append("]\n");
            text.Add(stringBuilder.ToString());
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append(Opcodes[opcode]).Append(' ');
            AppendLabel(label);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitLabel(Label label)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(ltab);
            AppendLabel(label);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitLdcInsn(object value)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("LDC ");
            if (value is string)
                AppendString(stringBuilder, (string) value);
            else if (value is Type)
                stringBuilder.Append(((Type) value).GetDescriptor()).Append(Class_Suffix);
            else
                stringBuilder.Append(value);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitIincInsn(int var, int increment)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("IINC ").Append(var).Append(' ').Append(increment
            ).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("TABLESWITCH\n");
            for (var i = 0; i < labels.Length; ++i)
            {
                stringBuilder.Append(tab3).Append(min + i).Append(": ");
                AppendLabel(labels[i]);
                stringBuilder.Append('\n');
            }

            stringBuilder.Append(tab3).Append("default: ");
            AppendLabel(dflt);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("LOOKUPSWITCH\n");
            for (var i = 0; i < labels.Length; ++i)
            {
                stringBuilder.Append(tab3).Append(keys[i]).Append(": ");
                AppendLabel(labels[i]);
                stringBuilder.Append('\n');
            }

            stringBuilder.Append(tab3).Append("default: ");
            AppendLabel(dflt);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("MULTIANEWARRAY ");
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append(' ').Append(numDimensions).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitInsnAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            return VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("TRYCATCHBLOCK ");
            AppendLabel(start);
            stringBuilder.Append(' ');
            AppendLabel(end);
            stringBuilder.Append(' ');
            AppendLabel(handler);
            stringBuilder.Append(' ');
            AppendDescriptor(Internal_Name, type);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitTryCatchAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("TRYCATCHBLOCK @");
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('(');
            text.Add(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append(") : ");
            AppendTypeReference(typeRef);
            stringBuilder.Append(", ").Append(typePath);
            stringBuilder.Append(visible ? "\n" : Invisible);
            return AddNewTextifier(stringBuilder.ToString());
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("LOCALVARIABLE ").Append(name).Append(' ');
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append(' ');
            AppendLabel(start);
            stringBuilder.Append(' ');
            AppendLabel(end);
            stringBuilder.Append(' ').Append(index).Append('\n');
            if (signature != null)
            {
                stringBuilder.Append(tab2);
                AppendDescriptor(Field_Signature, signature);
                stringBuilder.Append(tab2);
                AppendJavaDeclaration(name, signature);
            }

            text.Add(stringBuilder.ToString());
        }

        public override Printer VisitLocalVariableAnnotation(int typeRef, TypePath typePath
            , Label[] start, Label[] end, int[] index, string descriptor, bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("LOCALVARIABLE @");
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('(');
            text.Add(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append(") : ");
            AppendTypeReference(typeRef);
            stringBuilder.Append(", ").Append(typePath);
            for (var i = 0; i < start.Length; ++i)
            {
                stringBuilder.Append(" [ ");
                AppendLabel(start[i]);
                stringBuilder.Append(" - ");
                AppendLabel(end[i]);
                stringBuilder.Append(" - ").Append(index[i]).Append(" ]");
            }

            stringBuilder.Append(visible ? "\n" : Invisible);
            return AddNewTextifier(stringBuilder.ToString());
        }

        public override void VisitLineNumber(int line, Label start)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("LINENUMBER ").Append(line).Append(' ');
            AppendLabel(start);
            stringBuilder.Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("MAXSTACK = ").Append(maxStack).Append('\n');
            text.Add(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append(tab2).Append("MAXLOCALS = ").Append(maxLocals).Append('\n');
            text.Add(stringBuilder.ToString());
        }

        public override void VisitMethodEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Common methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Prints a disassembled view of the given annotation.</summary>
        /// <param name="descriptor">the class descriptor of the annotation class.</param>
        /// <param name="visible">
        ///     <literal>true</literal>
        ///     if the annotation is visible at runtime.
        /// </param>
        /// <returns>a visitor to visit the annotation values.</returns>
        public virtual Textifier VisitAnnotation(string descriptor, bool visible)
        {
            // DontCheck(OverloadMethodsDeclarationOrder): overloads are semantically different.
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append('@');
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('(');
            text.Add(stringBuilder.ToString());
            return AddNewTextifier(visible ? ")\n" : ") // invisible\n");
        }

        /// <summary>Prints a disassembled view of the given type annotation.</summary>
        /// <param name="typeRef">
        ///     a reference to the annotated type. See
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
        /// <returns>a visitor to visit the annotation values.</returns>
        public virtual Textifier VisitTypeAnnotation(int typeRef, TypePath typePath, string
            descriptor, bool visible)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append('@');
            AppendDescriptor(Field_Descriptor, descriptor);
            stringBuilder.Append('(');
            text.Add(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append(") : ");
            AppendTypeReference(typeRef);
            stringBuilder.Append(", ").Append(typePath);
            stringBuilder.Append(visible ? "\n" : Invisible);
            return AddNewTextifier(stringBuilder.ToString());
        }

        /// <summary>Prints a disassembled view of the given attribute.</summary>
        /// <param name="attribute">an attribute.</param>
        public virtual void VisitAttribute(Attribute attribute)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(tab).Append("ATTRIBUTE ");
            AppendDescriptor(-1, attribute.type);
            if (attribute is TextifierSupport)
            {
                if (labelNames == null) labelNames = new Dictionary<Label, string>();
                ((TextifierSupport) attribute).Textify(stringBuilder, labelNames);
            }
            else
            {
                stringBuilder.Append(" : unknown\n");
            }

            text.Add(stringBuilder.ToString());
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Appends a string representation of the given access flags to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="accessFlags">some access flags.</param>
        private void AppendAccess(int accessFlags)
        {
            if ((accessFlags & OpcodesConstants.Acc_Public) != 0) stringBuilder.Append("public ");
            if ((accessFlags & OpcodesConstants.Acc_Private) != 0) stringBuilder.Append("private ");
            if ((accessFlags & OpcodesConstants.Acc_Protected) != 0) stringBuilder.Append("protected ");
            if ((accessFlags & OpcodesConstants.Acc_Final) != 0) stringBuilder.Append("final ");
            if ((accessFlags & OpcodesConstants.Acc_Static) != 0) stringBuilder.Append("static ");
            if ((accessFlags & OpcodesConstants.Acc_Synchronized) != 0) stringBuilder.Append("synchronized ");
            if ((accessFlags & OpcodesConstants.Acc_Volatile) != 0) stringBuilder.Append("volatile ");
            if ((accessFlags & OpcodesConstants.Acc_Transient) != 0) stringBuilder.Append("transient ");
            if ((accessFlags & OpcodesConstants.Acc_Abstract) != 0) stringBuilder.Append("abstract ");
            if ((accessFlags & OpcodesConstants.Acc_Strict) != 0) stringBuilder.Append("strictfp ");
            if ((accessFlags & OpcodesConstants.Acc_Synthetic) != 0) stringBuilder.Append("synthetic ");
            if ((accessFlags & OpcodesConstants.Acc_Mandated) != 0) stringBuilder.Append("mandated ");
            if ((accessFlags & OpcodesConstants.Acc_Enum) != 0) stringBuilder.Append("enum ");
        }

        /// <summary>
        ///     Appends the hexadecimal value of the given access flags to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="accessFlags">some access flags.</param>
        private void AppendRawAccess(int accessFlags)
        {
            stringBuilder.Append("// access flags 0x").Append(accessFlags.ToString("x8").ToUpper
                ()).Append('\n');
        }

        /// <summary>
        ///     Appends an internal name, a type descriptor or a type signature to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="type">
        ///     the type of 'value'. Must be one of
        ///     <see cref="Internal_Name" />
        ///     ,
        ///     <see cref="Field_Descriptor" />
        ///     ,
        ///     <see cref="Field_Signature" />
        ///     ,
        ///     <see cref="Method_Descriptor" />
        ///     ,
        ///     <see cref="Method_Signature" />
        ///     ,
        ///     <see cref="Class_Signature" />
        ///     or
        ///     <see cref="Handle_Descriptor" />
        ///     .
        /// </param>
        /// <param name="value">
        ///     an internal name, type descriptor or a type signature. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        protected internal virtual void AppendDescriptor(int type, string value)
        {
            if (type == Class_Signature || type == Field_Signature || type == Method_Signature)
            {
                if (value != null) stringBuilder.Append("// signature ").Append(value).Append('\n');
            }
            else
            {
                stringBuilder.Append(value);
            }
        }

        /// <summary>
        ///     Appends the Java generic type declaration corresponding to the given signature.
        /// </summary>
        /// <param name="name">a class, field or method name.</param>
        /// <param name="signature">a class, field or method signature.</param>
        private void AppendJavaDeclaration(string name, string signature)
        {
            var traceSignatureVisitor = new TraceSignatureVisitor(access);
            new SignatureReader(signature).Accept(traceSignatureVisitor);
            stringBuilder.Append("// declaration: ");
            if (traceSignatureVisitor.GetReturnType() != null)
            {
                stringBuilder.Append(traceSignatureVisitor.GetReturnType());
                stringBuilder.Append(' ');
            }

            stringBuilder.Append(name);
            stringBuilder.Append(traceSignatureVisitor.GetDeclaration());
            if (traceSignatureVisitor.GetExceptions() != null)
                stringBuilder.Append(" throws ").Append(traceSignatureVisitor.GetExceptions());
            stringBuilder.Append('\n');
        }

        /// <summary>
        ///     Appends the name of the given label to
        ///     <see cref="Printer.stringBuilder" />
        ///     . Constructs a new label name if
        ///     the given label does not yet have one.
        /// </summary>
        /// <param name="label">a label.</param>
        protected internal virtual void AppendLabel(Label label)
        {
            if (labelNames == null) labelNames = new Dictionary<Label, string>();
            var name = labelNames.GetOrNull(label);
            if (name == null)
            {
                name = "L" + labelNames.Count;
                Collections.Put(labelNames, label, name);
            }

            stringBuilder.Append(name);
        }

        /// <summary>
        ///     Appends a string representation of the given handle to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="handle">a handle.</param>
        protected internal virtual void AppendHandle(Handle handle)
        {
            var tag = handle.GetTag();
            stringBuilder.Append("// handle kind 0x").Append(tag.ToString("x8")).Append(" : "
            );
            var isMethodHandle = false;
            switch (tag)
            {
                case OpcodesConstants.H_Getfield:
                {
                    stringBuilder.Append("GETFIELD");
                    break;
                }

                case OpcodesConstants.H_Getstatic:
                {
                    stringBuilder.Append("GETSTATIC");
                    break;
                }

                case OpcodesConstants.H_Putfield:
                {
                    stringBuilder.Append("PUTFIELD");
                    break;
                }

                case OpcodesConstants.H_Putstatic:
                {
                    stringBuilder.Append("PUTSTATIC");
                    break;
                }

                case OpcodesConstants.H_Invokeinterface:
                {
                    stringBuilder.Append("INVOKEINTERFACE");
                    isMethodHandle = true;
                    break;
                }

                case OpcodesConstants.H_Invokespecial:
                {
                    stringBuilder.Append("INVOKESPECIAL");
                    isMethodHandle = true;
                    break;
                }

                case OpcodesConstants.H_Invokestatic:
                {
                    stringBuilder.Append("INVOKESTATIC");
                    isMethodHandle = true;
                    break;
                }

                case OpcodesConstants.H_Invokevirtual:
                {
                    stringBuilder.Append("INVOKEVIRTUAL");
                    isMethodHandle = true;
                    break;
                }

                case OpcodesConstants.H_Newinvokespecial:
                {
                    stringBuilder.Append("NEWINVOKESPECIAL");
                    isMethodHandle = true;
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }

            stringBuilder.Append('\n');
            stringBuilder.Append(tab3);
            AppendDescriptor(Internal_Name, handle.GetOwner());
            stringBuilder.Append('.');
            stringBuilder.Append(handle.GetName());
            if (!isMethodHandle) stringBuilder.Append('(');
            AppendDescriptor(Handle_Descriptor, handle.GetDesc());
            if (!isMethodHandle) stringBuilder.Append(')');
            if (handle.IsInterface()) stringBuilder.Append(" itf");
        }

        /// <summary>
        ///     Appends a comma to
        ///     <see cref="Printer.stringBuilder" />
        ///     if the given number is strictly positive.
        /// </summary>
        /// <param name="numValues">
        ///     a number of 'values visited so far', for instance the number of annotation
        ///     values visited so far in an annotation visitor.
        /// </param>
        private void MaybeAppendComma(int numValues)
        {
            if (numValues > 0) stringBuilder.Append(", ");
        }

        /// <summary>
        ///     Appends a string representation of the given type reference to
        ///     <see cref="Printer.stringBuilder" />
        ///     .
        /// </summary>
        /// <param name="typeRef">
        ///     a type reference. See
        ///     <see cref="TypeReference" />
        ///     .
        /// </param>
        private void AppendTypeReference(int typeRef)
        {
            var typeReference = new TypeReference(typeRef);
            switch (typeReference.GetSort())
            {
                case TypeReference.Class_Type_Parameter:
                {
                    stringBuilder.Append("CLASS_TYPE_PARAMETER ").Append(typeReference.GetTypeParameterIndex
                        ());
                    break;
                }

                case TypeReference.Method_Type_Parameter:
                {
                    stringBuilder.Append("METHOD_TYPE_PARAMETER ").Append(typeReference.GetTypeParameterIndex
                        ());
                    break;
                }

                case TypeReference.Class_Extends:
                {
                    stringBuilder.Append("CLASS_EXTENDS ").Append(typeReference.GetSuperTypeIndex());
                    break;
                }

                case TypeReference.Class_Type_Parameter_Bound:
                {
                    stringBuilder.Append("CLASS_TYPE_PARAMETER_BOUND ").Append(typeReference.GetTypeParameterIndex
                        ()).Append(", ").Append(typeReference.GetTypeParameterBoundIndex());
                    break;
                }

                case TypeReference.Method_Type_Parameter_Bound:
                {
                    stringBuilder.Append("METHOD_TYPE_PARAMETER_BOUND ").Append(typeReference.GetTypeParameterIndex
                        ()).Append(", ").Append(typeReference.GetTypeParameterBoundIndex());
                    break;
                }

                case TypeReference.Field:
                {
                    stringBuilder.Append("FIELD");
                    break;
                }

                case TypeReference.Method_Return:
                {
                    stringBuilder.Append("METHOD_RETURN");
                    break;
                }

                case TypeReference.Method_Receiver:
                {
                    stringBuilder.Append("METHOD_RECEIVER");
                    break;
                }

                case TypeReference.Method_Formal_Parameter:
                {
                    stringBuilder.Append("METHOD_FORMAL_PARAMETER ").Append(typeReference.GetFormalParameterIndex
                        ());
                    break;
                }

                case TypeReference.Throws:
                {
                    stringBuilder.Append("THROWS ").Append(typeReference.GetExceptionIndex());
                    break;
                }

                case TypeReference.Local_Variable:
                {
                    stringBuilder.Append("LOCAL_VARIABLE");
                    break;
                }

                case TypeReference.Resource_Variable:
                {
                    stringBuilder.Append("RESOURCE_VARIABLE");
                    break;
                }

                case TypeReference.Exception_Parameter:
                {
                    stringBuilder.Append("EXCEPTION_PARAMETER ").Append(typeReference.GetTryCatchBlockIndex
                        ());
                    break;
                }

                case TypeReference.Instanceof:
                {
                    stringBuilder.Append("INSTANCEOF");
                    break;
                }

                case TypeReference.New:
                {
                    stringBuilder.Append("NEW");
                    break;
                }

                case TypeReference.Constructor_Reference:
                {
                    stringBuilder.Append("CONSTRUCTOR_REFERENCE");
                    break;
                }

                case TypeReference.Method_Reference:
                {
                    stringBuilder.Append("METHOD_REFERENCE");
                    break;
                }

                case TypeReference.Cast:
                {
                    stringBuilder.Append("CAST ").Append(typeReference.GetTypeArgumentIndex());
                    break;
                }

                case TypeReference.Constructor_Invocation_Type_Argument:
                {
                    stringBuilder.Append("CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT ").Append(typeReference
                        .GetTypeArgumentIndex());
                    break;
                }

                case TypeReference.Method_Invocation_Type_Argument:
                {
                    stringBuilder.Append("METHOD_INVOCATION_TYPE_ARGUMENT ").Append(typeReference.GetTypeArgumentIndex
                        ());
                    break;
                }

                case TypeReference.Constructor_Reference_Type_Argument:
                {
                    stringBuilder.Append("CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT ").Append(typeReference
                        .GetTypeArgumentIndex());
                    break;
                }

                case TypeReference.Method_Reference_Type_Argument:
                {
                    stringBuilder.Append("METHOD_REFERENCE_TYPE_ARGUMENT ").Append(typeReference.GetTypeArgumentIndex
                        ());
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
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
                if (i > 0) stringBuilder.Append(' ');
                if (frameTypes[i] is string)
                {
                    var descriptor = (string) frameTypes[i];
                    if (descriptor[0] == '[')
                        AppendDescriptor(Field_Descriptor, descriptor);
                    else
                        AppendDescriptor(Internal_Name, descriptor);
                }
                else if (frameTypes[i] is int)
                {
                    stringBuilder.Append(Frame_Types[(int) frameTypes[i]]);
                }
                else
                {
                    AppendLabel((Label) frameTypes[i]);
                }
            }
        }

        /// <summary>
        ///     Creates and adds to
        ///     <see cref="Printer.text" />
        ///     a new
        ///     <see cref="Textifier" />
        ///     , followed by the given string.
        /// </summary>
        /// <param name="endText">
        ///     the text to add to
        ///     <see cref="Printer.text" />
        ///     after the textifier. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <returns>
        ///     the newly created
        ///     <see cref="Textifier" />
        ///     .
        /// </returns>
        private Textifier AddNewTextifier(string endText)
        {
            var textifier = CreateTextifier();
            text.Add(textifier.GetText());
            if (endText != null) text.Add(endText);
            return textifier;
        }

        /// <summary>
        ///     Creates a new
        ///     <see cref="Textifier" />
        ///     .
        /// </summary>
        /// <returns>
        ///     a new
        ///     <see cref="Textifier" />
        ///     .
        /// </returns>
        protected internal virtual Textifier CreateTextifier()
        {
            return new Textifier(api);
        }
    }
}