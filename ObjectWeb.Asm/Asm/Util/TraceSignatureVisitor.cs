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
using ObjectWeb.Asm.Signature;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="SignatureVisitor" />
    ///     that builds the Java generic type declaration corresponding to the
    ///     signature it visits.
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    /// <author>Eric Bruneton</author>
    public sealed class TraceSignatureVisitor : SignatureVisitor
    {
        private const string Comma_Separator = ", ";

        private const string Extends_Separator = " extends ";

        private const string Implements_Separator = " implements ";

        private static readonly IDictionary<char, string> Base_Types;

        /// <summary>
        ///     The Java generic type declaration corresponding to the visited signature.
        /// </summary>
        private readonly StringBuilder declaration;

        /// <summary>Whether the visited signature is a class signature of a Java interface.</summary>
        private readonly bool isInterface;

        /// <summary>The stack used to keep track of class types that have arguments.</summary>
        /// <remarks>
        ///     The stack used to keep track of class types that have arguments. Each element of this stack is
        ///     a boolean encoded in one bit. The top of the stack is the least significant bit. Pushing false
        ///     = *2, pushing true = *2+1, popping = /2.
        /// </remarks>
        private int argumentStack;

        /// <summary>The stack used to keep track of array class types.</summary>
        /// <remarks>
        ///     The stack used to keep track of array class types. Each element of this stack is a boolean
        ///     encoded in one bit. The top of the stack is the lowest order bit. Pushing false = *2, pushing
        ///     true = *2+1, popping = /2.
        /// </remarks>
        private int arrayStack;

        /// <summary>
        ///     The Java generic exception types declaration corresponding to the visited signature.
        /// </summary>
        private StringBuilder exceptions;

        /// <summary>
        ///     Whether
        ///     <see cref="VisitFormalTypeParameter(string)" />
        ///     has been called.
        /// </summary>
        private bool formalTypeParameterVisited;

        /// <summary>
        ///     Whether
        ///     <see cref="VisitInterfaceBound()" />
        ///     has been called.
        /// </summary>
        private bool interfaceBoundVisited;

        /// <summary>
        ///     Whether
        ///     <see cref="VisitInterface()" />
        ///     has been called.
        /// </summary>
        private bool interfaceVisited;

        /// <summary>
        ///     Whether
        ///     <see cref="VisitParameterType()" />
        ///     has been called.
        /// </summary>
        private bool parameterTypeVisited;

        /// <summary>
        ///     The Java generic method return type declaration corresponding to the visited signature.
        /// </summary>
        private StringBuilder returnType;

        /// <summary>
        ///     The separator to append before the next visited class or inner class type.
        /// </summary>
        private string separator = string.Empty;

        static TraceSignatureVisitor()
        {
            var baseTypes = new Dictionary<char, string>();
            Collections.Put(baseTypes, 'Z', "boolean");
            Collections.Put(baseTypes, 'B', "byte");
            Collections.Put(baseTypes, 'C', "char");
            Collections.Put(baseTypes, 'S', "short");
            Collections.Put(baseTypes, 'I', "int");
            Collections.Put(baseTypes, 'J', "long");
            Collections.Put(baseTypes, 'F', "float");
            Collections.Put(baseTypes, 'D', "double");
            Collections.Put(baseTypes, 'V', "void");
            Base_Types = baseTypes;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceSignatureVisitor" />
        ///     .
        /// </summary>
        /// <param name="accessFlags">
        ///     for class type signatures, the access flags of the class.
        /// </param>
        public TraceSignatureVisitor(ObjectWeb.Asm.Enums.AccessFlags accessFlags)
            : base(VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            isInterface = accessFlags.HasFlagFast(ObjectWeb.Asm.Enums.AccessFlags.Interface);
            declaration = new StringBuilder();
        }

        private TraceSignatureVisitor(StringBuilder stringBuilder)
            : base(VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            isInterface = false;
            declaration = stringBuilder;
        }

        public override void VisitFormalTypeParameter(string name)
        {
            declaration.Append(formalTypeParameterVisited ? Comma_Separator : "<").Append(name
            );
            formalTypeParameterVisited = true;
            interfaceBoundVisited = false;
        }

        public override SignatureVisitor VisitClassBound()
        {
            separator = Extends_Separator;
            StartType();
            return this;
        }

        public override SignatureVisitor VisitInterfaceBound()
        {
            separator = interfaceBoundVisited ? Comma_Separator : Extends_Separator;
            interfaceBoundVisited = true;
            StartType();
            return this;
        }

        public override SignatureVisitor VisitSuperclass()
        {
            EndFormals();
            separator = Extends_Separator;
            StartType();
            return this;
        }

        public override SignatureVisitor VisitInterface()
        {
            if (interfaceVisited)
            {
                separator = Comma_Separator;
            }
            else
            {
                separator = isInterface ? Extends_Separator : Implements_Separator;
                interfaceVisited = true;
            }

            StartType();
            return this;
        }

        public override SignatureVisitor VisitParameterType()
        {
            EndFormals();
            if (parameterTypeVisited)
            {
                declaration.Append(Comma_Separator);
            }
            else
            {
                declaration.Append('(');
                parameterTypeVisited = true;
            }

            StartType();
            return this;
        }

        public override SignatureVisitor VisitReturnType()
        {
            EndFormals();
            if (parameterTypeVisited)
                parameterTypeVisited = false;
            else
                declaration.Append('(');
            declaration.Append(')');
            returnType = new StringBuilder();
            return new TraceSignatureVisitor(returnType);
        }

        public override SignatureVisitor VisitExceptionType()
        {
            if (exceptions == null)
                exceptions = new StringBuilder();
            else
                exceptions.Append(Comma_Separator);
            return new TraceSignatureVisitor(exceptions);
        }

        public override void VisitBaseType(char descriptor)
        {
            var baseType = Base_Types.GetOrNull(descriptor);
            if (baseType == null) throw new ArgumentException();
            declaration.Append(baseType);
            EndType();
        }

        public override void VisitTypeVariable(string name)
        {
            declaration.Append(separator).Append(name);
            separator = string.Empty;
            EndType();
        }

        public override SignatureVisitor VisitArrayType()
        {
            StartType();
            arrayStack |= 1;
            return this;
        }

        public override void VisitClassType(string name)
        {
            if ("java/lang/Object".Equals(name))
            {
                // 'Map<java.lang.Object,java.util.List>' or 'abstract public V get(Object key);' should have
                // Object 'but java.lang.String extends java.lang.Object' is unnecessary.
                var needObjectClass = argumentStack % 2 != 0 || parameterTypeVisited;
                if (needObjectClass) declaration.Append(separator).Append(name.Replace('/', '.'));
            }
            else
            {
                declaration.Append(separator).Append(name.Replace('/', '.'));
            }

            separator = string.Empty;
            argumentStack *= 2;
        }

        public override void VisitInnerClassType(string name)
        {
            if (argumentStack % 2 != 0) declaration.Append('>');
            argumentStack /= 2;
            declaration.Append('.');
            declaration.Append(separator).Append(name.Replace('/', '.'));
            separator = string.Empty;
            argumentStack *= 2;
        }

        public override void VisitTypeArgument()
        {
            if (argumentStack % 2 == 0)
            {
                ++argumentStack;
                declaration.Append('<');
            }
            else
            {
                declaration.Append(Comma_Separator);
            }

            declaration.Append('?');
        }

        public override SignatureVisitor VisitTypeArgument(char tag)
        {
            if (argumentStack % 2 == 0)
            {
                ++argumentStack;
                declaration.Append('<');
            }
            else
            {
                declaration.Append(Comma_Separator);
            }

            if (tag == Extends)
                declaration.Append("? extends ");
            else if (tag == Super) declaration.Append("? super ");
            StartType();
            return this;
        }

        public override void VisitEnd()
        {
            if (argumentStack % 2 != 0) declaration.Append('>');
            argumentStack /= 2;
            EndType();
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the Java generic type declaration corresponding to the visited signature.
        /// </summary>
        /// <returns>
        ///     the Java generic type declaration corresponding to the visited signature.
        /// </returns>
        public string GetDeclaration()
        {
            return declaration.ToString();
        }

        /// <summary>
        ///     Returns the Java generic method return type declaration corresponding to the visited signature.
        /// </summary>
        /// <returns>
        ///     the Java generic method return type declaration corresponding to the visited signature.
        /// </returns>
        public string GetReturnType()
        {
            return returnType == null ? null : returnType.ToString();
        }

        /// <summary>
        ///     Returns the Java generic exception types declaration corresponding to the visited signature.
        /// </summary>
        /// <returns>
        ///     the Java generic exception types declaration corresponding to the visited signature.
        /// </returns>
        public string GetExceptions()
        {
            return exceptions == null ? null : exceptions.ToString();
        }

        // -----------------------------------------------------------------------------------------------
        private void EndFormals()
        {
            if (formalTypeParameterVisited)
            {
                declaration.Append('>');
                formalTypeParameterVisited = false;
            }
        }

        private void StartType()
        {
            arrayStack *= 2;
        }

        private void EndType()
        {
            if (arrayStack % 2 == 0)
                arrayStack /= 2;
            else
                while (arrayStack % 2 != 0)
                {
                    arrayStack /= 2;
                    declaration.Append("[]");
                }
        }
    }
}