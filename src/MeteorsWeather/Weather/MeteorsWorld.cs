using System;
using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using MeteorsWeather.Utils;

namespace MeteorsWeather.Weather;

public class MeteorWeatherMeteorsWorld : NetworkBehaviour
{
    private System.Random _random = null!;

    private readonly List<MeteorInstance> _fallingMeteors = [];
    private readonly List<GameObject> _explosions = [];

    private readonly List<GameObject> _outsideAiNodes = [];

    public static float SkyAngularTilt => 0.5f;

    public static float AverageMeteorScale => 8f;

    private List<MeteorScales> _meteorScaleConfigurations = [];

    public static float HourAmountToLand
    {
        get
        {
            var value = Plugin.MeteorWeathersConfig.LandDuration.Value;
            if (value <= 0f)
                return (float)Plugin.MeteorWeathersConfig.LandDuration.DefaultValue;
            return value;
        }
    }
    public static float NormalizedTimeAmountToLand => HourAmountToLand * TimeOfDay.Instance.lengthOfHours / TimeOfDay.Instance.totalTime;

    // Tracks normalized day time, but only gets incremented after spawning a meteor.
    private float _meteorSpawnTimer;
    private float _meteorSpawnStartTimer;
    private float _meteorSpawnEndTimer;

    public bool HasSetUpForThisDay { get; set; }



    public void SetUpForThisDay()
    {
        var startClockTime = Plugin.MeteorWeathersConfig.LandStartTime.Value;
        var endClockTime = Plugin.MeteorWeathersConfig.LandEndTime.Value;

        if (!TimeUtils.IsValidClock(startClockTime))
        {
            startClockTime = (string)Plugin.MeteorWeathersConfig.LandStartTime.DefaultValue;
        }
        if (!TimeUtils.IsValidClock(endClockTime))
        {
            endClockTime = (string)Plugin.MeteorWeathersConfig.LandEndTime.DefaultValue;
        }

        _meteorSpawnStartTimer = TimeUtils.ClockToTime(startClockTime);
        _meteorSpawnEndTimer = TimeUtils.ClockToTime(endClockTime);

        _meteorSpawnStartTimer = Mathf.Clamp(_meteorSpawnStartTimer, 0f, 1f);
        _meteorSpawnEndTimer = Mathf.Clamp(_meteorSpawnEndTimer, 0f, 1f);

        if (_meteorSpawnEndTimer < _meteorSpawnStartTimer)
        {
            _meteorSpawnEndTimer = _meteorSpawnStartTimer;
        }

        _meteorSpawnTimer = _meteorSpawnStartTimer - NormalizedTimeAmountToLand;

        _random = new System.Random(StartOfRound.Instance.randomMapSeed + 12);

        foreach (var meteor in _fallingMeteors)
        {
            meteor.Dispose();
        }
        _fallingMeteors.Clear();

        foreach (var explosion in _explosions)
        {
            GameObject.Destroy(explosion);
        }
        _explosions.Clear();

        GenerateMeteorScales();
        FindMeteorLandSpots();
    }

    private void FindMeteorLandSpots()
    {
        var biggestMeteorScale = 0f;
        foreach (var scaleConfiguration in _meteorScaleConfigurations)
        {
            var biggestScaleForThisConfig = scaleConfiguration.Average + scaleConfiguration.Deviation;
            if (biggestMeteorScale < biggestScaleForThisConfig)
            {
                biggestMeteorScale = biggestScaleForThisConfig;
            }
        }

        _outsideAiNodes.Clear();

        var entrancePosition = RoundManager.FindMainEntrancePosition(getTeleportPosition: true, getOutsideEntrance: true);
        var shipPosition = StartOfRound.Instance.shipLandingPosition.position;

        var minimumPermittedDistance = biggestMeteorScale * 0.85f;

        foreach (var outsideNode in RoundManager.Instance.outsideAINodes)
        {
            var nodePosition = outsideNode.transform.position;

            if (!(UnityEngine.Vector3.Distance(nodePosition, shipPosition) < minimumPermittedDistance) && !(UnityEngine.Vector3.Distance(nodePosition, entrancePosition) < minimumPermittedDistance))
            {
                _outsideAiNodes.Add(outsideNode);
            }
        }
    }

    private void GenerateMeteorScales()
    {
        _meteorScaleConfigurations.Clear();

        var meteorScalesConfig = Plugin.MeteorWeathersConfig.MeteorScales.Value;
        if (!MeteorScalesUtils.IsValidList(meteorScalesConfig))
        {
            meteorScalesConfig = (string)Plugin.MeteorWeathersConfig.MeteorScales.DefaultValue;
        }

        _meteorScaleConfigurations = MeteorScalesUtils.StringToList(meteorScalesConfig);
    }



