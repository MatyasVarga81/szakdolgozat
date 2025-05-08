using SQLite;
using System;

namespace szakdolgozat.Database
{
    public class SALUTE
    {
        [PrimaryKey, AutoIncrement]
        public int SaluteId { get; set; }

        public int ReportId { get; set; } // Logikai kapcsolat a Reports táblához

        public string Size { get; set; } = string.Empty;

        public string Activity { get; set; } = string.Empty;

        public string? Unit { get; set; }

        public string? Equipment { get; set; }
    }
}
