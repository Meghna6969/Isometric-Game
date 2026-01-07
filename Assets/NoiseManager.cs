using UnityEngine;
using UnityEngine.UI;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance;

    [Header("Noise Settings")]
    [SerializeField] private float maxNoiseLevel = 100f;
    [SerializeField] private float noiseDecayRate = 5f;

    private float currentNoiseLevel = 0f;

    [Header("Noise Threshold")]
    [SerializeField] private float alertThreshold = 30f;
    [SerializeField] private float investigateThreshold = 60f;
    [SerializeField] private float chaseThreshold = 80f;   

    public float CurrentNoiseLevel => currentNoiseLevel;
    public float NoisePercentage => currentNoiseLevel / maxNoiseLevel;

    [Header("UI and Display")]
    public Slider noiseSlider;

    void Awake()
    {
        if(Instance == null)
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
        if(currentNoiseLevel > 0)
        {
            currentNoiseLevel -= noiseDecayRate * Time.deltaTime;
            currentNoiseLevel = Mathf.Max(0, currentNoiseLevel);
        }
        noiseSlider.value = currentNoiseLevel;
    }
    public void AddNoise(float amount)
    {
        currentNoiseLevel += amount;
        currentNoiseLevel = Mathf.Clamp(currentNoiseLevel, 0, maxNoiseLevel);
    }
}
