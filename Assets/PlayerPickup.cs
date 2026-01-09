using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPosition;
    public float pickupDistance;

    [Header("Pickup Offset")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Throw Settings")]
    public float maxThrowDistance = 10f;
    public LayerMask groundLayer;
    public float arcHeight = 2f;
    public float ignoreCollisionTime = 0.5f;

    [Header("Trajectory Visulization")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public GameObject targetIndicator;

    [Header("Trajectory Visulaization")]
    
}
