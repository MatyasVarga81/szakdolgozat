using SQLite;
using System;

namespace szakdolgozat.Database
{
    public class Coordinates
    {
        [PrimaryKey, AutoIncrement]
        public int CoordinatesId { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }

        public string MGRS { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
