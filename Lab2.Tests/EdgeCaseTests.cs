using System.Xml;
using Lab2.Core.Model;
using Lab2.Core.Reading;
using Lab2.Core.Search;

namespace Lab2.Tests;

public class EdgeCaseTests : IDisposable
{
	private readonly string path = Path.GetTempFileName();

	public void Dispose() => File.Delete(path);

	private static readonly ISearchStrategy[] Strategies =
		[new SaxSearchStrategy(), new DomSearchStrategy(), new LinqSearchStrategy()];

	private List<string>[] SearchAll(string xml, SearchCriteria criteria)
	{
		File.WriteAllText(path, xml);
		return Strategies.Select(s => s.Search(path, criteria).Select(x => x.FullName).ToList()).ToArray();
	}

	[Fact]
	public void ExtraSpacesInXml_AreIgnored()
	{
		File.WriteAllText(path, "<Scientists><Scientist FullName=' Іван ' Faculty='  Фізичний   факультет '>" +
			"<Titles><Title Name=' Доцент ' /></Titles></Scientist></Scientists>");
		var values = new XmlAttributeReader().GetAttributeValues(path);
		Assert.Equal("Фізичний факультет", Assert.Single(values[XmlNames.Faculty]));

		var criteria = new SearchCriteria(new Dictionary<string, string?>
		{
			[XmlNames.Faculty] = values[XmlNames.Faculty][0],
			[XmlNames.Title] = values[XmlNames.Title][0]
		});
		foreach (var strategy in Strategies)
		{
			Assert.Equal("Іван", Assert.Single(strategy.Search(path, criteria)).FullName);
		}
	}

	[Fact]
	public void SecondTitle_IsAlsoChecked()
	{
		var results = SearchAll("<Scientists><Scientist FullName='A'><Titles><Title Name='Доцент' />" +
			"<Title Name='Професор' /></Titles></Scientist></Scientists>", new SearchCriteria { Title = "Професор" });

		Assert.All(results, r => Assert.Equal(["A"], r));
	}

	[Fact]
	public void NodesInWrongPlace_AreIgnoredByAllStrategies()
	{
		var results = SearchAll("<Scientists><Scientist FullName='A' Faculty='Ф'><Title Name='Професор' />" +
			"<Scientist FullName='B' Faculty='Ф' /></Scientist></Scientists>", new SearchCriteria { Faculty = "Ф" });
		Assert.All(results, r => Assert.Equal(["A"], r));

		results = SearchAll("<Scientists><Scientist FullName='A'><Title Name='Професор' /></Scientist></Scientists>",
			new SearchCriteria { Title = "Професор" });
		Assert.All(results, Assert.Empty);
	}

	[Fact]
	public void QuotesAndApostrophesInValue_Work()
	{
		var results = SearchAll("<Scientists><Scientist FullName='Мар&apos;яна &quot;Ж&quot;' /></Scientists>",
			new SearchCriteria { FullName = "Мар'яна \"Ж\"" });

		Assert.All(results, r => Assert.Single(r));
	}

	[Fact]
	public void DtdIsRejectedEverywhere()
	{
		File.WriteAllText(path, "<!DOCTYPE Scientists [<!ENTITY f 'Ф'>]><Scientists><Scientist FullName='A' Faculty='&f;' /></Scientists>");

		Assert.Throws<XmlException>(() => new XmlAttributeReader().GetAttributeValues(path));
		foreach (var strategy in Strategies)
		{
			Assert.Throws<XmlException>(() => strategy.Search(path, new SearchCriteria { Faculty = "Ф" }));
		}
	}

	[Theory]
	[InlineData("<Books><Book /></Books>")]
	[InlineData("<Scientists xmlns='urn:other'><Scientist FullName='A' /></Scientists>")]
	[InlineData("<Scientists />")]
	public void WrongFile_IsRejectedOnOpen(string xml)
	{
		File.WriteAllText(path, xml);
		Assert.Throws<InvalidDataException>(() => new XmlAttributeReader().GetAttributeValues(path));
	}

	[Fact]
	public void BrokenXml_IsRejected()
	{
		File.WriteAllText(path, "<Scientists><Scientist FullName='A'></Scientists>");
		Assert.Throws<XmlException>(() => new XmlAttributeReader().GetAttributeValues(path));
	}

	[Theory]
	[InlineData("2020-05-14")]
	[InlineData("14.05.2020")]
	public void DateFormats_AreParsed(string date)
	{
		File.WriteAllText(path, $"<Scientists><Scientist FullName='A'><Titles><Title Name='Д' AssignedDate='{date}' /></Titles></Scientist></Scientists>");
		foreach (var strategy in Strategies)
		{
			var title = strategy.Search(path, new SearchCriteria { FullName = "A" })[0].Titles[0];
			Assert.Equal(new DateTime(2020, 5, 14), title.AssignedDate);
		}
	}
}
