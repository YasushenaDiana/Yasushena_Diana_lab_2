using System.Xml.Linq;
using Lab2.Core.Model;

namespace Lab2.Core.Search;

public sealed class LinqSearchStrategy : ISearchStrategy
{
	public List<Scientist> Search(string path, SearchCriteria criteria)
	{
		XDocument document;
		using (var reader = XmlUtils.CreateReader(path))
		{
			document = XDocument.Load(reader);
		}

		var query = document.Root?.Name == XmlNames.Root
			? document.Root.Elements(XmlNames.Scientist)
			: [];

		if (criteria.FullName.Length > 0)
		{
			query = query.Where(s => XmlUtils.Normalize((string?)s.Attribute(XmlNames.FullName)) == criteria.FullName);
		}
		if (criteria.Faculty.Length > 0)
		{
			query = query.Where(s => XmlUtils.Normalize((string?)s.Attribute(XmlNames.Faculty)) == criteria.Faculty);
		}
		if (criteria.Department.Length > 0)
		{
			query = query.Where(s => XmlUtils.Normalize((string?)s.Attribute(XmlNames.Department)) == criteria.Department);
		}
		if (criteria.Degree.Length > 0)
		{
			query = query.Where(s => XmlUtils.Normalize((string?)s.Attribute(XmlNames.Degree)) == criteria.Degree);
		}
		if (criteria.Title.Length > 0)
		{
			query = query.Where(s => s.Elements(XmlNames.Titles).Elements(XmlNames.Title)
				.Any(t => XmlUtils.Normalize((string?)t.Attribute(XmlNames.TitleName)) == criteria.Title));
		}

		return (from s in query
				select new Scientist
				{
					FullName = XmlUtils.Normalize((string?)s.Attribute(XmlNames.FullName)),
					Faculty = XmlUtils.Normalize((string?)s.Attribute(XmlNames.Faculty)),
					Department = XmlUtils.Normalize((string?)s.Attribute(XmlNames.Department)),
					Degree = XmlUtils.Normalize((string?)s.Attribute(XmlNames.Degree)),
					Titles = (from t in s.Elements(XmlNames.Titles).Elements(XmlNames.Title)
							  select new AcademicTitle
							  {
								  Name = XmlUtils.Normalize((string?)t.Attribute(XmlNames.TitleName)),
								  AssignedDate = XmlUtils.ParseDate((string?)t.Attribute(XmlNames.AssignedDate))
							  }).ToList()
				}).ToList();
	}
}
