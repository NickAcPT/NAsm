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

namespace ObjectWeb.Asm.Tree
{
	/// <summary>A node that represents an inner class.</summary>
	/// <author>Eric Bruneton</author>
	public class InnerClassNode
    {
	    /// <summary>
	    ///     The access flags of the inner class as originally declared in the enclosing class.
	    /// </summary>
	    public AccessFlags access;

	    /// <summary>The (simple) name of the inner class inside its enclosing class.</summary>
	    /// <remarks>
	    ///     The (simple) name of the inner class inside its enclosing class. May be
	    ///     <literal>null</literal>
	    ///     for
	    ///     anonymous inner classes.
	    /// </remarks>
	    public string innerName;

	    /// <summary>
	    ///     The internal name of an inner class (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ).
	    /// </summary>
	    public string name;

	    /// <summary>
	    ///     The internal name of the class to which the inner class belongs (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ). May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </summary>
	    public string outerName;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="InnerClassNode" />
	    ///     .
	    /// </summary>
	    /// <param name="name">
	    ///     the internal name of an inner class (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ).
	    /// </param>
	    /// <param name="outerName">
	    ///     the internal name of the class to which the inner class belongs (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ). May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    /// <param name="innerName">
	    ///     the (simple) name of the inner class inside its enclosing class. May be
	    ///     <literal>null</literal>
	    ///     for anonymous inner classes.
	    /// </param>
	    /// <param name="access">
	    ///     the access flags of the inner class as originally declared in the enclosing
	    ///     class.
	    /// </param>
	    public InnerClassNode(string name, string outerName, string innerName, AccessFlags access
        )
        {
            this.name = name;
            this.outerName = outerName;
            this.innerName = innerName;
            this.access = access;
        }

	    /// <summary>Makes the given class visitor visit this inner class.</summary>
	    /// <param name="classVisitor">a class visitor.</param>
	    public virtual void Accept(ClassVisitor classVisitor)
        {
            classVisitor.VisitInnerClass(name, outerName, innerName, access);
        }
    }
}