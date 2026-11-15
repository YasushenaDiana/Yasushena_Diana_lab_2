namespace Lab2.Core.Model;

public sealed class SearchCriteria
{
	public string FullName { get; set; } = string.Empty;
	public string Faculty { get; set; } = string.Empty;
	public string Department { get; set; } = string.Empty;
	public string Degree { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;

	public SearchCriteria()
	{
	}

	public SearchCriteria(IReadOnlyDictionary<string, string?> values)
	{
		FullName = Get(values, XmlNames.FullName);
		Faculty = Get(values, XmlNames.Faculty);
		Department = Get(values, XmlNames.Department);
		Degree = Get(values, XmlNames.Degree);
		Title = Get(values, XmlNames.Title);
	}

	public bool IsEmpty =>
		FullName.Length == 0 && Faculty.Length == 0 && Department.Length == 0 &&
		Degree.Length == 0 && Title.Length == 0;

	private static string Get(IReadOnlyDictionary<string, string?> values, string key) =>
		values.TryGetValue(key, out var value) ? XmlUtils.Normalize(value) : string.Empty;
}
