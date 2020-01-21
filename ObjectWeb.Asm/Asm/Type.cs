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
using System.Reflection;
using System.Text;
using ObjectWeb.Misc.Java.Lang;

namespace ObjectWeb.Asm
{
    /// <summary>A Java field or method type.</summary>
    /// <remarks>
    ///     A Java field or method type. This class can be used to make it easier to manipulate type and
    ///     method descriptors.
    /// </remarks>
    /// <author>Eric Bruneton</author>
    /// <author>Chris Nokleberg</author>
    public sealed class Type
    {
        /// <summary>
        ///     The sort of the
        ///     <c>void</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Void = 0;

        /// <summary>
        ///     The sort of the
        ///     <c>boolean</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Boolean = 1;

        /// <summary>
        ///     The sort of the
        ///     <c>char</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Char = 2;

        /// <summary>
        ///     The sort of the
        ///     <c>byte</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Byte = 3;

        /// <summary>
        ///     The sort of the
        ///     <c>short</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Short = 4;

        /// <summary>
        ///     The sort of the
        ///     <c>int</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Int = 5;

        /// <summary>
        ///     The sort of the
        ///     <c>float</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Float = 6;

        /// <summary>
        ///     The sort of the
        ///     <c>long</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Long = 7;

        /// <summary>
        ///     The sort of the
        ///     <c>double</c>
        ///     type. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </summary>
        public const int Double = 8;

        /// <summary>The sort of array reference types.</summary>
        /// <remarks>
        ///     The sort of array reference types. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </remarks>
        public const int Array = 9;

        /// <summary>The sort of object reference types.</summary>
        /// <remarks>
        ///     The sort of object reference types. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </remarks>
        public const int Object = 10;

        /// <summary>The sort of method types.</summary>
        /// <remarks>
        ///     The sort of method types. See
        ///     <see cref="GetSort()" />
        ///     .
        /// </remarks>
        public const int Method = 11;

        /// <summary>
        ///     The (private) sort of object reference types represented with an internal name.
        /// </summary>
        private const int Internal = 12;

        /// <summary>The descriptors of the primitive types.</summary>
        private const string Primitive_Descriptors = "VZCBSIFJD";

        /// <summary>
        ///     The
        ///     <c>void</c>
        ///     type.
        /// </summary>
        public static readonly Type Void_Type = new Type(Void, Primitive_Descriptors, Void
            , Void + 1);

        /// <summary>
        ///     The
        ///     <c>boolean</c>
        ///     type.
        /// </summary>
        public static readonly Type Boolean_Type = new Type(Boolean, Primitive_Descriptors
            , Boolean, Boolean + 1);

        /// <summary>
        ///     The
        ///     <c>char</c>
        ///     type.
        /// </summary>
        public static readonly Type Char_Type = new Type(Char, Primitive_Descriptors, Char
            , Char + 1);

        /// <summary>
        ///     The
        ///     <c>byte</c>
        ///     type.
        /// </summary>
        public static readonly Type Byte_Type = new Type(Byte, Primitive_Descriptors, Byte
            , Byte + 1);

        /// <summary>
        ///     The
        ///     <c>short</c>
        ///     type.
        /// </summary>
        public static readonly Type Short_Type = new Type(Short, Primitive_Descriptors, Short
            , Short + 1);

        /// <summary>
        ///     The
        ///     <c>int</c>
        ///     type.
        /// </summary>
        public static readonly Type Int_Type = new Type(Int, Primitive_Descriptors, Int,
            Int + 1);

        /// <summary>
        ///     The
        ///     <c>float</c>
        ///     type.
        /// </summary>
        public static readonly Type Float_Type = new Type(Float, Primitive_Descriptors, Float
            , Float + 1);

        /// <summary>
        ///     The
        ///     <c>long</c>
        ///     type.
        /// </summary>
        public static readonly Type Long_Type = new Type(Long, Primitive_Descriptors, Long
            , Long + 1);

        /// <summary>
        ///     The
        ///     <c>double</c>
        ///     type.
        /// </summary>
        public static readonly Type Double_Type = new Type(Double, Primitive_Descriptors,
            Double, Double + 1);

