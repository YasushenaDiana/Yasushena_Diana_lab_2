namespace Lab2.Core.Model;

public sealed class AcademicTitle
{
	public string Name { get; set; } = string.Empty;

	public DateTime? AssignedDate { get; set; }

	public override string ToString() =>
		AssignedDate is { } date ? $"{Name} ({date:dd.MM.yyyy})" : $"{Name} (дата не вказана)";
}
