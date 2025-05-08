using Microsoft.Maui.Controls;
using szakdolgozat.Database;
using System.Linq;
using System.Text.RegularExpressions;

namespace szakdolgozat.Pages
{
    public partial class RegistrationPage : ContentPage
    {
        private readonly DatabaseHelper _db;
        private bool isPasswordVisible = false;
        private bool isConfirmPasswordVisible = false;

        public RegistrationPage()
        {
            InitializeComponent();
            _db = new DatabaseHelper();
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text?.Trim();
            string email = EmailEntry.Text?.Trim().ToLower();
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Hiba", "Kérlek töltsd ki az összes mezõt!", "OK");
                return;
            }

            if (!IsValidEmail(email))
            {
                await DisplayAlert("Hiba", "Hibás email formátum!", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Hiba", "A jelszavak nem egyeznek!", "OK");
                return;
            }

            if (_db.GetUsers().Any(u => u.Email == email))
            {
                await DisplayAlert("Hiba", "Ez az e-mail cím már regisztrálva van!", "OK");
                return;
            }

            var user = new User
            {
                Name = name,
                Email = email,
                Password = password
            };

            _db.AddUser(user);

            await DisplayAlert("Siker", "Sikeres regisztráció!", "OK");
            await Navigation.PopAsync(); // vissza a LoginPage-re
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private void OnPasswordChanged(object sender, TextChangedEventArgs e)
        {
            string password = PasswordEntry.Text;
            string strength = GetPasswordStrength(password);

            PasswordStrengthLabel.Text = $"Jelszó erõsség: {strength}";
            PasswordStrengthLabel.TextColor = strength switch
            {
                "Erõs" => Colors.Green,
                "Közepes" => Colors.Orange,
                _ => Colors.Red
            };
        }

        private string GetPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return "Gyenge";

            int score = 0;
            if (password.Length >= 12) score++;
            if (Regex.IsMatch(password, @"\d")) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;

            return score switch
            {
                >= 4 => "Erõs",
                3 => "Közepes",
                _ => "Gyenge"
            };
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordEntry.IsPassword = !isPasswordVisible;
            TogglePasswordButton.Source = isPasswordVisible ? "eye_off.png" : "eye.png";
        }

        private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
        {
            isConfirmPasswordVisible = !isConfirmPasswordVisible;
            ConfirmPasswordEntry.IsPassword = !isConfirmPasswordVisible;
            ToggleConfirmPasswordButton.Source = isConfirmPasswordVisible ? "eye_off.png" : "eye.png";
        }
    }
}
