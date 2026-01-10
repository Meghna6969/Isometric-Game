using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public GameManager Instance;

    public TextMeshProUGUI infoText;


    private void Awake()
    {
        Instance = this;
    }
    public void invokeFridgeInteraction()
    {
        Debug.Log("Invoked At the GameManager");
    }
}
