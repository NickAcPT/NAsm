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

namespace ObjectWeb.Asm.Tree
{
    /// <summary>A node that represents a class.</summary>
    /// <author>Eric Bruneton</author>
    public class ClassNode : ClassVisitor
    {
        /// <summary>
        ///     The class's access flags (see
        ///     <see cref="Opcodes" />
        ///     ). This field also indicates if
        ///     the class is deprecated.
        /// </summary>
        public int access;

        /// <summary>The non standard attributes of this class.</summary>
        /// <remarks>
        ///     The non standard attributes of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<Attribute> attrs;

        /// <summary>The fields of this class.</summary>
        public List<FieldNode> fields;

        /// <summary>The inner classes of this class.</summary>
        public List<InnerClassNode> innerClasses;

        /// <summary>
        ///     The internal names of the interfaces directly implemented by this class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        /// </summary>
        public List<string> interfaces;

        /// <summary>The runtime invisible annotations of this class.</summary>
        /// <remarks>
        ///     The runtime invisible annotations of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<AnnotationNode> invisibleAnnotations;

        /// <summary>The runtime invisible type annotations of this class.</summary>
        /// <remarks>
        ///     The runtime invisible type annotations of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<TypeAnnotationNode> invisibleTypeAnnotations;

        /// <summary>The methods of this class.</summary>
        public List<MethodNode> methods;

        /// <summary>The module stored in this class.</summary>
        /// <remarks>
        ///     The module stored in this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public ModuleNode module;

        /// <summary>
        ///     The internal name of this class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        /// </summary>
        public string name;

        /// <summary>The internal name of the nest host class of this class.</summary>
        /// <remarks>
        ///     The internal name of the nest host class of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string nestHostClass;

        /// <summary>The internal names of the nest members of this class.</summary>
        /// <remarks>
        ///     The internal names of the nest members of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<string> nestMembers;

        /// <summary>The internal name of the enclosing class of this class.</summary>
        /// <remarks>
        ///     The internal name of the enclosing class of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string outerClass;

        /// <summary>
        ///     The name of the method that contains this class, or
        ///     <literal>null</literal>
        ///     if this class is not
        ///     enclosed in a method.
        /// </summary>
        public string outerMethod;

        /// <summary>
        ///     The descriptor of the method that contains this class, or
        ///     <literal>null</literal>
        ///     if this class is not
        ///     enclosed in a method.
        /// </summary>
        public string outerMethodDesc;

        /// <summary>
        ///     <b>Experimental, use at your own risk.
        /// </summary>
        /// <remarks>
        ///     <b>
        ///         Experimental, use at your own risk. This method will be renamed when it becomes stable, this
        ///         will break existing code using it
        ///     </b>
        ///     . The internal names of the permitted subtypes of this
        ///     class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        //  [Obsolete(@"this API is experimental.")]
        public List<string> permittedSubtypesExperimental;

        /// <summary>The record components of this class.</summary>
        /// <remarks>
        ///     The record components of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        //  [Obsolete(@"this API is experimental.")]
        public List<RecordComponentNode> recordComponentsExperimental;

        /// <summary>The signature of this class.</summary>
        /// <remarks>
        ///     The signature of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string signature;

        /// <summary>The correspondence between source and compiled elements of this class.</summary>
        /// <remarks>
        ///     The correspondence between source and compiled elements of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string sourceDebug;

        /// <summary>The name of the source file from which this class was compiled.</summary>
        /// <remarks>
        ///     The name of the source file from which this class was compiled. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public string sourceFile;

        /// <summary>
        ///     The internal of name of the super class (see
        ///     <see cref="Type.GetInternalName()" />
        ///     ).
        ///     For interfaces, the super class is
        ///     <see cref="object" />
        ///     . May be
        ///     <literal>null</literal>
        ///     , but only for the
        ///     <see cref="object" />
        ///     class.
        /// </summary>
        public string superName;

        /// <summary>The class version.</summary>
        /// <remarks>
        ///     The class version. The minor version is stored in the 16 most significant bits, and the major
        ///     version in the 16 least significant bits.
        /// </remarks>
        public int version;

        /// <summary>The runtime visible annotations of this class.</summary>
        /// <remarks>
        ///     The runtime visible annotations of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<AnnotationNode> visibleAnnotations;

