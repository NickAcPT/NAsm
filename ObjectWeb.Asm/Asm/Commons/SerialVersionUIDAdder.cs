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
using System.Security.Cryptography;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Misc.Java.IO;
using ObjectWeb.Misc.Java.Nio;

namespace ObjectWeb.Asm.Commons
{
	/// <summary>
	///     A
	///     <see cref="ClassVisitor" />
	///     that adds a serial version unique identifier to a class if missing. A
	///     typical usage of this class is:
	///     <pre>
	///         ClassWriter classWriter = new ClassWriter(...);
	///         ClassVisitor svuidAdder = new SerialVersionUIDAdder(classWriter);
	///         ClassVisitor classVisitor = new MyClassAdapter(svuidAdder);
	///         new ClassReader(orginalClass).accept(classVisitor, 0);
	///     </pre>
	///     <p>
	///         The SVUID algorithm can be found at &lt;a href=
	///         "https://docs.oracle.com/javase/10/docs/specs/serialization/class.html#stream-unique-identifiers"
	///         &gt;https://docs.oracle.com/javase/10/docs/specs/serialization/class.html#stream-unique-identifiers</a>:
	///         <p>
	///             The serialVersionUID is computed using the signature of a stream of bytes that reflect the
	///             class definition. The National Institute of Standards and Technology (NIST) Secure Hash Algorithm
	///             (SHA-1) is used to compute a signature for the stream. The first two 32-bit quantities are used
	///             to form a 64-bit hash. A java.lang.DataOutputStream is used to convert primitive data types to a
	///             sequence of bytes. The values input to the stream are defined by the Java Virtual Machine (VM)
	///             specification for classes.
	///             <p>
	///                 The sequence of items in the stream is as follows:
	///                 <ol>
	///                     <li>
	///                         The class name written using UTF encoding.
	///                         <li>
	///                             The class modifiers written as a 32-bit integer.
	///                             <li>
	///                                 The name of each interface sorted by name written using UTF encoding.
	///                                 <li>
	///                                     For each field of the class sorted by field name (except private static and private
	///                                     transient fields):
	///                                     <ol>
	///                                         <li>
	///                                             The name of the field in UTF encoding.
	///                                             <li>
	///                                                 The modifiers of the field written as a 32-bit integer.
	///                                                 <li>The descriptor of the field in UTF encoding
	///                                     </ol>
	///                                     <li>
	///                                         If a class initializer exists, write out the following:
	///                                         <ol>
	///                                             <li>
	///                                                 The name of the method, &lt;clinit&gt;, in UTF encoding.
	///                                                 <li>
	///                                                     The modifier of the method, STATIC, written as a 32-bit integer.
	///                                                     <li>The descriptor of the method, ()V, in UTF encoding.
	///                                         </ol>
	///                                         <li>
	///                                             For each non-private constructor sorted by method name and signature:
	///                                             <ol>
	///                                                 <li>
	///                                                     The name of the method, &lt;init&gt;, in UTF encoding.
	///                                                     <li>
	///                                                         The modifiers of the method written as a 32-bit integer.
	///                                                         <li>The descriptor of the method in UTF encoding.
	///                                             </ol>
	///                                             <li>
	///                                                 For each non-private method sorted by method name and signature:
	///                                                 <ol>
	///                                                     <li>
	///                                                         The name of the method in UTF encoding.
	///                                                         <li>
	///                                                             The modifiers of the method written as a 32-bit integer.
	///                                                             <li>The descriptor of the method in UTF encoding.
	///                                                 </ol>
	///                                                 <li>
	///                                                     The SHA-1 algorithm is executed on the stream of bytes produced by
	///                                                     DataOutputStream and
	///                                                     produces five 32-bit values sha[0..4].
	///                                                     <li>
	///                                                         The hash value is assembled from the first and second 32-bit
	///                                                         values of the SHA-1 message
	///                                                         digest. If the result of the message digest, the five 32-bit
	///                                                         words H0 H1 H2 H3 H4, is in an
	///                                                         array of five int values named sha, the hash value would be
	///                                                         computed as follows: long hash
	///                                                         = ((sha[0] &gt;&gt;&gt; 24) &amp; 0xFF) | ((sha[0] &gt;&gt;&gt;
	///                                                         16) &amp; 0xFF) &lt;&lt; 8
	///                                                         | ((sha[0] &gt;&gt;&gt; 8) &amp; 0xFF) &lt;&lt; 16 | ((sha[0]
	///                                                         &gt;&gt;&gt; 0) &amp; 0xFF)
	///                                                         &lt;&lt; 24 | ((sha[1] &gt;&gt;&gt; 24) &amp; 0xFF) &lt;&lt; 32
	///                                                         | ((sha[1] &gt;&gt;&gt; 16)
	///                                                         &amp; 0xFF) &lt;&lt; 40 | ((sha[1] &gt;&gt;&gt; 8) &amp; 0xFF)
	///                                                         &lt;&lt; 48 | ((sha[1]
	///                                                         &gt;&gt;&gt; 0) &amp; 0xFF) &lt;&lt; 56;
	///                 </ol>
	/// </summary>
	/// <author>Rajendra Inamdar, Vishal Vishnoi</author>
	public class SerialVersionUIDAdder : ClassVisitor
    {
        /// <summary>The JVM name of static initializer methods.</summary>
        private const string Clinit = "<clinit>";

