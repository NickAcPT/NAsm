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

namespace ObjectWeb.Asm
{
	/// <summary>A visitor to visit a Java class.</summary>
	/// <remarks>
	///     A visitor to visit a Java class. The methods of this class must be called in the following order:
	///     <c>visit</c>
	///     [
	///     <c>visitSource</c>
	///     ] [
	///     <c>visitModule</c>
	///     ][
	///     <c>visitNestHost</c>
	///     ][
	///     <c>visitPermittedSubtype</c>
	///     ][
	///     <c>visitOuterClass</c>
	///     ] (
	///     <c>visitAnnotation</c>
	///     |
	///     <c>visitTypeAnnotation</c>
	///     |
	///     <c>visitAttribute</c>
	///     )* (
	///     <c>visitNestMember</c>
	///     |
	///     <c>visitInnerClass</c>
	///     |
	///     <c>visitField</c>
	///     |
	///     <c>visitMethod</c>
	///     )*
	///     <c>visitEnd</c>
	///     .
	/// </remarks>
	/// <author>Eric Bruneton</author>
	public abstract class ClassVisitor
    {
	    /// <summary>The ASM API version implemented by this visitor.</summary>
	    /// <remarks>
	    ///     The ASM API version implemented by this visitor. The value of this field must be one of
	    ///     <see cref="Opcodes.Asm4" />
	    ///     ,
	    ///     <see cref="Opcodes.Asm5" />
	    ///     ,
	    ///     <see cref="Opcodes.Asm6" />
	    ///     or
	    ///     <see cref="Opcodes.Asm7" />
	    ///     .
	    /// </remarks>
	    protected internal readonly VisitorAsmApiVersion api;

	    /// <summary>The class visitor to which this visitor must delegate method calls.</summary>
	    /// <remarks>
	    ///     The class visitor to which this visitor must delegate method calls. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </remarks>
	    protected internal ClassVisitor cv;

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="ClassVisitor" />
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
	    public ClassVisitor(VisitorAsmApiVersion api)
            : this(api, null)
        {
        }

	    /// <summary>
	    ///     Constructs a new
	    ///     <see cref="ClassVisitor" />
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
	    /// <param name="classVisitor">
	    ///     the class visitor to which this visitor must delegate method calls. May be
	    ///     null.
	    /// </param>
	    public ClassVisitor(VisitorAsmApiVersion api, ClassVisitor classVisitor)
        {
            if (api != VisitorAsmApiVersion.Asm7 && api != VisitorAsmApiVersion.Asm6 && api != VisitorAsmApiVersion
                    .Asm5 && api != VisitorAsmApiVersion.Asm4 && api != VisitorAsmApiVersion.Asm8Experimental)
                throw new ArgumentException("Unsupported api " + api);
            if (api == VisitorAsmApiVersion.Asm8Experimental) Constants.CheckAsm8Experimental(this);
            this.api = api;
            cv = classVisitor;
        }

	    /// <summary>Visits the header of the class.</summary>
	    /// <param name="version">
	    ///     the class version. The minor version is stored in the 16 most significant bits,
	    ///     and the major version in the 16 least significant bits.
	    /// </param>
	    /// <param name="access">
	    ///     the class's access flags (see
	    ///     <see cref="Opcodes" />
	    ///     ). This parameter also indicates if
	    ///     the class is deprecated.
	    /// </param>
	    /// <param name="name">
	    ///     the internal name of the class (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ).
	    /// </param>
	    /// <param name="signature">
	    ///     the signature of this class. May be
	    ///     <literal>null</literal>
	    ///     if the class is not a
	    ///     generic one, and does not extend or implement generic classes or interfaces.
	    /// </param>
	    /// <param name="superName">
	    ///     the internal of name of the super class (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ).
	    ///     For interfaces, the super class is
	    ///     <see cref="object" />
	    ///     . May be
	    ///     <literal>null</literal>
	    ///     , but only for the
	    ///     <see cref="object" />
	    ///     class.
	    /// </param>
	    /// <param name="interfaces">
	    ///     the internal names of the class's interfaces (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ). May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    public virtual void Visit(int version, AccessFlags access, string name, string signature,
            string superName, string[] interfaces)
        {
            if (cv != null) cv.Visit(version, access, name, signature, superName, interfaces);
        }

