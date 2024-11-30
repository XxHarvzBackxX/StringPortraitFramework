using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

/// <summary>
/// The actual patch to be applied. The string path will be the <see cref="string"> key in the <see cref="Dictionary{string, Patch}"/>. Key in format: "file/path:stringkey.0", e.g: "Strings/StringsFromCSFiles:Npc.0001"
/// </summary>
public class Patch
{
    public string ImagePath { get; set; }
    public string NPCName { get; set; }
    internal Texture2D Image { get; set; }
    public bool ShouldTrimColons { get; set; }
    public float FuzzRatio { get; set; }
    public Patch(string imagePath, bool trimColons, string npcName)
    {
        ImagePath = imagePath;
        ShouldTrimColons = trimColons;
        NPCName = npcName;
        Image = ModEntry.Instance!.Helper.GameContent.Load<Texture2D>(imagePath);
    }
}