        /// <summary>The class access flags.</summary>
        private int access;

        /// <summary>A flag that indicates if we need to compute SVUID.</summary>
        private bool computeSvuid;

        /// <summary>Whether the class has a static initializer.</summary>
        private bool hasStaticInitializer;

        /// <summary>Whether the class already has a SVUID.</summary>
        private bool hasSvuid;

        /// <summary>The interfaces implemented by the class.</summary>
        private string[] interfaces;

        /// <summary>The internal name of the class.</summary>
        private string name;

        /// <summary>The constructors of the class that are needed to compute the SVUID.</summary>
        private ICollection<Item> svuidConstructors;

        /// <summary>The fields of the class that are needed to compute the SVUID.</summary>
        private ICollection<Item> svuidFields;

        /// <summary>The methods of the class that are needed to compute the SVUID.</summary>
        private ICollection<Item> svuidMethods;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SerialVersionUIDAdder" />
        ///     .
        ///     <i>
        ///         Subclasses must not use this
        ///         constructor
        ///     </i>
        ///     . Instead, they must use the
        ///     <see cref="SerialVersionUIDAdder(int,ObjectWeb.Asm.ClassVisitor)" />
        ///     version.
        /// </summary>
        /// <param name="classVisitor">
        ///     a
        ///     <see cref="ClassVisitor" />
        ///     to which this visitor will delegate calls.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public SerialVersionUIDAdder(ClassVisitor classVisitor)
            : this(VisitorAsmApiVersion.Asm7, classVisitor)
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            /* latest api = */
            if (GetType() != typeof(SerialVersionUIDAdder)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SerialVersionUIDAdder" />
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
        ///     a
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor" />
        ///     to which this visitor will delegate calls.
        /// </param>
        protected internal SerialVersionUIDAdder(VisitorAsmApiVersion api, ClassVisitor classVisitor)
            : base(api, classVisitor)
        {
        }

        // -----------------------------------------------------------------------------------------------
        // Overridden methods
        // -----------------------------------------------------------------------------------------------
        public override void Visit(int version, int access, string name, string signature
            , string superName, string[] interfaces)
        {
            // Get the class name, access flags, and interfaces information (step 1, 2 and 3) for SVUID
            // computation.
            computeSvuid = (access & OpcodesConstants.Acc_Enum) == 0;
            if (computeSvuid)
            {
                this.name = name;
                this.access = access;
                this.interfaces = (string[]) interfaces.Clone();
                svuidFields = new List<Item>();
                svuidConstructors = new List<Item>();
                svuidMethods = new List<Item>();
            }

            base.Visit(version, access, name, signature, superName, interfaces);
        }

        public override MethodVisitor VisitMethod(int access, string name, string descriptor
            , string signature, string[] exceptions)
        {
            // Get constructor and method information (step 5 and 7). Also determine if there is a class
            // initializer (step 6).
            if (computeSvuid)
            {
                if (Clinit.Equals(name)) hasStaticInitializer = true;
                // Collect the non private constructors and methods. Only the ACC_PUBLIC, ACC_PRIVATE,
                // ACC_PROTECTED, ACC_STATIC, ACC_FINAL, ACC_SYNCHRONIZED, ACC_NATIVE, ACC_ABSTRACT and
                // ACC_STRICT flags are used.
                var mods = access & (OpcodesConstants.Acc_Public | OpcodesConstants.Acc_Private |
                                     OpcodesConstants.Acc_Protected | OpcodesConstants.Acc_Static | OpcodesConstants
                                         .Acc_Final | OpcodesConstants.Acc_Synchronized | OpcodesConstants.Acc_Native |
                                     OpcodesConstants
                                         .Acc_Abstract | OpcodesConstants.Acc_Strict);
                if ((access & OpcodesConstants.Acc_Private) == 0)
                {
                    if ("<init>".Equals(name))
                        svuidConstructors.Add(new Item(name, mods, descriptor));
                    else if (!Clinit.Equals(name)) svuidMethods.Add(new Item(name, mods, descriptor));
                }
            }

            return base.VisitMethod(access, name, descriptor, signature, exceptions);
        }

        public override FieldVisitor VisitField(int access, string name, string desc, string
            signature, object value)
        {
            // Get the class field information for step 4 of the algorithm. Also determine if the class
            // already has a SVUID.
            if (computeSvuid)
            {
                if ("serialVersionUID".Equals(name))
                {
                    // Since the class already has SVUID, we won't be computing it.
                    computeSvuid = false;
                    hasSvuid = true;
                }

                // Collect the non private fields. Only the ACC_PUBLIC, ACC_PRIVATE, ACC_PROTECTED,
                // ACC_STATIC, ACC_FINAL, ACC_VOLATILE, and ACC_TRANSIENT flags are used when computing
                // serialVersionUID values.
                if ((access & OpcodesConstants.Acc_Private) == 0 || (access & (OpcodesConstants.Acc_Static
                                                                               | OpcodesConstants.Acc_Transient)) == 0)
                {
                    var mods = access & (OpcodesConstants.Acc_Public | OpcodesConstants.Acc_Private |
                                         OpcodesConstants.Acc_Protected | OpcodesConstants.Acc_Static | OpcodesConstants
                                             .Acc_Final | OpcodesConstants.Acc_Volatile |
                                         OpcodesConstants.Acc_Transient);
                    svuidFields.Add(new Item(name, mods, desc));
                }
            }

            return base.VisitField(access, name, desc, signature, value);
        }

        public override void VisitInnerClass(string innerClassName, string outerName, string
            innerName, int innerClassAccess)
        {
            // Handles a bizarre special case. Nested classes (static classes declared inside another class)
            // that are protected have their access bit set to public in their class files to deal with some
            // odd reflection situation. Our SVUID computation must do as the JVM does and ignore access
            // bits in the class file in favor of the access bits of the InnerClass attribute.
            if (name != null && name.Equals(innerClassName)) access = innerClassAccess;
            base.VisitInnerClass(innerClassName, outerName, innerName, innerClassAccess);
        }

        public override void VisitEnd()
        {
            // Add the SVUID field to the class if it doesn't have one.
            if (computeSvuid && !hasSvuid)
                try
                {
                    AddSVUID(ComputeSVUID());
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Error while computing SVUID for " + name, e);
                }

            base.VisitEnd();
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns true if the class already has a SVUID field.</summary>
        /// <remarks>
        ///     Returns true if the class already has a SVUID field. The result of this method is only valid
        ///     when visitEnd has been called.
        /// </remarks>
        /// <returns>true if the class already has a SVUID field.</returns>
        public virtual bool HasSVUID()
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            return hasSvuid;
        }

        /// <summary>
        ///     Adds a final static serialVersionUID field to the class, with the given value.
        /// </summary>
        /// <param name="svuid">the serialVersionUID field value.</param>
        protected internal virtual void AddSVUID(long svuid)
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            var fieldVisitor = base.VisitField(OpcodesConstants.Acc_Final + OpcodesConstants
                                                   .Acc_Static, "serialVersionUID", "J", null, svuid);
            if (fieldVisitor != null) fieldVisitor.VisitEnd();
        }

