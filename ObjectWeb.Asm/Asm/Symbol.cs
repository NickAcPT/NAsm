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

namespace ObjectWeb.Asm
{
	/// <summary>
	///     An entry of the constant pool, of the BootstrapMethods attribute, or of the (ASM specific) type
	///     table of a class.
	/// </summary>
	/// <seealso>
	///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.4">
	///         JVMS
	///         *     4.4
	///     </a>
	/// </seealso>
	/// <seealso>
	///     <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.23">
	///         JVMS
	///         *     4.7.23
	///     </a>
	/// </seealso>
	/// <author>Eric Bruneton</author>
	internal abstract class Symbol
    {
        /// <summary>The tag value of CONSTANT_Class_info JVMS structures.</summary>
        internal const int Constant_Class_Tag = 7;

        /// <summary>The tag value of CONSTANT_Fieldref_info JVMS structures.</summary>
        internal const int Constant_Fieldref_Tag = 9;

        /// <summary>The tag value of CONSTANT_Methodref_info JVMS structures.</summary>
        internal const int Constant_Methodref_Tag = 10;

        /// <summary>The tag value of CONSTANT_InterfaceMethodref_info JVMS structures.</summary>
        internal const int Constant_Interface_Methodref_Tag = 11;

        /// <summary>The tag value of CONSTANT_String_info JVMS structures.</summary>
        internal const int Constant_String_Tag = 8;

        /// <summary>The tag value of CONSTANT_Integer_info JVMS structures.</summary>
        internal const int Constant_Integer_Tag = 3;

        /// <summary>The tag value of CONSTANT_Float_info JVMS structures.</summary>
        internal const int Constant_Float_Tag = 4;

        /// <summary>The tag value of CONSTANT_Long_info JVMS structures.</summary>
        internal const int Constant_Long_Tag = 5;

        /// <summary>The tag value of CONSTANT_Double_info JVMS structures.</summary>
        internal const int Constant_Double_Tag = 6;

        /// <summary>The tag value of CONSTANT_NameAndType_info JVMS structures.</summary>
        internal const int Constant_Name_And_Type_Tag = 12;

        /// <summary>The tag value of CONSTANT_Utf8_info JVMS structures.</summary>
        internal const int Constant_Utf8_Tag = 1;

        /// <summary>The tag value of CONSTANT_MethodHandle_info JVMS structures.</summary>
        internal const int Constant_Method_Handle_Tag = 15;

        /// <summary>The tag value of CONSTANT_MethodType_info JVMS structures.</summary>
        internal const int Constant_Method_Type_Tag = 16;

        /// <summary>The tag value of CONSTANT_Dynamic_info JVMS structures.</summary>
        internal const int Constant_Dynamic_Tag = 17;

        /// <summary>The tag value of CONSTANT_InvokeDynamic_info JVMS structures.</summary>
        internal const int Constant_Invoke_Dynamic_Tag = 18;

        /// <summary>The tag value of CONSTANT_Module_info JVMS structures.</summary>
        internal const int Constant_Module_Tag = 19;

        /// <summary>The tag value of CONSTANT_Package_info JVMS structures.</summary>
        internal const int Constant_Package_Tag = 20;

        /// <summary>The tag value of the BootstrapMethods attribute entries.</summary>
        internal const int Bootstrap_Method_Tag = 64;

        /// <summary>
        ///     The tag value of a normal type entry in the (ASM specific) type table of a class.
        /// </summary>
        internal const int Type_Tag = 128;

        /// <summary>
        ///     The tag value of an
        ///     <see cref="Frame.Item_Uninitialized" />
        ///     type entry in the type table of a class.
        /// </summary>
        internal const int Uninitialized_Type_Tag = 129;

        /// <summary>
        ///     The tag value of a merged type entry in the (ASM specific) type table of a class.
        /// </summary>
        internal const int Merged_Type_Tag = 130;

