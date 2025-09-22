using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneFingerLullaby.UI
{
    /// <summary>
    /// Programmatically builds the main menu hierarchy so that the scene can be
    /// generated in environments without the Unity editor.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class MenuBuilder : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string campaignSceneName = "Game_Scene";
        [SerializeField] private string endlessSceneName = "Endless_Scene";

        [Header("Visuals")]
        [SerializeField] private Color backgroundColor = new Color(0.101f, 0.110f, 0.161f, 1f);
        [SerializeField] private Color panelColor = new Color(0.125f, 0.145f, 0.204f, 0.92f);
        [SerializeField] private Vector2 referenceResolution = new Vector2(1280f, 720f);

        private MenuManager menuManager;

        private void Awake()
        {
            menuManager = GetComponent<MenuManager>();
            if (menuManager == null)
            {
                Debug.LogError("MenuBuilder requires a MenuManager component on the same GameObject.", this);
                return;
            }

            var canvas = BuildCanvas();
            BuildEventSystem();

            CreateBackground(canvas.transform);
            CreateTitle(canvas.transform);

            var buttonPlay = CreateMenuButton(canvas.transform, "Button_Play", new Vector2(0, 140), "PLAY");
            buttonPlay.onClick.AddListener(() => menuManager.PlayGame(campaignSceneName));

            var buttonEndless = CreateMenuButton(canvas.transform, "Button_Endless", new Vector2(0, 70), "ENDLESS");
            buttonEndless.onClick.AddListener(() => menuManager.PlayGame(endlessSceneName));

            var buttonSettings = CreateMenuButton(canvas.transform, "Button_Settings", new Vector2(0, 0), "SETTINGS");
            buttonSettings.onClick.AddListener(menuManager.OpenSettings);

            var buttonCredits = CreateMenuButton(canvas.transform, "Button_Credits", new Vector2(0, -70), "CREDITS");
            buttonCredits.onClick.AddListener(menuManager.OpenCredits);

            var buttonQuit = CreateMenuButton(canvas.transform, "Button_Quit", new Vector2(0, -140), "QUIT");
            buttonQuit.onClick.AddListener(menuManager.QuitGame);

            var settingsPanel = BuildSettingsPanel(canvas.transform);
            var creditsPanel = BuildCreditsPanel(canvas.transform);
            var loadingPanel = BuildLoadingPanel(canvas.transform);

            menuManager.ConfigureUI(
                settingsPanel.gameObject,
                creditsPanel.gameObject,
                loadingPanel.gameObject,
                loadingPanel.loadingBar,
                loadingPanel.loadingText,
                settingsPanel.masterVolume,
                settingsPanel.sensitivity,
                settingsPanel.renderScaleDropdown,
                settingsPanel.pixelPerfectToggle,
                buttonQuit.gameObject);

            settingsPanel.closeButton.onClick.AddListener(menuManager.CloseSettings);
            creditsPanel.closeButton.onClick.AddListener(menuManager.CloseCredits);
        }

        private Canvas BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var rect = canvasGo.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private void BuildEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemGo.transform.SetParent(transform, false);
        }

        private void CreateBackground(Transform parent)
        {
            var image = CreateImage("Panel_Background", parent, backgroundColor, Vector2.zero, Vector2.zero);
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.raycastTarget = false;
        }

        private void CreateTitle(Transform parent)
        {
            var title = CreateText("Title", parent, new Vector2(0f, -60f), new Vector2(600f, 120f), "DOG DISTRACTION", 72);
            var rect = title.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
        }

        private Button CreateMenuButton(Transform parent, string name, Vector2 anchoredPosition, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(320f, 60f);
            rect.anchoredPosition = anchoredPosition;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);

            var image = go.GetComponent<Image>();
            image.sprite = SpriteFromColor(Color.white);
            image.type = Image.Type.Sliced;
            image.color = new Color(0.25f, 0.44f, 0.64f, 1f);

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.96f, 0.99f, 1f);
            colors.pressedColor = new Color(0.78f, 0.84f, 0.88f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            var text = CreateText("Label", go.transform, Vector2.zero, Vector2.zero, label, 32);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 12f);
            textRect.offsetMax = new Vector2(-24f, -12f);
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.UpperCase | FontStyles.Bold;

            return button;
        }

        private SettingsPanel BuildSettingsPanel(Transform parent)
        {
            var panelGo = new GameObject("Panel_Settings", typeof(RectTransform), typeof(Image));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(640f, 480f);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var image = panelGo.GetComponent<Image>();
            image.color = panelColor;

            var title = CreateText("Header", panelGo.transform, new Vector2(0f, -32f), new Vector2(560f, 80f), "SETTINGS", 48);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            title.alignment = TextAlignmentOptions.Center;

            var masterSlider = CreateSlider(panelGo.transform, "Slider_MasterVolume", new Vector2(0f, 120f));
            masterSlider.minValue = 0f;
            masterSlider.maxValue = 1f;
            masterSlider.value = 0.8f;

            var masterLabel = CreateText("Label_Master", panelGo.transform, new Vector2(-200f, 120f), new Vector2(220f, 40f), "Master Volume", 28);
            masterLabel.alignment = TextAlignmentOptions.MidlineRight;

            var sensitivitySlider = CreateSlider(panelGo.transform, "Slider_Sensitivity", new Vector2(0f, 40f));
            sensitivitySlider.minValue = 0.5f;
            sensitivitySlider.maxValue = 2f;
            sensitivitySlider.value = 1f;

            var sensitivityLabel = CreateText("Label_Sensitivity", panelGo.transform, new Vector2(-200f, 40f), new Vector2(220f, 40f), "Sensitivity", 28);
            sensitivityLabel.alignment = TextAlignmentOptions.MidlineRight;

            var dropdown = CreateDropdown(panelGo.transform, "Dropdown_RenderScale", new Vector2(0f, -60f));
            dropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("100%"),
                new TMP_Dropdown.OptionData("150%"),
                new TMP_Dropdown.OptionData("200%")
            };
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            var renderScaleLabel = CreateText("Label_RenderScale", panelGo.transform, new Vector2(-220f, -60f), new Vector2(240f, 40f), "Render Scale", 28);
            renderScaleLabel.alignment = TextAlignmentOptions.MidlineRight;

            var toggle = CreateToggle(panelGo.transform, "Toggle_PixelPerfect", new Vector2(-160f, -160f));
            toggle.isOn = true;

            var toggleLabel = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (toggleLabel != null)
            {
                toggleLabel.text = "Pixel Perfect";
                toggleLabel.fontSize = 28f;
                toggleLabel.alignment = TextAlignmentOptions.MidlineLeft;
                var toggleRect = toggleLabel.rectTransform;
                toggleRect.offsetMin = new Vector2(30f, -20f);
                toggleRect.offsetMax = new Vector2(0f, 20f);
            }

            var closeButton = CreateMenuButton(panelGo.transform, "Button_CloseSettings", new Vector2(0f, -200f), "CLOSE");
            closeButton.onClick.AddListener(() => panelGo.SetActive(false));

            panelGo.SetActive(false);

            return new SettingsPanel
            {
                gameObject = panelGo,
                masterVolume = masterSlider,
                sensitivity = sensitivitySlider,
                renderScaleDropdown = dropdown,
                pixelPerfectToggle = toggle,
                closeButton = closeButton
            };
        }

        private CreditsPanel BuildCreditsPanel(Transform parent)
        {
            var panelGo = new GameObject("Panel_Credits", typeof(RectTransform), typeof(Image));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(640f, 480f);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var image = panelGo.GetComponent<Image>();
            image.color = panelColor;

            var title = CreateText("Header", panelGo.transform, new Vector2(0f, -32f), new Vector2(560f, 80f), "CREDITS", 48);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            title.alignment = TextAlignmentOptions.Center;

            var creditsText = CreateText(
                "CreditsText",
                panelGo.transform,
                new Vector2(0f, -40f),
                new Vector2(560f, 260f),
                "Design & Code: OpenAI Assistant\nMusic: Your Name Here\nSpecial Thanks: One Finger Lullaby Team",
                28);
            var creditsRect = creditsText.rectTransform;
            creditsRect.anchorMin = new Vector2(0.5f, 1f);
            creditsRect.anchorMax = new Vector2(0.5f, 1f);
            creditsRect.pivot = new Vector2(0.5f, 1f);
            creditsText.alignment = TextAlignmentOptions.Top;
            creditsText.enableWordWrapping = true;

            var closeButton = CreateMenuButton(panelGo.transform, "Button_CloseCredits", new Vector2(0f, -200f), "CLOSE");
            closeButton.onClick.AddListener(() => panelGo.SetActive(false));

            panelGo.SetActive(false);

            return new CreditsPanel
            {
                gameObject = panelGo,
                closeButton = closeButton
            };
        }

        private LoadingPanel BuildLoadingPanel(Transform parent)
        {
            var panelGo = new GameObject("Panel_Loading", typeof(RectTransform), typeof(Image));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(480f, 200f);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var image = panelGo.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.6f);

            var text = CreateText("LoadingText", panelGo.transform, new Vector2(0f, 40f), new Vector2(440f, 60f), "Loading...", 32);
            text.alignment = TextAlignmentOptions.Center;

            var bar = CreateImage("LoadingBar", panelGo.transform, new Color(0.26f, 0.54f, 0.69f, 1f), new Vector2(0f, -40f), new Vector2(420f, 30f));
            var barRect = bar.rectTransform;
            barRect.anchorMin = barRect.anchorMax = new Vector2(0.5f, 0.5f);
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.fillAmount = 0f;

            panelGo.SetActive(false);

            return new LoadingPanel
            {
                gameObject = panelGo,
                loadingText = text,
                loadingBar = bar
            };
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string text, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = new Color(0.96f, 0.96f, 0.96f, 1f);
            tmp.enableWordWrapping = false;
            tmp.alignment = TextAlignmentOptions.Left;
            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            return tmp;
        }

        private Image CreateImage(string name, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);

            var image = go.GetComponent<Image>();
            image.sprite = SpriteFromColor(color);
            image.color = Color.white;
            image.type = Image.Type.Sliced;

            return image;
        }

        private Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition)
        {
            var sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
            var rect = sliderGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(360f, 30f);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;

            var slider = sliderGo.GetComponent<Slider>();
            slider.transition = Selectable.Transition.ColorTint;

            var background = CreateImage("Background", sliderGo.transform, new Color(0.07f, 0.07f, 0.09f, 1f), Vector2.zero, Vector2.zero);
            var backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.type = Image.Type.Sliced;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.SetParent(sliderGo.transform, false);
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(12f, 0f);
            fillAreaRect.offsetMax = new Vector2(-28f, 0f);

            var fill = CreateImage("Fill", fillArea.transform, new Color(0.26f, 0.54f, 0.69f, 1f), Vector2.zero, Vector2.zero);
            var fillRect = fill.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.SetParent(sliderGo.transform, false);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(12f, 0f);
            handleAreaRect.offsetMax = new Vector2(-12f, 0f);

            var handle = CreateImage("Handle", handleArea.transform, new Color(0.92f, 0.93f, 0.96f, 1f), Vector2.zero, new Vector2(24f, 24f));
            var handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;

            return slider;
        }

        private TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 anchoredPosition)
        {
            var resources = GetDefaultResources();
            var dropdownGo = TMP_DefaultControls.CreateDropdown(resources);
            dropdownGo.name = name;
            var rect = dropdownGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(260f, 40f);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;

            var dropdown = dropdownGo.GetComponent<TMP_Dropdown>();
            dropdown.captionText.color = new Color(0.96f, 0.96f, 0.96f, 1f);
            dropdown.itemText.color = new Color(0.96f, 0.96f, 0.96f, 1f);

            return dropdown;
        }

        private Toggle CreateToggle(Transform parent, string name, Vector2 anchoredPosition)
        {
            var resources = new DefaultControls.Resources(); // ← NOTE: UnityEngine.UI version
            var toggleGo = DefaultControls.CreateToggle(resources);

            toggleGo.name = name;
            var rect = toggleGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(200f, 40f);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;

            var toggle = toggleGo.GetComponent<Toggle>();
            var graphic = toggle.graphic as Graphic;
            if (graphic != null)
                graphic.color = new Color(0.26f, 0.54f, 0.69f, 1f);

            // replace label text with TextMeshPro if needed
            var oldText = toggleGo.GetComponentInChildren<Text>();
            if (oldText != null)
            {
                GameObject.DestroyImmediate(oldText.gameObject);
                var label = new GameObject("Label", typeof(RectTransform));
                label.transform.SetParent(toggleGo.transform, false);
                var tmp = label.AddComponent<TMPro.TextMeshProUGUI>();
                tmp.text = name;
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
            }

            return toggle;
        }

        private TMP_DefaultControls.Resources GetDefaultResources()
        {
            return new TMP_DefaultControls.Resources
            {
                standard = SpriteFromColor(new Color(0.18f, 0.20f, 0.28f, 1f)),
                background = SpriteFromColor(new Color(0.14f, 0.15f, 0.22f, 1f)),
                inputField = SpriteFromColor(new Color(0.12f, 0.13f, 0.18f, 1f)),
                knob = SpriteFromColor(new Color(0.92f, 0.93f, 0.96f, 1f)),
                checkmark = SpriteFromColor(new Color(0.26f, 0.54f, 0.69f, 1f)),
                dropdown = SpriteFromColor(new Color(0.18f, 0.20f, 0.28f, 1f)),
                mask = SpriteFromColor(Color.white)
            };
        }

        private Sprite SpriteFromColor(Color color)
        {
            var texture = new Texture2D(2, 2)
            {
                name = $"Color_{color.r:F2}_{color.g:F2}_{color.b:F2}",
                hideFlags = HideFlags.HideAndDontSave
            };
            var fillColor = new Color(color.r, color.g, color.b, 1f);
            var pixels = new Color[4] { fillColor, fillColor, fillColor, fillColor };
            texture.SetPixels(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private struct SettingsPanel
        {
            public GameObject gameObject;
            public Slider masterVolume;
            public Slider sensitivity;
            public TMP_Dropdown renderScaleDropdown;
            public Toggle pixelPerfectToggle;
            public Button closeButton;
        }

        private struct CreditsPanel
        {
            public GameObject gameObject;
            public Button closeButton;
        }

        private struct LoadingPanel
        {
            public GameObject gameObject;
            public TMP_Text loadingText;
            public Image loadingBar;
        }
    }
}
