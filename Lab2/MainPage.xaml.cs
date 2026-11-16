using System.Xml;
using System.Xml.Xsl;
using Lab2.Core.Model;
using Lab2.Core.Reading;
using Lab2.Core.Search;
using Lab2.Core.Transform;
using Lab2.Services;

namespace Lab2;

public partial class MainPage : ContentPage
{
	private const string AnyValue = "(усі)";

	private readonly XmlSearchContext searchContext = new();

	private readonly Dictionary<string, ISearchStrategy> strategies = new()
	{
		["SAX"] = new SaxSearchStrategy(),
		["DOM"] = new DomSearchStrategy(),
		["LINQ"] = new LinqSearchStrategy()
	};

	private readonly XmlAttributeReader attributeReader = new();
	private readonly XslTransformer transformer = new();
	private readonly XmlResultWriter resultWriter = new();
	private readonly FileDialogService fileDialogs = new();
	private readonly Dictionary<string, Label> attributeLabels;
	private readonly Dictionary<string, string> attributeCaptions = new()
	{
		[XmlNames.FullName] = "П.І.П.",
		[XmlNames.Faculty] = "Факультет",
		[XmlNames.Department] = "Кафедра",
		[XmlNames.Degree] = "Науковий ступінь",
		[XmlNames.Title] = "Вчене звання"
	};

	private Dictionary<string, List<string>> attributeValues = [];
	private readonly Dictionary<string, string?> selectedValues = [];

	private string? xmlPath;
	private bool isExiting;
	private TaskCompletionSource<string?>? selection;
	private List<Scientist>? results;

	public MainPage()
	{
		InitializeComponent();

		attributeLabels = new Dictionary<string, Label>
		{
			[XmlNames.FullName] = fullNameLabel,
			[XmlNames.Faculty] = facultyLabel,
			[XmlNames.Department] = departmentLabel,
			[XmlNames.Degree] = degreeLabel,
			[XmlNames.Title] = titleLabel
		};
	}

	public async Task OpenFile(string path)
	{
		CloseSelection(null);
		try
		{
			var values = attributeReader.GetAttributeValues(path);
			xmlPath = path;
			ResetResults();
			ShowAttributeValues(values);
			fileLabel.Text = Path.GetFileName(path);
			ShowStatus("Файл відкрито");
		}
		catch (Exception ex)
		{
			await ShowMessage("Помилка", "Не вдалося відкрити файл.\n" + DescribeError(ex));
		}
	}

	public async Task Search(IReadOnlyDictionary<string, string?> values, string method)
	{
		if (xmlPath is null)
		{
			await ShowMessage("Пошук", "Спочатку відкрийте XML-файл.");
			return;
		}

		var criteria = new SearchCriteria(values);
		if (criteria.IsEmpty)
		{
			await ShowMessage("Пошук", "Оберіть значення хоча б одного атрибута.");
			return;
		}

		try
		{
			searchContext.SetStrategy(strategies[method]);
			results = searchContext.ExecuteSearch(xmlPath, criteria);
			ShowResults(results);
			ShowStatus($"Пошук: {method}");

			if (results.Count == 0)
			{
				await ShowMessage("Пошук", "Нічого не знайдено.");
			}
		}
		catch (Exception ex)
		{
			await ShowMessage("Помилка", DescribeError(ex));
		}
	}

	public async Task TransformToHtml()
	{
		if (xmlPath is null)
		{
			await ShowMessage("HTML", "Спочатку відкрийте XML-файл.");
			return;
		}

		if (results is { Count: 0 })
		{
			await ShowMessage("HTML", "Немає результатів пошуку для збереження.");
			return;
		}

		try
		{
			var xslPath = await fileDialogs.GetXslPathAsync();
			var htmlPath = fileDialogs.GetTemporaryHtmlPath(xmlPath);
			var sourcePath = xmlPath;
			if (results is not null)
			{
				sourcePath = Path.Combine(Path.GetDirectoryName(htmlPath)!, "search_results.xml");
				resultWriter.Save(results, sourcePath);
			}

			transformer.Transform(sourcePath, xslPath, htmlPath);

			var savedPath = await fileDialogs.SaveHtmlAsync(htmlPath);
			if (savedPath is null)
			{
				return;
			}

			ShowStatus("Збережено: " + savedPath);
			await ShowMessage("HTML", "HTML-файл створено.");
		}
		catch (Exception ex)
		{
			await ShowMessage("Помилка", "Не вдалося створити HTML.\n" + DescribeError(ex));
		}
	}

	public void Clear()
	{
		CloseSelection(null);
		foreach (var attribute in attributeLabels.Keys)
		{
			SetSelectedValue(attribute, null);
		}

		saxRadio.IsChecked = true;
		ResetResults();
		ShowStatus("Готово");
	}

	public async Task Exit()
	{
		if (isExiting)
		{
			return;
		}

		isExiting = true;
		var confirmed = await ConfirmExit();
		isExiting = false;

		if (confirmed)
		{
			CloseApp();
		}
	}

