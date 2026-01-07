using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    private IInteractable interactable;

    void Awake()
    {
        interactable = GetComponentInParent<IInteractable>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        if(interactable != null)
        {
            interactable.OnFocusEnter();
            PlayerInteractionInput.Instance.SetCurrent(interactable);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        if(interactable != null)
        {
            interactable.OnFocusExit();
            PlayerInteractionInput.Instance.ClearCurrent(interactable);
        }
    }
}
