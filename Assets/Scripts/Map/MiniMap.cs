using UnityEngine;
using UnityEngine.InputSystem;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject miniMapPanel;
    private bool isVisible;

    private void Start()
    {
        isVisible = false;
        miniMapPanel.SetActive(false);   // closed at the start
    }

    private void Update()
    {
        // M key as a shortcut (optional)
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        miniMapPanel.SetActive(isVisible);
    }
}