        /// <summary>Computes and returns the value of SVUID.</summary>
        /// <returns>the serial version UID.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        protected internal virtual long ComputeSVUID()
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            long svuid = 0;
            using (var byteArrayOutputStream = new MemoryStream())
            {
                using (var dataOutputStream = new DataOutputStream(byteArrayOutputStream.ToOutputStream()
                ))
                {
                    // 1. The class name written using UTF encoding.
                    dataOutputStream.WriteUTF(name.Replace('/', '.'));
                    // 2. The class modifiers written as a 32-bit integer.
                    var mods = access;
                    if ((mods & OpcodesConstants.Acc_Interface) != 0)
                        mods = svuidMethods.Count == 0
                            ? mods & ~OpcodesConstants.Acc_Abstract
                            : mods
                              | OpcodesConstants.Acc_Abstract;
                    dataOutputStream.WriteInt(mods & (OpcodesConstants.Acc_Public | OpcodesConstants.Acc_Final |
                                                      OpcodesConstants.Acc_Interface | OpcodesConstants.Acc_Abstract));
                    // 3. The name of each interface sorted by name written using UTF encoding.
                    Array.Sort(interfaces);
                    foreach (var interfaceName in interfaces)
                        dataOutputStream.WriteUTF(interfaceName.Replace('/', '.'));
                    // 4. For each field of the class sorted by field name (except private static and private
                    // transient fields):
                    //   1. The name of the field in UTF encoding.
                    //   2. The modifiers of the field written as a 32-bit integer.
                    //   3. The descriptor of the field in UTF encoding.
                    // Note that field signatures are not dot separated. Method and constructor signatures are dot
                    // separated. Go figure...
                    WriteItems(svuidFields, dataOutputStream, false);
                    // 5. If a class initializer exists, write out the following:
                    //   1. The name of the method, <clinit>, in UTF encoding.
                    //   2. The modifier of the method, ACC_STATIC, written as a 32-bit integer.
                    //   3. The descriptor of the method, ()V, in UTF encoding.
                    if (hasStaticInitializer)
                    {
                        dataOutputStream.WriteUTF(Clinit);
                        dataOutputStream.WriteInt(OpcodesConstants.Acc_Static);
                        dataOutputStream.WriteUTF("()V");
                    }

                    // 6. For each non-private constructor sorted by method name and signature:
                    //   1. The name of the method, <init>, in UTF encoding.
                    //   2. The modifiers of the method written as a 32-bit integer.
                    //   3. The descriptor of the method in UTF encoding.
                    WriteItems(svuidConstructors, dataOutputStream, true);
                    // 7. For each non-private method sorted by method name and signature:
                    //   1. The name of the method in UTF encoding.
                    //   2. The modifiers of the method written as a 32-bit integer.
                    //   3. The descriptor of the method in UTF encoding.
                    WriteItems(svuidMethods, dataOutputStream, true);
                    dataOutputStream.Flush();
                    // 8. The SHA-1 algorithm is executed on the stream of bytes produced by DataOutputStream and
                    // produces five 32-bit values sha[0..4].
                    var hashBytes = ComputeSHAdigest(byteArrayOutputStream.ToArray());
                    // 9. The hash value is assembled from the first and second 32-bit values of the SHA-1 message
                    // digest. If the result of the message digest, the five 32-bit words H0 H1 H2 H3 H4, is in an
                    // array of five int values named sha, the hash value would be computed as follows:
                    for (var i = Math.Min(hashBytes.Length, 8) - 1; i >= 0; i--)
                        svuid = (svuid << 8) | (hashBytes[i] & 0xFF);
                }
            }

