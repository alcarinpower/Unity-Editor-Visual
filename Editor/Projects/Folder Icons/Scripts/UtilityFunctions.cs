using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CodeDestroyer.Editor.EditorVisual
{
    internal static class TextureFunctions
    {
        // Create TextureData and return it
        internal static TextureData CreateTextureData(Color color, Texture2D emptyFolderTexture, Texture2D folderTexture, Texture2D customTexture)
        {
            TextureData newTextureData = new TextureData();

            newTextureData.color = color;
            newTextureData.emptyFolderTexture = emptyFolderTexture;
            newTextureData.folderTexture = folderTexture;
            newTextureData.customTexture = customTexture;

            return newTextureData;
        }
        // Write bytes of a texture
        internal static void CreateTexture(Texture2D importTexture, string assetFullPath)
        {
            byte[] bytes = importTexture.EncodeToPNG();

            File.WriteAllBytes(assetFullPath, bytes);
            AssetDatabase.Refresh();
        }
        // Import texture as Editor GUI, readable and Uncompressed, we need these because when converting texture to base64 these are necessary
        internal static void ImportTexture(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            importer.textureType = TextureImporterType.GUI;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
        // Create a texture with specified color
        internal static void CreateTexture2DWithColor(ref Texture2D texture, Color color, int width, int height)
        {
            Texture2D newTexture2D = new(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newTexture2D.SetPixel(x, y, color);
                }
            }
            newTexture2D.Apply();
            texture = newTexture2D;
        }
        // Convert base64 info to texture2D
        internal static void Base64ToTexture2D(string base64String, string path)
        {
            if (base64String.Length == 0) return;

            byte[] textureArray = Convert.FromBase64String(base64String);
            File.WriteAllBytes(path, textureArray);
            AssetDatabase.Refresh();

        }
        // Create both empty and filled unity's default texture. We need both because a folder can be empty or not
        internal static void CreateDefaultFolderWithColor(Color currentColor, ref Texture2D emptyFolderTexture, ref Texture2D defaultFolderTexture)
        {
            emptyFolderTexture = new Texture2D(IconManager.persistentFolderIconsData.emptyDefaultFolderIcon.width, IconManager.persistentFolderIconsData.emptyDefaultFolderIcon.height);
            defaultFolderTexture = new Texture2D(IconManager.persistentFolderIconsData.defaultFolderIcon.width, IconManager.persistentFolderIconsData.defaultFolderIcon.height);
            for (int x = 0; x < IconManager.persistentFolderIconsData.defaultFolderIcon.width; x++)
            {
                for (int y = 0; y < IconManager.persistentFolderIconsData.defaultFolderIcon.height; y++)
                {
                    // Set empty folder 
                    Color emptyOldColor = IconManager.persistentFolderIconsData.emptyDefaultFolderIcon.GetPixel(x, y);
                    Color emptyNewCol = currentColor;

                    emptyNewCol.a = emptyOldColor.a;
                    emptyFolderTexture.SetPixel(x, y, emptyNewCol);



                    // Set default folder
                    Color defaultOldColor = IconManager.persistentFolderIconsData.defaultFolderIcon.GetPixel(x, y);
                    Color defaultNewCol = currentColor;

                    defaultNewCol.a = defaultOldColor.a;
                    defaultFolderTexture.SetPixel(x, y, defaultNewCol);

                }
            }

            emptyFolderTexture.Apply();
            defaultFolderTexture.Apply();
        }
    }

    internal static class PopupWindowContentFunctions
    {
        // Create and load a newly created colorful default folder texture and create TextureData as well
        internal static void CreateAndLoadDefaultFolderWithColor(string emptyAssetPath, string emptyFullPath, string folderAssetPath, string folderFullPath)
        {
            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            TextureFunctions.CreateTexture(IconManager.projectCurrentEmptyFolderTexture, emptyFullPath);
            TextureFunctions.ImportTexture(emptyAssetPath);

            TextureFunctions.CreateTexture(IconManager.projectCurrentFolderTexture, folderFullPath);
            TextureFunctions.ImportTexture(folderAssetPath);

        }
        // Create color folder texture, its data then assign it to relevant places. update folder empty dictionary
        private static void HandleCreatingColorFolderTexture(string selectedAssetGUID, ref Texture2D emptyTexture, ref Texture2D texture)
        {
            if (IconManager.persistentFolderIconsData != null)
            {
                Undo.RecordObject(IconManager.persistentFolderIconsData, "Create Folder Colors");
            }
            UtilityFunctions.CheckAndCreateIconFolders();


            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            byte[] emptyFolderTextureBytes = emptyTexture.EncodeToPNG();
            byte[] folderTextureBytes = texture.EncodeToPNG();


            string emptyTexturePath = $"{ProjectConstants.emptyIconFolderPath}/{selectedAssetGUID}.png";
            string texturePath = $"{ProjectConstants.iconFolderPath}/{selectedAssetGUID}.png";


            File.WriteAllBytes(Path.GetFullPath(emptyTexturePath), emptyFolderTextureBytes);
            File.WriteAllBytes(Path.GetFullPath(texturePath), folderTextureBytes);

            AssetDatabase.Refresh();

            TextureFunctions.ImportTexture(emptyTexturePath);
            TextureFunctions.ImportTexture(texturePath);

            Texture2D loadedEmptyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyTexturePath);
            Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            GUIDTextureData guidTextureData = new GUIDTextureData();
            guidTextureData.guid = selectedAssetGUID;
            guidTextureData.textureData = TextureFunctions.CreateTextureData(IconManager.projectCurrentColor, loadedEmptyTexture, loadedTexture, null);

            if (IconManager.persistentFolderIconsData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                int index = IconManager.persistentFolderIconsData.guidTextureList.FindIndex(x => x.guid == selectedAssetGUID);
                IconManager.persistentFolderIconsData.guidTextureList[index] = guidTextureData;
            }
            else
            {
                IconManager.persistentFolderIconsData.guidTextureList.Add(guidTextureData);
            }

            if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);


            string emptyIconPath = ProjectConstants.emptyIconFolderPath + "\\" + selectedAssetGUID + ".png";
            string iconFolderPath = ProjectConstants.iconFolderPath + "\\" + selectedAssetGUID + ".png";

            string emptyIconPathParentDir = Path.GetDirectoryName(emptyIconPath).Replace("\\", "/");
            string iconFolderPathParentDir = Path.GetDirectoryName(iconFolderPath).Replace("\\", "/");

            if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
        }
        // Create default folder with custom color and delete existing ones if it exists (Do not use this in any update function)
        internal static void HandleColorFoldersTexture(string selectedAssetGUID)
        {
            IconManager.projectCurrentCustomTexture = null;

            // Creating folder texture color with selected color
            if (!IconManager.persistentFolderIconsData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                HandleCreatingColorFolderTexture(selectedAssetGUID, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            }
            // Deleting and changing folder texture color with selected color
            else
            {
                if (!AssetDatabase.AssetPathExists(ProjectConstants.emptyIconFolderPath) && !AssetDatabase.AssetPathExists(ProjectConstants.iconFolderPath))
                {
                    HandleCreatingColorFolderTexture(selectedAssetGUID, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
                }
                else
                {
                    AssetDatabase.DeleteAsset(ProjectConstants.emptyIconFolderPath + "/" + selectedAssetGUID + ".png");
                    AssetDatabase.DeleteAsset(ProjectConstants.iconFolderPath + "/" + selectedAssetGUID + ".png");

                    HandleCreatingColorFolderTexture(selectedAssetGUID, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
                }
            }
        }
        // Create custom texture data and assign it to relevant places
        internal static void HandleCustomTexture(string selectedAssetGUID)
        {
            Undo.RecordObject(IconManager.persistentFolderIconsData, "Assign Custom Texture To Folder");

            UtilityFunctions.CheckAndCreateIconFolders();


            IconManager.projectCurrentEmptyFolderTexture = null;
            IconManager.projectCurrentFolderTexture = null;

            if (!IconManager.persistentFolderIconsData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                GUIDTextureData guidTextureData = new GUIDTextureData();
                guidTextureData.guid = selectedAssetGUID;
                guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, IconManager.projectCurrentCustomTexture);

                IconManager.persistentFolderIconsData.guidTextureList.Add(guidTextureData);
                if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
            }
            else if (IconManager.persistentFolderIconsData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                if (AssetDatabase.AssetPathExists(ProjectConstants.emptyIconFolderPath) && AssetDatabase.AssetPathExists(ProjectConstants.iconFolderPath))
                {
                    AssetDatabase.DeleteAsset(ProjectConstants.emptyIconFolderPath + "/" + selectedAssetGUID + ".png");
                    AssetDatabase.DeleteAsset(ProjectConstants.iconFolderPath + "/" + selectedAssetGUID + ".png");
                }

                GUIDTextureData guidTextureData = new GUIDTextureData();
                guidTextureData.guid = selectedAssetGUID;
                guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, IconManager.projectCurrentCustomTexture);

                int index = IconManager.persistentFolderIconsData.guidTextureList.FindIndex(x => x.guid == selectedAssetGUID);
                IconManager.persistentFolderIconsData.guidTextureList[index] = guidTextureData;
                if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
            }
            if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
        }
    }
    internal static class UtilityFunctions
    {
        // Check and create necessary folders if they are not exist
        internal static void CheckAndCreateIconFolders()
        {
            EnsureFolder(ProjectConstants.emptyIconFolderPath, ProjectConstants.dataPath, ProjectConstants.EmptyColorFolderIcons);
            EnsureFolder(ProjectConstants.iconFolderPath, ProjectConstants.dataPath, ProjectConstants.ColorFolderIcons);
            EnsureFolder(ProjectConstants.loadedIconSetPath, ProjectConstants.dataPath, ProjectConstants.LoadedIconSets);
            EnsureFolder(ProjectConstants.loadedIconsPath, ProjectConstants.dataPath, ProjectConstants.LoadedIcons);

            IconManager.LoadOrCreatePersistentFolderIconData();
        }

        private static void EnsureFolder(string fullPath, string parentPath, string newFolderName)
        {
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentPath, newFolderName);
            }
        }
        // Handle if is a folder filled or not
        internal static bool IsFolderFilled(ref string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    return Directory.GetFileSystemEntries(Path.GetFullPath(folderPath)).Length > 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking folder: {folderPath}. Exception: {ex.Message}");
                return false;
            }
        }

        // Update folder empty dictionary according to parent folder
        internal static void UpdateFolderEmptyDict(string parentDirectory, ref Dictionary<string, bool> dict)
        {
            string assetFullPath = Path.GetFullPath(parentDirectory).Replace("\\", "/");
            if (string.IsNullOrEmpty(assetFullPath)) return;
            bool isFolderFilled = IsFolderFilled(ref assetFullPath);


            if (!dict.ContainsKey(parentDirectory))
            {
                dict.Add(parentDirectory, isFolderFilled);
            }
            else
            {
                dict[parentDirectory] = isFolderFilled;
            }


            int folderEmptinessIndex = IconManager.persistentFolderIconsData.folderEmptinessList.FindIndex((x) => x.folderPath == parentDirectory);
            if (folderEmptinessIndex == -1)
            {
                FolderEmptinessData folderEmptiness = new FolderEmptinessData();
                folderEmptiness.folderPath = parentDirectory;
                folderEmptiness.isFolderEmpty = isFolderFilled;
                IconManager.persistentFolderIconsData.folderEmptinessList.Add(folderEmptiness);
            }
            else
            {
                IconManager.persistentFolderIconsData.folderEmptinessList[folderEmptinessIndex].isFolderEmpty = isFolderFilled;
            }
        }
     
        // Check all folders and assign to empty-filled state of folders to empty folderemptydict
        internal static void CheckAllFoldersCurrentEmptiness(ref Dictionary<string, bool> folderEmptyDict)
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            for (int assetPathIndex = 0; assetPathIndex < allAssets.Length; assetPathIndex++)
            {
                string assetPath = allAssets[assetPathIndex];
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    bool isFolderEmpty = Directory.GetFileSystemEntries(Path.GetFullPath(assetPath)).Length > 0;


                    if (!folderEmptyDict.ContainsKey(assetPath))
                    {
                        folderEmptyDict.Add(assetPath, isFolderEmpty);
                    }
                    else
                    {
                        folderEmptyDict[assetPath] = isFolderEmpty;
                    }
                    int folderEmptinessIndex = IconManager.persistentFolderIconsData.folderEmptinessList.FindIndex((x) => x.folderPath == assetPath);
                    if (folderEmptinessIndex == -1)
                    {
                        FolderEmptinessData folderEmptiness = new FolderEmptinessData();
                        folderEmptiness.folderPath = assetPath;
                        folderEmptiness.isFolderEmpty = isFolderEmpty;
                        IconManager.persistentFolderIconsData.folderEmptinessList.Add(folderEmptiness);
                    }
                    else
                    {
                        IconManager.persistentFolderIconsData.folderEmptinessList[folderEmptinessIndex].isFolderEmpty = isFolderEmpty;
                    }
                }
            }
        }


        // When creating a new folder, update its icon to given name to itself. This is for Icon Sets.
        internal static void UpdateCurrentFolderIconWithIconSet(string assetPath)
        {
            if (IconManager.persistentFolderIconsData == null) return;
            if (IconManager.persistentFolderIconsData.currentIconSetIndex == 0) return;
            if (IconManager.persistentFolderIconsData.iconSetDataList.Count == 0) return;
            if (IconManager.persistentFolderIconsData.iconSetDataList.Count > IconManager.persistentFolderIconsData.currentIconSetIndex)
            {
                int iconSetIndex = IconManager.persistentFolderIconsData.currentIconSetIndex;

                int iconIndex = IconManager.persistentFolderIconsData.iconSetDataList[iconSetIndex].iconSetData.
                    FindIndex(iconSetData => iconSetData.folderName == Path.GetFileNameWithoutExtension(assetPath));


                string currentGUID = AssetDatabase.AssetPathToGUID(assetPath);

                GUIDTextureData currentGUIDTextureData = IconManager.persistentFolderIconsData.guidTextureList.Find(word => word.guid == currentGUID);

                if (iconIndex != -1)
                {
                    Texture2D icon = IconManager.persistentFolderIconsData.iconSetDataList[iconSetIndex].iconSetData.
                        Find(x => x.folderName == Path.GetFileNameWithoutExtension(assetPath)).icon;


                    if (!IconManager.persistentFolderIconsData.guidTextureList.Contains(currentGUIDTextureData))
                    {
                        GUIDTextureData guidTextureData = new GUIDTextureData();
                        guidTextureData.guid = currentGUID;
                        guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, icon);

                        IconManager.persistentFolderIconsData.guidTextureList.Add(guidTextureData);
                        if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
                    }
                    else
                    {
                        GUIDTextureData guidTextureData = new GUIDTextureData();
                        guidTextureData.guid = currentGUID;
                        guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, icon);

                        int guidTextureDataIndex = IconManager.persistentFolderIconsData.guidTextureList.FindIndex(x => x.guid == currentGUID);
                        IconManager.persistentFolderIconsData.guidTextureList[guidTextureDataIndex] = guidTextureData;

                    }

                }
                else
                {
                    IconManager.persistentFolderIconsData.guidTextureList.Remove(currentGUIDTextureData);
                    if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);

                    ReDrawFolders();
                }

            }
        }

        // Draw one item in the project window
        internal static void DrawTextures(string guid, Rect rect, Texture2D texture2d)
        {
            bool treeView = rect.width > rect.height;
            bool sideView = rect.x != 14;

            // For vertical folder view
            if (treeView)
            {
                rect.width = 16f;
                rect.height = 16f;

                // Small offset
                if (!sideView) rect.x += 3f;
            }
            else rect.height -= 14f;

            if (texture2d == null) return;
          
            GUI.DrawTexture(rect, texture2d, ScaleMode.ScaleAndCrop);
        }

        // Main function for drawing project window items
        internal static void DrawFolders(string guid, Rect selectionRect)
        {
            if (IconManager.persistentFolderIconsData.guidTextureList.Count == 0) return; // If there is not a custom icon return
            //if (guid == "00000000000000000000000000000000") return; // Ignore main assets folder
            //if (guid == "00000000000000001000000000000000") return; // Ignore main packages folder

            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!AssetDatabase.IsValidFolder(folderPath)) return;


            // Draw default and empty folders
            if (!IconManager.persistentFolderIconsData.guidTextureList.Any(x => x.guid == guid))
            {

                IconManager.isFolderFilledDict.TryGetValue(folderPath, out bool outputBool);
                if (outputBool)
                {
                    DrawTextures(guid, selectionRect, IconManager.persistentFolderIconsData.defaultFolderIcon);
                }
                else
                    DrawTextures(guid, selectionRect, IconManager.persistentFolderIconsData.emptyDefaultFolderIcon);
            }

            else
            {
                for (int i = 0; i < IconManager.persistentFolderIconsData.guidTextureList.Count; i++)
                {
                    GUIDTextureData textureData = IconManager.persistentFolderIconsData.guidTextureList[i];

                    if (textureData.guid != guid) continue;

                    if (textureData.textureData.folderTexture != null)
                    {
                        IconManager.isFolderFilledDict.TryGetValue(folderPath, out bool outputBool);
                        if (outputBool)
                        {
                            DrawTextures(guid, selectionRect, textureData.textureData.folderTexture);
                        }
                        else
                        {
                            DrawTextures(guid, selectionRect, textureData.textureData.emptyFolderTexture);
                        }
                    }
                    else if (textureData.textureData.customTexture != null)
                    {
                        DrawTextures(guid, selectionRect, textureData.textureData.customTexture);
                    }
                }
            }
        }


        // Redraw EveryFolders with DrawFolders Function
        internal static void ReDrawFolders()
        {
            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += DrawFolders;
            AssetDatabase.Refresh();
            EditorApplication.RepaintProjectWindow();
        }
    }



    // Without JsonHelper we have to wrap every list to a serializable class
    internal static class JsonHelper
    {
        internal static void SaveJson<T>(string savePath, T item)
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(item));
        }
        internal static void SaveJson<T>(string savePath, T[] items)
        {
            JsonArray<T> itemData = new JsonArray<T>();
            itemData.objectArray = items;

            File.WriteAllText(savePath, JsonUtility.ToJson(itemData));
        }
        internal static void SaveJson<T>(string savePath, List<T> items)
        {
            JsonList<T> itemData = new JsonList<T>();
            itemData.objectList = items;

            File.WriteAllText(savePath, JsonUtility.ToJson(itemData));
        }


        internal static void LoadJson<T>(string loadPath, out T data)
        {
            data = JsonUtility.FromJson<T>(File.ReadAllText(loadPath));
        }
        internal static void LoadJson<T>(string loadPath, out T[] outArray)
        {
            outArray = JsonUtility.FromJson<JsonArray<T>>(File.ReadAllText(loadPath)).objectArray;
        }
        internal static void LoadJson<T>(string loadPath, out List<T> outList)
        {
            outList = JsonUtility.FromJson<JsonList<T>>(File.ReadAllText(loadPath)).objectList;
        }



        internal static void FromJsonOverwrite<T>(T json, T objectToOverWrite)
        {
            string jsonString = JsonUtility.ToJson(json);
            JsonUtility.FromJsonOverwrite(jsonString, objectToOverWrite);
        }

        [Serializable]
        private struct JsonArray<T>
        {
            [SerializeField] internal T[] objectArray;
        }
        [Serializable]
        private struct JsonList<T>
        {
            [SerializeField] internal List<T> objectList;
        }
    }
}
