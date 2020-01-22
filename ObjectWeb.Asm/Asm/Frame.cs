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
using System.Text;
using ObjectWeb.Asm.Enums;
using ObjectWeb.Misc.Java.Lang;

namespace ObjectWeb.Asm
{
    /// <summary>The input and output stack map frames of a basic block.</summary>
    /// <remarks>
    ///     The input and output stack map frames of a basic block.
    ///     <p>
    ///         Stack map frames are computed in two steps:
    ///         <ul>
    ///             <li>
    ///                 During the visit of each instruction in MethodWriter, the state of the frame at the end of
    ///                 the current basic block is updated by simulating the action of the instruction on the
    ///                 previous state of this so called "output frame".
    ///                 <li>
    ///                     After all instructions have been visited, a fix point algorithm is used in MethodWriter to
    ///                     compute the "input frame" of each basic block (i.e. the stack map frame at the beginning of
    ///                     the basic block). See
    ///                     <see cref="MethodWriter.ComputeAllFrames()" />
    ///                     .
    ///         </ul>
    ///         <p>
    ///             Output stack map frames are computed relatively to the input frame of the basic block, which
    ///             is not yet known when output frames are computed. It is therefore necessary to be able to
    ///             represent abstract types such as "the type at position x in the input frame locals" or "the type
    ///             at position x from the top of the input frame stack" or even "the type at position x in the input
    ///             frame, with y more (or less) array dimensions". This explains the rather complicated type format
    ///             used in this class, explained below.
    ///             <p>
    ///                 The local variables and the operand stack of input and output frames contain values called
    ///                 "abstract types" hereafter. An abstract type is represented with 4 fields named DIM, KIND, FLAGS
    ///                 and VALUE, packed in a single int value for better performance and memory efficiency:
    ///                 <pre>
    ///                     =====================================
    ///                     |...DIM|KIND|.F|...............VALUE|
    ///                     =====================================
    ///                 </pre>
    ///                 <ul>
    ///                     <li>
    ///                         the DIM field, stored in the 6 most significant bits, is a signed number of array
    ///                         dimensions (from -32 to 31, included). It can be retrieved with
    ///                         <see cref="Dim_Mask" />
    ///                         and a
    ///                         right shift of
    ///                         <see cref="Dim_Shift" />
    ///                         .
    ///                         <li>
    ///                             the KIND field, stored in 4 bits, indicates the kind of VALUE used. These 4 bits can be
    ///                             retrieved with
    ///                             <see cref="Kind_Mask" />
    ///                             and, without any shift, must be equal to
    ///                             <see cref="Constant_Kind" />
    ///                             ,
    ///                             <see cref="Reference_Kind" />
    ///                             ,
    ///                             <see cref="Uninitialized_Kind" />
    ///                             ,
    ///                             <see cref="Local_Kind" />
    ///                             or
    ///                             <see cref="Stack_Kind" />
    ///                             .
    ///                             <li>
    ///                                 the FLAGS field, stored in 2 bits, contains up to 2 boolean flags. Currently only one
    ///                                 flag
    ///                                 is defined, namely
    ///                                 <see cref="Top_If_Long_Or_Double_Flag" />
    ///                                 .
    ///                                 <li>
    ///                                     the VALUE field, stored in the remaining 20 bits, contains either
    ///                                     <ul>
    ///                                         <li>
    ///                                             one of the constants
    ///                                             <see cref="Item_Top" />
    ///                                             ,
    ///                                             <see cref="Item_Asm_Boolean" />
    ///                                             ,
    ///                                             <see cref="Item_Asm_Byte" />
    ///                                             ,
    ///                                             <see cref="Item_Asm_Char" />
    ///                                             or
    ///                                             <see cref="Item_Asm_Short" />
    ///                                             ,
    ///                                             <see cref="Item_Integer" />
    ///                                             ,
    ///                                             <see cref="Item_Float" />
    ///                                             ,
    ///                                             <see cref="Item_Long" />
    ///                                             ,
    ///                                             <see cref="Item_Double" />
    ///                                             ,
    ///                                             <see cref="Item_Null" />
    ///                                             or
    ///                                             <see cref="Item_Uninitialized_This" />
    ///                                             , if KIND is equal to
    ///                                             <see cref="Constant_Kind" />
    ///                                             .
    ///                                             <li>
    ///                                                 the index of a
    ///                                                 <see cref="Symbol.Type_Tag" />
    ///                                                 <see cref="Symbol" />
    ///                                                 in the type table of a
    ///                                                 <see cref="SymbolTable" />
    ///                                                 , if KIND is equal to
    ///                                                 <see cref="Reference_Kind" />
    ///                                                 .
    ///                                                 <li>
    ///                                                     the index of an
    ///                                                     <see cref="Symbol.Uninitialized_Type_Tag" />
    ///                                                     <see cref="Symbol" />
    ///                                                     in the type
    ///                                                     table of a SymbolTable, if KIND is equal to
    ///                                                     <see cref="Uninitialized_Kind" />
    ///                                                     .
    ///                                                     <li>
    ///                                                         the index of a local variable in the input stack frame, if KIND
    ///                                                         is equal to
    ///                                                         <see cref="Local_Kind" />
    ///                                                         .
    ///                                                         <li>
    ///                                                             a position relatively to the top of the stack of the input
    ///                                                             stack frame, if KIND is
    ///                                                             equal to
    ///                                                             <see cref="Stack_Kind" />
    ///                                                             ,
    ///                                     </ul>
    ///                 </ul>
    ///                 <p>
    ///                     Output frames can contain abstract types of any kind and with a positive or negative array
    ///                     dimension (and even unassigned types, represented by 0 - which does not correspond to any valid
    ///                     abstract type value). Input frames can only contain CONSTANT_KIND, REFERENCE_KIND or
    ///                     UNINITIALIZED_KIND abstract types of positive or
    ///                     <literal>null</literal>
    ///                     array dimension. In all cases
    ///                     the type table contains only internal type names (array type descriptors are forbidden - array
    ///                     dimensions must be represented through the DIM field).
    ///                     <p>
    ///                         The LONG and DOUBLE types are always represented by using two slots (LONG + TOP or DOUBLE +
    ///                         TOP), for local variables as well as in the operand stack. This is necessary to be able to
    ///                         simulate DUPx_y instructions, whose effect would be dependent on the concrete types represented
    ///                         by the abstract types in the stack (which are not always known).
    /// </remarks>
    /// <author>Eric Bruneton</author>
    internal class Frame
    {
        internal const int Same_Frame = 0;

        internal const int Same_Locals_1_Stack_Item_Frame = 64;

        internal const int Reserved = 128;

        internal const int Same_Locals_1_Stack_Item_Frame_Extended = 247;

        internal const int Chop_Frame = 248;

        internal const int Same_Frame_Extended = 251;

        internal const int Append_Frame = 252;

        internal const int Full_Frame = 255;

        internal const int Item_Top = 0;

        internal const int Item_Integer = 1;

        internal const int Item_Float = 2;

        internal const int Item_Double = 3;

        internal const int Item_Long = 4;

        internal const int Item_Null = 5;

        internal const int Item_Uninitialized_This = 6;

        internal const int Item_Object = 7;

        internal const int Item_Uninitialized = 8;

        private const int Item_Asm_Boolean = 9;

        private const int Item_Asm_Byte = 10;

        private const int Item_Asm_Char = 11;

        private const int Item_Asm_Short = 12;

        private const int Dim_Size = 6;

        private const int Kind_Size = 4;

        private const int Flags_Size = 2;

        private const int Value_Size = 32 - Dim_Size - Kind_Size - Flags_Size;

        private const int Dim_Shift = Kind_Size + Flags_Size + Value_Size;

        private const int Kind_Shift = Flags_Size + Value_Size;

