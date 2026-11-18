using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Base pour tout ce qu'on peut utiliser avec E / Espace quand on est à côté
    /// (PNJ, coffre...). Affiche une petite bulle "E" au-dessus de l'objet.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] protected float interactRadius = 1.3f;
        [SerializeField] protected Vector3 promptOffset = new Vector3(0f, 0.9f, 0f);

        private static Interactable _closest;
        private Transform _player;

        protected bool PlayerInRange { get; private set; }

        protected virtual void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        protected virtual void Update()
        {
            if (_player == null) return;
            float d = Vector2.Distance(_player.position, transform.position);
            PlayerInRange = d <= interactRadius;

            // seul l'objet le plus proche réagit
            if (PlayerInRange && (_closest == null || !_closest.PlayerInRange ||
                d < Vector2.Distance(_player.position, _closest.transform.position)))
                _closest = this;
            if (!PlayerInRange && _closest == this) _closest = null;

            if (_closest == this && CanInteract && !DialogueBox.IsOpen
                && Time.frameCount != DialogueBox.LastClosedFrame && GameInput.Interact)
                Interact();
        }

        protected virtual bool CanInteract => true;
        protected abstract void Interact();

        protected virtual void OnGUI()
        {
            if (_closest != this || !CanInteract || DialogueBox.IsOpen) return;
            Vector2 v = PixelGUI.WorldToVirtual(transform.position + promptOffset);
            float bob = Mathf.Sin(Time.time * 5f) * 1.5f;
            PixelGUI.Window(new Rect(v.x - 9, v.y - 18 + bob, 18, 18));
            PixelGUI.Text(new Rect(v.x - 9, v.y - 18 + bob, 18, 18), "E", 11, PixelGUI.Highlight, TextAnchor.MiddleCenter, false);
        }
    }
}
