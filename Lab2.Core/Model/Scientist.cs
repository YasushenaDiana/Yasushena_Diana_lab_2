namespace Lab2.Core.Model;

public sealed class Scientist
{
	public string FullName { get; set; } = string.Empty;
	public string Faculty { get; set; } = string.Empty;
	public string Department { get; set; } = string.Empty;
	public string Degree { get; set; } = string.Empty;

	public List<AcademicTitle> Titles { get; set; } = [];
}
