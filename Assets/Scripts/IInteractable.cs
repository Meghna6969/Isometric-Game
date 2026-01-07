using System;
using UnityEngine;

public interface IInteractable
{
    void OnFocusEnter();
    void OnFocusExit();
    void Interact(GameObject interactor);
    string GetPrompt();
}