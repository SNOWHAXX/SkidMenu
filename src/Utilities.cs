using System.Collections.Generic;

namespace SkidMenu
{
    internal class Utilities
    {
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SkinData> allSkins = HatManager.Instance.allSkins;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData> allHats = HatManager.Instance.allHats;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData> allVisors = HatManager.Instance.allVisors;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PetData> allPets = HatManager.Instance.allPets;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NamePlateData> allNameplates = HatManager.Instance.allNamePlates;

        public static bool IsColorTaken(int colorId)
        {
            foreach (var p in PlayerControl.AllPlayerControls)
                if (p != null && p != PlayerControl.LocalPlayer && p.Data != null && p.Data.DefaultOutfit.ColorId == colorId)
                    return true;
            return false;
        }

        public static int GetFreeColor()
        {
            var rnd   = new System.Random();
            var taken = new System.Collections.Generic.HashSet<int>();
            foreach (var p in PlayerControl.AllPlayerControls)
                if (p != null && p != PlayerControl.LocalPlayer && p.Data != null)
                    taken.Add(p.Data.DefaultOutfit.ColorId);
            var free = new System.Collections.Generic.List<int>();
            for (int c = 0; c < 18; c++)
                if (!taken.Contains(c)) free.Add(c);
            return free.Count > 0 ? free[rnd.Next(0, free.Count)] : rnd.Next(0, 18);
        }

        public static void RandomizePlayer(bool ingame = false)
        {
            System.Random rnd = new System.Random();

            if (ingame)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    OutfitBypass.SetColor(rnd.Next(0, 18));
                }
                else
                {
                    var takenColors = new System.Collections.Generic.HashSet<int>();
                    foreach (var p in PlayerControl.AllPlayerControls)
                        if (p != null && p != PlayerControl.LocalPlayer && p.Data != null)
                            takenColors.Add(p.Data.DefaultOutfit.ColorId);

                    var available = new System.Collections.Generic.List<int>();
                    for (int c = 0; c < 18; c++)
                        if (!takenColors.Contains(c))
                            available.Add(c);

                    if (available.Count > 0)
                        OutfitBypass.SetColor(available[rnd.Next(0, available.Count)]);
                }

                // string randomName = AccountManager.Instance.GetRandomName();
                // PlayerControl.LocalPlayer.CmdCheckName(randomName);

                PlayerControl.LocalPlayer.RpcSetHat(allHats[rnd.Next(0, allHats.Length)].ProductId);
                PlayerControl.LocalPlayer.RpcSetVisor(allVisors[rnd.Next(0, allVisors.Length)].ProductId);
                PlayerControl.LocalPlayer.RpcSetSkin(allSkins[rnd.Next(0, allSkins.Length)].ProductId);
                PlayerControl.LocalPlayer.RpcSetPet(allPets[rnd.Next(0, allPets.Length)].ProductId);
            }
            else
            {
                PlayerCustomization.EquipSkin(allSkins[rnd.Next(0, allSkins.Length)]);
                PlayerCustomization.EquipHat(allHats[rnd.Next(0, allHats.Length)]);
                PlayerCustomization.EquipVisor(allVisors[rnd.Next(0, allVisors.Length)]);
                PlayerCustomization.EquipPet(allPets[rnd.Next(0, allPets.Length)]);
                PlayerCustomization.EquipNameplate(allNameplates[rnd.Next(0, allNameplates.Length)]);

                AccountManager.Instance.RandomizeName();
            }
        }

        public static PlayerControl GetRandomPlayer(bool excludeHost = false, bool excludeDead = false, bool excludeImposters = false, bool excludeSelf = true)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> allPlayers = PlayerControl.AllPlayerControls;
            List<PlayerControl> validPlayers = new List<PlayerControl>();

            foreach (PlayerControl player in allPlayers)
            {
                if (
                    (excludeSelf && AmongUsClient.Instance.ClientId == player.OwnerId) ||
                    (excludeHost && AmongUsClient.Instance.HostId == player.OwnerId) ||
                    (excludeDead && player.Data.IsDead) ||
                    (excludeImposters && player.Data.Role.CanUseKillButton)
                ) continue;

                validPlayers.Add(player);
            }

            System.Random rnd = new System.Random();
            return validPlayers[rnd.Next(validPlayers.Count)];
        }

        public static void CopyPlayer(PlayerControl player)
        {
            NetworkedPlayerInfo.PlayerOutfit outfit = player.CurrentOutfit;

            OutfitBypass.SetColor(AmongUsClient.Instance.AmHost || !IsColorTaken(outfit.ColorId) ? outfit.ColorId : GetFreeColor());
            OutfitBypass.SetName(outfit.PlayerName);

            PlayerControl.LocalPlayer.RpcSetNamePlate(outfit.NamePlateId);
            PlayerControl.LocalPlayer.RpcSetHat(outfit.HatId);
            PlayerControl.LocalPlayer.RpcSetVisor(outfit.VisorId);
            PlayerControl.LocalPlayer.RpcSetSkin(outfit.SkinId);
            PlayerControl.LocalPlayer.RpcSetPet(outfit.PetId);
        }

        public static void OpenMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            MeetingRoomManager.Instance.AssignSelf(reporter, target);
            reporter.RpcStartMeeting(target);
            HudManager.Instance.OpenMeetingRoom(reporter);
        }

        public static MapNames GetCurrentMap()
        {
            if (AmongUsClient.Instance == null) return MapNames.Skeld;

            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                return (MapNames)AmongUsClient.Instance.TutorialMapId;
            }
            else
            {
                if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.CurrentGameOptions == null) return MapNames.Skeld;
                return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
            }
        }
    }
}