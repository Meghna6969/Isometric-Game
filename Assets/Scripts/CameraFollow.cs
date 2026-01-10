using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothSpeed = 10f;

    private void FixedUpdate()
    {
        FollowTarget();
    }
    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with the wall");
            Renderer renderer = other.gameObject.GetComponent<Renderer>();
            Color color = renderer.material.color;
            color.a = 0.2f;

            renderer.material.color = color;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Exited collision with the wall");
            Renderer renderer = other.gameObject.GetComponent<Renderer>();
            Color color = renderer.material.color;
            color.a = 1f;
            renderer.material.color = color;
        }
    }
}
