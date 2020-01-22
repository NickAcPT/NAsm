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

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="ModuleVisitor" />
    ///     that prints the fields it visits with a
    ///     <see cref="Printer" />
    ///     .
    /// </summary>
    /// <author>Remi Forax</author>
    public sealed class TraceModuleVisitor : ModuleVisitor
    {
        /// <summary>The printer to convert the visited module into text.</summary>
        public readonly Printer p;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceModuleVisitor" />
        ///     .
        /// </summary>
        /// <param name="printer">the printer to convert the visited module into text.</param>
        public TraceModuleVisitor(Printer printer)
            : this(null, printer)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceModuleVisitor" />
        ///     .
        /// </summary>
        /// <param name="moduleVisitor">
        ///     the module visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printer">the printer to convert the visited module into text.</param>
        public TraceModuleVisitor(ModuleVisitor moduleVisitor, Printer printer)
            : base(VisitorAsmApiVersion.Asm7, moduleVisitor)
        {
            // DontCheck(MemberName): can't be renamed (for backward binary compatibility).
            /* latest api = */
            p = printer;
        }

        public override void VisitMainClass(string mainClass)
        {
            p.VisitMainClass(mainClass);
            base.VisitMainClass(mainClass);
        }

        public override void VisitPackage(string packaze)
        {
            p.VisitPackage(packaze);
            base.VisitPackage(packaze);
        }

        public override void VisitRequire(string module, ObjectWeb.Asm.Enums.AccessFlags access, string version)
        {
            p.VisitRequire(module, access, version);
            base.VisitRequire(module, access, version);
        }

        public override void VisitExport(string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params string[] modules
        )
        {
            p.VisitExport(packaze, access, modules);
            base.VisitExport(packaze, access, modules);
        }

        public override void VisitOpen(string packaze, ObjectWeb.Asm.Enums.AccessFlags access, params string[] modules
        )
        {
            p.VisitOpen(packaze, access, modules);
            base.VisitOpen(packaze, access, modules);
        }

        public override void VisitUse(string use)
        {
            p.VisitUse(use);
            base.VisitUse(use);
        }

        public override void VisitProvide(string service, params string[] providers)
        {
            p.VisitProvide(service, providers);
            base.VisitProvide(service, providers);
        }

        public override void VisitEnd()
        {
            p.VisitModuleEnd();
            base.VisitEnd();
        }
    }
}