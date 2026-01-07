using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionInput : MonoBehaviour
{
    public static PlayerInteractionInput Instance;
    private IInteractable current;
    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if(current != null && Keyboard.current.fKey.wasPressedThisFrame)
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
