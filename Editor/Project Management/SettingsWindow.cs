using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using CodeDestroyer.Editor.EditorVisual.UIElements;

namespace CodeDestroyer.Editor.EditorVisual
{
    public sealed class EditorVisualSettingsWindow : EditorWindow
    {
#if true // Folder Icons Marker
        private const string folderIconsName = "Folder Icons";
#endif

        private static readonly Vector2 windowSize = new Vector2(1020f, 610f);

        private List<TreeViewItemData<string>> projectSettingsList = new List<TreeViewItemData<string>>();
        private Dictionary<string, VisualElement> rootDict = new Dictionary<string, VisualElement>();

        [MenuItem("Tools/Code Destroyer/Editor Visual")]
        private static void ShowWindow()
        {
            EditorVisualSettingsWindow settingsWindow = GetWindow<EditorVisualSettingsWindow>();
            settingsWindow.titleContent.text = "Editor Visual Settings";
            settingsWindow.titleContent.image = EditorGUIUtility.FindTexture("SettingsIcon");
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Vector2 center = main.center;

            settingsWindow.position = new Rect(
                center.x - windowSize.x * 0.5f,
                center.y - windowSize.y * 0.5f,
                windowSize.x,
                windowSize.y
            );
        }
        private void OnEnable()
        {
#if true // Folder Icons Marker
            Undo.undoRedoPerformed += FolderIconsSettings.RefreshIconSetListViewOnUndo;
#endif
        }
        private void OnDisable()
        {
#if true // Folder Icons Marker
            Undo.undoRedoPerformed -= FolderIconsSettings.RefreshIconSetListViewOnUndo;
#endif
        }

        public void CreateGUI()
        {

#if true // Folder Icons Marker
            TreeViewItemData<string> folderIconsSetting = new TreeViewItemData<string>(0, folderIconsName);
            rootDict.Add(folderIconsName, FolderIconsSettings.FolderIconsSettingsVisualElement());
            projectSettingsList.Add(folderIconsSetting);
#endif

            SettingsPanel settingsWindow = new SettingsPanel(ref projectSettingsList, ref rootDict);

            rootVisualElement.Add(settingsWindow);
        }
    }
}