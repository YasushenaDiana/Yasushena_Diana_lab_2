using Lab2.Core.Model;

namespace Lab2.Core.Search;

public interface ISearchStrategy
{
	List<Scientist> Search(string path, SearchCriteria criteria);
}
