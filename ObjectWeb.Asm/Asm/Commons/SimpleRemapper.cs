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
    /// <summary>
    ///     A
    ///     <see cref="Remapper" />
    ///     using a
    ///     <see cref="System.Collections.IDictionary{K, V}" />
    ///     to define its mapping.
    /// </summary>
    /// <author>Eugene Kuleshov</author>
    public class SimpleRemapper : Remapper
    {
        private readonly IDictionary<string, string> mapping;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SimpleRemapper" />
        ///     with the given mapping.
        /// </summary>
        /// <param name="mapping">
        ///     a map specifying a remapping as follows:
        ///     <ul>
        ///         <li>
        ///             for method names, the key is the owner, name and descriptor of the method (in the
        ///             form &lt;owner&gt;.&lt;name&gt;&lt;descriptor&gt;), and the value is the new method
        ///             name.
        ///             <li>
        ///                 for invokedynamic method names, the key is the name and descriptor of the method (in
        ///                 the form .&lt;name&gt;&lt;descriptor&gt;), and the value is the new method name.
        ///                 <li>
        ///                     for field names, the key is the owner and name of the field (in the form
        ///                     &lt;owner&gt;.&lt;name&gt;), and the value is the new field name.
        ///                     <li>
        ///                         for internal names, the key is the old internal name, and the value is the new
        ///                         internal name.
        ///     </ul>
        /// </param>
        public SimpleRemapper(IDictionary<string, string> mapping)
        {
            this.mapping = mapping;
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="SimpleRemapper" />
        ///     with the given mapping.
        /// </summary>
        /// <param name="oldName">
        ///     the key corresponding to a method, field or internal name (see
        ///     <see cref="SimpleRemapper(System.Collections.Generic.IDictionary{K, V})" />
        ///     for the format of these keys).
        /// </param>
        /// <param name="newName">the new method, field or internal name.</param>
        public SimpleRemapper(string oldName, string newName)
        {
            mapping = new Dictionary<string, string>
                {{oldName, newName}};
        }

        public override string MapMethodName(string owner, string name, string descriptor
        )
        {
            var remappedName = Map(owner + '.' + name + descriptor);
            return remappedName == null ? name : remappedName;
        }

        public override string MapInvokeDynamicMethodName(string name, string descriptor)
        {
            var remappedName = Map('.' + name + descriptor);
            return remappedName == null ? name : remappedName;
        }

        public override string MapFieldName(string owner, string name, string descriptor)
        {
            var remappedName = Map(owner + '.' + name);
            return remappedName == null ? name : remappedName;
        }

        public override string Map(string key)
        {
            return mapping.GetOrNull(key);
        }
    }
}