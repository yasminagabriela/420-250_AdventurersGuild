using System;
using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Boîte de dialogue en bas de l'écran (texte qui s'écrit lettre par lettre).
    /// Bloque le PlayerController pendant la discussion.
    /// Utilisation : DialogueBox.Show("Ermite", new[] { "Bonjour", "Bonne route !" }, () => { ... });
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        private static DialogueBox _instance;

        public static bool IsOpen => _instance != null && _instance._lines != null;

        /// <summary>Frame où la boîte s'est fermée : évite de rouvrir le dialogue avec la même touche.</summary>
        public static int LastClosedFrame { get; private set; } = -1;

        [SerializeField] private float charsPerSecond = 45f;

        private string _speaker;
        private string[] _lines;
        private int _index;
        private float _visibleChars;
        private int _openedFrame;
        private Action _onDone;
        private PlayerController _player;

        public static void Show(string speaker, string[] lines, Action onDone = null)
        {
            if (lines == null || lines.Length == 0) { onDone?.Invoke(); return; }
            if (_instance == null)
            {
                var go = new GameObject("DialogueBox");
                _instance = go.AddComponent<DialogueBox>();
            }
            _instance.Open(speaker, lines, onDone);
        }

        private void Open(string speaker, string[] lines, Action onDone)
        {
            _speaker = speaker;
            _lines = lines;
            _index = 0;
            _visibleChars = 0;
            _onDone = onDone;
            _openedFrame = Time.frameCount;
            _player = FindFirstObjectByType<PlayerController>();
            if (_player != null) _player.enabled = false;
        }

        private void Update()
        {
            if (_lines == null || Time.frameCount == _openedFrame) return;
            string line = _lines[_index];
            _visibleChars = Mathf.Min(line.Length, _visibleChars + charsPerSecond * Time.deltaTime);

            if (!GameInput.Confirm) return;
            if (_visibleChars < line.Length) { _visibleChars = line.Length; return; } // afficher tout de suite
            _index++;
            _visibleChars = 0;
            if (_index >= _lines.Length) Close();
        }

        private void Close()
        {
            _lines = null;
            LastClosedFrame = Time.frameCount;
            if (_player != null) _player.enabled = true;
            Action done = _onDone;
            _onDone = null;
            done?.Invoke();
        }

        private void OnGUI()
        {
            if (_lines == null) return;
            float vw = PixelGUI.VirtualWidth;
            float w = Mathf.Min(560f, vw - 20f), x = (vw - w) / 2f, y = 264f;
            PixelGUI.Window(new Rect(x, y, w, 88));
            if (!string.IsNullOrEmpty(_speaker))
                PixelGUI.Text(new Rect(x + 12, y + 6, w - 24, 18), _speaker, 12, PixelGUI.Highlight);
            string line = _lines[_index];
            string shown = line.Substring(0, Mathf.Clamp(Mathf.FloorToInt(_visibleChars), 0, line.Length));
            PixelGUI.Text(new Rect(x + 12, y + 24, w - 24, 56), shown, 13);
            if (_visibleChars >= line.Length && Mathf.Repeat(Time.time, 0.8f) < 0.5f)
                PixelGUI.Text(new Rect(x + w - 26, y + 66, 16, 16), "▼", 11, PixelGUI.TextColor);
        }
    }
}
