using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using StarVaders;

namespace DiceVadersUltrawide
{
    // The game's hover selection is also used by mouse clicks and releases.
    // Scope enlargement to input so game layout/animations still see base sizes.
    [HarmonyPatch]
    internal static class PointerSizing
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GenericMouseInputController), nameof(GenericMouseInputController.RayCastPointerHover));
            yield return AccessTools.Method(typeof(GenericMouseInputController), nameof(GenericMouseInputController.OnMouseClick));
            yield return AccessTools.Method(typeof(GenericMouseInputController), nameof(GenericMouseInputController.OnMouseClickReleased));
            yield return AccessTools.Method(typeof(GenericMouseInputController), nameof(GenericMouseInputController.OnMouseCancel));
        }

        static void Prefix(out (HudSizing Sizing, bool Applied) __state)
        {
            var sizing = HudSizing.Active;
            __state = (sizing, false);
            if (sizing != null)
                __state = (sizing, sizing.BeginPointerInput());
        }

        static void Finalizer((HudSizing Sizing, bool Applied) __state)
        {
            if (__state.Sizing != null)
                __state.Sizing.EndPointerInput(__state.Applied);
        }
    }
}
