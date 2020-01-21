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
using ObjectWeb.Asm.Enums;

namespace ObjectWeb.Asm.Tree.Analysis
{
	/// <summary>A semantic bytecode interpreter.</summary>
	/// <remarks>
	///     A semantic bytecode interpreter. More precisely, this interpreter only manages the computation of
	///     values from other values: it does not manage the transfer of values to or from the stack, and to
	///     or from the local variables. This separation allows a generic bytecode
	///     <see cref="Analyzer{V}" />
	///     to work
	///     with various semantic interpreters, without needing to duplicate the code to simulate the
	///     transfer of values.
	/// </remarks>
	/// <?/>
	/// <author>Eric Bruneton</author>
	public abstract class Interpreter<V>
        where V : Value
    {
	    /// <summary>The ASM API version supported by this interpreter.</summary>
	    /// <remarks>
	    ///     The ASM API version supported by this interpreter. The value of this field must be one of
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
	    ///     ,
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
	    ///     ,
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
	    ///     or
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
	    ///     .
	    /// </remarks>
	    protected internal readonly VisitorAsmApiVersion api;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="Interpreter{V}" />
	    ///     .
	    /// </summary>
	    /// <param name="api">
	    ///     the ASM API version supported by this interpreter. Must be one of
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
	    ///     ,
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
	    ///     ,
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
	    ///     or
	    ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
	    ///     .
	    /// </param>
	    protected internal Interpreter(VisitorAsmApiVersion api)
        {
            this.api = api;
        }

	    /// <summary>Creates a new value that represents the given type.</summary>
	    /// <remarks>
	    ///     Creates a new value that represents the given type.
	    ///     <p>
	    ///         Called for method parameters (including <code>this</code>), exception handler variable and
	    ///         with <code>null</code> type for variables reserved by long and double types.
	    ///         <p>
	    ///             An interpreter may choose to implement one or more of
	    ///             <see cref="Interpreter{V}.NewReturnTypeValue(Org.Objectweb.Asm.Type)" />
	    ///             ,
	    ///             <see cref="Interpreter{V}.NewParameterValue(bool, int, Org.Objectweb.Asm.Type)" />
	    ///             ,
	    ///             <see cref="Interpreter{V}.NewEmptyValue(int)" />
	    ///             ,
	    ///             <see
	    ///                 cref="Interpreter{V}.NewExceptionValue(Org.Objectweb.Asm.Tree.TryCatchBlockNode, Frame{V}, Org.Objectweb.Asm.Type)
	    /// 	" />
	    ///             to distinguish different types
	    ///             of new value.
	    /// </remarks>
	    /// <param name="type">
	    ///     a primitive or reference type, or
	    ///     <literal>null</literal>
	    ///     to represent an uninitialized
	    ///     value.
	    /// </param>
	    /// <returns>
	    ///     a value that represents the given type. The size of the returned value must be equal to
	    ///     the size of the given type.
	    /// </returns>
	    public abstract V NewValue(Type type);

	    /// <summary>Creates a new value that represents the given parameter type.</summary>
	    /// <remarks>
	    ///     Creates a new value that represents the given parameter type. This method is called to
	    ///     initialize the value of a local corresponding to a method parameter in a frame.
	    ///     <p>By default, calls <code>newValue(type)</code>.
	    /// </remarks>
	    /// <param name="isInstanceMethod">
	    ///     <literal>true</literal>
	    ///     if the method is non-static.
	    /// </param>
	    /// <param name="local">the local variable index.</param>
	    /// <param name="type">a primitive or reference type.</param>
	    /// <returns>
	    ///     a value that represents the given type. The size of the returned value must be equal to
	    ///     the size of the given type.
	    /// </returns>
	    public virtual V NewParameterValue(bool isInstanceMethod, int local, Type type)
        {
            return NewValue(type);
        }

	    /// <summary>Creates a new value that represents the given return type.</summary>
	    /// <remarks>
	    ///     Creates a new value that represents the given return type. This method is called to initialize
	    ///     the return type value of a frame.
	    ///     <p>By default, calls <code>newValue(type)</code>.
	    /// </remarks>
	    /// <param name="type">a primitive or reference type.</param>
	    /// <returns>
	    ///     a value that represents the given type. The size of the returned value must be equal to
	    ///     the size of the given type.
	    /// </returns>
	    public virtual V NewReturnTypeValue(Type type)
        {
            return NewValue(type);
        }

	    /// <summary>Creates a new uninitialized value for a local variable.</summary>
	    /// <remarks>
	    ///     Creates a new uninitialized value for a local variable. This method is called to initialize the
	    ///     value of a local that does not correspond to a method parameter, and to reset one half of a
	    ///     size-2 value when the other half is assigned a size-1 value.
	    ///     <p>By default, calls <code>newValue(null)</code>.
	    /// </remarks>
	    /// <param name="local">the local variable index.</param>
	    /// <returns>
	    ///     a value representing an uninitialized value. The size of the returned value must be
	    ///     equal to 1.
	    /// </returns>
	    public virtual V NewEmptyValue(int local)
        {
            return NewValue(null);
        }

	    /// <summary>Creates a new value that represents the given exception type.</summary>
	    /// <remarks>
	    ///     Creates a new value that represents the given exception type. This method is called to
	    ///     initialize the exception value on the call stack at the entry of an exception handler.
	    ///     <p>By default, calls <code>newValue(exceptionType)</code>.
	    /// </remarks>
	    /// <param name="tryCatchBlockNode">the exception handler.</param>
	    /// <param name="handlerFrame">the exception handler frame.</param>
	    /// <param name="exceptionType">the exception type handled by this handler.</param>
	    /// <returns>
	    ///     a value that represents the given
	    ///     <paramref name="exceptionType" />
	    ///     . The size of the returned value
	    ///     must be equal to 1.
	    /// </returns>
	    public virtual V NewExceptionValue(TryCatchBlockNode tryCatchBlockNode, Frame<V>
            handlerFrame, Type exceptionType)
        {
            return NewValue(exceptionType);
        }

	    /// <summary>Interprets a bytecode instruction without arguments.</summary>
	    /// <remarks>
	    ///     Interprets a bytecode instruction without arguments. This method is called for the following
	    ///     opcodes:
	    ///     <p>
	    ///         ACONST_NULL, ICONST_M1, ICONST_0, ICONST_1, ICONST_2, ICONST_3, ICONST_4, ICONST_5,
	    ///         LCONST_0, LCONST_1, FCONST_0, FCONST_1, FCONST_2, DCONST_0, DCONST_1, BIPUSH, SIPUSH, LDC, JSR,
	    ///         GETSTATIC, NEW
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <returns>the result of the interpretation of the given instruction.</returns>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract V NewOperation(AbstractInsnNode insn);

	    /// <summary>
	    ///     Interprets a bytecode instruction that moves a value on the stack or to or from local
	    ///     variables.
	    /// </summary>
	    /// <remarks>
	    ///     Interprets a bytecode instruction that moves a value on the stack or to or from local
	    ///     variables. This method is called for the following opcodes:
	    ///     <p>
	    ///         ILOAD, LLOAD, FLOAD, DLOAD, ALOAD, ISTORE, LSTORE, FSTORE, DSTORE, ASTORE, DUP, DUP_X1,
	    ///         DUP_X2, DUP2, DUP2_X1, DUP2_X2, SWAP
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <param name="value">the value that must be moved by the instruction.</param>
	    /// <returns>
	    ///     the result of the interpretation of the given instruction. The returned value must be
	    ///     <c>equal</c>
	    ///     to the given value.
	    /// </returns>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract V CopyOperation(AbstractInsnNode insn, V value);

	    /// <summary>Interprets a bytecode instruction with a single argument.</summary>
	    /// <remarks>
	    ///     Interprets a bytecode instruction with a single argument. This method is called for the
	    ///     following opcodes:
	    ///     <p>
	    ///         INEG, LNEG, FNEG, DNEG, IINC, I2L, I2F, I2D, L2I, L2F, L2D, F2I, F2L, F2D, D2I, D2L, D2F,
	    ///         I2B, I2C, I2S, IFEQ, IFNE, IFLT, IFGE, IFGT, IFLE, TABLESWITCH, LOOKUPSWITCH, IRETURN, LRETURN,
	    ///         FRETURN, DRETURN, ARETURN, PUTSTATIC, GETFIELD, NEWARRAY, ANEWARRAY, ARRAYLENGTH, ATHROW,
	    ///         CHECKCAST, INSTANCEOF, MONITORENTER, MONITOREXIT, IFNULL, IFNONNULL
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <param name="value">the argument of the instruction to be interpreted.</param>
	    /// <returns>the result of the interpretation of the given instruction.</returns>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract V UnaryOperation(AbstractInsnNode insn, V value);

	    /// <summary>Interprets a bytecode instruction with two arguments.</summary>
	    /// <remarks>
	    ///     Interprets a bytecode instruction with two arguments. This method is called for the following
	    ///     opcodes:
	    ///     <p>
	    ///         IALOAD, LALOAD, FALOAD, DALOAD, AALOAD, BALOAD, CALOAD, SALOAD, IADD, LADD, FADD, DADD,
	    ///         ISUB, LSUB, FSUB, DSUB, IMUL, LMUL, FMUL, DMUL, IDIV, LDIV, FDIV, DDIV, IREM, LREM, FREM, DREM,
	    ///         ISHL, LSHL, ISHR, LSHR, IUSHR, LUSHR, IAND, LAND, IOR, LOR, IXOR, LXOR, LCMP, FCMPL, FCMPG,
	    ///         DCMPL, DCMPG, IF_ICMPEQ, IF_ICMPNE, IF_ICMPLT, IF_ICMPGE, IF_ICMPGT, IF_ICMPLE, IF_ACMPEQ,
	    ///         IF_ACMPNE, PUTFIELD
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <param name="value1">the first argument of the instruction to be interpreted.</param>
	    /// <param name="value2">the second argument of the instruction to be interpreted.</param>
	    /// <returns>the result of the interpretation of the given instruction.</returns>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract V BinaryOperation(AbstractInsnNode insn, V value1, V value2);

	    /// <summary>Interprets a bytecode instruction with three arguments.</summary>
	    /// <remarks>
	    ///     Interprets a bytecode instruction with three arguments. This method is called for the following
	    ///     opcodes:
	    ///     <p>IASTORE, LASTORE, FASTORE, DASTORE, AASTORE, BASTORE, CASTORE, SASTORE
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <param name="value1">the first argument of the instruction to be interpreted.</param>
	    /// <param name="value2">the second argument of the instruction to be interpreted.</param>
	    /// <param name="value3">the third argument of the instruction to be interpreted.</param>
	    /// <returns>the result of the interpretation of the given instruction.</returns>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract V TernaryOperation(AbstractInsnNode insn, V value1, V value2, V value3
        );

