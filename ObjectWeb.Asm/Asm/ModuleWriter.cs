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

namespace ObjectWeb.Asm
{
    /// <summary>
    ///     A
    ///     <see cref="ModuleVisitor" />
    ///     that generates the corresponding Module, ModulePackages and
    ///     ModuleMainClass attributes, as defined in the Java Virtual Machine Specification (JVMS).
    /// </summary>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.25">
    ///         JVMS
    ///         *     4.7.25
    ///     </a>
    /// </seealso>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.26">
    ///         JVMS
    ///         *     4.7.26
    ///     </a>
    /// </seealso>
    /// <seealso>
    ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.27">
    ///         JVMS
    ///         *     4.7.27
    ///     </a>
    /// </seealso>
    /// <author>Remi Forax</author>
    /// <author>Eric Bruneton</author>
    internal sealed class ModuleWriter : ModuleVisitor
    {
        /// <summary>The binary content of the 'exports' array of the JVMS Module attribute.</summary>
        private readonly ByteVector exports;

        /// <summary>The module_flags field of the JVMS Module attribute.</summary>
        private readonly int moduleFlags;

        /// <summary>The module_name_index field of the JVMS Module attribute.</summary>
        private readonly int moduleNameIndex;

        /// <summary>The module_version_index field of the JVMS Module attribute.</summary>
        private readonly int moduleVersionIndex;

        /// <summary>The binary content of the 'opens' array of the JVMS Module attribute.</summary>
        private readonly ByteVector opens;

        /// <summary>
        ///     The binary content of the 'package_index' array of the JVMS ModulePackages attribute.
        /// </summary>
        private readonly ByteVector packageIndex;

        /// <summary>
        ///     The binary content of the 'provides' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector provides;

        /// <summary>
        ///     The binary content of the 'requires' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector requires;

        /// <summary>Where the constants used in this AnnotationWriter must be stored.</summary>
        private readonly SymbolTable symbolTable;

        /// <summary>
        ///     The binary content of the 'uses_index' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector usesIndex;

        /// <summary>The exports_count field of the JVMS Module attribute.</summary>
        private int exportsCount;

        /// <summary>The main_class_index field of the JVMS ModuleMainClass attribute, or 0.</summary>
        private int mainClassIndex;

        /// <summary>The opens_count field of the JVMS Module attribute.</summary>
        private int opensCount;

        /// <summary>The provides_count field of the JVMS ModulePackages attribute.</summary>
        private int packageCount;

        /// <summary>The provides_count field of the JVMS Module attribute.</summary>
        private int providesCount;

        /// <summary>The requires_count field of the JVMS Module attribute.</summary>
        private int requiresCount;

        /// <summary>The uses_count field of the JVMS Module attribute.</summary>
        private int usesCount;

        internal ModuleWriter(SymbolTable symbolTable, int name, int access, int version)
            : base(VisitorAsmApiVersion.Asm7)
        {
            /* latest api = */
            this.symbolTable = symbolTable;
            moduleNameIndex = name;
            moduleFlags = access;
            moduleVersionIndex = version;
            requires = new ByteVector();
            exports = new ByteVector();
            opens = new ByteVector();
            usesIndex = new ByteVector();
            provides = new ByteVector();
            packageIndex = new ByteVector();
        }

        public override void VisitMainClass(string mainClass)
        {
            mainClassIndex = symbolTable.AddConstantClass(mainClass).index;
        }

        public override void VisitPackage(string packaze)
        {
            packageIndex.PutShort(symbolTable.AddConstantPackage(packaze).index);
            packageCount++;
        }

        public override void VisitRequire(string module, int access, string version)
        {
            requires.PutShort(symbolTable.AddConstantModule(module).index).PutShort(access).PutShort
                (version == null ? 0 : symbolTable.AddConstantUtf8(version));
            requiresCount++;
        }

        public override void VisitExport(string packaze, int access, params string[] modules
        )
        {
            exports.PutShort(symbolTable.AddConstantPackage(packaze).index).PutShort(access);
            if (modules == null)
            {
                exports.PutShort(0);
            }
            else
            {
                exports.PutShort(modules.Length);
                foreach (var module in modules) exports.PutShort(symbolTable.AddConstantModule(module).index);
            }

            exportsCount++;
        }

