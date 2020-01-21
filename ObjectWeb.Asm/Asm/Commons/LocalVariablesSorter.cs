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
using ObjectWeb.Asm.Enums;
using ObjectWeb.Misc.Java.Lang;

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     that renumbers local variables in their order of appearance. This adapter
    ///     allows one to easily add new local variables to a method. It may be used by inheriting from this
    ///     class, but the preferred way of using it is via delegation: the next visitor in the chain can
    ///     indeed add new locals when needed by calling
    ///     <see cref="NewLocal" />
    ///     on this adapter (this requires a
    ///     reference back to this
    ///     <see cref="LocalVariablesSorter" />
    ///     ).
    /// </summary>
    /// <author>Chris Nokleberg</author>
    /// <author>Eugene Kuleshov</author>
    /// <author>Eric Bruneton</author>
    public class LocalVariablesSorter : MethodVisitor
    {
        /// <summary>The type of the java.lang.Object class.</summary>
        private static readonly Type Object_Type = Type.GetObjectType("java/lang/Object");

        /// <summary>The index of the first local variable, after formal parameters.</summary>
        protected internal readonly int firstLocal;

        /// <summary>
        ///     The index of the next local variable to be created by
        ///     <see cref="NewLocal" />
        ///     .
        /// </summary>
        protected internal int nextLocal;

        /// <summary>The local variable types after remapping.</summary>
        /// <remarks>
        ///     The local variable types after remapping. The format of this array is the same as in
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitFrame(int, int, object[], int, object[])
        /// 	" />
        ///     , except that long and double types use two slots.
        /// </remarks>
        private object[] remappedLocalTypes = new object[20];

        /// <summary>The mapping from old to new local variable indices.</summary>
        /// <remarks>
        ///     The mapping from old to new local variable indices. A local variable at index i of size 1 is
        ///     remapped to 'mapping[2*i]', while a local variable at index i of size 2 is remapped to
        ///     'mapping[2*i+1]'.
        /// </remarks>
        private int[] remappedVariableIndices = new int[40];

        /// <summary>
        ///     Constructs a new
        ///     <see cref="LocalVariablesSorter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="LocalVariablesSorter(int, int, string, Org.Objectweb.Asm.MethodVisitor)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="access">access flags of the adapted method.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        ///     if a subclass calls this constructor.
        /// </exception>
        public LocalVariablesSorter(int access, string descriptor, MethodVisitor methodVisitor
        )
            : this(ObjectWeb.Asm.Enums.VisitorAsmApiVersion.Asm7, access, descriptor, methodVisitor)
        {
            /* latest api = */
            if (GetType() != typeof(LocalVariablesSorter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="LocalVariablesSorter" />
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
        /// <param name="access">access flags of the adapted method.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        protected internal LocalVariablesSorter(VisitorAsmApiVersion api, int access, string descriptor, MethodVisitor
            methodVisitor)
            : base(api, methodVisitor)
        {
            nextLocal = (OpcodesConstants.Acc_Static & access) == 0 ? 1 : 0;
            foreach (var argumentType in Type.GetArgumentTypes(descriptor)) nextLocal += argumentType.GetSize();
            firstLocal = nextLocal;
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            Type varType;
            switch (opcode)
            {
                case OpcodesConstants.Lload:
                case OpcodesConstants.Lstore:
                {
                    varType = Type.Long_Type;
                    break;
                }

                case OpcodesConstants.Dload:
                case OpcodesConstants.Dstore:
                {
                    varType = Type.Double_Type;
                    break;
                }

                case OpcodesConstants.Fload:
                case OpcodesConstants.Fstore:
                {
                    varType = Type.Float_Type;
                    break;
                }

                case OpcodesConstants.Iload:
                case OpcodesConstants.Istore:
                {
                    varType = Type.Int_Type;
                    break;
                }

                case OpcodesConstants.Aload:
                case OpcodesConstants.Astore:
                case OpcodesConstants.Ret:
                {
                    varType = Object_Type;
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid opcode " + opcode);
                }
            }

            base.VisitVarInsn(opcode, Remap(var, varType));
        }

        public override void VisitIincInsn(int var, int increment)
        {
            base.VisitIincInsn(Remap(var, Type.Int_Type), increment);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            base.VisitMaxs(maxStack, nextLocal);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            var remappedIndex = Remap(index, Type.GetType(descriptor));
            base.VisitLocalVariable(name, descriptor, signature, start, end, remappedIndex);
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath
                typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible
        )
        {
            var type = Type.GetType(descriptor);
            var remappedIndex = new int[index.Length];
            for (var i = 0; i < remappedIndex.Length; ++i) remappedIndex[i] = Remap(index[i], type);
            return base.VisitLocalVariableAnnotation(typeRef, typePath, start, end, remappedIndex
                , descriptor, visible);
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            if (type != OpcodesConstants.F_New)
                // Uncompressed frame.
                throw new ArgumentException(
                    "LocalVariablesSorter only accepts expanded frames (see ClassReader.EXPAND_FRAMES)"
                );
            // Create a copy of remappedLocals.
            var oldRemappedLocals = new object[remappedLocalTypes.Length];
            Array.Copy(remappedLocalTypes, 0, oldRemappedLocals, 0, oldRemappedLocals.Length);
            UpdateNewLocals(remappedLocalTypes);
            // Copy the types from 'local' to 'remappedLocals'. 'remappedLocals' already contains the
            // variables added with 'newLocal'.
            var oldVar = 0;
            // Old local variable index.
            for (var i = 0; i < numLocal; ++i)
            {
                var localType = local[i];
                if (localType != (object) OpcodesConstants.Top)
                {
                    var varType = Object_Type;
                    if (localType == (object) OpcodesConstants.Integer)
                        varType = Type.Int_Type;
                    else if (localType == (object) OpcodesConstants.Float)
                        varType = Type.Float_Type;
                    else if (localType == (object) OpcodesConstants.Long)
                        varType = Type.Long_Type;
                    else if (localType == (object) OpcodesConstants.Double)
                        varType = Type.Double_Type;
                    else if (localType is string) varType = Type.GetObjectType((string) localType);
                    SetFrameLocal(Remap(oldVar, varType), localType);
                }

                oldVar += localType == (object) OpcodesConstants.Long || localType == (object) OpcodesConstants.Double
                    ? 2
                    : 1;
            }

            // Remove TOP after long and double types as well as trailing TOPs.
            oldVar = 0;
            var newVar = 0;
            var remappedNumLocal = 0;
            while (oldVar < remappedLocalTypes.Length)
            {
                var localType = remappedLocalTypes[oldVar];
                oldVar += localType == (object) OpcodesConstants.Long || localType == (object) OpcodesConstants.Double
                    ? 2
                    : 1;
                if (localType != null && localType != (object) OpcodesConstants.Top)
                {
                    remappedLocalTypes[newVar++] = localType;
                    remappedNumLocal = newVar;
                }
                else
                {
                    remappedLocalTypes[newVar++] = OpcodesConstants.Top;
                }
            }

            // Visit the remapped frame.
            base.VisitFrame(type, remappedNumLocal, remappedLocalTypes, numStack, stack);
            // Restore the original value of 'remappedLocals'.
            remappedLocalTypes = oldRemappedLocals;
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>Constructs a new local variable of the given type.</summary>
        /// <param name="type">the type of the local variable to be created.</param>
        /// <returns>the identifier of the newly created local variable.</returns>
        public virtual int NewLocal(Type type)
        {
            object localType;
            switch (type.GetSort())
            {
                case Type.Boolean:
                case Type.Char:
                case Type.Byte:
                case Type.Short:
                case Type.Int:
                {
                    localType = OpcodesConstants.Integer;
                    break;
                }

                case Type.Float:
                {
                    localType = OpcodesConstants.Float;
                    break;
                }

                case Type.Long:
                {
                    localType = OpcodesConstants.Long;
                    break;
                }

                case Type.Double:
                {
                    localType = OpcodesConstants.Double;
                    break;
                }

                case Type.Array:
                {
                    localType = type.GetDescriptor();
                    break;
                }

                case Type.Object:
                {
                    localType = type.GetInternalName();
                    break;
                }

                default:
                {
                    throw new AssertionError();
                }
            }

            var local = NewLocalMapping(type);
            SetLocalType(local, type);
            SetFrameLocal(local, localType);
            return local;
        }

        /// <summary>Notifies subclasses that a new stack map frame is being visited.</summary>
        /// <remarks>
        ///     Notifies subclasses that a new stack map frame is being visited. The array argument contains
        ///     the stack map frame types corresponding to the local variables added with
        ///     <see cref="NewLocal(Org.Objectweb.Asm.Type)" />
        ///     .
        ///     This method can update these types in place for the stack map frame being visited. The default
        ///     implementation of this method does nothing, i.e. a local variable added with
        ///     <see cref="NewLocal(Org.Objectweb.Asm.Type)" />
        ///     will have the same type in all stack map frames. But this behavior is not always the desired
        ///     one, for instance if a local variable is added in the middle of a try/catch block: the frame
        ///     for the exception handler should have a TOP type for this new local.
        /// </remarks>
        /// <param name="newLocals">
        ///     the stack map frame types corresponding to the local variables added with
        ///     <see cref="NewLocal(Org.Objectweb.Asm.Type)" />
        ///     (and null for the others). The format of this array is the same as in
        ///     <see cref="Org.Objectweb.Asm.MethodVisitor.VisitFrame(int, int, object[], int, object[])
        /// 	" />
        ///     , except that long and double types use two slots. The
        ///     types for the current stack map frame must be updated in place in this array.
        /// </param>
        protected internal virtual void UpdateNewLocals(object[] newLocals)
        {
        }

        // The default implementation does nothing.
        /// <summary>Notifies subclasses that a local variable has been added or remapped.</summary>
        /// <remarks>
        ///     Notifies subclasses that a local variable has been added or remapped. The default
        ///     implementation of this method does nothing.
        /// </remarks>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="NewLocal" />
        ///     .
        /// </param>
        /// <param name="type">the type of the value being stored in the local variable.</param>
        protected internal virtual void SetLocalType(int local, Type type)
        {
        }

        // The default implementation does nothing.
        private void SetFrameLocal(int local, object type)
        {
            var numLocals = remappedLocalTypes.Length;
            if (local >= numLocals)
            {
                var newRemappedLocalTypes = new object[Math.Max(2 * numLocals, local
                                                                               + 1)];
                Array.Copy(remappedLocalTypes, 0, newRemappedLocalTypes, 0, numLocals);
                remappedLocalTypes = newRemappedLocalTypes;
            }

            remappedLocalTypes[local] = type;
        }

        private int Remap(int var, Type type)
        {
            if (var + type.GetSize() <= firstLocal) return var;
            var key = 2 * var + type.GetSize() - 1;
            var size = remappedVariableIndices.Length;
            if (key >= size)
            {
                var newRemappedVariableIndices = new int[Math.Max(2 * size, key + 1)];
                Array.Copy(remappedVariableIndices, 0, newRemappedVariableIndices, 0, size
                );
                remappedVariableIndices = newRemappedVariableIndices;
            }

            var value = remappedVariableIndices[key];
            if (value == 0)
            {
                value = NewLocalMapping(type);
                SetLocalType(value, type);
                remappedVariableIndices[key] = value + 1;
            }
            else
            {
                value--;
            }

            return value;
        }

        protected internal virtual int NewLocalMapping(Type type)
        {
            var local = nextLocal;
            nextLocal += type.GetSize();
            return local;
        }
    }
}