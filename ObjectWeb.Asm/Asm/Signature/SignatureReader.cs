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

namespace ObjectWeb.Asm.Signature
{
    /// <summary>
    ///     A parser for signature literals, as defined in the Java Virtual Machine Specification (JVMS), to
    ///     visit them with a SignatureVisitor.
    /// </summary>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.9.1">
    ///         JVMS
    ///         *     4.7.9.1
    ///     </a>
    /// </seealso>
    /// <author>Thomas Hallgren</author>
    /// <author>Eric Bruneton</author>
    public class SignatureReader
    {
        /// <summary>The JVMS signature to be read.</summary>
        private readonly string signatureValue;

        /// <summary>
        ///     Constructs a
        ///     <see cref="SignatureReader" />
        ///     for the given signature.
        /// </summary>
        /// <param name="signature">
        ///     A <i>JavaTypeSignature</i>, <i>ClassSignature</i> or <i>MethodSignature</i>.
        /// </param>
        public SignatureReader(string signature)
        {
            signatureValue = signature;
        }

        /// <summary>
        ///     Makes the given visitor visit the signature of this
        ///     <see cref="SignatureReader" />
        ///     . This signature is
        ///     the one specified in the constructor (see
        ///     <see cref="SignatureReader(string)" />
        ///     ). This method is intended to
        ///     be called on a
        ///     <see cref="SignatureReader" />
        ///     that was created using a <i>ClassSignature</i> (such as
        ///     the <code>signature</code> parameter of the
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.Visit(int, int, string, string, string, string[])
        /// 	" />
        ///     method) or a <i>MethodSignature</i> (such as the <code>signature</code> parameter of the
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitMethod(int, string, string, string, string[])
        /// 	" />
        ///     method).
        /// </summary>
        /// <param name="signatureVistor">the visitor that must visit this signature.</param>
        public virtual void Accept(SignatureVisitor signatureVistor)
        {
            var signature = signatureValue;
            var length = signature.Length;
            int offset;
            // Current offset in the parsed signature (parsed from left to right).
            char currentChar;
            // The signature character at 'offset', or just before.
            // If the signature starts with '<', it starts with TypeParameters, i.e. a formal type parameter
            // identifier, followed by one or more pair ':',ReferenceTypeSignature (for its class bound and
            // interface bounds).
            if (signature[0] == '<')
            {
                // Invariant: offset points to the second character of a formal type parameter name at the
                // beginning of each iteration of the loop below.
                offset = 2;
                do
                {
                    // The formal type parameter name is everything between offset - 1 and the first ':'.
                    var classBoundStartOffset = signature.IndexOf(':', offset);
                    signatureVistor.VisitFormalTypeParameter(Runtime.Substring(signature, offset
                                                                                          - 1, classBoundStartOffset));
                    // If the character after the ':' class bound marker is not the start of a
                    // ReferenceTypeSignature, it means the class bound is empty (which is a valid case).
                    offset = classBoundStartOffset + 1;
                    currentChar = signature[offset];
                    if (currentChar == 'L' || currentChar == '[' || currentChar == 'T')
                        offset = ParseType(signature, offset, signatureVistor.VisitClassBound());
                    // While the character after the class bound or after the last parsed interface bound
                    // is ':', we need to parse another interface bound.
                    while ((currentChar = signature[offset++]) == ':')
                        offset = ParseType(signature, offset, signatureVistor.VisitInterfaceBound());
                } while (currentChar != '>');
            }
            else
            {
                // At this point a TypeParameter has been fully parsed, and we need to parse the next one
                // (note that currentChar is now the first character of the next TypeParameter, and that
                // offset points to the second character), unless the character just after this
                // TypeParameter signals the end of the TypeParameters.
                offset = 0;
            }

            // If the (optional) TypeParameters is followed by '(' this means we are parsing a
            // MethodSignature, which has JavaTypeSignature type inside parentheses, followed by a Result
            // type and optional ThrowsSignature types.
            if (signature[offset] == '(')
            {
                offset++;
                while (signature[offset] != ')')
                    offset = ParseType(signature, offset, signatureVistor.VisitParameterType());
                // Use offset + 1 to skip ')'.
                offset = ParseType(signature, offset + 1, signatureVistor.VisitReturnType());
                while (offset < length)
                    // Use offset + 1 to skip the first character of a ThrowsSignature, i.e. '^'.
                    offset = ParseType(signature, offset + 1, signatureVistor.VisitExceptionType());
            }
            else
            {
                // Otherwise we are parsing a ClassSignature (by hypothesis on the method input), which has
                // one or more ClassTypeSignature for the super class and the implemented interfaces.
                offset = ParseType(signature, offset, signatureVistor.VisitSuperclass());
                while (offset < length) offset = ParseType(signature, offset, signatureVistor.VisitInterface());
            }
        }

