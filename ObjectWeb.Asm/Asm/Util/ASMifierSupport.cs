using System.Collections.Generic;
using System.Text;

namespace ObjectWeb.Asm.Util
{
	/// <summary>
	///     An
	///     <see cref="Attribute" />
	///     that can generate the ASM code to create an equivalent
	///     attribute.
	/// </summary>
	/// <author>Eugene Kuleshov</author>
	public interface ASMifierSupport
    {
        // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
        /// <summary>Generates the ASM code to create an attribute equal to this attribute.</summary>
        /// <param name="outputBuilder">where the generated code must be appended.</param>
        /// <param name="visitorVariableName">
        ///     the name of the visitor variable in the produced code.
        /// </param>
        /// <param name="labelNames">the names of the labels in the generated code.</param>
        void Asmify(StringBuilder outputBuilder, string visitorVariableName, IDictionary<
            Label, string> labelNames);
    }
}