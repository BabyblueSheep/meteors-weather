using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace MeteorsWeather.Weather;

// WHYYYYYYYYYYYYYYYYYYYYY DO I NEED TO MAKE THIS A SEPARATE CLASS
internal class MeteorSpawnSyncer : NetworkBehaviour
{
    public static MeteorSpawnSyncer Instance { get; private set; } = null!;

    public void Awake()
    {
        Plugin.Logger.LogInfo("TEST555");

        if (Instance != null)
        {
            UnityEngine.Object.Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ReceiveMeteorRpc
    (
        UnityEngine.Vector3 landingPosition,
        UnityEngine.Vector3 skyDirection,
        float landingSpeed,
        float scale,
        float normalizedSpawnTime,
        float normalizedLandTime
    )
    {
        var instance = new MeteorInstance
        (
            landingPosition, skyDirection,
            landingSpeed,
            scale,
            normalizedSpawnTime, normalizedLandTime
        );

        MeteorWeatherRegistry.MeteorShowerWeather.Effect.WorldObject.GetComponent<MeteorWeatherMeteorsWorld>().AddMeteor(instance);
    }
}
