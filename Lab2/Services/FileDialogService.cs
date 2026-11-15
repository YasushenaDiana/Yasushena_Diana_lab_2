using CommunityToolkit.Maui.Storage;

namespace Lab2.Services;

public sealed class FileDialogService
{
	public const string XslFileName = "scientists.xsl";

	private static readonly FilePickerFileType XmlFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
	{
		[DevicePlatform.MacCatalyst] = ["public.xml"],
		[DevicePlatform.WinUI] = [".xml"]
	});

	public async Task<string?> PickXmlFileAsync()
	{
		var file = await FilePicker.Default.PickAsync(new PickOptions
		{
			PickerTitle = "Виберіть XML-файл",
			FileTypes = XmlFileType
		});
		if (file is null)
		{
			return null;
		}

		var localPath = Path.Combine(FileSystem.CacheDirectory, file.FileName);
		await using (var source = await file.OpenReadAsync())
		await using (var target = File.Create(localPath))
		{
			await source.CopyToAsync(target);
		}

		return localPath;
	}

	public async Task<string> GetXslPathAsync()
	{
		var path = Path.Combine(FileSystem.CacheDirectory, XslFileName);
		await using var source = await FileSystem.OpenAppPackageFileAsync(XslFileName);
		await using var target = File.Create(path);
		await source.CopyToAsync(target);
		return path;
	}

	public string GetTemporaryHtmlPath(string xmlPath) =>
		Path.Combine(FileSystem.CacheDirectory, Path.GetFileNameWithoutExtension(xmlPath) + ".html");

	public async Task<string?> SaveHtmlAsync(string htmlPath)
	{
		await using var stream = File.OpenRead(htmlPath);
		var result = await FileSaver.Default.SaveAsync(Path.GetFileName(htmlPath), stream, CancellationToken.None);
		return result.IsSuccessful ? result.FilePath : null;
	}
}
