using System;
using System.Collections.Generic;
using StarVaders;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DiceVadersUltrawide
{
    // Reuse the settings screen's actual slider artwork and font.
    // Clones get fresh events and disabled game scripts before activation.
    public class ModMenu : MonoBehaviour
    {
        public ModMenu(IntPtr pointer) : base(pointer) { }
        GameObject _root;
        GameObject _panel;
        SettingsSceneController _settings;
        TMP_FontAsset _font;
        Material _fontMaterial;
        Sprite _buttonSprite;
        Slider _hud;
        Slider _board;
        TextMeshProUGUI _hudLabel;
        TextMeshProUGUI _boardLabel;
        TextMeshProUGUI _enabledLabel;
        float _nextSearch;
        bool _syncing;
        bool _nativeSliders;
        GameObject _entry;
        readonly List<MonoBehaviour> _suspendedInput = new();
        float _previousTimeScale;
        bool _previousCursorVisible;
        CursorLockMode _previousCursorLock;
        int _scene;

        void Update()
        {
            int scene = SceneManager.GetActiveScene().handle;
            if (_scene != scene)
            {
                if (_panel != null && _panel.activeSelf) SetOpen(false);
                _scene = scene;
                _nextSearch = 0f;
            }
            var keyboard = Keyboard.current;
            bool toggle = keyboard != null && keyboard.f10Key.wasPressedThisFrame;
            if (Time.unscaledTime >= _nextSearch || toggle)
            {
                _nextSearch = Time.unscaledTime + 1f;
                _settings = null;
                foreach (var settings in UnityEngine.Object.FindObjectsByType<SettingsSceneController>(FindObjectsSortMode.None))
                {
                    if (settings.isActiveAndEnabled) { _settings = settings; break; }
                }
                if (_root != null && !_nativeSliders && !_panel.activeSelf && _settings != null && _settings.MasterVolumeSlider != null)
                {
                    UnityEngine.Object.Destroy(_root);
                    _root = null;
                }
                if (_root == null && HudSizing._hudScale != null && (_settings != null || toggle))
                {
                    try { Build(_settings); }
                    catch (Exception error)
                    {
                        Plugin.Instance.Log.LogError("Mod menu creation failed: " + error);
                        if (_root != null) UnityEngine.Object.Destroy(_root);
                        _root = null;
                        _nextSearch = Time.unscaledTime + 10f;
                    }
                }
            }
            if (_root == null) return;
            _entry.SetActive(_settings != null && _settings.isActiveAndEnabled);
            if (toggle) SetOpen(!_panel.activeSelf);
            if (_panel.activeSelf)
            {
                Sync();
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) SetOpen(false);
            }
        }

        void OnDestroy()
        {
            if (_panel != null && _panel.activeSelf) SetOpen(false);
            if (_root != null) UnityEngine.Object.Destroy(_root);
        }

        void SetOpen(bool open)
        {
            if (_panel == null || _panel.activeSelf == open) return;
            if (open)
            {
                _previousTimeScale = Time.timeScale;
                _previousCursorVisible = Cursor.visible;
                _previousCursorLock = Cursor.lockState;
                _suspendedInput.Clear();
                foreach (var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                {
                    var type = behaviour.GetIl2CppType();
                    if (behaviour.enabled && type.Namespace != null && type.Namespace.StartsWith("StarVaders") &&
                        (type.Name.Contains("InputHandler") || type.Name.Contains("KeyInputController")))
                    {
                        behaviour.enabled = false;
                        _suspendedInput.Add(behaviour);
                    }
                }
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Sync();
            }
            else
            {
                Time.timeScale = _previousTimeScale;
                Cursor.lockState = _previousCursorLock;
                Cursor.visible = _previousCursorVisible;
                foreach (var behaviour in _suspendedInput)
                    if (behaviour != null) behaviour.enabled = true;
                _suspendedInput.Clear();
            }
            _panel.SetActive(open);
        }

        static RectTransform Rect(GameObject obj, Transform parent, Vector2 size, Vector2 position)
        {
            var rect = obj.GetComponent<RectTransform>() ?? obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one;
            return rect;
        }

        void Build(SettingsSceneController settings)
        {
            Slider source = settings != null && settings.MasterVolumeSlider != null ? settings.MasterVolumeSlider.Slider : null;
            if (source == null)
                foreach (var option in Resources.FindObjectsOfTypeAll<OptionSlider>())
                    if (option.Slider != null) { source = option.Slider; break; }
            _nativeSliders = source != null;
            TextMeshProUGUI text = settings != null ? settings.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            if (text == null)
                foreach (var candidate in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
                    if (candidate.font != null) { text = candidate; break; }
            if (text == null) throw new InvalidOperationException("Waiting for the game's UI font to load.");
            _font = text.font;
            _fontMaterial = text.fontSharedMaterial;
            var image = source != null ? source.GetComponentInChildren<Image>(true) : null;
            _buttonSprite = image != null ? image.sprite : null;

            _root = new GameObject("UltrawideModMenu");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            _root.AddComponent<GraphicRaycaster>();

            _entry = Button(_root.transform, "ULTRAWIDE MOD", new Vector2(0, -470), new Vector2(360, 66), () =>
            {
                SetOpen(true);
            });
            // A fixed lower-center entry avoids changing native menu navigation.
            _panel = new GameObject("UltrawideSettings");
            var panelRect = Rect(_panel, _root.transform, Vector2.zero, Vector2.zero);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            var dim = _panel.AddComponent<Image>();
            dim.color = new Color(0.015f, 0.025f, 0.05f, 0.96f);
            dim.raycastTarget = true;
            var frame = new GameObject("Panel");
            Rect(frame, _panel.transform, new Vector2(900, 670), Vector2.zero);
            var background = frame.AddComponent<Image>();
            background.sprite = _buttonSprite;
            background.type = Image.Type.Sliced;
            background.color = new Color(0.12f, 0.28f, 0.30f, 1f);
            Label(frame.transform, "ULTRAWIDE MOD", new Vector2(0, 270), new Vector2(800, 70), 42);
            _enabledLabel = Button(frame.transform, "", new Vector2(0, 170), new Vector2(620, 65), () =>
            {
                Plugin.Enabled.Value = !Plugin.Enabled.Value;
                Sync();
            }).GetComponentInChildren<TextMeshProUGUI>();
            _hudLabel = Label(frame.transform, "HUD SIZE", new Vector2(0, 90), new Vector2(760, 50), 30);
            _hud = MakeSlider(source, frame.transform, new Vector2(0, 35), value =>
            {
                if (!_syncing) HudSizing._hudScale.Value = Mathf.Round(value) / 100f;
            });
            _boardLabel = Label(frame.transform, "BOARD SIZE", new Vector2(0, -40), new Vector2(760, 50), 30);
            _board = MakeSlider(source, frame.transform, new Vector2(0, -95), value =>
            {
                if (!_syncing) HudSizing._boardScale.Value = Mathf.Round(value) / 100f;
            });
            Button(frame.transform, "RESET TO 123%", new Vector2(-190, -200), new Vector2(340, 70), () =>
            {
                HudSizing._hudScale.Value = HudSizing.DefaultScale;
                HudSizing._boardScale.Value = HudSizing.DefaultScale;
                Sync();
            });
            Button(frame.transform, "CLOSE", new Vector2(200, -200), new Vector2(300, 70), () => SetOpen(false));
            Label(frame.transform, "Changes save automatically   /   F10 opens this panel", new Vector2(0, -280), new Vector2(850, 45), 22);
            Sync();
            _panel.SetActive(false);
            Plugin.Instance.Log.LogInfo("Mod menu ready. F10 opens it anywhere; Settings also has an Ultrawide Mod entry.");
        }

        Slider MakeSlider(Slider source, Transform parent, Vector2 position, Action<float> changed)
        {
            if (source == null) return MakeFallbackSlider(parent, position, changed);
            // Clone under an inactive parent, so inherited scripts never run enabled.
            var staging = new GameObject("SliderStaging");
            staging.SetActive(false);
            var clone = UnityEngine.Object.Instantiate(source.gameObject, staging.transform);
            clone.SetActive(false);
            foreach (var behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string space = behaviour.GetIl2CppType().Namespace;
                if (space != "UnityEngine.UI" && space != "TMPro") behaviour.enabled = false;
            }
            var slider = clone.GetComponent<Slider>();
            slider.onValueChanged = new Slider.SliderEvent();
            slider.minValue = 100;
            slider.maxValue = 135;
            slider.wholeNumbers = true;
            slider.interactable = true;
            slider.onValueChanged.AddListener((UnityAction<float>)(value => changed(value)));
            Rect(clone, parent, new Vector2(620, 50), position);
            UnityEngine.Object.Destroy(staging);
            clone.SetActive(true);
            return slider;
        }

        Slider MakeFallbackSlider(Transform parent, Vector2 position, Action<float> changed)
        {
            var obj = new GameObject("SizeSlider");
            Rect(obj, parent, new Vector2(620, 50), position);
            var track = new GameObject("Track");
            Rect(track, obj.transform, new Vector2(600, 14), Vector2.zero);
            var trackImage = track.AddComponent<Image>();
            trackImage.color = new Color(0.08f, 0.18f, 0.22f, 1f);
            var handle = new GameObject("Handle");
            var handleRect = Rect(handle, track.transform, new Vector2(28, 42), Vector2.zero);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.35f, 0.9f, 0.65f, 1f);
            var slider = obj.AddComponent<Slider>();
            slider.targetGraphic = handleImage;
            slider.handleRect = handleRect;
            slider.minValue = 100;
            slider.maxValue = 135;
            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener((UnityAction<float>)(value => changed(value)));
            return slider;
        }

        TextMeshProUGUI Label(Transform parent, string value, Vector2 position, Vector2 size, float fontSize)
        {
            var obj = new GameObject("Label");
            Rect(obj, parent, size, position);
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = _font;
            label.fontSharedMaterial = _fontMaterial;
            label.text = value;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.74f, 0.96f, 0.89f, 1);
            label.raycastTarget = false;
            return label;
        }

        GameObject Button(Transform parent, string value, Vector2 position, Vector2 size, Action clicked)
        {
            var obj = new GameObject(value.Length == 0 ? "EnableFix" : value);
            Rect(obj, parent, size, position);
            var image = obj.AddComponent<Image>();
            image.sprite = _buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.18f, 0.50f, 0.43f, 1);
            var button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener((UnityAction)(() => clicked()));
            Label(obj.transform, value, Vector2.zero, size - new Vector2(20, 10), 28);
            return obj;
        }

        void Sync()
        {
            _syncing = true;
            _hud.SetValueWithoutNotify(HudSizing._hudScale.Value * 100);
            _board.SetValueWithoutNotify(HudSizing._boardScale.Value * 100);
            _hudLabel.text = $"HUD SIZE   {HudSizing._hudScale.Value:P0}";
            _boardLabel.text = $"BOARD SIZE   {HudSizing._boardScale.Value:P0}";
            _enabledLabel.text = "ULTRAWIDE FIX: " + (Plugin.Enabled.Value ? "ON" : "OFF");
            _syncing = false;
        }
    }
}
