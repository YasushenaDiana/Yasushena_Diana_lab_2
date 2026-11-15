namespace Lab2;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell())
		{
			Title = "YasushenaDiana_xml",
			Width = 1280,
			Height = 820,
			MinimumWidth = 1150,
			MinimumHeight = 700
		};

#if MACCATALYST
		window.Created += (_, _) => window.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), CloseButton.Disable);
		window.Activated += (_, _) => window.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), CloseButton.Disable);
#endif

		return window;
	}
}