	    /// <summary>Visits the source of the class.</summary>
	    /// <param name="source">
	    ///     the name of the source file from which the class was compiled. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    /// <param name="debug">
	    ///     additional debug information to compute the correspondence between source and
	    ///     compiled elements of the class. May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    public virtual void VisitSource(string source, string debug)
        {
            if (cv != null) cv.VisitSource(source, debug);
        }

	    /// <summary>Visit the module corresponding to the class.</summary>
	    /// <param name="name">the fully qualified name (using dots) of the module.</param>
	    /// <param name="access">
	    ///     the module access flags, among
	    ///     <c>ACC_OPEN</c>
	    ///     ,
	    ///     <c>ACC_SYNTHETIC</c>
	    ///     and
	    ///     <c>ACC_MANDATED</c>
	    ///     .
	    /// </param>
	    /// <param name="version">
	    ///     the module version, or
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit the module values, or
	    ///     <literal>null</literal>
	    ///     if this visitor is not
	    ///     interested in visiting this module.
	    /// </returns>
	    public virtual ModuleVisitor VisitModule(string name, AccessFlags access, string version)
        {
            if (api < VisitorAsmApiVersion.Asm6) throw new NotSupportedException("This feature requires ASM6");
            if (cv != null) return cv.VisitModule(name, access, version);
            return null;
        }

	    /// <summary>Visits the nest host class of the class.</summary>
	    /// <remarks>
	    ///     Visits the nest host class of the class. A nest is a set of classes of the same package that
	    ///     share access to their private members. One of these classes, called the host, lists the other
	    ///     members of the nest, which in turn should link to the host of their nest. This method must be
	    ///     called only once and only if the visited class is a non-host member of a nest. A class is
	    ///     implicitly its own nest, so it's invalid to call this method with the visited class name as
	    ///     argument.
	    /// </remarks>
	    /// <param name="nestHost">the internal name of the host class of the nest.</param>
	    public virtual void VisitNestHost(string nestHost)
        {
            if (api < VisitorAsmApiVersion.Asm7) throw new NotSupportedException("This feature requires ASM7");
            if (cv != null) cv.VisitNestHost(nestHost);
        }

	    /// <summary>Visits the enclosing class of the class.</summary>
	    /// <remarks>
	    ///     Visits the enclosing class of the class. This method must be called only if the class has an
	    ///     enclosing class.
	    /// </remarks>
	    /// <param name="owner">internal name of the enclosing class of the class.</param>
	    /// <param name="name">
	    ///     the name of the method that contains the class, or
	    ///     <literal>null</literal>
	    ///     if the class is
	    ///     not enclosed in a method of its enclosing class.
	    /// </param>
	    /// <param name="descriptor">
	    ///     the descriptor of the method that contains the class, or
	    ///     <literal>null</literal>
	    ///     if
	    ///     the class is not enclosed in a method of its enclosing class.
	    /// </param>
	    public virtual void VisitOuterClass(string owner, string name, string descriptor)
        {
            if (cv != null) cv.VisitOuterClass(owner, name, descriptor);
        }