        private const int Flags_Shift = Value_Size;

        private const int Dim_Mask = ((1 << Dim_Size) - 1) << Dim_Shift;

        private const int Kind_Mask = ((1 << Kind_Size) - 1) << Kind_Shift;

        private const int Value_Mask = (1 << Value_Size) - 1;

        /// <summary>
        ///     The constant to be added to an abstract type to get one with one more array dimension.
        /// </summary>
        private const int Array_Of = +1 << Dim_Shift;

        /// <summary>
        ///     The constant to be added to an abstract type to get one with one less array dimension.
        /// </summary>
        private const int Element_Of = -1 << Dim_Shift;

        private const int Constant_Kind = 1 << Kind_Shift;

        private const int Reference_Kind = 2 << Kind_Shift;

        private const int Uninitialized_Kind = 3 << Kind_Shift;

        private const int Local_Kind = 4 << Kind_Shift;

        private const int Stack_Kind = 5 << Kind_Shift;

        /// <summary>
        ///     A flag used for LOCAL_KIND and STACK_KIND abstract types, indicating that if the resolved,
        ///     concrete type is LONG or DOUBLE, TOP should be used instead (because the value has been
        ///     partially overridden with an xSTORE instruction).
        /// </summary>
        private const int Top_If_Long_Or_Double_Flag = 1 << Flags_Shift;

        private const int Top = Constant_Kind | Item_Top;

        private const int Boolean = Constant_Kind | Item_Asm_Boolean;

        private const int Byte = Constant_Kind | Item_Asm_Byte;

        private const int Char = Constant_Kind | Item_Asm_Char;

        private const int Short = Constant_Kind | Item_Asm_Short;

        private const int Integer = Constant_Kind | Item_Integer;

        private const int Float = Constant_Kind | Item_Float;

        private const int Long = Constant_Kind | Item_Long;

        private const int Double = Constant_Kind | Item_Double;

        private const int Null = Constant_Kind | Item_Null;

        private const int Uninitialized_This = Constant_Kind | Item_Uninitialized_This;

        /// <summary>The number of types that are initialized in the basic block.</summary>
        /// <remarks>
        ///     The number of types that are initialized in the basic block. See
        ///     <see cref="initializations" />
        ///     .
        /// </remarks>
        private int initializationCount;

        /// <summary>The abstract types that are initialized in the basic block.</summary>
        /// <remarks>
        ///     The abstract types that are initialized in the basic block. A constructor invocation on an
        ///     UNINITIALIZED or UNINITIALIZED_THIS abstract type must replace <i>every occurrence</i> of this
        ///     type in the local variables and in the operand stack. This cannot be done during the first step
        ///     of the algorithm since, during this step, the local variables and the operand stack types are
        ///     still abstract. It is therefore necessary to store the abstract types of the constructors which
        ///     are invoked in the basic block, in order to do this replacement during the second step of the
        ///     algorithm, where the frames are fully computed. Note that this array can contain abstract types
        ///     that are relative to the input locals or to the input stack.
        /// </remarks>
        private int[] initializations;

        /// <summary>The input stack map frame locals.</summary>
        /// <remarks>The input stack map frame locals. This is an array of abstract types.</remarks>
        private int[] inputLocals;

        /// <summary>The input stack map frame stack.</summary>
        /// <remarks>The input stack map frame stack. This is an array of abstract types.</remarks>
        private int[] inputStack;

        /// <summary>The output stack map frame locals.</summary>
        /// <remarks>The output stack map frame locals. This is an array of abstract types.</remarks>
        private int[] outputLocals;

        /// <summary>The output stack map frame stack.</summary>
        /// <remarks>The output stack map frame stack. This is an array of abstract types.</remarks>
        private int[] outputStack;

        /// <summary>The start of the output stack, relatively to the input stack.</summary>
        /// <remarks>
        ///     The start of the output stack, relatively to the input stack. This offset is always negative or
        ///     null. A null offset means that the output stack must be appended to the input stack. A -n
        ///     offset means that the first n output stack elements must replace the top n input stack
        ///     elements, and that the other elements must be appended to the input stack.
        /// </remarks>
        private int outputStackStart;

        /// <summary>
        ///     The index of the top stack element in
        ///     <see cref="outputStack" />
        ///     .
        /// </summary>
        private short outputStackTop;

        /// <summary>
        ///     The basic block to which these input and output stack map frames correspond.
        /// </summary>
        internal Label owner;

        /// <summary>Constructs a new Frame.</summary>
        /// <param name="owner">
        ///     the basic block to which these input and output stack map frames correspond.
        /// </param>
        internal Frame(Label owner)
        {
            // Constants used in the StackMapTable attribute.
            // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.4.
            // Additional, ASM specific constants used in abstract types below.
            // The size and offset in bits of each field of an abstract type.
            // Bitmasks to get each field of an abstract type.
            // Constants to manipulate the DIM field of an abstract type.
            // Possible values for the KIND field of an abstract type.
            // Possible flags for the FLAGS field of an abstract type.
            // Useful predefined abstract types (all the possible CONSTANT_KIND types).
            // -----------------------------------------------------------------------------------------------
            // Instance fields
            // -----------------------------------------------------------------------------------------------
            // -----------------------------------------------------------------------------------------------
            // Constructor
            // -----------------------------------------------------------------------------------------------
            this.owner = owner;
        }

        /// <summary>Sets this frame to the value of the given frame.</summary>
        /// <remarks>
        ///     Sets this frame to the value of the given frame.
        ///     <p>
        ///         WARNING: after this method is called the two frames share the same data structures. It is
        ///         recommended to discard the given frame to avoid unexpected side effects.
        /// </remarks>
        /// <param name="frame">The new frame value.</param>
        internal void CopyFrom(Frame frame)
        {
            inputLocals = frame.inputLocals;
            inputStack = frame.inputStack;
            outputStackStart = 0;
            outputLocals = frame.outputLocals;
            outputStack = frame.outputStack;
            outputStackTop = frame.outputStackTop;
            initializationCount = frame.initializationCount;
            initializations = frame.initializations;
        }

        // -----------------------------------------------------------------------------------------------
        // Static methods to get abstract types from other type formats
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the abstract type corresponding to the given public API frame element type.
        /// </summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="type">
        ///     a frame element type described using the same format as in
        ///     <see cref="MethodVisitor.VisitFrame(int, int, object[], int, object[])" />
        ///     , i.e. either
        ///     <see cref="Opcodes.Top" />
        ///     ,
        ///     <see cref="Opcodes.Integer" />
        ///     ,
        ///     <see cref="Opcodes.Float" />
        ///     ,
        ///     <see cref="Opcodes.Long" />
        ///     ,
        ///     <see cref="double" />
        ///     ,
        ///     <see cref="Opcodes.Null" />
        ///     , or
        ///     <see cref="Opcodes.Uninitialized_This" />
        ///     , or the internal name of a class, or a Label designating
        ///     a NEW instruction (for uninitialized types).
        /// </param>
        /// <returns>the abstract type corresponding to the given frame element type.</returns>
        internal static int GetAbstractTypeFromApiFormat(SymbolTable symbolTable, object
            type)
        {
            if (type is int) return Constant_Kind | (int) type;

            if (type is string)
            {
                var descriptor = Type.GetObjectType((string) type).GetDescriptor();
                return GetAbstractTypeFromDescriptor(symbolTable, descriptor, 0);
            }

            return Uninitialized_Kind | symbolTable.AddUninitializedType(string.Empty, ((Label
                       ) type).bytecodeOffset);
        }

