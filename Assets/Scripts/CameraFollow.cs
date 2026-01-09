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
            other.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Exited collision with the wall");
            other.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