        /// <summary>The sort of this type.</summary>
        /// <remarks>
        ///     The sort of this type. Either
        ///     <see cref="Void" />
        ///     ,
        ///     <see cref="Boolean" />
        ///     ,
        ///     <see cref="Char" />
        ///     ,
        ///     <see cref="Byte" />
        ///     ,
        ///     <see cref="Short" />
        ///     ,
        ///     <see cref="Int" />
        ///     ,
        ///     <see cref="Float" />
        ///     ,
        ///     <see cref="Long" />
        ///     ,
        ///     <see cref="Double" />
        ///     ,
        ///     <see cref="Array" />
        ///     ,
        ///     <see cref="Object" />
        ///     ,
        ///     <see cref="Method" />
        ///     or
        ///     <see cref="Internal" />
        ///     .
        /// </remarks>
        private readonly int sort;

        /// <summary>
        ///     The beginning index, inclusive, of the value of this Java field or method type in
        ///     <see cref="valueBuffer" />
        ///     . This value is an internal name for
        ///     <see cref="Object" />
        ///     and
        ///     <see cref="Internal" />
        ///     types,
        ///     and a field or method descriptor in the other cases.
        /// </summary>
        private readonly int valueBegin;

        /// <summary>A buffer containing the value of this field or method type.</summary>
        /// <remarks>
        ///     A buffer containing the value of this field or method type. This value is an internal name for
        ///     <see cref="Object" />
        ///     and
        ///     <see cref="Internal" />
        ///     types, and a field or method descriptor in the other
        ///     cases.
        ///     <p>
        ///         For
        ///         <see cref="Object" />
        ///         types, this field also contains the descriptor: the characters in
        ///         [
        ///         <see cref="valueBegin" />
        ///         ,
        ///         <see cref="valueEnd" />
        ///         ) contain the internal name, and those in [
        ///         <see cref="valueBegin" />
        ///         - 1,
        ///         <see cref="valueEnd" />
        ///         + 1) contain the descriptor.
        /// </remarks>
        private readonly string valueBuffer;

        /// <summary>
        ///     The end index, exclusive, of the value of this Java field or method type in
        ///     <see cref="valueBuffer" />
        ///     . This value is an internal name for
        ///     <see cref="Object" />
        ///     and
        ///     <see cref="Internal" />
        ///     types,
        ///     and a field or method descriptor in the other cases.
        /// </summary>
        private readonly int valueEnd;

        /// <summary>Constructs a reference type.</summary>
        /// <param name="sort">
        ///     the sort of this type, see
        ///     <see cref="sort" />
        ///     .
        /// </param>
        /// <param name="valueBuffer">
        ///     a buffer containing the value of this field or method type.
        /// </param>
        /// <param name="valueBegin">
        ///     the beginning index, inclusive, of the value of this field or method type in
        ///     valueBuffer.
        /// </param>
        /// <param name="valueEnd">
        ///     the end index, exclusive, of the value of this field or method type in
        ///     valueBuffer.
        /// </param>
        private Type(int sort, string valueBuffer, int valueBegin, int valueEnd)
        {
            // -----------------------------------------------------------------------------------------------
            // Fields
            // -----------------------------------------------------------------------------------------------
            this.sort = sort;
            this.valueBuffer = valueBuffer;
            this.valueBegin = valueBegin;
            this.valueEnd = valueEnd;
        }