	    /// <summary>Visits an annotation of the class.</summary>
	    /// <param name="descriptor">the class descriptor of the annotation class.</param>
	    /// <param name="visible">
	    ///     <literal>true</literal>
	    ///     if the annotation is visible at runtime.
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit the annotation values, or
	    ///     <literal>null</literal>
	    ///     if this visitor is not
	    ///     interested in visiting this annotation.
	    /// </returns>
	    public virtual AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            if (cv != null) return cv.VisitAnnotation(descriptor, visible);
            return null;
        }

	    /// <summary>Visits an annotation on a type in the class signature.</summary>
	    /// <param name="typeRef">
	    ///     a reference to the annotated type. The sort of this type reference must be
	    ///     <see cref="TypeReference.Class_Type_Parameter" />
	    ///     ,
	    ///     <see cref="TypeReference.Class_Type_Parameter_Bound" />
	    ///     or
	    ///     <see cref="TypeReference.Class_Extends" />
	    ///     . See
	    ///     <see cref="TypeReference" />
	    ///     .
	    /// </param>
	    /// <param name="typePath">
	    ///     the path to the annotated type argument, wildcard bound, array element type, or
	    ///     static inner type within 'typeRef'. May be
	    ///     <literal>null</literal>
	    ///     if the annotation targets
	    ///     'typeRef' as a whole.
	    /// </param>
	    /// <param name="descriptor">the class descriptor of the annotation class.</param>
	    /// <param name="visible">
	    ///     <literal>true</literal>
	    ///     if the annotation is visible at runtime.
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit the annotation values, or
	    ///     <literal>null</literal>
	    ///     if this visitor is not
	    ///     interested in visiting this annotation.
	    /// </returns>
	    public virtual AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            if (api < VisitorAsmApiVersion.Asm5) throw new NotSupportedException("This feature requires ASM5");
            if (cv != null) return cv.VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
            return null;
        }

	    /// <summary>Visits a non standard attribute of the class.</summary>
	    /// <param name="attribute">an attribute.</param>
	    public virtual void VisitAttribute(Attribute attribute)
        {
            if (cv != null) cv.VisitAttribute(attribute);
        }

	    /// <summary>Visits a member of the nest.</summary>
	    /// <remarks>
	    ///     Visits a member of the nest. A nest is a set of classes of the same package that share access
	    ///     to their private members. One of these classes, called the host, lists the other members of the
	    ///     nest, which in turn should link to the host of their nest. This method must be called only if
	    ///     the visited class is the host of a nest. A nest host is implicitly a member of its own nest, so
	    ///     it's invalid to call this method with the visited class name as argument.
	    /// </remarks>
	    /// <param name="nestMember">the internal name of a nest member.</param>
	    public virtual void VisitNestMember(string nestMember)
        {
            if (api < VisitorAsmApiVersion.Asm7) throw new NotSupportedException("This feature requires ASM7");
            if (cv != null) cv.VisitNestMember(nestMember);
        }

	    /// <summary>
	    ///     <b>Experimental, use at your own risk.
	    /// </summary>
	    /// <remarks>
	    ///     <b>
	    ///         Experimental, use at your own risk. This method will be renamed when it becomes stable, this
	    ///         will break existing code using it
	    ///     </b>
	    ///     . Visits a permitted subtypes. A permitted subtypes is one
	    ///     of the allowed subtypes of the current class.
	    /// </remarks>
	    /// <param name="permittedSubtype">the internal name of a permitted subtype.</param>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual void VisitPermittedSubtypeExperimental(string permittedSubtype)
        {
            if (api != VisitorAsmApiVersion.Asm8Experimental)
                throw new NotSupportedException("This feature requires ASM8_EXPERIMENTAL");
            if (cv != null) cv.VisitPermittedSubtypeExperimental(permittedSubtype);
        }

	    /// <summary>Visits information about an inner class.</summary>
	    /// <remarks>
	    ///     Visits information about an inner class. This inner class is not necessarily a member of the
	    ///     class being visited.
	    /// </remarks>
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
	    ///     for not member classes.
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
	    public virtual void VisitInnerClass(string name, string outerName, string innerName
            , AccessFlags access)
        {
            if (cv != null) cv.VisitInnerClass(name, outerName, innerName, access);
        }

	    /// <summary>Visits a record component of the class.</summary>
	    /// <param name="access">
	    ///     the record component access flags, the only possible value is
	    ///     <see cref="Opcodes.Acc_Deprecated" />
	    ///     .
	    /// </param>
	    /// <param name="name">the record component name.</param>
	    /// <param name="descriptor">
	    ///     the record component descriptor (see
	    ///     <see cref="Type" />
	    ///     ).
	    /// </param>
	    /// <param name="signature">
	    ///     the record component signature. May be
	    ///     <literal>null</literal>
	    ///     if the record component
	    ///     type does not use generic types.
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit this record component annotations and attributes, or
	    ///     <literal>null</literal>
	    ///     if this class visitor is not interested in visiting these annotations and attributes.
	    /// </returns>
	    //  [Obsolete(@"this API is experimental.")]
        public virtual RecordComponentVisitor VisitRecordComponentExperimental(AccessFlags access
            , string name, string descriptor, string signature)
        {
            if (api < VisitorAsmApiVersion.Asm8Experimental)
                throw new NotSupportedException("This feature requires ASM8_EXPERIMENTAL");
            if (cv != null) return cv.VisitRecordComponentExperimental(access, name, descriptor, signature);
            return null;
        }

	    /// <summary>Visits a field of the class.</summary>
	    /// <param name="access">
	    ///     the field's access flags (see
	    ///     <see cref="Opcodes" />
	    ///     ). This parameter also indicates if
	    ///     the field is synthetic and/or deprecated.
	    /// </param>
	    /// <param name="name">the field's name.</param>
	    /// <param name="descriptor">
	    ///     the field's descriptor (see
	    ///     <see cref="Type" />
	    ///     ).
	    /// </param>
	    /// <param name="signature">
	    ///     the field's signature. May be
	    ///     <literal>null</literal>
	    ///     if the field's type does not use
	    ///     generic types.
	    /// </param>
	    /// <param name="value">
	    ///     the field's initial value. This parameter, which may be
	    ///     <literal>null</literal>
	    ///     if the
	    ///     field does not have an initial value, must be an
	    ///     <see cref="int" />
	    ///     , a
	    ///     <see cref="float" />
	    ///     , a
	    ///     <see cref="long" />
	    ///     , a
	    ///     <see cref="double" />
	    ///     or a
	    ///     <see cref="string" />
	    ///     (for
	    ///     <c>int</c>
	    ///     ,
	    ///     <c>float</c>
	    ///     ,
	    ///     <c>long</c>
	    ///     or
	    ///     <c>String</c>
	    ///     fields respectively).
	    ///     <i>
	    ///         This parameter is only used for static
	    ///         fields
	    ///     </i>
	    ///     . Its value is ignored for non static fields, which must be initialized through
	    ///     bytecode instructions in constructors or methods.
	    /// </param>
	    /// <returns>
	    ///     a visitor to visit field annotations and attributes, or
	    ///     <literal>null</literal>
	    ///     if this class
	    ///     visitor is not interested in visiting these annotations and attributes.
	    /// </returns>
	    public virtual FieldVisitor VisitField(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor
            , string signature, object value)
        {
            if (cv != null) return cv.VisitField(access, name, descriptor, signature, value);
            return null;
        }

	    /// <summary>Visits a method of the class.</summary>
	    /// <remarks>
	    ///     Visits a method of the class. This method <i>must</i> return a new
	    ///     <see cref="MethodVisitor" />
	    ///     instance (or
	    ///     <literal>null</literal>
	    ///     ) each time it is called, i.e., it should not return a previously
	    ///     returned visitor.
	    /// </remarks>
	    /// <param name="access">
	    ///     the method's access flags (see
	    ///     <see cref="Opcodes" />
	    ///     ). This parameter also indicates if
	    ///     the method is synthetic and/or deprecated.
	    /// </param>
	    /// <param name="name">the method's name.</param>
	    /// <param name="descriptor">
	    ///     the method's descriptor (see
	    ///     <see cref="Type" />
	    ///     ).
	    /// </param>
	    /// <param name="signature">
	    ///     the method's signature. May be
	    ///     <literal>null</literal>
	    ///     if the method parameters,
	    ///     return type and exceptions do not use generic types.
	    /// </param>
	    /// <param name="exceptions">
	    ///     the internal names of the method's exception classes (see
	    ///     <see cref="Type.GetInternalName()" />
	    ///     ). May be
	    ///     <literal>null</literal>
	    ///     .
	    /// </param>
	    /// <returns>
	    ///     an object to visit the byte code of the method, or
	    ///     <literal>null</literal>
	    ///     if this class
	    ///     visitor is not interested in visiting the code of this method.
	    /// </returns>
	    public virtual MethodVisitor VisitMethod(ObjectWeb.Asm.Enums.AccessFlags access, string name, string descriptor
            , string signature, string[] exceptions)
        {
            if (cv != null) return cv.VisitMethod(access, name, descriptor, signature, exceptions);
            return null;
        }

	    /// <summary>Visits the end of the class.</summary>
	    /// <remarks>
	    ///     Visits the end of the class. This method, which is the last one to be called, is used to inform
	    ///     the visitor that all the fields and methods of the class have been visited.
	    /// </remarks>
	    public virtual void VisitEnd()
        {
            if (cv != null) cv.VisitEnd();
        }
    }
}