using System.Text;
using System.Xml;
using Lab2.Core.Model;

namespace Lab2.Core.Search;

public sealed class DomSearchStrategy : ISearchStrategy
{
	public List<Scientist> Search(string path, SearchCriteria criteria)
	{
		var document = new XmlDocument();
		using (var reader = XmlUtils.CreateReader(path))
		{
			document.Load(reader);
		}

		var nodes = document.SelectNodes(BuildXPath(criteria));
		var results = new List<Scientist>();
		if (nodes is null)
		{
			return results;
		}

		foreach (XmlNode node in nodes)
		{
			results.Add(ToScientist((XmlElement)node));
		}

		return results;
	}

	public static string BuildXPath(SearchCriteria criteria)
	{
		var xpath = new StringBuilder($"/{XmlNames.Root}/{XmlNames.Scientist}");
		AppendCondition(xpath, XmlNames.FullName, criteria.FullName);
		AppendCondition(xpath, XmlNames.Faculty, criteria.Faculty);
		AppendCondition(xpath, XmlNames.Department, criteria.Department);
		AppendCondition(xpath, XmlNames.Degree, criteria.Degree);

		if (criteria.Title.Length > 0)
		{
			xpath.Append($"[{XmlNames.Titles}/{XmlNames.Title}");
			AppendCondition(xpath, XmlNames.TitleName, criteria.Title);
			xpath.Append(']');
		}

		return xpath.ToString();
	}

	private static void AppendCondition(StringBuilder xpath, string attribute, string value)
	{
		if (value.Length > 0)
		{
			xpath.Append($"[normalize-space(@{attribute})={ToXPathLiteral(value)}]");
		}
	}

	private static string ToXPathLiteral(string value)
	{
		if (!value.Contains('\''))
		{
			return $"'{value}'";
		}

		if (!value.Contains('"'))
		{
			return $"\"{value}\"";
		}

		var parts = value.Split('\'').Select(part => $"'{part}'");
		return $"concat({string.Join(", \"'\", ", parts)})";
	}

	private static Scientist ToScientist(XmlElement element)
	{
		var scientist = new Scientist
		{
			FullName = XmlUtils.Normalize(element.GetAttribute(XmlNames.FullName)),
			Faculty = XmlUtils.Normalize(element.GetAttribute(XmlNames.Faculty)),
			Department = XmlUtils.Normalize(element.GetAttribute(XmlNames.Department)),
			Degree = XmlUtils.Normalize(element.GetAttribute(XmlNames.Degree))
		};

		var titles = element.SelectNodes($"{XmlNames.Titles}/{XmlNames.Title}");
		if (titles is not null)
		{
			foreach (XmlElement title in titles)
			{
				scientist.Titles.Add(new AcademicTitle
				{
					Name = XmlUtils.Normalize(title.GetAttribute(XmlNames.TitleName)),
					AssignedDate = XmlUtils.ParseDate(title.GetAttribute(XmlNames.AssignedDate))
				});
			}
		}

		return scientist;
	}
}
