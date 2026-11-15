using System.Runtime.InteropServices;
using ObjCRuntime;

namespace Lab2;

internal static class CloseButton
{
	private const string ObjC = "/usr/lib/libobjc.dylib";
	private const long NSWindowCloseButton = 0;

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	private static extern IntPtr Send(IntPtr receiver, IntPtr selector);

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	private static extern IntPtr Send(IntPtr receiver, IntPtr selector, nuint index);

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	private static extern IntPtr Send(IntPtr receiver, IntPtr selector, long value);

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	private static extern void Send(IntPtr receiver, IntPtr selector, bool value);

	public static void Disable()
	{
		var app = Send(Class.GetHandle("NSApplication"), Selector.GetHandle("sharedApplication"));
		if (app == IntPtr.Zero)
		{
			return;
		}

		var windows = Send(app, Selector.GetHandle("windows"));
		var count = (long)Send(windows, Selector.GetHandle("count"));
		for (nuint i = 0; i < (nuint)count; i++)
		{
			var window = Send(windows, Selector.GetHandle("objectAtIndex:"), i);
			var button = Send(window, Selector.GetHandle("standardWindowButton:"), NSWindowCloseButton);
			if (button != IntPtr.Zero)
			{
				Send(button, Selector.GetHandle("setEnabled:"), false);
			}
		}
	}
}
