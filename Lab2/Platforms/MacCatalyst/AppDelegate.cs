using Foundation;
using ObjCRuntime;
using UIKit;

namespace Lab2;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override void BuildMenu(IUIMenuBuilder builder)
	{
		base.BuildMenu(builder);

		foreach (var menu in new[] { UIMenuIdentifier.File, UIMenuIdentifier.Format, UIMenuIdentifier.View })
		{
			builder.RemoveMenu(menu.GetConstant()!);
		}

		builder.RemoveMenu(UIMenuIdentifier.Quit.GetConstant()!);
		var quit = UIKeyCommand.Create(new NSString("Вийти з YasushenaDiana_xml"), null,
			new Selector("confirmQuit:"), new NSString("q"), UIKeyModifierFlags.Command, null!);
		var quitMenu = UIMenu.Create(string.Empty, null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, [quit]);
		builder.InsertChildMenuAtEnd(quitMenu, UIMenuIdentifier.Application.GetConstant()!);
	}

	[Export("confirmQuit:")]
	public async void ConfirmQuit(NSObject sender)
	{
		if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page is Shell { CurrentPage: MainPage page })
		{
			await page.Exit();
		}
	}
}
