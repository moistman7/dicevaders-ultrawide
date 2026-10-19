using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using StarVaders;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceVadersUltrawide
{
    // Undo our changes before game animation/layout, then apply once after it.
    public class HudSizing : MonoBehaviour
    {
        public HudSizing(IntPtr pointer) : base(pointer) { }
        internal static ConfigEntry<float> _hudScale;
        internal static ConfigEntry<float> _boardScale;
        internal const float DefaultScale = 1.23f;
        float _nextDiscovery;
        readonly List<Transform> _targets = new();
        Transform _board;
        readonly List<(Transform Target, Vector3 Scale, Vector3 Position)> _applied = new();

        void Start()
        {
            _hudScale = Plugin.Instance.Config.Bind("Interface", "HudScale", DefaultScale,
                new ConfigDescription("Encounter panel size multiplier. F6 decreases and F7 increases both HUD and board size.", new AcceptableValueRange<float>(1f, 1.35f)));
            _boardScale = Plugin.Instance.Config.Bind("Interface", "BoardScale", DefaultScale,
                new ConfigDescription("Encounter board size multiplier.", new AcceptableValueRange<float>(1f, 1.35f)));
        }

        void Restore()
        {
            foreach (var entry in _applied)
            {
                if (entry.Target == null) continue;
                entry.Target.localScale = entry.Scale;
                entry.Target.localPosition = entry.Position;
            }
            _applied.Clear();
        }

        void OnDestroy() => Restore();

        void Update()
        {
            Restore();
            if (_hudScale == null) return;
            float step = Hotkeys.Pressed(Hotkeys.Increase) ? 0.02f :
                Hotkeys.Pressed(Hotkeys.Decrease) ? -0.02f : 0f;
            if (step != 0f)
            {
                _hudScale.Value = Mathf.Clamp(_hudScale.Value + step, 1f, 1.35f);
                _boardScale.Value = Mathf.Clamp(_boardScale.Value + step, 1f, 1.35f);
                Plugin.Instance.Log.LogInfo($"HUD size: {_hudScale.Value:P0}; board: {_boardScale.Value:P0}");
            }
            if (Time.unscaledTime < _nextDiscovery) return;
            _nextDiscovery = Time.unscaledTime + 0.25f;
            _targets.Clear();
            _board = null;
            foreach (var view in UnityEngine.Object.FindObjectsByType<EncounterView>(FindObjectsSortMode.None))
            {
                Add(view.ArtifactPanel);
                Add(view.ScoreArea);
                Add(view.ButtonsArea);
                Add(view.GridAreaHolder);
                _board = view.GridAreaHolder;
            }
        }

        void Add(Transform target)
        {
            if (target == null || _targets.Contains(target)) return;
            // Never scale both an ancestor and its descendant.
            foreach (var other in _targets)
                if (target.IsChildOf(other)) return;
            _targets.RemoveAll(other => other.IsChildOf(target));
            _targets.Add(target);
        }

        void LateUpdate()
        {
            if (_hudScale == null || !Plugin.Enabled.Value || Screen.height <= 0 ||
                (float)Screen.width / Screen.height <= 16f / 9f + 0.01f) return;
            foreach (var target in _targets)
            {
                if (target == null || !target.gameObject.activeInHierarchy) continue;
                float scale = target == _board ? _boardScale.Value : _hudScale.Value;
                if (scale <= 1f) continue;
                _applied.Add((target, target.localScale, target.localPosition));
                target.localScale *= scale;
                KeepOnScreen(target);
            }
        }

        static void KeepOnScreen(Transform target)
        {
            var rect = target.TryCast<RectTransform>();
            if (rect == null) return;
            var canvas = rect.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var corners = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
            rect.GetWorldCorners(corners);
            Vector2 min = new(float.MaxValue, float.MaxValue), max = new(float.MinValue, float.MinValue);
            foreach (var corner in corners)
            {
                var point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            // Full-screen containers are not useful bounds for their artwork.
            if (max.x - min.x > Screen.width || max.y - min.y > Screen.height) return;
            const float margin = 8f;
            float x = min.x < margin ? margin - min.x : max.x > Screen.width - margin ? Screen.width - margin - max.x : 0f;
            float y = min.y < margin ? margin - min.y : max.y > Screen.height - margin ? Screen.height - margin - max.y : 0f;
            if (x == 0f && y == 0f) return;
            var parent = target.parent?.TryCast<RectTransform>();
            if (parent == null) return;
            Vector2 current = RectTransformUtility.WorldToScreenPoint(camera, target.position);
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, current + new Vector2(x, y), camera, out Vector3 world))
                target.position = world;
        }
    }
}
