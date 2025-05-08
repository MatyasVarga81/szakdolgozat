using szakdolgozat.Database;
using szakdolgozat.Helpers;
using Microsoft.Maui.Controls.Maps;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using SQLite;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls;
using Newtonsoft.Json.Linq;
using CoordinateSharp;
using System.Text.Json;
using System.Text;

namespace szakdolgozat.Pages
{
    public class SaluteReportDto
    {
        public string Callsign { get; set; }
        public DateTime ObservationTime { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string MGRS { get; set; }
        public string Size { get; set; }
        public string Activity { get; set; }
        public string Unit { get; set; }
        public string Equipment { get; set; }
    }

    public class MethaneReportDto
    {
        public string Callsign { get; set; }
        public DateTime ObservationTime { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string MGRS { get; set; }
        public int NumberOfCasualties { get; set; }
        public string Nationality { get; set; }
        public string Dangers { get; set; }
        public string Approach { get; set; }
        public string AdditionalHelp { get; set; }
    }

    public class MedevacReportDto
    {
        public string Callsign { get; set; }
        public DateTime ObservationTime { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string MGRS { get; set; }
        public int NumberOfCasualties { get; set; }
        public string Nationality { get; set; }
        public string EquipmentNeeded { get; set; }
        public string TypeOfInjuries { get; set; }
        public string SecuritySituation { get; set; }
        public string LocationMarking { get; set; }
        public string LocationEnvironment { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private readonly SQLiteConnection _db;
        private bool _showElevation = false;
        private const string GoogleElevationApiKey = "AIzaSyACd9gmcVAzITbDVBzHub_fGw-zSnLRFqA";

        private bool _routePlanningMode = false;
        private Location? _startLocation;
        public MainPage()
        {
            InitializeComponent();
            UsernameLabel.Text = $"Felhasználó: {App.LoggedInUsername}";
            //  RouteSummaryLabel.IsVisible = false;

            try
            {
                _db = InitializeDatabase();
                _ = RequestLocationPermissionAndInitializeMap();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainPage konstruktorhiba: {ex.Message}");
                Application.Current.MainPage.DisplayAlert("Hiba", $"MainPage betöltési hiba:\n{ex.Message}", "OK");
            }
        }

        private async Task RequestLocationPermissionAndInitializeMap()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var location = await Geolocation.GetLastKnownLocationAsync();
                    if (location == null)
                    {
                        location = await Geolocation.GetLocationAsync(new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Medium,
                            Timeout = TimeSpan.FromSeconds(10)
                        });
                    }

                    if (location != null)
                    {
                        _startLocation = new Location(location.Latitude, location.Longitude);
                        string elevationInfo = _showElevation ? await GetElevationFromGoogleAsync(location.Latitude, location.Longitude) : "";
                        var coordinate = new Coordinate(location.Latitude, location.Longitude);
                        MgrsLabel.Text = $"Koordináta: {location.Latitude:F6}, {location.Longitude:F6} | MGRS: {coordinate.MGRS}{elevationInfo}";


                        var mapLocation = new Location(location.Latitude, location.Longitude);
                        Map.MoveToRegion(MapSpan.FromCenterAndRadius(mapLocation, Microsoft.Maui.Maps.Distance.FromKilometers(1)));

                        var pin = new Pin
                        {
                            Label = "Jelenlegi helyzet",
                            Location = mapLocation,
                            Type = PinType.Place
                        };
                        Map.Pins.Clear();
                        Map.Pins.Add(pin);
                    }
                    else
                    {
                        await DisplayAlert("Hiba", "Nem sikerült lekérni a pozíciót.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Engedély szükséges", "A helyhozzáférés nem engedélyezett.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Engedélykérés hiba: {ex.Message}", "OK");
            }
        }
        private void ClearMap()
        {
            Map.MapElements.Clear();
            Map.Pins.Clear();
            _ = RequestLocationPermissionAndInitializeMap();
            RouteSummaryLabel.IsVisible = false;
        }

