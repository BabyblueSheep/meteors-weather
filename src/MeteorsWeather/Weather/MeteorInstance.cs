using MeteorsWeather.Utils;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace MeteorsWeather.Weather;

internal class MeteorInstance : IDisposable
{
    // These match what's outputted, not what's found when decompiling
    public static float StartingHeightInSky => 2700f;

    public static GameObject VanillaMeteorPrefab { get; set; } = null!;
    public static GameObject VanillaDistantSpritePrefab { get; set; } = null!;
    public static GameObject VanillaMeteorLandingExplosionPrefab { get; set; } = null!;
    public static AudioClip VanillaInsideFactoryMeteorClip { get; set; } = null!;



    private bool _isNear = false; // Stored so that the far behavior can run at least once
    private float _landingTime = 0f;
    private bool _wasFarPreviously = false;
    private bool _wasNearPreviously = false;
    private bool _wasVeryNearPreviously = false; // Terrific naming (meteors have 4 phases; 0 is idle/ghost, 1 is far, last 2 seem to be "near" and "very near")

    private bool _hasLanded = false;

    public UnityEngine.Vector3 LandingPosition { get; set; }
    public UnityEngine.Vector3 SkyDirection { get; set; }
    public float LandingSpeed { get; set; }

    public Ray SkyRay => new(LandingPosition, SkyDirection);

    public float Scale { get; set; }

    private UnityEngine.Vector3 _positionAtStartOfLandingPhase;
    public float NormalizedTimeToLandAt { get; set; }
    public float NormalizedTimeOfSpawning { get; set; }

    private readonly ParticleSystem _breakAtmosphereFireA;
    private readonly ParticleSystem _breakAtmosphereFireB;

    public GameObject MeteorPrefab { get; private set; }
    public GameObject DistantSpritePrefab { get; private set; }
    public GameObject LandingExplosionPrefab { get; private set; }
    public bool HasGivenAwayLandingExplosion { get; set; } = false;

    public bool ShouldBeRemoved { get; set; } = false;

    public bool IsDisposed { get; private set; }

    public MeteorInstance
    (
        UnityEngine.Vector3 landingPosition,
        UnityEngine.Vector3 skyDirection,
        float landingSpeed,
        float scale,
        float normalizedSpawnTime,
        float normalizedLandTime
    )
    {
        LandingPosition = landingPosition;
        SkyDirection = skyDirection;

        Scale = scale;

        NormalizedTimeOfSpawning = normalizedSpawnTime;
        NormalizedTimeToLandAt = normalizedLandTime;

        LandingSpeed = landingSpeed;

        #region Initialize prefabs

        var skyRay = SkyRay;

        MeteorPrefab = UnityEngine.Object.Instantiate
        (
            VanillaMeteorPrefab,
            position: skyRay.GetPoint(800f),
            rotation: UnityEngine.Quaternion.LookRotation(LandingPosition - skyRay.GetPoint(800f)),
            parent: RoundManager.Instance.mapPropsContainer.transform
        );

        var particleSystems = MeteorPrefab.GetComponentsInChildren<ParticleSystem>();
        _breakAtmosphereFireA = particleSystems[0];
        _breakAtmosphereFireB = particleSystems[1];

        DistantSpritePrefab = UnityEngine.Object.Instantiate
        (
            VanillaDistantSpritePrefab,
            position: MeteorPrefab.transform.position,
            rotation: UnityEngine.Quaternion.identity,
            parent: RoundManager.Instance.mapPropsContainer.transform
        );

        LandingExplosionPrefab = null!;

        #endregion
    }



    public void Update()
    {
        var travelProgress = Mathf.InverseLerpUnclamped(NormalizedTimeOfSpawning, NormalizedTimeToLandAt, TimeOfDay.Instance.normalizedTimeOfDay);

        if (_isNear)
        {
            UpdateNear();
        }
        else
        {
            UpdateFar(travelProgress);
        }

        _isNear = travelProgress >= 1f;
    }

    private void UpdateFar(float travelProgress)
    {
        var deltaTimeSeconds = Time.deltaTime;

        var skyRay = SkyRay;

        if (!_wasFarPreviously)
        {
            AudioSource[] audioSources = MeteorPrefab.GetComponentsInChildren<AudioSource>();
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (!audioSources[i].CompareTag("Aluminum")) // Aluminium is used to tag the whistle sound (???)
                {
                    audioSources[i].Play();
                }
            }

            _wasFarPreviously = true;
        }

