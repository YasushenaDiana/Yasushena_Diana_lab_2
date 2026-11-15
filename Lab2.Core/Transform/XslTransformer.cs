using System.Xml.Xsl;

namespace Lab2.Core.Transform;

public sealed class XslTransformer
{
	public void Transform(string xmlPath, string xslPath, string htmlPath)
	{
		var xslt = new XslCompiledTransform();
		xslt.Load(xslPath);
		xslt.Transform(xmlPath, htmlPath);
	}
}
