using System.Xml.Linq;
using Lab2.Core.Model;

namespace Lab2.Core.Transform;

public sealed class XmlResultWriter
{
	public void Save(List<Scientist> scientists, string path)
	{
		var root = new XElement(XmlNames.Root);

		foreach (var scientist in scientists)
		{
			var element = new XElement(XmlNames.Scientist,
				Attribute(XmlNames.FullName, scientist.FullName),
				Attribute(XmlNames.Faculty, scientist.Faculty),
				Attribute(XmlNames.Department, scientist.Department),
				Attribute(XmlNames.Degree, scientist.Degree));

			var titles = new XElement(XmlNames.Titles);
			foreach (var title in scientist.Titles)
			{
				titles.Add(new XElement(XmlNames.Title,
					Attribute(XmlNames.TitleName, title.Name),
					Attribute(XmlNames.AssignedDate, title.AssignedDate?.ToString("yyyy-MM-dd"))));
			}

			element.Add(titles);
			root.Add(element);
		}

		new XDocument(root).Save(path);
	}

	private static XAttribute? Attribute(string name, string? value) =>
		string.IsNullOrEmpty(value) ? null : new XAttribute(name, value);
}
