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
    /// <summary>
    ///     The constant pool entries, the BootstrapMethods attribute entries and the (ASM specific) type
    ///     table entries of a class.
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
    internal sealed class SymbolTable
    {
        /// <summary>The ClassWriter to which this SymbolTable belongs.</summary>
        /// <remarks>
        ///     The ClassWriter to which this SymbolTable belongs. This is only used to get access to
        ///     <see cref="ClassWriter.GetCommonSuperClass(string, string)" />
        ///     and to serialize custom attributes with
        ///     <see cref="Attribute.Write(ClassWriter, byte[], int, int, int)" />
        ///     .
        /// </remarks>
        internal readonly ClassWriter classWriter;

        /// <summary>
        ///     The content of the ClassFile's constant_pool JVMS structure corresponding to this SymbolTable.
        /// </summary>
        /// <remarks>
        ///     The content of the ClassFile's constant_pool JVMS structure corresponding to this SymbolTable.
        ///     The ClassFile's constant_pool_count field is <i>not</i> included.
        /// </remarks>
        private readonly ByteVector constantPool;

        /// <summary>
        ///     The ClassReader from which this SymbolTable was constructed, or
        ///     <literal>null</literal>
        ///     if it was
        ///     constructed from scratch.
        /// </summary>
        private readonly ClassReader sourceClassReader;

        /// <summary>
        ///     The number of bootstrap methods in
        ///     <see cref="bootstrapMethods" />
        ///     . Corresponds to the
        ///     BootstrapMethods_attribute's num_bootstrap_methods field value.
        /// </summary>
        private int bootstrapMethodCount;

        /// <summary>
        ///     The content of the BootstrapMethods attribute 'bootstrap_methods' array corresponding to this
        ///     SymbolTable.
        /// </summary>
        /// <remarks>
        ///     The content of the BootstrapMethods attribute 'bootstrap_methods' array corresponding to this
        ///     SymbolTable. Note that the first 6 bytes of the BootstrapMethods_attribute, and its
        ///     num_bootstrap_methods field, are <i>not</i> included.
        /// </remarks>
        private ByteVector bootstrapMethods;

        /// <summary>The internal name of the class to which this symbol table belongs.</summary>
        private string className;

        /// <summary>
        ///     The number of constant pool items in
        ///     <see cref="constantPool" />
        ///     , plus 1. The first constant pool
        ///     item has index 1, and long and double items count for two items.
        /// </summary>
        private int constantPoolCount;

        /// <summary>
        ///     A hash set of all the entries in this SymbolTable (this includes the constant pool entries, the
        ///     bootstrap method entries and the type table entries).
        /// </summary>
        /// <remarks>
        ///     A hash set of all the entries in this SymbolTable (this includes the constant pool entries, the
        ///     bootstrap method entries and the type table entries). Each
        ///     <see cref="Entry" />
        ///     instance is stored at
        ///     the array index given by its hash code modulo the array size. If several entries must be stored
        ///     at the same array index, they are linked together via their
        ///     <see cref="Entry.next" />
        ///     field. The
        ///     factory methods of this class make sure that this table does not contain duplicated entries.
        /// </remarks>
        private Entry[] entries;

        /// <summary>
        ///     The total number of
        ///     <see cref="Entry" />
        ///     instances in
        ///     <see cref="entries" />
        ///     . This includes entries that are
        ///     accessible (recursively) via
        ///     <see cref="Entry.next" />
        ///     .
        /// </summary>
        private int entryCount;

        /// <summary>
        ///     The major version number of the class to which this symbol table belongs.
        /// </summary>
        private int majorVersion;

        /// <summary>
        ///     The actual number of elements in
        ///     <see cref="typeTable" />
        ///     . These elements are stored from index 0 to
        ///     typeCount (excluded). The other array entries are empty.
        /// </summary>
        private int typeCount;

        /// <summary>
        ///     An ASM specific type table used to temporarily store internal names that will not necessarily
        ///     be stored in the constant pool.
        /// </summary>
        /// <remarks>
        ///     An ASM specific type table used to temporarily store internal names that will not necessarily
        ///     be stored in the constant pool. This type table is used by the control flow and data flow
        ///     analysis algorithm used to compute stack map frames from scratch. This array stores
        ///     <see cref="Symbol.Type_Tag" />
        ///     and
        ///     <see cref="Symbol.Uninitialized_Type_Tag" />
        ///     ) Symbol. The type symbol at index
        ///     <c>i</c>
        ///     has its
        ///     <see cref="Symbol.index" />
        ///     equal to
        ///     <c>i</c>
        ///     (and vice versa).
        /// </remarks>
        private Entry[] typeTable;

        /// <summary>Constructs a new, empty SymbolTable for the given ClassWriter.</summary>
        /// <param name="classWriter">a ClassWriter.</param>
        internal SymbolTable(ClassWriter classWriter)
        {
            this.classWriter = classWriter;
            sourceClassReader = null;
            entries = new Entry[256];
            constantPoolCount = 1;
            constantPool = new ByteVector();
        }

        /// <summary>
        ///     Constructs a new SymbolTable for the given ClassWriter, initialized with the constant pool and
        ///     bootstrap methods of the given ClassReader.
        /// </summary>
        /// <param name="classWriter">a ClassWriter.</param>
        /// <param name="classReader">
        ///     the ClassReader whose constant pool and bootstrap methods must be copied to
        ///     initialize the SymbolTable.
        /// </param>
        internal SymbolTable(ClassWriter classWriter, ClassReader classReader)
        {
            this.classWriter = classWriter;
            sourceClassReader = classReader;
            // Copy the constant pool binary content.
            var inputBytes = classReader.classFileBuffer;
            var constantPoolOffset = classReader.GetItem(1) - 1;
            var constantPoolLength = classReader.header - constantPoolOffset;
            constantPoolCount = classReader.ItemCount;
            constantPool = new ByteVector(constantPoolLength);
            constantPool.PutByteArray(inputBytes, constantPoolOffset, constantPoolLength);
            // Add the constant pool items in the symbol table entries. Reserve enough space in 'entries' to
            // avoid too many hash set collisions (entries is not dynamically resized by the addConstant*
            // method calls below), and to account for bootstrap method entries.
            entries = new Entry[constantPoolCount * 2];
            var charBuffer = new char[classReader.GetMaxStringLength()];
            var hasBootstrapMethods = false;
            var itemIndex = 1;
            while (itemIndex < constantPoolCount)
            {
                var itemOffset = classReader.GetItem(itemIndex);
                int itemTag = inputBytes[itemOffset - 1];
                int nameAndTypeItemOffset;
                switch (itemTag)
                {
                    case Symbol.Constant_Fieldref_Tag:
                    case Symbol.Constant_Methodref_Tag:
                    case Symbol.Constant_Interface_Methodref_Tag:
                    {
                        nameAndTypeItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(itemOffset
                                                                                                  + 2));
                        AddConstantMemberReference(itemIndex, itemTag, classReader.ReadClass(itemOffset,
                            charBuffer), classReader.ReadUTF8(nameAndTypeItemOffset, charBuffer), classReader
                            .ReadUTF8(nameAndTypeItemOffset + 2, charBuffer));
                        break;
                    }

                    case Symbol.Constant_Integer_Tag:
                    case Symbol.Constant_Float_Tag:
                    {
                        AddConstantIntegerOrFloat(itemIndex, itemTag, classReader.ReadInt(itemOffset));
                        break;
                    }

                    case Symbol.Constant_Name_And_Type_Tag:
                    {
                        AddConstantNameAndType(itemIndex, classReader.ReadUTF8(itemOffset, charBuffer), classReader
                            .ReadUTF8(itemOffset + 2, charBuffer));
                        break;
                    }

                    case Symbol.Constant_Long_Tag:
                    case Symbol.Constant_Double_Tag:
                    {
                        AddConstantLongOrDouble(itemIndex, itemTag, classReader.ReadLong(itemOffset));
                        break;
                    }

                    case Symbol.Constant_Utf8_Tag:
                    {
                        AddConstantUtf8(itemIndex, classReader.ReadUtf(itemIndex, charBuffer));
                        break;
                    }

                    case Symbol.Constant_Method_Handle_Tag:
                    {
                        var memberRefItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(itemOffset
                                                                                                    + 1));
                        nameAndTypeItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(memberRefItemOffset
                                                                                                  + 2));
                        AddConstantMethodHandle(itemIndex, classReader.ReadByte(itemOffset),
                            classReader.ReadClass(memberRefItemOffset, charBuffer), classReader.ReadUTF8(
                                nameAndTypeItemOffset
                                , charBuffer), classReader.ReadUTF8(nameAndTypeItemOffset + 2, charBuffer));
                        break;
                    }

                    case Symbol.Constant_Dynamic_Tag:
                    case Symbol.Constant_Invoke_Dynamic_Tag:
                    {
                        hasBootstrapMethods = true;
                        nameAndTypeItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(itemOffset
                                                                                                  + 2));
                        AddConstantDynamicOrInvokeDynamicReference(itemTag, itemIndex, classReader.ReadUTF8
                                (nameAndTypeItemOffset, charBuffer), classReader.ReadUTF8(nameAndTypeItemOffset
                                                                                          + 2, charBuffer),
                            classReader.ReadUnsignedShort(itemOffset));
                        break;
                    }

                    case Symbol.Constant_String_Tag:
                    case Symbol.Constant_Class_Tag:
                    case Symbol.Constant_Method_Type_Tag:
                    case Symbol.Constant_Module_Tag:
                    case Symbol.Constant_Package_Tag:
                    {
                        AddConstantUtf8Reference(itemIndex, itemTag, classReader.ReadUTF8(itemOffset, charBuffer
                        ));
                        break;
                    }

                    default:
                    {
                        throw new ArgumentException();
                    }
                }

                itemIndex += itemTag == Symbol.Constant_Long_Tag || itemTag == Symbol.Constant_Double_Tag ? 2 : 1;
            }

            // Copy the BootstrapMethods, if any.
            if (hasBootstrapMethods) CopyBootstrapMethods(classReader, charBuffer);
        }

        /// <summary>
        ///     Read the BootstrapMethods 'bootstrap_methods' array binary content and add them as entries of
        ///     the SymbolTable.
        /// </summary>
        /// <param name="classReader">
        ///     the ClassReader whose bootstrap methods must be copied to initialize the
        ///     SymbolTable.
        /// </param>
        /// <param name="charBuffer">a buffer used to read strings in the constant pool.</param>
        private void CopyBootstrapMethods(ClassReader classReader, char[] charBuffer)
        {
            // Find attributOffset of the 'bootstrap_methods' array.
            var inputBytes = classReader.classFileBuffer;
            var currentAttributeOffset = classReader.GetFirstAttributeOffset();
            for (var i = classReader.ReadUnsignedShort(currentAttributeOffset - 2);
                i > 0;
                --
                    i)
            {
                var attributeName = classReader.ReadUTF8(currentAttributeOffset, charBuffer);
                if (Constants.Bootstrap_Methods.Equals(attributeName))
                {
                    bootstrapMethodCount = classReader.ReadUnsignedShort(currentAttributeOffset + 6);
                    break;
                }

                currentAttributeOffset += 6 + classReader.ReadInt(currentAttributeOffset + 2);
            }

            if (bootstrapMethodCount > 0)
            {
                // Compute the offset and the length of the BootstrapMethods 'bootstrap_methods' array.
                var bootstrapMethodsOffset = currentAttributeOffset + 8;
                var bootstrapMethodsLength = classReader.ReadInt(currentAttributeOffset + 2) - 2;
                bootstrapMethods = new ByteVector(bootstrapMethodsLength);
                bootstrapMethods.PutByteArray(inputBytes, bootstrapMethodsOffset, bootstrapMethodsLength
                );
                // Add each bootstrap method in the symbol table entries.
                var currentOffset = bootstrapMethodsOffset;
                for (var i = 0; i < bootstrapMethodCount; i++)
                {
                    var offset = currentOffset - bootstrapMethodsOffset;
                    var bootstrapMethodRef = classReader.ReadUnsignedShort(currentOffset);
                    currentOffset += 2;
                    var numBootstrapArguments = classReader.ReadUnsignedShort(currentOffset);
                    currentOffset += 2;
                    var hashCode = classReader.ReadConst(bootstrapMethodRef, charBuffer).GetHashCode(
                    );
                    while (numBootstrapArguments-- > 0)
                    {
                        var bootstrapArgument = classReader.ReadUnsignedShort(currentOffset);
                        currentOffset += 2;
                        hashCode ^= classReader.ReadConst(bootstrapArgument, charBuffer).GetHashCode();
                    }

                    Add(new Entry(i, Symbol.Bootstrap_Method_Tag, offset, hashCode & 0x7FFFFFFF));
                }
            }
        }

        /// <summary>Returns the ClassReader from which this SymbolTable was constructed.</summary>
        /// <returns>
        ///     the ClassReader from which this SymbolTable was constructed, or
        ///     <literal>null</literal>
        ///     if it
        ///     was constructed from scratch.
        /// </returns>
        internal ClassReader GetSource()
        {
            return sourceClassReader;
        }

        /// <summary>
        ///     Returns the major version of the class to which this symbol table belongs.
        /// </summary>
        /// <returns>the major version of the class to which this symbol table belongs.</returns>
        internal int GetMajorVersion()
        {
            return majorVersion;
        }

        /// <summary>
        ///     Returns the internal name of the class to which this symbol table belongs.
        /// </summary>
        /// <returns>the internal name of the class to which this symbol table belongs.</returns>
        internal string GetClassName()
        {
            return className;
        }

        /// <summary>
        ///     Sets the major version and the name of the class to which this symbol table belongs.
        /// </summary>
        /// <remarks>
        ///     Sets the major version and the name of the class to which this symbol table belongs. Also adds
        ///     the class name to the constant pool.
        /// </remarks>
        /// <param name="majorVersion">a major ClassFile version number.</param>
        /// <param name="className">an internal class name.</param>
        /// <returns>
        ///     the constant pool index of a new or already existing Symbol with the given class name.
        /// </returns>
        internal int SetMajorVersionAndClassName(int majorVersion, string className)
        {
            this.majorVersion = majorVersion;
            this.className = className;
            return AddConstantClass(className).index;
        }

        /// <summary>
        ///     Returns the number of items in this symbol table's constant_pool array (plus 1).
        /// </summary>
        /// <returns>
        ///     the number of items in this symbol table's constant_pool array (plus 1).
        /// </returns>
        internal int GetConstantPoolCount()
        {
            return constantPoolCount;
        }

        /// <summary>Returns the length in bytes of this symbol table's constant_pool array.</summary>
        /// <returns>the length in bytes of this symbol table's constant_pool array.</returns>
        internal int GetConstantPoolLength()
        {
            return constantPool.length;
        }

        /// <summary>
        ///     Puts this symbol table's constant_pool array in the given ByteVector, preceded by the
        ///     constant_pool_count value.
        /// </summary>
        /// <param name="output">where the JVMS ClassFile's constant_pool array must be put.</param>
        internal void PutConstantPool(ByteVector output)
        {
            output.PutShort(constantPoolCount).PutByteArray(constantPool.data, 0, constantPool
                .length);
        }

        /// <summary>
        ///     Returns the size in bytes of this symbol table's BootstrapMethods attribute.
        /// </summary>
        /// <remarks>
        ///     Returns the size in bytes of this symbol table's BootstrapMethods attribute. Also adds the
        ///     attribute name in the constant pool.
        /// </remarks>
        /// <returns>the size in bytes of this symbol table's BootstrapMethods attribute.</returns>
        internal int ComputeBootstrapMethodsSize()
        {
            if (bootstrapMethods != null)
            {
                AddConstantUtf8(Constants.Bootstrap_Methods);
                return 8 + bootstrapMethods.length;
            }

            return 0;
        }

        /// <summary>
        ///     Puts this symbol table's BootstrapMethods attribute in the given ByteVector.
        /// </summary>
        /// <remarks>
        ///     Puts this symbol table's BootstrapMethods attribute in the given ByteVector. This includes the
        ///     6 attribute header bytes and the num_bootstrap_methods value.
        /// </remarks>
        /// <param name="output">where the JVMS BootstrapMethods attribute must be put.</param>
        internal void PutBootstrapMethods(ByteVector output)
        {
            if (bootstrapMethods != null)
                output.PutShort(AddConstantUtf8(Constants.Bootstrap_Methods)).PutInt(bootstrapMethods
                                                                                         .length + 2)
                    .PutShort(bootstrapMethodCount).PutByteArray(bootstrapMethods.data,
                        0, bootstrapMethods.length);
        }

        // -----------------------------------------------------------------------------------------------
        // Generic symbol table entries management.
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the list of entries which can potentially have the given hash code.
        /// </summary>
        /// <param name="hashCode">
        ///     a
        ///     <see cref="Entry.hashCode" />
        ///     value.
        /// </param>
        /// <returns>
        ///     the list of entries which can potentially have the given hash code. The list is stored
        ///     via the
        ///     <see cref="Entry.next" />
        ///     field.
        /// </returns>
        private Entry Get(int hashCode)
        {
            return entries[hashCode % entries.Length];
        }

        /// <summary>
        ///     Puts the given entry in the
        ///     <see cref="entries" />
        ///     hash set. This method does <i>not</i> check
        ///     whether
        ///     <see cref="entries" />
        ///     already contains a similar entry or not.
        ///     <see cref="entries" />
        ///     is resized
        ///     if necessary to avoid hash collisions (multiple entries needing to be stored at the same
        ///     <see cref="entries" />
        ///     array index) as much as possible, with reasonable memory usage.
        /// </summary>
        /// <param name="entry">
        ///     an Entry (which must not already be contained in
        ///     <see cref="entries" />
        ///     ).
        /// </param>
        /// <returns>the given entry</returns>
        private Entry Put(Entry entry)
        {
            if (entryCount > entries.Length * 3 / 4)
            {
                var currentCapacity = entries.Length;
                var newCapacity = currentCapacity * 2 + 1;
                var newEntries = new Entry[newCapacity];
                for (var i = currentCapacity - 1; i >= 0; --i)
                {
                    var currentEntry = entries[i];
                    while (currentEntry != null)
                    {
                        var newCurrentEntryIndex = currentEntry.hashCode % newCapacity;
                        var nextEntry = currentEntry.next;
                        currentEntry.next = newEntries[newCurrentEntryIndex];
                        newEntries[newCurrentEntryIndex] = currentEntry;
                        currentEntry = nextEntry;
                    }
                }

                entries = newEntries;
            }

            entryCount++;
            var index = entry.hashCode % entries.Length;
            entry.next = entries[index];
            return entries[index] = entry;
        }

        /// <summary>
        ///     Adds the given entry in the
        ///     <see cref="entries" />
        ///     hash set. This method does <i>not</i> check
        ///     whether
        ///     <see cref="entries" />
        ///     already contains a similar entry or not, and does <i>not</i> resize
        ///     <see cref="entries" />
        ///     if necessary.
        /// </summary>
        /// <param name="entry">
        ///     an Entry (which must not already be contained in
        ///     <see cref="entries" />
        ///     ).
        /// </param>
        private void Add(Entry entry)
        {
            entryCount++;
            var index = entry.hashCode % entries.Length;
            entry.next = entries[index];
            entries[index] = entry;
        }

        // -----------------------------------------------------------------------------------------------
        // Constant pool entries management.
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds a number or string constant to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a number or string constant to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">
        ///     the value of the constant to be added to the constant pool. This parameter must be
        ///     an
        ///     <see cref="int" />
        ///     ,
        ///     <see cref="byte" />
        ///     ,
        ///     <see cref="char" />
        ///     ,
        ///     <see cref="short" />
        ///     ,
        ///     <see cref="bool" />
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
        ///     .
        /// </param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstant(object value)
        {
            if (value is int) return AddConstantInteger((int) value);

            if (value is byte) return AddConstantInteger((byte) value);

            if (value is char) return AddConstantInteger((char) value);

            if (value is short) return AddConstantInteger((short) value);

            if (value is bool) return AddConstantInteger((bool) value ? 1 : 0);

            if (value is float) return AddConstantFloat((float) value);

            if (value is long) return AddConstantLong((long) value);

            if (value is double) return AddConstantDouble((double) value);

            if (value is string) return AddConstantString((string) value);

            if (value is Type)
            {
                var type = (Type) value;
                var typeSort = type.GetSort();
                if (typeSort == Type.Object)
                    return AddConstantClass(type.GetInternalName());
                if (typeSort == Type.Method)
                    return AddConstantMethodType(type.GetDescriptor());
                return AddConstantClass(type.GetDescriptor());
            }

            if (value is Handle)
            {
                var handle = (Handle) value;
                return AddConstantMethodHandle(handle.GetTag(), handle.GetOwner(), handle.GetName
                    (), handle.GetDesc(), handle.IsInterface());
            }

            if (value is ConstantDynamic)
            {
                var constantDynamic = (ConstantDynamic) value;
                return AddConstantDynamic(constantDynamic.GetName(), constantDynamic.GetDescriptor
                    (), constantDynamic.GetBootstrapMethod(), constantDynamic.GetBootstrapMethodArgumentsUnsafe
                    ());
            }

            throw new ArgumentException("value " + value);
        }

        /// <summary>Adds a CONSTANT_Class_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Class_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">the internal name of a class.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantClass(string value)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Class_Tag, value);
        }

        /// <summary>
        ///     Adds a CONSTANT_Fieldref_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Fieldref_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="owner">the internal name of a class.</param>
        /// <param name="name">a field name.</param>
        /// <param name="descriptor">a field descriptor.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantFieldref(string owner, string name, string descriptor)
        {
            return AddConstantMemberReference(Symbol.Constant_Fieldref_Tag, owner, name, descriptor
            );
        }

        /// <summary>
        ///     Adds a CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to the constant pool of this
        ///     symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to the constant pool of this
        ///     symbol table. Does nothing if the constant pool already contains a similar item.
        /// </remarks>
        /// <param name="owner">the internal name of a class.</param>
        /// <param name="name">a method name.</param>
        /// <param name="descriptor">a method descriptor.</param>
        /// <param name="isInterface">whether owner is an interface or not.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantMethodref(string owner, string name, string descriptor
            , bool isInterface)
        {
            var tag = isInterface ? Symbol.Constant_Interface_Methodref_Tag : Symbol.Constant_Methodref_Tag;
            return AddConstantMemberReference(tag, owner, name, descriptor);
        }

        /// <summary>
        ///     Adds a CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to
        ///     the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to
        ///     the constant pool of this symbol table. Does nothing if the constant pool already contains a
        ///     similar item.
        /// </remarks>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Fieldref_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_Methodref_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Interface_Methodref_Tag" />
        ///     .
        /// </param>
        /// <param name="owner">the internal name of a class.</param>
        /// <param name="name">a field or method name.</param>
        /// <param name="descriptor">a field or method descriptor.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        private Entry AddConstantMemberReference(int tag, string owner, string
            name, string descriptor)
        {
            var hashCode = Hash(tag, owner, name, descriptor);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.owner.Equals(owner) &&
                    entry.name.Equals(name) && entry.value.Equals(descriptor))
                    return entry;
                entry = entry.next;
            }

            constantPool.Put122(tag, AddConstantClass(owner).index, AddConstantNameAndType(name
                , descriptor));
            return Put(new Entry(constantPoolCount++, tag, owner, name, descriptor
                , 0, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info
        ///     to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Fieldref_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_Methodref_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Interface_Methodref_Tag" />
        ///     .
        /// </param>
        /// <param name="owner">the internal name of a class.</param>
        /// <param name="name">a field or method name.</param>
        /// <param name="descriptor">a field or method descriptor.</param>
        private void AddConstantMemberReference(int index, int tag, string owner, string
            name, string descriptor)
        {
            Add(new Entry(index, tag, owner, name, descriptor, 0, Hash(tag, owner
                , name, descriptor)));
        }

        /// <summary>Adds a CONSTANT_String_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_String_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">a string.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantString(string value)
        {
            return AddConstantUtf8Reference(Symbol.Constant_String_Tag, value);
        }

        /// <summary>Adds a CONSTANT_Integer_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Integer_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">an int.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantInteger(int value)
        {
            return AddConstantIntegerOrFloat(Symbol.Constant_Integer_Tag, value);
        }

        /// <summary>Adds a CONSTANT_Float_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Float_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">a float.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantFloat(float value)
        {
            return AddConstantIntegerOrFloat(Symbol.Constant_Float_Tag, Runtime.FloatToRawIntBits
                (value));
        }

        /// <summary>
        ///     Adds a CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol table.
        ///     Does nothing if the constant pool already contains a similar item.
        /// </remarks>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Integer_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Float_Tag" />
        ///     .
        /// </param>
        /// <param name="value">an int or float.</param>
        /// <returns>a constant pool constant with the given tag and primitive values.</returns>
        private Symbol AddConstantIntegerOrFloat(int tag, int value)
        {
            var hashCode = Hash(tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == value) return entry;
                entry = entry.next;
            }

            constantPool.PutByte(tag).PutInt(value);
            return Put(new Entry(constantPoolCount++, tag, value, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol
        ///     table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Integer_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Float_Tag" />
        ///     .
        /// </param>
        /// <param name="value">an int or float.</param>
        private void AddConstantIntegerOrFloat(int index, int tag, int value)
        {
            Add(new Entry(index, tag, value, Hash(tag, value)));
        }

        /// <summary>Adds a CONSTANT_Long_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Long_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">a long.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantLong(long value)
        {
            return AddConstantLongOrDouble(Symbol.Constant_Long_Tag, value);
        }

        /// <summary>Adds a CONSTANT_Double_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Double_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">a double.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantDouble(double value)
        {
            return AddConstantLongOrDouble(Symbol.Constant_Double_Tag, Runtime.DoubleToRawLongBits
                (value));
        }

        /// <summary>
        ///     Adds a CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol table.
        ///     Does nothing if the constant pool already contains a similar item.
        /// </remarks>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Long_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Double_Tag" />
        ///     .
        /// </param>
        /// <param name="value">a long or double.</param>
        /// <returns>a constant pool constant with the given tag and primitive values.</returns>
        private Symbol AddConstantLongOrDouble(int tag, long value)
        {
            var hashCode = Hash(tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == value) return entry;
                entry = entry.next;
            }

            var index = constantPoolCount;
            constantPool.PutByte(tag).PutLong(value);
            constantPoolCount += 2;
            return Put(new Entry(index, tag, value, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol
        ///     table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Long_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Double_Tag" />
        ///     .
        /// </param>
        /// <param name="value">a long or double.</param>
        private void AddConstantLongOrDouble(int index, int tag, long value)
        {
            Add(new Entry(index, tag, value, Hash(tag, value)));
        }

        /// <summary>
        ///     Adds a CONSTANT_NameAndType_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_NameAndType_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="name">a field or method name.</param>
        /// <param name="descriptor">a field or method descriptor.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal int AddConstantNameAndType(string name, string descriptor)
        {
            var tag = Symbol.Constant_Name_And_Type_Tag;
            var hashCode = Hash(tag, name, descriptor);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.name.Equals(name) &&
                    entry.value.Equals(descriptor))
                    return entry.index;
                entry = entry.next;
            }

            constantPool.Put122(tag, AddConstantUtf8(name), AddConstantUtf8(descriptor));
            return Put(new Entry(constantPoolCount++, tag, name, descriptor, hashCode
            )).index;
        }

        /// <summary>
        ///     Adds a new CONSTANT_NameAndType_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="name">a field or method name.</param>
        /// <param name="descriptor">a field or method descriptor.</param>
        private void AddConstantNameAndType(int index, string name, string descriptor)
        {
            var tag = Symbol.Constant_Name_And_Type_Tag;
            Add(new Entry(index, tag, name, descriptor, Hash(tag, name, descriptor
            )));
        }

        /// <summary>Adds a CONSTANT_Utf8_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Utf8_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="value">a string.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal int AddConstantUtf8(string value)
        {
            var hashCode = Hash(Symbol.Constant_Utf8_Tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Constant_Utf8_Tag && entry.hashCode == hashCode && entry.value.Equals(value))
                    return entry.index;
                entry = entry.next;
            }

            constantPool.PutByte(Symbol.Constant_Utf8_Tag).PutUTF8(value);
            return Put(new Entry(constantPoolCount++, Symbol.Constant_Utf8_Tag, value
                , hashCode)).index;
        }

        /// <summary>
        ///     Adds a new CONSTANT_String_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="value">a string.</param>
        private void AddConstantUtf8(int index, string value)
        {
            Add(new Entry(index, Symbol.Constant_Utf8_Tag, value, Hash(Symbol.Constant_Utf8_Tag
                , value)));
        }

        /// <summary>
        ///     Adds a CONSTANT_MethodHandle_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_MethodHandle_info to the constant pool of this symbol table. Does nothing if
        ///     the constant pool already contains a similar item.
        /// </remarks>
        /// <param name="referenceKind">
        ///     one of
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
        /// <param name="owner">the internal name of a class of interface.</param>
        /// <param name="name">a field or method name.</param>
        /// <param name="descriptor">a field or method descriptor.</param>
        /// <param name="isInterface">whether owner is an interface or not.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantMethodHandle(int referenceKind, string owner, string name
            , string descriptor, bool isInterface)
        {
            var tag = Symbol.Constant_Method_Handle_Tag;
            // Note that we don't need to include isInterface in the hash computation, because it is
            // redundant with owner (we can't have the same owner with different isInterface values).
            var hashCode = Hash(tag, owner, name, descriptor, referenceKind);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == referenceKind
                    && entry.owner.Equals(owner) && entry.name.Equals(name) && entry.value.Equals(descriptor
                    ))
                    return entry;
                entry = entry.next;
            }

            if (referenceKind <= OpcodesConstants.H_Putstatic)
                constantPool.Put112(tag, referenceKind, AddConstantFieldref(owner, name, descriptor
                ).index);
            else
                constantPool.Put112(tag, referenceKind, AddConstantMethodref(owner, name, descriptor
                    , isInterface).index);
            return Put(new Entry(constantPoolCount++, tag, owner, name, descriptor
                , referenceKind, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_MethodHandle_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="referenceKind">
        ///     one of
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
        /// <param name="owner">the internal name of a class of interface.</param>
        /// <param name="name">a field or method name.</param>
        /// <param name="descriptor">a field or method descriptor.</param>
        private void AddConstantMethodHandle(int index, int referenceKind, string owner,
            string name, string descriptor)
        {
            var tag = Symbol.Constant_Method_Handle_Tag;
            var hashCode = Hash(tag, owner, name, descriptor, referenceKind);
            Add(new Entry(index, tag, owner, name, descriptor, referenceKind, hashCode
            ));
        }

        /// <summary>
        ///     Adds a CONSTANT_MethodType_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_MethodType_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="methodDescriptor">a method descriptor.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantMethodType(string methodDescriptor)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Method_Type_Tag, methodDescriptor
            );
        }

        /// <summary>Adds a CONSTANT_Dynamic_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Dynamic_info to the constant pool of this symbol table. Also adds the related
        ///     bootstrap method to the BootstrapMethods of this symbol table. Does nothing if the constant
        ///     pool already contains a similar item.
        /// </remarks>
        /// <param name="name">a method name.</param>
        /// <param name="descriptor">a field descriptor.</param>
        /// <param name="bootstrapMethodHandle">a bootstrap method handle.</param>
        /// <param name="bootstrapMethodArguments">the bootstrap method arguments.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantDynamic(string name, string descriptor, Handle bootstrapMethodHandle
            , params object[] bootstrapMethodArguments)
        {
            var bootstrapMethod = AddBootstrapMethod(bootstrapMethodHandle, bootstrapMethodArguments
            );
            return AddConstantDynamicOrInvokeDynamicReference(Symbol.Constant_Dynamic_Tag, name
                , descriptor, bootstrapMethod.index);
        }

        /// <summary>
        ///     Adds a CONSTANT_InvokeDynamic_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_InvokeDynamic_info to the constant pool of this symbol table. Also adds the
        ///     related bootstrap method to the BootstrapMethods of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="name">a method name.</param>
        /// <param name="descriptor">a method descriptor.</param>
        /// <param name="bootstrapMethodHandle">a bootstrap method handle.</param>
        /// <param name="bootstrapMethodArguments">the bootstrap method arguments.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantInvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle
            , params object[] bootstrapMethodArguments)
        {
            var bootstrapMethod = AddBootstrapMethod(bootstrapMethodHandle, bootstrapMethodArguments
            );
            return AddConstantDynamicOrInvokeDynamicReference(Symbol.Constant_Invoke_Dynamic_Tag
                , name, descriptor, bootstrapMethod.index);
        }

        /// <summary>
        ///     Adds a CONSTANT_Dynamic or a CONSTANT_InvokeDynamic_info to the constant pool of this symbol
        ///     table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Dynamic or a CONSTANT_InvokeDynamic_info to the constant pool of this symbol
        ///     table. Does nothing if the constant pool already contains a similar item.
        /// </remarks>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Dynamic_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Invoke_Dynamic_Tag" />
        ///     .
        /// </param>
        /// <param name="name">a method name.</param>
        /// <param name="descriptor">
        ///     a field descriptor for CONSTANT_DYNAMIC_TAG) or a method descriptor for
        ///     CONSTANT_INVOKE_DYNAMIC_TAG.
        /// </param>
        /// <param name="bootstrapMethodIndex">
        ///     the index of a bootstrap method in the BootstrapMethods attribute.
        /// </param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        private Symbol AddConstantDynamicOrInvokeDynamicReference(int tag, string name, string
            descriptor, int bootstrapMethodIndex)
        {
            var hashCode = Hash(tag, name, descriptor, bootstrapMethodIndex);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == bootstrapMethodIndex
                    && entry.name.Equals(name) && entry.value.Equals(descriptor))
                    return entry;
                entry = entry.next;
            }

            constantPool.Put122(tag, bootstrapMethodIndex, AddConstantNameAndType(name, descriptor
            ));
            return Put(new Entry(constantPoolCount++, tag, null, name, descriptor
                , bootstrapMethodIndex, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Dynamic_info or CONSTANT_InvokeDynamic_info to the constant pool of this
        ///     symbol table.
        /// </summary>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Dynamic_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Invoke_Dynamic_Tag" />
        ///     .
        /// </param>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="name">a method name.</param>
        /// <param name="descriptor">
        ///     a field descriptor for CONSTANT_DYNAMIC_TAG or a method descriptor for
        ///     CONSTANT_INVOKE_DYNAMIC_TAG.
        /// </param>
        /// <param name="bootstrapMethodIndex">
        ///     the index of a bootstrap method in the BootstrapMethods attribute.
        /// </param>
        private void AddConstantDynamicOrInvokeDynamicReference(int tag, int index, string
            name, string descriptor, int bootstrapMethodIndex)
        {
            var hashCode = Hash(tag, name, descriptor, bootstrapMethodIndex);
            Add(new Entry(index, tag, null, name, descriptor, bootstrapMethodIndex
                , hashCode));
        }

        /// <summary>Adds a CONSTANT_Module_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Module_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="moduleName">a fully qualified name (using dots) of a module.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantModule(string moduleName)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Module_Tag, moduleName);
        }

        /// <summary>Adds a CONSTANT_Package_info to the constant pool of this symbol table.</summary>
        /// <remarks>
        ///     Adds a CONSTANT_Package_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </remarks>
        /// <param name="packageName">the internal name of a package.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddConstantPackage(string packageName)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Package_Tag, packageName);
        }

        /// <summary>
        ///     Adds a CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
        ///     CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
        ///     CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table. Does
        ///     nothing if the constant pool already contains a similar item.
        /// </remarks>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Class_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_String_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_Method_Type_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_Module_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Package_Tag" />
        ///     .
        /// </param>
        /// <param name="value">
        ///     an internal class name, an arbitrary string, a method descriptor, a module or a
        ///     package name, depending on tag.
        /// </param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        private Symbol AddConstantUtf8Reference(int tag, string value)
        {
            var hashCode = Hash(tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.value.Equals(value)) return entry;
                entry = entry.next;
            }

            constantPool.Put12(tag, AddConstantUtf8(value));
            return Put(new Entry(constantPoolCount++, tag, value, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
        ///     CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index">the constant pool index of the new Symbol.</param>
        /// <param name="tag">
        ///     one of
        ///     <see cref="Symbol.Constant_Class_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_String_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_Method_Type_Tag" />
        ///     ,
        ///     <see cref="Symbol.Constant_Module_Tag" />
        ///     or
        ///     <see cref="Symbol.Constant_Package_Tag" />
        ///     .
        /// </param>
        /// <param name="value">
        ///     an internal class name, an arbitrary string, a method descriptor, a module or a
        ///     package name, depending on tag.
        /// </param>
        private void AddConstantUtf8Reference(int index, int tag, string value)
        {
            Add(new Entry(index, tag, value, Hash(tag, value)));
        }

        // -----------------------------------------------------------------------------------------------
        // Bootstrap method entries management.
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds a bootstrap method to the BootstrapMethods attribute of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a bootstrap method to the BootstrapMethods attribute of this symbol table. Does nothing if
        ///     the BootstrapMethods already contains a similar bootstrap method.
        /// </remarks>
        /// <param name="bootstrapMethodHandle">a bootstrap method handle.</param>
        /// <param name="bootstrapMethodArguments">the bootstrap method arguments.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        internal Symbol AddBootstrapMethod(Handle bootstrapMethodHandle, params object[]
            bootstrapMethodArguments)
        {
            var bootstrapMethodsAttribute = bootstrapMethods;
            if (bootstrapMethodsAttribute == null) bootstrapMethodsAttribute = bootstrapMethods = new ByteVector();
            // The bootstrap method arguments can be Constant_Dynamic values, which reference other
            // bootstrap methods. We must therefore add the bootstrap method arguments to the constant pool
            // and BootstrapMethods attribute first, so that the BootstrapMethods attribute is not modified
            // while adding the given bootstrap method to it, in the rest of this method.
            foreach (var bootstrapMethodArgument in bootstrapMethodArguments) AddConstant(bootstrapMethodArgument);
            // Write the bootstrap method in the BootstrapMethods table. This is necessary to be able to
            // compare it with existing ones, and will be reverted below if there is already a similar
            // bootstrap method.
            var bootstrapMethodOffset = bootstrapMethodsAttribute.length;
            bootstrapMethodsAttribute.PutShort(AddConstantMethodHandle(bootstrapMethodHandle.GetTag(),
                bootstrapMethodHandle.GetOwner(), bootstrapMethodHandle.GetName(), bootstrapMethodHandle
                    .GetDesc(), bootstrapMethodHandle.IsInterface()).index);
            var numBootstrapArguments = bootstrapMethodArguments.Length;
            bootstrapMethodsAttribute.PutShort(numBootstrapArguments);
            foreach (var bootstrapMethodArgument in bootstrapMethodArguments)
                bootstrapMethodsAttribute.PutShort(AddConstant(bootstrapMethodArgument).index);
            // Compute the length and the hash code of the bootstrap method.
            var bootstrapMethodlength = bootstrapMethodsAttribute.length - bootstrapMethodOffset;
            var hashCode = bootstrapMethodHandle.GetHashCode();
            foreach (var bootstrapMethodArgument in bootstrapMethodArguments)
                hashCode ^= bootstrapMethodArgument.GetHashCode();
            hashCode &= 0x7FFFFFFF;
            // Add the bootstrap method to the symbol table or revert the above changes.
            return AddBootstrapMethod(bootstrapMethodOffset, bootstrapMethodlength, hashCode);
        }

        /// <summary>
        ///     Adds a bootstrap method to the BootstrapMethods attribute of this symbol table.
        /// </summary>
        /// <remarks>
        ///     Adds a bootstrap method to the BootstrapMethods attribute of this symbol table. Does nothing if
        ///     the BootstrapMethods already contains a similar bootstrap method (more precisely, reverts the
        ///     content of
        ///     <see cref="bootstrapMethods" />
        ///     to remove the last, duplicate bootstrap method).
        /// </remarks>
        /// <param name="offset">
        ///     the offset of the last bootstrap method in
        ///     <see cref="bootstrapMethods" />
        ///     , in bytes.
        /// </param>
        /// <param name="length">
        ///     the length of this bootstrap method in
        ///     <see cref="bootstrapMethods" />
        ///     , in bytes.
        /// </param>
        /// <param name="hashCode">the hash code of this bootstrap method.</param>
        /// <returns>a new or already existing Symbol with the given value.</returns>
        private Symbol AddBootstrapMethod(int offset, int length, int hashCode)
        {
            var bootstrapMethodsData = bootstrapMethods.data;
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Bootstrap_Method_Tag && entry.hashCode == hashCode)
                {
                    var otherOffset = (int) entry.data;
                    var isSameBootstrapMethod = true;
                    for (var i = 0; i < length; ++i)
                        if (bootstrapMethodsData[offset + i] != bootstrapMethodsData[otherOffset + i])
                        {
                            isSameBootstrapMethod = false;
                            break;
                        }

                    if (isSameBootstrapMethod)
                    {
                        bootstrapMethods.length = offset;
                        // Revert to old position.
                        return entry;
                    }
                }

                entry = entry.next;
            }

            return Put(new Entry(bootstrapMethodCount++, Symbol.Bootstrap_Method_Tag
                , offset, hashCode));
        }

        // -----------------------------------------------------------------------------------------------
        // Type table entries management.
        // -----------------------------------------------------------------------------------------------
        /// <summary>Returns the type table element whose index is given.</summary>
        /// <param name="typeIndex">a type table index.</param>
        /// <returns>the type table element whose index is given.</returns>
        internal Symbol GetType(int typeIndex)
        {
            return typeTable[typeIndex];
        }

        /// <summary>Adds a type in the type table of this symbol table.</summary>
        /// <remarks>
        ///     Adds a type in the type table of this symbol table. Does nothing if the type table already
        ///     contains a similar type.
        /// </remarks>
        /// <param name="value">an internal class name.</param>
        /// <returns>
        ///     the index of a new or already existing type Symbol with the given value.
        /// </returns>
        internal int AddType(string value)
        {
            var hashCode = Hash(Symbol.Type_Tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Type_Tag && entry.hashCode == hashCode && entry.value.Equals
                        (value))
                    return entry.index;
                entry = entry.next;
            }

            return AddTypeInternal(new Entry(typeCount, Symbol.Type_Tag, value, hashCode
            ));
        }

        /// <summary>
        ///     Adds an
        ///     <see cref="Frame.Item_Uninitialized" />
        ///     type in the type table of this symbol table. Does
        ///     nothing if the type table already contains a similar type.
        /// </summary>
        /// <param name="value">an internal class name.</param>
        /// <param name="bytecodeOffset">
        ///     the bytecode offset of the NEW instruction that created this
        ///     <see cref="Frame.Item_Uninitialized" />
        ///     type value.
        /// </param>
        /// <returns>
        ///     the index of a new or already existing type Symbol with the given value.
        /// </returns>
        internal int AddUninitializedType(string value, int bytecodeOffset)
        {
            var hashCode = Hash(Symbol.Uninitialized_Type_Tag, value, bytecodeOffset);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Uninitialized_Type_Tag && entry.hashCode == hashCode && entry
                        .data == bytecodeOffset && entry.value.Equals(value))
                    return entry.index;
                entry = entry.next;
            }

            return AddTypeInternal(new Entry(typeCount, Symbol.Uninitialized_Type_Tag
                , value, bytecodeOffset, hashCode));
        }

        /// <summary>Adds a merged type in the type table of this symbol table.</summary>
        /// <remarks>
        ///     Adds a merged type in the type table of this symbol table. Does nothing if the type table
        ///     already contains a similar type.
        /// </remarks>
        /// <param name="typeTableIndex1">
        ///     a
        ///     <see cref="Symbol.Type_Tag" />
        ///     type, specified by its index in the type
        ///     table.
        /// </param>
        /// <param name="typeTableIndex2">
        ///     another
        ///     <see cref="Symbol.Type_Tag" />
        ///     type, specified by its index in the type
        ///     table.
        /// </param>
        /// <returns>
        ///     the index of a new or already existing
        ///     <see cref="Symbol.Type_Tag" />
        ///     type Symbol,
        ///     corresponding to the common super class of the given types.
        /// </returns>
        internal int AddMergedType(int typeTableIndex1, int typeTableIndex2)
        {
            var data = typeTableIndex1 < typeTableIndex2
                ? typeTableIndex1 | ((long) typeTableIndex2 << 32)
                : typeTableIndex2 | ((long) typeTableIndex1 << 32);
            var hashCode = Hash(Symbol.Merged_Type_Tag, typeTableIndex1 + typeTableIndex2);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Merged_Type_Tag && entry.hashCode == hashCode && entry.data
                    == data)
                    return entry.info;
                entry = entry.next;
            }

            var type1 = typeTable[typeTableIndex1].value;
            var type2 = typeTable[typeTableIndex2].value;
            var commonSuperTypeIndex = AddType(classWriter.GetCommonSuperClass(type1, type2));
            Put(new Entry(typeCount, Symbol.Merged_Type_Tag, data, hashCode)).info
                = commonSuperTypeIndex;
            return commonSuperTypeIndex;
        }

        /// <summary>
        ///     Adds the given type Symbol to
        ///     <see cref="typeTable" />
        ///     .
        /// </summary>
        /// <param name="entry">
        ///     a
        ///     <see cref="Symbol.Type_Tag" />
        ///     or
        ///     <see cref="Symbol.Uninitialized_Type_Tag" />
        ///     type symbol.
        ///     The index of this Symbol must be equal to the current value of
        ///     <see cref="typeCount" />
        ///     .
        /// </param>
        /// <returns>
        ///     the index in
        ///     <see cref="typeTable" />
        ///     where the given type was added, which is also equal to
        ///     entry's index by hypothesis.
        /// </returns>
        private int AddTypeInternal(Entry entry)
        {
            if (typeTable == null) typeTable = new Entry[16];
            if (typeCount == typeTable.Length)
            {
                var newTypeTable = new Entry[2 * typeTable.Length];
                Array.Copy(typeTable, 0, newTypeTable, 0, typeTable.Length);
                typeTable = newTypeTable;
            }

            typeTable[typeCount++] = entry;
            return Put(entry).index;
        }

        // -----------------------------------------------------------------------------------------------
        // Static helper methods to compute hash codes.
        // -----------------------------------------------------------------------------------------------
        private static int Hash(int tag, int value)
        {
            return 0x7FFFFFFF & (tag + value);
        }

        private static int Hash(int tag, long value)
        {
            return 0x7FFFFFFF & (tag + (int) value + (int) (long) ((ulong) value >> 32));
        }

        private static int Hash(int tag, string value)
        {
            return 0x7FFFFFFF & (tag + value.GetHashCode());
        }

        private static int Hash(int tag, string value1, int value2)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() + value2);
        }

        private static int Hash(int tag, string value1, string value2)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode
                                     ());
        }

        private static int Hash(int tag, string value1, string value2, int value3)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode
                                     () * (value3 + 1));
        }

        private static int Hash(int tag, string value1, string value2, string value3)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode
                                     () * value3.GetHashCode());
        }

        private static int Hash(int tag, string value1, string value2, string value3, int
            value4)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode
                                     () * value3.GetHashCode() * value4);
        }

        /// <summary>An entry of a SymbolTable.</summary>
        /// <remarks>
        ///     An entry of a SymbolTable. This concrete and private subclass of
        ///     <see cref="Symbol" />
        ///     adds two fields
        ///     which are only used inside SymbolTable, to implement hash sets of symbols (in order to avoid
        ///     duplicate symbols). See
        ///     <see cref="SymbolTable.entries" />
        ///     .
        /// </remarks>
        /// <author>Eric Bruneton</author>
        private class Entry : Symbol
        {
            /// <summary>The hash code of this entry.</summary>
            internal readonly int hashCode;

            /// <summary>
            ///     Another entry (and so on recursively) having the same hash code (modulo the size of
            ///     <see cref="SymbolTable.entries" />
            ///     ) as this one.
            /// </summary>
            internal Entry next;

            internal Entry(int index, int tag, string owner, string name, string value, long
                data, int hashCode)
                : base(index, tag, owner, name, value, data)
            {
                this.hashCode = hashCode;
            }

            internal Entry(int index, int tag, string value, int hashCode)
                : base(index, tag, null, null, value, 0)
            {
                /* owner = */
                /* name = */
                /* data = */
                this.hashCode = hashCode;
            }

            internal Entry(int index, int tag, string value, long data, int hashCode)
                : base(index, tag, null, null, value, data)
            {
                /* owner = */
                /* name = */
                this.hashCode = hashCode;
            }

            internal Entry(int index, int tag, string name, string value, int hashCode)
                : base(index, tag, null, name, value, 0)
            {
                /* owner = */
                /* data = */
                this.hashCode = hashCode;
            }

            internal Entry(int index, int tag, long data, int hashCode)
                : base(index, tag, null, null, null, data)
            {
                /* owner = */
                /* name = */
                /* value = */
                this.hashCode = hashCode;
            }
        }
    }
}