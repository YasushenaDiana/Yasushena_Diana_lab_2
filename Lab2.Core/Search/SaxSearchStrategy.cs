using System.Xml;
using Lab2.Core.Model;

namespace Lab2.Core.Search;

public sealed class SaxSearchStrategy : ISearchStrategy
{
	private const int ScientistDepth = 1;
	private const int TitleDepth = 3;

	public List<Scientist> Search(string path, SearchCriteria criteria)
	{
		var results = new List<Scientist>();
		using var reader = XmlUtils.CreateReader(path);

		Scientist? current = null;
		var inTitles = false;

		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.Depth == 0 && (reader.Name != XmlNames.Root || reader.NamespaceURI.Length > 0))
				{
					return results;
				}

				if (reader.Depth == ScientistDepth && reader.Name == XmlNames.Scientist)
				{
					current = ReadScientist(reader);
					if (reader.IsEmptyElement)
					{
						AddIfMatches(current, criteria, results);
						current = null;
					}
				}
				else if (current != null && reader.Depth == ScientistDepth + 1 && reader.Name == XmlNames.Titles)
				{
					inTitles = !reader.IsEmptyElement;
				}
				else if (current != null && inTitles && reader.Depth == TitleDepth && reader.Name == XmlNames.Title)
				{
					current.Titles.Add(new AcademicTitle
					{
						Name = XmlUtils.Normalize(reader.GetAttribute(XmlNames.TitleName)),
						AssignedDate = XmlUtils.ParseDate(reader.GetAttribute(XmlNames.AssignedDate))
					});
				}
			}
			else if (reader.NodeType == XmlNodeType.EndElement)
			{
				if (reader.Depth == ScientistDepth + 1 && reader.Name == XmlNames.Titles)
				{
					inTitles = false;
				}
				else if (current != null && reader.Depth == ScientistDepth && reader.Name == XmlNames.Scientist)
				{
					AddIfMatches(current, criteria, results);
					current = null;
				}
			}
		}

		return results;
	}

	private static Scientist ReadScientist(XmlReader reader) => new()
	{
		FullName = XmlUtils.Normalize(reader.GetAttribute(XmlNames.FullName)),
		Faculty = XmlUtils.Normalize(reader.GetAttribute(XmlNames.Faculty)),
		Department = XmlUtils.Normalize(reader.GetAttribute(XmlNames.Department)),
		Degree = XmlUtils.Normalize(reader.GetAttribute(XmlNames.Degree))
	};

	private static void AddIfMatches(Scientist scientist, SearchCriteria criteria, List<Scientist> results)
	{
		if (Matches(scientist.FullName, criteria.FullName) &&
			Matches(scientist.Faculty, criteria.Faculty) &&
			Matches(scientist.Department, criteria.Department) &&
			Matches(scientist.Degree, criteria.Degree) &&
			(criteria.Title.Length == 0 || scientist.Titles.Any(t => t.Name == criteria.Title)))
		{
			results.Add(scientist);
		}
	}

	private static bool Matches(string value, string expected) =>
		expected.Length == 0 || value == expected;
}
