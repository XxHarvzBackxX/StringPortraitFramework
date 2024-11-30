using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using FuzzySharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    public static Dictionary<string, Patch> PatchDictionary = new Dictionary<string, Patch>();
    public static Dictionary<string, string> StringsFromPatches = new Dictionary<string, string>();
    public static Harmony? HarmonyInstance { get; private set; }
    public const string PATH = "Mods/harv.SPF/Strings";
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        HarmonyInstance = new Harmony(ModManifest.UniqueID);

        helper.Events.Content.AssetRequested += AssetRequested;
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        HarmonyInstance.Patch(AccessTools.Method("Game1:drawObjectDialogue", [typeof(string)]), prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Prefix_OneString))));
        HarmonyInstance.Patch(AccessTools.Method("Game1:drawObjectDialogue", [typeof(List<string>)]), prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Prefix_StringList))));
    }

    public static bool Prefix_OneString(string dialogue)
    {
        foreach (var kvp in StringsFromPatches)
            if (Fuzz.Ratio(kvp.Value, dialogue) >= PatchDictionary[kvp.Key].FuzzRatio) // Match according to fuzziness provided by the patch
            {
                ConstructNewSpeakerDialogue(new List<string> { dialogue }, PatchDictionary[kvp.Key].Image, PatchDictionary[kvp.Key].NPCName, kvp.Key, PatchDictionary[kvp.Key].ShouldTrimColons);
                return false;
            }
        return true;
    }
    public static bool Prefix_StringList(List<string> dialogue)
    {
        foreach (string s in dialogue)
            foreach (var kvp in StringsFromPatches)
                if (Fuzz.Ratio(kvp.Value, s) >= PatchDictionary[kvp.Key].FuzzRatio) // Match according to fuzziness provided by the patch
                {
                    ConstructNewSpeakerDialogue(dialogue, PatchDictionary[kvp.Key].Image, PatchDictionary[kvp.Key].NPCName, kvp.Key, PatchDictionary[kvp.Key].ShouldTrimColons);
                    return false;
                }
        return true;
    }

    public static void ConstructNewSpeakerDialogue(List<string> dialogue, Texture2D image, string speakerName, string key, bool shouldTrimColons)
    {
        string newDialogue = ConvertFromObjectToNPCDialogue(string.Join("#", dialogue), shouldTrimColons);
        var speaker = new NPC(new AnimatedSprite(), Vector2.One * 9999999, "", 0, speakerName, false, image);
        var dialogueInstance = new Dialogue(speaker, key, newDialogue);
        Game1.DrawDialogue(dialogueInstance);
    }

    /// <summary>
    /// Convert hash new dialogues to NPC dialogue new dialogues
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns></returns>
    public static string ConvertFromObjectToNPCDialogue(string dialogue, bool trim)
    {
        var list = dialogue.Split('#');
        string[] newList = new string[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            if (trim)
                newList[i] = list[i].Split(':')[1].Trim();
        }
        return string.Join("#$b#", newList);
    }

    /// <summary>
    /// Populate the patch dictionary when the save is loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        PopulatePatchDictionary();
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested"/>
    internal static void AssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(PATH))
            e.LoadFrom(static () => new Dictionary<string, Patch>(), AssetLoadPriority.Exclusive);
    }
    /// <summary>
    /// Populate the patch dictionary with dynamic tokens parsed also
    /// </summary>
    public void PopulatePatchDictionary()
    {
        StringsFromPatches.Clear();
        PatchDictionary = Helper.GameContent.Load<Dictionary<string, Patch>>(PATH);

        foreach (string key in PatchDictionary.Keys)
        {
            string rawString = Helper.GameContent.Load<Dictionary<string, string>>(key.Split(":")[0])[key.Split(':')[1]];
            StringsFromPatches.Add(key, new Dialogue(null, key, rawString).ReplacePlayerEnteredStrings(rawString));
        }
    }
}