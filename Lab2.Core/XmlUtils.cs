using System.Globalization;
using System.Xml;

namespace Lab2.Core;

internal static class XmlUtils
{
	private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd.MM.yyyy"];

	public static XmlReader CreateReader(string path) =>
		XmlReader.Create(path, new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit,
			IgnoreComments = true,
			IgnoreWhitespace = true,
			IgnoreProcessingInstructions = true
		});

	public static string Normalize(string? value)
	{
		if (value is null)
		{
			return string.Empty;
		}

		var words = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		return string.Join(" ", words);
	}

	public static DateTime? ParseDate(string? text) =>
		DateTime.TryParseExact(Normalize(text), DateFormats, CultureInfo.InvariantCulture,
			DateTimeStyles.None, out var date)
			? date
			: null;
}
