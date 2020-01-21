using System;

namespace ObjectWeb.Asm.Tree
{
	/// <summary>
	///     Exception thrown in
	///     <see cref="AnnotationNode.Check" />
	///     ,
	///     <see cref="ClassNode.Check(int)" />
	///     ,
	///     <see cref="FieldNode.Check(int)" />
	///     and
	///     <see cref="MethodNode.Check(int)" />
	///     when these nodes (or their children, recursively)
	///     contain elements that were introduced in more recent versions of the ASM API than version passed
	///     to these methods.
	/// </summary>
	/// <author>Eric Bruneton</author>
	[Serializable]
    public class UnsupportedClassVersionException : Exception
    {
        private const long serialVersionUID = -3502347765891805831L;
    }
}