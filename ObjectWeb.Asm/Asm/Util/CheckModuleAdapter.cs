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
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="ModuleVisitor" />
    ///     that checks that its methods are properly used.
    /// </summary>
    /// <author>Remi Forax</author>
    public class CheckModuleAdapter : ModuleVisitor
    {
        /// <summary>The internal names of the packages exported by the visited module.</summary>
        private readonly NameSet exportedPackages = new NameSet
            ("Module exports");

        /// <summary>Whether the visited module is open.</summary>
        private readonly bool isOpen;

        /// <summary>The internal names of the packages opened by the visited module.</summary>
        private readonly NameSet openedPackages = new NameSet
            ("Module opens");

        /// <summary>The internal names of the services provided by the visited module.</summary>
        private readonly NameSet providedServices = new NameSet
            ("Module provides");

        /// <summary>The fully qualified names of the dependencies of the visited module.</summary>
        private readonly NameSet requiredModules = new NameSet
            ("Modules requires");

        /// <summary>The internal names of the services used by the visited module.</summary>
        private readonly NameSet usedServices = new NameSet
            ("Module uses");

        /// <summary>The class version number.</summary>
        internal int classVersion;

        /// <summary>
        ///     Whether the
        ///     <see cref="VisitEnd()" />
        ///     method has been called.
        /// </summary>
        private bool visitEndCalled;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckModuleAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="CheckModuleAdapter(int, Org.Objectweb.Asm.ModuleVisitor, bool)" />
        ///     version.
        /// </summary>
        /// <param name="moduleVisitor">
        ///     the module visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="isOpen">
        ///     whether the visited module is open. Open modules have their
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Acc_Open" />
        ///     access flag set in
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitModule(string, int, string)" />
        ///     .
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public CheckModuleAdapter(ModuleVisitor moduleVisitor, bool isOpen)
            : this(VisitorAsmApiVersion.Asm7, moduleVisitor, isOpen)
        {
            /* latest api = */
            if (GetType() != typeof(CheckModuleAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckModuleAdapter" />
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
        /// <param name="moduleVisitor">
        ///     the module visitor to which this adapter must delegate calls.
        /// </param>
        /// <param name="isOpen">
        ///     whether the visited module is open. Open modules have their
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Acc_Open" />
        ///     access flag set in
        ///     <see cref="Org.Objectweb.Asm.ClassVisitor.VisitModule(string, int, string)" />
        ///     .
        /// </param>
        protected internal CheckModuleAdapter(VisitorAsmApiVersion api, ModuleVisitor moduleVisitor, bool
            isOpen)
            : base(api, moduleVisitor)
        {
            this.isOpen = isOpen;
        }

        public override void VisitMainClass(string mainClass)
        {
            // Modules can only appear in V9 or more classes.
            CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, mainClass, "module main class"
            );
            base.VisitMainClass(mainClass);
        }

        public override void VisitPackage(string packaze)
        {
            CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, packaze, "module package"
            );
            base.VisitPackage(packaze);
        }

        public override void VisitRequire(string module, ObjectWeb.Asm.Enums.AccessFlags access, string version)
        {
            CheckVisitEndNotCalled();
            CheckClassAdapter.CheckFullyQualifiedName(OpcodesConstants.V9, module, "required module"
            );
            requiredModules.CheckNameNotAlreadyDeclared(module);
            CheckClassAdapter.CheckAccess(access, ObjectWeb.Asm.Enums.AccessFlags.StaticPhase | ObjectWeb.Asm.Enums.AccessFlags
                                                      .Transitive | ObjectWeb.Asm.Enums.AccessFlags.Synthetic |
                                                  ObjectWeb.Asm.Enums.AccessFlags.Mandated
            );
            if (classVersion >= OpcodesConstants.V10 && module.Equals("java.base") && (access
                                                                                       & (ObjectWeb.Asm.Enums.AccessFlags
                                                                                              .StaticPhase |
                                                                                          ObjectWeb.Asm.Enums.AccessFlags
                                                                                              .Transitive)) != 0)
                throw new ArgumentException(
                    "Invalid access flags: " + access +
                    " java.base can not be declared ACC_TRANSITIVE or ACC_STATIC_PHASE"
                );
            base.VisitRequire(module, access, version);
        }

        public override void VisitExport(string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params string[] modules
        )
        {
            CheckVisitEndNotCalled();
            CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, packaze, "package name"
            );
            exportedPackages.CheckNameNotAlreadyDeclared(packaze);
            CheckClassAdapter.CheckAccess(access, ObjectWeb.Asm.Enums.AccessFlags.Synthetic | ObjectWeb.Asm.Enums.AccessFlags
                                                      .Mandated);
            if (modules != null)
                foreach (var module in modules)
                    CheckClassAdapter.CheckFullyQualifiedName(OpcodesConstants.V9, module, "module export to"
                    );
            base.VisitExport(packaze, access, modules);
        }

        public override void VisitOpen(string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params string[] modules
        )
        {
            CheckVisitEndNotCalled();
            if (isOpen) throw new NotSupportedException("An open module can not use open directive");
            CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, packaze, "package name"
            );
            openedPackages.CheckNameNotAlreadyDeclared(packaze);
            CheckClassAdapter.CheckAccess(access, ObjectWeb.Asm.Enums.AccessFlags.Synthetic | ObjectWeb.Asm.Enums.AccessFlags
                                                      .Mandated);
            if (modules != null)
                foreach (var module in modules)
                    CheckClassAdapter.CheckFullyQualifiedName(OpcodesConstants.V9, module, "module open to"
                    );
            base.VisitOpen(packaze, access, modules);
        }

        public override void VisitUse(string service)
        {
            CheckVisitEndNotCalled();
            CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, service, "service");
            usedServices.CheckNameNotAlreadyDeclared(service);
            base.VisitUse(service);
        }

        public override void VisitProvide(string service, params string[] providers)
        {
            CheckVisitEndNotCalled();
            CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, service, "service");
            providedServices.CheckNameNotAlreadyDeclared(service);
            if (providers == null || providers.Length == 0)
                throw new ArgumentException("Providers cannot be null or empty");
            foreach (var provider in providers)
                CheckMethodAdapter.CheckInternalName(OpcodesConstants.V9, provider, "provider");
            base.VisitProvide(service, providers);
        }

        public override void VisitEnd()
        {
            CheckVisitEndNotCalled();
            visitEndCalled = true;
            base.VisitEnd();
        }

        private void CheckVisitEndNotCalled()
        {
            if (visitEndCalled)
                throw new InvalidOperationException("Cannot call a visit method after visitEnd has been called"
                );
        }

        private class NameSet
        {
            private readonly HashSet<string> names;
            private readonly string type;

            internal NameSet(string type)
            {
                this.type = type;
                names = new HashSet<string>();
            }

            internal virtual void CheckNameNotAlreadyDeclared(string name)
            {
                if (!names.Add(name)) throw new ArgumentException(type + " '" + name + "' already declared");
            }
        }
    }
}