        /// <summary>
        ///     Makes the given visitor visit the signature of this
        ///     <see cref="SignatureReader" />
        ///     . This signature is
        ///     the one specified in the constructor (see
        ///     <see cref="SignatureReader(string)" />
        ///     ). This method is intended to
        ///     be called on a
        ///     <see cref="SignatureReader" />
        ///     that was created using a <i>JavaTypeSignature</i>, such
        ///     as the <code>signature</code> parameter of the
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitField(int, string, string, string, object)
        /// 	" />
        ///     or
        ///     <see
        ///         cref="Org.Objectweb.Asm.MethodVisitor.VisitLocalVariable(string, string, string, Org.Objectweb.Asm.Label, Org.Objectweb.Asm.Label, int)
        /// 	" />
        ///     methods.
        /// </summary>
        /// <param name="signatureVisitor">the visitor that must visit this signature.</param>
        public virtual void AcceptType(SignatureVisitor signatureVisitor)
        {
            ParseType(signatureValue, 0, signatureVisitor);
        }

        /// <summary>Parses a JavaTypeSignature and makes the given visitor visit it.</summary>
        /// <param name="signature">a string containing the signature that must be parsed.</param>
        /// <param name="startOffset">
        ///     index of the first character of the signature to parsed.
        /// </param>
        /// <param name="signatureVisitor">the visitor that must visit this signature.</param>
        /// <returns>the index of the first character after the parsed signature.</returns>
        private static int ParseType(string signature, int startOffset, SignatureVisitor
            signatureVisitor)
        {
            var offset = startOffset;
            // Current offset in the parsed signature.
            var currentChar = signature[offset++];
            switch (currentChar)
            {
                case 'Z':
                case 'C':
                case 'B':
                case 'S':
                case 'I':
                case 'F':
                case 'J':
                case 'D':
                case 'V':
                {
                    // The signature character at 'offset'.
                    // Switch based on the first character of the JavaTypeSignature, which indicates its kind.
                    // Case of a BaseType or a VoidDescriptor.
                    signatureVisitor.VisitBaseType(currentChar);
                    return offset;
                }

                case '[':
                {
                    // Case of an ArrayTypeSignature, a '[' followed by a JavaTypeSignature.
                    return ParseType(signature, offset, signatureVisitor.VisitArrayType());
                }

                case 'T':
                {
                    // Case of TypeVariableSignature, an identifier between 'T' and ';'.
                    var endOffset = signature.IndexOf(';', offset);
                    signatureVisitor.VisitTypeVariable(Runtime.Substring(signature, offset, endOffset
                    ));
                    return endOffset + 1;
                }

                case 'L':
                {
                    // Case of a ClassTypeSignature, which ends with ';'.
                    // These signatures have a main class type followed by zero or more inner class types
                    // (separated by '.'). Each can have type arguments, inside '<' and '>'.
                    var start = offset;
                    // The start offset of the currently parsed main or inner class name.
                    var visited = false;
                    // Whether the currently parsed class name has been visited.
                    var inner = false;
                    // Whether we are currently parsing an inner class type.
                    // Parses the signature, one character at a time.
                    while (true)
                    {
                        currentChar = signature[offset++];
                        if (currentChar == '.' || currentChar == ';')
                        {
                            // If a '.' or ';' is encountered, this means we have fully parsed the main class name
                            // or an inner class name. This name may already have been visited it is was followed by
                            // type arguments between '<' and '>'. If not, we need to visit it here.
                            if (!visited)
                            {
                                var name = Runtime.Substring(signature, start, offset - 1);
                                if (inner)
                                    signatureVisitor.VisitInnerClassType(name);
                                else
                                    signatureVisitor.VisitClassType(name);
                            }

                            // If we reached the end of the ClassTypeSignature return, otherwise start the parsing
                            // of a new class name, which is necessarily an inner class name.
                            if (currentChar == ';')
                            {
                                signatureVisitor.VisitEnd();
                                break;
                            }

                            start = offset;
                            visited = false;
                            inner = true;
                        }
                        else if (currentChar == '<')
                        {
                            // If a '<' is encountered, this means we have fully parsed the main class name or an
                            // inner class name, and that we now need to parse TypeArguments. First, we need to
                            // visit the parsed class name.
                            var name = Runtime.Substring(signature, start, offset - 1);
                            if (inner)
                                signatureVisitor.VisitInnerClassType(name);
                            else
                                signatureVisitor.VisitClassType(name);
                            visited = true;
                            // Now, parse the TypeArgument(s), one at a time.
                            while ((currentChar = signature[offset]) != '>')
                                switch (currentChar)
                                {
                                    case '*':
                                    {
                                        // Unbounded TypeArgument.
                                        ++offset;
                                        signatureVisitor.VisitTypeArgument();
                                        break;
                                    }

                                    case '+':
                                    case '-':
                                    {
                                        // Extends or Super TypeArgument. Use offset + 1 to skip the '+' or '-'.
                                        offset = ParseType(signature, offset + 1, signatureVisitor.VisitTypeArgument(
                                            currentChar
                                        ));
                                        break;
                                    }

                                    default:
                                    {
                                        // Instanceof TypeArgument. The '=' is implicit.
                                        offset = ParseType(signature, offset, signatureVisitor.VisitTypeArgument('='));
                                        break;
                                    }
                                }
                        }
                    }

                    return offset;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}