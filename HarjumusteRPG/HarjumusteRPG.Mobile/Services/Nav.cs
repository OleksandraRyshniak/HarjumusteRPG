namespace HarjumusteRPG.Mobile.Services;

// All page changes go through here, strictly one at a time.
// Shell (especially on Windows) throws "Pending Navigations still processing"
// if a new navigation starts before the previous one is fully finished.
public static class Nav
{
    private static readonly SemaphoreSlim _gate = new(1, 1);

    public static async Task GoAsync(string route)
    {
        await _gate.WaitAsync();
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // already on this page (e.g. several "session expired" -> //login at once)
                var current = Shell.Current.CurrentState?.Location.OriginalString ?? string.Empty;
                if (route.StartsWith("//") && current.StartsWith(route))
                    return;

                for (var attempt = 1; attempt <= 5; attempt++)
                {
                    try
                    {
                        await Shell.Current.GoToAsync(route);
                        return;
                    }
                    catch (InvalidOperationException) when (attempt < 5)
                    {
                        await Task.Delay(300);
                    }
                }
            });

            // Shell finishes some work after GoToAsync returns: give it a moment
            await Task.Delay(150);
        }
        catch (InvalidOperationException)
        {
            // still busy after several tries: skip instead of crashing
        }
        finally
        {
            _gate.Release();
        }
    }
}