using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleMenu : MonoBehaviour
{
    [SerializeField] private SpriteRenderer menuRenderer;
    [SerializeField] private Sprite[] choiceSprites;   // battle_choice01, 02, 03
    [SerializeField] private TMP_Text[] labels;        // Label_1, Label_2, Label_3
    [SerializeField] private string[] optionNames = { "Attaque", "Sort", "Fuite" };
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private int _index;

    public event System.Action<int> Confirmed;

    private void Start()
    {
        for (int i = 0; i < labels.Length; i++)
            labels[i].text = optionNames[i];
        Refresh();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame ||
            kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            Move(-1);

        if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame ||
            kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            Move(1);

        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Chose: " + optionNames[_index]);
            Confirmed?.Invoke(_index);
        }
    }

    private void Move(int delta)
    {
        _index = (_index + delta + optionNames.Length) % optionNames.Length;
        Refresh();
    }

    private void Refresh()
    {
        menuRenderer.sprite = choiceSprites[_index];

        for (int i = 0; i < labels.Length; i++)
            labels[i].color = (i == _index) ? selectedColor : normalColor;
    }
}