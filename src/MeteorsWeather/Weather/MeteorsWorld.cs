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
    private readonly List<GameObject> _outsideAiNodes = [];

    public static float SkyAngularTilt => 0.5f;

    public static float AverageMeteorScale => 8f;

    public static float HourAmountToLand => 1;
    public static float NormalizedTimeAmountToLand => HourAmountToLand * TimeOfDay.Instance.lengthOfHours / TimeOfDay.Instance.totalTime;

    // Tracks normalized day time, but only gets incremented after spawning a meteor.
    public float MeteorSpawnTimer { get; set; }

    public bool HasSetUpForThisDay { get; set; }

    // Modified from HUDManager.SetClock and https://discord.com/channels/1168655651455639582/1168656318345777313/1229245905379524680
    public static float ClockToTime(string timeString)
    {
        var numberOfHours = TimeOfDay.Instance.numberOfHours;

        var timeParts = timeString.Split(':');
        var hoursString = int.Parse(timeParts[0]);
        var minutesString = int.Parse(timeParts[1][..2]);
        var amPm = timeParts[1][2..];

        if (amPm == "PM" && hoursString != 12)
        {
            hoursString += 12;
        }
        if (amPm == "AM" && hoursString == 12)
        {
            hoursString += 12;
        }

        var totalTime = hoursString * 60 + minutesString;
        var timeNormalized = (totalTime - 360) / (60f * numberOfHours);

        return timeNormalized;
    }

    public void SetUpForThisDay()
    {
        var landTimeStart = ClockToTime("8:00AM");
        MeteorSpawnTimer = landTimeStart - NormalizedTimeAmountToLand;

        _random = new System.Random(StartOfRound.Instance.randomMapSeed + 12);

        foreach (var meteor in _fallingMeteors)
        {
            meteor.Dispose();
        }
        _fallingMeteors.Clear();

        FindMeteorLandSpots();
    }

    private void FindMeteorLandSpots()
    {
        _outsideAiNodes.Clear();

        var entrancePosition = RoundManager.FindMainEntrancePosition(getTeleportPosition: true, getOutsideEntrance: true);
        var shipPosition = StartOfRound.Instance.shipLandingPosition.position;

        var minimumPermittedDistance = AverageMeteorScale;

        foreach (var outsideNode in RoundManager.Instance.outsideAINodes)
        {
            var nodePosition = outsideNode.transform.position;

            if (!(UnityEngine.Vector3.Distance(nodePosition, shipPosition) < minimumPermittedDistance) && !(UnityEngine.Vector3.Distance(nodePosition, entrancePosition) < minimumPermittedDistance))
            {
                _outsideAiNodes.Add(outsideNode);
            }
        }
    }

    public void GenerateMeteorToSend(float spawnTime)
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

        var meteorScale = AverageMeteorScale;

        var landTime = spawnTime + NormalizedTimeAmountToLand;

        ReceiveMeteor
        (
            landingPosition, skyDirection,
            meteorScale,
            spawnTime, landTime
        );
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveMeteor
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
    }
}
