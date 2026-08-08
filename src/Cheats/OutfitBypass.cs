using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using InnerNet;

namespace SkidMenu;

internal static class OutfitBypass
{
    public static void SetColor(int colorId)
    {
        var player = PlayerControl.LocalPlayer;
        if (player?.Data == null) return;

        player.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
        DataManager.Player.Customization.Color = (byte)colorId;
        try { player.cosmetics.SetColor(colorId); } catch { }

        if (AmongUsClient.Instance.AmHost)
            try { player.RpcSetColor((byte)colorId); } catch { }
        else
            try { player.CmdCheckColor((byte)colorId); } catch { }
    }

    public static void SetName(string playerName)
    {
        if (string.IsNullOrEmpty(playerName)) return;
        var player = PlayerControl.LocalPlayer;
        if (player?.Data == null) return;

        player.Data.Outfits[PlayerOutfitType.Default].PlayerName = playerName;
        DataManager.Player.Customization.Name = playerName;
        try { player.cosmetics.SetName(playerName); } catch { }

        if (AmongUsClient.Instance.AmHost)
            try { player.RpcSetName(playerName); } catch { }
    }
}
