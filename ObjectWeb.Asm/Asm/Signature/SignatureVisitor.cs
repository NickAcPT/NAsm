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
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Signature
{
	/// <summary>A visitor to visit a generic signature.</summary>
	/// <remarks>
	///     A visitor to visit a generic signature. The methods of this interface must be called in one of
	///     the three following orders (the last one is the only valid order for a
	///     <see cref="SignatureVisitor" />
	///     that is returned by a method of this interface):
	///     <ul>
	///         <li>
	///             <i>ClassSignature</i> = (
	///             <c>visitFormalTypeParameter</c>
	///             <c>visitClassBound</c>
	///             ?
	///             <c>visitInterfaceBound</c>
	///             * )* (
	///             <c>visitSuperclass</c>
	///             <c>visitInterface</c>
	///             * )
	///             <li>
	///                 <i>MethodSignature</i> = (
	///                 <c>visitFormalTypeParameter</c>
	///                 <c>visitClassBound</c>
	///                 ?
	///                 <c>visitInterfaceBound</c>
	///                 * )* (
	///                 <c>visitParameterType</c>
	///                 *
	///                 <c>visitReturnType</c>
	///                 <c>visitExceptionType</c>
	///                 * )
	///                 <li>
	///                     <i>TypeSignature</i> =
	///                     <c>visitBaseType</c>
	///                     |
	///                     <c>visitTypeVariable</c>
	///                     |
	///                     <c>visitArrayType</c>
	///                     | (
	///                     <c>visitClassType</c>
	///                     <c>visitTypeArgument</c>
	///                     * (
	///                     <c>visitInnerClassType</c>
	///                     <c>visitTypeArgument</c>
	///                     * )*
	///                     <c>visitEnd</c>
	///                     ) )
	///     </ul>
	/// </remarks>
	/// <author>Thomas Hallgren</author>
	/// <author>Eric Bruneton</author>
	public abstract class SignatureVisitor
    {
        /// <summary>Wildcard for an "extends" type argument.</summary>
        public const char Extends = '+';

        /// <summary>Wildcard for a "super" type argument.</summary>
        public const char Super = '-';

        /// <summary>Wildcard for a normal type argument.</summary>
        public const char Instanceof = '=';

        /// <summary>The ASM API version implemented by this visitor.</summary>
        /// <remarks>
        ///     The ASM API version implemented by this visitor. The value of this field must be one of
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

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SignatureVisitor" />
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
        public SignatureVisitor(VisitorAsmApiVersion api)
        {
            if (api != VisitorAsmApiVersion.Asm7 && api != VisitorAsmApiVersion.Asm6 && api != VisitorAsmApiVersion
                    .Asm5 && api != VisitorAsmApiVersion.Asm4 && api != VisitorAsmApiVersion.Asm8Experimental)
                throw new ArgumentException("Unsupported api " + api);
            this.api = api;
        }

        /// <summary>Visits a formal type parameter.</summary>
        /// <param name="name">the name of the formal parameter.</param>
        public virtual void VisitFormalTypeParameter(string name)
        {
        }

        /// <summary>Visits the class bound of the last visited formal type parameter.</summary>
        /// <returns>a non null visitor to visit the signature of the class bound.</returns>
        public virtual SignatureVisitor VisitClassBound()
        {
            return this;
        }

        /// <summary>Visits an interface bound of the last visited formal type parameter.</summary>
        /// <returns>a non null visitor to visit the signature of the interface bound.</returns>
        public virtual SignatureVisitor VisitInterfaceBound()
        {
            return this;
        }

        /// <summary>Visits the type of the super class.</summary>
        /// <returns>a non null visitor to visit the signature of the super class type.</returns>
        public virtual SignatureVisitor VisitSuperclass()
        {
            return this;
        }

        /// <summary>Visits the type of an interface implemented by the class.</summary>
        /// <returns>a non null visitor to visit the signature of the interface type.</returns>
        public virtual SignatureVisitor VisitInterface()
        {
            return this;
        }

        /// <summary>Visits the type of a method parameter.</summary>
        /// <returns>a non null visitor to visit the signature of the parameter type.</returns>
        public virtual SignatureVisitor VisitParameterType()
        {
            return this;
        }

        /// <summary>Visits the return type of the method.</summary>
        /// <returns>a non null visitor to visit the signature of the return type.</returns>
        public virtual SignatureVisitor VisitReturnType()
        {
            return this;
        }

        /// <summary>Visits the type of a method exception.</summary>
        /// <returns>a non null visitor to visit the signature of the exception type.</returns>
        public virtual SignatureVisitor VisitExceptionType()
        {
            return this;
        }

        /// <summary>Visits a signature corresponding to a primitive type.</summary>
        /// <param name="descriptor">
        ///     the descriptor of the primitive type, or 'V' for
        ///     <c>void</c>
        ///     .
        /// </param>
        public virtual void VisitBaseType(char descriptor)
        {
        }

        /// <summary>Visits a signature corresponding to a type variable.</summary>
        /// <param name="name">the name of the type variable.</param>
        public virtual void VisitTypeVariable(string name)
        {
        }

        /// <summary>Visits a signature corresponding to an array type.</summary>
        /// <returns>a non null visitor to visit the signature of the array element type.</returns>
        public virtual SignatureVisitor VisitArrayType()
        {
            return this;
        }

        /// <summary>
        ///     Starts the visit of a signature corresponding to a class or interface type.
        /// </summary>
        /// <param name="name">the internal name of the class or interface.</param>
        public virtual void VisitClassType(string name)
        {
        }

        /// <summary>Visits an inner class.</summary>
        /// <param name="name">the local name of the inner class in its enclosing class.</param>
        public virtual void VisitInnerClassType(string name)
        {
        }

        /// <summary>
        ///     Visits an unbounded type argument of the last visited class or inner class type.
        /// </summary>
        public virtual void VisitTypeArgument()
        {
        }

        /// <summary>Visits a type argument of the last visited class or inner class type.</summary>
        /// <param name="wildcard">'+', '-' or '='.</param>
        /// <returns>a non null visitor to visit the signature of the type argument.</returns>
        public virtual SignatureVisitor VisitTypeArgument(char wildcard)
        {
            return this;
        }

        /// <summary>
        ///     Ends the visit of a signature corresponding to a class or interface type.
        /// </summary>
        public virtual void VisitEnd()
        {
        }
    }
}