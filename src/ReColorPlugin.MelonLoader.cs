#if MELONLOADER
using MelonLoader;
using RestoryReColor;

[assembly: MelonInfo(typeof(ReColorPlugin), ReColorPlugin.PluginName, ReColorPlugin.PluginVersion,
    ReColorPlugin.PluginAuthor)]
[assembly: MelonGame("Mandragora", "Restory")]

namespace RestoryReColor;

public partial class ReColorPlugin : MelonMod
{
    private const string GeneralSection = "RestoryReColorGeneral";

    internal static MelonPreferences_Entry<bool> UpdateCheckEnabled;

    private static MelonPreferences_Category _general;

    public override void OnEarlyInitializeMelon()
    {
        Log.Source = LoggerInstance;
    }

    public override void OnInitializeMelon()
    {
        BindConfig();

        RunSelfChecks();

        if (UpdateCheckEnabled.Value)
        {
            MelonCoroutines.Start(UpdateCheck.Run(PluginVersion));
        }
    }

    public override void OnUpdate()
    {
        Tick();
    }

    private void BindConfig()
    {
        _general = MelonPreferences.CreateCategory(GeneralSection, $"{PluginName} - General");

        UpdateCheckEnabled = _general.CreateEntry(UpdateCheckName, DefaultUpdateCheck,
            description: UpdateCheckPurpose);

        MelonPreferences.Save();
    }
}
#endif
