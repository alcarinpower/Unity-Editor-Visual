using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using CodeDestroyer.Editor.EditorVisual.UIElements;
using System.Text.RegularExpressions;

namespace CodeDestroyer.Editor.EditorVisual
{
    public sealed class FolderIconsSettings
    {
        private static ListView iconSetListView;
        private static PopupField<string> popupField;
        private static bool isIconSetADefaultIconSet;
        private static readonly Color lineColor = new Color(144, 144, 144, 0.2f);

        private static float globalMargin = 20f;


        private static string settingsWindowPath = GlobalVariables.ProjectManagerPath + "/SettingsWindow.cs";

        public static VisualElement FolderIconsSettingsVisualElement()
        {
            UtilityFunctions.CheckAndCreateIconFolders();
            VisualElement rootVisualElement = new VisualElement();


            VisualElement headerAndDeleteEverythingContainer = new VisualElement();
            headerAndDeleteEverythingContainer.style.flexDirection = FlexDirection.Row;
            headerAndDeleteEverythingContainer.style.justifyContent = Justify.SpaceBetween;



            Label rightPaneHeaderLabel = new Label("Folder Icons");
            rightPaneHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            rightPaneHeaderLabel.style.fontSize = 20;
            rightPaneHeaderLabel.style.marginLeft = globalMargin;


            Button deleteFolderIcons = new Button();
            deleteFolderIcons.text = "Delete Folder Icons";
            deleteFolderIcons.style.marginRight = globalMargin;

            deleteFolderIcons.clicked += () => {

                if (EditorUtility.DisplayDialog("Delete Folder Icons", "This will delete folder icons completely", "ok", "cancel"))
                {
                    Directory.Delete(GlobalVariables.FolderIconsPath.Replace("/", "\\"), true);

                    FindAndChangeFolderIconsMarkersOnDelete();

                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                    AssetDatabase.Refresh();
                }

            };


            Line headerLine = new Line(1f, false, lineColor);
            headerLine.style.marginLeft = globalMargin;
            headerLine.style.marginRight = globalMargin;
            headerLine.style.marginBottom = 20;


            TextField newIconSetName = new TextField("Icon Set");
            newIconSetName.style.marginLeft = globalMargin;
            newIconSetName.style.flexGrow = 1;

            VisualElement addRemoveContainer = new VisualElement();
            addRemoveContainer.style.flexDirection = FlexDirection.Row;


            popupField = new PopupField<string>(IconManager.iconSetNames, IconManager.persistentFolderIconsData.currentIconSetIndex);
            popupField.style.flexGrow = 1;
            popupField.style.marginLeft = globalMargin;
            popupField.style.marginRight = globalMargin;


            iconSetListView = new ListView(IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData, 45);
            iconSetListView.showAddRemoveFooter = true;
            iconSetListView.showFoldoutHeader = true;
            iconSetListView.headerTitle = "Icon Set";
            iconSetListView.reorderable = true;
            iconSetListView.reorderMode = ListViewReorderMode.Simple;
            iconSetListView.style.marginLeft = globalMargin;
            iconSetListView.style.marginRight = globalMargin;

            iconSetListView.onAdd = baseListView =>
            {
                Undo.RecordObject(IconManager.persistentFolderIconsData, "Add New Icon Set");
                int itemsSourceCount = baseListView.itemsSource.Count;
                baseListView.itemsSource.Add(new IconSetData());
                baseListView.RefreshItem(baseListView.selectedIndex);
                baseListView.ScrollToItem(itemsSourceCount);


                iconSetListView.Unbind();
                iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                iconSetListView.bindItem = BindItemForIconSetListView;
                iconSetListView.Rebuild();
            };
            iconSetListView.onRemove = baseListView =>
            {
                if (baseListView.selectedItem == null)
                {
                    Undo.RecordObject(IconManager.persistentFolderIconsData, "Remove Icon Set");
                    IconSetData data = baseListView.itemsSource[baseListView.itemsSource.Count - 1] as IconSetData;
                    if (data != null)
                    {
                        int index = IconManager.persistentFolderIconsData.guidTextureList.FindIndex((x) => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x.guid)) == data.folderName);
                        if (index != -1) IconManager.persistentFolderIconsData.guidTextureList.RemoveAt(index);

                    }

                    baseListView.ScrollToItem(baseListView.itemsSource.Count - 1);
                    baseListView.itemsSource.RemoveAt(baseListView.itemsSource.Count - 1);
                    iconSetListView.Unbind();
                    iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                    iconSetListView.bindItem = BindItemForIconSetListView;
                    iconSetListView.Rebuild();
                }
                else
                {
                    if (baseListView.selectedIndex >= baseListView.itemsSource.Count) return;
                    Undo.RecordObject(IconManager.persistentFolderIconsData, "Remove Icon Set");

                    int index = IconManager.persistentFolderIconsData.guidTextureList.FindIndex((x) => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x.guid)) == (baseListView.selectedItem as IconSetData).folderName);
                    if (index != -1) IconManager.persistentFolderIconsData.guidTextureList.RemoveAt(index);

