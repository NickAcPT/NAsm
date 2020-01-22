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

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     with convenient methods to generate code. For example, using this
    ///     adapter, the class below
    ///     <pre>
    ///         public class Example {
    ///         public static void main(String[] args) {
    ///         System.out.println(&quot;Hello world!&quot;);
    ///         }
    ///         }
    ///     </pre>
    ///     <p>
    ///         can be generated as follows:
    ///         <pre>
    ///             ClassWriter cw = new ClassWriter(0);
    ///             cw.visit(V1_1, ACC_PUBLIC, &quot;Example&quot;, null, &quot;java/lang/Object&quot;, null);
    ///             Method m = Method.getMethod(&quot;void &lt;init&gt; ()&quot;);
    ///             GeneratorAdapter mg = new GeneratorAdapter(ACC_PUBLIC, m, null, null, cw);
    ///             mg.loadThis();
    ///             mg.invokeConstructor(Type.getType(Object.class), m);
    ///             mg.returnValue();
    ///             mg.endMethod();
    ///             m = Method.getMethod(&quot;void main (String[])&quot;);
    ///             mg = new GeneratorAdapter(ACC_PUBLIC + ACC_STATIC, m, null, null, cw);
    ///             mg.getStatic(Type.getType(System.class), &quot;out&quot;, Type.getType(PrintStream.class));
    ///             mg.push(&quot;Hello world!&quot;);
    ///             mg.invokeVirtual(Type.getType(PrintStream.class),
    ///             Method.getMethod(&quot;void println (String)&quot;));
    ///             mg.returnValue();
    ///             mg.endMethod();
    ///             cw.visitEnd();
    ///         </pre>
    /// </summary>
    /// <author>Juozas Baliuka</author>
    /// <author>Chris Nokleberg</author>
    /// <author>Eric Bruneton</author>
    /// <author>Prashant Deva</author>
    public class GeneratorAdapter : LocalVariablesSorter
    {
        private const string Class_Descriptor = "Ljava/lang/Class;";

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Add = OpcodesConstants.Iadd;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Sub = OpcodesConstants.Isub;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Mul = OpcodesConstants.Imul;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Div = OpcodesConstants.Idiv;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Rem = OpcodesConstants.Irem;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Neg = OpcodesConstants.Ineg;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Shl = OpcodesConstants.Ishl;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Shr = OpcodesConstants.Ishr;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Ushr = OpcodesConstants.Iushr;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int And = OpcodesConstants.Iand;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Or = OpcodesConstants.Ior;

        /// <summary>
        ///     Constant for the
        ///     <see cref="Math" />
        ///     method.
        /// </summary>
        public const int Xor = OpcodesConstants.Ixor;

        /// <summary>
        ///     Constant for the
        ///     <see cref="IfCmp" />
        ///     method.
        /// </summary>
        public const int Eq = OpcodesConstants.Ifeq;

        /// <summary>
        ///     Constant for the
        ///     <see cref="IfCmp" />
        ///     method.
        /// </summary>
        public const int Ne = OpcodesConstants.Ifne;

        /// <summary>
        ///     Constant for the
        ///     <see cref="IfCmp" />
        ///     method.
        /// </summary>
        public const int Lt = OpcodesConstants.Iflt;

        /// <summary>
        ///     Constant for the
        ///     <see cref="IfCmp" />
        ///     method.
        /// </summary>
        public const int Ge = OpcodesConstants.Ifge;

        /// <summary>
        ///     Constant for the
        ///     <see cref="IfCmp" />
        ///     method.
        /// </summary>
        public const int Gt = OpcodesConstants.Ifgt;

        /// <summary>
        ///     Constant for the
        ///     <see cref="IfCmp" />
        ///     method.
        /// </summary>
        public const int Le = OpcodesConstants.Ifle;

        private static readonly Type Byte_Type = Type.GetObjectType("java/lang/Byte");

        private static readonly Type Boolean_Type = Type.GetObjectType("java/lang/Boolean"
        );

        private static readonly Type Short_Type = Type.GetObjectType("java/lang/Short");

        private static readonly Type Character_Type = Type.GetObjectType("java/lang/Character"
        );

        private static readonly Type Integer_Type = Type.GetObjectType("java/lang/Integer"
        );

        private static readonly Type Float_Type = Type.GetObjectType("java/lang/Float");

        private static readonly Type Long_Type = Type.GetObjectType("java/lang/Long");

        private static readonly Type Double_Type = Type.GetObjectType("java/lang/Double");

        private static readonly Type Number_Type = Type.GetObjectType("java/lang/Number");

        private static readonly Type Object_Type = Type.GetObjectType("java/lang/Object");

        private static readonly Method Boolean_Value = Method.GetMethod("boolean booleanValue()"
        );

        private static readonly Method Char_Value = Method.GetMethod("char charValue()");

        private static readonly Method Int_Value = Method.GetMethod("int intValue()");

        private static readonly Method Float_Value = Method.GetMethod("float floatValue()"
        );

        private static readonly Method Long_Value = Method.GetMethod("long longValue()");

        private static readonly Method Double_Value = Method.GetMethod("double doubleValue()"
        );

        /// <summary>The access flags of the visited method.</summary>
        private readonly AccessFlags access;

        /// <summary>The argument types of the visited method.</summary>
        private readonly Type[] argumentTypes;

        /// <summary>The types of the local variables of the visited method.</summary>
        private readonly IList<Type> localTypes = new List<Type>();

        /// <summary>The name of the visited method.</summary>
        private readonly string name;

        /// <summary>The return type of the visited method.</summary>
        private readonly Type returnType;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="GeneratorAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="GeneratorAdapter(int, MethodVisitor, int, string, string)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     if a subclass calls this constructor.
        /// </exception>
        public GeneratorAdapter(MethodVisitor methodVisitor, AccessFlags access, string name, string
            descriptor)
            : this(VisitorAsmApiVersion.Asm7, methodVisitor, access, name, descriptor)
        {
            /* latest api = */
            if (GetType() != typeof(GeneratorAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="GeneratorAdapter" />
        ///     .
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of
        ///     <see cref="Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Opcodes.Asm6" />
        ///     or
        ///     <see cref="Opcodes.Asm7" />
        ///     .
        /// </param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        /// <param name="access">
        ///     the method's access flags (see
        ///     <see cref="Opcodes" />
        ///     ).
        /// </param>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Org.Objectweb.Asm.Type" />
        ///     ).
        /// </param>
        protected internal GeneratorAdapter(VisitorAsmApiVersion api, MethodVisitor methodVisitor, AccessFlags access
            , string name, string descriptor)
            : base(api, access, descriptor, methodVisitor)
        {
            this.access = access;
            this.name = name;
            returnType = Type.GetReturnType(descriptor);
            argumentTypes = Type.GetArgumentTypes(descriptor);
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="GeneratorAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="GeneratorAdapter(int, MethodVisitor, int, string, string)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="access">access flags of the adapted method.</param>
        /// <param name="method">the adapted method.</param>
        /// <param name="methodVisitor">
        ///     the method visitor to which this adapter delegates calls.
        /// </param>
        public GeneratorAdapter(AccessFlags access, Method method, MethodVisitor methodVisitor)
            : this(methodVisitor, access, method.GetName(), method.GetDescriptor())
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="GeneratorAdapter" />
        ///     . <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the
        ///     <see cref="GeneratorAdapter(int, MethodVisitor, int, string, string)
        /// 	" />
        ///     version.
        /// </summary>
        /// <param name="access">access flags of the adapted method.</param>
        /// <param name="method">the adapted method.</param>
        /// <param name="signature">
        ///     the signature of the adapted method (may be
        ///     <literal>null</literal>
        ///     ).
        /// </param>
        /// <param name="exceptions">
        ///     the exceptions thrown by the adapted method (may be
        ///     <literal>null</literal>
        ///     ).
        /// </param>
        /// <param name="classVisitor">
        ///     the class visitor to which this adapter delegates calls.
        /// </param>
        public GeneratorAdapter(AccessFlags access, Method method, string signature, Type[] exceptions
            , ClassVisitor classVisitor)
            : this(access, method, classVisitor.VisitMethod(access, method.GetName(), method.GetDescriptor(), signature,
                exceptions == null
                    ? null
                    : GetInternalNames(exceptions
                    )))
        {
        }

        /// <summary>Returns the internal names of the given types.</summary>
        /// <param name="types">a set of types.</param>
        /// <returns>the internal names of the given types.</returns>
        private static string[] GetInternalNames(Type[] types)
        {
            var names = new string[types.Length];
            for (var i = 0; i < names.Length; ++i) names[i] = types[i].GetInternalName();
            return names;
        }

        public virtual AccessFlags Access
        {
            get { return access; }
        }

        public virtual string Name
        {
            get { return name; }
        }

        public virtual Type ReturnType
        {
            get { return returnType; }
        }

        public virtual Type[] ArgumentTypes
        {
            get { return (Type[]) argumentTypes.Clone(); }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to push constants on the stack
        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">the value to be pushed on the stack.</param>
        public virtual void Push(bool value)
        {
            Push(value ? 1 : 0);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">the value to be pushed on the stack.</param>
        public virtual void Push(int value)
        {
            if (value >= -1 && value <= 5)
                mv.VisitInsn(OpcodesConstants.Iconst_0 + value);
            else if (value >= byte.MinValue && value <= byte.MaxValue)
                mv.VisitIntInsn(OpcodesConstants.Bipush, value);
            else if (value >= short.MinValue && value <= short.MaxValue)
                mv.VisitIntInsn(OpcodesConstants.Sipush, value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">the value to be pushed on the stack.</param>
        public virtual void Push(long value)
        {
            if (value == 0L || value == 1L)
                mv.VisitInsn(OpcodesConstants.Lconst_0 + (int) value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">the value to be pushed on the stack.</param>
        public virtual void Push(float value)
        {
            var bits = Runtime.FloatToIntBits(value);
            if (bits == 0L || bits == 0x3F800000 || bits == 0x40000000)
                // 0..2
                mv.VisitInsn(OpcodesConstants.Fconst_0 + (int) value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">the value to be pushed on the stack.</param>
        public virtual void Push(double value)
        {
            var bits = BitConverter.DoubleToInt64Bits(value);
            if (bits == 0L || bits == 0x3FF0000000000000L)
                // +0.0d and 1.0d
                mv.VisitInsn(OpcodesConstants.Dconst_0 + (int) value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">
        ///     the value to be pushed on the stack. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        public virtual void Push(string value)
        {
            if (value == null)
                mv.VisitInsn(OpcodesConstants.Aconst_Null);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>Generates the instruction to push the given value on the stack.</summary>
        /// <param name="value">the value to be pushed on the stack.</param>
        public virtual void Push(Type value)
        {
            if (value == null)
                mv.VisitInsn(OpcodesConstants.Aconst_Null);
            else
                switch (value.GetSort())
                {
                    case Type.Boolean:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Boolean", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Char:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Character", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Byte:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Byte", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Short:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Short", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Int:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Integer", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Float:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Float", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Long:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Long", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    case Type.Double:
                    {
                        mv.VisitFieldInsn(OpcodesConstants.Getstatic, "java/lang/Double", "TYPE", Class_Descriptor
                        );
                        break;
                    }

                    default:
                    {
                        mv.VisitLdcInsn(value);
                        break;
                    }
                }
        }

        /// <summary>Generates the instruction to push a handle on the stack.</summary>
        /// <param name="handle">the handle to be pushed on the stack.</param>
        public virtual void Push(Handle handle)
        {
            if (handle == null)
                mv.VisitInsn(OpcodesConstants.Aconst_Null);
            else
                mv.VisitLdcInsn(handle);
        }

        /// <summary>Generates the instruction to push a constant dynamic on the stack.</summary>
        /// <param name="constantDynamic">the constant dynamic to be pushed on the stack.</param>
        public virtual void Push(ConstantDynamic constantDynamic)
        {
            if (constantDynamic == null)
                mv.VisitInsn(OpcodesConstants.Aconst_Null);
            else
                mv.VisitLdcInsn(constantDynamic);
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to load and store method arguments
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the index of the given method argument in the frame's local variables array.
        /// </summary>
        /// <param name="arg">the index of a method argument.</param>
        /// <returns>
        ///     the index of the given method argument in the frame's local variables array.
        /// </returns>
        private int GetArgIndex(int arg)
        {
            var index = access.HasNotFlagFast(AccessFlags.Static) ? 1 : 0;
            for (var i = 0; i < arg; i++) index += argumentTypes[i].GetSize();
            return index;
        }

        /// <summary>Generates the instruction to push a local variable on the stack.</summary>
        /// <param name="type">the type of the local variable to be loaded.</param>
        /// <param name="index">an index in the frame's local variables array.</param>
        private void LoadInsn(Type type, int index)
        {
            mv.VisitVarInsn(type.GetOpcode(OpcodesConstants.Iload), index);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in a local variable.
        /// </summary>
        /// <param name="type">the type of the local variable to be stored.</param>
        /// <param name="index">an index in the frame's local variables array.</param>
        private void StoreInsn(Type type, int index)
        {
            mv.VisitVarInsn(type.GetOpcode(OpcodesConstants.Istore), index);
        }

        /// <summary>Generates the instruction to load 'this' on the stack.</summary>
        public virtual void LoadThis()
        {
            if (access.HasFlagFast(AccessFlags.Static))
                throw new InvalidOperationException("no 'this' pointer within static method");
            mv.VisitVarInsn(OpcodesConstants.Aload, 0);
        }

        /// <summary>
        ///     Generates the instruction to load the given method argument on the stack.
        /// </summary>
        /// <param name="arg">the index of a method argument.</param>
        public virtual void LoadArg(int arg)
        {
            LoadInsn(argumentTypes[arg], GetArgIndex(arg));
        }

        /// <summary>
        ///     Generates the instructions to load the given method arguments on the stack.
        /// </summary>
        /// <param name="arg">the index of the first method argument to be loaded.</param>
        /// <param name="count">the number of method arguments to be loaded.</param>
        public virtual void LoadArgs(int arg, int count)
        {
            var index = GetArgIndex(arg);
            for (var i = 0; i < count; ++i)
            {
                var argumentType = argumentTypes[arg + i];
                LoadInsn(argumentType, index);
                index += argumentType.GetSize();
            }
        }

        /// <summary>
        ///     Generates the instructions to load all the method arguments on the stack.
        /// </summary>
        public virtual void LoadArgs()
        {
            LoadArgs(0, argumentTypes.Length);
        }

        /// <summary>
        ///     Generates the instructions to load all the method arguments on the stack, as a single object
        ///     array.
        /// </summary>
        public virtual void LoadArgArray()
        {
            Push(argumentTypes.Length);
            NewArray(Object_Type);
            for (var i = 0; i < argumentTypes.Length; i++)
            {
                Dup();
                Push(i);
                LoadArg(i);
                Box(argumentTypes[i]);
                ArrayStore(Object_Type);
            }
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in the given method argument.
        /// </summary>
        /// <param name="arg">the index of a method argument.</param>
        public virtual void StoreArg(int arg)
        {
            StoreInsn(argumentTypes[arg], GetArgIndex(arg));
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to load and store local variables
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns the type of the given local variable.</summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="LocalVariablesSorter.NewLocal" />
        ///     .
        /// </param>
        /// <returns>the type of the given local variable.</returns>
        public virtual Type GetLocalType(int local)
        {
            return localTypes[local - firstLocal];
        }

        protected internal override void SetLocalType(int local, Type type)
        {
            var index = local - firstLocal;
            while (localTypes.Count < index + 1) localTypes.Add(null);
            localTypes[index] = type;
        }

        /// <summary>
        ///     Generates the instruction to load the given local variable on the stack.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="LocalVariablesSorter.NewLocal" />
        ///     .
        /// </param>
        public virtual void LoadLocal(int local)
        {
            LoadInsn(GetLocalType(local), local);
        }

        /// <summary>
        ///     Generates the instruction to load the given local variable on the stack.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="LocalVariablesSorter.NewLocal" />
        ///     .
        /// </param>
        /// <param name="type">the type of this local variable.</param>
        public virtual void LoadLocal(int local, Type type)
        {
            SetLocalType(local, type);
            LoadInsn(type, local);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in the given local variable.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="LocalVariablesSorter.NewLocal" />
        ///     .
        /// </param>
        public virtual void StoreLocal(int local)
        {
            StoreInsn(GetLocalType(local), local);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in the given local variable.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="LocalVariablesSorter.NewLocal" />
        ///     .
        /// </param>
        /// <param name="type">the type of this local variable.</param>
        public virtual void StoreLocal(int local, Type type)
        {
            SetLocalType(local, type);
            StoreInsn(type, local);
        }

        /// <summary>Generates the instruction to load an element from an array.</summary>
        /// <param name="type">the type of the array element to be loaded.</param>
        public virtual void ArrayLoad(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iaload));
        }

        /// <summary>Generates the instruction to store an element in an array.</summary>
        /// <param name="type">the type of the array element to be stored.</param>
        public virtual void ArrayStore(Type type)
        {
            mv.VisitInsn(type.GetOpcode(OpcodesConstants.Iastore));
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to manage the stack
        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates a POP instruction.</summary>
        public virtual void Pop()
        {
            mv.VisitInsn(OpcodesConstants.Pop);
        }

        /// <summary>Generates a POP2 instruction.</summary>
        public virtual void Pop2()
        {
            mv.VisitInsn(OpcodesConstants.Pop2);
        }

        /// <summary>Generates a DUP instruction.</summary>
        public virtual void Dup()
        {
            mv.VisitInsn(OpcodesConstants.Dup);
        }

        /// <summary>Generates a DUP2 instruction.</summary>
        public virtual void Dup2()
        {
            mv.VisitInsn(OpcodesConstants.Dup2);
        }

        /// <summary>Generates a DUP_X1 instruction.</summary>
        public virtual void DupX1()
        {
            mv.VisitInsn(OpcodesConstants.Dup_X1);
        }

        /// <summary>Generates a DUP_X2 instruction.</summary>
        public virtual void DupX2()
        {
            mv.VisitInsn(OpcodesConstants.Dup_X2);
        }

        /// <summary>Generates a DUP2_X1 instruction.</summary>
        public virtual void Dup2X1()
        {
            mv.VisitInsn(OpcodesConstants.Dup2_X1);
        }

        /// <summary>Generates a DUP2_X2 instruction.</summary>
        public virtual void Dup2X2()
        {
            mv.VisitInsn(OpcodesConstants.Dup2_X2);
        }

        /// <summary>Generates a SWAP instruction.</summary>
        public virtual void Swap()
        {
            mv.VisitInsn(OpcodesConstants.Swap);
        }

        /// <summary>Generates the instructions to swap the top two stack values.</summary>
        /// <param name="prev">type of the top - 1 stack value.</param>
        /// <param name="type">type of the top stack value.</param>
        public virtual void Swap(Type prev, Type type)
        {
            if (type.GetSize() == 1)
            {
                if (prev.GetSize() == 1)
                {
                    Swap();
                }
                else
                {
                    // Same as dupX1 pop.
                    DupX2();
                    Pop();
                }
            }
            else if (prev.GetSize() == 1)
            {
                Dup2X1();
                Pop2();
            }
            else
            {
                Dup2X2();
                Pop2();
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to do mathematical and logical operations
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generates the instruction to do the specified mathematical or logical operation.
        /// </summary>
        /// <param name="op">
        ///     a mathematical or logical operation. Must be one of ADD, SUB, MUL, DIV, REM, NEG,
        ///     SHL, SHR, USHR, AND, OR, XOR.
        /// </param>
        /// <param name="type">the type of the operand(s) for this operation.</param>
        public virtual void Math(int op, Type type)
        {
            mv.VisitInsn(type.GetOpcode(op));
        }

        /// <summary>
        ///     Generates the instructions to compute the bitwise negation of the top stack value.
        /// </summary>
        public virtual void Not()
        {
            mv.VisitInsn(OpcodesConstants.Iconst_1);
            mv.VisitInsn(OpcodesConstants.Ixor);
        }

        /// <summary>Generates the instruction to increment the given local variable.</summary>
        /// <param name="local">the local variable to be incremented.</param>
        /// <param name="amount">the amount by which the local variable must be incremented.</param>
        public virtual void Iinc(int local, int amount)
        {
            mv.VisitIincInsn(local, amount);
        }

        /// <summary>
        ///     Generates the instructions to cast a numerical value from one type to another.
        /// </summary>
        /// <param name="from">the type of the top stack value</param>
        /// <param name="to">the type into which this value must be cast.</param>
        public virtual void Cast(Type from, Type to)
        {
            if (from != to)
            {
                if (from.GetSort() < Type.Boolean || from.GetSort() > Type.Double || to.GetSort()
                    < Type.Boolean || to.GetSort() > Type.Double)
                    throw new ArgumentException("Cannot cast from " + from + " to " + to);
                InstructionAdapter.Cast(mv, from, to);
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to do boxing and unboxing operations
        // -----------------------------------------------------------------------------------------------
        private static Type GetBoxedType(Type type)
        {
            switch (type.GetSort())
            {
                case Type.Byte:
                {
                    return Byte_Type;
                }

                case Type.Boolean:
                {
                    return Boolean_Type;
                }

                case Type.Short:
                {
                    return Short_Type;
                }

                case Type.Char:
                {
                    return Character_Type;
                }

                case Type.Int:
                {
                    return Integer_Type;
                }

                case Type.Float:
                {
                    return Float_Type;
                }

                case Type.Long:
                {
                    return Long_Type;
                }

                case Type.Double:
                {
                    return Double_Type;
                }

                default:
                {
                    return type;
                }
            }
        }

        /// <summary>Generates the instructions to box the top stack value.</summary>
        /// <remarks>
        ///     Generates the instructions to box the top stack value. This value is replaced by its boxed
        ///     equivalent on top of the stack.
        /// </remarks>
        /// <param name="type">the type of the top stack value.</param>
        public virtual void Box(Type type)
        {
            if (type.GetSort() == Type.Object || type.GetSort() == Type.Array) return;
            if (type == Type.Void_Type)
            {
                Push((string) null);
            }
            else
            {
                var boxedType = GetBoxedType(type);
                NewInstance(boxedType);
                if (type.GetSize() == 2)
                {
                    // Pp -> Ppo -> oPpo -> ooPpo -> ooPp -> o
                    DupX2();
                    DupX2();
                    Pop();
                }
                else
                {
                    // p -> po -> opo -> oop -> o
                    DupX1();
                    Swap();
                }

                InvokeConstructor(boxedType, new Method("<init>", Type.Void_Type, new[]
                {
                    type
                }));
            }
        }

        /// <summary>
        ///     Generates the instructions to box the top stack value using Java 5's valueOf() method.
        /// </summary>
        /// <remarks>
        ///     Generates the instructions to box the top stack value using Java 5's valueOf() method. This
        ///     value is replaced by its boxed equivalent on top of the stack.
        /// </remarks>
        /// <param name="type">the type of the top stack value.</param>
        public virtual void ValueOf(Type type)
        {
            if (type.GetSort() == Type.Object || type.GetSort() == Type.Array) return;
            if (type == Type.Void_Type)
            {
                Push((string) null);
            }
            else
            {
                var boxedType = GetBoxedType(type);
                InvokeStatic(boxedType, new Method("valueOf", boxedType, new[] {type}));
            }
        }

        /// <summary>Generates the instructions to unbox the top stack value.</summary>
        /// <remarks>
        ///     Generates the instructions to unbox the top stack value. This value is replaced by its unboxed
        ///     equivalent on top of the stack.
        /// </remarks>
        /// <param name="type">the type of the top stack value.</param>
        public virtual void Unbox(Type type)
        {
            var boxedType = Number_Type;
            Method unboxMethod;
            switch (type.GetSort())
            {
                case Type.Void:
                {
                    return;
                }

                case Type.Char:
                {
                    boxedType = Character_Type;
                    unboxMethod = Char_Value;
                    break;
                }

                case Type.Boolean:
                {
                    boxedType = Boolean_Type;
                    unboxMethod = Boolean_Value;
                    break;
                }

                case Type.Double:
                {
                    unboxMethod = Double_Value;
                    break;
                }

                case Type.Float:
                {
                    unboxMethod = Float_Value;
                    break;
                }

                case Type.Long:
                {
                    unboxMethod = Long_Value;
                    break;
                }

                case Type.Int:
                case Type.Short:
                case Type.Byte:
                {
                    unboxMethod = Int_Value;
                    break;
                }

                default:
                {
                    unboxMethod = null;
                    break;
                }
            }

            if (unboxMethod == null)
            {
                CheckCast(type);
            }
            else
            {
                CheckCast(boxedType);
                InvokeVirtual(boxedType, unboxMethod);
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to jump to other instructions
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Constructs a new
        ///     <see cref="Label" />
        ///     .
        /// </summary>
        /// <returns>
        ///     a new
        ///     <see cref="Label" />
        ///     .
        /// </returns>
        public virtual Label NewLabel()
        {
            return new Label();
        }

        /// <summary>Marks the current code position with the given label.</summary>
        /// <param name="label">a label.</param>
        public virtual void Mark(Label label)
        {
            mv.VisitLabel(label);
        }

        /// <summary>Marks the current code position with a new label.</summary>
        /// <returns>the label that was created to mark the current code position.</returns>
        public virtual Label Mark()
        {
            var label = new Label();
            mv.VisitLabel(label);
            return label;
        }

        /// <summary>
        ///     Generates the instructions to jump to a label based on the comparison of the top two stack
        ///     values.
        /// </summary>
        /// <param name="type">the type of the top two stack values.</param>
        /// <param name="mode">
        ///     how these values must be compared. One of EQ, NE, LT, GE, GT, LE.
        /// </param>
        /// <param name="label">
        ///     where to jump if the comparison result is
        ///     <literal>true</literal>
        ///     .
        /// </param>
        public virtual void IfCmp(Type type, int mode, Label label)
        {
            switch (type.GetSort())
            {
                case Type.Long:
                {
                    mv.VisitInsn(OpcodesConstants.Lcmp);
                    break;
                }

                case Type.Double:
                {
                    mv.VisitInsn(mode == Ge || mode == Gt
                        ? OpcodesConstants.Dcmpl
                        : OpcodesConstants
                            .Dcmpg);
                    break;
                }

                case Type.Float:
                {
                    mv.VisitInsn(mode == Ge || mode == Gt
                        ? OpcodesConstants.Fcmpl
                        : OpcodesConstants
                            .Fcmpg);
                    break;
                }

                case Type.Array:
                case Type.Object:
                {
                    if (mode == Eq)
                    {
                        mv.VisitJumpInsn(OpcodesConstants.If_Acmpeq, label);
                        return;
                    }

                    if (mode == Ne)
                    {
                        mv.VisitJumpInsn(OpcodesConstants.If_Acmpne, label);
                        return;
                    }

                    throw new ArgumentException("Bad comparison for type " + type);
                    goto default;
                }

                default:
                {
                    var intOp = -1;
                    switch (mode)
                    {
                        case Eq:
                        {
                            intOp = OpcodesConstants.If_Icmpeq;
                            break;
                        }

                        case Ne:
                        {
                            intOp = OpcodesConstants.If_Icmpne;
                            break;
                        }

                        case Ge:
                        {
                            intOp = OpcodesConstants.If_Icmpge;
                            break;
                        }

                        case Lt:
                        {
                            intOp = OpcodesConstants.If_Icmplt;
                            break;
                        }

                        case Le:
                        {
                            intOp = OpcodesConstants.If_Icmple;
                            break;
                        }

                        case Gt:
                        {
                            intOp = OpcodesConstants.If_Icmpgt;
                            break;
                        }

                        default:
                        {
                            throw new ArgumentException("Bad comparison mode " + mode);
                        }
                    }

                    mv.VisitJumpInsn(intOp, label);
                    return;
                }
            }

            mv.VisitJumpInsn(mode, label);
        }

        /// <summary>
        ///     Generates the instructions to jump to a label based on the comparison of the top two integer
        ///     stack values.
        /// </summary>
        /// <param name="mode">
        ///     how these values must be compared. One of EQ, NE, LT, GE, GT, LE.
        /// </param>
        /// <param name="label">
        ///     where to jump if the comparison result is
        ///     <literal>true</literal>
        ///     .
        /// </param>
        public virtual void IfICmp(int mode, Label label)
        {
            IfCmp(Type.Int_Type, mode, label);
        }

        /// <summary>
        ///     Generates the instructions to jump to a label based on the comparison of the top integer stack
        ///     value with zero.
        /// </summary>
        /// <param name="mode">
        ///     how these values must be compared. One of EQ, NE, LT, GE, GT, LE.
        /// </param>
        /// <param name="label">
        ///     where to jump if the comparison result is
        ///     <literal>true</literal>
        ///     .
        /// </param>
        public virtual void IfZCmp(int mode, Label label)
        {
            mv.VisitJumpInsn(mode, label);
        }

        /// <summary>
        ///     Generates the instruction to jump to the given label if the top stack value is null.
        /// </summary>
        /// <param name="label">
        ///     where to jump if the condition is
        ///     <literal>true</literal>
        ///     .
        /// </param>
        public virtual void IfNull(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifnull, label);
        }

        /// <summary>
        ///     Generates the instruction to jump to the given label if the top stack value is not null.
        /// </summary>
        /// <param name="label">
        ///     where to jump if the condition is
        ///     <literal>true</literal>
        ///     .
        /// </param>
        public virtual void IfNonNull(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Ifnonnull, label);
        }

        /// <summary>Generates the instruction to jump to the given label.</summary>
        /// <param name="label">
        ///     where to jump if the condition is
        ///     <literal>true</literal>
        ///     .
        /// </param>
        public virtual void GoTo(Label label)
        {
            mv.VisitJumpInsn(OpcodesConstants.Goto, label);
        }

        /// <summary>Generates a RET instruction.</summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by
        ///     <see cref="LocalVariablesSorter.NewLocal" />
        ///     .
        /// </param>
        public virtual void Ret(int local)
        {
            mv.VisitVarInsn(OpcodesConstants.Ret, local);
        }

        /// <summary>Generates the instructions for a switch statement.</summary>
        /// <param name="keys">the switch case keys.</param>
        /// <param name="generator">a generator to generate the code for the switch cases.</param>
        public virtual void TableSwitch(int[] keys, TableSwitchGenerator generator)
        {
            float density;
            if (keys.Length == 0)
                density = 0;
            else
                density = (float) keys.Length / (keys[keys.Length - 1] - keys[0] + 1);
            TableSwitch(keys, generator, density >= 0.5f);
        }

        /// <summary>Generates the instructions for a switch statement.</summary>
        /// <param name="keys">the switch case keys.</param>
        /// <param name="generator">a generator to generate the code for the switch cases.</param>
        /// <param name="useTable">
        ///     <literal>true</literal>
        ///     to use a TABLESWITCH instruction, or
        ///     <literal>false</literal>
        ///     to use a
        ///     LOOKUPSWITCH instruction.
        /// </param>
        public virtual void TableSwitch(int[] keys, TableSwitchGenerator generator, bool
            useTable)
        {
            for (var i = 1; i < keys.Length; ++i)
                if (keys[i] < keys[i - 1])
                    throw new ArgumentException("keys must be sorted in ascending order");
            var defaultLabel = NewLabel();
            var endLabel = NewLabel();
            if (keys.Length > 0)
            {
                var numKeys = keys.Length;
                if (useTable)
                {
                    var min = keys[0];
                    var max = keys[numKeys - 1];
                    var range = max - min + 1;
                    var labels = new Label[range];
                    Arrays.Fill(labels, defaultLabel);
                    for (var i = 0; i < numKeys; ++i) labels[keys[i] - min] = NewLabel();
                    mv.VisitTableSwitchInsn(min, max, defaultLabel, labels);
                    for (var i = 0; i < range; ++i)
                    {
                        var label = labels[i];
                        if (label != defaultLabel)
                        {
                            Mark(label);
                            generator.GenerateCase(i + min, endLabel);
                        }
                    }
                }
                else
                {
                    var labels = new Label[numKeys];
                    for (var i = 0; i < numKeys; ++i) labels[i] = NewLabel();
                    mv.VisitLookupSwitchInsn(defaultLabel, keys, labels);
                    for (var i = 0; i < numKeys; ++i)
                    {
                        Mark(labels[i]);
                        generator.GenerateCase(keys[i], endLabel);
                    }
                }
            }

            Mark(defaultLabel);
            generator.GenerateDefault();
            Mark(endLabel);
        }

        /// <summary>Generates the instruction to return the top stack value to the caller.</summary>
        public virtual void ReturnValue()
        {
            mv.VisitInsn(returnType.GetOpcode(OpcodesConstants.Ireturn));
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to load and store fields
        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates a get field or set field instruction.</summary>
        /// <param name="opcode">the instruction's opcode.</param>
        /// <param name="ownerType">the class in which the field is defined.</param>
        /// <param name="name">the name of the field.</param>
        /// <param name="fieldType">the type of the field.</param>
        private void FieldInsn(int opcode, Type ownerType, string name, Type fieldType)
        {
            mv.VisitFieldInsn(opcode, ownerType.GetInternalName(), name, fieldType.GetDescriptor
                ());
        }

        /// <summary>
        ///     Generates the instruction to push the value of a static field on the stack.
        /// </summary>
        /// <param name="owner">the class in which the field is defined.</param>
        /// <param name="name">the name of the field.</param>
        /// <param name="type">the type of the field.</param>
        public virtual void GetStatic(Type owner, string name, Type type)
        {
            FieldInsn(OpcodesConstants.Getstatic, owner, name, type);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in a static field.
        /// </summary>
        /// <param name="owner">the class in which the field is defined.</param>
        /// <param name="name">the name of the field.</param>
        /// <param name="type">the type of the field.</param>
        public virtual void PutStatic(Type owner, string name, Type type)
        {
            FieldInsn(OpcodesConstants.Putstatic, owner, name, type);
        }

        /// <summary>
        ///     Generates the instruction to push the value of a non static field on the stack.
        /// </summary>
        /// <param name="owner">the class in which the field is defined.</param>
        /// <param name="name">the name of the field.</param>
        /// <param name="type">the type of the field.</param>
        public virtual void GetField(Type owner, string name, Type type)
        {
            FieldInsn(OpcodesConstants.Getfield, owner, name, type);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in a non static field.
        /// </summary>
        /// <param name="owner">the class in which the field is defined.</param>
        /// <param name="name">the name of the field.</param>
        /// <param name="type">the type of the field.</param>
        public virtual void PutField(Type owner, string name, Type type)
        {
            FieldInsn(OpcodesConstants.Putfield, owner, name, type);
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to invoke methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates an invoke method instruction.</summary>
        /// <param name="opcode">the instruction's opcode.</param>
        /// <param name="type">the class in which the method is defined.</param>
        /// <param name="method">the method to be invoked.</param>
        /// <param name="isInterface">whether the 'type' class is an interface or not.</param>
        private void InvokeInsn(int opcode, Type type, Method method, bool isInterface)
        {
            var owner = type.GetSort() == Type.Array
                ? type.GetDescriptor()
                : type.GetInternalName
                    ();
            mv.VisitMethodInsn(opcode, owner, method.GetName(), method.GetDescriptor(), isInterface
            );
        }

        /// <summary>Generates the instruction to invoke a normal method.</summary>
        /// <param name="owner">the class in which the method is defined.</param>
        /// <param name="method">the method to be invoked.</param>
        public virtual void InvokeVirtual(Type owner, Method method)
        {
            InvokeInsn(OpcodesConstants.Invokevirtual, owner, method, false);
        }

        /// <summary>Generates the instruction to invoke a constructor.</summary>
        /// <param name="type">the class in which the constructor is defined.</param>
        /// <param name="method">the constructor to be invoked.</param>
        public virtual void InvokeConstructor(Type type, Method method)
        {
            InvokeInsn(OpcodesConstants.Invokespecial, type, method, false);
        }

        /// <summary>Generates the instruction to invoke a static method.</summary>
        /// <param name="owner">the class in which the method is defined.</param>
        /// <param name="method">the method to be invoked.</param>
        public virtual void InvokeStatic(Type owner, Method method)
        {
            InvokeInsn(OpcodesConstants.Invokestatic, owner, method, false);
        }

        /// <summary>Generates the instruction to invoke an interface method.</summary>
        /// <param name="owner">the class in which the method is defined.</param>
        /// <param name="method">the method to be invoked.</param>
        public virtual void InvokeInterface(Type owner, Method method)
        {
            InvokeInsn(OpcodesConstants.Invokeinterface, owner, method, true);
        }

        /// <summary>Generates an invokedynamic instruction.</summary>
        /// <param name="name">the method's name.</param>
        /// <param name="descriptor">
        ///     the method's descriptor (see
        ///     <see cref="Type" />
        ///     ).
        /// </param>
        /// <param name="bootstrapMethodHandle">the bootstrap method.</param>
        /// <param name="bootstrapMethodArguments">
        ///     the bootstrap method constant arguments. Each argument must be
        ///     an
        ///     <see cref="int" />
        ///     ,
        ///     <see cref="float" />
        ///     ,
        ///     <see cref="long" />
        ///     ,
        ///     <see cref="double" />
        ///     ,
        ///     <see cref="string" />
        ///     ,
        ///     <see cref="Type" />
        ///     or
        ///     <see cref="Handle" />
        ///     value. This method is allowed to modify the content of the array so
        ///     a caller should expect that this array may change.
        /// </param>
        public virtual void InvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle
            , params object[] bootstrapMethodArguments)
        {
            mv.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to create objects and arrays
        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates a type dependent instruction.</summary>
        /// <param name="opcode">the instruction's opcode.</param>
        /// <param name="type">the instruction's operand.</param>
        private void TypeInsn(int opcode, Type type)
        {
            mv.VisitTypeInsn(opcode, type.GetInternalName());
        }

        /// <summary>Generates the instruction to create a new object.</summary>
        /// <param name="type">the class of the object to be created.</param>
        public virtual void NewInstance(Type type)
        {
            TypeInsn(OpcodesConstants.New, type);
        }

        /// <summary>Generates the instruction to create a new array.</summary>
        /// <param name="type">the type of the array elements.</param>
        public virtual void NewArray(Type type)
        {
            InstructionAdapter.Newarray(mv, type);
        }

        // -----------------------------------------------------------------------------------------------
        // Miscellaneous instructions
        // -----------------------------------------------------------------------------------------------
        /// <summary>Generates the instruction to compute the length of an array.</summary>
        public virtual void ArrayLength()
        {
            mv.VisitInsn(OpcodesConstants.Arraylength);
        }

        /// <summary>Generates the instruction to throw an exception.</summary>
        public virtual void ThrowException()
        {
            mv.VisitInsn(OpcodesConstants.Athrow);
        }

        /// <summary>Generates the instructions to create and throw an exception.</summary>
        /// <remarks>
        ///     Generates the instructions to create and throw an exception. The exception class must have a
        ///     constructor with a single String argument.
        /// </remarks>
        /// <param name="type">the class of the exception to be thrown.</param>
        /// <param name="message">the detailed message of the exception.</param>
        public virtual void ThrowException(Type type, string message)
        {
            NewInstance(type);
            Dup();
            Push(message);
            InvokeConstructor(type, Method.GetMethod("void <init> (String)"));
            ThrowException();
        }

        /// <summary>
        ///     Generates the instruction to check that the top stack value is of the given type.
        /// </summary>
        /// <param name="type">a class or interface type.</param>
        public virtual void CheckCast(Type type)
        {
            if (!type.Equals(Object_Type)) TypeInsn(OpcodesConstants.Checkcast, type);
        }

        /// <summary>
        ///     Generates the instruction to test if the top stack value is of the given type.
        /// </summary>
        /// <param name="type">a class or interface type.</param>
        public virtual void InstanceOf(Type type)
        {
            TypeInsn(OpcodesConstants.Instanceof, type);
        }

        /// <summary>Generates the instruction to get the monitor of the top stack value.</summary>
        public virtual void MonitorEnter()
        {
            mv.VisitInsn(OpcodesConstants.Monitorenter);
        }

        /// <summary>
        ///     Generates the instruction to release the monitor of the top stack value.
        /// </summary>
        public virtual void MonitorExit()
        {
            mv.VisitInsn(OpcodesConstants.Monitorexit);
        }

        // -----------------------------------------------------------------------------------------------
        // Non instructions
        // -----------------------------------------------------------------------------------------------
        /// <summary>Marks the end of the visited method.</summary>
        public virtual void EndMethod()
        {
            if (access.HasNotFlagFast(AccessFlags.Abstract)) mv.VisitMaxs(0, 0);
            mv.VisitEnd();
        }

        /// <summary>Marks the start of an exception handler.</summary>
        /// <param name="start">beginning of the exception handler's scope (inclusive).</param>
        /// <param name="end">end of the exception handler's scope (exclusive).</param>
        /// <param name="exception">
        ///     internal name of the type of exceptions handled by the handler.
        /// </param>
        public virtual void CatchException(Label start, Label end, Type exception)
        {
            var catchLabel = new Label();
            if (exception == null)
                mv.VisitTryCatchBlock(start, end, catchLabel, null);
            else
                mv.VisitTryCatchBlock(start, end, catchLabel, exception.GetInternalName());
            Mark(catchLabel);
        }
    }
}