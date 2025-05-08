using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading.Tasks;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace szakdolgozat.Helpers
{
    public static class MapsHelper
    {
        public static async Task CenterMapOnCurrentLocationAsync(Map map)
        {
            try
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
                    var position = new Location(location.Latitude, location.Longitude);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(1)));

                    var pin = new Pin
                    {
                        Label = "Jelenlegi helyzet",
                        Address = "Ez az aktuális helyed automatikusan hozzáadva!",
                        Type = PinType.Place,
                        Location = position
                    };

                    map.Pins.Clear();
                    map.Pins.Add(pin);
                }
                else
                {
                    Console.WriteLine("Nem sikerült meghatározni az aktuális helyzetet.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Helymeghatározási hiba: {ex.Message}");
            }
        }
    }
}