                    baseListView.ScrollToItem(baseListView.selectedIndex);
                    baseListView.itemsSource.RemoveAt(baseListView.selectedIndex);

                    iconSetListView.Unbind();
                    iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                    iconSetListView.bindItem = BindItemForIconSetListView;
                    iconSetListView.Rebuild();
                }
            };


            iconSetListView.makeItem = MakeItemForIconSetListView;
            iconSetListView.bindItem = BindItemForIconSetListView;



            isIconSetADefaultIconSet = IsIconSetIsDefault();

            iconSetListView.SetEnabled(isIconSetADefaultIconSet);

            Button addIconSetButton = new Button();
            addIconSetButton.clicked += () =>
            {
                int previousIconDataIndex = IconManager.persistentFolderIconsData.currentIconSetIndex;
                AddIconSet(newIconSetName.text);
                IconManager.persistentFolderIconsData.currentIconSetIndex = IconManager.persistentFolderIconsData.iconSetDataList.Count - 1;

                popupField.choices = IconManager.iconSetNames;

                popupField.value = IconManager.iconSetNames[^1];
                newIconSetName.value = "";

                UpdateAllFolderIcons(previousIconDataIndex, true);

                iconSetListView.Unbind();
                iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                iconSetListView.bindItem = BindItemForIconSetListView;
                iconSetListView.Rebuild();

            };
            addIconSetButton.text = "Add";

            Button deleteIconSetButton = new Button();
            deleteIconSetButton.style.marginRight = globalMargin;
            deleteIconSetButton.clicked += () =>
            {
                DeleteIconSet();

                popupField.choices = IconManager.iconSetNames;

                newIconSetName.value = IconManager.iconSetNames[^1];

                newIconSetName.value = "";
                popupField.value = IconManager.iconSetNames[^1];

                iconSetListView.Unbind();
                iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                iconSetListView.bindItem = BindItemForIconSetListView;
                iconSetListView.Rebuild();
            };
            deleteIconSetButton.text = "Delete";


            deleteIconSetButton.SetEnabled(isIconSetADefaultIconSet);
            popupField.RegisterValueChangedCallback<string>(evt =>
            {
                int previousIconSetIndex = IconManager.persistentFolderIconsData.currentIconSetIndex;

                IconManager.persistentFolderIconsData.currentIconSetIndex = popupField.index;

                UpdateAllFolderIcons(previousIconSetIndex, true);

                isIconSetADefaultIconSet = IsIconSetIsDefault();

                deleteIconSetButton.SetEnabled(isIconSetADefaultIconSet);
                iconSetListView.SetEnabled(isIconSetADefaultIconSet);

                iconSetListView.Unbind();
                iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                iconSetListView.bindItem = BindItemForIconSetListView;
                iconSetListView.Rebuild();
            }
            );



            Line iconsSetLine = new Line(1f, false, lineColor);
            iconsSetLine.style.marginLeft = globalMargin;
            iconsSetLine.style.marginRight = globalMargin;
            iconsSetLine.style.marginBottom = 20;
            iconsSetLine.style.marginTop = 40;

            Button loadIconsButton = new Button();
            loadIconsButton.clicked += LoadIcons;
            loadIconsButton.text = "Load Icons";
            loadIconsButton.style.flexGrow = 1;
            loadIconsButton.style.marginLeft = globalMargin;
            loadIconsButton.style.marginRight = globalMargin;

            Button saveIconsButton = new Button();
            saveIconsButton.style.marginLeft = globalMargin;
            saveIconsButton.style.marginRight = globalMargin;
            saveIconsButton.clicked += SaveIcons;
            saveIconsButton.text = "Save Icons";
            saveIconsButton.style.flexGrow = 1;

            Button resetEverythingButton = new Button();
            resetEverythingButton.style.marginLeft = globalMargin;
            resetEverythingButton.style.marginRight = globalMargin;
            resetEverythingButton.text = "Reset All Icons";
            resetEverythingButton.clicked += ResetAllIcons;


            VisualElement loadSaveRow = new VisualElement();
            loadSaveRow.style.flexDirection = FlexDirection.Row;
            loadSaveRow.style.justifyContent = Justify.Center;
            loadSaveRow.style.alignItems = Align.Center;


            loadSaveRow.Add(loadIconsButton);
            loadSaveRow.Add(saveIconsButton);

            addRemoveContainer.Add(newIconSetName);
            addRemoveContainer.Add(addIconSetButton);
            addRemoveContainer.Add(deleteIconSetButton);


            headerAndDeleteEverythingContainer.Add(rightPaneHeaderLabel);
            headerAndDeleteEverythingContainer.Add(deleteFolderIcons);

            rootVisualElement.Add(headerAndDeleteEverythingContainer);
            rootVisualElement.Add(headerLine);
            rootVisualElement.Add(addRemoveContainer);
            rootVisualElement.Add(popupField);
            rootVisualElement.Add(iconSetListView);
            rootVisualElement.Add(iconsSetLine);
            rootVisualElement.Add(loadSaveRow);
            rootVisualElement.Add(resetEverythingButton);

            return rootVisualElement;
        }
        private static VisualElement MakeItemForIconSetListView()
        {
            VisualElement element = new VisualElement();

            TextField textField = new TextField();
            textField.name = nameof(IconSetData.folderName);

            ObjectField customTexture = new ObjectField()
            {
                objectType = typeof(Texture2D)
            };
            customTexture.name = nameof(IconSetData.icon);


            element.Add(textField);
            element.Add(customTexture);

            return element;
        }

        private static void BindItemForIconSetListView(VisualElement element, int index)
        {
            TextField textField = element.Q<TextField>();
            ObjectField customTexture = element.Q<ObjectField>();


            if (textField != null && customTexture != null)
            {
                textField.label = "Folder Name";
                customTexture.label = "Folder Icon";

                textField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(IconManager.persistentFolderIconsData, "Change Folder Name");

                    IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData[index].folderName = evt.newValue;

                    if (!string.IsNullOrEmpty(textField.value) && !string.IsNullOrWhiteSpace(textField.value))
                    {
                        UpdateAllFolderIcons(-1, false);
                    }
                });


                customTexture.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(IconManager.persistentFolderIconsData, "Change Folder Icon");
                    IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData[index].icon = evt.newValue as Texture2D;

                    if (!string.IsNullOrEmpty(textField.value) && !string.IsNullOrWhiteSpace(textField.value))
                    {
                        UpdateAllFolderIcons(-1, false);
                    }
                });

                textField.value = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData[index].folderName;
                customTexture.value = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData[index].icon;
                textField.style.marginLeft = globalMargin;
                customTexture.style.marginLeft = globalMargin;
                textField.style.marginTop = 3f;
                customTexture.style.marginTop = 3f;

            }
        }
        public static void RefreshIconSetListViewOnUndo()
        {
            iconSetListView.Unbind();
            iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
            iconSetListView.Rebuild();
            iconSetListView.RefreshItems();
        }
        private static void AddIconSet(string newIconSetNameText)
        {
            if (!IconManager.iconSetNames.Contains(newIconSetNameText) && newIconSetNameText != null && newIconSetNameText.Trim() != "")
            {

                IconManager.iconSetNames.Add(newIconSetNameText);

                IconSetDataListWrapper iconSetData = new IconSetDataListWrapper();

                iconSetData.iconSetName = newIconSetNameText;
                iconSetData.iconSetData = new List<IconSetData>();

                IconManager.persistentFolderIconsData.iconSetDataList.Add(iconSetData);

                UpdateAllFolderIcons();

                if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);

                isIconSetADefaultIconSet = false;
            }
        }
        private static void DeleteIconSet()
        {
            isIconSetADefaultIconSet = IsIconSetIsDefault();
            if (isIconSetADefaultIconSet)
            {
                IconManager.iconSetNames.Remove(IconManager.iconSetNames[IconManager.persistentFolderIconsData.currentIconSetIndex]);

                IconManager.persistentFolderIconsData.iconSetDataList.RemoveAt(IconManager.persistentFolderIconsData.currentIconSetIndex);
                IconManager.persistentFolderIconsData.currentIconSetIndex = IconManager.iconSetNames.Count - 1;

                UpdateAllFolderIcons();
            }
            else
            {
                Debug.LogWarning("You can't delete default icon sets!");
            }
            isIconSetADefaultIconSet = IsIconSetIsDefault();
        }
        private static void LoadIcons()
        {
            string selectedFile = EditorUtility.OpenFilePanel("Select a .json file to load!", Application.dataPath, "json");

            if (selectedFile.Length > 0)
            {
                IconManager.LoadIcons(selectedFile);

                IconManager.iconSetNames = new List<string>(IconManager.persistentFolderIconsData.iconSetDataList.Count);
                for (int i = 0; i < IconManager.persistentFolderIconsData.iconSetDataList.Count; i++)
                {
                    IconManager.iconSetNames.Add(IconManager.persistentFolderIconsData.iconSetDataList[i].iconSetName);
                }

                popupField.choices = IconManager.iconSetNames;
                popupField.value = IconManager.iconSetNames[0];


                iconSetListView.Unbind();
                iconSetListView.itemsSource = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex].iconSetData;
                iconSetListView.bindItem = BindItemForIconSetListView;
                iconSetListView.Rebuild();
                AssetDatabase.Refresh();
            }
        }
        private static void SaveIcons()
        {
            string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", Application.dataPath, "Folder Icons Data.json", "json");

            if (selectedFile.Length > 0)
            {
                IconManager.SaveIcons(selectedFile);
            }
        }
        private static void ResetAllIcons()
        {
            bool warningReset = EditorUtility.DisplayDialog("WARNING!", "This will delete every icons and every icon sets!", "Continue!", "Cancel!");
            if (warningReset)
            {
                UtilityFunctions.CheckAndCreateIconFolders();

                try
                {
                    IconManager.persistentFolderIconsData.currentIconSetIndex = 0;
                    popupField.index = 0;


                    string[] foldersForDeletion = new string[] { ProjectConstants.emptyIconFolderPath, ProjectConstants.iconFolderPath,
                                                                 ProjectConstants.loadedIconSetPath, ProjectConstants.loadedIconsPath};

                    string[] assetPaths = AssetDatabase.FindAssets("t:Texture2D", foldersForDeletion);
                    string[] folderPaths = AssetDatabase.FindAssets("t:DefaultAsset", foldersForDeletion);


                    for (int i = 0; i < assetPaths.Length; i++)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assetPaths[i]));
                    }
                    for (int i = 0; i < folderPaths.Length; i++)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(folderPaths[i]));
                    }

                    int iconSetDataIndex = IconManager.persistentFolderIconsData.iconSetDataList.Count - 1;

                    List<string> iconSetList = IconManager.iconSetNames;
                    iconSetList.RemoveRange(1, iconSetDataIndex);
                    IconManager.iconSetNames = iconSetList;

                    IconManager.persistentFolderIconsData.iconSetDataList.RemoveRange(1, iconSetDataIndex);


                    isIconSetADefaultIconSet = IsIconSetIsDefault();

                    if (IconManager.persistentFolderIconsData != null) IconManager.persistentFolderIconsData.guidTextureList.Clear();
                    if (IconManager.persistentFolderIconsData != null) IconManager.persistentFolderIconsData.tempGuidTextureListForIconSets.Clear();


                    UpdateAllFolderIcons(-1, true);

                    Debug.Log("All Icons Has been reset!");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
        }


        #region General Region
        // Check if selected dropdown menu is first to disable editing
        private static bool IsIconSetIsDefault()
        {
            if (IconManager.persistentFolderIconsData.currentIconSetIndex == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        // When selecting a icon set update entire project folder icons according to selected icon set
        private static void UpdateAllFolderIcons(int previousIconSetIndex = -1, bool isCalledFromPopupSelection = false)
        {
            if (IconManager.persistentFolderIconsData.iconSetDataList.Count == 0) return;


            if (IconManager.persistentFolderIconsData.currentIconSetIndex == 0)
            {
                if (isCalledFromPopupSelection)
                {
                    IconManager.persistentFolderIconsData.guidTextureList.Clear();


                    for (int i = 0; i < IconManager.persistentFolderIconsData.tempGuidTextureListForIconSets.Count; i++)
                    {
                        GUIDTextureData GUIDTextureData = IconManager.persistentFolderIconsData.tempGuidTextureListForIconSets[i];
                        IconManager.persistentFolderIconsData.guidTextureList.Add(GUIDTextureData);
                    }
                }
                isIconSetADefaultIconSet = IsIconSetIsDefault();


                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();
                AssetDatabase.Refresh();

                IconManager.persistentFolderIconsData.tempGuidTextureListForIconSets.Clear();
                EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
                return;
            }
            else if (IconManager.persistentFolderIconsData.currentIconSetIndex != 0)
            {
                if (isCalledFromPopupSelection && previousIconSetIndex == 0)
                {
                    for (int i = 0; i < IconManager.persistentFolderIconsData.guidTextureList.Count; i++)
                    {
                        GUIDTextureData GUIDTextureData = IconManager.persistentFolderIconsData.guidTextureList[i];

                        if (!IconManager.persistentFolderIconsData.tempGuidTextureListForIconSets.Contains(GUIDTextureData))
                        {
                            IconManager.persistentFolderIconsData.tempGuidTextureListForIconSets.Add(GUIDTextureData);
                        }
                    }
                }
            }
            Undo.RecordObject(IconManager.persistentFolderIconsData, "Clear Folder Icons");
            IconManager.persistentFolderIconsData.guidTextureList.Clear();

            isIconSetADefaultIconSet = IsIconSetIsDefault();

            IconSetDataListWrapper selectedIconDataList = IconManager.persistentFolderIconsData.iconSetDataList[IconManager.persistentFolderIconsData.currentIconSetIndex];
            for (int x = 0; x < selectedIconDataList.iconSetData.Count; x++)
            {
                IconSetData data = selectedIconDataList.iconSetData[x];
                string assetPath = IconManager.persistentFolderIconsData.allFoldersPathList.Find(x => Path.GetFileNameWithoutExtension(x) == data.folderName);

                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) return;

                if (IconManager.persistentFolderIconsData.guidTextureList.Any(x => x.guid == guid)) return;

                GUIDTextureData guidTextureData = new GUIDTextureData();
                guidTextureData.guid = guid;
                guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, data.icon);

                Undo.RecordObject(IconManager.persistentFolderIconsData, "Update Icons Sets");

                IconManager.persistentFolderIconsData.guidTextureList.Add(guidTextureData);
            }

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();
            AssetDatabase.Refresh();
        }
        #endregion

        private static void FindAndChangeFolderIconsMarkersOnDelete()
        {
            string[] paths = new string[] { settingsWindowPath };

            for (int i = 0; i < paths.Length; i++)
            {
                string currentPath = paths[i].Replace("/", "\\");

                string fullPath = Path.GetFullPath(currentPath);

                string allContents = File.ReadAllText(fullPath);

                string pattern = @"#if\s+.*?(?=\s*// Folder Icons Marker)";
                string replacedContents = Regex.Replace(allContents, pattern, "#if false");

                File.WriteAllText(fullPath, replacedContents);
                AssetDatabase.Refresh();
            }
        }

    }
}