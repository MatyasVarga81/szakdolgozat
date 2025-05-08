using SQLite;
using System;

namespace szakdolgozat.Database
{
    public class Reports
    {
        [PrimaryKey, AutoIncrement]
        public int ReportId { get; set; }

        public string? ReportType { get; set; }

        public DateTime? ReportDate { get; set; }

        public DateTime ObservationTime { get; set; }

        // Idegen kulcs ID-k (nem igazi relációs FK, de logikailag igen)
        public int? CoordinatesId { get; set; }

        public int? CasualtiesId { get; set; }

        public string Callsign { get; set; } = string.Empty;
    }
}
