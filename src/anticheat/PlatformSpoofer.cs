using HarmonyLib;
using InnerNet;
using System.Collections.Generic;

namespace SkidMenu.anticheat
{
	internal class PlatformSpoofer
	{
		// Starlight (All Of Us) is a mod loader that reports itself as platform 112.
		// Because it is a mod loader and not a real client platform, any player carrying
		// it is by definition a mod user. Hook it into both the anticheat flagger and the
		// mod-detection tagger so ESP's "mod user" readout shows "Starlight".
		public static Platforms StarlightPlatform => (Platforms)112;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class PlatformSpoof
		{
			static void Postfix(PlayerControl __instance)
			{
				if(!Anticheat.Enabled && !ModDetection.Enabled) return;
				if(__instance.AmOwner) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;

				if(platformData.Platform == StarlightPlatform)
				{
					DetectStarlight(__instance, clientData);
					return;
				}

				if(Anticheat.Enabled && Anticheat.CheckSpoofedPlatforms && !IsValidPlatform(platformData))
				{
					Anticheat.Flag(__instance, $"{clientData.PlayerName} was detected with spoofed platform information. Platform: {platformData.Platform}, Platform name: {platformData.PlatformName}, XUID: {platformData.XboxPlatformId}, PSID: {platformData.PsnPlatformId}.");
				}
			}
		}

		static void DetectStarlight(PlayerControl player, ClientData clientData)
		{
			// Respect the per-mod toggle from the Anticheat tab / config, just like RPC mods.
			ModEntry entry = ModDetection.KnownMods.Find(m => m.Name == "Starlight");
			if (entry != null && !entry.Enabled) return;
			if (!Anticheat.Enabled && !ModDetection.Enabled) return;

			if(!ModDetection.DetectedMods.TryGetValue(player.PlayerId, out var mods))
				ModDetection.DetectedMods[player.PlayerId] = mods = new HashSet<string>();
			mods.Add("Starlight");

			string playerName = clientData.PlayerName ?? "Unknown";
			if(Anticheat.Enabled)
			{
				Blacklist.OnModDetected(player, "Starlight");
				Anticheat.Flag(player, $"{playerName} is using Starlight mod loader (platform 112)", shouldPunish: entry == null || entry.ShouldPunish);
			}
			SkidMenu.Log.LogMessage($"[Anticheat] {playerName} detected on Starlight mod loader.");
		}

		public static bool IsValidPlatform(PlatformSpecificData platform)
		{
			string platformName = platform.PlatformName;
			ulong xuid = platform.XboxPlatformId;
			ulong psid = platform.PsnPlatformId;

			switch(platform.Platform)
			{
				case Platforms.StandaloneEpicPC:
				case Platforms.StandaloneSteamPC:
				case Platforms.StandaloneMac:
				case Platforms.StandaloneItch:
				case Platforms.IPhone:
				case Platforms.Android:
					if(IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
					break;

				case Platforms.StandaloneWin10:
					if(IsGenericPlatformName(platformName) && xuid != 0 && psid == 0) return true;
					break;

				case Platforms.Xbox:
					// Xbox Gamertags must be in the range of 3 to 16 characters
					// Other rules for gamertags: https://learn.microsoft.com/en-us/gaming/gdk/docs/store/policies/xr/xr046?view=gdk-2510
					// We could potentially resolve XUIDs into gamertags and see if it matches, but the Xbox live API endpoint for XUID->gamertag is
					// authentication locked
					if(!IsGenericPlatformName(platformName) && platformName.Length >= 3 && platformName.Length <= 16 && xuid != 0 && psid == 0) return true;
					break;

				case Platforms.Playstation:
					if(!IsGenericPlatformName(platformName) && xuid == 0 && psid != 0) return true;
					break;

				case Platforms.Switch:
					if(!IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
					break;

				// On Local lobbies, all players have a platform ID of 255
				case (Platforms)255:
					if(AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return true;
					break;

				// Starlight is always a mod loader, so it is never a valid/clean platform.
				case (Platforms)112:
					return false;
			}

			// If the Platform ID is invalid, or the platform specific data for each platform is invalid, then we know that the player's device is spoofed
			return false;
		}

		public static bool IsGenericPlatformName(string platformName)
		{
			return platformName == "TESTNAME";
		}
	}
}