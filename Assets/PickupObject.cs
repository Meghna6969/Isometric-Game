using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = ""; // Gotta assign this in the inspector
    public Sprite itemIcon;
    [Header("Pickup Prompt")]
    public string pickupPrompt = "Press E to pick up";
    public Collider triggerCollider;
    public Collider physicsCollider;

    private bool isInRange = false;
    private PlayerPickup playerPickup;
    private InputAction pickupAction;
    private Rigidbody rb;

    private bool isBeingHeld = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void OnEnable()
    {
        pickupAction = new InputAction(type:InputActionType.Button);
        pickupAction.AddBinding("<Keyboard>/e");
        pickupAction.Enable();
    }
    private void Update()
    {
        if(isInRange && playerPickup != null && pickupAction.WasPressedThisFrame())
        {
            playerPickup.PickupObject(gameObject, physicsCollider, triggerCollider);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            playerPickup = other.GetComponent<PlayerPickup>();
            if(playerPickup != null)
            {
                playerPickup.ShowPickupPrompt(pickupPrompt);
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if(playerPickup != null)
            {
                playerPickup.HidePickupPrompt();
                playerPickup = null;
            }
        }
    }
    public void OnPickedUp()
    {
       isBeingHeld = true;
       triggerCollider.enabled = false;

       NoiseMaker noiseMaker = GetComponent<NoiseMaker>();
       if(noiseMaker != null)
        {
            noiseMaker.OnPickedUp();
        }
    }
    public void OnThrown()
    {
        isBeingHeld = false;
        triggerCollider.enabled = false;

        NoiseMaker noiseMaker = GetComponent<NoiseMaker>();
        if(noiseMaker != null)
        {
            noiseMaker.OnThrown();
        }
    }
    public void OnDropped()
    {
        isBeingHeld = false;
        triggerCollider.enabled = true;
    }
}
