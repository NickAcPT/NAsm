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
using System.Text;
using ObjectWeb.Asm.Signature;

namespace ObjectWeb.Asm.Commons
{
	/// <summary>A class responsible for remapping types and names.</summary>
	/// <author>Eugene Kuleshov</author>
	public abstract class Remapper
    {
	    /// <summary>
	    ///     Returns the given descriptor, remapped with
	    ///     <see cref="Map(string)" />
	    ///     .
	    /// </summary>
	    /// <param name="descriptor">a type descriptor.</param>
	    /// <returns>
	    ///     the given descriptor, with its [array element type] internal name remapped with
	    ///     <see cref="Map(string)" />
	    ///     (if the descriptor corresponds to an array or object type, otherwise the
	    ///     descriptor is returned as is).
	    /// </returns>
	    public virtual string MapDesc(string descriptor)
        {
            return MapType(Type.GetType(descriptor)).GetDescriptor();
        }

	    /// <summary>
	    ///     Returns the given
	    ///     <see cref="Type" />
	    ///     , remapped with
	    ///     <see cref="Map(string)" />
	    ///     or
	    ///     <see cref="MapMethodDesc(string)" />
	    ///     .
	    /// </summary>
	    /// <param name="type">a type, which can be a method type.</param>
	    /// <returns>
	    ///     the given type, with its [array element type] internal name remapped with
	    ///     <see cref="Map(string)" />
	    ///     (if the type is an array or object type, otherwise the type is returned as
	    ///     is) or, of the type is a method type, with its descriptor remapped with
	    ///     <see cref="MapMethodDesc(string)" />
	    ///     .
	    /// </returns>
	    private Type MapType(Type type)
        {
            switch (type.GetSort())
            {
                case Type.Array:
                {
                    var remappedDescriptor = new StringBuilder();
                    for (var i = 0; i < type.GetDimensions(); ++i) remappedDescriptor.Append('[');
                    remappedDescriptor.Append(MapType(type.GetElementType()).GetDescriptor());
                    return Type.GetType(remappedDescriptor.ToString());
                }

                case Type.Object:
                {
                    var remappedInternalName = Map(type.GetInternalName());
                    return remappedInternalName != null ? Type.GetObjectType(remappedInternalName) : type;
                }

                case Type.Method:
                {
                    return Type.GetMethodType(MapMethodDesc(type.GetDescriptor()));
                }

                default:
                {
                    return type;
                }
            }
        }

	    /// <summary>
	    ///     Returns the given internal name, remapped with
	    ///     <see cref="Map(string)" />
	    ///     .
	    /// </summary>
	    /// <param name="internalName">
	    ///     the internal name (or array type descriptor) of some (array) class.
	    /// </param>
	    /// <returns>
	    ///     the given internal name, remapped with
	    ///     <see cref="Map(string)" />
	    ///     .
	    /// </returns>
	    public virtual string MapType(string internalName)
        {
            if (internalName == null) return null;
            return MapType(Type.GetObjectType(internalName)).GetInternalName();
        }

	    /// <summary>
	    ///     Returns the given internal names, remapped with
	    ///     <see cref="Map(string)" />
	    ///     .
	    /// </summary>
	    /// <param name="internalNames">
	    ///     the internal names (or array type descriptors) of some (array) classes.
	    /// </param>
	    /// <returns>
	    ///     the given internal name, remapped with
	    ///     <see cref="Map(string)" />
	    ///     .
	    /// </returns>
	    public virtual string[] MapTypes(string[] internalNames)
        {
            string[] remappedInternalNames = null;
            for (var i = 0; i < internalNames.Length; ++i)
            {
                var internalName = internalNames[i];
                var remappedInternalName = MapType(internalName);
                if (remappedInternalName != null)
                {
                    if (remappedInternalNames == null) remappedInternalNames = (string[]) internalNames.Clone();
                    remappedInternalNames[i] = remappedInternalName;
                }
            }

            return remappedInternalNames != null ? remappedInternalNames : internalNames;
        }

	    /// <summary>
	    ///     Returns the given method descriptor, with its argument and return type descriptors remapped
	    ///     with
	    ///     <see cref="MapDesc(string)" />
	    ///     .
	    /// </summary>
	    /// <param name="methodDescriptor">a method descriptor.</param>
	    /// <returns>
	    ///     the given method descriptor, with its argument and return type descriptors remapped
	    ///     with
	    ///     <see cref="MapDesc(string)" />
	    ///     .
	    /// </returns>
	    public virtual string MapMethodDesc(string methodDescriptor)
        {
            if ("()V".Equals(methodDescriptor)) return methodDescriptor;
            var stringBuilder = new StringBuilder("(");
            foreach (var argumentType in Type.GetArgumentTypes(methodDescriptor))
                stringBuilder.Append(MapType(argumentType).GetDescriptor());
            var returnType = Type.GetReturnType(methodDescriptor);
            if (returnType == Type.Void_Type)
                stringBuilder.Append(")V");
            else
                stringBuilder.Append(')').Append(MapType(returnType).GetDescriptor());
            return stringBuilder.ToString();
        }

