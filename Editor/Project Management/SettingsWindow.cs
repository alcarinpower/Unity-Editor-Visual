using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using CodeDestroyer._Editor.EditorVisual.UIElements;

namespace CodeDestroyer._Editor.EditorVisual
{
    public sealed class EditorVisualSettingsWindow : EditorWindow
    {
#if true // Folder Icons Marker
        private const string folderIconsName = "Folder Icons";
#endif

        private static readonly Vector2 minWindowSize = new Vector2(310f, 200f);

        private List<TreeViewItemData<string>> projectSettingsList = new List<TreeViewItemData<string>>();
        private Dictionary<string, VisualElement> rootDict = new Dictionary<string, VisualElement>();

        [MenuItem("Tools/Code Destroyer/Editor Visual")]
        private static void ShowWindow()
        {
            EditorVisualSettingsWindow settingsWindow = GetWindow<EditorVisualSettingsWindow>();
            settingsWindow.titleContent.text = "Editor Visual Settings";
            settingsWindow.titleContent.image = EditorGUIUtility.FindTexture("SettingsIcon");
            settingsWindow.minSize = minWindowSize;
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