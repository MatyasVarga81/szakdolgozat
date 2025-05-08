using SQLite;
using System;

namespace szakdolgozat.Database
{
    public class METHANE
    {
        [PrimaryKey, AutoIncrement]
        public int MethaneId { get; set; }

        public int ReportId { get; set; }  // logikai kapcsolat

        public string? Dangers { get; set; }

        public string? Approach { get; set; }

        public string? AdditionalHelp { get; set; }
    }
}
