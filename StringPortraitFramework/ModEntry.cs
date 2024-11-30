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
    public static ModEntry? Instance { get; private set; }
    public static Dictionary<string, Patch> PatchDictionary = new Dictionary<string, Patch>();
    public static Dictionary<string, string> StringsFromPatches = new Dictionary<string, string>();
    public static Harmony? HarmonyInstance { get; private set; }
    public const string PATH = "Mods/harv.SPF/Strings";
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Instance = this;
        HarmonyInstance = new Harmony(ModManifest.UniqueID);

        helper.Events.Content.AssetRequested += AssetRequested;
        helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        HarmonyInstance.Patch(AccessTools.Method("Game1:drawObjectDialogue", new[] { typeof(string)}), prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Prefix_OneString))));
        HarmonyInstance.Patch(AccessTools.Method("Game1:drawObjectDialogue", new[] { typeof(List<string>) } ), prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Prefix_StringList))));
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
        string newDialogue;
        newDialogue = ConvertFromObjectToNPCDialogue(string.Join("#", dialogue), shouldTrimColons);
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
        if (string.IsNullOrWhiteSpace(dialogue))
        {
            throw new ArgumentException("Dialogue string cannot be null or whitespace.", nameof(dialogue));
        }

        // Split by `#` if present; otherwise, treat the entire string as a single entry.
        var list = dialogue.Contains("#") ? dialogue.Split('#') : new[] { dialogue };
        string[] newList = new string[list.Length];

        for (int i = 0; i < list.Length; i++)
        {
            // Check if the entry contains a colon before attempting to split.
            if (trim && list[i].Contains(":"))
            {
                newList[i] = list[i].Split(new[] { ':' }, 2)[1].Trim(); // Use max count of 2 to avoid over-splitting.
            }
            else
            {
                newList[i] = list[i].Trim(); // Keep the text as-is, trimming whitespace.
            }
        }

        return string.Join("#$b#", newList).Replace("^", "#$b#");
    }

    /// <summary>
    /// Populate the patch dictionary when the day starts
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
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