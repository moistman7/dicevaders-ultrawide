using HarmonyLib;
using StarVaders;
using StarVaders.PauseMenu;

namespace DiceVadersUltrawide
{
    [HarmonyPatch(typeof(SettingsSceneController), nameof(SettingsSceneController.OnEnable))]
    internal static class SettingsMenuLifecycle
    {
        static void Postfix(SettingsSceneController __instance) => ModMenu.SettingsOpened(__instance);
    }

    [HarmonyPatch(typeof(PauseMenuController), nameof(PauseMenuController.OnPause))]
    internal static class PauseMenuLifecycle
    {
        static void Prefix(PauseMenuController __instance) => ModMenu.PauseOpened(__instance);
    }
}
