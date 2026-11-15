using Lab2.Core.Model;

namespace Lab2.Core.Search;

public sealed class XmlSearchContext
{
	private ISearchStrategy? strategy;

	public XmlSearchContext()
	{
	}

	public XmlSearchContext(ISearchStrategy strategy)
	{
		this.strategy = strategy;
	}

	public void SetStrategy(ISearchStrategy strategy)
	{
		this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
	}

	public List<Scientist> ExecuteSearch(string path, SearchCriteria criteria)
	{
		if (strategy is null)
		{
			throw new InvalidOperationException("Не обрано спосіб пошуку.");
		}

		return strategy.Search(path, criteria);
	}
}
