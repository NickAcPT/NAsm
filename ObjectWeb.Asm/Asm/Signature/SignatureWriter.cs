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

using System.Text;

namespace ObjectWeb.Asm.Signature
{
    /// <summary>
    ///     A SignatureVisitor that generates signature literals, as defined in the Java Virtual Machine
    ///     Specification (JVMS).
    /// </summary>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1">
    ///         JVMS
    ///         *     4.7.9.1
    ///     </a>
    /// </seealso>
    /// <author>Thomas Hallgren</author>
    /// <author>Eric Bruneton</author>
    public class SignatureWriter : SignatureVisitor
    {
        /// <summary>The builder used to construct the visited signature.</summary>
        private readonly StringBuilder stringBuilder = new StringBuilder();

        /// <summary>The stack used to keep track of class types that have arguments.</summary>
        /// <remarks>
        ///     The stack used to keep track of class types that have arguments. Each element of this stack is
        ///     a boolean encoded in one bit. The top of the stack is the least significant bit. Pushing false
        ///     = *2, pushing true = *2+1, popping = /2.
        ///     <p>
        ///         Class type arguments must be surrounded with '&lt;' and '&gt;' and, because
        ///         <ol>
        ///             <li>
        ///                 class types can be nested (because type arguments can themselves be class types),
        ///                 <li>
        ///                     SignatureWriter always returns 'this' in each visit* method (to avoid allocating new
        ///                     SignatureWriter instances),
        ///         </ol>
        ///         <p>
        ///             we need a stack to properly balance these 'parentheses'. A new element is pushed on this
        ///             stack for each new visited type, and popped when the visit of this type ends (either is
        ///             visitEnd, or because visitInnerClassType is called).
        /// </remarks>
        private int argumentStack;

        /// <summary>Whether the visited signature contains formal type parameters.</summary>
        private bool hasFormals;

        /// <summary>Whether the visited signature contains method parameter types.</summary>
        private bool hasParameters;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SignatureWriter" />
        ///     .
        /// </summary>
        public SignatureWriter()
            : base(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7)
        {
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the SignatureVisitor interface
        // -----------------------------------------------------------------------------------------------
        public override void VisitFormalTypeParameter(string name)
        {
            if (!hasFormals)
            {
                hasFormals = true;
                stringBuilder.Append('<');
            }

            stringBuilder.Append(name);
            stringBuilder.Append(':');
        }

        public override SignatureVisitor VisitClassBound()
        {
            return this;
        }

        public override SignatureVisitor VisitInterfaceBound()
        {
            stringBuilder.Append(':');
            return this;
        }

        public override SignatureVisitor VisitSuperclass()
        {
            EndFormals();
            return this;
        }

        public override SignatureVisitor VisitInterface()
        {
            return this;
        }

        public override SignatureVisitor VisitParameterType()
        {
            EndFormals();
            if (!hasParameters)
            {
                hasParameters = true;
                stringBuilder.Append('(');
            }

            return this;
        }

        public override SignatureVisitor VisitReturnType()
        {
            EndFormals();
            if (!hasParameters) stringBuilder.Append('(');
            stringBuilder.Append(')');
            return this;
        }

        public override SignatureVisitor VisitExceptionType()
        {
            stringBuilder.Append('^');
            return this;
        }

        public override void VisitBaseType(char descriptor)
        {
            stringBuilder.Append(descriptor);
        }

        public override void VisitTypeVariable(string name)
        {
            stringBuilder.Append('T');
            stringBuilder.Append(name);
            stringBuilder.Append(';');
        }

        public override SignatureVisitor VisitArrayType()
        {
            stringBuilder.Append('[');
            return this;
        }

        public override void VisitClassType(string name)
        {
            stringBuilder.Append('L');
            stringBuilder.Append(name);
            // Pushes 'false' on the stack, meaning that this type does not have type arguments (as far as
            // we can tell at this point).
            argumentStack *= 2;
        }

        public override void VisitInnerClassType(string name)
        {
            EndArguments();
            stringBuilder.Append('.');
            stringBuilder.Append(name);
            // Pushes 'false' on the stack, meaning that this type does not have type arguments (as far as
            // we can tell at this point).
            argumentStack *= 2;
        }

        public override void VisitTypeArgument()
        {
            // If the top of the stack is 'false', this means we are visiting the first type argument of the
            // currently visited type. We therefore need to append a '<', and to replace the top stack
            // element with 'true' (meaning that the current type does have type arguments).
            if (argumentStack % 2 == 0)
            {
                argumentStack |= 1;
                stringBuilder.Append('<');
            }

            stringBuilder.Append('*');
        }

        public override SignatureVisitor VisitTypeArgument(char wildcard)
        {
            // If the top of the stack is 'false', this means we are visiting the first type argument of the
            // currently visited type. We therefore need to append a '<', and to replace the top stack
            // element with 'true' (meaning that the current type does have type arguments).
            if (argumentStack % 2 == 0)
            {
                argumentStack |= 1;
                stringBuilder.Append('<');
            }

            if (wildcard != '=') stringBuilder.Append(wildcard);
            return this;
        }

        public override void VisitEnd()
        {
            EndArguments();
            stringBuilder.Append(';');
        }

        /// <summary>Returns the signature that was built by this signature writer.</summary>
        /// <returns>the signature that was built by this signature writer.</returns>
        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Ends the formal type parameters section of the signature.</summary>
        private void EndFormals()
        {
            if (hasFormals)
            {
                hasFormals = false;
                stringBuilder.Append('>');
            }
        }

        /// <summary>Ends the type arguments of a class or inner class type.</summary>
        private void EndArguments()
        {
            // If the top of the stack is 'true', this means that some type arguments have been visited for
            // the type whose visit is now ending. We therefore need to append a '>', and to pop one element
            // from the stack.
            if (argumentStack % 2 == 1) stringBuilder.Append('>');
            argumentStack /= 2;
        }
    }
}