	private void ShowAttributeValues(Dictionary<string, List<string>> values)
	{
		attributeValues = values;
		foreach (var attribute in attributeLabels.Keys)
		{
			SetSelectedValue(attribute, null);
		}
	}

	private async Task SelectAttributeValue(string attribute)
	{
		if (xmlPath is null)
		{
			await ShowMessage("Пошук", "Спочатку відкрийте XML-файл.");
			return;
		}

		if (!attributeValues.TryGetValue(attribute, out var values) || values.Count == 0)
		{
			await ShowMessage("Пошук", "У файлі немає значень цього атрибута.");
			return;
		}

		var choice = await ChooseValue(attributeCaptions[attribute], [AnyValue, .. values], selectedValues[attribute] ?? AnyValue);
		if (choice == AnyValue)
		{
			SetSelectedValue(attribute, null);
		}
		else if (choice != null && values.Contains(choice))
		{
			SetSelectedValue(attribute, choice);
		}
	}

	private Task<string?> ChooseValue(string title, List<string> options, string current)
	{
		selection = new TaskCompletionSource<string?>();
		selectTitle.Text = title;
		selectOptions.Children.Clear();

		foreach (var option in options)
		{
			var button = new Button { Text = option, Style = (Style)Resources["OptionBtn"] };
			if (option == current)
			{
				button.FontAttributes = FontAttributes.Bold;
				button.TextColor = AppColors.Primary;
			}

			button.Clicked += (_, _) => CloseSelection(option);
			selectOptions.Children.Add(button);
		}

		selectOverlay.IsVisible = true;
		return selection.Task;
	}

	private void CloseSelection(string? value)
	{
		selectOverlay.IsVisible = false;
		selection?.TrySetResult(value);
	}

	private void SetSelectedValue(string attribute, string? value)
	{
		selectedValues[attribute] = value;
		var label = attributeLabels[attribute];
		label.Text = value ?? AnyValue;
		label.TextColor = value is null ? AppColors.TextSecondary : AppColors.Text;
	}

	private void ShowResults(List<Scientist> scientists)
	{
		resultsView.ItemsSource = scientists.Select((s, i) => new ScientistItem(i + 1, s)).ToList();
		resultsHeader.Text = $"знайдено: {scientists.Count}";
		emptyLabel.Text = "Нічого не знайдено";
	}

	private void ResetResults()
	{
		results = null;
		resultsView.ItemsSource = null;
		resultsHeader.Text = "";
		emptyLabel.Text = xmlPath is null
			? "Відкрийте XML-файл, щоб почати пошук"
			: "Оберіть параметри і натисніть «Знайти»";
	}

	private static string DescribeError(Exception ex) => ex switch
	{
		XmlException xml when xml.Message.Contains("DTD") => "Файли з DTD не підтримуються.",
		XmlException xml => $"Файл не є коректним XML (рядок {xml.LineNumber}, позиція {xml.LinePosition}).",
		XsltException => "Помилка у файлі XSL.",
		InvalidDataException => ex.Message,
		UnauthorizedAccessException => "Немає доступу до файлу.",
		IOException => "Помилка читання або запису файлу.",
		_ => ex.Message
	};

	private Task ShowMessage(string title, string text) =>
		DisplayAlertAsync(title, text, "OK");

	private void ShowStatus(string text) => statusLabel.Text = text;

	private Task<bool> ConfirmExit() =>
		DisplayAlertAsync("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");

	private static void CloseApp() => Application.Current?.Quit();

	private Dictionary<string, string?> GetSelectedValues() => new(selectedValues);

	private string GetSelectedMethod()
	{
		if (domRadio.IsChecked) return "DOM";
		if (linqRadio.IsChecked) return "LINQ";
		return "SAX";
	}

	private async void OpenButton_Clicked(object? sender, EventArgs e)
	{
		string? path;
		try
		{
			path = await fileDialogs.PickXmlFileAsync();
		}
		catch (Exception ex)
		{
			await ShowMessage("Помилка", DescribeError(ex));
			return;
		}

		if (path != null)
		{
			await OpenFile(path);
		}
	}

	private async void SearchButton_Clicked(object? sender, EventArgs e) =>
		await Search(GetSelectedValues(), GetSelectedMethod());

	private async void TransformButton_Clicked(object? sender, EventArgs e) =>
		await TransformToHtml();

	private async void Selector_Tapped(object? sender, TappedEventArgs e)
	{
		if (e.Parameter is string attribute)
		{
			await SelectAttributeValue(attribute);
		}
	}

	private void SelectCancel_Clicked(object? sender, EventArgs e) => CloseSelection(null);

	private void ClearButton_Clicked(object? sender, EventArgs e) => Clear();

	private async void ExitButton_Clicked(object? sender, EventArgs e) => await Exit();
}
