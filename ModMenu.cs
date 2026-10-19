using System;
using System.Collections.Generic;
using StarVaders;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using StarVaders.PauseMenu;
using Il2CppInterop.Runtime.Attributes;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace DiceVadersUltrawide
{
    // Reuse the settings screen's actual slider artwork and font.
    // Clones get fresh events and disabled game scripts before activation.
    public class ModMenu : MonoBehaviour
    {
        public ModMenu(IntPtr pointer) : base(pointer) { }
        static ModMenu _live;

        void Awake() => _live = this;

        internal static void SettingsOpened(SettingsSceneController settings)
        {
            if (_live == null) return;
            _live._settings = settings;
            _live.ShowEntryNow(true);
        }

        internal static void PauseOpened(PauseMenuController pause)
        {
            if (_live == null) return;
            _live._pause = pause;
            _live.ShowEntryNow(false);
        }

        void ShowEntryNow(bool settings)
        {
            _nextSearch = Time.unscaledTime + 1f;
            if (_root == null && HudSizing._hudScale != null) Build(_settings);
            if (_entry == null) return;
            PositionEntry(settings, !settings);
            _entry.SetActive(true);
            Plugin.Instance.Log.LogDebug(settings ? "Settings entry shown by OnEnable hook." : "Pause entry prepared by OnPause hook.");
        }
        GameObject _root;
        GameObject _panel;
        SettingsSceneController _settings;

        Font _labelFont;


        Slider _hud;
        Slider _board;
        Text _hudLabel;
        Text _boardLabel;
        Text _enabledLabel;
        float _nextSearch;
        bool _syncing;

        GameObject _entry;
        readonly List<MonoBehaviour> _suspendedInput = new();
        float _previousTimeScale;
        bool _previousCursorVisible;
        CursorLockMode _previousCursorLock;
        int _scene;
        PauseMenuController _pause;
        GameObject _eventRoot;
        readonly List<EventSystem> _suspendedEvents = new();

        bool _smokeOpened;
        float _smokeCloseAt;
        bool _captureStarted;
        bool _buttonTestDone;



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
            bool smoke = Environment.GetEnvironmentVariable("DICEVADERS_UW_MENU_TEST") == "1";
            bool toggle = Hotkeys.Pressed(Hotkeys.Menu) || (smoke && !_smokeOpened && Time.unscaledTime > 5f && Time.unscaledTime >= _nextSearch);
            if (Time.unscaledTime >= _nextSearch || toggle)
            {
                _nextSearch = Time.unscaledTime + 1f;
                _settings = null;
                _pause = null;
                foreach (var pause in UnityEngine.Object.FindObjectsByType<PauseMenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (!pause.gameObject.scene.IsValid()) continue;
                    if (pause.IsPaused) { _pause = pause; break; }
                    if (_pause == null) _pause = pause;
                }
                foreach (var settings in UnityEngine.Object.FindObjectsByType<SettingsSceneController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (!settings.gameObject.scene.IsValid()) continue;
                    if (settings.isActiveAndEnabled) { _settings = settings; break; }
                    if (_settings == null) _settings = settings;
                }
                if (_root == null && HudSizing._hudScale != null)
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
            bool inSettings = _settings != null && _settings.isActiveAndEnabled;
            bool inPause = _pause != null && _pause.IsPaused;
            _entry.SetActive(inSettings || inPause);
            PositionEntry(inSettings, inPause);

            var mouse = Mouse.current;
            if (!_panel.activeSelf && _entry.activeSelf && mouse != null && mouse.leftButton.wasPressedThisFrame &&
                RectTransformUtility.RectangleContainsScreenPoint(_entry.GetComponent<RectTransform>(), mouse.position.ReadValue(), null))
                OpenFromEntry();
            if (toggle)
            {
                if (_panel.activeSelf) SetOpen(false);
                else OpenFromEntry();
            }
            if (_panel.activeSelf)
            {
                if (smoke && !_smokeOpened)
                {
                    _smokeOpened = true;
                    _smokeCloseAt = Time.unscaledTime + 2f;
                }
                Sync();
                if (smoke && !_buttonTestDone && Environment.GetEnvironmentVariable("DICEVADERS_UW_BUTTON_TEST") == "1")
                {
                    _buttonTestDone = true;
                    StartCoroutine(TestButtonsAfterLayout().WrapToIl2Cpp());
                }
                string capture = Environment.GetEnvironmentVariable("DICEVADERS_UW_MENU_CAPTURE");
                if (smoke && !_captureStarted && !string.IsNullOrEmpty(capture) && Time.unscaledTime >= _smokeCloseAt - 1f)
                {
                    _captureStarted = true;
                    StartCoroutine(CaptureAfterRender(capture).WrapToIl2Cpp());
                }
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) SetOpen(false);
                if (smoke && _smokeOpened && Time.unscaledTime >= _smokeCloseAt)
                {
                    SetOpen(false);
                    _smokeCloseAt = float.PositiveInfinity;
                    Plugin.Instance.Log.LogInfo($"Menu smoke test closed: restored timeScale={Time.timeScale}; previous EventSystems restored.");
                }
            }
        }

        [HideFromIl2Cpp]
        System.Collections.IEnumerator TestButtonsAfterLayout()
        {
            yield return null;
            yield return null;
            TestButtons();
        }

        void TestButtons()
        {
            bool enabled = Plugin.Enabled.Value;
            float hud = HudSizing._hudScale.Value, board = HudSizing._boardScale.Value;
            try
            {
                ClickTestButton("EnableFix");
                if (Plugin.Enabled.Value == enabled) throw new InvalidOperationException("Toggle button callback did not run.");
                HudSizing._hudScale.Value = 1.01f;
                HudSizing._boardScale.Value = 1.02f;
                ClickTestButton("RESET TO 123%");
                if (Math.Abs(HudSizing._hudScale.Value - HudSizing.DefaultScale) > 0.001f ||
                    Math.Abs(HudSizing._boardScale.Value - HudSizing.DefaultScale) > 0.001f)
                    throw new InvalidOperationException("Reset button callback did not run.");
                ClickTestButton("CLOSE");
                if (_panel.activeSelf) throw new InvalidOperationException("Close button callback did not run.");
                Plugin.Instance.Log.LogInfo("BUTTON TEST PASS: Toggle, Reset, Close are the top raycast targets and their native pointer-click callbacks work.");
            }
            catch (Exception error) { Plugin.Instance.Log.LogError("BUTTON TEST FAIL: " + error); }
            finally
            {
                Plugin.Enabled.Value = enabled;
                HudSizing._hudScale.Value = hud;
                HudSizing._boardScale.Value = board;
                if (_panel.activeSelf) SetOpen(false);
                _smokeCloseAt = float.PositiveInfinity;
            }
        }

        void ClickTestButton(string name)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var button in _panel.GetComponentsInChildren<Button>())
            {
                if (button.gameObject.name != name) continue;
                var rect = button.GetComponent<RectTransform>();
                var data = new PointerEventData(_eventRoot.GetComponent<EventSystem>());
                data.position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
                data.button = PointerEventData.InputButton.Left;
                var results = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
                _eventRoot.GetComponent<EventSystem>().RaycastAll(data, results);
                if (results.Count == 0 || results[0].gameObject.GetComponentInParent<Button>() != button)
                    throw new InvalidOperationException($"Button is not the top raycast target: {name}; hits={results.Count}; first={(results.Count > 0 ? results[0].gameObject.name : "none")}; depth={button.GetComponent<Image>().depth}; culled={button.GetComponent<Image>().canvasRenderer.cull}");
                button.OnPointerClick(data);
                return;
            }
            throw new InvalidOperationException("Button not found: " + name);
        }

        [HideFromIl2Cpp]
        System.Collections.IEnumerator CaptureAfterRender(string path)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                if (texture != null)
                {
                    System.IO.File.WriteAllBytes(path, ImageConversion.EncodeToPNG(texture));
                    UnityEngine.Object.Destroy(texture);
                    Plugin.Instance.Log.LogInfo("End-of-frame menu capture saved.");
                }
            }
            catch (Exception error) { Plugin.Instance.Log.LogWarning("Menu capture unavailable: " + error.Message); }
        }

        void OpenFromEntry()
        {
            SetOpen(true);
        }

        void PositionEntry(bool inSettings, bool inPause)
        {
            var entry = _entry.GetComponent<RectTransform>();
            // Settings retains its existing placement. Pause gets its own row above
            // the Game Menu header instead of overlapping Resume and Options.
            entry.anchoredPosition = new Vector2(0, inSettings ? -470 : -230);
            if (inSettings || !inPause || _pause.ResumeButton == null) return;
            var resume = _pause.ResumeButton.GetComponent<RectTransform>();
            if (resume == null) return;
            var canvas = resume.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var corners = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
            resume.GetWorldCorners(corners);
            float top = float.MinValue;
            foreach (var corner in corners)
                top = Mathf.Max(top, RectTransformUtility.WorldToScreenPoint(camera, corner).y);
            float scale = _root.GetComponent<Canvas>().scaleFactor;
            float screenY = Mathf.Clamp(top + 130f * scale, 50f * scale, Screen.height - 50f * scale);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_root.GetComponent<RectTransform>(),
                new Vector2(Screen.width * 0.5f, screenY), null, out Vector2 local))
                entry.anchoredPosition = local;
        }

        void OnDestroy()
        {
            if (_live == this) _live = null;
            if (_panel != null && _panel.activeSelf) SetOpen(false);
            if (_root != null) UnityEngine.Object.Destroy(_root);
            if (_labelFont != null) UnityEngine.Object.Destroy(_labelFont);
        }

        void EnsureFont()
        {
            if (_labelFont != null) return;
            _labelFont = Font.CreateDynamicFontFromOSFont("Arial", 32);
            _labelFont.hideFlags = HideFlags.HideAndDontSave;
            _labelFont.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%:/.- ", 32, FontStyle.Bold);
            if (_root != null)
                foreach (var label in _root.GetComponentsInChildren<Text>(true)) label.font = _labelFont;
        }

        void SetOpen(bool open)
        {
            if (_panel == null || _panel.activeSelf == open) return;
            if (open)
            {
                EnsureFont();
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
                _suspendedEvents.Clear();
                foreach (var events in UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                    if (events.enabled) { events.enabled = false; _suspendedEvents.Add(events); }
                _eventRoot.SetActive(true);
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
                _eventRoot.SetActive(false);
                foreach (var events in _suspendedEvents)
                    if (events != null) events.enabled = true;
                _suspendedEvents.Clear();
            }
            _panel.SetActive(open);
            if (open)
            {
                Canvas.ForceUpdateCanvases();
                _hudLabel.SetAllDirty();
                _boardLabel.SetAllDirty();
                Canvas.ForceUpdateCanvases();
                int vertices = _hudLabel.cachedTextGenerator.vertexCount;
                Plugin.Instance.Log.LogInfo($"Menu opened: font={(_labelFont != null ? _labelFont.name : "missing")}, HUD text vertices={vertices}, size={_hudLabel.rectTransform.rect.size}");
                if (vertices <= 4)
                    Plugin.Instance.Log.LogError("Mod menu text generated no visible geometry. Please include this log when reporting UI problems.");
            }
        }

        static RectTransform Rect(GameObject obj, Transform parent, Vector2 size, Vector2 position)
        {
            var rect = obj.GetComponent<RectTransform>() ?? obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var anchored = rect.anchoredPosition3D;
            rect.anchoredPosition3D = new Vector3(anchored.x, anchored.y, 0f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return rect;
        }

        void Build(SettingsSceneController settings)
        {
            EnsureFont();
            _root = new GameObject("UltrawideModMenu");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 |
                AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            _root.AddComponent<GraphicRaycaster>();
            _eventRoot = new GameObject("ModMenuInput");
            _eventRoot.SetActive(false);
            _eventRoot.transform.SetParent(_root.transform, false);
            _eventRoot.AddComponent<EventSystem>();
            _eventRoot.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

            _entry = Button(_root.transform, "ULTRAWIDE MOD", new Vector2(0, -470), new Vector2(360, 66), () =>
            {
                OpenFromEntry();
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
            var background = AddGraphic(frame);
            background.Bottom = 0.23f;
            background.Top = 0.82f;
            background.Slant = 20f;
            background.color = new Color(0.075f, 0.18f, 0.21f, 1f);
            Label(frame.transform, "ULTRAWIDE MOD", new Vector2(0, 270), new Vector2(800, 70), 42);
            _enabledLabel = Button(frame.transform, "", new Vector2(0, 170), new Vector2(620, 65), () =>
            {
                Plugin.Enabled.Value = !Plugin.Enabled.Value;
                Sync();
            }).GetComponentInChildren<Text>();
            _hudLabel = Label(frame.transform, "HUD SIZE", new Vector2(0, 90), new Vector2(760, 50), 30);
            _hud = MakeSlider(null, frame.transform, new Vector2(0, 35), value =>
            {
                if (!_syncing) HudSizing._hudScale.Value = Mathf.Round(value) / 100f;
            });
            _boardLabel = Label(frame.transform, "BOARD SIZE", new Vector2(0, -40), new Vector2(760, 50), 30);
            _board = MakeSlider(null, frame.transform, new Vector2(0, -95), value =>
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
            Label(frame.transform, $"Changes save automatically   /   Menu key: {Hotkeys.Menu.Value}", new Vector2(0, -280), new Vector2(850, 45), 22);
            Sync();
            _panel.SetActive(false);
            Plugin.Instance.Log.LogInfo($"Mod menu ready. {Hotkeys.Menu.Value} opens it anywhere; Settings and Pause also have an Ultrawide Mod entry.");
        }

        [HideFromIl2Cpp]
        Slider MakeSlider(Slider source, Transform parent, Vector2 position, Action<float> changed)
        {
            return MakeFallbackSlider(parent, position, changed);
        }

        [HideFromIl2Cpp]
        Slider MakeFallbackSlider(Transform parent, Vector2 position, Action<float> changed)
        {
            var obj = new GameObject("SizeSlider");
            Rect(obj, parent, new Vector2(620, 50), position);
            var hit = obj.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0.001f);
            hit.canvasRenderer.cullTransparentMesh = false;
            var track = new GameObject("Track");
            Rect(track, obj.transform, new Vector2(620, 22), Vector2.zero);
            var trackImage = AddGraphic(track);

            trackImage.color = new Color(0.18f, 0.33f, 0.39f, 1f);
            var fill = new GameObject("Fill");
            var fillRect = Rect(fill, track.transform, Vector2.zero, Vector2.zero);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            var fillImage = AddGraphic(fill);

            fillImage.color = new Color(0.42f, 0.72f, 0.80f, 1f);
            var handle = new GameObject("Handle");
            var handleRect = Rect(handle, track.transform, new Vector2(20, 30), Vector2.zero);
            Image handleImage = null;
            for (int i = 0; i < 2; i++)
            {
                var slash = new GameObject("Slash");
                var slashRect = Rect(slash, handle.transform, new Vector2(3, 29), new Vector2(i * 7 - 3, 0));
                slashRect.localRotation = Quaternion.Euler(0, 0, -42);
                var slashImage = slash.AddComponent<Image>();
                slashImage.color = new Color(0.55f, 0.85f, 0.90f, 1f);
                slashImage.raycastTarget = false;
                if (i == 0) handleImage = slashImage;
            }
            var slider = obj.AddComponent<Slider>();
            slider.targetGraphic = handleImage;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.minValue = 100;
            slider.maxValue = 135;
            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener((UnityAction<float>)(value => changed(value)));
            return slider;
        }
        static MenuGraphic AddGraphic(GameObject obj)
        {
            if (obj.GetComponent<CanvasRenderer>() == null) obj.AddComponent<CanvasRenderer>();
            return obj.AddComponent<MenuGraphic>();
        }

        Text Label(Transform parent, string value, Vector2 position, Vector2 size, float fontSize)
        {
            var obj = new GameObject("ModLabel");
            Rect(obj, parent, size, position);
            var label = obj.AddComponent<Text>();
            label.font = _labelFont;
            label.text = value;
            label.fontSize = (int)fontSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.74f, 0.96f, 0.89f, 1);
            label.raycastTarget = false;
            label.maskable = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.canvasRenderer.SetAlpha(1f);
            return label;
        }
        [HideFromIl2Cpp]
        GameObject Button(Transform parent, string value, Vector2 position, Vector2 size, Action clicked)
        {
            var obj = new GameObject(value.Length == 0 ? "EnableFix" : value);
            Rect(obj, parent, size, position);
            // Use a standard Image for GraphicRaycaster registration. The custom
            // graphic is visual-only; it must not be the button's mouse target.
            var hit = obj.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0.001f);
            hit.canvasRenderer.cullTransparentMesh = false;
            hit.raycastTarget = true;
            var artwork = new GameObject("Artwork");
            Rect(artwork, obj.transform, size, Vector2.zero);
            var image = AddGraphic(artwork);
            image.raycastTarget = false;
            image.Bottom = 0.22f;
            image.Top = 0.78f;
            image.color = new Color(0.12f, 0.34f, 0.32f, 1);
            var button = obj.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hit;
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
