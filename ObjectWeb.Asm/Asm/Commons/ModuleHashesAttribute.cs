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

namespace ObjectWeb.Asm.Commons
{
    /// <summary>A ModuleHashes attribute.</summary>
    /// <remarks>
    ///     A ModuleHashes attribute. This attribute is specific to the OpenJDK and may change in the future.
    /// </remarks>
    /// <author>Remi Forax</author>
    public sealed class ModuleHashesAttribute : Attribute
    {
        /// <summary>The name of the hashing algorithm.</summary>
        public string algorithm;

        /// <summary>
        ///     The hash of the modules in
        ///     <see cref="modules" />
        ///     . The two lists must have the same size.
        /// </summary>
        public IList<byte[]> hashes;

        /// <summary>A list of module names.</summary>
        public IList<string> modules;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ModuleHashesAttribute" />
        ///     .
        /// </summary>
        /// <param name="algorithm">the name of the hashing algorithm.</param>
        /// <param name="modules">a list of module names.</param>
        /// <param name="hashes">
        ///     the hash of the modules in 'modules'. The two lists must have the same size.
        /// </param>
        public ModuleHashesAttribute(string algorithm, IList<string> modules, IList<byte[]
        > hashes)
            : base("ModuleHashes")
        {
            this.algorithm = algorithm;
            this.modules = modules;
            this.hashes = hashes;
        }

        /// <summary>
        ///     Constructs an empty
        ///     <see cref="ModuleHashesAttribute" />
        ///     . This object can be passed as a prototype to
        ///     the
        ///     <see
        ///         cref="Org.Objectweb.Asm.ClassReader.Accept(Org.Objectweb.Asm.ClassVisitor, Org.Objectweb.Asm.Attribute[], int)
        /// 	" />
        ///     method.
        /// </summary>
        public ModuleHashesAttribute()
            : this(null, null, null)
        {
        }

        protected internal override Attribute Read(ClassReader classReader, int offset, int
            length, char[] charBuffer, int codeAttributeOffset, Label[] labels)
        {
            var currentOffset = offset;
            var hashAlgorithm = classReader.ReadUTF8(currentOffset, charBuffer);
            currentOffset += 2;
            var numModules = classReader.ReadUnsignedShort(currentOffset);
            currentOffset += 2;
            var moduleList = new List<string>(numModules);
            var hashList = new List<byte[]>(numModules);
            for (var i = 0; i < numModules; ++i)
            {
                var module = classReader.ReadModule(currentOffset, charBuffer);
                currentOffset += 2;
                moduleList.Add(module);
                var hashLength = classReader.ReadUnsignedShort(currentOffset);
                currentOffset += 2;
                var hash = new byte[hashLength];
                for (var j = 0; j < hashLength; ++j)
                {
                    hash[j] = unchecked((byte) (classReader.ReadByte(currentOffset) & 0xFF));
                    currentOffset += 1;
                }

                hashList.Add(hash);
            }

            return new ModuleHashesAttribute(hashAlgorithm, moduleList, hashList);
        }

        protected internal override ByteVector Write(ClassWriter classWriter, byte[] code
            , int codeLength, int maxStack, int maxLocals)
        {
            var byteVector = new ByteVector();
            byteVector.PutShort(classWriter.NewUTF8(algorithm));
            if (modules == null)
            {
                byteVector.PutShort(0);
            }
            else
            {
                var numModules = modules.Count;
                byteVector.PutShort(numModules);
                for (var i = 0; i < numModules; ++i)
                {
                    var module = modules[i];
                    var hash = hashes[i];
                    byteVector.PutShort(classWriter.NewModule(module)).PutShort(hash.Length).PutByteArray
                        (hash, 0, hash.Length);
                }
            }

            return byteVector;
        }
    }
}