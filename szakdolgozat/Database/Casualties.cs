using SQLite;
using System;

namespace szakdolgozat.Database
{
    public class Casualties
    {
        [PrimaryKey, AutoIncrement]
        public int CasualtiesId { get; set; }

        public int? NumberOfCasualties { get; set; }

        public string? Nationality { get; set; }
    }
}
