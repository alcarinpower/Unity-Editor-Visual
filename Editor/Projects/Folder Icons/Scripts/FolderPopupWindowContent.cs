using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CompilerDestroyer.Editor.EditorVisual
{

    [Serializable]
    internal sealed class FolderPopupWindowContent : PopupWindowContent
    {
        private string currentAssetPath;
        private static string currentAssetGUID;
        private static ColorField selectedColorField;
        private static ObjectField selectedObjectField;
        public override VisualElement CreateGUI()
        {
            VisualElement rootVisualElement = new VisualElement();

            if (IconManager.persistentFolderIconsData.currentIconSetIndex == 0)
            {
                rootVisualElement.style.width = 300f;
                rootVisualElement.style.height = 62f;

                selectedColorField = new ColorField("Folder Color");
                selectedColorField.RegisterValueChangedCallback(evt =>
                {
                    IconManager.projectCurrentColor = evt.newValue;
                    // Alternatively we could just ignore changing color of folder icon directly here and apply color in the OnClose method of the popup window
                    // But its worth to tradeoff because this way we can see folder color change immediately.
                    ChangeFolderColor();
                });
                selectedColorField.style.minWidth = 200f;
                selectedColorField.style.marginLeft = 2;

                selectedObjectField = new ObjectField("Custom Texture");
                selectedObjectField.objectType = typeof(Texture2D);
                selectedObjectField.RegisterValueChangedCallback(evt =>
                {
                    IconManager.projectCurrentCustomTexture = evt.newValue as Texture2D;
                    ChangeFolderTexture();
                });
                Button deleteButton = new Button();
                deleteButton.clicked += DeleteIcon;
                deleteButton.text = "Delete Icon";

                rootVisualElement.Add(selectedColorField);
                rootVisualElement.Add(selectedObjectField);
                rootVisualElement.Add(deleteButton);
            }
            else
            {
                rootVisualElement.style.width = 270f;
                rootVisualElement.style.height = 45f;

                Label disabledLabel = new Label("Icon Sets Are Enabled! For Disabling Icon Sets:\nChoose \"None\" from dropdown menu\nTools > Compiler Destroyer > Editor Visual > Folder Icons");

                rootVisualElement.Add(disabledLabel);
            }
            return rootVisualElement;
        }

        public override void OnOpen()
        {
            if (!IconManager.persistentFolderIconsData.packageIsInstalledLocally)
            {
                Debug.LogWarning("Editor visual is installed with git. Folder icons will not be working!");
                return;
            }

            currentAssetPath = AssetDatabase.GetAssetPath(Selection.activeEntityId);
            currentAssetGUID = AssetDatabase.AssetPathToGUID(currentAssetPath);


            GUIDTextureData guidTextureData = IconManager.persistentFolderIconsData.guidTextureList.Find(x => x.guid == currentAssetGUID);

            if (!string.IsNullOrEmpty(guidTextureData.guid))
            {
                IconManager.projectCurrentColor = guidTextureData.textureData.color;
                IconManager.projectCurrentCustomTexture = guidTextureData.textureData.customTexture;

                EditorApplication.delayCall += () =>
                {
                    selectedColorField?.SetValueWithoutNotify(IconManager.projectCurrentColor);
                    selectedObjectField?.SetValueWithoutNotify(IconManager.projectCurrentCustomTexture);
                };
            }
        }

        // When closing this PopupWindowContent Save all textures and draw them in the main DrawFolders function to make it persistent
        public override void OnClose()
        {
            if (!IconManager.persistentFolderIconsData.packageIsInstalledLocally)
            {
                Debug.LogWarning("Editor visual is installed with git. Folder icons will not be working!");
                return;
            }

            if (IconManager.projectCurrentEmptyFolderTexture != null)
            {
                PopupWindowContentFunctions.HandleColorFoldersTexture(currentAssetGUID);
            }
            else if (IconManager.projectCurrentFolderTexture != null)
            {
                PopupWindowContentFunctions.HandleColorFoldersTexture(currentAssetGUID);
            }
            if (IconManager.projectCurrentCustomTexture != null)
            {
                PopupWindowContentFunctions.HandleCustomTexture(currentAssetGUID);
            }


            IconManager.projectCurrentColor = Color.clear;
            IconManager.projectCurrentEmptyFolderTexture = null;
            IconManager.projectCurrentFolderTexture = null;
            IconManager.projectCurrentCustomTexture = null;

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();

            if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
        }



        // Three functions for temporary project drawing. When clicking and dragging in the color property of this PopupWindowContent these three functions will run.
        // In order to temporarily creating a texture and drawing color folder. Otherwise we would have to create a GUIDTextureData and that would not be performant.
        private static void PrivateCreateDefaultFolderWithColor(Color currentColor, ref Texture2D emptyFolderTexture, ref Texture2D defaultFolderTexture)
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeEntityId);
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                IconManager.isFolderFilledDict.TryGetValue(folderPath, out bool isFolderFilled);
                if (isFolderFilled)
                {
                    defaultFolderTexture = new Texture2D(IconManager.persistentFolderIconsData.defaultFolderIcon.width, IconManager.persistentFolderIconsData.defaultFolderIcon.height);

                    for (int x = 0; x < IconManager.persistentFolderIconsData.defaultFolderIcon.width; x++)
                    {
                        for (int y = 0; y < IconManager.persistentFolderIconsData.defaultFolderIcon.height; y++)
                        {
                            // Set default folder
                            Color defaultOldColor = IconManager.persistentFolderIconsData.defaultFolderIcon.GetPixel(x, y);
                            Color defaultNewCol = currentColor;

                            defaultNewCol.a = defaultOldColor.a;
                            defaultFolderTexture.SetPixel(x, y, defaultNewCol);
                        }
                    }

                    defaultFolderTexture.Apply();

                }
                else
                {

                    emptyFolderTexture = new Texture2D(IconManager.persistentFolderIconsData.emptyDefaultFolderIcon.width, IconManager.persistentFolderIconsData.emptyDefaultFolderIcon.height);
                    for (int x = 0; x < IconManager.persistentFolderIconsData.defaultFolderIcon.width; x++)
                    {
                        for (int y = 0; y < IconManager.persistentFolderIconsData.defaultFolderIcon.height; y++)
                        {
                            // Set empty folder 
                            Color emptyOldColor = IconManager.persistentFolderIconsData.emptyDefaultFolderIcon.GetPixel(x, y);
                            Color emptyNewCol = currentColor;

                            emptyNewCol.a = emptyOldColor.a;
                            emptyFolderTexture.SetPixel(x, y, emptyNewCol);
                        }
                    }

                    emptyFolderTexture.Apply();
                }
            }
        }
        internal static void DrawFolderColor(string guid, Rect selectionRect)
        {
            if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeEntityId))) return;

            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                IconManager.isFolderFilledDict.TryGetValue(folderPath, out bool isFolderFilled);
                if (isFolderFilled)
                {
                    UtilityFunctions.DrawTextures(guid, selectionRect, IconManager.projectCurrentFolderTexture);
                }
                else
                {
                    UtilityFunctions.DrawTextures(guid, selectionRect, IconManager.projectCurrentEmptyFolderTexture);
                }

            }
        }
        internal static void ChangeFolderColor()
        {
            PrivateCreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            IconManager.projectCurrentCustomTexture = null;
            EditorApplication.projectWindowItemOnGUI += DrawFolderColor;
            EditorApplication.RepaintProjectWindow();
        }



        // Handle showing of custom texture in the project view
        internal static void DrawFolderTexture(string guid, Rect selectionRect)
        {
            if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeEntityId))) return;
            UtilityFunctions.DrawTextures(guid, selectionRect, IconManager.projectCurrentCustomTexture);
        }
        internal static void ChangeFolderTexture()
        {
            IconManager.projectCurrentFolderTexture = null;

            EditorApplication.projectWindowItemOnGUI += DrawFolderTexture;
            EditorApplication.RepaintProjectWindow();
        }
        private void DeleteIcon()
        {
            if (AssetDatabase.AssetPathExists(ProjectConstants.emptyIconFolderPath) && AssetDatabase.AssetPathExists(ProjectConstants.iconFolderPath))
            {
                AssetDatabase.DeleteAsset(ProjectConstants.emptyIconFolderPath + "/" + currentAssetGUID + ".png");
                AssetDatabase.DeleteAsset(ProjectConstants.iconFolderPath + "/" + currentAssetGUID + ".png");
                AssetDatabase.Refresh();
            }


            for (int guidTextureIndex = 0; guidTextureIndex < IconManager.persistentFolderIconsData.guidTextureList.Count; guidTextureIndex++)
            {
                if (IconManager.persistentFolderIconsData.guidTextureList[guidTextureIndex].guid == currentAssetGUID)
                {
                    IconManager.persistentFolderIconsData.guidTextureList.RemoveAt(guidTextureIndex);
                    break;
                }
            }

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();

            IconManager.projectCurrentFolderTexture = null;
            IconManager.projectCurrentEmptyFolderTexture = null;
            IconManager.projectCurrentCustomTexture = null;
            editorWindow.Close();
        }
    }
}