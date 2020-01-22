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

namespace ObjectWeb.Asm.Util
{
    /// <summary>
    ///     A
    ///     <see cref="MethodVisitor" />
    ///     that prints the methods it visits with a
    ///     <see cref="Printer" />
    ///     .
    /// </summary>
    /// <author>Eric Bruneton</author>
    public sealed class TraceMethodVisitor : MethodVisitor
    {
        /// <summary>The printer to convert the visited method into text.</summary>
        public readonly Printer p;

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceMethodVisitor" />
        ///     .
        /// </summary>
        /// <param name="printer">the printer to convert the visited method into text.</param>
        public TraceMethodVisitor(Printer printer)
            : this(null, printer)
        {
        }

        /// <summary>
        ///     Constructs a new
        ///     <see cref="TraceMethodVisitor" />
        ///     .
        /// </summary>
        /// <param name="methodVisitor">
        ///     the method visitor to which to delegate calls. May be
        ///     <literal>null</literal>
        ///     .
        /// </param>
        /// <param name="printer">the printer to convert the visited method into text.</param>
        public TraceMethodVisitor(MethodVisitor methodVisitor, Printer printer)
            : base(VisitorAsmApiVersion.Asm7, methodVisitor)
        {
            // DontCheck(MemberName): can't be renamed (for backward binary compatibility).
            /* latest api = */
            p = printer;
        }

        public override void VisitParameter(string name, AccessFlags access)
        {
            p.VisitParameter(name, access);
            base.VisitParameter(name, access);
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible
        )
        {
            var annotationPrinter = p.VisitMethodAnnotation(descriptor, visible);
            return new TraceAnnotationVisitor(base.VisitAnnotation(descriptor, visible), annotationPrinter
            );
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var annotationPrinter = p.VisitMethodTypeAnnotation(typeRef, typePath, descriptor
                , visible);
            return new TraceAnnotationVisitor(base.VisitTypeAnnotation(typeRef, typePath, descriptor
                , visible), annotationPrinter);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            p.VisitMethodAttribute(attribute);
            base.VisitAttribute(attribute);
        }

        public override AnnotationVisitor VisitAnnotationDefault()
        {
            var annotationPrinter = p.VisitAnnotationDefault();
            return new TraceAnnotationVisitor(base.VisitAnnotationDefault(), annotationPrinter
            );
        }

        public override void VisitAnnotableParameterCount(int parameterCount, bool visible
        )
        {
            p.VisitAnnotableParameterCount(parameterCount, visible);
            base.VisitAnnotableParameterCount(parameterCount, visible);
        }

        public override AnnotationVisitor VisitParameterAnnotation(int parameter, string
            descriptor, bool visible)
        {
            var annotationPrinter = p.VisitParameterAnnotation(parameter, descriptor, visible
            );
            return new TraceAnnotationVisitor(base.VisitParameterAnnotation(parameter, descriptor
                , visible), annotationPrinter);
        }

        public override void VisitCode()
        {
            p.VisitCode();
            base.VisitCode();
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack
            , object[] stack)
        {
            p.VisitFrame(type, numLocal, local, numStack, stack);
            base.VisitFrame(type, numLocal, local, numStack, stack);
        }

        public override void VisitInsn(int opcode)
        {
            p.VisitInsn(opcode);
            base.VisitInsn(opcode);
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            p.VisitIntInsn(opcode, operand);
            base.VisitIntInsn(opcode, operand);
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            p.VisitVarInsn(opcode, var);
            base.VisitVarInsn(opcode, var);
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            p.VisitTypeInsn(opcode, type);
            base.VisitTypeInsn(opcode, type);
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string
            descriptor)
        {
            p.VisitFieldInsn(opcode, owner, name, descriptor);
            base.VisitFieldInsn(opcode, owner, name, descriptor);
        }

        public override void VisitMethodInsn(int opcode, string owner, string name, string
            descriptor, bool isInterface)
        {
            // Call the method that p is supposed to implement, depending on its api version.
            if (p.api < VisitorAsmApiVersion.Asm5)
            {
                if (isInterface != (opcode == OpcodesConstants.Invokeinterface))
                    throw new ArgumentException("INVOKESPECIAL/STATIC on interfaces require ASM5");
                // If p is an ASMifier (resp. Textifier), or a subclass that does not override the old
                // visitMethodInsn method, the default implementation in Printer will redirect this to the
                // new method in ASMifier (resp. Textifier). In all other cases, p overrides the old method
                // and this call executes it.
                p.VisitMethodInsn(opcode, owner, name, descriptor);
            }
            else
            {
                p.VisitMethodInsn(opcode, owner, name, descriptor, isInterface);
            }

            if (mv != null) mv.VisitMethodInsn(opcode, owner, name, descriptor, isInterface);
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle
            bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            p.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments
            );
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            p.VisitJumpInsn(opcode, label);
            base.VisitJumpInsn(opcode, label);
        }

        public override void VisitLabel(Label label)
        {
            p.VisitLabel(label);
            base.VisitLabel(label);
        }

        public override void VisitLdcInsn(object value)
        {
            p.VisitLdcInsn(value);
            base.VisitLdcInsn(value);
        }

        public override void VisitIincInsn(int var, int increment)
        {
            p.VisitIincInsn(var, increment);
            base.VisitIincInsn(var, increment);
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label
            [] labels)
        {
            p.VisitTableSwitchInsn(min, max, dflt, labels);
            base.VisitTableSwitchInsn(min, max, dflt, labels);
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels
        )
        {
            p.VisitLookupSwitchInsn(dflt, keys, labels);
            base.VisitLookupSwitchInsn(dflt, keys, labels);
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions
        )
        {
            p.VisitMultiANewArrayInsn(descriptor, numDimensions);
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
        }

        public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var annotationPrinter = p.VisitInsnAnnotation(typeRef, typePath, descriptor,
                visible);
            return new TraceAnnotationVisitor(base.VisitInsnAnnotation(typeRef, typePath, descriptor
                , visible), annotationPrinter);
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string
            type)
        {
            p.VisitTryCatchBlock(start, end, handler, type);
            base.VisitTryCatchBlock(start, end, handler, type);
        }

        public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath
            , string descriptor, bool visible)
        {
            var annotationPrinter = p.VisitTryCatchAnnotation(typeRef, typePath, descriptor
                , visible);
            return new TraceAnnotationVisitor(base.VisitTryCatchAnnotation(typeRef, typePath,
                descriptor, visible), annotationPrinter);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature
            , Label start, Label end, int index)
        {
            p.VisitLocalVariable(name, descriptor, signature, start, end, index);
            base.VisitLocalVariable(name, descriptor, signature, start, end, index);
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath
                typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible
        )
        {
            var annotationPrinter = p.VisitLocalVariableAnnotation(typeRef, typePath, start
                , end, index, descriptor, visible);
            return new TraceAnnotationVisitor(base.VisitLocalVariableAnnotation(typeRef, typePath
                , start, end, index, descriptor, visible), annotationPrinter);
        }

        public override void VisitLineNumber(int line, Label start)
        {
            p.VisitLineNumber(line, start);
            base.VisitLineNumber(line, start);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            p.VisitMaxs(maxStack, maxLocals);
            base.VisitMaxs(maxStack, maxLocals);
        }

        public override void VisitEnd()
        {
            p.VisitMethodEnd();
            base.VisitEnd();
        }
    }
}