        /// <summary>The numeric value of this symbol.</summary>
        /// <remarks>
        ///     The numeric value of this symbol. This is:
        ///     <ul>
        ///         <li>
        ///             the symbol's value for
        ///             <see cref="Constant_Integer_Tag" />
        ///             ,
        ///             <see cref="Constant_Float_Tag" />
        ///             ,
        ///             <see cref="Constant_Long_Tag" />
        ///             ,
        ///             <see cref="Constant_Double_Tag" />
        ///             ,
        ///             <li>
        ///                 the CONSTANT_MethodHandle_info reference_kind field value for
        ///                 <see cref="Constant_Method_Handle_Tag" />
        ///                 symbols,
        ///                 <li>
        ///                     the CONSTANT_InvokeDynamic_info bootstrap_method_attr_index field value for
        ///                     <see cref="Constant_Invoke_Dynamic_Tag" />
        ///                     symbols,
        ///                     <li>
        ///                         the offset of a bootstrap method in the BootstrapMethods boostrap_methods array, for
        ///                         <see cref="Constant_Dynamic_Tag" />
        ///                         or
        ///                         <see cref="Bootstrap_Method_Tag" />
        ///                         symbols,
        ///                         <li>
        ///                             the bytecode offset of the NEW instruction that created an
        ///                             <see cref="Frame.Item_Uninitialized" />
        ///                             type for
        ///                             <see cref="Uninitialized_Type_Tag" />
        ///                             symbols,
        ///                             <li>
        ///                                 the indices (in the class' type table) of two
        ///                                 <see cref="Type_Tag" />
        ///                                 source types for
        ///                                 <see cref="Merged_Type_Tag" />
        ///                                 symbols,
        ///                                 <li>0 for the other types of symbol.
        ///     </ul>
        /// </remarks>
        internal readonly long data;

        /// <summary>
        ///     The index of this symbol in the constant pool, in the BootstrapMethods attribute, or in the
        ///     (ASM specific) type table of a class (depending on the
        ///     <see cref="tag" />
        ///     value).
        /// </summary>
        internal readonly int index;

        /// <summary>The name of the class field or method corresponding to this symbol.</summary>
        /// <remarks>
        ///     The name of the class field or method corresponding to this symbol. Only used for
        ///     <see cref="Constant_Fieldref_Tag" />
        ///     ,
        ///     <see cref="Constant_Methodref_Tag" />
        ///     ,
        ///     <see cref="Constant_Interface_Methodref_Tag" />
        ///     ,
        ///     <see cref="Constant_Name_And_Type_Tag" />
        ///     ,
        ///     <see cref="Constant_Method_Handle_Tag" />
        ///     ,
        ///     <see cref="Constant_Dynamic_Tag" />
        ///     and
        ///     <see cref="Constant_Invoke_Dynamic_Tag" />
        ///     symbols.
        /// </remarks>
        internal readonly string name;

        /// <summary>The internal name of the owner class of this symbol.</summary>
        /// <remarks>
        ///     The internal name of the owner class of this symbol. Only used for
        ///     <see cref="Constant_Fieldref_Tag" />
        ///     ,
        ///     <see cref="Constant_Methodref_Tag" />
        ///     ,
        ///     <see cref="Constant_Interface_Methodref_Tag" />
        ///     , and
        ///     <see cref="Constant_Method_Handle_Tag" />
        ///     symbols.
        /// </remarks>
        internal readonly string owner;

        /// <summary>A tag indicating the type of this symbol.</summary>
        /// <remarks>
        ///     A tag indicating the type of this symbol. Must be one of the static tag values defined in this
        ///     class.
        /// </remarks>
        internal readonly int tag;

        /// <summary>The string value of this symbol.</summary>
        /// <remarks>
        ///     The string value of this symbol. This is:
        ///     <ul>
        ///         <li>
        ///             a field or method descriptor for
        ///             <see cref="Constant_Fieldref_Tag" />
        ///             ,
        ///             <see cref="Constant_Methodref_Tag" />
        ///             ,
        ///             <see cref="Constant_Interface_Methodref_Tag" />
        ///             ,
        ///             <see cref="Constant_Name_And_Type_Tag" />
        ///             ,
        ///             <see cref="Constant_Method_Handle_Tag" />
        ///             ,
        ///             <see cref="Constant_Method_Type_Tag" />
        ///             ,
        ///             <see cref="Constant_Dynamic_Tag" />
        ///             and
        ///             <see cref="Constant_Invoke_Dynamic_Tag" />
        ///             symbols,
        ///             <li>
        ///                 an arbitrary string for
        ///                 <see cref="Constant_Utf8_Tag" />
        ///                 and
        ///                 <see cref="Constant_String_Tag" />
        ///                 symbols,
        ///                 <li>
        ///                     an internal class name for
        ///                     <see cref="Constant_Class_Tag" />
        ///                     ,
        ///                     <see cref="Type_Tag" />
        ///                     and
        ///                     <see cref="Uninitialized_Type_Tag" />
        ///                     symbols,
        ///                     <li>
        ///                         <literal>null</literal>
        ///                         for the other types of symbol.
        ///     </ul>
        /// </remarks>
        internal readonly string value;

