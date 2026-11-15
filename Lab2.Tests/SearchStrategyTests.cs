using Lab2.Core.Model;
using Lab2.Core.Reading;
using Lab2.Core.Search;
using Lab2.Core.Transform;

namespace Lab2.Tests;

public class SearchStrategyTests
{
	private static readonly string XmlPath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "scientists.xml");
	private static readonly string XslPath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "scientists.xsl");

	public static TheoryData<string> Strategies => new() { "SAX", "DOM", "LINQ" };

	private static ISearchStrategy Create(string name) => name switch
	{
		"SAX" => new SaxSearchStrategy(),
		"DOM" => new DomSearchStrategy(),
		"LINQ" => new LinqSearchStrategy(),
		_ => throw new ArgumentOutOfRangeException(nameof(name))
	};

	private static List<Scientist> Run(string strategy, SearchCriteria criteria)
	{
		var context = new XmlSearchContext();
		context.SetStrategy(Create(strategy));
		return context.ExecuteSearch(XmlPath, criteria);
	}

	[Theory]
	[MemberData(nameof(Strategies))]
	public void SearchByFaculty_ReturnsOnlyThatFaculty(string strategy)
	{
		var results = Run(strategy, new SearchCriteria { Faculty = "Фізичний факультет" });

		Assert.Equal(3, results.Count);
		Assert.All(results, s => Assert.Equal("Фізичний факультет", s.Faculty));
	}

	[Theory]
	[MemberData(nameof(Strategies))]
	public void SearchByTitle_ChecksNestedTitles(string strategy)
	{
		var results = Run(strategy, new SearchCriteria { Title = "Професор" });

		Assert.Equal(5, results.Count);
		Assert.All(results, s => Assert.Contains(s.Titles, t => t.Name == "Професор"));
	}

	[Theory]
	[MemberData(nameof(Strategies))]
	public void SearchBySeveralAttributes_CombinesConditions(string strategy)
	{
		var results = Run(strategy, new SearchCriteria
		{
			Faculty = "Факультет комп'ютерних наук та кібернетики",
			Title = "Професор"
		});

		Assert.Equal(["Коваленко Олександр Петрович", "Гончаренко Юлія Павлівна"], results.Select(s => s.FullName));
	}

	[Theory]
	[MemberData(nameof(Strategies))]
	public void SearchByNameWithApostrophe_Works(string strategy)
	{
		var results = Run(strategy, new SearchCriteria { FullName = "Лисенко Мар'яна Андріївна" });

		var scientist = Assert.Single(results);
		var title = Assert.Single(scientist.Titles);
		Assert.Equal("Доцент", title.Name);
		Assert.Null(title.AssignedDate);
	}

	[Theory]
	[MemberData(nameof(Strategies))]
	public void MissingAttribute_DoesNotMatch(string strategy)
	{
		var results = Run(strategy, new SearchCriteria
		{
			Faculty = "Фізичний факультет",
			Degree = "Доктор фізико-математичних наук"
		});

		var scientist = Assert.Single(results);
		Assert.Equal("Олійник Тарас Богданович", scientist.FullName);
		Assert.Equal(new DateTime(2010, 7, 7), scientist.Titles[1].AssignedDate);
	}

	[Theory]
	[MemberData(nameof(Strategies))]
	public void NoMatches_ReturnsEmptyList(string strategy)
	{
		var results = Run(strategy, new SearchCriteria
		{
			Faculty = "Хімічний факультет",
			Department = "Кафедра оптики"
		});

		Assert.Empty(results);
	}

	[Fact]
	public void AllStrategies_ReturnSameResults()
	{
		var reader = new XmlAttributeReader();
		var faculties = reader.GetAttributeValues(XmlPath)[XmlNames.Faculty];

		foreach (var faculty in faculties)
		{
			var criteria = new SearchCriteria { Faculty = faculty };
			var sax = Run("SAX", criteria).Select(Describe);
			Assert.Equal(sax, Run("DOM", criteria).Select(Describe));
			Assert.Equal(sax, Run("LINQ", criteria).Select(Describe));
		}
	}

	[Fact]
	public void DomXPath_IsGeneratedDynamically()
	{
		var xpath = DomSearchStrategy.BuildXPath(new SearchCriteria { Degree = "Доктор хімічних наук", Title = "Доцент" });

		Assert.Equal("/Scientists/Scientist[normalize-space(@Degree)='Доктор хімічних наук'][Titles/Title[normalize-space(@Name)='Доцент']]", xpath);
	}

	[Fact]
	public void AttributeReader_ReturnsOnlyValuesPresentInFile()
	{
		var values = new XmlAttributeReader().GetAttributeValues(XmlPath);

		Assert.Equal(12, values[XmlNames.FullName].Count);
		Assert.Equal(4, values[XmlNames.Faculty].Count);
		Assert.Equal(["Доцент", "Професор", "Старший дослідник"], values[XmlNames.Title]);
		Assert.DoesNotContain("", values[XmlNames.Degree]);
	}

	[Fact]
	public void SearchCriteria_FromValues_IgnoresEmpty()
	{
		var criteria = new SearchCriteria(new Dictionary<string, string?>
		{
			[XmlNames.Faculty] = "Хімічний факультет",
			[XmlNames.Degree] = null
		});

		Assert.Equal("Хімічний факультет", criteria.Faculty);
		Assert.Equal(string.Empty, criteria.Degree);
		Assert.False(criteria.IsEmpty);
		Assert.True(new SearchCriteria().IsEmpty);
	}

	[Fact]
	public void XslTransformer_CreatesHtmlTable()
	{
		var htmlPath = Path.Combine(Path.GetTempPath(), $"scientists_{Guid.NewGuid():N}.html");
		try
		{
			new XslTransformer().Transform(XmlPath, XslPath, htmlPath);

			var html = File.ReadAllText(htmlPath);
			Assert.Contains("<table>", html);
			Assert.Contains("Коваленко Олександр Петрович", html);
			Assert.Contains("Професор (01.12.2015)", html);
			Assert.Contains("Доцент (дата не вказана)", html);
		}
		finally
		{
			File.Delete(htmlPath);
		}
	}

	private static string Describe(Scientist s) =>
		$"{s.FullName}|{s.Faculty}|{s.Department}|{s.Degree}|{string.Join(";", s.Titles)}";
}
