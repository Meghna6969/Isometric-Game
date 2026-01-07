using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    [Header("Noise")]
    public float noiseIntensity;
    //private float noiseRadius = 15f;
    //private float noiseDuration = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MakeNoise();
        }
    }
    void MakeNoise()
    {
        if(NoiseManager.Instance != null)
        {
            NoiseManager.Instance.AddNoise(noiseIntensity);
        }
    }
}
