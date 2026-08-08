using AmongUs.Data;
using System.IO;
using System.Linq;
using HarmonyLib;

namespace SkidMenu;

public static class SavedPlayerInfo
{
    public static bool   HasData     = false;
    public static string Name        = "";
    public static string HatId       = "";
    public static string SkinId      = "";
    public static string VisorId     = "";
    public static string PetId       = "";
    public static string NameplateId = "";
    public static int    ColorId     = 0;
    public static int    Level       = 0;
    public static string Platform    = "";

    public static string FolderPath => Path.Combine(BepInEx.Paths.GameRootPath, "SkidMenu", "PlayerInfos");

    private static string SafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return $"{name}_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
    }

    public static void SaveToDisk(string playerName)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            var path = Path.Combine(FolderPath, SafeFileName(playerName));
            File.WriteAllLines(path, new[]
            {
                $"Name={Name}",
                $"HatId={HatId}",
                $"SkinId={SkinId}",
                $"VisorId={VisorId}",
                $"PetId={PetId}",
                $"NameplateId={NameplateId}",
                $"ColorId={ColorId}",
                $"Level={Level}",
                $"Platform={Platform}"
            });
        }
        catch { }
    }

    public static bool LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return false;
            foreach (var line in File.ReadAllLines(filePath))
            {
                int idx = line.IndexOf('=');
                if (idx < 0) continue;
                string key = line.Substring(0, idx);
                string val = line.Substring(idx + 1);
                switch (key)
                {
                    case "Name":        Name        = val; break;
                    case "HatId":       HatId       = val; break;
                    case "SkinId":      SkinId      = val; break;
                    case "VisorId":     VisorId     = val; break;
                    case "PetId":       PetId       = val; break;
                    case "NameplateId": NameplateId = val; break;
                    case "ColorId":     int.TryParse(val, out ColorId);  break;
                    case "Level":       int.TryParse(val, out Level);    break;
                    case "Platform":    Platform    = val; break;
                }
            }
            HasData = !string.IsNullOrEmpty(Name);
            return HasData;
        }
        catch { return false; }
    }

    public static string[] ListSaves()
    {
        try
        {
            if (!Directory.Exists(FolderPath)) return System.Array.Empty<string>();
            return Directory.GetFiles(FolderPath, "*.txt").OrderBy(f => f).ToArray();
        }
        catch { return System.Array.Empty<string>(); }
    }

    public static void ApplyToLocalPlayer()
    {
        if (!HasData) return;
        try
        {
            // Always write to DataManager — works even in main menu
            DataManager.Player.Customization.Color     = (byte)ColorId;
            DataManager.Player.Customization.Hat       = HatId;
            DataManager.Player.Customization.Skin      = SkinId;
            DataManager.Player.Customization.Visor     = VisorId;
            DataManager.Player.Customization.Pet       = PetId;
            DataManager.Player.Customization.NamePlate = NameplateId;
            DataManager.Player.Customization.Name      = Name;
            SkidMenu.nameSpoofName.Value    = Name;
            SkidMenu.nameSpoofEnabled.Value = true;
            features.NameSpoofer.ApplyName(Name);

            // RPC calls only work in-lobby
            var lp = PlayerControl.LocalPlayer;
            if (lp == null) return;
            OutfitBypass.SetColor(ColorId);
            lp.RpcSetHat(HatId);
            lp.RpcSetSkin(SkinId);
            lp.RpcSetVisor(VisorId);
            lp.RpcSetPet(PetId);
            if (!string.IsNullOrEmpty(NameplateId)) lp.RpcSetNamePlate(NameplateId);
            OutfitBypass.SetName(Name);
            if (Level > 0) SkidMenu.spoofLevel.Value = Level.ToString();
            if (!string.IsNullOrEmpty(Platform)) SkidMenu.spoofPlatform.Value = Platform;
        }
        catch { }
    }

    // Re-apply saved outfit every time we spawn into a lobby or game
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class Patch_ReapplyOnJoin
    {
        public static void Postfix()
        {
            if (!HasData || PlayerControl.LocalPlayer == null) return;
            ApplyToLocalPlayer();
        }
    }
}