        /// <summary>Additional information about this symbol, generally computed lazily.</summary>
        /// <remarks>
        ///     Additional information about this symbol, generally computed lazily.
        ///     <i>
        ///         Warning: the value of
        ///         this field is ignored when comparing Symbol instances
        ///     </i>
        ///     (to avoid duplicate entries in a
        ///     SymbolTable). Therefore, this field should only contain data that can be computed from the
        ///     other fields of this class. It contains:
        ///     <ul>
        ///         <li>
        ///             the
        ///             <see cref="Type.GetArgumentsAndReturnSizes()" />
        ///             of the symbol's method descriptor for
        ///             <see cref="Constant_Methodref_Tag" />
        ///             ,
        ///             <see cref="Constant_Interface_Methodref_Tag" />
        ///             and
        ///             <see cref="Constant_Invoke_Dynamic_Tag" />
        ///             symbols,
        ///             <li>
        ///                 the index in the InnerClasses_attribute 'classes' array (plus one) corresponding to this
        ///                 class, for
        ///                 <see cref="Constant_Class_Tag" />
        ///                 symbols,
        ///                 <li>
        ///                     the index (in the class' type table) of the merged type of the two source types for
        ///                     <see cref="Merged_Type_Tag" />
        ///                     symbols,
        ///                     <li>0 for the other types of symbol, or if this field has not been computed yet.
        ///     </ul>
        /// </remarks>
        internal int info;

        /// <summary>Constructs a new Symbol.</summary>
        /// <remarks>
        ///     Constructs a new Symbol. This constructor can't be used directly because the Symbol class is
        ///     abstract. Instead, use the factory methods of the
        ///     <see cref="SymbolTable" />
        ///     class.
        /// </remarks>
        /// <param name="index">
        ///     the symbol index in the constant pool, in the BootstrapMethods attribute, or in
        ///     the (ASM specific) type table of a class (depending on 'tag').
        /// </param>
        /// <param name="tag">
        ///     the symbol type. Must be one of the static tag values defined in this class.
        /// </param>
        /// <param name="owner">
        ///     The internal name of the symbol's owner class. Maybe
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="name">
        ///     The name of the symbol's corresponding class field or method. Maybe
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="value">
        ///     The string value of this symbol. Maybe
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="data">The numeric value of this symbol.</param>
        internal Symbol(int index, int tag, string owner, string name, string value, long
            data)
        {
            // Tag values for the constant pool entries (using the same order as in the JVMS).
            // Tag values for the BootstrapMethods attribute entries (ASM specific tag).
            // Tag values for the type table entries (ASM specific tags).
            // Instance fields.
            this.index = index;
            this.tag = tag;
            this.owner = owner;
            this.name = name;
            this.value = value;
            this.data = data;
        }

        /// <summary>
        ///     Returns the result
        ///     <see cref="Type.GetArgumentsAndReturnSizes()" />
        ///     on
        ///     <see cref="value" />
        ///     .
        /// </summary>
        /// <returns>
        ///     the result
        ///     <see cref="Type.GetArgumentsAndReturnSizes()" />
        ///     on
        ///     <see cref="value" />
        ///     (memoized in
        ///     <see cref="info" />
        ///     for efficiency). This should only be used for
        ///     <see cref="Constant_Methodref_Tag" />
        ///     ,
        ///     <see cref="Constant_Interface_Methodref_Tag" />
        ///     and
        ///     <see cref="Constant_Invoke_Dynamic_Tag" />
        ///     symbols.
        /// </returns>
        internal virtual int GetArgumentsAndReturnSizes()
        {
            if (info == 0) info = Type.GetArgumentsAndReturnSizes(value);
            return info;
        }
    }
}