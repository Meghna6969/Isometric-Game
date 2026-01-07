using UnityEngine;

public class FridgeInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isOpen;
    public GameObject focusUI;
    public Animator animator;
    void Awake()
    {
        focusUI.SetActive(false);
    }
    public void OnFocusEnter()
    {
        focusUI.SetActive(true);
    }
    public void OnFocusExit()
    {
        focusUI.SetActive(false);
    }
    public void Interact(GameObject interactor)
    {
        isOpen = !isOpen;
        animator.SetBool("DoorOpen", isOpen);
    }
    public string GetPrompt()
    {
        return isOpen ? "Close fridge" : "Open Fridge";
    }
}
