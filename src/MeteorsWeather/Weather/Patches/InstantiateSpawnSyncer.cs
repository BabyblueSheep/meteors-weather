using HarmonyLib;
using MeteorsWeather.Weather;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

[HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
internal static class InstantiateSpawnSyncer
{
    [HarmonyPostfix]
    static void Postfix()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;

        GameObject networkHandlerHost = GameObject.Instantiate(MeteorWeatherRegistry.SpawnSyncer);

        GameObject.DontDestroyOnLoad(networkHandlerHost);

        networkHandlerHost.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
    }
}