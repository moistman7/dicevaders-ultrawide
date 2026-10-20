using System.Collections.Generic;
using UnityEngine;

namespace DiceVadersUltrawide;

public partial class HudSizing
{
    const string FramePrefix = "UltrawideFrame_";
    // Wrapper per side panel, keyed by the panel's instance ID.
    readonly Dictionary<int, RectTransform> _frameWrappers = new();

    // Each side panel is moved into an invisible parent that exactly matches
    // its original parent. Centering offsets only that parent, so the game's
    // own panel animations and sway keep working unchanged underneath, and
    // full width is an exact zero offset.
    RectTransform Wrap(Transform target)
    {
        var panel = target?.TryCast<RectTransform>();
        var parent = panel?.parent?.TryCast<RectTransform>();
        if (parent == null) return null;
        if (parent.name == FramePrefix + panel.name) return parent;
        var obj = new GameObject(FramePrefix + panel.name);
        obj.layer = panel.gameObject.layer;
        var wrapper = obj.AddComponent<RectTransform>();
        wrapper.SetParent(parent, false);
        wrapper.SetSiblingIndex(panel.GetSiblingIndex());
        wrapper.anchorMin = Vector2.zero;
        wrapper.anchorMax = Vector2.one;
        wrapper.pivot = parent.pivot;
        wrapper.offsetMin = wrapper.offsetMax = Vector2.zero;
        wrapper.localScale = Vector3.one;
        wrapper.localRotation = Quaternion.identity;
        panel.SetParent(wrapper, false);
        return wrapper;
    }

    void Frame(Transform target, bool centered)
    {
        var panel = target?.TryCast<RectTransform>();
        if (panel == null) return;
        var wrapper = Wrap(panel);
        var parent = wrapper?.parent?.TryCast<RectTransform>();
        if (parent == null) return;
        _frameWrappers[panel.GetInstanceID()] = wrapper;
        float width = Mathf.Min(parent.rect.width, parent.rect.height * (16f / 9f));
        float shift = centered ? (.5f - (panel.anchorMin.x + panel.anchorMax.x) * .5f) * (parent.rect.width - width) : 0f;
        var desired = new Vector2(shift, 0f);
        if (wrapper.anchoredPosition != desired) wrapper.anchoredPosition = desired;
    }

    void RestoreHudFrame()
    {
        foreach (var wrapper in _frameWrappers.Values)
            if (wrapper != null && wrapper.anchoredPosition != Vector2.zero) wrapper.anchoredPosition = Vector2.zero;
    }

    static Transform FindPanel(Transform holder, string name) =>
        holder?.Find(name) ?? holder?.Find(FramePrefix + name + "/" + name);

    void UpdateHudFrame()
    {
        // Wrappers stay in place at zero offset; full width needs no tracking.
        if (!_centerHud.Value) { RestoreHudFrame(); _frameWrappers.Clear(); return; }
        // Uses the encounter view cached by target discovery; no scene search.
        if (_view == null) return;
        Frame(FindPanel(_view.AllHolder, "LeftPanel"), true);
        Frame(FindPanel(_view.AllHolder, "RightPanel"), true);
    }
}
