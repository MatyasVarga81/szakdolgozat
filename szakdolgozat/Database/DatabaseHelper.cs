using SQLite;
using System.IO;

namespace szakdolgozat.Database
{
    public class DatabaseHelper
    {
        private readonly SQLiteConnection _database;

        public DatabaseHelper()
        {
            // Adatbázis elérési útjának meghatározása
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mydatabase.db");
            _database = new SQLiteConnection(dbPath);

            // Felhasználói tábla létrehozása
            _database.CreateTable<User>();
        }

        // Felhasználó hozzáadása az adatbázishoz
        public void AddUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Name) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("Name, Email, és Password mezők nem lehetnek üresek!");
            }

            _database.Insert(user);
        }

        // Felhasználók lekérdezése az adatbázisból
        public List<User> GetUsers()
        {
            return _database.Table<User>().ToList();
        }
    }

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Kötelező mezők megjelölése (nem nullázható)
        public string Name { get; set; } = string.Empty; // Nem lehet null, alapértelmezett üres érték
        public string Email { get; set; } = string.Empty; // Nem lehet null, alapértelmezett üres érték
        public string Password { get; set; } = string.Empty; // Nem lehet null, alapértelmezett üres érték
    }
}