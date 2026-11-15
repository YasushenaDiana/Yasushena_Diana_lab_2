using System.Xml.Linq;
using Lab2.Core.Model;

namespace Lab2.Core.Reading;

public sealed class XmlAttributeReader
{
	public Dictionary<string, List<string>> GetAttributeValues(string path)
	{
		XDocument document;
		using (var reader = XmlUtils.CreateReader(path))
		{
			document = XDocument.Load(reader);
		}

		if (document.Root?.Name != XmlNames.Root)
		{
			throw new InvalidDataException(
				$"Файл не містить даних про науковців (очікується кореневий елемент <{XmlNames.Root}>).");
		}

		var scientists = document.Root.Elements(XmlNames.Scientist).ToList();
		if (scientists.Count == 0)
		{
			throw new InvalidDataException($"У файлі немає жодного елемента <{XmlNames.Scientist}>.");
		}

		var values = new Dictionary<string, List<string>>();
		foreach (var attribute in new[] { XmlNames.FullName, XmlNames.Faculty, XmlNames.Department, XmlNames.Degree })
		{
			values[attribute] = Distinct(scientists.Select(s => (string?)s.Attribute(attribute)));
		}

		values[XmlNames.Title] = Distinct(scientists
			.Elements(XmlNames.Titles)
			.Elements(XmlNames.Title)
			.Select(t => (string?)t.Attribute(XmlNames.TitleName)));

		return values;
	}

	private static List<string> Distinct(IEnumerable<string?> values) =>
		values
			.Select(XmlUtils.Normalize)
			.Where(v => v.Length > 0)
			.Distinct()
			.Order(StringComparer.Create(new System.Globalization.CultureInfo("uk-UA"), false))
			.ToList();
}