        var destinationPosition = UnityEngine.Vector3.Lerp(skyRay.GetPoint(StartingHeightInSky), skyRay.GetPoint(150f), travelProgress);

        MeteorPrefab.transform.position = UnityEngine.Vector3.Lerp(MeteorPrefab.transform.position, destinationPosition, 5f * deltaTimeSeconds);

        if (DistantSpritePrefab.activeSelf)//(meteor.distantSprite != null)
        {
            var audioListenerPosition = StartOfRound.Instance.audioListener.transform.position; // TODO: local player position?
            var rayTowardsMeteor = new Ray(audioListenerPosition, MeteorPrefab.transform.position - audioListenerPosition);

            DistantSpritePrefab.transform.LookAt(audioListenerPosition, UnityEngine.Vector3.up);
            DistantSpritePrefab.transform.localScale = UnityEngine.Vector3.one * Mathf.Lerp(0.3f, Scale, travelProgress * 2f);
            DistantSpritePrefab.transform.position = rayTowardsMeteor.GetPoint(380f);

            MeteorPrefab.transform.localScale = 0.25f * Scale * UnityEngine.Vector3.one;

            if (UnityEngine.Vector3.Distance(audioListenerPosition, MeteorPrefab.transform.position) < UnityEngine.Vector3.Distance(audioListenerPosition, DistantSpritePrefab.transform.position) && travelProgress > 0.75f)
            {
                DistantSpritePrefab.SetActive(false);

                PlayNonWhistleSounds();
            }
        }
        else
        {
            MeteorPrefab.transform.localScale = UnityEngine.Vector3.Lerp
            (
                MeteorPrefab.transform.localScale,
                UnityEngine.Vector3.one * Mathf.Lerp(Scale * 0.25f, Scale, travelProgress),
                5f * Time.deltaTime
            );
        }

