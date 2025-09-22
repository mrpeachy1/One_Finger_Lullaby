using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneFingerLullaby.UI
{
    /// <summary>
    /// Controls high level behaviour for the main menu including button events,
    /// settings defaults, and asynchronous scene loading feedback.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject panelSettings;
        [SerializeField] private GameObject panelCredits;
        [SerializeField] private GameObject panelLoading;

        [Header("Loading UI")]
        [SerializeField] private Image loadingBar;
        [SerializeField] private TMP_Text loadingText;

        [Header("Settings UI")]
        [SerializeField] private Slider sliderMasterVolume;
        [SerializeField] private Slider sliderSensitivity;
        [SerializeField] private TMP_Dropdown dropdownRenderScale;
        [SerializeField] private Toggle togglePixelPerfect;

        [Header("Platform UI")]
        [SerializeField] private GameObject quitButton;

        private const float MasterVolumeDefault = 0.8f;
        private const float SensitivityDefault = 1.0f;

        private Coroutine loadRoutine;

        private void Awake()
        {
            HandlePlatformSpecificUI();
        }

        /// <summary>
        /// Begin loading a scene asynchronously. Shows loading progress while the scene loads.
        /// </summary>
        /// <param name="sceneName">The scene to load. Must be added to build settings.</param>
        public void PlayGame(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("MenuManager.PlayGame called with an invalid scene name.");
                return;
            }

            if (loadRoutine != null)
            {
                StopCoroutine(loadRoutine);
            }

            loadRoutine = StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void OpenSettings()
        {
            CloseAllPanels();
            Show(panelSettings);
        }

        public void CloseSettings()
        {
            Hide(panelSettings);
        }

        public void OpenCredits()
        {
            CloseAllPanels();
            Show(panelCredits);
        }

        public void CloseCredits()
        {
            Hide(panelCredits);
        }

        public void QuitGame()
        {
            Debug.Log("Quit requested from main menu.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            Show(panelLoading);
            UpdateLoadingUI(0f);

            AsyncOperation operation;

            try
            {
                operation = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Failed to load scene '{sceneName}': {exception}");
                Hide(panelLoading);
                yield break;
            }

            if (operation == null)
            {
                Debug.LogError($"Scene '{sceneName}' could not be loaded. Ensure it is added to Build Settings.");
                Hide(panelLoading);
                yield break;
            }

            while (!operation.isDone)
            {
                var progress = Mathf.Clamp01(operation.progress / 0.9f);
                UpdateLoadingUI(progress);
                yield return null;
            }

            UpdateLoadingUI(1f);
        }

        private void InitializeSettingsDefaults()
        {
            if (sliderMasterVolume != null)
            {
                sliderMasterVolume.minValue = 0f;
                sliderMasterVolume.maxValue = 1f;
                sliderMasterVolume.value = MasterVolumeDefault;
            }

            if (sliderSensitivity != null)
            {
                sliderSensitivity.minValue = 0.5f;
                sliderSensitivity.maxValue = 2f;
                sliderSensitivity.value = SensitivityDefault;
            }

            if (dropdownRenderScale != null)
            {
                var options = new List<string> { "100%", "150%", "200%" };
                dropdownRenderScale.ClearOptions();
                dropdownRenderScale.AddOptions(options);
                dropdownRenderScale.value = 0;
                dropdownRenderScale.RefreshShownValue();
            }

            if (togglePixelPerfect != null)
            {
                togglePixelPerfect.isOn = true;
            }
        }

        public void ConfigureUI(
            GameObject settingsPanel,
            GameObject creditsPanel,
            GameObject loadingPanel,
            Image loadingBarImage,
            TMP_Text loadingTextLabel,
            Slider masterSlider,
            Slider sensitivitySlider,
            TMP_Dropdown renderScaleDropdown,
            Toggle pixelPerfectToggle,
            GameObject quitButtonObject)
        {
            panelSettings = settingsPanel;
            panelCredits = creditsPanel;
            panelLoading = loadingPanel;
            loadingBar = loadingBarImage;
            loadingText = loadingTextLabel;
            sliderMasterVolume = masterSlider;
            sliderSensitivity = sensitivitySlider;
            dropdownRenderScale = renderScaleDropdown;
            togglePixelPerfect = pixelPerfectToggle;
            quitButton = quitButtonObject;

            HideImmediate(panelSettings);
            HideImmediate(panelCredits);
            HideImmediate(panelLoading);

            InitializeSettingsDefaults();
            HandlePlatformSpecificUI();
        }

        private void HandlePlatformSpecificUI()
        {
#if UNITY_IOS
            if (quitButton != null)
            {
                quitButton.SetActive(false);
            }
#else
            if (quitButton != null)
            {
                quitButton.SetActive(true);
            }
#endif
        }

        private void UpdateLoadingUI(float progress)
        {
            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;
            }

            if (loadingText != null)
            {
                var percentage = Mathf.RoundToInt(progress * 100f);
                loadingText.text = progress >= 1f ? "Ready" : $"Loading... {percentage}%";
            }
        }

        private void CloseAllPanels()
        {
            Hide(panelSettings);
            Hide(panelCredits);
        }

        private static void Hide(GameObject target)
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        private static void HideImmediate(GameObject target)
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        private static void Show(GameObject target)
        {
            if (target != null)
            {
                target.SetActive(true);
            }
        }
    }
}