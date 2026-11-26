namespace HarjumusteRPG.Mobile.Models;

// The API sends the achievement texts in Estonian, so the app shows the translated
// text for the known achievements (by Id). New achievements (e.g. added by an admin)
// keep the text from the API.
public static class AchievementTexts
{
    public static string Name(AchievementInfo a) => a.Id switch
    {
        1 => AppResources.Achievement1Name,
        2 => AppResources.Achievement2Name,
        3 => AppResources.Achievement3Name,
        4 => AppResources.Achievement4Name,
        5 => AppResources.Achievement5Name,
        _ => a.Name
    };

    public static string Description(AchievementInfo a) => a.Id switch
    {
        1 => AppResources.Achievement1Desc,
        2 => AppResources.Achievement2Desc,
        3 => AppResources.Achievement3Desc,
        4 => AppResources.Achievement4Desc,
        5 => AppResources.Achievement5Desc,
        _ => a.Description
    };
}