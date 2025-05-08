using Foundation;
using Google.Maps;
using UIKit;

namespace szakdolgozat;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    // Google Maps API kulcs beállítása iOS-re
    MapServices.ProvideApiKey("AIzaSyCvKc9xzYLBOaFXXkn3v848gvw-4QlRhdY");

    return base.FinishedLaunching(app, options);
}

}