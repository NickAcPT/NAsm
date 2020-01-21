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

namespace ObjectWeb.Asm
{
    /// <summary>A reference to a field or a method.</summary>
    /// <author>Remi Forax</author>
    /// <author>Eric Bruneton</author>
    public sealed class Handle
    {
        /// <summary>The descriptor of the field or method designated by this handle.</summary>
        private readonly string descriptor;

        /// <summary>Whether the owner is an interface or not.</summary>
        private readonly bool isInterface__;

        /// <summary>The name of the field or method designated by this handle.</summary>
        private readonly string name;

        /// <summary>
        ///     The internal name of the class that owns the field or method designated by this handle.
        /// </summary>
        private readonly string owner;

        /// <summary>The kind of field or method designated by this Handle.</summary>
        /// <remarks>
        ///     The kind of field or method designated by this Handle. Should be
        ///     <see cref="Opcodes.H_Getfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Getstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Putfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Putstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokevirtual" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokestatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokespecial" />
        ///     ,
        ///     <see cref="Opcodes.H_Newinvokespecial" />
        ///     or
        ///     <see cref="Opcodes.H_Invokeinterface" />
        ///     .
        /// </remarks>
        private readonly int tag;

        /// <summary>Constructs a new field or method handle.</summary>
        /// <param name="tag">
        ///     the kind of field or method designated by this Handle. Must be
        ///     <see cref="Opcodes.H_Getfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Getstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Putfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Putstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokevirtual" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokestatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokespecial" />
        ///     ,
        ///     <see cref="Opcodes.H_Newinvokespecial" />
        ///     or
        ///     <see cref="Opcodes.H_Invokeinterface" />
        ///     .
        /// </param>
        /// <param name="owner">
        ///     the internal name of the class that owns the field or method designated by this
        ///     handle.
        /// </param>
        /// <param name="name">the name of the field or method designated by this handle.</param>
        /// <param name="descriptor">
        ///     the descriptor of the field or method designated by this handle.
        /// </param>
        [Obsolete(@"this constructor has been superseded by Handle(int, string, string, string, bool) ."
        )]
        public Handle(int tag, string owner, string name, string descriptor)
            : this(tag, owner, name, descriptor, tag == OpcodesConstants.H_Invokeinterface)
        {
        }

        /// <summary>Constructs a new field or method handle.</summary>
        /// <param name="tag">
        ///     the kind of field or method designated by this Handle. Must be
        ///     <see cref="Opcodes.H_Getfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Getstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Putfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Putstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokevirtual" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokestatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokespecial" />
        ///     ,
        ///     <see cref="Opcodes.H_Newinvokespecial" />
        ///     or
        ///     <see cref="Opcodes.H_Invokeinterface" />
        ///     .
        /// </param>
        /// <param name="owner">
        ///     the internal name of the class that owns the field or method designated by this
        ///     handle.
        /// </param>
        /// <param name="name">the name of the field or method designated by this handle.</param>
        /// <param name="descriptor">
        ///     the descriptor of the field or method designated by this handle.
        /// </param>
        /// <param name="isInterface">whether the owner is an interface or not.</param>
        public Handle(int tag, string owner, string name, string descriptor, bool isInterface
        )
        {
            this.tag = tag;
            this.owner = owner;
            this.name = name;
            this.descriptor = descriptor;
            isInterface__ = isInterface;
        }

        /// <summary>Returns the kind of field or method designated by this handle.</summary>
        /// <returns>
        ///     <see cref="Opcodes.H_Getfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Getstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Putfield" />
        ///     ,
        ///     <see cref="Opcodes.H_Putstatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokevirtual" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokestatic" />
        ///     ,
        ///     <see cref="Opcodes.H_Invokespecial" />
        ///     ,
        ///     <see cref="Opcodes.H_Newinvokespecial" />
        ///     or
        ///     <see cref="Opcodes.H_Invokeinterface" />
        ///     .
        /// </returns>
        public int GetTag()
        {
            return tag;
        }

        /// <summary>
        ///     Returns the internal name of the class that owns the field or method designated by this handle.
        /// </summary>
        /// <returns>
        ///     the internal name of the class that owns the field or method designated by this handle.
        /// </returns>
        public string GetOwner()
        {
            return owner;
        }

        /// <summary>Returns the name of the field or method designated by this handle.</summary>
        /// <returns>the name of the field or method designated by this handle.</returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        ///     Returns the descriptor of the field or method designated by this handle.
        /// </summary>
        /// <returns>the descriptor of the field or method designated by this handle.</returns>
        public string GetDesc()
        {
            return descriptor;
        }

        /// <summary>
        ///     Returns true if the owner of the field or method designated by this handle is an interface.
        /// </summary>
        /// <returns>
        ///     true if the owner of the field or method designated by this handle is an interface.
        /// </returns>
        public bool IsInterface()
        {
            return isInterface__;
        }

        public override bool Equals(object @object)
        {
            if (@object == this) return true;
            if (!(@object is Handle)) return false;
            var handle = (Handle) @object;
            return tag == handle.tag && isInterface__ == handle.IsInterface() && owner.Equals(handle
                       .owner) && name.Equals(handle.name) && descriptor.Equals(handle.descriptor);
        }

        public override int GetHashCode()
        {
            return tag + (isInterface__ ? 64 : 0) + owner.GetHashCode() * name.GetHashCode()
                                                                        * descriptor.GetHashCode();
        }

        /// <summary>Returns the textual representation of this handle.</summary>
        /// <remarks>
        ///     Returns the textual representation of this handle. The textual representation is:
        ///     <ul>
        ///         <li>
        ///             for a reference to a class: owner "." name descriptor " (" tag ")",
        ///             <li>for a reference to an interface: owner "." name descriptor " (" tag " itf)".
        ///     </ul>
        /// </remarks>
        public override string ToString()
        {
            return owner + '.' + name + descriptor + " (" + tag + (isInterface__ ? " itf" : string.Empty
                   ) + ')';
        }
    }
}