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
using ObjectWeb.Asm.Signature;

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="SignatureVisitor" />
    ///     that checks that its methods are properly used.
    /// </summary>
    /// <author>Eric Bruneton</author>
    public class CheckSignatureAdapter : SignatureVisitor
    {
        /// <summary>Type to be used to check class signatures.</summary>
        /// <remarks>
        ///     Type to be used to check class signatures. See
        ///     <see cref="CheckSignatureAdapter(int, Org.Objectweb.Asm.Signature.SignatureVisitor)
        /// 	" />
        ///     .
        /// </remarks>
        public const int Class_Signature = 0;

        /// <summary>Type to be used to check method signatures.</summary>
        /// <remarks>
        ///     Type to be used to check method signatures. See
        ///     <see cref="CheckSignatureAdapter(int, Org.Objectweb.Asm.Signature.SignatureVisitor)
        /// 	" />
        ///     .
        /// </remarks>
        public const int Method_Signature = 1;

        /// <summary>
        ///     Type to be used to check type signatures.See
        ///     <see cref="CheckSignatureAdapter(int, Org.Objectweb.Asm.Signature.SignatureVisitor)
        /// 	" />
        ///     .
        /// </summary>
        public const int Type_Signature = 2;

        private const string Invalid = "Invalid ";

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitFormalTypeParameter(string)" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Formal_Type_Parameter_States
            = new HashSet<State>(new[]
            {
                State.Empty, State
                    .Formal,
                State.Bound
            });

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitClassBound()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Class_Bound_States
            = new HashSet<State>(new[] {State.Formal});

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitInterfaceBound()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Interface_Bound_States
            = new HashSet<State>(new[]
            {
                State.Formal, State
                    .Bound
            });

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitSuperclass()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Super_Class_States
            = new HashSet<State>(new[]
            {
                State.Empty, State
                    .Formal,
                State.Bound
            });

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitInterface()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Interface_States
            = new HashSet<State>(new[] {State.Super});

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitParameterType()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Parameter_Type_States
            = new HashSet<State>(new[]
            {
                State.Empty, State
                    .Formal,
                State.Bound, State.Param
            });

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitReturnType()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Return_Type_States
            = new HashSet<State>(new[]
            {
                State.Empty, State
                    .Formal,
                State.Bound, State.Param
            });

        /// <summary>
        ///     The valid automaton states for a
        ///     <see cref="VisitExceptionType()" />
        ///     method call.
        /// </summary>
        private static readonly HashSet<State> Visit_Exception_Type_States
            = new HashSet<State>(new[] {State.Return});

        /// <summary>The visitor to which this adapter must delegate calls.</summary>
        /// <remarks>
        ///     The visitor to which this adapter must delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        private readonly SignatureVisitor signatureVisitor;

        /// <summary>The type of the visited signature.</summary>
        private readonly int type;

        /// <summary>Whether the visited signature can be 'V'.</summary>
        private bool canBeVoid;

        /// <summary>
        ///     The current state of the automaton used to check the order of method calls.
        /// </summary>
        private State state;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckSignatureAdapter" />
        ///     .
        ///     <i>
        ///         Subclasses must not use this
        ///         constructor
        ///     </i>
        ///     . Instead, they must use the
        ///     <see cref="CheckSignatureAdapter(int, int, Org.Objectweb.Asm.Signature.SignatureVisitor)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="type">
        ///     the type of signature to be checked. See
        ///     <see cref="Class_Signature" />
        ///     ,
        ///     <see cref="Method_Signature" />
        ///     and
        ///     <see cref="Type_Signature" />
        ///     .
        /// </param>
        /// <param name="signatureVisitor">
        ///     the visitor to which this adapter must delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public CheckSignatureAdapter(int type, SignatureVisitor signatureVisitor)
            : this(VisitorAsmApiVersion.Asm7, type, signatureVisitor)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="CheckSignatureAdapter" />
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
        /// <param name="type">
        ///     the type of signature to be checked. See
        ///     <see cref="Class_Signature" />
        ///     ,
        ///     <see cref="Method_Signature" />
        ///     and
        ///     <see cref="Type_Signature" />
        ///     .
        /// </param>
        /// <param name="signatureVisitor">
        ///     the visitor to which this adapter must delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        protected internal CheckSignatureAdapter(VisitorAsmApiVersion api, int type, SignatureVisitor signatureVisitor
        )
            : base(api)
        {
            /* latest api = */
            this.type = type;
            state = State.Empty;
            this.signatureVisitor = signatureVisitor;
        }

        // class and method signatures
        public override void VisitFormalTypeParameter(string name)
        {
            if (type == Type_Signature || !Visit_Formal_Type_Parameter_States.Contains(state))
                throw new InvalidOperationException();
            CheckIdentifier(name, "formal type parameter");
            state = State.Formal;
            if (signatureVisitor != null) signatureVisitor.VisitFormalTypeParameter(name);
        }

        public override SignatureVisitor VisitClassBound()
        {
            if (type == Type_Signature || !Visit_Class_Bound_States.Contains(state))
                throw new InvalidOperationException();
            state = State.Bound;
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitClassBound());
        }

        public override SignatureVisitor VisitInterfaceBound()
        {
            if (type == Type_Signature || !Visit_Interface_Bound_States.Contains(state))
                throw new InvalidOperationException();
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitInterfaceBound());
        }

        // class signatures
        public override SignatureVisitor VisitSuperclass()
        {
            if (type != Class_Signature || !Visit_Super_Class_States.Contains(state))
                throw new InvalidOperationException();
            state = State.Super;
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitSuperclass());
        }

        public override SignatureVisitor VisitInterface()
        {
            if (type != Class_Signature || !Visit_Interface_States.Contains(state))
                throw new InvalidOperationException();
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitInterface());
        }

        // method signatures
        public override SignatureVisitor VisitParameterType()
        {
            if (type != Method_Signature || !Visit_Parameter_Type_States.Contains(state))
                throw new InvalidOperationException();
            state = State.Param;
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitParameterType());
        }

        public override SignatureVisitor VisitReturnType()
        {
            if (type != Method_Signature || !Visit_Return_Type_States.Contains(state))
                throw new InvalidOperationException();
            state = State.Return;
            var checkSignatureAdapter = new CheckSignatureAdapter(Type_Signature
                , signatureVisitor == null ? null : signatureVisitor.VisitReturnType());
            checkSignatureAdapter.canBeVoid = true;
            return checkSignatureAdapter;
        }

        public override SignatureVisitor VisitExceptionType()
        {
            if (type != Method_Signature || !Visit_Exception_Type_States.Contains(state))
                throw new InvalidOperationException();
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitExceptionType());
        }

        // type signatures
        public override void VisitBaseType(char descriptor)
        {
            if (type != Type_Signature || state != State.Empty) throw new InvalidOperationException();
            if (descriptor == 'V')
            {
                if (!canBeVoid) throw new ArgumentException("Base type descriptor can't be V");
            }
            else if ("ZCBSIFJD".IndexOf(descriptor) == -1)
            {
                throw new ArgumentException("Base type descriptor must be one of ZCBSIFJD");
            }

            state = State.Simple_Type;
            if (signatureVisitor != null) signatureVisitor.VisitBaseType(descriptor);
        }

        public override void VisitTypeVariable(string name)
        {
            if (type != Type_Signature || state != State.Empty) throw new InvalidOperationException();
            CheckIdentifier(name, "type variable");
            state = State.Simple_Type;
            if (signatureVisitor != null) signatureVisitor.VisitTypeVariable(name);
        }

        public override SignatureVisitor VisitArrayType()
        {
            if (type != Type_Signature || state != State.Empty) throw new InvalidOperationException();
            state = State.Simple_Type;
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitArrayType());
        }

        public override void VisitClassType(string name)
        {
            if (type != Type_Signature || state != State.Empty) throw new InvalidOperationException();
            CheckClassName(name, "class name");
            state = State.Class_Type;
            if (signatureVisitor != null) signatureVisitor.VisitClassType(name);
        }

        public override void VisitInnerClassType(string name)
        {
            if (state != State.Class_Type) throw new InvalidOperationException();
            CheckIdentifier(name, "inner class name");
            if (signatureVisitor != null) signatureVisitor.VisitInnerClassType(name);
        }

        public override void VisitTypeArgument()
        {
            if (state != State.Class_Type) throw new InvalidOperationException();
            if (signatureVisitor != null) signatureVisitor.VisitTypeArgument();
        }

        public override SignatureVisitor VisitTypeArgument(char wildcard)
        {
            if (state != State.Class_Type) throw new InvalidOperationException();
            if ("+-=".IndexOf(wildcard) == -1) throw new ArgumentException("Wildcard must be one of +-=");
            return new CheckSignatureAdapter(Type_Signature,
                signatureVisitor == null ? null : signatureVisitor.VisitTypeArgument(wildcard));
        }

        public override void VisitEnd()
        {
            if (state != State.Class_Type) throw new InvalidOperationException();
            state = State.End;
            if (signatureVisitor != null) signatureVisitor.VisitEnd();
        }

        private void CheckClassName(string name, string message)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentException(Invalid + message + " (must not be null or empty)");
            for (var i = 0; i < name.Length; ++i)
                if (".;[<>:".IndexOf(name[i]) != -1)
                    throw new ArgumentException(Invalid + message + " (must not contain . ; [ < > or :): "
                                                + name);
        }

        private void CheckIdentifier(string name, string message)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentException(Invalid + message + " (must not be null or empty)");
            for (var i = 0; i < name.Length; ++i)
                if (".;[/<>:".IndexOf(name[i]) != -1)
                    throw new ArgumentException(Invalid + message + " (must not contain . ; [ / < > or :): "
                                                + name);
        }

        /// <summary>
        ///     The possible states of the automaton used to check the order of method calls.
        /// </summary>
        [Serializable]
        private sealed class State : EnumBase
        {
            public static readonly State Empty = new State
                (0, "EMPTY");

            public static readonly State Formal = new State
                (1, "FORMAL");

            public static readonly State Bound = new State
                (2, "BOUND");

            public static readonly State Super = new State
                (3, "SUPER");

            public static readonly State Param = new State
                (4, "PARAM");

            public static readonly State Return = new State
                (5, "RETURN");

            public static readonly State Simple_Type = new State
                (6, "SIMPLE_TYPE");

            public static readonly State Class_Type = new State
                (7, "CLASS_TYPE");

            public static readonly State End = new State
                (8, "END");

            static State()
            {
                RegisterValues<State>(Values());
            }

            private State(int ordinal, string name)
                : base(ordinal, name)
            {
            }

            public static State[] Values()
            {
                return new[] {Empty, Formal, Bound, Super, Param, Return, Simple_Type, Class_Type, End};
            }
        }
    }
}