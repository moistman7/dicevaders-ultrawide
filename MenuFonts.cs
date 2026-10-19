using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DiceVadersUltrawide
{
    internal sealed class MenuFonts
    {
        readonly Dictionary<string, Font> _copies = new();

        static string Key(string name) => name.ToLowerInvariant().Replace(" sdf", "").Replace("-regular", "").Replace(" updated", "").Replace(" ", "");

        internal void CaptureAvailable()
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<Font>())
                if (font != null && !font.name.StartsWith("Ultrawide-") && font.name.ToLowerInvariant().Contains("vader")) CopySource(font);
        }

        internal Font Find(string family)
        {
            string key = Key(family);
            foreach (var pair in _copies)
                if (pair.Value != null && pair.Key.Contains(key)) return pair.Value;
            return null;
        }

        internal Font Copy(TextMeshProUGUI label)
        {
            if (label == null || label.font == null) return null;
            var source = label.font.sourceFontFile;
            if (source != null) return CopySource(source);
            // Some baked Settings TMP assets strip sourceFontFile in the build.
            // Keep sources while they are loaded and resolve the same named family.
            CaptureAvailable();
            string target = Key(label.font.name);
            foreach (var pair in _copies)
                if (pair.Value != null && (target.Contains(pair.Key) || pair.Key.Contains(target))) return pair.Value;
            return null;
        }

        Font CopySource(Font source)
        {
            string key = Key(source.name);
            if (_copies.TryGetValue(key, out var cached) && cached != null) return cached;
            // Own the Font object rather than borrowing the source object that
            // disappeared when the game unloaded a previous menu scene.
            var owned = UnityEngine.Object.Instantiate(source);
            owned.name = "Ultrawide-" + source.name;
            owned.hideFlags = HideFlags.HideAndDontSave;
            if (owned.dynamic) owned.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%:/.- ", 42, FontStyle.Normal);
            if (owned.material != null)
            {
                owned.material.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                if (owned.material.mainTexture != null) owned.material.mainTexture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }
            _copies[key] = owned;
            Plugin.Instance.Log.LogInfo("Copied native menu font: " + source.name);
            return owned;
        }

        internal void Clear()
        {
            foreach (var font in _copies.Values) if (font != null) UnityEngine.Object.Destroy(font);
            _copies.Clear();
        }
    }
}
