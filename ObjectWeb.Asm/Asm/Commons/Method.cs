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
using System.Reflection;
using System.Text;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>A named method descriptor.</summary>
    /// <author>Juozas Baliuka</author>
    /// <author>Chris Nokleberg</author>
    /// <author>Eric Bruneton</author>
    public class Method
    {
        /// <summary>The descriptors of the primitive Java types (plus void).</summary>
        private static readonly IDictionary<string, string> Primitive_Type_Descriptors;

        /// <summary>The method descriptor.</summary>
        private readonly string descriptor;

        /// <summary>The method name.</summary>
        private readonly string name;

        static Method()
        {
            var descriptors = new Dictionary<string, string>();
            Collections.Put(descriptors, "void", "V");
            Collections.Put(descriptors, "byte", "B");
            Collections.Put(descriptors, "char", "C");
            Collections.Put(descriptors, "double", "D");
            Collections.Put(descriptors, "float", "F");
            Collections.Put(descriptors, "int", "I");
            Collections.Put(descriptors, "long", "J");
            Collections.Put(descriptors, "short", "S");
            Collections.Put(descriptors, "boolean", "Z");
            Primitive_Type_Descriptors = descriptors;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Method" />
        ///     .
        /// </summary>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">the method's descriptor.</param>
        public Method(string name, string descriptor)
        {
            this.name = name;
            this.descriptor = descriptor;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="Method" />
        ///     .
        /// </summary>
        /// <param name="name">the method's name.</param>
        /// <param name="returnType">the method's return type.</param>
        /// <param name="argumentTypes">the method's argument types.</param>
        public Method(string name, Type returnType, Type[] argumentTypes)
            : this(name, Type.GetMethodDescriptor(returnType, argumentTypes))
        {
        }

        /// <summary>
        ///     Creates a new
        ///     <see cref="Method" />
        ///     .
        /// </summary>
        /// <param name="method">a java.lang.reflect method descriptor</param>
        /// <returns>
        ///     a
        ///     <see cref="Method" />
        ///     corresponding to the given Java method declaration.
        /// </returns>
        public static Method GetMethod(MethodInfo method)
        {
            return new Method(method.Name, Type.GetMethodDescriptor(method));
        }

        /// <summary>
        ///     Creates a new
        ///     <see cref="Method" />
        ///     .
        /// </summary>
        /// <param name="constructor">a java.lang.reflect constructor descriptor</param>
        /// <returns>
        ///     a
        ///     <see cref="Method" />
        ///     corresponding to the given Java constructor declaration.
        /// </returns>
        public static Method GetMethod(ConstructorInfo constructor)
        {
            return new Method("<init>", Type.GetConstructorDescriptor(constructor));
        }

        /// <summary>
        ///     Returns a
        ///     <see cref="Method" />
        ///     corresponding to the given Java method declaration.
        /// </summary>
        /// <param name="method">
        ///     a Java method declaration, without argument names, of the form "returnType name
        ///     (argumentType1, ... argumentTypeN)", where the types are in plain Java (e.g. "int",
        ///     "float", "java.util.List", ...). Classes of the java.lang package can be specified by their
        ///     unqualified name; all other classes names must be fully qualified.
        /// </param>
        /// <returns>
        ///     a
        ///     <see cref="Method" />
        ///     corresponding to the given Java method declaration.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     if <code>method</code> could not get parsed.
        /// </exception>
        public static Method GetMethod(string method)
        {
            return GetMethod(method, false);
        }

        /// <summary>
        ///     Returns a
        ///     <see cref="Method" />
        ///     corresponding to the given Java method declaration.
        /// </summary>
        /// <param name="method">
        ///     a Java method declaration, without argument names, of the form "returnType name
        ///     (argumentType1, ... argumentTypeN)", where the types are in plain Java (e.g. "int",
        ///     "float", "java.util.List", ...). Classes of the java.lang package may be specified by their
        ///     unqualified name, depending on the defaultPackage argument; all other classes names must be
        ///     fully qualified.
        /// </param>
        /// <param name="defaultPackage">
        ///     true if unqualified class names belong to the default package, or false
        ///     if they correspond to java.lang classes. For instance "Object" means "Object" if this
        ///     option is true, or "java.lang.Object" otherwise.
        /// </param>
        /// <returns>
        ///     a
        ///     <see cref="Method" />
        ///     corresponding to the given Java method declaration.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     if <code>method</code> could not get parsed.
        /// </exception>
        public static Method GetMethod(string method, bool defaultPackage)
        {
            var spaceIndex = method.IndexOf(' ');
            var currentArgumentStartIndex = method.IndexOf('(', spaceIndex) + 1;
            var endIndex = method.IndexOf(')', currentArgumentStartIndex);
            if (spaceIndex == -1 || currentArgumentStartIndex == 0 || endIndex == -1) throw new ArgumentException();
            var returnType = Runtime.Substring(method, 0, spaceIndex);
            var methodName = Runtime.Substring(method, spaceIndex + 1, currentArgumentStartIndex
                                                                       - 1).Trim();
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('(');
            int currentArgumentEndIndex;
            do
            {
                string argumentDescriptor;
                currentArgumentEndIndex = method.IndexOf(',', currentArgumentStartIndex);
                if (currentArgumentEndIndex == -1)
                {
                    argumentDescriptor = GetDescriptorInternal(Runtime.Substring(method, currentArgumentStartIndex
                        , endIndex).Trim(), defaultPackage);
                }
                else
                {
                    argumentDescriptor = GetDescriptorInternal(Runtime.Substring(method, currentArgumentStartIndex
                        , currentArgumentEndIndex).Trim(), defaultPackage);
                    currentArgumentStartIndex = currentArgumentEndIndex + 1;
                }

                stringBuilder.Append(argumentDescriptor);
            } while (currentArgumentEndIndex != -1);

            stringBuilder.Append(')').Append(GetDescriptorInternal(returnType, defaultPackage
            ));
            return new Method(methodName, stringBuilder.ToString());
        }

        /// <summary>Returns the descriptor corresponding to the given type name.</summary>
        /// <param name="type">a Java type name.</param>
        /// <param name="defaultPackage">
        ///     true if unqualified class names belong to the default package, or false
        ///     if they correspond to java.lang classes. For instance "Object" means "Object" if this
        ///     option is true, or "java.lang.Object" otherwise.
        /// </param>
        /// <returns>the descriptor corresponding to the given type name.</returns>
        private static string GetDescriptorInternal(string type, bool defaultPackage)
        {
            if (string.Empty.Equals(type)) return type;
            var stringBuilder = new StringBuilder();
            var arrayBracketsIndex = 0;
            while ((arrayBracketsIndex = type.IndexOf("[]", arrayBracketsIndex) + 1) > 0) stringBuilder.Append('[');
            var elementType = Runtime.Substring(type, 0, type.Length - stringBuilder
                                                             .Length * 2);
            var descriptor = Primitive_Type_Descriptors.GetOrNull(elementType);
            if (descriptor != null)
            {
                stringBuilder.Append(descriptor);
            }
            else
            {
                stringBuilder.Append('L');
                if (elementType.IndexOf('.') < 0)
                {
                    if (!defaultPackage) stringBuilder.Append("java/lang/");
                    stringBuilder.Append(elementType);
                }
                else
                {
                    stringBuilder.Append(elementType.Replace('.', '/'));
                }

                stringBuilder.Append(';');
            }

            return stringBuilder.ToString();
        }

        /// <summary>Returns the name of the method described by this object.</summary>
        /// <returns>the name of the method described by this object.</returns>
        public virtual string GetName()
        {
            return name;
        }

        /// <summary>Returns the descriptor of the method described by this object.</summary>
        /// <returns>the descriptor of the method described by this object.</returns>
        public virtual string GetDescriptor()
        {
            return descriptor;
        }

        /// <summary>Returns the return type of the method described by this object.</summary>
        /// <returns>the return type of the method described by this object.</returns>
        public virtual Type GetReturnType()
        {
            return Type.GetReturnType(descriptor);
        }

        /// <summary>Returns the argument types of the method described by this object.</summary>
        /// <returns>the argument types of the method described by this object.</returns>
        public virtual Type[] GetArgumentTypes()
        {
            return Type.GetArgumentTypes(descriptor);
        }

        public override string ToString()
        {
            return name + descriptor;
        }

        public override bool Equals(object other)
        {
            if (!(other is Method)) return false;
            var otherMethod = (Method) other;
            return name.Equals(otherMethod.name) && descriptor.Equals(otherMethod.descriptor);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() ^ descriptor.GetHashCode();
        }
    }
}