    public void TrySpawningMeteors(float totalTime, float currentNormalizedTime)
    {
        var landTime = _meteorSpawnTimer + NormalizedTimeAmountToLand;
        if (landTime > _meteorSpawnEndTimer)
            return;

        while (_meteorSpawnTimer < currentNormalizedTime)
        {
            GenerateMeteorToSend(_meteorSpawnTimer);


            var landTimeAverage = Plugin.MeteorWeathersConfig.MeteorSpawnIntervalAverage.Value;
            if (landTimeAverage <= 0f)
            {
                landTimeAverage = (float)Plugin.MeteorWeathersConfig.MeteorSpawnIntervalAverage.DefaultValue;
            }

            var landTimeDeviation = Plugin.MeteorWeathersConfig.MeteorSpawnIntervalDeviation.Value;
            if (landTimeDeviation < 0f)
            {
                landTimeDeviation = (float)Plugin.MeteorWeathersConfig.MeteorSpawnIntervalDeviation.DefaultValue;
            }

            var landInterval = landTimeAverage;
            landInterval += _random.NextFloat(-landTimeDeviation, landTimeDeviation);
            if (landInterval <= 0.1f)
            {
                landInterval = 0.1f;
            }
            var landIntervalNormalized = landInterval / totalTime;

            _meteorSpawnTimer += landIntervalNormalized;
        }
    }

    private void GenerateMeteorToSend(float spawnTime)
    {
        var landingPosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable
        (
            pos: _outsideAiNodes[_random.Next(_outsideAiNodes.Count)].transform.position,
            radius: 15f,
            navHit: default,
            randomSeed: _random, 
            layerMask: -273
        );
        if (Physics.Raycast
            (
                origin: landingPosition + UnityEngine.Vector3.up * 100f, 
                direction: UnityEngine.Vector3.down, 
                out var hitInfo, 
                maxDistance: 150f, 
                StartOfRound.Instance.collidersAndRoomMaskAndDefault, 
                QueryTriggerInteraction.Ignore
            )
        )
        {
            landingPosition = hitInfo.point;
        }

        var skyDirection = UnityEngine.Vector3.up * 10f +
            new UnityEngine.Vector3
            (
                _random.NextFloat(-10f, 10f) * SkyAngularTilt,
                0f,
                _random.NextFloat(-10f, 10f) * SkyAngularTilt
            );

        var totalWeight = 0f;
        for (int i = 0; i < _meteorScaleConfigurations.Count; i++)
        {
            totalWeight += _meteorScaleConfigurations[i].Weight;
        }

        var randomWeight = _random.NextFloat(totalWeight);

        var scaleConfigurationToUse = _meteorScaleConfigurations[0];
        var currentTotalWeight = 0f;
        for (int i = 0; i < _meteorScaleConfigurations.Count; i++)
        {
            var currentScaleConfiguration = _meteorScaleConfigurations[i];
            if (currentScaleConfiguration.Weight < randomWeight)
            {
                scaleConfigurationToUse = currentScaleConfiguration;
                break;
            }
            currentTotalWeight += currentScaleConfiguration.Weight;
        }

        var meteorScale = scaleConfigurationToUse.Average + _random.NextFloat(-scaleConfigurationToUse.Deviation, scaleConfigurationToUse.Deviation);

        var landTime = spawnTime + NormalizedTimeAmountToLand;

        ReceiveMeteor
        (
            landingPosition, skyDirection,
            meteorScale,
            spawnTime, landTime
        );
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReceiveMeteor
    (
        UnityEngine.Vector3 landingPosition,
        UnityEngine.Vector3 skyDirection,
        float scale,
        float normalizedSpawnTime,
        float normalizedLandTime
    )
    {
        var instance = new MeteorInstance
        (
            landingPosition, skyDirection,
            scale,
            normalizedSpawnTime, normalizedLandTime
        );

        _fallingMeteors.Add(instance);
    }



    public void Update()
    {
        UpdateMeteors();
    }

    public void UpdateMeteors()
    {
        for (int i = _fallingMeteors.Count - 1; i >= 0; i--)
        {
            var meteor = _fallingMeteors[i];

            meteor.Update();

            if (meteor.ShouldBeRemoved)
            {
                _explosions.Add(meteor.LandingExplosionPrefab);
                meteor.HasGivenAwayLandingExplosion = true;

                _fallingMeteors.RemoveAt(i);
                meteor.Dispose();
            }
        }
    }



    public override void OnDestroy()
    {
        foreach (var meteor in _fallingMeteors)
        {
            meteor.Dispose();
        }
        _fallingMeteors.Clear();

        foreach (var explosion in _explosions)
        {
            GameObject.Destroy(explosion);
        }
        _explosions.Clear();
    }
}
