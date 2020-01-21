using System.Collections.Generic;
using System.Text;

namespace ObjectWeb.Asm.Util
{
	/// <summary>
	///     An
	///     <see cref="Attribute" />
	///     that can print a readable representation of itself.
	/// </summary>
	/// <author>Eugene Kuleshov</author>
	public interface TextifierSupport
    {
	    /// <summary>Generates a human readable representation of this attribute.</summary>
	    /// <param name="outputBuilder">
	    ///     where the human representation of this attribute must be appended.
	    /// </param>
	    /// <param name="labelNames">the human readable names of the labels.</param>
	    void Textify(StringBuilder outputBuilder, IDictionary<Label, string> labelNames);
    }
}