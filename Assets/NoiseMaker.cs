using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    [Header("Noise")]
    public float noiseIntensity;
    private float noiseRadius = 15f;
    private float noiseDuration = 3f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    private bool hasBeenThrown = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MakeNoise();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") && hasBeenThrown)
        {
            MakeNoise();
        }
    }
    void MakeNoise()
    {
        if(NoiseManager.Instance != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.5f);
            audioSource.clip = clip;
            audioSource.Play();
            NoiseManager.Instance.MakeNoise(transform.position, noiseIntensity, noiseRadius, noiseDuration);
        }
    }
    public void OnThrown()
    {
        hasBeenThrown = true;
    }
    public void OnPickedUp()
    {
        hasBeenThrown = false;
    }
}
