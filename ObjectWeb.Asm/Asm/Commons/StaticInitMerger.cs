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

using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="ClassVisitor" />
    ///     that merges &lt;clinit&gt; methods into a single one. All the existing
    ///     &lt;clinit&gt; methods are renamed, and a new one is created, which calls all the renamed
    ///     methods.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class StaticInitMerger : ClassVisitor
    {
        /// <summary>The prefix to use to rename the existing &lt;clinit&gt; methods.</summary>
        private readonly string renamedClinitMethodPrefix;

        /// <summary>The MethodVisitor for the merged &lt;clinit&gt; method.</summary>
        private MethodVisitor mergedClinitVisitor;

        /// <summary>The number of &lt;clinit&gt; methods visited so far.</summary>
        private int numClinitMethods;

        /// <summary>The internal name of the visited class.</summary>
        private string owner;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="StaticInitMerger" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="StaticInitMerger(int,string,ObjectWeb.Asm.ClassVisitor)" />
        ///     version.
        /// </summary>
        /// <param name="prefix">
        ///     the prefix to use to rename the existing &lt;clinit&gt; methods.
        /// </param>
        /// <param name="classVisitor">
        ///     the class visitor to which this visitor must delegate method calls. May be
        ///     null.
        /// </param>
        public StaticInitMerger(string prefix, ClassVisitor classVisitor)
            : this(VisitorAsmApiVersion.Asm7, prefix, classVisitor)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="StaticInitMerger" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     .
        /// </param>
        /// <param name="prefix">
        ///     the prefix to use to rename the existing &lt;clinit&gt; methods.
        /// </param>
        /// <param name="classVisitor">
        ///     the class visitor to which this visitor must delegate method calls. May be
        ///     null.
        /// </param>
        protected internal StaticInitMerger(VisitorAsmApiVersion api, string prefix, ClassVisitor classVisitor
        )
            : base(api, classVisitor)
        {
            /* latest api = */
            renamedClinitMethodPrefix = prefix;
        }

        public override void Visit(int version, AccessFlags access, string name, string signature
            , string superName, string[] interfaces)
        {
            base.Visit(version, access, name, signature, superName, interfaces);
            owner = name;
        }

        public override MethodVisitor VisitMethod(AccessFlags access, string name, string descriptor
            , string signature, string[] exceptions)
        {
            MethodVisitor methodVisitor;
            if ("<clinit>".Equals(name))
            {
                var newAccess = AccessFlags.Private + (int) AccessFlags.Static;
                var newName = renamedClinitMethodPrefix + numClinitMethods++;
                methodVisitor = base.VisitMethod(newAccess, newName, descriptor, signature, exceptions
                );
                if (mergedClinitVisitor == null)
                    mergedClinitVisitor = base.VisitMethod(newAccess, name, descriptor, null, null);
                mergedClinitVisitor.VisitMethodInsn(OpcodesConstants.Invokestatic, owner, newName
                    , descriptor, false);
            }
            else
            {
                methodVisitor = base.VisitMethod(access, name, descriptor, signature, exceptions);
            }

            return methodVisitor;
        }

        public override void VisitEnd()
        {
            if (mergedClinitVisitor != null)
            {
                mergedClinitVisitor.VisitInsn(OpcodesConstants.Return);
                mergedClinitVisitor.VisitMaxs(0, 0);
            }

            base.VisitEnd();
        }
    }
}