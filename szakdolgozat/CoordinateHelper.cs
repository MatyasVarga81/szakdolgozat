using CoordinateSharp;

namespace szakdolgozat.Helpers
{
    public static class CoordinateHelper
    {
        public static string ConvertToMgrs(double latitude, double longitude)
        {
            var coordinate = new Coordinate(latitude, longitude);
            return coordinate.MGRS.ToString(); // Például: "33TWN1234567890"
        }
    }
}
