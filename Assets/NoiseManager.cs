using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance;

    public class NoiseEvent
    {
        public Vector3 position;
        public float intensity;
        public float radius;
        public float timeCreated;
        public float duration;

        public NoiseEvent(Vector3 pos, float intense, float rad, float dur)
        {
            position = pos;
            intensity = intense;
            radius = rad;
            duration = dur;
            timeCreated = Time.time;
        }
        public bool IsExpired()
        {
            return Time.time - timeCreated >= duration;
        }
        public float GetCurrentIntensity()
        {
            float timeAlive = Time.time - timeCreated;
            float fadeAmount = 1f - (timeAlive / duration);
            return intensity * Mathf.Max(0, fadeAmount);
        }
    }

    [SerializeField] private List<NoiseEvent> activeNoises = new List<NoiseEvent>();

    [Header("Noise Settings")]
    [SerializeField] private float maxNoiseLevel = 100f;

    [Header("All the UI stuff")]
    [SerializeField] private Slider noiseSlider;
    [SerializeField] private Transform player;
    [SerializeField] private float maxDistanceForUI = 30f;

    private float currentDisplayedNoise = 0f;
    [SerializeField] private float nosieDecaySpeed = 50f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        activeNoises.RemoveAll(noise => noise.IsExpired());
        UpdateNoiseUI();
    }
    private void UpdateNoiseUI()
    {
        float targetNoise = CalculateCurrentNoiseLevel();

        if(targetNoise > currentDisplayedNoise)
        {
            currentDisplayedNoise = targetNoise;
        }
        else
        {
            currentDisplayedNoise = Mathf.MoveTowards(currentDisplayedNoise, targetNoise, nosieDecaySpeed * Time.deltaTime);
        }
        noiseSlider.value = currentDisplayedNoise / maxNoiseLevel;
    }
    private float CalculateCurrentNoiseLevel()
    {
        float loudestNoise = 0f;
        foreach(var noise in activeNoises)
        {
            float distance = Vector3.Distance(player.position, noise.position);
            if(distance <= maxDistanceForUI)
            {
                float distanceFalloff = 1f - (distance / maxDistanceForUI);
                float perceivedIntensity = noise.GetCurrentIntensity() * distanceFalloff;

                if(perceivedIntensity > loudestNoise)
                {
                    loudestNoise = perceivedIntensity;
                }
            }
        }
        return loudestNoise;
    }
    public void MakeNoise(Vector3 position, float intensity, float radius, float duration = 2f)
    {
        NoiseEvent newNoise = new NoiseEvent(position, intensity, radius, duration);
        activeNoises.Add(newNoise);
        // NotifyEnemies(newNoise);
    }
    public List<NoiseEvent> GetActiveNoises()
    {
        return activeNoises;
    }
    public float GetCurrentNoiseLevel()
    {
        return currentDisplayedNoise;
    }
}
