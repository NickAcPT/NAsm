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

using System.Collections.Generic;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Asm.Signature;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="SignatureVisitor" />
    ///     that remaps types with a
    ///     <see cref="Remapper" />
    ///     .
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    public class SignatureRemapper : SignatureVisitor
    {
        private readonly List<string> classNames = new List<string>();
        private readonly Remapper remapper;
        private readonly SignatureVisitor signatureVisitor;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SignatureRemapper" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="SignatureRemapper(int, Org.Objectweb.Asm.Signature.SignatureVisitor, Remapper)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="signatureVisitor">
        ///     the signature visitor this remapper must deleted to.
        /// </param>
        /// <param name="remapper">
        ///     the remapper to use to remap the types in the visited signature.
        /// </param>
        public SignatureRemapper(SignatureVisitor signatureVisitor, Remapper remapper)
            : this(VisitorAsmApiVersion.Asm7, signatureVisitor, remapper)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SignatureRemapper" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this remapper. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     .
        /// </param>
        /// <param name="signatureVisitor">
        ///     the signature visitor this remapper must deleted to.
        /// </param>
        /// <param name="remapper">
        ///     the remapper to use to remap the types in the visited signature.
        /// </param>
        protected internal SignatureRemapper(VisitorAsmApiVersion api, SignatureVisitor signatureVisitor,
            Remapper remapper)
            : base(api)
        {
            /* latest api = */
            this.signatureVisitor = signatureVisitor;
            this.remapper = remapper;
        }

        public override void VisitClassType(string name)
        {
            classNames.Add(name);
            signatureVisitor.VisitClassType(remapper.MapType(name));
        }

        public override void VisitInnerClassType(string name)
        {
            var outerClassName = classNames.RemoveAtReturningValue(classNames.Count - 1);
            var className = outerClassName + '$' + name;
            classNames.Add(className);
            var remappedOuter = remapper.MapType(outerClassName) + '$';
            var remappedName = remapper.MapType(className);
            var index = remappedName.StartsWith(remappedOuter)
                ? remappedOuter.Length
                : remappedName
                      .LastIndexOf('$') + 1;
            signatureVisitor.VisitInnerClassType(Runtime.Substring(remappedName, index
            ));
        }

        public override void VisitFormalTypeParameter(string name)
        {
            signatureVisitor.VisitFormalTypeParameter(name);
        }

        public override void VisitTypeVariable(string name)
        {
            signatureVisitor.VisitTypeVariable(name);
        }

        public override SignatureVisitor VisitArrayType()
        {
            signatureVisitor.VisitArrayType();
            return this;
        }

        public override void VisitBaseType(char descriptor)
        {
            signatureVisitor.VisitBaseType(descriptor);
        }

        public override SignatureVisitor VisitClassBound()
        {
            signatureVisitor.VisitClassBound();
            return this;
        }

        public override SignatureVisitor VisitExceptionType()
        {
            signatureVisitor.VisitExceptionType();
            return this;
        }

        public override SignatureVisitor VisitInterface()
        {
            signatureVisitor.VisitInterface();
            return this;
        }

        public override SignatureVisitor VisitInterfaceBound()
        {
            signatureVisitor.VisitInterfaceBound();
            return this;
        }

        public override SignatureVisitor VisitParameterType()
        {
            signatureVisitor.VisitParameterType();
            return this;
        }

        public override SignatureVisitor VisitReturnType()
        {
            signatureVisitor.VisitReturnType();
            return this;
        }

        public override SignatureVisitor VisitSuperclass()
        {
            signatureVisitor.VisitSuperclass();
            return this;
        }

        public override void VisitTypeArgument()
        {
            signatureVisitor.VisitTypeArgument();
        }

        public override SignatureVisitor VisitTypeArgument(char wildcard)
        {
            signatureVisitor.VisitTypeArgument(wildcard);
            return this;
        }

        public override void VisitEnd()
        {
            signatureVisitor.VisitEnd();
            classNames.RemoveAtReturningValue(classNames.Count - 1);
        }
    }
}