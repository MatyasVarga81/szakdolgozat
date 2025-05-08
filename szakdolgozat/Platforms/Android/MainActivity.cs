using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                                 ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
[MetaData("com.google.android.geo.API_KEY", Value = "AIzaSyCvKc9xzYLBOaFXXkn3v848gvw-4QlRhdY")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new string[]
            {
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessCoarseLocation
            }, 1);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == 1 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
        {
            Console.WriteLine("Helymeghatározási engedély megadva.");
        }
        else
        {
            Console.WriteLine("A felhasználó nem adott engedélyt.");
        }
    }
}