        /// <summary>The runtime visible type annotations of this class.</summary>
        /// <remarks>
        ///     The runtime visible type annotations of this class. May be
        ///     <literal>null</literal>
        ///     .
        /// </remarks>
        public List<TypeAnnotationNode> visibleTypeAnnotations;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ClassNode" />
        ///     . <i>Subclasses must not use this constructor</i>. Instead,
        ///     they must use the
        ///     <see cref="ClassNode(int)" />
        ///     version.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     If a subclass calls this constructor.
        /// </exception>
        public ClassNode()
            : this(VisitorAsmApiVersion.Asm7)
        {
            if (GetType() != typeof(ClassNode)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="ClassNode" />
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
        public ClassNode(VisitorAsmApiVersion api)
            : base(api)
        {
            interfaces = new List<string>();
            innerClasses = new List<InnerClassNode>();
            fields = new List<FieldNode>();
            methods = new List<MethodNode>();
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the ClassVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override void Visit(int version, int access, string name, string signature
            , string superName, string[] interfaces)
        {
            this.version = version;
            this.access = access;
            this.name = name;
            this.signature = signature;
            this.superName = superName;
            this.interfaces = Util.AsArrayList(interfaces);
        }

        public override void VisitSource(string file, string debug)
        {
            sourceFile = file;
            sourceDebug = debug;
        }

        public override ModuleVisitor VisitModule(string name, int access, string version
        )
        {
            module = new ModuleNode(name, access, version);
            return module;
        }

        public override void VisitNestHost(string nestHost)
        {
            nestHostClass = nestHost;
        }

        public override void VisitOuterClass(string owner, string name, string descriptor
        )
        {
            outerClass = owner;
            outerMethod = name;
            outerMethodDesc = descriptor;
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            var annotation = new AnnotationNode(descriptor);
            if (visible)
                visibleAnnotations = Util.Add(visibleAnnotations, annotation);
            else
                invisibleAnnotations = Util.Add(invisibleAnnotations, annotation);
            return annotation;
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor
            );
            if (visible)
                visibleTypeAnnotations = Util.Add(visibleTypeAnnotations, typeAnnotation);
            else
                invisibleTypeAnnotations = Util.Add(invisibleTypeAnnotations, typeAnnotation);
            return typeAnnotation;
        }

        public override void VisitAttribute(Attribute attribute)
        {
            attrs = Util.Add(attrs, attribute);
        }

        public override void VisitNestMember(string nestMember)
        {
            nestMembers = Util.Add(nestMembers, nestMember);
        }

        public override void VisitPermittedSubtypeExperimental(string permittedSubtype)
        {
            permittedSubtypesExperimental = Util.Add(permittedSubtypesExperimental, permittedSubtype
            );
        }

        public override void VisitInnerClass(string name, string outerName, string innerName
            , int access)
        {
            var innerClass = new InnerClassNode(name, outerName, innerName, access
            );
            innerClasses.Add(innerClass);
        }

        public override RecordComponentVisitor VisitRecordComponentExperimental(int access
            , string name, string descriptor, string signature)
        {
            var recordComponent = new RecordComponentNode(access, name, descriptor
                , signature);
            recordComponentsExperimental = Util.Add(recordComponentsExperimental, recordComponent
            );
            return recordComponent;
        }

        public override FieldVisitor VisitField(int access, string name, string descriptor
            , string signature, object value)
        {
            var field = new FieldNode(access, name, descriptor, signature, value);
            fields.Add(field);
            return field;
        }

        public override MethodVisitor VisitMethod(int access, string name, string descriptor
            , string signature, string[] exceptions)
        {
            var method = new MethodNode(access, name, descriptor, signature, exceptions
            );
            methods.Add(method);
            return method;
        }

        public override void VisitEnd()
        {
        }

        // Nothing to do.
        // -----------------------------------------------------------------------------------------------
        // Accept method
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Checks that this class node is compatible with the given ASM API version.
        /// </summary>
        /// <remarks>
        ///     Checks that this class node is compatible with the given ASM API version. This method checks
        ///     that this node, and all its children recursively, do not contain elements that were introduced
        ///     in more recent versions of the ASM API than the given version.
        /// </remarks>
        /// <param name="api">
        ///     an ASM API version. Must be one of
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm4" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm5" />
        ///     ,
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm6" />
        ///     or
        ///     <see cref="Org.Objectweb.Asm.Opcodes.Asm7" />
        ///     .
        /// </param>
        public virtual void Check(VisitorAsmApiVersion api)
        {
            if (api != VisitorAsmApiVersion.Asm8Experimental && permittedSubtypesExperimental !=
                null)
                throw new UnsupportedClassVersionException();
            if (api != VisitorAsmApiVersion.Asm8Experimental && recordComponentsExperimental !=
                null)
                throw new UnsupportedClassVersionException();
            if (api < VisitorAsmApiVersion.Asm7 && (nestHostClass != null || nestMembers != null))
                throw new UnsupportedClassVersionException();
            if (api < VisitorAsmApiVersion.Asm6 && module != null) throw new UnsupportedClassVersionException();
            if (api < VisitorAsmApiVersion.Asm5)
            {
                if (visibleTypeAnnotations != null && !(visibleTypeAnnotations.Count == 0))
                    throw new UnsupportedClassVersionException();
                if (invisibleTypeAnnotations != null && !(invisibleTypeAnnotations.Count == 0))
                    throw new UnsupportedClassVersionException();
            }

            // Check the annotations.
            if (visibleAnnotations != null)
                for (var i = visibleAnnotations.Count - 1; i >= 0; --i)
                    visibleAnnotations[i].Check(api);
            if (invisibleAnnotations != null)
                for (var i = invisibleAnnotations.Count - 1; i >= 0; --i)
                    invisibleAnnotations[i].Check(api);
            if (visibleTypeAnnotations != null)
                for (var i = visibleTypeAnnotations.Count - 1; i >= 0; --i)
                    visibleTypeAnnotations[i].Check(api);
            if (invisibleTypeAnnotations != null)
                for (var i = invisibleTypeAnnotations.Count - 1; i >= 0; --i)
                    invisibleTypeAnnotations[i].Check(api);
            if (recordComponentsExperimental != null)
                for (var i = recordComponentsExperimental.Count - 1; i >= 0; --i)
                    recordComponentsExperimental[i].CheckExperimental(api);
            for (var i = fields.Count - 1; i >= 0; --i) fields[i].Check(api);
            for (var i = methods.Count - 1; i >= 0; --i) methods[i].Check(api);
        }

        /// <summary>Makes the given class visitor visit this class.</summary>
        /// <param name="classVisitor">a class visitor.</param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            // Visit the header.
            var interfacesArray = new string[interfaces.Count];
            Collections.ToArray(interfaces, interfacesArray);
            classVisitor.Visit(version, access, name, signature, superName, interfacesArray);
            // Visit the source.
            if (sourceFile != null || sourceDebug != null) classVisitor.VisitSource(sourceFile, sourceDebug);
            // Visit the module.
            if (module != null) module.Accept(classVisitor);
            // Visit the nest host class.
            if (nestHostClass != null) classVisitor.VisitNestHost(nestHostClass);
            // Visit the outer class.
            if (outerClass != null) classVisitor.VisitOuterClass(outerClass, outerMethod, outerMethodDesc);
            // Visit the annotations.
            if (visibleAnnotations != null)
                for (int i = 0, n = visibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = visibleAnnotations[i];
                    annotation.Accept(classVisitor.VisitAnnotation(annotation.desc, true));
                }

            if (invisibleAnnotations != null)
                for (int i = 0, n = invisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = invisibleAnnotations[i];
                    annotation.Accept(classVisitor.VisitAnnotation(annotation.desc, false));
                }

            if (visibleTypeAnnotations != null)
                for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = visibleTypeAnnotations[i];
                    typeAnnotation.Accept(classVisitor.VisitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation
                        .typePath, typeAnnotation.desc, true));
                }

