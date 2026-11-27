using UnityEngine;
using UnityEngine.InputSystem;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject miniMapPanel;
    [SerializeField] private RectTransform playerIcon;
    [SerializeField] private RectTransform currentPlanet;   // chaque scène choisit la sienne
    [SerializeField] private Vector2 iconOffset = new Vector2(0, 80); // au-dessus de la planète

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
        if (isVisible) PlaceIcon();
    }

    private void PlaceIcon()
    {
        if (playerIcon == null || currentPlanet == null) return;
        playerIcon.anchorMin = currentPlanet.anchorMin;
        playerIcon.anchorMax = currentPlanet.anchorMax;
        playerIcon.anchoredPosition = iconOffset;
    }
}