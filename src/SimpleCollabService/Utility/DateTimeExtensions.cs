namespace SimpleCollabService.Utility;

static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dateTime) =>
        (dateTime.Ticks - DateTime.UnixEpoch.Ticks) / TimeSpan.TicksPerSecond;

    public static DateTime FromUnixTimeSeconds(long seconds) =>
        DateTime.UnixEpoch.AddSeconds(seconds);
}
