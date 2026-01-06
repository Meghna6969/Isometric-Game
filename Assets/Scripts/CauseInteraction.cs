using UnityEngine;

public class CauseInteraction : MonoBehaviour
{
    public InteractionType interactionType;
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered with an interaction");
        if (other.gameObject.CompareTag("Player"))
        {
            if(interactionType == InteractionType.FridgeDoor)
            {
                Debug.Log("Fridge Interaction");
                FridgeInteraction();
            }
        }
    }
    private void FridgeInteraction()
    {
        gameManager.invokeFridgeInteraction();
    }
}

