namespace HarjumusteRPG.Mobile.Services;

// "Today" as the API sees it. The demo button in settings can move the API clock forward,
// so the app takes the date from the API instead of the phone.
public static class AppClock
{
    private static int _daysAhead;

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today).AddDays(_daysAhead);

    public static void SetServerToday(DateOnly serverToday)
    {
        if (serverToday == default)
            return;

        _daysAhead = serverToday.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
    }
}