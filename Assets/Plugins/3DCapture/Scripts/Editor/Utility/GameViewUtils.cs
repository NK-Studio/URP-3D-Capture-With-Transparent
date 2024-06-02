using UnityEditor;

namespace NKStudio
{
    public static class GameViewUtils
    {
        public static bool IsMainPlayViewGameView()
        {
            return PlayModeWindow.GetViewType() == PlayModeWindow.PlayModeViewTypes.GameView;
        }

        public static void SwapMainPlayViewToGameView()
        {
            if (IsMainPlayViewGameView())
                return;

            PlayModeWindow.SetViewType(PlayModeWindow.PlayModeViewTypes.GameView);
        }

        public static void DisableMaxOnPlay()
        {
            PlayModeWindow.SetPlayModeFocused(true);
        }

        public static void GetGameRenderSize(out uint width, out uint height)
        {
            PlayModeWindow.GetRenderingResolution(out width, out height);
        }

        public static void SetCustomSize(int width, int height)
        {
            PlayModeWindow.SetCustomRenderingResolution((uint)width, (uint)height, "Capture Resolution");
        }
    }
}