using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace PuppetEnemy;

[BepInPlugin("VyrusGames.PuppetEnemy", "PuppetEnemy", "1.1.0")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class PuppetEnemy : BaseUnityPlugin
{
    internal static PuppetEnemy Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    private void Awake()
    {
        Instance = this;
        
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        
        Logger.LogInfo("Patching PuppetEnemy...");
        
        Logger.LogInfo("Loading assets...");
        LoadAssets();
        
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }
    
    private static void LoadAssets()
    {
        AssetBundle puppetAssetBundle = LoadAssetBundle("puppet");

        Logger.LogInfo("Loading Puppet enemy setup...");
        EnemySetup puppetEnemySetup = puppetAssetBundle.LoadAsset<EnemySetup>("Assets/REPO/Mods/plugins/PuppetEnemy/Enemy - Puppet.asset");
        
        Enemies.RegisterEnemy(puppetEnemySetup);
        
        Logger.LogDebug("Loaded Puppet enemy!");
    }
    
    public static AssetBundle LoadAssetBundle(string name)
    {
        Logger.LogDebug("Loading Asset Bundle: " + name);
        AssetBundle bundle = null;
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), name);
        bundle = AssetBundle.LoadFromFile(path);
        return bundle;
    }
}