        private void OnClearMapClicked(object sender, EventArgs e)
        {
            ClearMap();
        }

        private SQLiteConnection InitializeDatabase()
        {
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "reports.db");
            var database = new SQLiteConnection(dbPath);
            database.CreateTable<SALUTE>();
            database.CreateTable<METHANE>();
            database.CreateTable<MEDEVAC>();
            database.CreateTable<Coordinates>();
            database.CreateTable<Casualties>();
            database.CreateTable<Reports>();
            return database;
        }

        private async void OnCenterMapButtonClicked(object sender, EventArgs e)
        {
            await RequestLocationPermissionAndInitializeMap();
        }

        private async void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            var tappedLocation = e.Location;

            if (_routePlanningMode)
            {
                await PlanRouteAsync(_startLocation, tappedLocation);
                _routePlanningMode = false;
                return;
            }

            string elevationInfo = _showElevation ? await GetElevationFromGoogleAsync(tappedLocation.Latitude, tappedLocation.Longitude) : "";
            MgrsLabel.Text = $"Koordináta: {tappedLocation.Latitude:F6}, {tappedLocation.Longitude:F6}{elevationInfo}";

            var pin = new Pin
            {
                Label = "Kattintott pont",
                Location = tappedLocation,
                Type = PinType.SavedPin
            };
            Map.Pins.Add(pin);
        }
        private async Task PlanRouteAsync(Location origin, Location destination)
        {
            try
            {
                string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin.Latitude},{origin.Longitude}&destination={destination.Latitude},{destination.Longitude}&key={GoogleElevationApiKey}&mode=walking";

                using HttpClient client = new();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);
                var route = obj["routes"]?[0];
                var leg = route?["legs"]?[0];

                if (route != null && leg != null)
                {
                    int distanceMeters = leg["distance"]?["value"]?.ToObject<int>() ?? 0;
                    string distanceKm = $"{distanceMeters / 1000.0:F2} km";

                    string rawDuration = leg["duration"]?["text"]?.ToString() ?? "";
                    string duration = rawDuration
                        .Replace("hours", "óra")
                        .Replace("hour", "óra")
                        .Replace("mins", "perc")
                        .Replace("min", "perc");

                    RouteSummaryLabel.Text = $"Útvonal: {distanceKm}, Idő: {duration}";

                    RouteSummaryLabel.IsVisible = true;

                    string encodedPolyline = route["overview_polyline"]?["points"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(encodedPolyline))
                    {
                        var positions = DecodePolyline(encodedPolyline);
                        var polyline = new Polyline
                        {
                            StrokeColor = Colors.Blue,
                            StrokeWidth = 5
                        };

                        foreach (var pos in positions)
                            polyline.Geopath.Add(pos);

                        Map.MapElements.Clear();
                        Map.MapElements.Add(polyline);
                    }
                }
                else
                {
                    await DisplayAlert("Hiba", "Nem sikerült útvonalat találni.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Útvonalhiba: {ex.Message}", "OK");
            }
        }

