using szakdolgozat.Pages;

namespace szakdolgozat;

public partial class App : Application
{
    // 🔹 Statikus tulajdonság a bejelentkezett felhasználónév tárolására
    public static string LoggedInUsername { get; set; } = string.Empty;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Alkalmazás főablakának létrehozása
        return new Window(new AppShell());
    }

    public async void OnLoginSuccessful()
    {
        try
        {
            // Navigálás a főképernyőre bejelentkezés után
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            // Hibakezelés navigációs probléma esetén
            Console.WriteLine($"Navigáció sikertelen: {ex.Message}");
        }
    }
}
