namespace ReColor;

public partial class ReColorPlugin
{
    public const string PluginGuid = "com.archives.recolor";
    internal const string PluginName = "ReColor";
    internal const string PluginVersion = "1.0.0";
    internal const string PluginAuthor = "Archives";

    internal const string UpdateCheckName = "UpdateCheck";

    internal const string UpdateCheckPurpose =
        "Looks up the latest release on github.com once at startup and logs a line when yours is older. "
        + "Sends nothing about you. Set to false to keep the mod entirely offline.";

    internal const bool DefaultUpdateCheck = true;

    private static void RunSelfChecks()
    {
        bool hasPassedSelfCheck = UpdateCheck.SelfCheck();
        string outcome = hasPassedSelfCheck ? "Loaded. Self-check passed." : "Loaded. SELF-CHECK FAILED.";

        Log.Info(outcome);
    }

    private static void Tick()
    {
        SurfaceLook.Tick();
    }
}
