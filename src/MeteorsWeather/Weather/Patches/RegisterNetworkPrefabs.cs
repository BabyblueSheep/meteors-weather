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
    private static bool _hasSpawned = false;

    [HarmonyPostfix]
    static void Postfix()
    {
        if (_hasSpawned)
            return;

        _hasSpawned = true;

        NetworkManager.Singleton.AddNetworkPrefab(MeteorWeatherRegistry.SpawnSyncer);
    }
}