        _breakAtmosphereFireA.transform.localScale = MeteorPrefab.transform.localScale with
        {
            z = MeteorPrefab.transform.localScale.z * 0.72f
        };
        _breakAtmosphereFireB.transform.localScale = _breakAtmosphereFireA.transform.localScale;
    }

    private void UpdateNear()
    {
        var isVeryNear = _landingTime > 0.4f;

        if (!_wasNearPreviously || (isVeryNear && !_wasVeryNearPreviously))
        {
            DistantSpritePrefab.SetActive(false);

            var audioSources = MeteorPrefab.GetComponentsInChildren<AudioSource>();
            if (!audioSources[0].isPlaying)
            {
                PlayNonWhistleSounds();
            }

            if (!isVeryNear)
            {
                _positionAtStartOfLandingPhase = MeteorPrefab.transform.position;
            }

            _wasNearPreviously = true;
            if (isVeryNear)
            {
                _wasVeryNearPreviously = true;
            }
        }

        _landingTime = Mathf.Min(_landingTime + Time.deltaTime * LandingSpeed, 1.3f);

        MeteorPrefab.transform.localScale = UnityEngine.Vector3.one * Scale;

        _breakAtmosphereFireA.transform.localScale = MeteorPrefab.transform.localScale with
        {
            z = MeteorPrefab.transform.localScale.z * 0.72f
        };
        _breakAtmosphereFireB.transform.localScale = _breakAtmosphereFireA.transform.localScale;

        MeteorPrefab.transform.position = UnityEngine.Vector3.LerpUnclamped(_positionAtStartOfLandingPhase, LandingPosition, _landingTime);
        MeteorPrefab.transform.position = MeteorPrefab.transform.position with
        {
            y = Mathf.Max(MeteorPrefab.transform.position.y, -80f)
        };

        if (_landingTime >= 0.4f && !isVeryNear)
        {
            if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                var audioSources = MeteorPrefab.GetComponentsInChildren<AudioSource>();
                for (int k = 0; k < audioSources.Length; k++)
                {
                    if (audioSources[k].CompareTag("Aluminum")) // Whistle sound
                    {
                        audioSources[k].pitch = UnityEngine.Random.Range(0.84f, 1.16f);
                        audioSources[k].Play();
                        break;
                    }
                }
            }
        }

        if (_landingTime >= 1f && !_hasLanded)
        {
            _hasLanded = true;

            // Just using any available MonoBehavior
            TimeOfDay.Instance.StartCoroutine(Land());
        }
    }

    private void PlayNonWhistleSounds()
    {
        var audioSources = MeteorPrefab.GetComponentsInChildren<AudioSource>();

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].Play();
        }
        if (Scale > 25f)
        {
            audioSources = MeteorPrefab.GetComponentsInChildren<AudioSource>();
            for (int j = 0; j < audioSources.Length; j++)
            {
                audioSources[j].pitch *= 0.5f;
                audioSources[j].maxDistance += 120f;
            }
        }
    }

    private readonly static WaitForSeconds _waitForSeconds0_05 = new(0.05f);
    private readonly static WaitForSeconds _waitForSeconds0_6 = new(0.6f);

    private IEnumerator Land()
    {
        yield return _waitForSeconds0_05;

        LandingExplosionPrefab = UnityEngine.Object.Instantiate
        (
            VanillaMeteorLandingExplosionPrefab,
            position: LandingPosition,
            rotation: UnityEngine.Quaternion.Euler(-90f, 0f, 0f),
            parent: RoundManager.Instance.mapPropsContainer.transform
        );
        LandingExplosionPrefab.transform.localScale = UnityEngine.Vector3.one * Scale;

        var explosionAudioSources = LandingExplosionPrefab.GetComponentsInChildren<AudioSource>();
        for (int i = 0; i < explosionAudioSources.Length; i++)
        {
            if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                explosionAudioSources[i].spatialBlend = 0f;
                explosionAudioSources[i].clip = VanillaInsideFactoryMeteorClip;
                explosionAudioSources[i].pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                explosionAudioSources[i].volume = UnityEngine.Random.Range(0.2f, 0.8f);
                explosionAudioSources[i].Play();
                break;
            }
            explosionAudioSources[i].pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            explosionAudioSources[i].Play();

            WalkieTalkie.TransmitOneShotAudio(explosionAudioSources[i], explosionAudioSources[i].clip);
        }

        ParticleSystem[] explosionParticles = LandingExplosionPrefab.GetComponentsInChildren<ParticleSystem>();
        for (int j = 0; j < explosionParticles.Length; j++)
        {
            explosionParticles[j].transform.localScale = LandingExplosionPrefab.transform.localScale * 0.12f;
        }

        var localPlayerPosition = GameNetworkManager.Instance.localPlayerController.transform.position;

        var isMeteorReallyBig = Scale > 110f / 3f; // 110f = MeteorShowers.baseMeteorSize;
        var playerDistanceToLandingPosition = UnityEngine.Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, LandingPosition);
        Landmine.SpawnExplosion
        (
            LandingPosition,
            spawnExplosionEffect: false,
            killRange: Scale, 
            damageRange: Mathf.Min(Scale * 1.4f, Scale + 12f), 
            nonLethalDamage: 40,
            physicsForce: 50f
        );
        RoundManager.Instance.DestroyTreeAtPosition(LandingPosition, Scale);

        if  
        (
                                  playerDistanceToLandingPosition < Mathf.Max(Scale * 1.5f, 35f) || 
            (isMeteorReallyBig && playerDistanceToLandingPosition < Mathf.Max(Scale * 2f, 90f))
        )
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
            GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += UnityEngine.Vector3.Normalize(localPlayerPosition - (LandingPosition - UnityEngine.Vector3.up * 25f)) * 110f / playerDistanceToLandingPosition;
        }
        else if 
        (
                                  playerDistanceToLandingPosition < Mathf.Max(Scale * 2f, 60f) || 
            (isMeteorReallyBig && playerDistanceToLandingPosition < Mathf.Max(Scale * 2.5f, 150f))
        )
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
            GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += UnityEngine.Vector3.Normalize(localPlayerPosition - (LandingPosition - UnityEngine.Vector3.up * 20f)) * 85f / playerDistanceToLandingPosition;
        }

        yield return _waitForSeconds0_6;

        ShouldBeRemoved = true;
    }

    private void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {

            }

            UnityEngine.Object.Destroy(MeteorPrefab);
            UnityEngine.Object.Destroy(DistantSpritePrefab);
            if (!HasGivenAwayLandingExplosion)
            {
                UnityEngine.Object.Destroy(LandingExplosionPrefab);
            }

            IsDisposed = true;
        }
    }

    ~MeteorInstance()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