        public override void VisitOpen(string packaze, int access, params string[] modules
        )
        {
            opens.PutShort(symbolTable.AddConstantPackage(packaze).index).PutShort(access);
            if (modules == null)
            {
                opens.PutShort(0);
            }
            else
            {
                opens.PutShort(modules.Length);
                foreach (var module in modules) opens.PutShort(symbolTable.AddConstantModule(module).index);
            }

            opensCount++;
        }

        public override void VisitUse(string service)
        {
            usesIndex.PutShort(symbolTable.AddConstantClass(service).index);
            usesCount++;
        }

        public override void VisitProvide(string service, params string[] providers)
        {
            provides.PutShort(symbolTable.AddConstantClass(service).index);
            provides.PutShort(providers.Length);
            foreach (var provider in providers) provides.PutShort(symbolTable.AddConstantClass(provider).index);
            providesCount++;
        }

        public override void VisitEnd()
        {
        }

        // Nothing to do.
        /// <summary>
        ///     Returns the number of Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter.
        /// </summary>
        /// <returns>
        ///     the number of Module, ModulePackages and ModuleMainClass attributes (between 1 and 3).
        /// </returns>
        internal int GetAttributeCount()
        {
            return 1 + (packageCount > 0 ? 1 : 0) + (mainClassIndex > 0 ? 1 : 0);
        }

        /// <summary>
        ///     Returns the size of the Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter.
        /// </summary>
        /// <remarks>
        ///     Returns the size of the Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter. Also add the names of these attributes in the constant pool.
        /// </remarks>
        /// <returns>
        ///     the size in bytes of the Module, ModulePackages and ModuleMainClass attributes.
        /// </returns>
        internal int ComputeAttributesSize()
        {
            symbolTable.AddConstantUtf8(Constants.Module);
            // 6 attribute header bytes, 6 bytes for name, flags and version, and 5 * 2 bytes for counts.
            var size = 22 + requires.length + exports.length + opens.length + usesIndex.length
                       + provides.length;
            if (packageCount > 0)
            {
                symbolTable.AddConstantUtf8(Constants.Module_Packages);
                // 6 attribute header bytes, and 2 bytes for package_count.
                size += 8 + packageIndex.length;
            }

            if (mainClassIndex > 0)
            {
                symbolTable.AddConstantUtf8(Constants.Module_Main_Class);
                // 6 attribute header bytes, and 2 bytes for main_class_index.
                size += 8;
            }

            return size;
        }

        /// <summary>
        ///     Puts the Module, ModulePackages and ModuleMainClass attributes generated by this ModuleWriter
        ///     in the given ByteVector.
        /// </summary>
        /// <param name="output">where the attributes must be put.</param>
        internal void PutAttributes(ByteVector output)
        {
            // 6 bytes for name, flags and version, and 5 * 2 bytes for counts.
            var moduleAttributeLength = 16 + requires.length + exports.length + opens.length
                                        + usesIndex.length + provides.length;
            output.PutShort(symbolTable.AddConstantUtf8(Constants.Module)).PutInt(moduleAttributeLength
            ).PutShort(moduleNameIndex).PutShort(moduleFlags).PutShort(moduleVersionIndex).PutShort
                (requiresCount).PutByteArray(requires.data, 0, requires.length).PutShort(exportsCount
            ).PutByteArray(exports.data, 0, exports.length).PutShort(opensCount).PutByteArray
                (opens.data, 0, opens.length).PutShort(usesCount).PutByteArray(usesIndex.data, 0
                , usesIndex.length).PutShort(providesCount).PutByteArray(provides.data, 0, provides
                .length);
            if (packageCount > 0)
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Module_Packages)).PutInt(2
                                                                                               + packageIndex.length)
                    .PutShort(packageCount).PutByteArray(packageIndex.data, 0,
                        packageIndex.length);
            if (mainClassIndex > 0)
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Module_Main_Class)).PutInt(
                    2).PutShort(mainClassIndex);
        }
    }
}