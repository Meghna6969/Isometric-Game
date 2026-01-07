using UnityEngine;

public class PlayerInteractionInput : MonoBehaviour
{
    public static PlayerInteractionInput Instance;
    private IInteractable current;
    void Awake()
    {
        Instance = this;
    }
    void OnInteract()
    {
        current?.Interact(gameObject);
    }
    void Update()
    {
        if(current != null && Input.GetKeyDown(KeyCode.F))
        {
            current.Interact(gameObject);
        }
    }
    public void SetCurrent(IInteractable interactable)
    {
        current = interactable;
    }
    public void ClearCurrent(IInteractable interactable)
    {
        if(current == interactable)
        {
            current = null;
        }
    }
}
