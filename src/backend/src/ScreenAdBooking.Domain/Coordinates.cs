namespace ScreenAdBooking.Domain;

internal static class Coordinates
{
    public static double Latitude(double value, string paramName) =>
        double.IsFinite(value) && value is >= -90 and <= 90
            ? value
            : throw new ArgumentOutOfRangeException(paramName, value, "Latitude must be between -90 and 90.");

    public static double Longitude(double value, string paramName) =>
        double.IsFinite(value) && value is >= -180 and <= 180
            ? value
            : throw new ArgumentOutOfRangeException(paramName, value, "Longitude must be between -180 and 180.");
}
