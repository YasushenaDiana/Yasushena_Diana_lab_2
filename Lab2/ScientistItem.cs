using Lab2.Core.Model;

namespace Lab2;

public sealed class ScientistItem
{
	public ScientistItem(int number, Scientist scientist)
	{
		Number = number.ToString();
		FullName = scientist.FullName;
		Faculty = OrDash(scientist.Faculty);
		Department = OrDash(scientist.Department);
		Degree = OrDash(scientist.Degree);
		Titles = scientist.Titles.Count == 0
			? "-"
			: string.Join("\n", scientist.Titles.Select(t => t.ToString()));
		RowColor = number % 2 == 0 ? AppColors.RowAlt : AppColors.Surface;
	}

	public string Number { get; }
	public string FullName { get; }
	public string Faculty { get; }
	public string Department { get; }
	public string Degree { get; }
	public string Titles { get; }
	public Color RowColor { get; }

	private static string OrDash(string value) => value.Length == 0 ? "-" : value;
}
