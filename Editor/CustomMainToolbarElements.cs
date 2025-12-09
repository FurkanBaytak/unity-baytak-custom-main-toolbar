using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Baytak.CustomToolbar
{
    internal static class CustomMainToolbarElements
    {
        private const string SceneDropdownPath = "Baytak Custom Toolbar/Scene Selector";
        private const string TimeScaleSliderPath = "Baytak Custom Toolbar/Time Scale";
        private const string CompileButtonPath = "Baytak Custom Toolbar/Compile Scripts";

        private static string[] _scenePaths = System.Array.Empty<string>();
        private static string _activeSceneName = "Untitled";

        private const float k_MinTimeScale = 0f;
        private const float k_MaxTimeScale = 4f;

        static CustomMainToolbarElements()
        {
            RefreshSceneList();

            EditorApplication.projectChanged += RefreshSceneList;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        [MainToolbarElement(
            SceneDropdownPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 0
        )]
        public static MainToolbarElement SceneSelectorDropdown()
        {
            UpdateActiveSceneName();

            var icon = EditorGUIUtility.IconContent("UnityLogo").image as Texture2D;
            var content = new MainToolbarContent(
                _activeSceneName,
                icon,
                "Select and open a scene"
            );

            return new MainToolbarDropdown(content, ShowSceneDropdownMenu);
        }

        private static void ShowSceneDropdownMenu(Rect dropDownRect)
        {
            var menu = new GenericMenu();

            if (_scenePaths == null || _scenePaths.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No Scenes in Project"));
                menu.DropDown(dropDownRect);
                return;
            }

            var rootScenes = new List<string>();
            var scenesByFolder = new Dictionary<string, List<string>>();
            var otherScenes = new List<string>();

            const string scenesRootPrefix = "Assets/Scenes/";

            foreach (var rawPath in _scenePaths)
            {
                if (string.IsNullOrEmpty(rawPath))
                    continue;

                var normPath = rawPath.Replace('\\', '/');

                if (normPath.StartsWith(scenesRootPrefix))
                {
                    var relative = normPath.Substring(scenesRootPrefix.Length);

                    if (!relative.Contains("/"))
                    {
                        rootScenes.Add(normPath);
                    }
                    else
                    {
                        var lastSlash = relative.LastIndexOf('/');
                        var folderKey = relative.Substring(0, lastSlash);

                        if (!scenesByFolder.TryGetValue(folderKey, out var list))
                        {
                            list = new List<string>();
                            scenesByFolder[folderKey] = list;
                        }

                        list.Add(normPath);
                    }
                }
                else
                {
                    otherScenes.Add(normPath);
                }
            }

            rootScenes.Sort((a, b) =>
                string.Compare(Path.GetFileNameWithoutExtension(a),
                               Path.GetFileNameWithoutExtension(b),
                               System.StringComparison.OrdinalIgnoreCase));

            var folderKeys = new List<string>(scenesByFolder.Keys);
            folderKeys.Sort(System.StringComparer.OrdinalIgnoreCase);

            otherScenes.Sort((a, b) =>
                string.Compare(Path.GetFileNameWithoutExtension(a),
                               Path.GetFileNameWithoutExtension(b),
                               System.StringComparison.OrdinalIgnoreCase));

            bool hasAny = false;

            if (rootScenes.Count > 0)
            {
                foreach (var scenePath in rootScenes)
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    bool isActive = sceneName == _activeSceneName;

                    menu.AddItem(new GUIContent(sceneName), isActive, () =>
                    {
                        SwitchScene(scenePath);
                    });

                    hasAny = true;
                }
            }

            if (folderKeys.Count > 0)
            {
                if (hasAny)
                    menu.AddSeparator("");

                foreach (var folderKey in folderKeys)
                {
                    if (!scenesByFolder.TryGetValue(folderKey, out var list) || list == null)
                        continue;

                    list.Sort((a, b) =>
                        string.Compare(Path.GetFileNameWithoutExtension(a),
                                       Path.GetFileNameWithoutExtension(b),
                                       System.StringComparison.OrdinalIgnoreCase));

                    foreach (var scenePath in list)
                    {
                        var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                        bool isActive = sceneName == _activeSceneName;

                        var label = $"{folderKey}/{sceneName}";
                        menu.AddItem(new GUIContent(label), isActive, () =>
                        {
                            SwitchScene(scenePath);
                        });

                        hasAny = true;
                    }
                }
            }

            if (otherScenes.Count > 0)
            {
                if (hasAny)
                    menu.AddSeparator("");

                foreach (var scenePath in otherScenes)
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    bool isActive = sceneName == _activeSceneName;

                    var label = $"Other/{sceneName}";
                    menu.AddItem(new GUIContent(label), isActive, () =>
                    {
                        SwitchScene(scenePath);
                    });

                    hasAny = true;
                }
            }

            if (!hasAny)
            {
                menu.AddDisabledItem(new GUIContent("No Scenes Found Under Assets"));
            }

            menu.DropDown(dropDownRect);
        }

        private static void SwitchScene(string scenePath)
        {
            if (Application.isPlaying)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    Debug.Log($"[CustomToolbar] Loading scene in Play Mode: {sceneName}");
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Debug.LogError(
                        $"[CustomToolbar] Scene '{sceneName}' is not in Build Settings.");
                }
            }
            else
            {
                if (File.Exists(scenePath))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        Debug.Log($"[CustomToolbar] Opening scene in Edit Mode: {scenePath}");
                        EditorSceneManager.OpenScene(scenePath);
                    }
                }
                else
                {
                    Debug.LogError(
                        $"[CustomToolbar] Scene at path '{scenePath}' does not exist.");
                }
            }
        }

        private static void RefreshSceneList()
        {
            _scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
        }

        private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            _activeSceneName = string.IsNullOrEmpty(newScene.name) ? "Untitled" : newScene.name;
            MainToolbar.Refresh(SceneDropdownPath);
        }

        private static void UpdateActiveSceneName()
        {
            if (Application.isPlaying)
            {
                _activeSceneName = SceneManager.GetActiveScene().name;
            }
            else
            {
                var s = EditorSceneManager.GetActiveScene();
                _activeSceneName = string.IsNullOrEmpty(s.name) ? "Untitled" : s.name;
            }
        }

        [MainToolbarElement(
            TimeScaleSliderPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 1
        )]
        public static MainToolbarElement TimeScaleSlider()
        {
            var content = new MainToolbarContent(
                "Time",
                "Time.timeScale (0 = paused, 1 = normal)"
            );

            return new MainToolbarSlider(
                content,
                Time.timeScale,
                k_MinTimeScale,
                k_MaxTimeScale,
                OnTimeScaleChanged,
                rounded: false
            );
        }

        private static void OnTimeScaleChanged(float newValue)
        {
            Time.timeScale = Mathf.Clamp(newValue, k_MinTimeScale, k_MaxTimeScale);
        }

        [MainToolbarElement(
            CompileButtonPath,
            defaultDockPosition = MainToolbarDockPosition.Right,
            defaultDockIndex = 0
        )]
        public static MainToolbarElement CompileScriptsButton()
        {
            var icon = EditorGUIUtility.IconContent("cs Script Icon")?.image as Texture2D;
            var content = icon != null
                ? new MainToolbarContent(icon, "Force script compilation")
                : new MainToolbarContent("Compile", "Force script compilation");

            return new MainToolbarButton(content, RequestCompilation);
        }

        private static void RequestCompilation()
        {
            CompilationPipeline.RequestScriptCompilation();
            Debug.Log("[CustomToolbar] Requested script compilation.");
        }
    }
}