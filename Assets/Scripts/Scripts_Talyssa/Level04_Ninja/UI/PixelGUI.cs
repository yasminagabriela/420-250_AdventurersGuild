using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Petites aides pour dessiner des fenêtres bleues façon Final Fantasy 1 avec OnGUI.
    /// Tout est exprimé dans une résolution virtuelle de 640x360, puis mis à l'échelle
    /// (le texte reste net car on agrandit la taille de police plutôt que de zoomer).
    /// </summary>
    public static class PixelGUI
    {
        public const float VirtualHeight = 360f;

        public static readonly Color WindowTop = new Color32(32, 56, 168, 235);
        public static readonly Color WindowBottom = new Color32(10, 20, 80, 235);
        public static readonly Color Border = new Color32(240, 240, 255, 255);
        public static readonly Color TextColor = new Color32(255, 255, 255, 255);
        public static readonly Color Disabled = new Color32(130, 130, 160, 255);
        public static readonly Color Highlight = new Color32(255, 230, 120, 255);
        public static readonly Color HpColor = new Color32(90, 230, 140, 255);
        public static readonly Color HpLow = new Color32(240, 80, 90, 255);
        public static readonly Color MpColor = new Color32(110, 170, 255, 255);

        private static Texture2D _white;
        private static Texture2D _gradient;
        private static GUIStyle _label;

        public static float Scale => Screen.height / VirtualHeight;
        public static float VirtualWidth => Screen.width / Scale;

        private static Texture2D White
        {
            get
            {
                if (_white == null)
                {
                    _white = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                    _white.SetPixel(0, 0, Color.white);
                    _white.Apply();
                }
                return _white;
            }
        }

        private static Texture2D Gradient
        {
            get
            {
                if (_gradient == null)
                {
                    _gradient = new Texture2D(1, 32) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
                    for (int y = 0; y < 32; y++)
                        _gradient.SetPixel(0, y, Color.Lerp(WindowBottom, WindowTop, y / 31f));
                    _gradient.Apply();
                }
                return _gradient;
            }
        }

        /// <summary>Convertit un rectangle virtuel (640x360) en pixels écran.</summary>
        public static Rect R(float x, float y, float w, float h)
        {
            float s = Scale;
            return new Rect(x * s, y * s, w * s, h * s);
        }

        public static void Fill(Rect virtualRect, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(R(virtualRect.x, virtualRect.y, virtualRect.width, virtualRect.height), White);
            GUI.color = old;
        }

        /// <summary>Fenêtre bleue avec bordure blanche arrondie (style FF1).</summary>
        public static void Window(Rect v)
        {
            Fill(new Rect(v.x + 2, v.y, v.width - 4, v.height), Border);
            Fill(new Rect(v.x, v.y + 2, v.width, v.height - 4), Border);
            Rect inner = new Rect(v.x + 2, v.y + 2, v.width - 4, v.height - 4);
            GUI.DrawTexture(R(inner.x, inner.y, inner.width, inner.height), Gradient);
        }

        public static void Text(Rect v, string text, int size = 13, Color? color = null,
                                TextAnchor anchor = TextAnchor.UpperLeft, bool shadow = true)
        {
            if (_label == null)
            {
                _label = new GUIStyle(GUI.skin.label) { wordWrap = true, richText = true };
            }
            _label.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * Scale));
            _label.alignment = anchor;
            Rect r = R(v.x, v.y, v.width, v.height);
            if (shadow)
            {
                _label.normal.textColor = new Color(0, 0, 0, 0.7f);
                GUI.Label(new Rect(r.x + Scale, r.y + Scale, r.width, r.height), text, _label);
            }
            _label.normal.textColor = color ?? TextColor;
            GUI.Label(r, text, _label);
        }

        public static void Bar(Rect v, float ratio, Color fill)
        {
            Fill(v, new Color(0, 0, 0, 0.6f));
            Fill(new Rect(v.x + 1, v.y + 1, (v.width - 2) * Mathf.Clamp01(ratio), v.height - 2), fill);
        }

        /// <summary>Position écran (virtuelle) d'un point du monde, pour placer du texte au-dessus d'un sprite.</summary>
        public static Vector2 WorldToVirtual(Vector3 world)
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.zero;
            Vector3 sp = cam.WorldToScreenPoint(world);
            return new Vector2(sp.x / Scale, (Screen.height - sp.y) / Scale);
        }
    }
}