	    /// <summary>Returns the given value, remapped with this remapper.</summary>
	    /// <remarks>
	    ///     Returns the given value, remapped with this remapper. Possible values are
	    ///     <see cref="bool" />
	    ///     ,
	    ///     <see cref="byte" />
	    ///     ,
	    ///     <see cref="short" />
	    ///     ,
	    ///     <see cref="char" />
	    ///     ,
	    ///     <see cref="int" />
	    ///     ,
	    ///     <see cref="long" />
	    ///     ,
	    ///     <see cref="double" />
	    ///     ,
	    ///     <see cref="float" />
	    ///     ,
	    ///     <see cref="string" />
	    ///     ,
	    ///     <see cref="Type" />
	    ///     ,
	    ///     <see cref="Handle" />
	    ///     ,
	    ///     <see cref="ConstantDynamic" />
	    ///     or arrays
	    ///     of primitive types .
	    /// </remarks>
	    /// <param name="value">
	    ///     an object. Only
	    ///     <see cref="Type" />
	    ///     ,
	    ///     <see cref="Handle" />
	    ///     and
	    ///     <see cref="ConstantDynamic" />
	    ///     values
	    ///     are remapped.
	    /// </param>
	    /// <returns>the given value, remapped with this remapper.</returns>
	    public virtual object MapValue(object value)
        {
            if (value is Type) return MapType((Type) value);
            if (value is Handle)
            {
                var handle = (Handle) value;
                return new Handle(handle.GetTag(), MapType(handle.GetOwner()), MapMethodName(handle
                        .GetOwner(), handle.GetName(), handle.GetDesc()), handle.GetTag() <= OpcodesConstants
                                                                              .H_Putstatic
                        ? MapDesc(handle.GetDesc())
                        : MapMethodDesc(handle.GetDesc()), handle
                        .IsInterface());
            }

            if (value is ConstantDynamic)
            {
                var constantDynamic = (ConstantDynamic) value;
                var bootstrapMethodArgumentCount = constantDynamic.GetBootstrapMethodArgumentCount
                    ();
                var remappedBootstrapMethodArguments = new object[bootstrapMethodArgumentCount
                ];
                for (var i = 0; i < bootstrapMethodArgumentCount; ++i)
                    remappedBootstrapMethodArguments[i] = MapValue(constantDynamic.GetBootstrapMethodArgument
                        (i));
                var descriptor = constantDynamic.GetDescriptor();
                return new ConstantDynamic(MapInvokeDynamicMethodName(constantDynamic.GetName(),
                    descriptor), MapDesc(descriptor), (Handle) MapValue(constantDynamic.GetBootstrapMethod
                    ()), remappedBootstrapMethodArguments);
            }

            return value;
        }

	    /// <summary>
	    ///     Returns the given signature, remapped with the
	    ///     <see cref="SignatureVisitor" />
	    ///     returned by
	    ///     <see cref="CreateSignatureRemapper" />
	    ///     .
	    /// </summary>
	    /// <param name="signature">
	    ///     a <i>JavaTypeSignature</i>, <i>ClassSignature</i> or <i>MethodSignature</i>.
	    /// </param>
	    /// <param name="typeSignature">
	    ///     whether the given signature is a <i>JavaTypeSignature</i>.
	    /// </param>
	    /// <returns>
	    ///     signature the given signature, remapped with the
	    ///     <see cref="SignatureVisitor" />
	    ///     returned by
	    ///     <see cref="CreateSignatureRemapper" />
	    ///     .
	    /// </returns>
	    public virtual string MapSignature(string signature, bool typeSignature)
        {
            if (signature == null) return null;
            var signatureReader = new SignatureReader(signature);
            var signatureWriter = new SignatureWriter();
            var signatureRemapper = CreateSignatureRemapper(signatureWriter);
            if (typeSignature)
                signatureReader.AcceptType(signatureRemapper);
            else
                signatureReader.Accept(signatureRemapper);
            return signatureWriter.ToString();
        }

	    /// <summary>Constructs a new remapper for signatures.</summary>
	    /// <remarks>
	    ///     Constructs a new remapper for signatures. The default implementation of this method returns a
	    ///     new
	    ///     <see cref="SignatureRemapper" />
	    ///     .
	    /// </remarks>
	    /// <param name="signatureVisitor">
	    ///     the SignatureVisitor the remapper must delegate to.
	    /// </param>
	    /// <returns>the newly created remapper.</returns>
	    [Obsolete(@"use CreateSignatureRemapper(Org.Objectweb.Asm.Signature.SignatureVisitor) instead."
        )]
        protected internal virtual SignatureVisitor CreateRemappingSignatureAdapter(SignatureVisitor
            signatureVisitor)
        {
            return CreateSignatureRemapper(signatureVisitor);
        }

