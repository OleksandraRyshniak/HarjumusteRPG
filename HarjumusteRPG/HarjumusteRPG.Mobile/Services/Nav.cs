namespace HarjumusteRPG.Mobile.Services;

// All page changes go through here. If Shell is still finishing the previous
// navigation ("Pending Navigations still processing"), wait a moment and try again.
public static class Nav
{
    public static async Task GoAsync(string route)
    {
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
            catch (InvalidOperationException)
            {
                // still busy after several tries: skip this tap instead of crashing
                return;
            }
        }
    }
}