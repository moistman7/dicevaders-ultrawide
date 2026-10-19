using UnityEngine;

namespace DiceVadersUltrawide
{
    // Small original UI textures keep the Settings-style shapes available even
    // before the game has loaded its Settings scene. No game assets are shipped.
    internal static class MenuSkin
    {
        internal static Sprite Button => _button != null ? _button : _button = Shape(128, 32, 6, 26, 8);
        internal static Sprite Panel => _panel != null ? _panel : _panel = Shape(128, 128, 29, 105, 5);
        internal static Sprite Bar => _bar != null ? _bar : _bar = Shape(128, 24, 0, 24, 6);
        static Sprite _button, _panel, _bar;

        internal static Sprite Keep(Sprite sprite)
        {
            if (sprite == null) return null;
            sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            if (sprite.texture != null) sprite.texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }

        static Sprite Shape(int width, int height, int bottom, int top, int skew)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Color32>(width * height);
            for (int y = 0; y < height; y++)
            {
                float fraction = (float)(y - bottom) / (top - bottom);
                for (int x = 0; x < width; x++)
                {
                    bool inside = y >= bottom && y < top && x >= skew * (1f - fraction) && x < width - skew * fraction;
                    pixels[y * width + x] = new Color32(255, 255, 255, inside ? (byte)255 : (byte)0);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            // FullRect preserves intentional transparent padding; Tight trims it
            // and turns our slim buttons/panel into oversized solid rectangles.
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
