namespace KmTimer.Updates;

public sealed record AppReleaseProfile(
    string GitHubRepo,
    string ReleaseTagPrefix,
    string AssetNamePattern,
    string DisplayName)
{
    public const string DefaultRepo = "honi0907/km-timer";

    public static AppReleaseProfile Default { get; } = new(
        DefaultRepo,
        "v",
        "kmtimer",
        "KM Timer");
}