	    /// <summary>Constructs a new remapper for signatures.</summary>
	    /// <remarks>
	    ///     Constructs a new remapper for signatures. The default implementation of this method returns a
	    ///     new
	    ///     <see cref="SignatureRemapper" />
	    ///     .
	    /// </remarks>
	    /// <param name="signatureVisitor">
	    ///     the SignatureVisitor the remapper must delegate to.
	    /// </param>
	    /// <returns>the newly created remapper.</returns>
	    protected internal virtual SignatureVisitor CreateSignatureRemapper(SignatureVisitor
            signatureVisitor)
        {
            return new SignatureRemapper(signatureVisitor, this);
        }

	    /// <summary>Maps an inner class name to its new name.</summary>
	    /// <remarks>
	    ///     Maps an inner class name to its new name. The default implementation of this method provides a
	    ///     strategy that will work for inner classes produced by Java, but not necessarily other
	    ///     languages. Subclasses can override.
	    /// </remarks>
	    /// <param name="name">the fully-qualified internal name of the inner class.</param>
	    /// <param name="ownerName">the internal name of the owner class of the inner class.</param>
	    /// <param name="innerName">the internal name of the inner class.</param>
	    /// <returns>the new inner name of the inner class.</returns>
	    public virtual string MapInnerClassName(string name, string ownerName, string innerName
        )
        {
            var remappedInnerName = MapType(name);
            if (remappedInnerName.Contains("$"))
            {
                var index = remappedInnerName.LastIndexOf('$') + 1;
                while (index < remappedInnerName.Length && char.IsDigit(remappedInnerName[index])
                )
                    index++;
                return Runtime.Substring(remappedInnerName, index);
            }

            return innerName;
        }

	    /// <summary>Maps a method name to its new name.</summary>
	    /// <remarks>
	    ///     Maps a method name to its new name. The default implementation of this method returns the given
	    ///     name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="owner">the internal name of the owner class of the method.</param>
	    /// <param name="name">the name of the method.</param>
	    /// <param name="descriptor">the descriptor of the method.</param>
	    /// <returns>the new name of the method.</returns>
	    public virtual string MapMethodName(string owner, string name, string descriptor)
        {
            return name;
        }

	    /// <summary>
	    ///     Maps an invokedynamic or a constant dynamic method name to its new name.
	    /// </summary>
	    /// <remarks>
	    ///     Maps an invokedynamic or a constant dynamic method name to its new name. The default
	    ///     implementation of this method returns the given name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="name">the name of the method.</param>
	    /// <param name="descriptor">the descriptor of the method.</param>
	    /// <returns>the new name of the method.</returns>
	    public virtual string MapInvokeDynamicMethodName(string name, string descriptor)
        {
            return name;
        }

	    /// <summary>Maps a record component name to its new name.</summary>
	    /// <remarks>
	    ///     Maps a record component name to its new name. The default implementation of this method returns
	    ///     the given name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="owner">the internal name of the owner class of the field.</param>
	    /// <param name="name">the name of the field.</param>
	    /// <param name="descriptor">the descriptor of the field.</param>
	    /// <returns>the new name of the field.</returns>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual string MapRecordComponentNameExperimental(string owner, string name
            , string descriptor)
        {
            return name;
        }

	    /// <summary>Maps a field name to its new name.</summary>
	    /// <remarks>
	    ///     Maps a field name to its new name. The default implementation of this method returns the given
	    ///     name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="owner">the internal name of the owner class of the field.</param>
	    /// <param name="name">the name of the field.</param>
	    /// <param name="descriptor">the descriptor of the field.</param>
	    /// <returns>the new name of the field.</returns>
	    public virtual string MapFieldName(string owner, string name, string descriptor)
        {
            return name;
        }

	    /// <summary>Maps a package name to its new name.</summary>
	    /// <remarks>
	    ///     Maps a package name to its new name. The default implementation of this method returns the
	    ///     given name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="name">the fully qualified name of the package (using dots).</param>
	    /// <returns>the new name of the package.</returns>
	    public virtual string MapPackageName(string name)
        {
            return name;
        }

	    /// <summary>Maps a module name to its new name.</summary>
	    /// <remarks>
	    ///     Maps a module name to its new name. The default implementation of this method returns the given
	    ///     name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="name">the fully qualified name (using dots) of a module.</param>
	    /// <returns>the new name of the module.</returns>
	    public virtual string MapModuleName(string name)
        {
            return name;
        }

	    /// <summary>Maps the internal name of a class to its new name.</summary>
	    /// <remarks>
	    ///     Maps the internal name of a class to its new name. The default implementation of this method
	    ///     returns the given name, unchanged. Subclasses can override.
	    /// </remarks>
	    /// <param name="internalName">the internal name of a class.</param>
	    /// <returns>the new internal name.</returns>
	    public virtual string Map(string internalName)
        {
            return internalName;
        }
    }
}