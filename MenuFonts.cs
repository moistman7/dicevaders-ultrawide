using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DiceVadersUltrawide
{
    internal sealed class MenuFonts
    {
        readonly Dictionary<int, Font> _copies = new();

        internal Font Copy(TextMeshProUGUI label)
        {
            if (label == null || label.font == null || label.font.sourceFontFile == null) return null;
            var source = label.font.sourceFontFile;
            int id = source.GetInstanceID();
            if (_copies.TryGetValue(id, out var cached) && cached != null) return cached;
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
            _copies[id] = owned;
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
