using SQLite;
using System;

namespace szakdolgozat.Database
{
    public class MEDEVAC
    {
        [PrimaryKey, AutoIncrement]
        public int MedevacId { get; set; }

        // Kapcsolat logikailag (nem FOREIGN KEY)
        public int ReportId { get; set; }

        public string? EquipmentNeeded { get; set; }

        public string? TypeOfInjuries { get; set; }

        public string? SecuritySituation { get; set; }

        public string? LocationMarking { get; set; }

        public string? LocationEnvironment { get; set; }
    }
}
