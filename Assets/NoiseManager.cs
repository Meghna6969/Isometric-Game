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
    }

    [SerializeField] private List<NoiseEvent> activeNoises = new List<NoiseEvent>();

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
}