        // -----------------------------------------------------------------------------------------------
        // Methods to get Type(s) from a descriptor, a reflected Method or Constructor, other types, etc.
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the given type descriptor.
        /// </summary>
        /// <param name="typeDescriptor">a field or method type descriptor.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the given type descriptor.
        /// </returns>
        public static Type GetType(string typeDescriptor)
        {
            return GetTypeInternal(typeDescriptor, 0, typeDescriptor.Length);
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the given class.
        /// </summary>
        /// <param name="clazz">a class.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the given class.
        /// </returns>
        public static Type GetType(global::System.Type clazz)
        {
            if (clazz.IsPrimitive)
            {
                if (clazz == typeof(int))
                    return Int_Type;
                if (clazz == typeof(void))
                    return Void_Type;
                if (clazz == typeof(bool))
                    return Boolean_Type;
                if (clazz == typeof(byte))
                    return Byte_Type;
                if (clazz == typeof(char))
                    return Char_Type;
                if (clazz == typeof(short))
                    return Short_Type;
                if (clazz == typeof(double))
                    return Double_Type;
                if (clazz == typeof(float))
                    return Float_Type;
                if (clazz == typeof(long))
                    return Long_Type;
                throw new AssertionError();
            }

            return GetType(GetDescriptor(clazz));
        }

        /// <summary>
        ///     Returns the method
        ///     <see cref="Type" />
        ///     corresponding to the given constructor.
        /// </summary>
        /// <param name="constructor">
        ///     a
        ///     <see cref="System.Reflection.ConstructorInfo{T}" />
        ///     object.
        /// </param>
        /// <returns>
        ///     the method
        ///     <see cref="Type" />
        ///     corresponding to the given constructor.
        /// </returns>
        public static Type GetType(ConstructorInfo constructor)
        {
            return GetType(GetConstructorDescriptor(constructor));
        }

        /// <summary>
        ///     Returns the method
        ///     <see cref="Type" />
        ///     corresponding to the given method.
        /// </summary>
        /// <param name="method">
        ///     a
        ///     <see cref="System.Reflection.MethodInfo" />
        ///     object.
        /// </param>
        /// <returns>
        ///     the method
        ///     <see cref="Type" />
        ///     corresponding to the given method.
        /// </returns>
        public static Type GetType(MethodInfo method)
        {
            return GetType(GetMethodDescriptor(method));
        }

        /// <summary>Returns the type of the elements of this array type.</summary>
        /// <remarks>
        ///     Returns the type of the elements of this array type. This method should only be used for an
        ///     array type.
        /// </remarks>
        /// <returns>Returns the type of the elements of this array type.</returns>
        public Type GetElementType()
        {
            var numDimensions = GetDimensions();
            return GetTypeInternal(valueBuffer, valueBegin + numDimensions, valueEnd);
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the given internal name.
        /// </summary>
        /// <param name="internalName">an internal name.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the given internal name.
        /// </returns>
        public static Type GetObjectType(string internalName)
        {
            return new Type(internalName[0] == '[' ? Array : Internal, internalName, 0, internalName
                .Length);
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the given method descriptor. Equivalent to
        ///     <code>
        /// Type.getType(methodDescriptor)</code>
        ///     .
        /// </summary>
        /// <param name="methodDescriptor">a method descriptor.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the given method descriptor.
        /// </returns>
        public static Type GetMethodType(string methodDescriptor)
        {
            return new Type(Method, methodDescriptor, 0, methodDescriptor.Length);
        }

        /// <summary>
        ///     Returns the method
        ///     <see cref="Type" />
        ///     corresponding to the given argument and return types.
        /// </summary>
        /// <param name="returnType">the return type of the method.</param>
        /// <param name="argumentTypes">the argument types of the method.</param>
        /// <returns>
        ///     the method
        ///     <see cref="Type" />
        ///     corresponding to the given argument and return types.
        /// </returns>
        public static Type GetMethodType(Type returnType, params Type[] argumentTypes)
        {
            return GetType(GetMethodDescriptor(returnType, argumentTypes));
        }

        /// <summary>Returns the argument types of methods of this type.</summary>
        /// <remarks>
        ///     Returns the argument types of methods of this type. This method should only be used for method
        ///     types.
        /// </remarks>
        /// <returns>the argument types of methods of this type.</returns>
        public Type[] GetArgumentTypes()
        {
            return GetArgumentTypes(GetDescriptor());
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     values corresponding to the argument types of the given method
        ///     descriptor.
        /// </summary>
        /// <param name="methodDescriptor">a method descriptor.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     values corresponding to the argument types of the given method
        ///     descriptor.
        /// </returns>
        public static Type[] GetArgumentTypes(string methodDescriptor)
        {
            // First step: compute the number of argument types in methodDescriptor.
            var numArgumentTypes = 0;
            // Skip the first character, which is always a '('.
            var currentOffset = 1;
            // Parse the argument types, one at a each loop iteration.
            while (methodDescriptor[currentOffset] != ')')
            {
                while (methodDescriptor[currentOffset] == '[') currentOffset++;
                if (methodDescriptor[currentOffset++] == 'L')
                {
                    // Skip the argument descriptor content.
                    var semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
                    currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
                }

                ++numArgumentTypes;
            }

            // Second step: create a Type instance for each argument type.
            var argumentTypes = new Type[numArgumentTypes];
            // Skip the first character, which is always a '('.
            currentOffset = 1;
            // Parse and create the argument types, one at each loop iteration.
            var currentArgumentTypeIndex = 0;
            while (methodDescriptor[currentOffset] != ')')
            {
                var currentArgumentTypeOffset = currentOffset;
                while (methodDescriptor[currentOffset] == '[') currentOffset++;
                if (methodDescriptor[currentOffset++] == 'L')
                {
                    // Skip the argument descriptor content.
                    var semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
                    currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
                }

                argumentTypes[currentArgumentTypeIndex++] = GetTypeInternal(methodDescriptor, currentArgumentTypeOffset
                    , currentOffset);
            }

            return argumentTypes;
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     values corresponding to the argument types of the given method.
        /// </summary>
        /// <param name="method">a method.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     values corresponding to the argument types of the given method.
        /// </returns>
        public static Type[] GetArgumentTypes(MethodInfo method)
        {
            var classes = method.GetParameterTypes();
            var types = new Type[classes.Count];
            for (var i = classes.Count - 1; i >= 0; --i) types[i] = GetType(classes[i]);
            return types;
        }

        /// <summary>Returns the return type of methods of this type.</summary>
        /// <remarks>
        ///     Returns the return type of methods of this type. This method should only be used for method
        ///     types.
        /// </remarks>
        /// <returns>the return type of methods of this type.</returns>
        public Type GetReturnType()
        {
            return GetReturnType(GetDescriptor());
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the return type of the given method descriptor.
        /// </summary>
        /// <param name="methodDescriptor">a method descriptor.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the return type of the given method descriptor.
        /// </returns>
        public static Type GetReturnType(string methodDescriptor)
        {
            return GetTypeInternal(methodDescriptor, GetReturnTypeOffset(methodDescriptor), methodDescriptor
                .Length);
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the return type of the given method.
        /// </summary>
        /// <param name="method">a method.</param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the return type of the given method.
        /// </returns>
        public static Type GetReturnType(MethodInfo method)
        {
            return GetType(method.ReturnType);
        }

        /// <summary>
        ///     Returns the start index of the return type of the given method descriptor.
        /// </summary>
        /// <param name="methodDescriptor">a method descriptor.</param>
        /// <returns>the start index of the return type of the given method descriptor.</returns>
        internal static int GetReturnTypeOffset(string methodDescriptor)
        {
            // Skip the first character, which is always a '('.
            var currentOffset = 1;
            // Skip the argument types, one at a each loop iteration.
            while (methodDescriptor[currentOffset] != ')')
            {
                while (methodDescriptor[currentOffset] == '[') currentOffset++;
                if (methodDescriptor[currentOffset++] == 'L')
                {
                    // Skip the argument descriptor content.
                    var semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
                    currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
                }
            }

            return currentOffset + 1;
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="Type" />
        ///     corresponding to the given field or method descriptor.
        /// </summary>
        /// <param name="descriptorBuffer">
        ///     a buffer containing the field or method descriptor.
        /// </param>
        /// <param name="descriptorBegin">
        ///     the beginning index, inclusive, of the field or method descriptor in
        ///     descriptorBuffer.
        /// </param>
        /// <param name="descriptorEnd">
        ///     the end index, exclusive, of the field or method descriptor in
        ///     descriptorBuffer.
        /// </param>
        /// <returns>
        ///     the
        ///     <see cref="Type" />
        ///     corresponding to the given type descriptor.
        /// </returns>
        private static Type GetTypeInternal(string descriptorBuffer, int descriptorBegin,
            int descriptorEnd)
        {
            switch (descriptorBuffer[descriptorBegin])
            {
                case 'V':
                {
                    return Void_Type;
                }

                case 'Z':
                {
                    return Boolean_Type;
                }

                case 'C':
                {
                    return Char_Type;
                }

                case 'B':
                {
                    return Byte_Type;
                }

                case 'S':
                {
                    return Short_Type;
                }

                case 'I':
                {
                    return Int_Type;
                }

                case 'F':
                {
                    return Float_Type;
                }

                case 'J':
                {
                    return Long_Type;
                }

                case 'D':
                {
                    return Double_Type;
                }

                case '[':
                {
                    return new Type(Array, descriptorBuffer, descriptorBegin, descriptorEnd);
                }

                case 'L':
                {
                    return new Type(Object, descriptorBuffer, descriptorBegin + 1, descriptorEnd - 1);
                }

                case '(':
                {
                    return new Type(Method, descriptorBuffer, descriptorBegin, descriptorEnd);
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Methods to get class names, internal names or descriptors.
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns the binary name of the class corresponding to this type.</summary>
        /// <remarks>
        ///     Returns the binary name of the class corresponding to this type. This method must not be used
        ///     on method types.
        /// </remarks>
        /// <returns>the binary name of the class corresponding to this type.</returns>
        public string GetClassName()
        {
            switch (sort)
            {
                case Void:
                {
                    return "void";
                }

                case Boolean:
                {
                    return "boolean";
                }

                case Char:
                {
                    return "char";
                }

                case Byte:
                {
                    return "byte";
                }

                case Short:
                {
                    return "short";
                }

                case Int:
                {
                    return "int";
                }

                case Float:
                {
                    return "float";
                }

                case Long:
                {
                    return "long";
                }

                case Double:
                {
                    return "double";
                }

                case Array:
                {
                    var stringBuilder = new StringBuilder(GetElementType().GetClassName());
                    for (var i = GetDimensions(); i > 0; --i) stringBuilder.Append("[]");
                    return stringBuilder.ToString();
                }

                case Object:
                case Internal:
                {
                    return Runtime.Substring(valueBuffer, valueBegin, valueEnd).Replace('/',
                        '.');
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        /// <summary>
        ///     Returns the internal name of the class corresponding to this object or array type.
        /// </summary>
        /// <remarks>
        ///     Returns the internal name of the class corresponding to this object or array type. The internal
        ///     name of a class is its fully qualified name (as returned by Class.getName(), where '.' are
        ///     replaced by '/'). This method should only be used for an object or array type.
        /// </remarks>
        /// <returns>the internal name of the class corresponding to this object type.</returns>
        public string GetInternalName()
        {
            return Runtime.Substring(valueBuffer, valueBegin, valueEnd);
        }

        /// <summary>Returns the internal name of the given class.</summary>
        /// <remarks>
        ///     Returns the internal name of the given class. The internal name of a class is its fully
        ///     qualified name, as returned by Class.getName(), where '.' are replaced by '/'.
        /// </remarks>
        /// <param name="clazz">an object or array class.</param>
        /// <returns>the internal name of the given class.</returns>
        public static string GetInternalName(global::System.Type clazz)
        {
            return clazz.FullName.Replace('.', '/');
        }

        /// <summary>Returns the descriptor corresponding to this type.</summary>
        /// <returns>the descriptor corresponding to this type.</returns>
        public string GetDescriptor()
        {
            if (sort == Object)
                return Runtime.Substring(valueBuffer, valueBegin - 1, valueEnd + 1);
            if (sort == Internal)
                return 'L' + Runtime.Substring(valueBuffer, valueBegin, valueEnd) + ';';
            return Runtime.Substring(valueBuffer, valueBegin, valueEnd);
        }

        /// <summary>Returns the descriptor corresponding to the given class.</summary>
        /// <param name="clazz">an object class, a primitive class or an array class.</param>
        /// <returns>the descriptor corresponding to the given class.</returns>
        public static string GetDescriptor(global::System.Type clazz)
        {
            var stringBuilder = new StringBuilder();
            AppendDescriptor(clazz, stringBuilder);
            return stringBuilder.ToString();
        }

        /// <summary>Returns the descriptor corresponding to the given constructor.</summary>
        /// <param name="constructor">
        ///     a
        ///     <see cref="System.Reflection.ConstructorInfo{T}" />
        ///     object.
        /// </param>
        /// <returns>the descriptor of the given constructor.</returns>
        public static string GetConstructorDescriptor(ConstructorInfo constructor)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('(');
            var parameters = constructor.GetParameterTypes();
            foreach (var parameter in parameters) AppendDescriptor(parameter, stringBuilder);
            return stringBuilder.Append(")V").ToString();
        }

        /// <summary>
        ///     Returns the descriptor corresponding to the given argument and return types.
        /// </summary>
        /// <param name="returnType">the return type of the method.</param>
        /// <param name="argumentTypes">the argument types of the method.</param>
        /// <returns>the descriptor corresponding to the given argument and return types.</returns>
        public static string GetMethodDescriptor(Type returnType, params Type[] argumentTypes
        )
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('(');
            foreach (var argumentType in argumentTypes) argumentType.AppendDescriptor(stringBuilder);
            stringBuilder.Append(')');
            returnType.AppendDescriptor(stringBuilder);
            return stringBuilder.ToString();
        }

        /// <summary>Returns the descriptor corresponding to the given method.</summary>
        /// <param name="method">
        ///     a
        ///     <see cref="System.Reflection.MethodInfo" />
        ///     object.
        /// </param>
        /// <returns>the descriptor of the given method.</returns>
        public static string GetMethodDescriptor(MethodInfo method)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('(');
            var parameters = method.GetParameterTypes();
            foreach (var parameter in parameters) AppendDescriptor(parameter, stringBuilder);
            stringBuilder.Append(')');
            AppendDescriptor(method.ReturnType, stringBuilder);
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Appends the descriptor corresponding to this type to the given string buffer.
        /// </summary>
        /// <param name="stringBuilder">
        ///     the string builder to which the descriptor must be appended.
        /// </param>
        private void AppendDescriptor(StringBuilder stringBuilder)
        {
            if (sort == Object)
                stringBuilder.Append(valueBuffer, valueBegin - 1, valueEnd + 1);
            else if (sort == Internal)
                stringBuilder.Append('L').Append(valueBuffer, valueBegin, valueEnd).Append(';');
            else
                stringBuilder.Append(valueBuffer, valueBegin, valueEnd);
        }

        /// <summary>Appends the descriptor of the given class to the given string builder.</summary>
        /// <param name="clazz">the class whose descriptor must be computed.</param>
        /// <param name="stringBuilder">
        ///     the string builder to which the descriptor must be appended.
        /// </param>
        private static void AppendDescriptor(global::System.Type clazz, StringBuilder stringBuilder)
        {
            var currentClass = clazz;
            while (currentClass.IsArray)
            {
                stringBuilder.Append('[');
                currentClass = currentClass.GetElementType();
            }

            if (currentClass.IsPrimitive)
            {
                char descriptor;
                if (currentClass == typeof(int))
                    descriptor = 'I';
                else if (currentClass == typeof(void))
                    descriptor = 'V';
                else if (currentClass == typeof(bool))
                    descriptor = 'Z';
                else if (currentClass == typeof(byte))
                    descriptor = 'B';
                else if (currentClass == typeof(char))
                    descriptor = 'C';
                else if (currentClass == typeof(short))
                    descriptor = 'S';
                else if (currentClass == typeof(double))
                    descriptor = 'D';
                else if (currentClass == typeof(float))
                    descriptor = 'F';
                else if (currentClass == typeof(long))
                    descriptor = 'J';
                else
                    throw new AssertionError();
                stringBuilder.Append(descriptor);
            }
            else
            {
                stringBuilder.Append('L').Append(GetInternalName(currentClass)).Append(';');
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Methods to get the sort, dimension, size, and opcodes corresponding to a Type or descriptor.
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns the sort of this type.</summary>
        /// <returns>
        ///     <see cref="Void" />
        ///     ,
        ///     <see cref="Boolean" />
        ///     ,
        ///     <see cref="Char" />
        ///     ,
        ///     <see cref="Byte" />
        ///     ,
        ///     <see cref="Short" />
        ///     ,
        ///     <see cref="Int" />
        ///     ,
        ///     <see cref="Float" />
        ///     ,
        ///     <see cref="Long" />
        ///     ,
        ///     <see cref="Double" />
        ///     ,
        ///     <see cref="Array" />
        ///     ,
        ///     <see cref="Object" />
        ///     or
        ///     <see cref="Method" />
        ///     .
        /// </returns>
        public int GetSort()
        {
            return sort == Internal ? Object : sort;
        }

        /// <summary>Returns the number of dimensions of this array type.</summary>
        /// <remarks>
        ///     Returns the number of dimensions of this array type. This method should only be used for an
        ///     array type.
        /// </remarks>
        /// <returns>the number of dimensions of this array type.</returns>
        public int GetDimensions()
        {
            var numDimensions = 1;
            while (valueBuffer[valueBegin + numDimensions] == '[') numDimensions++;
            return numDimensions;
        }

        /// <summary>Returns the size of values of this type.</summary>
        /// <remarks>
        ///     Returns the size of values of this type. This method must not be used for method types.
        /// </remarks>
        /// <returns>
        ///     the size of values of this type, i.e., 2 for
        ///     <c>long</c>
        ///     and
        ///     <c>double</c>
        ///     , 0 for
        ///     <c>void</c>
        ///     and 1 otherwise.
        /// </returns>
        public int GetSize()
        {
            switch (sort)
            {
                case Void:
                {
                    return 0;
                }

                case Boolean:
                case Char:
                case Byte:
                case Short:
                case Int:
                case Float:
                case Array:
                case Object:
                case Internal:
                {
                    return 1;
                }

                case Long:
                case Double:
                {
                    return 2;
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        /// <summary>
        ///     Returns the size of the arguments and of the return value of methods of this type.
        /// </summary>
        /// <remarks>
        ///     Returns the size of the arguments and of the return value of methods of this type. This method
        ///     should only be used for method types.
        /// </remarks>
        /// <returns>
        ///     the size of the arguments of the method (plus one for the implicit this argument),
        ///     argumentsSize, and the size of its return value, returnSize, packed into a single int i =
        ///     <c>(argumentsSize &lt;&lt; 2) | returnSize</c>
        ///     (argumentsSize is therefore equal to
        ///     <c>i &gt;&gt; 2</c>
        ///     , and returnSize to
        ///     <c>i &amp; 0x03</c>
        ///     ).
        /// </returns>
        public int GetArgumentsAndReturnSizes()
        {
            return GetArgumentsAndReturnSizes(GetDescriptor());
        }

        /// <summary>Computes the size of the arguments and of the return value of a method.</summary>
        /// <param name="methodDescriptor">a method descriptor.</param>
        /// <returns>
        ///     the size of the arguments of the method (plus one for the implicit this argument),
        ///     argumentsSize, and the size of its return value, returnSize, packed into a single int i =
        ///     <c>(argumentsSize &lt;&lt; 2) | returnSize</c>
        ///     (argumentsSize is therefore equal to
        ///     <c>i &gt;&gt; 2</c>
        ///     , and returnSize to
        ///     <c>i &amp; 0x03</c>
        ///     ).
        /// </returns>
        public static int GetArgumentsAndReturnSizes(string methodDescriptor)
        {
            var argumentsSize = 1;
            // Skip the first character, which is always a '('.
            var currentOffset = 1;
            int currentChar = methodDescriptor[currentOffset];
            // Parse the argument types and compute their size, one at a each loop iteration.
            while (currentChar != ')')
            {
                if (currentChar == 'J' || currentChar == 'D')
                {
                    currentOffset++;
                    argumentsSize += 2;
                }
                else
                {
                    while (methodDescriptor[currentOffset] == '[') currentOffset++;
                    if (methodDescriptor[currentOffset++] == 'L')
                    {
                        // Skip the argument descriptor content.
                        var semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
                        currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
                    }

                    argumentsSize += 1;
                }

                currentChar = methodDescriptor[currentOffset];
            }

            currentChar = methodDescriptor[currentOffset + 1];
            if (currentChar == 'V') return argumentsSize << 2;

            var returnSize = currentChar == 'J' || currentChar == 'D' ? 2 : 1;
            return (argumentsSize << 2) | returnSize;
        }

        /// <summary>
        ///     Returns a JVM instruction opcode adapted to this
        ///     <see cref="Type" />
        ///     . This method must not be used for
        ///     method types.
        /// </summary>
        /// <param name="opcode">
        ///     a JVM instruction opcode. This opcode must be one of ILOAD, ISTORE, IALOAD,
        ///     IASTORE, IADD, ISUB, IMUL, IDIV, IREM, INEG, ISHL, ISHR, IUSHR, IAND, IOR, IXOR and
        ///     IRETURN.
        /// </param>
        /// <returns>
        ///     an opcode that is similar to the given opcode, but adapted to this
        ///     <see cref="Type" />
        ///     . For
        ///     example, if this type is
        ///     <c>float</c>
        ///     and
        ///     <paramref name="opcode" />
        ///     is IRETURN, this method returns
        ///     FRETURN.
        /// </returns>
        public int GetOpcode(int opcode)
        {
            if (opcode == OpcodesConstants.Iaload || opcode == OpcodesConstants.Iastore)
                switch (sort)
                {
                    case Boolean:
                    case Byte:
                    {
                        return opcode + (OpcodesConstants.Baload - OpcodesConstants.Iaload);
                    }

                    case Char:
                    {
                        return opcode + (OpcodesConstants.Caload - OpcodesConstants.Iaload);
                    }

                    case Short:
                    {
                        return opcode + (OpcodesConstants.Saload - OpcodesConstants.Iaload);
                    }

                    case Int:
                    {
                        return opcode;
                    }

                    case Float:
                    {
                        return opcode + (OpcodesConstants.Faload - OpcodesConstants.Iaload);
                    }

                    case Long:
                    {
                        return opcode + (OpcodesConstants.Laload - OpcodesConstants.Iaload);
                    }

                    case Double:
                    {
                        return opcode + (OpcodesConstants.Daload - OpcodesConstants.Iaload);
                    }

                    case Array:
                    case Object:
                    case Internal:
                    {
                        return opcode + (OpcodesConstants.Aaload - OpcodesConstants.Iaload);
                    }

                    case Method:
                    case Void:
                    {
                        throw new NotSupportedException();
                    }

                    default:
                    {
                        throw new AssertionError();
                    }
                }

            switch (sort)
            {
                case Void:
                {
                    if (opcode != OpcodesConstants.Ireturn) throw new NotSupportedException();
                    return OpcodesConstants.Return;
                }

                case Boolean:
                case Byte:
                case Char:
                case Short:
                case Int:
                {
                    return opcode;
                }

                case Float:
                {
                    return opcode + (OpcodesConstants.Freturn - OpcodesConstants.Ireturn);
                }

                case Long:
                {
                    return opcode + (OpcodesConstants.Lreturn - OpcodesConstants.Ireturn);
                }

                case Double:
                {
                    return opcode + (OpcodesConstants.Dreturn - OpcodesConstants.Ireturn);
                }

                case Array:
                case Object:
                case Internal:
                {
                    if (opcode != OpcodesConstants.Iload && opcode != OpcodesConstants.Istore && opcode
                        != OpcodesConstants.Ireturn)
                        throw new NotSupportedException();
                    return opcode + (OpcodesConstants.Areturn - OpcodesConstants.Ireturn);
                }

                case Method:
                {
                    throw new NotSupportedException();
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Equals, hashCode and toString.
        // -----------------------------------------------------------------------------------------------
        /// <summary>Tests if the given object is equal to this type.</summary>
        /// <param name="object">the object to be compared to this type.</param>
        /// <returns>
        ///     <literal>true</literal>
        ///     if the given object is equal to this type.
        /// </returns>
        public override bool Equals(object @object)
        {
            if (this == @object) return true;
            if (!(@object is Type)) return false;
            var other = (Type) @object;
            if ((sort == Internal ? Object : sort) != (other.sort == Internal
                    ? Object
                    : other
                        .sort))
                return false;
            var begin = valueBegin;
            var end = valueEnd;
            var otherBegin = other.valueBegin;
            var otherEnd = other.valueEnd;
            // Compare the values.
            if (end - begin != otherEnd - otherBegin) return false;
            for (int i = begin, j = otherBegin; i < end; i++, j++)
                if (valueBuffer[i] != other.valueBuffer[j])
                    return false;
            return true;
        }

        /// <summary>Returns a hash code value for this type.</summary>
        /// <returns>a hash code value for this type.</returns>
        public override int GetHashCode()
        {
            var hashCode = 13 * (sort == Internal ? Object : sort);
            if (sort >= Array)
                for (int i = valueBegin, end = valueEnd; i < end; i++)
                    hashCode = 17 * (hashCode + valueBuffer[i]);
            return hashCode;
        }

        /// <summary>Returns a string representation of this type.</summary>
        /// <returns>the descriptor of this type.</returns>
        public override string ToString()
        {
            return GetDescriptor();
        }
    }
}