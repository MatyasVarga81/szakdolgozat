using Microsoft.Maui.Controls;
using szakdolgozat.Database;
using System.Linq;

namespace szakdolgozat.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseHelper _db;
        private bool isPasswordVisible = false; // 👁️ állapot

        public LoginPage()
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            UpdateLoginButtonState(); // Gomb állapot beállítás
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string usernameOrEmail = UsernameEntry.Text?.Trim();
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Hiba", "Kérlek töltsd ki az összes mezőt!", "OK");
                return;
            }

            // 🔹 1. Teszt admin fiók
            if (usernameOrEmail == "admin" && password == "1234")
            {
                App.LoggedInUsername = "admin";
                await DisplayAlert("Bejelentkezés", "Sikeres bejelentkezés (admin)!", "OK");
                await Shell.Current.GoToAsync("//MainPage");
                return;
            }

            // 🔹 2. Adatbázisban való keresés
            var user = _db.GetUsers()
                          .FirstOrDefault(u => u.Name == usernameOrEmail.ToLower() && u.Password == password);

            if (user != null)
            {
                App.LoggedInUsername = user.Name;
                await DisplayAlert("Bejelentkezés", $"Üdv, {user.Name}!", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await DisplayAlert("Hiba", "Helytelen e-mail vagy jelszó!", "OK");
            }
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage());
        }

        private void OnUsernameChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLoginButtonState();
        }

        private void OnPasswordChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLoginButtonState();
        }

        private void UpdateLoginButtonState()
        {
            LoginButton.IsEnabled =
                !string.IsNullOrWhiteSpace(UsernameEntry.Text) &&
                !string.IsNullOrWhiteSpace(PasswordEntry.Text);
        }

        // 👁️ Szem ikon kattintás eseménykezelő
        private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordEntry.IsPassword = !isPasswordVisible;
            TogglePasswordButton.Source = isPasswordVisible ? "eye_off.png" : "eye.png";
        }
    }
}
