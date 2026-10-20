using MeteorsWeather.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MeteorsWeather.Weather;

public class MeteorWeatherMeteorsWorld : MonoBehaviour
{
    private System.Random _random = null!;

    private readonly List<MeteorInstance> _fallingMeteors = [];
    private readonly List<GameObject> _explosions = [];

    private readonly List<GameObject> _outsideAiNodes = [];

    public static float SkyAngularTilt => 0.5f;

    private List<MeteorScales> _meteorScaleConfigurations = [];

    public static float HourAmountToLand
    {
        get
        {
            var value = Plugin.MeteorWeathersConfig.LandDuration.Value;
            if (value <= 0f)
            {
                var defaultValue = (float)Plugin.MeteorWeathersConfig.LandDuration.DefaultValue;
                LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.LandDuration.Definition.Key)} has an invalid value of {value}. Using default value.");
                return defaultValue;
            }
            return value;
        }
    }
    public static float NormalizedTimeAmountToLand => HourAmountToLand * TimeOfDay.Instance.lengthOfHours / TimeOfDay.Instance.totalTime;

    // Tracks normalized day time, but only gets incremented after spawning a meteor.
    private float _meteorSpawnTimer;
    private float _meteorSpawnStartTimer;
    private float _meteorSpawnEndTimer;

    private bool _hasLoggedStartOfShower;
    private bool _hasLoggedEndOfShower;

    public bool HasSetUpForThisDay { get; set; }



    public void SetUpForThisDay()
    {
        var startClockTime = Plugin.MeteorWeathersConfig.LandStartTime.Value;
        var endClockTime = Plugin.MeteorWeathersConfig.LandEndTime.Value;

        if (!TimeUtils.IsValidClock(startClockTime))
        {
            LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.LandStartTime.Definition.Key)} has an invalid value of {startClockTime}. Using default value.");
            startClockTime = (string)Plugin.MeteorWeathersConfig.LandStartTime.DefaultValue;
        }
        if (!TimeUtils.IsValidClock(endClockTime))
        {
            LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.LandEndTime.Definition.Key)} has an invalid value of {endClockTime}. Using default value.");
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

        _hasLoggedStartOfShower = false;
        _hasLoggedEndOfShower = false;

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
        var averageLargeMeteorScale = 0f;
        var largestMeteorScale = 0f;
        var totalWeight = 0f;
        foreach (var scaleConfiguration in _meteorScaleConfigurations)
        {
            var largeScaleForThisConfig = scaleConfiguration.Average + scaleConfiguration.Deviation;
            averageLargeMeteorScale += largeScaleForThisConfig * scaleConfiguration.Weight;
            if (largestMeteorScale <= largeScaleForThisConfig)
            {
                largestMeteorScale = largeScaleForThisConfig;
            }

            totalWeight += scaleConfiguration.Weight;
        }
        averageLargeMeteorScale /= totalWeight;

        _outsideAiNodes.Clear();

        var entrancePosition = RoundManager.FindMainEntrancePosition(getTeleportPosition: true, getOutsideEntrance: true);
        var shipPosition = StartOfRound.Instance.shipLandingPosition.position;

        foreach (var outsideNode in RoundManager.Instance.outsideAINodes)
        {
            var nodePosition = outsideNode.transform.position;

            if (!(UnityEngine.Vector3.Distance(nodePosition, shipPosition) < largestMeteorScale * 2) && !(UnityEngine.Vector3.Distance(nodePosition, entrancePosition) < (averageLargeMeteorScale * 2)))
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
            LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.MeteorScales.Definition.Key)} has an invalid value of {meteorScalesConfig}. Using default value.");
            meteorScalesConfig = (string)Plugin.MeteorWeathersConfig.MeteorScales.DefaultValue;
        }

        _meteorScaleConfigurations = MeteorScalesUtils.StringToList(meteorScalesConfig);
    }



    public void TrySpawningMeteors(float totalTime, float currentNormalizedTime)
    {
        var landTime = _meteorSpawnTimer + NormalizedTimeAmountToLand;
        if (landTime > _meteorSpawnEndTimer)
        {
            var logLevel = Plugin.MeteorWeathersConfig.LoggingLevel.Value;
            if (logLevel >= MeteorWeathersConfig.LoggingLevels.Info)
            {
                if (!_hasLoggedEndOfShower)
                {
                    LogUtils.LogInfo($"Reached the ending time for landing. Stopping meteor showers for today.");
                    _hasLoggedEndOfShower = true;
                }
            }
            return;
        }

        while (_meteorSpawnTimer < currentNormalizedTime)
        {
            if (!_hasLoggedStartOfShower)
            {
                LogUtils.LogInfo($"Reached the starting time for landing. Starting meteor showers for today.");
                _hasLoggedStartOfShower = true;
            }

            LogUtils.LogDebug($"Generating meteor to land at {HUDManager.Instance.GetClockTimeFormatted(_meteorSpawnTimer, TimeOfDay.Instance.numberOfHours, createNewLine: false)} ({_meteorSpawnTimer}).");

            GenerateMeteorToSendRpc(_meteorSpawnTimer);


            var landTimeAverage = Plugin.MeteorWeathersConfig.MeteorSpawnIntervalAverage.Value;
            if (landTimeAverage <= 0f)
            {
                LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.MeteorSpawnIntervalAverage.Definition.Key)} has an invalid value of {landTimeAverage}. Using default value.");
                landTimeAverage = (float)Plugin.MeteorWeathersConfig.MeteorSpawnIntervalAverage.DefaultValue;
            }

            var landTimeDeviation = Plugin.MeteorWeathersConfig.MeteorSpawnIntervalDeviation.Value;
            if (landTimeDeviation < 0f)
            {
                LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.MeteorSpawnIntervalDeviation.Definition.Key)} has an invalid value of {landTimeDeviation}. Using default value.");
                landTimeDeviation = (float)Plugin.MeteorWeathersConfig.MeteorSpawnIntervalDeviation.DefaultValue;
            }

            var landInterval = landTimeAverage;
            landInterval += _random.NextFloat(-landTimeDeviation, landTimeDeviation);
            if (landInterval <= 0.1f)
            {
                LogUtils.LogWarn($"The interval for meteor landing ended up being too small.");
                landInterval = 0.1f;
            }
            var landIntervalNormalized = landInterval / totalTime;

            _meteorSpawnTimer += landIntervalNormalized;

            if (landTime > _meteorSpawnEndTimer)
                break;
        }
    }

    //[Rpc(SendTo.Server)]
    private void GenerateMeteorToSendRpc(float spawnTime)
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
            currentTotalWeight += currentScaleConfiguration.Weight;
            if (randomWeight < currentTotalWeight)
            {
                scaleConfigurationToUse = currentScaleConfiguration;
                break;
            }
        }

        var meteorScale = scaleConfigurationToUse.Average + _random.NextFloat(-scaleConfigurationToUse.Deviation, scaleConfigurationToUse.Deviation);

        var landingSpeed = Plugin.MeteorWeathersConfig.LandSpeed.Value;
        if (landingSpeed <= 0f)
        {
            LogUtils.LogWarn($"{nameof(Plugin.MeteorWeathersConfig.LandSpeed.Definition.Key)} has an invalid value of {landingSpeed}. Using default value.");
            landingSpeed = (float)Plugin.MeteorWeathersConfig.LandSpeed.DefaultValue;
        }

        var landTime = spawnTime + NormalizedTimeAmountToLand;

        MeteorSpawnSyncer.Instance.ReceiveMeteorRpc
        (
            landingPosition, skyDirection,
            landingSpeed,
            meteorScale,
            spawnTime, landTime
        );
    }

    public void AddMeteor(MeteorInstance meteor)
    {
        _fallingMeteors.Add(meteor);
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



    public void OnDestroy()
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
