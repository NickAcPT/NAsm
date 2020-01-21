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
using System.Text;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Misc.Java.Lang;

namespace ObjectWeb.Asm.Tree.Analysis
{
    /// <summary>
    ///     An extended
    ///     <see cref="BasicVerifier" />
    ///     that performs more precise verifications. This verifier
    ///     computes exact class types, instead of using a single "object reference" type (as done in
    ///     <see cref="BasicVerifier" />
    ///     ).
    /// </summary>
    /// <author>Eric Bruneton</author>
    /// <author>Bing Ran</author>
    public class SimpleVerifier : BasicVerifier
    {
        /// <summary>The type of the class that is verified.</summary>
        private readonly Type currentClass;

        /// <summary>
        ///     The types of the interfaces directly implemented by the class that is verified.
        /// </summary>
        private readonly IList<Type> currentClassInterfaces;

        /// <summary>The type of the super class of the class that is verified.</summary>
        private readonly Type currentSuperClass;

        /// <summary>Whether the class that is verified is an interface.</summary>
        private readonly bool isInterface__;

        /// <summary>The loader to use to load the referenced classes.</summary>
        private AppDomain loader = AppDomain.CurrentDomain;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SimpleVerifier" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see
        ///         cref="SimpleVerifier(int, Org.Objectweb.Asm.Type, Org.Objectweb.Asm.Type, Sharpen.System.Collections.Generic.IList{E}, bool)
        /// 	" />
        ///     version.
        /// </summary>
        public SimpleVerifier()
            : this(null, null, false)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SimpleVerifier" />
        ///     to verify a specific class. This class will not be
        ///     loaded into the JVM since it may be incorrect. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see
        ///         cref="SimpleVerifier(int, Org.Objectweb.Asm.Type, Org.Objectweb.Asm.Type, Sharpen.System.Collections.Generic.IList{E}, bool)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="currentClass">the type of the class to be verified.</param>
        /// <param name="currentSuperClass">
        ///     the type of the super class of the class to be verified.
        /// </param>
        /// <param name="isInterface">whether the class to be verifier is an interface.</param>
        public SimpleVerifier(Type currentClass, Type currentSuperClass, bool isInterface
        )
            : this(currentClass, currentSuperClass, null, isInterface)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SimpleVerifier" />
        ///     to verify a specific class. This class will not be
        ///     loaded into the JVM since it may be incorrect. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see
        ///         cref="SimpleVerifier(int, Org.Objectweb.Asm.Type, Org.Objectweb.Asm.Type, Sharpen.System.Collections.Generic.IList{E}, bool)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="currentClass">the type of the class to be verified.</param>
        /// <param name="currentSuperClass">
        ///     the type of the super class of the class to be verified.
        /// </param>
        /// <param name="currentClassInterfaces">
        ///     the types of the interfaces directly implemented by the class to
        ///     be verified.
        /// </param>
        /// <param name="isInterface">whether the class to be verifier is an interface.</param>
        public SimpleVerifier(Type currentClass, Type currentSuperClass, IList<Type> currentClassInterfaces
            , bool isInterface)
            : this(VisitorAsmApiVersion.Asm7, currentClass, currentSuperClass, currentClassInterfaces
                , isInterface)
        {
            /* latest api = */
            if (GetType() != typeof(SimpleVerifier)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SimpleVerifier" />
        ///     to verify a specific class. This class will not be
        ///     loaded into the JVM since it may be incorrect.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this verifier. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        /// <param name="currentClass">the type of the class to be verified.</param>
        /// <param name="currentSuperClass">
        ///     the type of the super class of the class to be verified.
        /// </param>
        /// <param name="currentClassInterfaces">
        ///     the types of the interfaces directly implemented by the class to
        ///     be verified.
        /// </param>
        /// <param name="isInterface">whether the class to be verifier is an interface.</param>
        protected internal SimpleVerifier(VisitorAsmApiVersion api, Type currentClass, Type currentSuperClass
            , IList<Type> currentClassInterfaces, bool isInterface)
            : base(api)
        {
            this.currentClass = currentClass;
            this.currentSuperClass = currentSuperClass;
            this.currentClassInterfaces = currentClassInterfaces;
            isInterface__ = isInterface;
        }

        /// <summary>
        ///     Sets the <code>ClassLoader</code> to be used in
        ///     <see cref="object.GetType()" />
        ///     .
        /// </summary>
        /// <param name="loader">the <code>ClassLoader</code> to use.</param>
        public virtual void SetClassLoader(AppDomain loader)
        {
            this.loader = loader;
        }

        public override BasicValue NewValue(Type type)
        {
            if (type == null) return BasicValue.Uninitialized_Value;
            var isArray = type.GetSort() == Type.Array;
            if (isArray)
                switch (type.GetElementType().GetSort())
                {
                    case Type.Boolean:
                    case Type.Char:
                    case Type.Byte:
                    case Type.Short:
                    {
                        return new BasicValue(type);
                    }
                }

            var value = base.NewValue(type);
            if (BasicValue.Reference_Value.Equals(value))
            {
                if (isArray)
                {
                    value = NewValue(type.GetElementType());
                    var descriptor = new StringBuilder();
                    for (var i = 0; i < type.GetDimensions(); ++i) descriptor.Append('[');
                    descriptor.Append(value.GetType().GetDescriptor());
                    value = new BasicValue(Type.GetType(descriptor.ToString()));
                }
                else
                {
                    value = new BasicValue(type);
                }
            }

            return value;
        }

        protected internal override bool IsArrayValue(BasicValue value)
        {
            var type = value.GetType();
            return type != null && (type.GetSort() == Type.Array || type.Equals(Null_Type));
        }

        /// <exception cref="AnalyzerException" />
        protected internal override BasicValue GetElementValue(BasicValue objectArrayValue
        )
        {
            var arrayType = objectArrayValue.GetType();
            if (arrayType != null)
            {
                if (arrayType.GetSort() == Type.Array)
                    return NewValue(Type.GetType(Runtime.Substring(arrayType.GetDescriptor(),
                        1)));
                if (arrayType.Equals(Null_Type)) return objectArrayValue;
            }

            throw new AssertionError();
        }

        protected internal override bool IsSubTypeOf(BasicValue value, BasicValue expected
        )
        {
            var expectedType = expected.GetType();
            var type = value.GetType();
            switch (expectedType.GetSort())
            {
                case Type.Int:
                case Type.Float:
                case Type.Long:
                case Type.Double:
                {
                    return type.Equals(expectedType);
                }

                case Type.Array:
                case Type.Object:
                {
                    if (type.Equals(Null_Type)) return true;

                    if (type.GetSort() == Type.Object || type.GetSort() == Type.Array)
                    {
                        if (IsAssignableFrom(expectedType, type))
                            return true;
                        if (GetClass(expectedType).IsInterface)
                            // The merge of class or interface types can only yield class types (because it is not
                            // possible in general to find an unambiguous common super interface, due to multiple
                            // inheritance). Because of this limitation, we need to relax the subtyping check here
                            // if 'value' is an interface.
                            return Runtime.IsAssignableFrom(typeof(object), GetClass(type));
                        return false;
                    }

                    return false;
                }

                default:
                {
                    throw new AssertionError();
                }
            }
        }

        public override BasicValue Merge(BasicValue value1, BasicValue value2)
        {
            if (!value1.Equals(value2))
            {
                var type1 = value1.GetType();
                var type2 = value2.GetType();
                if (type1 != null && (type1.GetSort() == Type.Object || type1.GetSort() == Type.Array
                    ) && type2 != null && (type2.GetSort() == Type.Object || type2.GetSort() == Type
                                               .Array))
                {
                    if (type1.Equals(Null_Type)) return value2;
                    if (type2.Equals(Null_Type)) return value1;
                    if (IsAssignableFrom(type1, type2)) return value1;
                    if (IsAssignableFrom(type2, type1)) return value2;
                    var numDimensions = 0;
                    if (type1.GetSort() == Type.Array && type2.GetSort() == Type.Array && type1.GetDimensions
                            () == type2.GetDimensions() && type1.GetElementType().GetSort() == Type.Object &&
                        type2.GetElementType().GetSort() == Type.Object)
                    {
                        numDimensions = type1.GetDimensions();
                        type1 = type1.GetElementType();
                        type2 = type2.GetElementType();
                    }

                    while (true)
                    {
                        if (type1 == null || IsInterface(type1))
                            return NewArrayValue(Type.GetObjectType("java/lang/Object"), numDimensions);
                        type1 = GetSuperClass(type1);
                        if (IsAssignableFrom(type1, type2)) return NewArrayValue(type1, numDimensions);
                    }
                }

                return BasicValue.Uninitialized_Value;
            }

            return value1;
        }

        private BasicValue NewArrayValue(Type type, int dimensions)
        {
            if (dimensions == 0) return NewValue(type);

            var descriptor = new StringBuilder();
            for (var i = 0; i < dimensions; ++i) descriptor.Append('[');
            descriptor.Append(type.GetDescriptor());
            return NewValue(Type.GetType(descriptor.ToString()));
        }

        /// <summary>Returns whether the given type corresponds to the type of an interface.</summary>
        /// <remarks>
        ///     Returns whether the given type corresponds to the type of an interface. The default
        ///     implementation of this method loads the class and uses the reflection API to return its result
        ///     (unless the given type corresponds to the class being verified).
        /// </remarks>
        /// <param name="type">a type.</param>
        /// <returns>whether 'type' corresponds to an interface.</returns>
        protected internal virtual bool IsInterface(Type type)
        {
            if (currentClass != null && currentClass.Equals(type)) return isInterface__;
            return GetClass(type).IsInterface;
        }

        /// <summary>Returns the type corresponding to the super class of the given type.</summary>
        /// <remarks>
        ///     Returns the type corresponding to the super class of the given type. The default implementation
        ///     of this method loads the class and uses the reflection API to return its result (unless the
        ///     given type corresponds to the class being verified).
        /// </remarks>
        /// <param name="type">a type.</param>
        /// <returns>the type corresponding to the super class of 'type'.</returns>
        protected internal virtual Type GetSuperClass(Type type)
        {
            if (currentClass != null && currentClass.Equals(type)) return currentSuperClass;
            var superClass = GetClass(type).BaseType;
            return superClass == null ? null : Type.GetType(superClass);
        }

        /// <summary>
        ///     Returns whether the class corresponding to the first argument is either the same as, or is a
        ///     superclass or superinterface of the class corresponding to the second argument.
        /// </summary>
        /// <remarks>
        ///     Returns whether the class corresponding to the first argument is either the same as, or is a
        ///     superclass or superinterface of the class corresponding to the second argument. The default
        ///     implementation of this method loads the classes and uses the reflection API to return its
        ///     result (unless the result can be computed from the class being verified, and the types of its
        ///     super classes and implemented interfaces).
        /// </remarks>
        /// <param name="type1">a type.</param>
        /// <param name="type2">another type.</param>
        /// <returns>
        ///     whether the class corresponding to 'type1' is either the same as, or is a superclass or
        ///     superinterface of the class corresponding to 'type2'.
        /// </returns>
        protected internal virtual bool IsAssignableFrom(Type type1, Type type2)
        {
            if (type1.Equals(type2)) return true;
            if (currentClass != null && currentClass.Equals(type1))
            {
                if (GetSuperClass(type2) == null) return false;

                if (isInterface__) return type2.GetSort() == Type.Object || type2.GetSort() == Type.Array;
                return IsAssignableFrom(type1, GetSuperClass(type2));
            }

            if (currentClass != null && currentClass.Equals(type2))
            {
                if (IsAssignableFrom(type1, currentSuperClass)) return true;
                if (currentClassInterfaces != null)
                    foreach (var currentClassInterface in currentClassInterfaces)
                        if (IsAssignableFrom(type1, currentClassInterface))
                            return true;
                return false;
            }

            return Runtime.IsAssignableFrom(GetClass(type1), GetClass(type2));
        }

        /// <summary>Loads the class corresponding to the given type.</summary>
        /// <remarks>
        ///     Loads the class corresponding to the given type. The class is loaded with the class loader
        ///     specified with
        ///     <see cref="SetClassLoader(Java.Lang.ClassLoader)" />
        ///     , or with the class loader of this class if no class
        ///     loader was specified.
        /// </remarks>
        /// <param name="type">a type.</param>
        /// <returns>the class corresponding to 'type'.</returns>
        protected internal virtual global::System.Type GetClass(Type type)
        {
            try
            {
                if (type.GetSort() == Type.Array)
                    return global::System.Type.GetType(type.GetDescriptor().Replace('/', '.'));
                return global::System.Type.GetType(type.GetClassName());
            }
            catch (TypeLoadException e)
            {
                throw new TypeNotPresentException(e.ToString(), e);
            }
        }
    }
}