        /// <summary>
        ///     Returns the abstract type corresponding to the internal name of a class.
        /// </summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="internalName">
        ///     the internal name of a class. This must <i>not</i> be an array type
        ///     descriptor.
        /// </param>
        /// <returns>the abstract type value corresponding to the given internal name.</returns>
        internal static int GetAbstractTypeFromInternalName(SymbolTable symbolTable, string
            internalName)
        {
            return Reference_Kind | symbolTable.AddType(internalName);
        }

        /// <summary>Returns the abstract type corresponding to the given type descriptor.</summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="buffer">a string ending with a type descriptor.</param>
        /// <param name="offset">the start offset of the type descriptor in buffer.</param>
        /// <returns>the abstract type corresponding to the given type descriptor.</returns>
        private static int GetAbstractTypeFromDescriptor(SymbolTable symbolTable, string
            buffer, int offset)
        {
            string internalName;
            switch (buffer[offset])
            {
                case 'V':
                {
                    return 0;
                }

                case 'Z':
                case 'C':
                case 'B':
                case 'S':
                case 'I':
                {
                    return Integer;
                }

                case 'F':
                {
                    return Float;
                }

                case 'J':
                {
                    return Long;
                }

                case 'D':
                {
                    return Double;
                }

                case 'L':
                {
                    internalName = Runtime.Substring(buffer, offset + 1, buffer.Length - 1);
                    return Reference_Kind | symbolTable.AddType(internalName);
                }

                case '[':
                {
                    var elementDescriptorOffset = offset + 1;
                    while (buffer[elementDescriptorOffset] == '[') ++elementDescriptorOffset;
                    int typeValue;
                    switch (buffer[elementDescriptorOffset])
                    {
                        case 'Z':
                        {
                            typeValue = Boolean;
                            break;
                        }

                        case 'C':
                        {
                            typeValue = Char;
                            break;
                        }

                        case 'B':
                        {
                            typeValue = Byte;
                            break;
                        }

                        case 'S':
                        {
                            typeValue = Short;
                            break;
                        }

                        case 'I':
                        {
                            typeValue = Integer;
                            break;
                        }

                        case 'F':
                        {
                            typeValue = Float;
                            break;
                        }

                        case 'J':
                        {
                            typeValue = Long;
                            break;
                        }

                        case 'D':
                        {
                            typeValue = Double;
                            break;
                        }

                        case 'L':
                        {
                            internalName = Runtime.Substring(buffer, elementDescriptorOffset + 1, buffer
                                                                                                      .Length - 1);
                            typeValue = Reference_Kind | symbolTable.AddType(internalName);
                            break;
                        }

                        default:
                        {
                            throw new ArgumentException();
                        }
                    }

                    return ((elementDescriptorOffset - offset) << Dim_Shift) | typeValue;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Methods related to the input frame
        // -----------------------------------------------------------------------------------------------
        /// <summary>Sets the input frame from the given method description.</summary>
        /// <remarks>
        ///     Sets the input frame from the given method description. This method is used to initialize the
        ///     first frame of a method, which is implicit (i.e. not stored explicitly in the StackMapTable
        ///     attribute).
        /// </remarks>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="access">the method's access flags.</param>
        /// <param name="descriptor">the method descriptor.</param>
        /// <param name="maxLocals">the maximum number of local variables of the method.</param>
        internal void SetInputFrameFromDescriptor(SymbolTable symbolTable, AccessFlags access, string
            descriptor, int maxLocals)
        {
            inputLocals = new int[maxLocals];
            inputStack = new int[0];
            var inputLocalIndex = 0;
            if (access.HasNotFlagFast(AccessFlags.Static))
            {
                if (access.HasNotFlagFast(AccessFlags.Abstract))
                    inputLocals[inputLocalIndex++] = Reference_Kind | symbolTable.AddType(symbolTable
                                                         .GetClassName());
                else
                    inputLocals[inputLocalIndex++] = Uninitialized_This;
            }

            foreach (var argumentType in Type.GetArgumentTypes(descriptor))
            {
                var abstractType = GetAbstractTypeFromDescriptor(symbolTable, argumentType.GetDescriptor
                    (), 0);
                inputLocals[inputLocalIndex++] = abstractType;
                if (abstractType == Long || abstractType == Double) inputLocals[inputLocalIndex++] = Top;
            }

            while (inputLocalIndex < maxLocals) inputLocals[inputLocalIndex++] = Top;
        }

        /// <summary>Sets the input frame from the given public API frame description.</summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="numLocal">the number of local variables.</param>
        /// <param name="local">
        ///     the local variable types, described using the same format as in
        ///     <see cref="MethodVisitor.VisitFrame(int, int, object[], int, object[])" />
        ///     .
        /// </param>
        /// <param name="numStack">the number of operand stack elements.</param>
        /// <param name="stack">
        ///     the operand stack types, described using the same format as in
        ///     <see cref="MethodVisitor.VisitFrame(int, int, object[], int, object[])" />
        ///     .
        /// </param>
        internal void SetInputFrameFromApiFormat(SymbolTable symbolTable, int numLocal, object
            [] local, int numStack, object[] stack)
        {
            var inputLocalIndex = 0;
            for (var i = 0; i < numLocal; ++i)
            {
                inputLocals[inputLocalIndex++] = GetAbstractTypeFromApiFormat(symbolTable, local[
                    i]);
                if (local[i] == (object) OpcodesConstants.Long || local[i] == (object) OpcodesConstants.Double)
                    inputLocals[inputLocalIndex++] = Top;
            }

            while (inputLocalIndex < inputLocals.Length) inputLocals[inputLocalIndex++] = Top;
            var numStackTop = 0;
            for (var i = 0; i < numStack; ++i)
                if (stack[i] == (object) OpcodesConstants.Long || stack[i] == (object) OpcodesConstants.Double)
                    ++numStackTop;
            inputStack = new int[numStack + numStackTop];
            var inputStackIndex = 0;
            for (var i = 0; i < numStack; ++i)
            {
                inputStack[inputStackIndex++] = GetAbstractTypeFromApiFormat(symbolTable, stack[i
                ]);
                if (stack[i] == (object) OpcodesConstants.Long || stack[i] == (object) OpcodesConstants.Double)
                    inputStack[inputStackIndex++] = Top;
            }

            outputStackTop = 0;
            initializationCount = 0;
        }

        internal int GetInputStackSize()
        {
            return inputStack.Length;
        }

        // -----------------------------------------------------------------------------------------------
        // Methods related to the output frame
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the abstract type stored at the given local variable index in the output frame.
        /// </summary>
        /// <param name="localIndex">
        ///     the index of the local variable whose value must be returned.
        /// </param>
        /// <returns>
        ///     the abstract type stored at the given local variable index in the output frame.
        /// </returns>
        private int GetLocal(int localIndex)
        {
            if (outputLocals == null || localIndex >= outputLocals.Length)
                // If this local has never been assigned in this basic block, it is still equal to its value
                // in the input frame.
                return Local_Kind | localIndex;

            var abstractType = outputLocals[localIndex];
            if (abstractType == 0)
                // If this local has never been assigned in this basic block, so it is still equal to its
                // value in the input frame.
                abstractType = outputLocals[localIndex] = Local_Kind | localIndex;
            return abstractType;
        }

        /// <summary>
        ///     Replaces the abstract type stored at the given local variable index in the output frame.
        /// </summary>
        /// <param name="localIndex">
        ///     the index of the output frame local variable that must be set.
        /// </param>
        /// <param name="abstractType">the value that must be set.</param>
        private void SetLocal(int localIndex, int abstractType)
        {
            // Create and/or resize the output local variables array if necessary.
            if (outputLocals == null) outputLocals = new int[10];
            var outputLocalsLength = outputLocals.Length;
            if (localIndex >= outputLocalsLength)
            {
                var newOutputLocals = new int[Math.Max(localIndex + 1, 2 * outputLocalsLength
                )];
                Array.Copy(outputLocals, 0, newOutputLocals, 0, outputLocalsLength);
                outputLocals = newOutputLocals;
            }

            // Set the local variable.
            outputLocals[localIndex] = abstractType;
        }

        /// <summary>Pushes the given abstract type on the output frame stack.</summary>
        /// <param name="abstractType">an abstract type.</param>
        private void Push(int abstractType)
        {
            // Create and/or resize the output stack array if necessary.
            if (outputStack == null) outputStack = new int[10];
            var outputStackLength = outputStack.Length;
            if (outputStackTop >= outputStackLength)
            {
                var newOutputStack = new int[Math.Max(outputStackTop + 1, 2 * outputStackLength
                )];
                Array.Copy(outputStack, 0, newOutputStack, 0, outputStackLength);
                outputStack = newOutputStack;
            }

            // Pushes the abstract type on the output stack.
            outputStack[outputStackTop++] = abstractType;
            // Updates the maximum size reached by the output stack, if needed (note that this size is
            // relative to the input stack size, which is not known yet).
            var outputStackSize = (short) (outputStackStart + outputStackTop);
            if (outputStackSize > owner.outputStackMax) owner.outputStackMax = outputStackSize;
        }

        /// <summary>
        ///     Pushes the abstract type corresponding to the given descriptor on the output frame stack.
        /// </summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="descriptor">
        ///     a type or method descriptor (in which case its return type is pushed).
        /// </param>
        private void Push(SymbolTable symbolTable, string descriptor)
        {
            var typeDescriptorOffset = descriptor[0] == '('
                ? Type.GetReturnTypeOffset(descriptor
                )
                : 0;
            var abstractType = GetAbstractTypeFromDescriptor(symbolTable, descriptor, typeDescriptorOffset
            );
            if (abstractType != 0)
            {
                Push(abstractType);
                if (abstractType == Long || abstractType == Double) Push(Top);
            }
        }

        /// <summary>
        ///     Pops an abstract type from the output frame stack and returns its value.
        /// </summary>
        /// <returns>the abstract type that has been popped from the output frame stack.</returns>
        private int Pop()
        {
            if (outputStackTop > 0)
                return outputStack[--outputStackTop];
            return Stack_Kind | -(--outputStackStart);
        }

        /// <summary>Pops the given number of abstract types from the output frame stack.</summary>
        /// <param name="elements">the number of abstract types that must be popped.</param>
        private void Pop(int elements)
        {
            if (outputStackTop >= elements)
            {
                outputStackTop -= (short) elements;
            }
            else
            {
                // If the number of elements to be popped is greater than the number of elements in the output
                // stack, clear it, and pop the remaining elements from the input stack.
                outputStackStart -= elements - outputStackTop;
                outputStackTop = 0;
            }
        }

        /// <summary>
        ///     Pops as many abstract types from the output frame stack as described by the given descriptor.
        /// </summary>
        /// <param name="descriptor">
        ///     a type or method descriptor (in which case its argument types are popped).
        /// </param>
        private void Pop(string descriptor)
        {
            var firstDescriptorChar = descriptor[0];
            if (firstDescriptorChar == '(')
                Pop((Type.GetArgumentsAndReturnSizes(descriptor) >> 2) - 1);
            else if (firstDescriptorChar == 'J' || firstDescriptorChar == 'D')
                Pop(2);
            else
                Pop(1);
        }

        // -----------------------------------------------------------------------------------------------
        // Methods to handle uninitialized types
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds an abstract type to the list of types on which a constructor is invoked in the basic
        ///     block.
        /// </summary>
        /// <param name="abstractType">an abstract type on a which a constructor is invoked.</param>
        private void AddInitializedType(int abstractType)
        {
            // Create and/or resize the initializations array if necessary.
            if (initializations == null) initializations = new int[2];
            var initializationsLength = initializations.Length;
            if (initializationCount >= initializationsLength)
            {
                var newInitializations = new int[Math.Max(initializationCount + 1, 2 * initializationsLength
                )];
                Array.Copy(initializations, 0, newInitializations, 0, initializationsLength
                );
                initializations = newInitializations;
            }

            // Store the abstract type.
            initializations[initializationCount++] = abstractType;
        }

        /// <summary>
        ///     Returns the "initialized" abstract type corresponding to the given abstract type.
        /// </summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="abstractType">an abstract type.</param>
        /// <returns>
        ///     the REFERENCE_KIND abstract type corresponding to abstractType if it is
        ///     UNINITIALIZED_THIS or an UNINITIALIZED_KIND abstract type for one of the types on which a
        ///     constructor is invoked in the basic block. Otherwise returns abstractType.
        /// </returns>
        private int GetInitializedType(SymbolTable symbolTable, int abstractType)
        {
            if (abstractType == Uninitialized_This || (abstractType & (Dim_Mask | Kind_Mask))
                == Uninitialized_Kind)
                for (var i = 0; i < initializationCount; ++i)
                {
                    var initializedType = initializations[i];
                    var dim = initializedType & Dim_Mask;
                    var kind = initializedType & Kind_Mask;
                    var value = initializedType & Value_Mask;
                    if (kind == Local_Kind)
                        initializedType = dim + inputLocals[value];
                    else if (kind == Stack_Kind) initializedType = dim + inputStack[inputStack.Length - value];
                    if (abstractType == initializedType)
                    {
                        if (abstractType == Uninitialized_This)
                            return Reference_Kind | symbolTable.AddType(symbolTable.GetClassName());
                        return Reference_Kind | symbolTable.AddType(symbolTable.GetType(abstractType & Value_Mask
                               ).value);
                    }
                }

            return abstractType;
        }

        // -----------------------------------------------------------------------------------------------
        // Main method, to simulate the execution of each instruction on the output frame
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Simulates the action of the given instruction on the output stack frame.
        /// </summary>
        /// <param name="opcode">the opcode of the instruction.</param>
        /// <param name="arg">the numeric operand of the instruction, if any.</param>
        /// <param name="argSymbol">the Symbol operand of the instruction, if any.</param>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        internal virtual void Execute(int opcode, int arg, Symbol argSymbol, SymbolTable
            symbolTable)
        {
            // Abstract types popped from the stack or read from local variables.
            int abstractType1;
            int abstractType2;
            int abstractType3;
            int abstractType4;
            switch (opcode)
            {
                case OpcodesConstants.Nop:
                case OpcodesConstants.Ineg:
                case OpcodesConstants.Lneg:
                case OpcodesConstants.Fneg:
                case OpcodesConstants.Dneg:
                case OpcodesConstants.I2b:
                case OpcodesConstants.I2c:
                case OpcodesConstants.I2s:
                case OpcodesConstants.Goto:
                case OpcodesConstants.Return:
                {
                    break;
                }

                case OpcodesConstants.Aconst_Null:
                {
                    Push(Null);
                    break;
                }

                case OpcodesConstants.Iconst_M1:
                case OpcodesConstants.Iconst_0:
                case OpcodesConstants.Iconst_1:
                case OpcodesConstants.Iconst_2:
                case OpcodesConstants.Iconst_3:
                case OpcodesConstants.Iconst_4:
                case OpcodesConstants.Iconst_5:
                case OpcodesConstants.Bipush:
                case OpcodesConstants.Sipush:
                case OpcodesConstants.Iload:
                {
                    Push(Integer);
                    break;
                }

                case OpcodesConstants.Lconst_0:
                case OpcodesConstants.Lconst_1:
                case OpcodesConstants.Lload:
                {
                    Push(Long);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Fconst_0:
                case OpcodesConstants.Fconst_1:
                case OpcodesConstants.Fconst_2:
                case OpcodesConstants.Fload:
                {
                    Push(Float);
                    break;
                }

                case OpcodesConstants.Dconst_0:
                case OpcodesConstants.Dconst_1:
                case OpcodesConstants.Dload:
                {
                    Push(Double);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Ldc:
                {
                    switch (argSymbol.tag)
                    {
                        case Symbol.Constant_Integer_Tag:
                        {
                            Push(Integer);
                            break;
                        }

                        case Symbol.Constant_Long_Tag:
                        {
                            Push(Long);
                            Push(Top);
                            break;
                        }

                        case Symbol.Constant_Float_Tag:
                        {
                            Push(Float);
                            break;
                        }

                        case Symbol.Constant_Double_Tag:
                        {
                            Push(Double);
                            Push(Top);
                            break;
                        }

                        case Symbol.Constant_Class_Tag:
                        {
                            Push(Reference_Kind | symbolTable.AddType("java/lang/Class"));
                            break;
                        }

                        case Symbol.Constant_String_Tag:
                        {
                            Push(Reference_Kind | symbolTable.AddType("java/lang/String"));
                            break;
                        }

                        case Symbol.Constant_Method_Type_Tag:
                        {
                            Push(Reference_Kind | symbolTable.AddType("java/lang/invoke/MethodType"));
                            break;
                        }

                        case Symbol.Constant_Method_Handle_Tag:
                        {
                            Push(Reference_Kind | symbolTable.AddType("java/lang/invoke/MethodHandle"));
                            break;
                        }

                        case Symbol.Constant_Dynamic_Tag:
                        {
                            Push(symbolTable, argSymbol.value);
                            break;
                        }

                        default:
                        {
                            throw new AssertionError();
                        }
                    }

                    break;
                }

                case OpcodesConstants.Aload:
                {
                    Push(GetLocal(arg));
                    break;
                }

                case OpcodesConstants.Laload:
                case OpcodesConstants.D2l:
                {
                    Pop(2);
                    Push(Long);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Daload:
                case OpcodesConstants.L2d:
                {
                    Pop(2);
                    Push(Double);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Aaload:
                {
                    Pop(1);
                    abstractType1 = Pop();
                    Push(abstractType1 == Null ? abstractType1 : Element_Of + abstractType1);
                    break;
                }

                case OpcodesConstants.Istore:
                case OpcodesConstants.Fstore:
                case OpcodesConstants.Astore:
                {
                    abstractType1 = Pop();
                    SetLocal(arg, abstractType1);
                    if (arg > 0)
                    {
                        var previousLocalType = GetLocal(arg - 1);
                        if (previousLocalType == Long || previousLocalType == Double)
                            SetLocal(arg - 1, Top);
                        else if ((previousLocalType & Kind_Mask) == Local_Kind || (previousLocalType & Kind_Mask
                                 ) == Stack_Kind)
                            // The type of the previous local variable is not known yet, but if it later appears
                            // to be LONG or DOUBLE, we should then use TOP instead.
                            SetLocal(arg - 1, previousLocalType | Top_If_Long_Or_Double_Flag);
                    }

                    break;
                }

                case OpcodesConstants.Lstore:
                case OpcodesConstants.Dstore:
                {
                    Pop(1);
                    abstractType1 = Pop();
                    SetLocal(arg, abstractType1);
                    SetLocal(arg + 1, Top);
                    if (arg > 0)
                    {
                        var previousLocalType = GetLocal(arg - 1);
                        if (previousLocalType == Long || previousLocalType == Double)
                            SetLocal(arg - 1, Top);
                        else if ((previousLocalType & Kind_Mask) == Local_Kind || (previousLocalType & Kind_Mask
                                 ) == Stack_Kind)
                            // The type of the previous local variable is not known yet, but if it later appears
                            // to be LONG or DOUBLE, we should then use TOP instead.
                            SetLocal(arg - 1, previousLocalType | Top_If_Long_Or_Double_Flag);
                    }

                    break;
                }

                case OpcodesConstants.Iastore:
                case OpcodesConstants.Bastore:
                case OpcodesConstants.Castore:
                case OpcodesConstants.Sastore:
                case OpcodesConstants.Fastore:
                case OpcodesConstants.Aastore:
                {
                    Pop(3);
                    break;
                }

                case OpcodesConstants.Lastore:
                case OpcodesConstants.Dastore:
                {
                    Pop(4);
                    break;
                }

                case OpcodesConstants.Pop:
                case OpcodesConstants.Ifeq:
                case OpcodesConstants.Ifne:
                case OpcodesConstants.Iflt:
                case OpcodesConstants.Ifge:
                case OpcodesConstants.Ifgt:
                case OpcodesConstants.Ifle:
                case OpcodesConstants.Ireturn:
                case OpcodesConstants.Freturn:
                case OpcodesConstants.Areturn:
                case OpcodesConstants.Tableswitch:
                case OpcodesConstants.Lookupswitch:
                case OpcodesConstants.Athrow:
                case OpcodesConstants.Monitorenter:
                case OpcodesConstants.Monitorexit:
                case OpcodesConstants.Ifnull:
                case OpcodesConstants.Ifnonnull:
                {
                    Pop(1);
                    break;
                }

                case OpcodesConstants.Pop2:
                case OpcodesConstants.If_Icmpeq:
                case OpcodesConstants.If_Icmpne:
                case OpcodesConstants.If_Icmplt:
                case OpcodesConstants.If_Icmpge:
                case OpcodesConstants.If_Icmpgt:
                case OpcodesConstants.If_Icmple:
                case OpcodesConstants.If_Acmpeq:
                case OpcodesConstants.If_Acmpne:
                case OpcodesConstants.Lreturn:
                case OpcodesConstants.Dreturn:
                {
                    Pop(2);
                    break;
                }

                case OpcodesConstants.Dup:
                {
                    abstractType1 = Pop();
                    Push(abstractType1);
                    Push(abstractType1);
                    break;
                }

                case OpcodesConstants.Dup_X1:
                {
                    abstractType1 = Pop();
                    abstractType2 = Pop();
                    Push(abstractType1);
                    Push(abstractType2);
                    Push(abstractType1);
                    break;
                }

                case OpcodesConstants.Dup_X2:
                {
                    abstractType1 = Pop();
                    abstractType2 = Pop();
                    abstractType3 = Pop();
                    Push(abstractType1);
                    Push(abstractType3);
                    Push(abstractType2);
                    Push(abstractType1);
                    break;
                }

                case OpcodesConstants.Dup2:
                {
                    abstractType1 = Pop();
                    abstractType2 = Pop();
                    Push(abstractType2);
                    Push(abstractType1);
                    Push(abstractType2);
                    Push(abstractType1);
                    break;
                }

                case OpcodesConstants.Dup2_X1:
                {
                    abstractType1 = Pop();
                    abstractType2 = Pop();
                    abstractType3 = Pop();
                    Push(abstractType2);
                    Push(abstractType1);
                    Push(abstractType3);
                    Push(abstractType2);
                    Push(abstractType1);
                    break;
                }

                case OpcodesConstants.Dup2_X2:
                {
                    abstractType1 = Pop();
                    abstractType2 = Pop();
                    abstractType3 = Pop();
                    abstractType4 = Pop();
                    Push(abstractType2);
                    Push(abstractType1);
                    Push(abstractType4);
                    Push(abstractType3);
                    Push(abstractType2);
                    Push(abstractType1);
                    break;
                }

                case OpcodesConstants.Swap:
                {
                    abstractType1 = Pop();
                    abstractType2 = Pop();
                    Push(abstractType1);
                    Push(abstractType2);
                    break;
                }

                case OpcodesConstants.Iaload:
                case OpcodesConstants.Baload:
                case OpcodesConstants.Caload:
                case OpcodesConstants.Saload:
                case OpcodesConstants.Iadd:
                case OpcodesConstants.Isub:
                case OpcodesConstants.Imul:
                case OpcodesConstants.Idiv:
                case OpcodesConstants.Irem:
                case OpcodesConstants.Iand:
                case OpcodesConstants.Ior:
                case OpcodesConstants.Ixor:
                case OpcodesConstants.Ishl:
                case OpcodesConstants.Ishr:
                case OpcodesConstants.Iushr:
                case OpcodesConstants.L2i:
                case OpcodesConstants.D2i:
                case OpcodesConstants.Fcmpl:
                case OpcodesConstants.Fcmpg:
                {
                    Pop(2);
                    Push(Integer);
                    break;
                }

                case OpcodesConstants.Ladd:
                case OpcodesConstants.Lsub:
                case OpcodesConstants.Lmul:
                case OpcodesConstants.Ldiv:
                case OpcodesConstants.Lrem:
                case OpcodesConstants.Land:
                case OpcodesConstants.Lor:
                case OpcodesConstants.Lxor:
                {
                    Pop(4);
                    Push(Long);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Faload:
                case OpcodesConstants.Fadd:
                case OpcodesConstants.Fsub:
                case OpcodesConstants.Fmul:
                case OpcodesConstants.Fdiv:
                case OpcodesConstants.Frem:
                case OpcodesConstants.L2f:
                case OpcodesConstants.D2f:
                {
                    Pop(2);
                    Push(Float);
                    break;
                }

                case OpcodesConstants.Dadd:
                case OpcodesConstants.Dsub:
                case OpcodesConstants.Dmul:
                case OpcodesConstants.Ddiv:
                case OpcodesConstants.Drem:
                {
                    Pop(4);
                    Push(Double);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Lshl:
                case OpcodesConstants.Lshr:
                case OpcodesConstants.Lushr:
                {
                    Pop(3);
                    Push(Long);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.Iinc:
                {
                    SetLocal(arg, Integer);
                    break;
                }

                case OpcodesConstants.I2l:
                case OpcodesConstants.F2l:
                {
                    Pop(1);
                    Push(Long);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.I2f:
                {
                    Pop(1);
                    Push(Float);
                    break;
                }

                case OpcodesConstants.I2d:
                case OpcodesConstants.F2d:
                {
                    Pop(1);
                    Push(Double);
                    Push(Top);
                    break;
                }

                case OpcodesConstants.F2i:
                case OpcodesConstants.Arraylength:
                case OpcodesConstants.Instanceof:
                {
                    Pop(1);
                    Push(Integer);
                    break;
                }

                case OpcodesConstants.Lcmp:
                case OpcodesConstants.Dcmpl:
                case OpcodesConstants.Dcmpg:
                {
                    Pop(4);
                    Push(Integer);
                    break;
                }

                case OpcodesConstants.Jsr:
                case OpcodesConstants.Ret:
                {
                    throw new ArgumentException("JSR/RET are not supported with computeFrames option"
                    );
                }

                case OpcodesConstants.Getstatic:
                {
                    Push(symbolTable, argSymbol.value);
                    break;
                }

                case OpcodesConstants.Putstatic:
                {
                    Pop(argSymbol.value);
                    break;
                }

                case OpcodesConstants.Getfield:
                {
                    Pop(1);
                    Push(symbolTable, argSymbol.value);
                    break;
                }

                case OpcodesConstants.Putfield:
                {
                    Pop(argSymbol.value);
                    Pop();
                    break;
                }

                case OpcodesConstants.Invokevirtual:
                case OpcodesConstants.Invokespecial:
                case OpcodesConstants.Invokestatic:
                case OpcodesConstants.Invokeinterface:
                {
                    Pop(argSymbol.value);
                    if (opcode != OpcodesConstants.Invokestatic)
                    {
                        abstractType1 = Pop();
                        if (opcode == OpcodesConstants.Invokespecial && argSymbol.name[0] == '<')
                            AddInitializedType(abstractType1);
                    }

                    Push(symbolTable, argSymbol.value);
                    break;
                }

                case OpcodesConstants.Invokedynamic:
                {
                    Pop(argSymbol.value);
                    Push(symbolTable, argSymbol.value);
                    break;
                }

                case OpcodesConstants.New:
                {
                    Push(Uninitialized_Kind | symbolTable.AddUninitializedType(argSymbol.value, arg));
                    break;
                }

                case OpcodesConstants.Newarray:
                {
                    Pop();
                    switch (arg)
                    {
                        case OpcodesConstants.T_Boolean:
                        {
                            Push(Array_Of | Boolean);
                            break;
                        }

                        case OpcodesConstants.T_Char:
                        {
                            Push(Array_Of | Char);
                            break;
                        }

                        case OpcodesConstants.T_Byte:
                        {
                            Push(Array_Of | Byte);
                            break;
                        }

                        case OpcodesConstants.T_Short:
                        {
                            Push(Array_Of | Short);
                            break;
                        }

                        case OpcodesConstants.T_Int:
                        {
                            Push(Array_Of | Integer);
                            break;
                        }

                        case OpcodesConstants.T_Float:
                        {
                            Push(Array_Of | Float);
                            break;
                        }

                        case OpcodesConstants.T_Double:
                        {
                            Push(Array_Of | Double);
                            break;
                        }

                        case OpcodesConstants.T_Long:
                        {
                            Push(Array_Of | Long);
                            break;
                        }

                        default:
                        {
                            throw new ArgumentException();
                        }
                    }

                    break;
                }

                case OpcodesConstants.Anewarray:
                {
                    var arrayElementType = argSymbol.value;
                    Pop();
                    if (arrayElementType[0] == '[')
                        Push(symbolTable, '[' + arrayElementType);
                    else
                        Push(Array_Of | Reference_Kind | symbolTable.AddType(arrayElementType));
                    break;
                }

                case OpcodesConstants.Checkcast:
                {
                    var castType = argSymbol.value;
                    Pop();
                    if (castType[0] == '[')
                        Push(symbolTable, castType);
                    else
                        Push(Reference_Kind | symbolTable.AddType(castType));
                    break;
                }

                case OpcodesConstants.Multianewarray:
                {
                    Pop(arg);
                    Push(symbolTable, argSymbol.value);
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Frame merging methods, used in the second step of the stack map frame computation algorithm
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Computes the concrete output type corresponding to a given abstract output type.
        /// </summary>
        /// <param name="abstractOutputType">an abstract output type.</param>
        /// <param name="numStack">
        ///     the size of the input stack, used to resolve abstract output types of
        ///     STACK_KIND kind.
        /// </param>
        /// <returns>the concrete output type corresponding to 'abstractOutputType'.</returns>
        private int GetConcreteOutputType(int abstractOutputType, int numStack)
        {
            var dim = abstractOutputType & Dim_Mask;
            var kind = abstractOutputType & Kind_Mask;
            if (kind == Local_Kind)
            {
                // By definition, a LOCAL_KIND type designates the concrete type of a local variable at
                // the beginning of the basic block corresponding to this frame (which is known when
                // this method is called, but was not when the abstract type was computed).
                var concreteOutputType = dim + inputLocals[abstractOutputType & Value_Mask];
                if ((abstractOutputType & Top_If_Long_Or_Double_Flag) != 0 && (concreteOutputType
                                                                               == Long || concreteOutputType == Double))
                    concreteOutputType = Top;
                return concreteOutputType;
            }

            if (kind == Stack_Kind)
            {
                // By definition, a STACK_KIND type designates the concrete type of a local variable at
                // the beginning of the basic block corresponding to this frame (which is known when
                // this method is called, but was not when the abstract type was computed).
                var concreteOutputType = dim + inputStack[numStack - (abstractOutputType & Value_Mask
                                                          )];
                if ((abstractOutputType & Top_If_Long_Or_Double_Flag) != 0 && (concreteOutputType
                                                                               == Long || concreteOutputType == Double))
                    concreteOutputType = Top;
                return concreteOutputType;
            }

            return abstractOutputType;
        }

        /// <summary>
        ///     Merges the input frame of the given
        ///     <see cref="Frame" />
        ///     with the input and output frames of this
        ///     <see cref="Frame" />
        ///     . Returns
        ///     <literal>true</literal>
        ///     if the given frame has been changed by this operation
        ///     (the input and output frames of this
        ///     <see cref="Frame" />
        ///     are never changed).
        /// </summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="dstFrame">
        ///     the
        ///     <see cref="Frame" />
        ///     whose input frame must be updated. This should be the frame
        ///     of a successor, in the control flow graph, of the basic block corresponding to this frame.
        /// </param>
        /// <param name="catchTypeIndex">
        ///     if 'frame' corresponds to an exception handler basic block, the type
        ///     table index of the caught exception type, otherwise 0.
        /// </param>
        /// <returns>
        ///     <literal>true</literal>
        ///     if the input frame of 'frame' has been changed by this operation.
        /// </returns>
        internal bool Merge(SymbolTable symbolTable, Frame dstFrame, int catchTypeIndex)
        {
            var frameChanged = false;
            // Compute the concrete types of the local variables at the end of the basic block corresponding
            // to this frame, by resolving its abstract output types, and merge these concrete types with
            // those of the local variables in the input frame of dstFrame.
            var numLocal = inputLocals.Length;
            var numStack = inputStack.Length;
            if (dstFrame.inputLocals == null)
            {
                dstFrame.inputLocals = new int[numLocal];
                frameChanged = true;
            }

            for (var i = 0; i < numLocal; ++i)
            {
                int concreteOutputType;
                if (outputLocals != null && i < outputLocals.Length)
                {
                    var abstractOutputType = outputLocals[i];
                    if (abstractOutputType == 0)
                        // If the local variable has never been assigned in this basic block, it is equal to its
                        // value at the beginning of the block.
                        concreteOutputType = inputLocals[i];
                    else
                        concreteOutputType = GetConcreteOutputType(abstractOutputType, numStack);
                }
                else
                {
                    // If the local variable has never been assigned in this basic block, it is equal to its
                    // value at the beginning of the block.
                    concreteOutputType = inputLocals[i];
                }

                // concreteOutputType might be an uninitialized type from the input locals or from the input
                // stack. However, if a constructor has been called for this class type in the basic block,
                // then this type is no longer uninitialized at the end of basic block.
                if (initializations != null) concreteOutputType = GetInitializedType(symbolTable, concreteOutputType);
                frameChanged |= Merge(symbolTable, concreteOutputType, dstFrame.inputLocals, i);
            }

            // If dstFrame is an exception handler block, it can be reached from any instruction of the
            // basic block corresponding to this frame, in particular from the first one. Therefore, the
            // input locals of dstFrame should be compatible (i.e. merged) with the input locals of this
            // frame (and the input stack of dstFrame should be compatible, i.e. merged, with a one
            // element stack containing the caught exception type).
            if (catchTypeIndex > 0)
            {
                for (var i = 0; i < numLocal; ++i)
                    frameChanged |= Merge(symbolTable, inputLocals[i], dstFrame.inputLocals, i);
                if (dstFrame.inputStack == null)
                {
                    dstFrame.inputStack = new int[1];
                    frameChanged = true;
                }

                frameChanged |= Merge(symbolTable, catchTypeIndex, dstFrame.inputStack, 0);
                return frameChanged;
            }

            // Compute the concrete types of the stack operands at the end of the basic block corresponding
            // to this frame, by resolving its abstract output types, and merge these concrete types with
            // those of the stack operands in the input frame of dstFrame.
            var numInputStack = inputStack.Length + outputStackStart;
            if (dstFrame.inputStack == null)
            {
                dstFrame.inputStack = new int[numInputStack + outputStackTop];
                frameChanged = true;
            }

            // First, do this for the stack operands that have not been popped in the basic block
            // corresponding to this frame, and which are therefore equal to their value in the input
            // frame (except for uninitialized types, which may have been initialized).
            for (var i = 0; i < numInputStack; ++i)
            {
                var concreteOutputType = inputStack[i];
                if (initializations != null) concreteOutputType = GetInitializedType(symbolTable, concreteOutputType);
                frameChanged |= Merge(symbolTable, concreteOutputType, dstFrame.inputStack, i);
            }

            // Then, do this for the stack operands that have pushed in the basic block (this code is the
            // same as the one above for local variables).
            for (var i = 0; i < outputStackTop; ++i)
            {
                var abstractOutputType = outputStack[i];
                var concreteOutputType = GetConcreteOutputType(abstractOutputType, numStack);
                if (initializations != null) concreteOutputType = GetInitializedType(symbolTable, concreteOutputType);
                frameChanged |= Merge(symbolTable, concreteOutputType, dstFrame.inputStack, numInputStack
                                                                                            + i);
            }

            return frameChanged;
        }

        /// <summary>
        ///     Merges the type at the given index in the given abstract type array with the given type.
        /// </summary>
        /// <remarks>
        ///     Merges the type at the given index in the given abstract type array with the given type.
        ///     Returns
        ///     <literal>true</literal>
        ///     if the type array has been modified by this operation.
        /// </remarks>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="sourceType">
        ///     the abstract type with which the abstract type array element must be merged.
        ///     This type should be of
        ///     <see cref="Constant_Kind" />
        ///     ,
        ///     <see cref="Reference_Kind" />
        ///     or
        ///     <see cref="Uninitialized_Kind" />
        ///     kind, with positive or
        ///     <literal>null</literal>
        ///     array dimensions.
        /// </param>
        /// <param name="dstTypes">
        ///     an array of abstract types. These types should be of
        ///     <see cref="Constant_Kind" />
        ///     ,
        ///     <see cref="Reference_Kind" />
        ///     or
        ///     <see cref="Uninitialized_Kind" />
        ///     kind, with positive or
        ///     <literal>null</literal>
        ///     array dimensions.
        /// </param>
        /// <param name="dstIndex">the index of the type that must be merged in dstTypes.</param>
        /// <returns>
        ///     <literal>true</literal>
        ///     if the type array has been modified by this operation.
        /// </returns>
        private static bool Merge(SymbolTable symbolTable, int sourceType, int[] dstTypes
            , int dstIndex)
        {
            var dstType = dstTypes[dstIndex];
            if (dstType == sourceType)
                // If the types are equal, merge(sourceType, dstType) = dstType, so there is no change.
                return false;
            var srcType = sourceType;
            if ((sourceType & ~Dim_Mask) == Null)
            {
                if (dstType == Null) return false;
                srcType = Null;
            }

            if (dstType == 0)
            {
                // If dstTypes[dstIndex] has never been assigned, merge(srcType, dstType) = srcType.
                dstTypes[dstIndex] = srcType;
                return true;
            }

            int mergedType;
            if ((dstType & Dim_Mask) != 0 || (dstType & Kind_Mask) == Reference_Kind)
            {
                // If dstType is a reference type of any array dimension.
                if (srcType == Null)
                    // If srcType is the NULL type, merge(srcType, dstType) = dstType, so there is no change.
                    return false;

                if ((srcType & (Dim_Mask | Kind_Mask)) == (dstType & (Dim_Mask | Kind_Mask)))
                {
                    // If srcType has the same array dimension and the same kind as dstType.
                    if ((dstType & Kind_Mask) == Reference_Kind)
                    {
                        // If srcType and dstType are reference types with the same array dimension,
                        // merge(srcType, dstType) = dim(srcType) | common super class of srcType and dstType.
                        mergedType = (srcType & Dim_Mask) | Reference_Kind | symbolTable.AddMergedType(srcType
                                                                                                       & Value_Mask,
                                         dstType & Value_Mask);
                    }
                    else
                    {
                        // If srcType and dstType are array types of equal dimension but different element types,
                        // merge(srcType, dstType) = dim(srcType) - 1 | java/lang/Object.
                        var mergedDim = Element_Of + (srcType & Dim_Mask);
                        mergedType = mergedDim | Reference_Kind | symbolTable.AddType("java/lang/Object");
                    }
                }
                else if ((srcType & Dim_Mask) != 0 || (srcType & Kind_Mask) == Reference_Kind)
                {
                    // If srcType is any other reference or array type,
                    // merge(srcType, dstType) = min(srcDdim, dstDim) | java/lang/Object
                    // where srcDim is the array dimension of srcType, minus 1 if srcType is an array type
                    // with a non reference element type (and similarly for dstDim).
                    var srcDim = srcType & Dim_Mask;
                    if (srcDim != 0 && (srcType & Kind_Mask) != Reference_Kind) srcDim = Element_Of + srcDim;
                    var dstDim = dstType & Dim_Mask;
                    if (dstDim != 0 && (dstType & Kind_Mask) != Reference_Kind) dstDim = Element_Of + dstDim;
                    mergedType = Math.Min(srcDim, dstDim) | Reference_Kind | symbolTable.AddType
                                     ("java/lang/Object");
                }
                else
                {
                    // If srcType is any other type, merge(srcType, dstType) = TOP.
                    mergedType = Top;
                }
            }
            else if (dstType == Null)
            {
                // If dstType is the NULL type, merge(srcType, dstType) = srcType, or TOP if srcType is not a
                // an array type or a reference type.
                mergedType = (srcType & Dim_Mask) != 0 || (srcType & Kind_Mask) == Reference_Kind
                    ? srcType
                    : Top;
            }
            else
            {
                // If dstType is any other type, merge(srcType, dstType) = TOP whatever srcType.
                mergedType = Top;
            }

            if (mergedType != dstType)
            {
                dstTypes[dstIndex] = mergedType;
                return true;
            }

            return false;
        }

        // -----------------------------------------------------------------------------------------------
        // Frame output methods, to generate StackMapFrame attributes
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Makes the given
        ///     <see cref="MethodWriter" />
        ///     visit the input frame of this
        ///     <see cref="Frame" />
        ///     . The visit is
        ///     done with the
        ///     <see cref="MethodWriter.VisitFrameStart(int, int, int)" />
        ///     ,
        ///     <see cref="MethodWriter.VisitAbstractType(int, int)" />
        ///     and
        ///     <see cref="MethodWriter.VisitFrameEnd()" />
        ///     methods.
        /// </summary>
        /// <param name="methodWriter">
        ///     the
        ///     <see cref="MethodWriter" />
        ///     that should visit the input frame of this
        ///     <see cref="Frame" />
        ///     .
        /// </param>
        internal void Accept(MethodWriter methodWriter)
        {
            // Compute the number of locals, ignoring TOP types that are just after a LONG or a DOUBLE, and
            // all trailing TOP types.
            var localTypes = inputLocals;
            var numLocal = 0;
            var numTrailingTop = 0;
            var i = 0;
            while (i < localTypes.Length)
            {
                var localType = localTypes[i];
                i += localType == Long || localType == Double ? 2 : 1;
                if (localType == Top)
                {
                    numTrailingTop++;
                }
                else
                {
                    numLocal += numTrailingTop + 1;
                    numTrailingTop = 0;
                }
            }

            // Compute the stack size, ignoring TOP types that are just after a LONG or a DOUBLE.
            var stackTypes = inputStack;
            var numStack = 0;
            i = 0;
            while (i < stackTypes.Length)
            {
                var stackType = stackTypes[i];
                i += stackType == Long || stackType == Double ? 2 : 1;
                numStack++;
            }

            // Visit the frame and its content.
            var frameIndex = methodWriter.VisitFrameStart(owner.bytecodeOffset, numLocal, numStack
            );
            i = 0;
            while (numLocal-- > 0)
            {
                var localType = localTypes[i];
                i += localType == Long || localType == Double ? 2 : 1;
                methodWriter.VisitAbstractType(frameIndex++, localType);
            }

            i = 0;
            while (numStack-- > 0)
            {
                var stackType = stackTypes[i];
                i += stackType == Long || stackType == Double ? 2 : 1;
                methodWriter.VisitAbstractType(frameIndex++, stackType);
            }

            methodWriter.VisitFrameEnd();
        }

        /// <summary>
        ///     Put the given abstract type in the given ByteVector, using the JVMS verification_type_info
        ///     format used in StackMapTable attributes.
        /// </summary>
        /// <param name="symbolTable">
        ///     the type table to use to lookup and store type
        ///     <see cref="Symbol" />
        ///     .
        /// </param>
        /// <param name="abstractType">
        ///     an abstract type, restricted to
        ///     <see cref="Constant_Kind" />
        ///     ,
        ///     <see cref="Reference_Kind" />
        ///     or
        ///     <see cref="Uninitialized_Kind" />
        ///     types.
        /// </param>
        /// <param name="output">where the abstract type must be put.</param>
        /// <seealso>
        ///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.4">
        ///         JVMS
        ///         *     4.7.4
        ///     </a>
        /// </seealso>
        internal static void PutAbstractType(SymbolTable symbolTable, int abstractType, ByteVector
            output)
        {
            var arrayDimensions = (abstractType & Dim_Mask) >> Dim_Shift;
            if (arrayDimensions == 0)
            {
                var typeValue = abstractType & Value_Mask;
                switch (abstractType & Kind_Mask)
                {
                    case Constant_Kind:
                    {
                        output.PutByte(typeValue);
                        break;
                    }

                    case Reference_Kind:
                    {
                        output.PutByte(Item_Object).PutShort(symbolTable.AddConstantClass(symbolTable.GetType
                            (typeValue).value).index);
                        break;
                    }

                    case Uninitialized_Kind:
                    {
                        output.PutByte(Item_Uninitialized).PutShort((int) symbolTable.GetType(typeValue).data
                        );
                        break;
                    }

                    default:
                    {
                        throw new AssertionError();
                    }
                }
            }
            else
            {
                // Case of an array type, we need to build its descriptor first.
                var typeDescriptor = new StringBuilder();
                while (arrayDimensions-- > 0) typeDescriptor.Append('[');
                if ((abstractType & Kind_Mask) == Reference_Kind)
                    typeDescriptor.Append('L').Append(symbolTable.GetType(abstractType & Value_Mask).value).Append(';');
                else
                    switch (abstractType & Value_Mask)
                    {
                        case Item_Asm_Boolean:
                        {
                            typeDescriptor.Append('Z');
                            break;
                        }

                        case Item_Asm_Byte:
                        {
                            typeDescriptor.Append('B');
                            break;
                        }

                        case Item_Asm_Char:
                        {
                            typeDescriptor.Append('C');
                            break;
                        }

                        case Item_Asm_Short:
                        {
                            typeDescriptor.Append('S');
                            break;
                        }

                        case Item_Integer:
                        {
                            typeDescriptor.Append('I');
                            break;
                        }

                        case Item_Float:
                        {
                            typeDescriptor.Append('F');
                            break;
                        }

                        case Item_Long:
                        {
                            typeDescriptor.Append('J');
                            break;
                        }

                        case Item_Double:
                        {
                            typeDescriptor.Append('D');
                            break;
                        }

                        default:
                        {
                            throw new AssertionError();
                        }
                    }

                output.PutByte(Item_Object).PutShort(symbolTable.AddConstantClass(typeDescriptor.ToString()).index);
            }
        }
    }
}