            if (invisibleTypeAnnotations != null)
                for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = invisibleTypeAnnotations[i];
                    typeAnnotation.Accept(classVisitor.VisitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation
                        .typePath, typeAnnotation.desc, false));
                }

            // Visit the non standard attributes.
            if (attrs != null)
                for (int i = 0, n = attrs.Count; i < n; ++i)
                    classVisitor.VisitAttribute(attrs[i]);
            // Visit the nest members.
            if (nestMembers != null)
                for (int i = 0, n = nestMembers.Count; i < n; ++i)
                    classVisitor.VisitNestMember(nestMembers[i]);
            // Visit the permitted subtypes.
            if (permittedSubtypesExperimental != null)
                for (int i = 0, n = permittedSubtypesExperimental.Count; i < n; ++i)
                    classVisitor.VisitPermittedSubtypeExperimental(permittedSubtypesExperimental[i]);
            // Visit the inner classes.
            for (int i = 0, n = innerClasses.Count; i < n; ++i) innerClasses[i].Accept(classVisitor);
            // Visit the record components.
            if (recordComponentsExperimental != null)
                for (int i = 0, n = recordComponentsExperimental.Count; i < n; ++i)
                    recordComponentsExperimental[i].AcceptExperimental(classVisitor);
            // Visit the fields.
            for (int i = 0, n = fields.Count; i < n; ++i) fields[i].Accept(classVisitor);
            // Visit the methods.
            for (int i = 0, n = methods.Count; i < n; ++i) methods[i].Accept(classVisitor);
            classVisitor.VisitEnd();
        }
    }
}