        private List<Location> DecodePolyline(string encoded)
        {
            var poly = new List<Location>();
            int index = 0, len = encoded.Length;
            int lat = 0, lng = 0;

            while (index < len)
            {
                int b, shift = 0, result = 0;
                do
                {
                    b = encoded[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                }
                while (b >= 0x20);
                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lat += dlat;

                shift = 0;
                result = 0;
                do
                {
                    b = encoded[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                }
                while (b >= 0x20);
                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;

                double latitude = lat / 1E5;
                double longitude = lng / 1E5;
                poly.Add(new Location(latitude, longitude));
            }

            return poly;
        }

        private async void OnChangeMapTypeClicked(object sender, EventArgs e)
        {
            string result = await DisplayActionSheet("Térképtípus kiválasztása:", "Mégse", null,
                "Utcatérkép", "Műholdkép", "Hibrid");

            switch (result)
            {
                case "Utcatérkép":
                    Map.MapType = MapType.Street;
                    break;
                case "Műholdkép":
                    Map.MapType = MapType.Satellite;
                    break;
                case "Hibrid":
                    Map.MapType = MapType.Hybrid;
                    break;
            }
        }

        private void OnElevationSwitchToggled(object sender, ToggledEventArgs e)
        {
            _showElevation = e.Value;
            _ = RequestLocationPermissionAndInitializeMap();
        }

        private void OnStartRoutePlanningClicked(object sender, EventArgs e)
        {
            _routePlanningMode = true;
            RouteSummaryLabel.IsVisible = false;
            Map.MapElements.Clear();
            DisplayAlert("Útvonaltervezés", "Kérlek kattints a térképen a célpontra.", "OK");
        }

        private async Task<string> GetElevationFromGoogleAsync(double lat, double lon)
        {
            try
            {
                using HttpClient client = new();
                string url = $"https://maps.googleapis.com/maps/api/elevation/json?locations={lat},{lon}&key={GoogleElevationApiKey}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);
                var elevation = obj["results"]?[0]?["elevation"]?.ToObject<double>();

                return elevation.HasValue ? $" | Magasság: {elevation.Value:F1} m" : "";
            }
            catch
            {
                return " | Magasság nem elérhető";
            }
        }

        private async void OnSaluteButtonClicked(object sender, EventArgs e)
        {
            var saluteReport = await GetSaluteReportDetailsAsync();
            SaveSaluteReportToDatabase(saluteReport);
        }

        private async Task<SALUTE> GetSaluteReportDetailsAsync()
        {
            string size = await DisplayPromptAsync("SALUTE Jelentés", "Méret:");
            string activity = await DisplayPromptAsync("SALUTE Jelentés", "Tevékenység:");
            string locationType = await DisplayActionSheet("Helyszín típusa:", "Mégse", null, "MGRS", "Koordináták");

            Coordinates coordinates = new();
            if (locationType == "MGRS")
                coordinates.MGRS = await DisplayPromptAsync("SALUTE Jelentés", "MGRS:");
            else
            {
                string lat = await DisplayPromptAsync("SALUTE Jelentés", "Szélességi fok:");
                string lon = await DisplayPromptAsync("SALUTE Jelentés", "Hosszúsági fok:");
                coordinates.Latitude = float.TryParse(lat, out var la) ? la : 0;
                coordinates.Longitude = float.TryParse(lon, out var lo) ? lo : 0;
            }

            _db.Insert(coordinates);

            string unit = await DisplayPromptAsync("SALUTE Jelentés", "Egység:");
            string time = await DisplayPromptAsync("SALUTE Jelentés", "Idő:");
            string equipment = await DisplayPromptAsync("SALUTE Jelentés", "Felszerelés:");

            var report = new Reports
            {
                Callsign = App.LoggedInUsername,
                ObservationTime = DateTime.TryParse(time, out var t) ? t : DateTime.Now,
                CoordinatesId = coordinates.CoordinatesId
            };
            _db.Insert(report);

            return new SALUTE
            {
                Size = size,
                Activity = activity,
                Unit = unit,
                Equipment = equipment,
                ReportId = report.ReportId
            };
        }

        private async Task SaveSaluteReportToDatabase(SALUTE saluteReport)
        {
            _db.Insert(saluteReport);

            var report = _db.Table<Reports>().FirstOrDefault(r => r.ReportId == saluteReport.ReportId);
            var coord = _db.Find<Coordinates>(report.CoordinatesId);

            var dto = new SaluteReportDto
            {
                Callsign = report.Callsign,
                ObservationTime = report.ObservationTime,
                Latitude = coord.Latitude,
                Longitude = coord.Longitude,
                MGRS = coord.MGRS,
                Size = saluteReport.Size,
                Activity = saluteReport.Activity,
                Unit = saluteReport.Unit,
                Equipment = saluteReport.Equipment
            };

            await SendReportToServer(dto);
            await DisplayAlert("Siker", "SALUTE jelentés mentve és elküldve!", "OK");
        }

        private async void OnMethaneButtonClicked(object sender, EventArgs e)
        {
            var methaneReport = await GetMethaneReportDetailsAsync();
            SaveMethaneReportToDatabase(methaneReport);
        }

        private async Task<METHANE> GetMethaneReportDetailsAsync()
        {
            string time = await DisplayPromptAsync("METHANE Jelentés", "Esemény ideje:");
            string locationType = await DisplayActionSheet("Helyszín típusa:", "Mégse", null, "MGRS", "Koordináták");

            Coordinates coordinates = new();
            if (locationType == "MGRS")
                coordinates.MGRS = await DisplayPromptAsync("METHANE Jelentés", "MGRS:");
            else
            {
                string lat = await DisplayPromptAsync("METHANE Jelentés", "Szélességi fok:");
                string lon = await DisplayPromptAsync("METHANE Jelentés", "Hosszúsági fok:");
                coordinates.Latitude = float.TryParse(lat, out var la) ? la : 0;
                coordinates.Longitude = float.TryParse(lon, out var lo) ? lo : 0;
            }

            _db.Insert(coordinates);

            string num = await DisplayPromptAsync("METHANE Jelentés", "Áldozatok száma:");
            string nationality = await DisplayPromptAsync("METHANE Jelentés", "Nemzetiség:");
            string dangers = await DisplayPromptAsync("METHANE Jelentés", "Veszélyek:");
            string approach = await DisplayPromptAsync("METHANE Jelentés", "Megközelítés:");
            string help = await DisplayPromptAsync("METHANE Jelentés", "További segítség:");

            var casualties = new Casualties
            {
                NumberOfCasualties = int.TryParse(num, out var c) ? c : 0,
                Nationality = nationality
            };
            _db.Insert(casualties);

            var report = new Reports
            {
                Callsign = App.LoggedInUsername,
                ObservationTime = DateTime.TryParse(time, out var t) ? t : DateTime.Now,
                CoordinatesId = coordinates.CoordinatesId,
                CasualtiesId = casualties.CasualtiesId
            };
            _db.Insert(report);

            return new METHANE
            {
                Dangers = dangers,
                Approach = approach,
                AdditionalHelp = help,
                ReportId = report.ReportId
            };
        }

        private async Task SaveMethaneReportToDatabase(METHANE methaneReport)
        {
            _db.Insert(methaneReport);

            var report = _db.Table<Reports>().FirstOrDefault(r => r.ReportId == methaneReport.ReportId);
            var coord = _db.Find<Coordinates>(report.CoordinatesId);
            var casualties = _db.Find<Casualties>(report.CasualtiesId);

            var dto = new MethaneReportDto
            {
                Callsign = report.Callsign,
                ObservationTime = report.ObservationTime,
                Latitude = coord.Latitude,
                Longitude = coord.Longitude,
                MGRS = coord.MGRS,
                NumberOfCasualties = (int)casualties.NumberOfCasualties,
                Nationality = casualties.Nationality,
                Dangers = methaneReport.Dangers,
                Approach = methaneReport.Approach,
                AdditionalHelp = methaneReport.AdditionalHelp
            };

            await SendReportToServer(dto);
            await DisplayAlert("Siker", "METHANE jelentés mentve és elküldve!", "OK");
        }


        private async void OnMedevacButtonClicked(object sender, EventArgs e)
        {
            var medevacReport = await GetMedevacReportDetailsAsync();
            SaveMedevacReportToDatabase(medevacReport);
        }

        private async Task<MEDEVAC> GetMedevacReportDetailsAsync()
        {
            string time = await DisplayPromptAsync("MEDEVAC Jelentés", "Esemény ideje:");
            string locationType = await DisplayActionSheet("Helyszín típusa:", "Mégse", null, "MGRS", "Koordináták");

            Coordinates coordinates = new();
            if (locationType == "MGRS")
                coordinates.MGRS = await DisplayPromptAsync("MEDEVAC Jelentés", "MGRS:");
            else
            {
                string lat = await DisplayPromptAsync("MEDEVAC Jelentés", "Szélességi fok:");
                string lon = await DisplayPromptAsync("MEDEVAC Jelentés", "Hosszúsági fok:");
                coordinates.Latitude = float.TryParse(lat, out var la) ? la : 0;
                coordinates.Longitude = float.TryParse(lon, out var lo) ? lo : 0;
            }

            _db.Insert(coordinates);

            string num = await DisplayPromptAsync("MEDEVAC Jelentés", "Áldozatok száma:");
            string nationality = await DisplayPromptAsync("MEDEVAC Jelentés", "Nemzetiség:");
            string equipment = await DisplayPromptAsync("MEDEVAC Jelentés", "Felszerelés:");
            string injuries = await DisplayPromptAsync("MEDEVAC Jelentés", "Sérülések:");
            string security = await DisplayPromptAsync("MEDEVAC Jelentés", "Biztonsági helyzet:");
            string marking = await DisplayPromptAsync("MEDEVAC Jelentés", "Helyszín jelölése:");
            string environment = await DisplayPromptAsync("MEDEVAC Jelentés", "Helyszín környezete:");

            var casualties = new Casualties
            {
                NumberOfCasualties = int.TryParse(num, out var c) ? c : 0,
                Nationality = nationality
            };
            _db.Insert(casualties);

            var report = new Reports
            {
                Callsign = App.LoggedInUsername,
                ObservationTime = DateTime.TryParse(time, out var t) ? t : DateTime.Now,
                CoordinatesId = coordinates.CoordinatesId,
                CasualtiesId = casualties.CasualtiesId
            };
            _db.Insert(report);

            return new MEDEVAC
            {
                EquipmentNeeded = equipment,
                TypeOfInjuries = injuries,
                SecuritySituation = security,
                LocationMarking = marking,
                LocationEnvironment = environment,
                ReportId = report.ReportId
            };
        }

        private async Task SaveMedevacReportToDatabase(MEDEVAC medevacReport)
        {
            _db.Insert(medevacReport);

            var report = _db.Table<Reports>().FirstOrDefault(r => r.ReportId == medevacReport.ReportId);
            var coord = _db.Find<Coordinates>(report.CoordinatesId);
            var casualties = _db.Find<Casualties>(report.CasualtiesId);

            var dto = new MedevacReportDto
            {
                Callsign = report.Callsign,
                ObservationTime = report.ObservationTime,
                Latitude = coord.Latitude,
                Longitude = coord.Longitude,
                MGRS = coord.MGRS,
                NumberOfCasualties = (int)casualties.NumberOfCasualties,
                Nationality = casualties.Nationality,
                EquipmentNeeded = medevacReport.EquipmentNeeded,
                TypeOfInjuries = medevacReport.TypeOfInjuries,
                SecuritySituation = medevacReport.SecuritySituation,
                LocationMarking = medevacReport.LocationMarking,
                LocationEnvironment = medevacReport.LocationEnvironment
            };

            await SendReportToServer(dto);
            await DisplayAlert("Siker", "MEDEVAC jelentés mentve és elküldve!", "OK");
        }



        private async Task SendReportToServer(object reportData)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                string json = JsonSerializer.Serialize(reportData, options);

                Console.WriteLine("Küldött JSON:");
                Console.WriteLine(json);

                using var client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, "http://10.0.2.2/szakdolgozat_api/receive_report.php")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Accept", "application/json");

                var response = await client.SendAsync(request);

                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Szerver válasz:");
                Console.WriteLine(result);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Szerverhiba", $"Status: {response.StatusCode}\nVálasz: {result}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült az adatküldés: {ex.Message}", "OK");
            }
        }



    }
}
