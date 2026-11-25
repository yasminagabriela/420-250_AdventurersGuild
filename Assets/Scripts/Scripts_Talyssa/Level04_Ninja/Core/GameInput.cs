using UnityEngine.InputSystem;

namespace AdventurersGuild
{
    /// <summary>Raccourcis clavier communs (menus, dialogues, combat).</summary>
    public static class GameInput
    {
        private static Keyboard Kb => Keyboard.current;

        public static bool Confirm => Kb != null && (Kb.enterKey.wasPressedThisFrame || Kb.spaceKey.wasPressedThisFrame
                                                     || Kb.eKey.wasPressedThisFrame || Kb.zKey.wasPressedThisFrame
                                                     || Kb.numpadEnterKey.wasPressedThisFrame);
        public static bool Cancel => Kb != null && (Kb.escapeKey.wasPressedThisFrame || Kb.backspaceKey.wasPressedThisFrame
                                                    || Kb.xKey.wasPressedThisFrame);
        public static bool Up => Kb != null && (Kb.upArrowKey.wasPressedThisFrame || Kb.wKey.wasPressedThisFrame);
        public static bool Down => Kb != null && (Kb.downArrowKey.wasPressedThisFrame || Kb.sKey.wasPressedThisFrame);
        public static bool Left => Kb != null && (Kb.leftArrowKey.wasPressedThisFrame || Kb.aKey.wasPressedThisFrame);
        public static bool Right => Kb != null && (Kb.rightArrowKey.wasPressedThisFrame || Kb.dKey.wasPressedThisFrame);
        public static bool Interact => Kb != null && (Kb.eKey.wasPressedThisFrame || Kb.spaceKey.wasPressedThisFrame
                                                      || Kb.enterKey.wasPressedThisFrame);
    }
}
