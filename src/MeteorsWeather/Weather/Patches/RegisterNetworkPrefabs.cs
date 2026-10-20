using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace MeteorsWeather.Weather.Patches;


[HarmonyPatch, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
internal static class RegisterNetworkPrefabs
{
    [HarmonyPostfix]
    static void Postfix()
    {
        NetworkManager.Singleton.AddNetworkPrefab(MeteorWeatherRegistry.SpawnSyncer);
    }
}