            return svuid;
        }

        /// <summary>Returns the SHA-1 message digest of the given value.</summary>
        /// <param name="value">the value whose SHA message digest must be computed.</param>
        /// <returns>the SHA-1 message digest of the given value.</returns>
        protected internal virtual byte[] ComputeSHAdigest(byte[] value)
        {
            // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
            return SHA1.Create().ComputeHash(value);
        }

        /// <summary>
        ///     Sorts the items in the collection and writes it to the given output stream.
        /// </summary>
        /// <param name="itemCollection">a collection of items.</param>
        /// <param name="dataOutputStream">where the items must be written.</param>
        /// <param name="dotted">whether package names must use dots, instead of slashes.</param>
        /// <exception>
        ///     IOException
        ///     if an error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        private static void WriteItems(ICollection<Item> itemCollection
            , DataOutput dataOutputStream, bool dotted)
        {
            var items = Collections.ToArray(itemCollection,
                new Item[0]);
            Array.Sort(items);
            foreach (var item in items)
            {
                dataOutputStream.WriteUTF(item.name);
                dataOutputStream.WriteInt(item.access);
                dataOutputStream.WriteUTF(dotted ? item.descriptor.Replace('/', '.') : item.descriptor
                );
            }
        }

        private sealed class Item : IComparable<Item>
        {
            internal readonly int access;

            internal readonly string descriptor;
            internal readonly string name;

            internal Item(string name, int access, string descriptor)
            {
                // -----------------------------------------------------------------------------------------------
                // Inner classes
                // -----------------------------------------------------------------------------------------------
                this.name = name;
                this.access = access;
                this.descriptor = descriptor;
            }

            public int CompareTo(Item item)
            {
                var result = string.CompareOrdinal(name, item.name);
                if (result == 0) result = string.CompareOrdinal(descriptor, item.descriptor);
                return result;
            }

            public override bool Equals(object other)
            {
                if (other is Item) return CompareTo((Item) other) == 0;
                return false;
            }

            public override int GetHashCode()
            {
                return name.GetHashCode() ^ descriptor.GetHashCode();
            }
        }
    }
}