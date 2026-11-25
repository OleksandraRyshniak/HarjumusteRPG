// Typed access to Resources/Strings/AppResources.resx (Estonian) and AppResourcesEn.resx (English).
// Both files are embedded in the main assembly (no satellite assemblies), the language comes from Loc.
// Keys are checked by the compiler.
using System.Globalization;
using System.Resources;

namespace HarjumusteRPG.Mobile;

public static class AppResources
{
    private static ResourceManager? _estonian;
    private static ResourceManager? _english;

    private static ResourceManager Create(string suffix)
    {
        var assembly = typeof(AppResources).Assembly;
        var name = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith($"AppResources{suffix}.resources", StringComparison.Ordinal));
        return new ResourceManager(name[..^".resources".Length], assembly);
    }

    private static string Get(string key)
    {
        ResourceManager manager;
        if (Loc.IsEnglish)
            manager = _english ??= Create("En");
        else
            manager = _estonian ??= Create(string.Empty);

        return manager.GetString(key, CultureInfo.InvariantCulture) ?? key;
    }

    public static string NavToday => Get(nameof(NavToday));
    public static string NavPet => Get(nameof(NavPet));
    public static string NavStats => Get(nameof(NavStats));
    public static string NavLeaderboard => Get(nameof(NavLeaderboard));
    public static string NavShop => Get(nameof(NavShop));
    public static string Error => Get(nameof(Error));
    public static string Back => Get(nameof(Back));
    public static string BackToday => Get(nameof(BackToday));
    public static string Save => Get(nameof(Save));
    public static string Cancel => Get(nameof(Cancel));
    public static string Continue => Get(nameof(Continue));
    public static string Start => Get(nameof(Start));
    public static string Yes => Get(nameof(Yes));
    public static string No => Get(nameof(No));
    public static string Settings => Get(nameof(Settings));
    public static string SettingsTitle => Get(nameof(SettingsTitle));
    public static string LanguageTitle => Get(nameof(LanguageTitle));
    public static string ThemeTitle => Get(nameof(ThemeTitle));
    public static string ThemeCandy => Get(nameof(ThemeCandy));
    public static string ThemeMint => Get(nameof(ThemeMint));
    public static string ThemeNight => Get(nameof(ThemeNight));
    public static string LoginTitle => Get(nameof(LoginTitle));
    public static string LoginButton => Get(nameof(LoginButton));
    public static string Logout => Get(nameof(Logout));
    public static string Email => Get(nameof(Email));
    public static string Password => Get(nameof(Password));
    public static string GoRegister => Get(nameof(GoRegister));
    public static string RegisterTitle => Get(nameof(RegisterTitle));
    public static string RegisterButton => Get(nameof(RegisterButton));
    public static string UsernameField => Get(nameof(UsernameField));
    public static string PasswordField => Get(nameof(PasswordField));
    public static string RepeatPassword => Get(nameof(RepeatPassword));
    public static string GoLogin => Get(nameof(GoLogin));
    public static string ErrEnterEmail => Get(nameof(ErrEnterEmail));
    public static string ErrEmailInvalid => Get(nameof(ErrEmailInvalid));
    public static string ErrEnterPassword => Get(nameof(ErrEnterPassword));
    public static string ErrUsernameLength => Get(nameof(ErrUsernameLength));
    public static string ErrEmailTooLong => Get(nameof(ErrEmailTooLong));
    public static string ErrPasswordLength => Get(nameof(ErrPasswordLength));
    public static string ErrPasswordsMismatch => Get(nameof(ErrPasswordsMismatch));
    public static string GreetingFormat => Get(nameof(GreetingFormat));
    public static string PetSetupTitle => Get(nameof(PetSetupTitle));
    public static string PetSetupHint => Get(nameof(PetSetupHint));
    public static string PetNameField => Get(nameof(PetNameField));
    public static string ErrPetName => Get(nameof(ErrPetName));
    public static string PetTitle => Get(nameof(PetTitle));
    public static string LevelFormat => Get(nameof(LevelFormat));
    public static string NextLevelFormat => Get(nameof(NextLevelFormat));
    public static string StreakCaption => Get(nameof(StreakCaption));
    public static string LongestStreakCaption => Get(nameof(LongestStreakCaption));
    public static string CompletionsCaption => Get(nameof(CompletionsCaption));
    public static string CoinsCaption => Get(nameof(CoinsCaption));
    public static string Achievements => Get(nameof(Achievements));
    public static string AchievementsSummary => Get(nameof(AchievementsSummary));
    public static string FeedFormat => Get(nameof(FeedFormat));
    public static string PetWaitingFormat => Get(nameof(PetWaitingFormat));
    public static string PetHappyFormat => Get(nameof(PetHappyFormat));
    public static string PetGoodFormat => Get(nameof(PetGoodFormat));
    public static string StreakFormat => Get(nameof(StreakFormat));
    public static string StreakStart => Get(nameof(StreakStart));
    public static string TodayEmpty => Get(nameof(TodayEmpty));
    public static string ManageHabits => Get(nameof(ManageHabits));
    public static string RewardHint => Get(nameof(RewardHint));
    public static string LevelUpTitle => Get(nameof(LevelUpTitle));
    public static string NewAchievementTitle => Get(nameof(NewAchievementTitle));
    public static string RewardFormat => Get(nameof(RewardFormat));
    public static string NewAchievementOne => Get(nameof(NewAchievementOne));
    public static string NewAchievementMany => Get(nameof(NewAchievementMany));
    public static string LevelUpMessageFormat => Get(nameof(LevelUpMessageFormat));
    public static string HabitsTitle => Get(nameof(HabitsTitle));
    public static string SearchHabit => Get(nameof(SearchHabit));
    public static string FilterAll => Get(nameof(FilterAll));
    public static string FilterActive => Get(nameof(FilterActive));
    public static string ActiveLabel => Get(nameof(ActiveLabel));
    public static string Paused => Get(nameof(Paused));
    public static string NewShort => Get(nameof(NewShort));
    public static string HabitsNotFound => Get(nameof(HabitsNotFound));
    public static string SortName => Get(nameof(SortName));
    public static string SortNewest => Get(nameof(SortNewest));
    public static string PageFormat => Get(nameof(PageFormat));
    public static string TotalFormat => Get(nameof(TotalFormat));
    public static string EveryDay => Get(nameof(EveryDay));
    public static string NewHabitTitle => Get(nameof(NewHabitTitle));
    public static string EditHabitTitle => Get(nameof(EditHabitTitle));
    public static string HabitName => Get(nameof(HabitName));
    public static string Weekdays => Get(nameof(Weekdays));
    public static string Icon => Get(nameof(Icon));
    public static string Delete => Get(nameof(Delete));
    public static string ConfirmDelete => Get(nameof(ConfirmDelete));
    public static string ErrHabitName => Get(nameof(ErrHabitName));
    public static string ErrPickWeekday => Get(nameof(ErrPickWeekday));
    public static string History => Get(nameof(History));
    public static string HistoryTitle => Get(nameof(HistoryTitle));
    public static string HistoryLegend => Get(nameof(HistoryLegend));
    public static string HistoryEmpty => Get(nameof(HistoryEmpty));
    public static string HistorySummaryFormat => Get(nameof(HistorySummaryFormat));
    public static string AchievementsTitle => Get(nameof(AchievementsTitle));
    public static string EarnedFormat => Get(nameof(EarnedFormat));
    public static string EarnedSummaryFormat => Get(nameof(EarnedSummaryFormat));
    public static string Locked => Get(nameof(Locked));
    public static string StatsTitle => Get(nameof(StatsTitle));
    public static string Month => Get(nameof(Month));
    public static string TotalXp => Get(nameof(TotalXp));
    public static string Last7Days => Get(nameof(Last7Days));
    public static string Last30Days => Get(nameof(Last30Days));
    public static string LeaderboardTitle => Get(nameof(LeaderboardTitle));
    public static string InvitesTitle => Get(nameof(InvitesTitle));
    public static string SendInvite => Get(nameof(SendInvite));
    public static string Remove => Get(nameof(Remove));
    public static string ErrFriendName => Get(nameof(ErrFriendName));
    public static string InviteSentFormat => Get(nameof(InviteSentFormat));
    public static string NowFriendsFormat => Get(nameof(NowFriendsFormat));
    public static string RemovedFriendFormat => Get(nameof(RemovedFriendFormat));
    public static string YouFormat => Get(nameof(YouFormat));
    public static string LevelXpFormat => Get(nameof(LevelXpFormat));
    public static string DaysShortFormat => Get(nameof(DaysShortFormat));
    public static string FriendLevelStreakFormat => Get(nameof(FriendLevelStreakFormat));
    public static string ErrConnection => Get(nameof(ErrConnection));
    public static string ErrUnknown => Get(nameof(ErrUnknown));
    public static string ErrSessionExpired => Get(nameof(ErrSessionExpired));
    public static string ErrWrongCredentials => Get(nameof(ErrWrongCredentials));
    public static string ErrTaken => Get(nameof(ErrTaken));
    public static string ErrCheckData => Get(nameof(ErrCheckData));
    public static string ErrHabitExists => Get(nameof(ErrHabitExists));
    public static string ErrHabitNotFound => Get(nameof(ErrHabitNotFound));
    public static string ErrUserNotFound => Get(nameof(ErrUserNotFound));
    public static string ErrInviteExists => Get(nameof(ErrInviteExists));
    public static string ErrFriendNameCheck => Get(nameof(ErrFriendNameCheck));
    public static string ErrInviteNotFound => Get(nameof(ErrInviteNotFound));
    public static string ErrFriendNotFound => Get(nameof(ErrFriendNotFound));
    public static string ErrAlreadyDone => Get(nameof(ErrAlreadyDone));
    public static string ErrCannotMarkToday => Get(nameof(ErrCannotMarkToday));
    public static string TabWeek => Get(nameof(TabWeek));
    public static string StatsEmpty => Get(nameof(StatsEmpty));
    public static string StatsActiveDays => Get(nameof(StatsActiveDays));
    public static string StatsBestDay => Get(nameof(StatsBestDay));
    public static string TabFriends => Get(nameof(TabFriends));
    public static string OnlyMeHint => Get(nameof(OnlyMeHint));
    public static string AddFriendTitle => Get(nameof(AddFriendTitle));
    public static string FriendNameField => Get(nameof(FriendNameField));
    public static string FriendsTitle => Get(nameof(FriendsTitle));
    public static string NoFriends => Get(nameof(NoFriends));
    public static string WeekdayLetters => Get(nameof(WeekdayLetters));
    public static string Achievement1Name => Get(nameof(Achievement1Name));
    public static string Achievement1Desc => Get(nameof(Achievement1Desc));
    public static string Achievement2Name => Get(nameof(Achievement2Name));
    public static string Achievement2Desc => Get(nameof(Achievement2Desc));
    public static string Achievement3Name => Get(nameof(Achievement3Name));
    public static string Achievement3Desc => Get(nameof(Achievement3Desc));
    public static string Achievement4Name => Get(nameof(Achievement4Name));
    public static string Achievement4Desc => Get(nameof(Achievement4Desc));
    public static string Achievement5Name => Get(nameof(Achievement5Name));
    public static string Achievement5Desc => Get(nameof(Achievement5Desc));
    public static string PetGreyCat => Get(nameof(PetGreyCat));
    public static string PetDog => Get(nameof(PetDog));
    public static string GiftTitle => Get(nameof(GiftTitle));
    public static string GiftHint => Get(nameof(GiftHint));
    public static string GiftClaimed => Get(nameof(GiftClaimed));
    public static string GiftReceivedFormat => Get(nameof(GiftReceivedFormat));
    public static string ErrGiftClaimed => Get(nameof(ErrGiftClaimed));
}