	    /// <summary>Interprets a bytecode instruction with a variable number of arguments.</summary>
	    /// <remarks>
	    ///     Interprets a bytecode instruction with a variable number of arguments. This method is called
	    ///     for the following opcodes:
	    ///     <p>
	    ///         INVOKEVIRTUAL, INVOKESPECIAL, INVOKESTATIC, INVOKEINTERFACE, MULTIANEWARRAY and
	    ///         INVOKEDYNAMIC
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <param name="values">the arguments of the instruction to be interpreted.</param>
	    /// <returns>the result of the interpretation of the given instruction.</returns>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract V NaryOperation<_T0>(AbstractInsnNode insn, IList<_T0> values)
            where _T0 : V;

	    /// <summary>Interprets a bytecode return instruction.</summary>
	    /// <remarks>
	    ///     Interprets a bytecode return instruction. This method is called for the following opcodes:
	    ///     <p>IRETURN, LRETURN, FRETURN, DRETURN, ARETURN
	    /// </remarks>
	    /// <param name="insn">the bytecode instruction to be interpreted.</param>
	    /// <param name="value">the argument of the instruction to be interpreted.</param>
	    /// <param name="expected">the expected return type of the analyzed method.</param>
	    /// <exception cref="AnalyzerException">
	    ///     if an error occurred during the interpretation.
	    /// </exception>
	    /// <exception cref="AnalyzerException" />
	    public abstract void ReturnOperation(AbstractInsnNode insn, V value, V expected);

	    /// <summary>Merges two values.</summary>
	    /// <remarks>
	    ///     Merges two values. The merge operation must return a value that represents both values (for
	    ///     instance, if the two values are two types, the merged value must be a common super type of the
	    ///     two types. If the two values are integer intervals, the merged value must be an interval that
	    ///     contains the previous ones. Likewise for other types of values).
	    /// </remarks>
	    /// <param name="value1">a value.</param>
	    /// <param name="value2">another value.</param>
	    /// <returns>
	    ///     the merged value. If the merged value is equal to
	    ///     <paramref name="value1" />
	    ///     , this method
	    ///     <i>must</i> return
	    ///     <paramref name="value1" />
	    ///     .
	    /// </returns>
	    public abstract V Merge(V value1, V value2);
    }
}