using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CodeDestroyer.Editor.EditorVisual
{
    internal sealed class IconManager
    {
        internal static PersistentFolderIconsData persistentFolderIconsData;

        // Inspector header contents
        internal static HeaderContents headerContents;

        // Project current values
        internal static Color projectCurrentColor;
        internal static Texture2D projectCurrentEmptyFolderTexture;
        internal static Texture2D projectCurrentFolderTexture;

        internal static Texture2D projectCurrentCustomTexture;

        internal static Dictionary<string, bool> isFolderFilledDict;
        [SerializeField] internal static List<string> iconSetNames;



        // Main function that includes everything that must be running from AssetPostProccesor with didDomainReload block
        internal static void InitializeFolderIcons()
        {
            AssetOperations();

            PackageSource packageSource = PackageInfo.FindForPackageName(GlobalVariables.UnityEditorVisualPackageName).source;

            if (File.Exists(GlobalVariables.ProjectTempInstalledFilePath))
            {
                Debug.Log("FÝle Exist");
                bool isLocal = packageSource == PackageSource.Embedded || packageSource == PackageSource.Local || packageSource == PackageSource.LocalTarball;

                if (!isLocal)
                {
                    Debug.Log("isLocal");

                    persistentFolderIconsData.packageIsInstalledLocally = false;
                    SavePersistentData();
                    Debug.LogWarning("Project is installed with git. Nothing will work! Please save icons, then remove and reinstall entire project.");
                }
                else
                {
                    Debug.Log("not Local");

                    persistentFolderIconsData.packageIsInstalledLocally = true;
                    SavePersistentData();
                }
            }

            InitInspectorHeaderContents();


            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();
            AssetDatabase.Refresh();

            EditorApplication.quitting += SavePersistentData;
        }

        // Initialize inspector header contents
        private static void InitInspectorHeaderContents()
        {
            if (persistentFolderIconsData != null)
            {
                headerContents ??= new HeaderContents();

                headerContents.headerIconGUIStyle = new GUIStyle();
                headerContents.headerIconGUIStyle.normal.background = persistentFolderIconsData.buttonBackgroundTexture;
                headerContents.headerIconGUIStyle.hover.background = persistentFolderIconsData.buttonHoverTexture;

                headerContents.resetButtonGUIContent = new GUIContent("Reset");
                headerContents.openButton = EditorGUIUtility.TrTextContent("Open");
            }
        }
       
        // Check folders, create if it is not exist, load persistentData, create if it is not exist check all folders if they are empty or not
        // Finally update iconSetNames 
        internal static void AssetOperations()
        {
            UtilityFunctions.CheckAndCreateIconFolders();

            isFolderFilledDict = new Dictionary<string, bool>();

            if (persistentFolderIconsData.emptyDefaultFolderIcon == null)
            {
                persistentFolderIconsData.emptyDefaultFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.dynamicDefaultEmptyFolderPath);
            }
            if (persistentFolderIconsData.defaultFolderIcon == null)
            {
                persistentFolderIconsData.defaultFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.dynamicDefaultFolderPath);
            }
            if (persistentFolderIconsData.buttonBackgroundTexture == null)
            {
                persistentFolderIconsData.buttonBackgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.defaultButtonPath);
            }
            if (persistentFolderIconsData.buttonHoverTexture == null)
            {
                persistentFolderIconsData.buttonHoverTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.hoverButtonPath);
            }


            if (!SessionState.GetBool("isCheckedFolderEmptiness", false))
            {
                SessionState.SetBool("isCheckedFolderEmptiness", true);

                UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref isFolderFilledDict);

                string[] allFolders = AssetDatabase.FindAssets("t:DefaultAsset");

                persistentFolderIconsData.allFoldersPathList = new List<string>();

                for (int i = 0; i < allFolders.Length; i++)
                {
                    string folderPath = AssetDatabase.GUIDToAssetPath(allFolders[i]);
                    if (AssetDatabase.IsValidFolder(folderPath))
                    {
                        persistentFolderIconsData.allFoldersPathList.Add(folderPath);
                    }
                }

                if (persistentFolderIconsData != null) EditorUtility.SetDirty(persistentFolderIconsData);
            }
            else
            {
                int count = persistentFolderIconsData.folderEmptinessList.Count;

                for (int i = 0; i < count; i++)
                {
                    FolderEmptinessData folderEmptiness = persistentFolderIconsData.folderEmptinessList[i];
                    string folderPath = folderEmptiness.folderPath;

                    isFolderFilledDict.TryAdd(folderPath, folderEmptiness.isFolderEmpty);
                }
            }
            iconSetNames = new List<string>(persistentFolderIconsData.iconSetDataList.Count);
            for (int i = 0; i < persistentFolderIconsData.iconSetDataList.Count; i++)
            {
                iconSetNames.Add(persistentFolderIconsData.iconSetDataList[i].iconSetName);
            }
        }
        internal static void LoadOrCreatePersistentFolderIconData()
        {
            persistentFolderIconsData = AssetDatabase.LoadAssetAtPath<PersistentFolderIconsData>(ProjectConstants.persistentDataPath);

            if (persistentFolderIconsData == null)
            {
                persistentFolderIconsData = ScriptableObject.CreateInstance<PersistentFolderIconsData>();
                AssetDatabase.CreateAsset(persistentFolderIconsData, ProjectConstants.persistentDataPath);
                AssetDatabase.ImportAsset(ProjectConstants.persistentDataPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();


                isFolderFilledDict = new Dictionary<string, bool>();

                if (persistentFolderIconsData.emptyDefaultFolderIcon == null)
                {
                    persistentFolderIconsData.emptyDefaultFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.dynamicDefaultEmptyFolderPath);
                }
                if (persistentFolderIconsData.defaultFolderIcon == null)
                {
                    persistentFolderIconsData.defaultFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.dynamicDefaultFolderPath);
                }
                if (persistentFolderIconsData.buttonBackgroundTexture == null)
                {
                    persistentFolderIconsData.buttonBackgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.defaultButtonPath);
                }
                if (persistentFolderIconsData.buttonHoverTexture == null)
                {
                    persistentFolderIconsData.buttonHoverTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectConstants.hoverButtonPath);
                }


                UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref isFolderFilledDict);

                int count = persistentFolderIconsData.folderEmptinessList.Count;

                for (int i = 0; i < count; i++)
                {
                    FolderEmptinessData folderEmptiness = persistentFolderIconsData.folderEmptinessList[i];
                    string folderPath = folderEmptiness.folderPath;

                    isFolderFilledDict.TryAdd(folderPath, folderEmptiness.isFolderEmpty);
                }

                iconSetNames = new List<string>(1);
                iconSetNames.Add("None");

                IconSetDataListWrapper iconSetDataListWrapper = new IconSetDataListWrapper();
                iconSetDataListWrapper.iconSetName = "None";
                iconSetDataListWrapper.iconSetData = null;
                persistentFolderIconsData.iconSetDataList.Add(iconSetDataListWrapper);

                SavePersistentData();
            }

        }
        // Save persistentData when exiting editor
        private static void SavePersistentData()
        {
            if (persistentFolderIconsData != null)
            {
                EditorUtility.SetDirty(persistentFolderIconsData);
                AssetDatabase.SaveAssets();
            }
        }
        
        internal static void LoadIcons(string selectedFile)
        {
            UtilityFunctions.CheckAndCreateIconFolders();

            ProjectWideFolderIcons projectWideFolderIcons;
            JsonHelper.LoadJson<ProjectWideFolderIcons>(selectedFile, out projectWideFolderIcons);


            List<IconSetListData> iconSetDataList = projectWideFolderIcons.iconSetListDataList;
            List<JsonTextureData> jsonDataList = projectWideFolderIcons.jsonTextureDataList;

            // ______________________________________________________ Load Icon Sets_______________________________________________________________________
            {

                for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < iconSetDataList.Count; loadedJsonDataIndex++)
                {
                    IconSetListData data = iconSetDataList[loadedJsonDataIndex];

                    IconSetDataListWrapper newIconSetDataListWrapper = new IconSetDataListWrapper();
                    newIconSetDataListWrapper.iconSetData = new List<IconSetData>();
                    newIconSetDataListWrapper.iconSetName = data.iconSetName;

                    string iconSetImportPath = $"{ProjectConstants.loadedIconSetPath}/{data.iconSetName}";

                    if (!AssetDatabase.IsValidFolder(iconSetImportPath))
                    {
                        AssetDatabase.CreateFolder(ProjectConstants.loadedIconSetPath, data.iconSetName);
                    }

                    IconSetData newIconSetData;

                    for (int iconSetIndex = 0; iconSetIndex < data.iconSetData.Count; iconSetIndex++)
                    {
                        newIconSetData = new();
                        string folderName = data.iconSetData[iconSetIndex].folderName;
                        string iconName = data.iconSetData[iconSetIndex].iconName;
                        string base64Texture = data.iconSetData[iconSetIndex].iconBase64;

                        string fullPackagePath = Path.GetFullPath(iconSetImportPath);

                        TextureFunctions.Base64ToTexture2D(base64Texture, fullPackagePath + $"/{iconName}.png");
                        TextureFunctions.ImportTexture($"{iconSetImportPath}/{iconName}.png");


                        newIconSetData.folderName = folderName;
                        newIconSetData.icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconSetImportPath}/{iconName}.png");
                        newIconSetData.icon.name = iconName;

                        newIconSetDataListWrapper.iconSetData.Add(newIconSetData);
                    }


                    IconManager.persistentFolderIconsData.iconSetDataList.Add(newIconSetDataListWrapper);
                    if (IconManager.persistentFolderIconsData != null) EditorUtility.SetDirty(IconManager.persistentFolderIconsData);
                }
                AssetDatabase.Refresh();

                Debug.Log($"Loaded icon sets from: {selectedFile}");
            }
            // _____________________________________________________________________________________________________________________________________________________



            // Load Icons
            {
                UtilityFunctions.CheckAndCreateIconFolders();
                
                string folderName = "";
                Color color;
                string emptyFolderBase64String;
                string folderTextureBase64String;
                string customTextureBase64String;

                for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
                {
                    JsonTextureData jsonTextureData = jsonDataList[loadedJsonDataIndex];


                    folderName = jsonTextureData.folderName;
                    color = new Color(jsonTextureData.color.x, jsonTextureData.color.y, jsonTextureData.color.z, jsonTextureData.color.w);


                    emptyFolderBase64String = jsonTextureData.emptyFolderTextureBase64;
                    folderTextureBase64String = jsonTextureData.folderTextureBase64;
                    customTextureBase64String = jsonTextureData.customTextureBase64;


                    string emptyFolderTexturePath = Path.Combine(ProjectConstants.emptyIconFolderPath, jsonTextureData.emptyFolderTextureName + ".png");
                    string folderTexturePath = Path.Combine(ProjectConstants.iconFolderPath, jsonTextureData.folderTextureName + ".png");
                    string customFolderTexturePath = Path.Combine(ProjectConstants.loadedIconsPath, jsonTextureData.customTextureName + ".png");

                    TextureFunctions.Base64ToTexture2D(emptyFolderBase64String, Path.GetFullPath(emptyFolderTexturePath));
                    TextureFunctions.Base64ToTexture2D(folderTextureBase64String, Path.GetFullPath(folderTexturePath));
                    TextureFunctions.Base64ToTexture2D(customTextureBase64String, Path.GetFullPath(customFolderTexturePath));

                    AssetDatabase.Refresh();

                    if (emptyFolderBase64String.Length > 0)
                        TextureFunctions.ImportTexture(emptyFolderTexturePath);
                    if (folderTextureBase64String.Length > 0)
                        TextureFunctions.ImportTexture(folderTexturePath);
                    if (customTextureBase64String.Length > 0)
                        TextureFunctions.ImportTexture(customFolderTexturePath);



                    Color currentColor = new Color(jsonTextureData.color.x, jsonTextureData.color.y, jsonTextureData.color.z, jsonTextureData.color.w);
                    Texture2D emptyFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyFolderTexturePath);
                    Texture2D colorFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(folderTexturePath);
                    Texture2D customFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(customFolderTexturePath);


                    // Creating all folders within json data
                    string[] pathSegments = jsonTextureData.folderName.Split('/');

                    string[] resultPathLevels = new string[pathSegments.Length];

                    for (int i = 0; i < pathSegments.Length; i++)
                    {
                        resultPathLevels[i] = string.Join("/", pathSegments.Take(i + 1));


                        string currentFolderPath = resultPathLevels[i];


                        if (currentFolderPath == "Assets") continue;


                        if (!AssetDatabase.IsValidFolder(currentFolderPath))
                        {
                            string currentFolderGUID = AssetDatabase.CreateFolder(Path.GetDirectoryName(currentFolderPath), Path.GetFileName(currentFolderPath));

                            if (currentFolderPath != jsonTextureData.folderName) continue;

                            GUIDTextureData newGuidTextureData = new GUIDTextureData();

                            newGuidTextureData.guid = currentFolderGUID;
                            newGuidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, emptyFolderTexture, colorFolderTexture, customFolderTexture);

                            persistentFolderIconsData.guidTextureList.Add(newGuidTextureData);

                            string fullPath = Path.GetFullPath(currentFolderPath);
                            string parentDir = Path.GetDirectoryName(fullPath);
                            string currentFolderPathParentDir = parentDir != null ? parentDir.Replace(Path.DirectorySeparatorChar, '/') : string.Empty;

                            UtilityFunctions.UpdateFolderEmptyDict(currentFolderPathParentDir, ref isFolderFilledDict);

                        }
                        else if (AssetDatabase.IsValidFolder(currentFolderPath))
                        {
                            if (currentFolderPath != jsonTextureData.folderName) continue;

                            if (!persistentFolderIconsData.guidTextureList.Any(textureData => textureData.guid == AssetDatabase.AssetPathToGUID(currentFolderPath)))
                            {
                                GUIDTextureData newGuidTextureData = new GUIDTextureData();

                                newGuidTextureData.guid = AssetDatabase.AssetPathToGUID(currentFolderPath);
                                newGuidTextureData.textureData = TextureFunctions.CreateTextureData(IconManager.projectCurrentColor, emptyFolderTexture, colorFolderTexture, customFolderTexture);

                                persistentFolderIconsData.guidTextureList.Add(newGuidTextureData);
                                string fullPath = Path.GetFullPath(currentFolderPath);
                                string parentDir = Path.GetDirectoryName(fullPath);
                                string currentFolderPathParentDir = parentDir != null ? parentDir.Replace(Path.DirectorySeparatorChar, '/') : string.Empty;

                                UtilityFunctions.UpdateFolderEmptyDict(currentFolderPathParentDir, ref IconManager.isFolderFilledDict);

                            }
                            else if (persistentFolderIconsData.guidTextureList.Any(textureData => textureData.guid == AssetDatabase.AssetPathToGUID(currentFolderPath)))
                            {
                                GUIDTextureData newGuidTextureData = new GUIDTextureData();

                                newGuidTextureData.guid = AssetDatabase.AssetPathToGUID(currentFolderPath);
                                newGuidTextureData.textureData = TextureFunctions.CreateTextureData(IconManager.projectCurrentColor, emptyFolderTexture, colorFolderTexture, customFolderTexture);

                                int index = persistentFolderIconsData.guidTextureList.FindIndex(guidData => guidData.guid == AssetDatabase.AssetPathToGUID(currentFolderPath));
                                persistentFolderIconsData.guidTextureList[index] = newGuidTextureData;


                                string fullPath = Path.GetFullPath(currentFolderPath);
                                string parentDir = Path.GetDirectoryName(fullPath);
                                string currentFolderPathParentDir = parentDir != null ? parentDir.Replace(Path.DirectorySeparatorChar, '/') : string.Empty;

                                UtilityFunctions.UpdateFolderEmptyDict(currentFolderPathParentDir, ref IconManager.isFolderFilledDict);
                            }
                        }
                    }
                }


                if (persistentFolderIconsData != null) EditorUtility.SetDirty(persistentFolderIconsData);
                AssetDatabase.SaveAssets();


                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();
                AssetDatabase.Refresh();
            }
            // _____________________________________________________________________________________________________________________________________________________
        }
        internal static void SaveIcons(string selectedFile)
        {
            // ------------------------------------------------ Save Icon Sets ------------------------------------------------------------------------------------------------
            List<IconSetListData> finalPackedIconSetData = new List<IconSetListData>();

            for (int i = 1; i < persistentFolderIconsData.iconSetDataList.Count; i++)
            {
                IconSetDataListWrapper iconSet = persistentFolderIconsData.iconSetDataList[i];

                IconSetListData newMainIconSetData = new();
                newMainIconSetData.iconSetName = iconSet.iconSetName;
                newMainIconSetData.iconSetData = new List<Base64IconSetData>();

                for (int i2 = 0; i2 < iconSet.iconSetData.Count; i2++)
                {
                    IconSetData iconSetData = iconSet.iconSetData[i2];

                    Base64IconSetData newBase64IconSetData = new Base64IconSetData();
                    newBase64IconSetData.folderName = iconSetData.folderName;
                    newBase64IconSetData.iconName = iconSetData.icon.name;
                    newBase64IconSetData.iconBase64 = Convert.ToBase64String(ImageConversion.EncodeToPNG(iconSetData.icon));

                    newMainIconSetData.iconSetData.Add(newBase64IconSetData);
                }
                finalPackedIconSetData.Add(newMainIconSetData);
            }
            // ------------------------------------------------ Save Icon Sets ------------------------------------------------------------------------------------------------


            // ----------------------------------------------- Save Icons ------------------------------------------------------------------------------------------------------
            List<JsonTextureData> finalPackedJsonList = new List<JsonTextureData>();


            // If icon sets are disabled user's icons is in the main guidTextureList
            if (persistentFolderIconsData.currentIconSetIndex == 0)
            {
                for (int i = 0; i < persistentFolderIconsData.guidTextureList.Count; i++)
                {
                    string guid = persistentFolderIconsData.guidTextureList[i].guid;
                    TextureData textureData = persistentFolderIconsData.guidTextureList[i].textureData;

                    if (textureData.emptyFolderTexture != null)
                    {
                        if (!textureData.emptyFolderTexture.isReadable)
                        {
                            Debug.LogWarning($"{textureData.emptyFolderTexture.name} is not readable! Skipping it. Please make sure every texture is readable before saving them!");

                            continue;
                        }
                        else if (!textureData.folderTexture.isReadable)
                        {
                            Debug.LogWarning($"{textureData.folderTexture.name} is not readable! Skipping it. Please make sure every texture is readable before saving them!");

                            continue;
                        }
                    }
                    else if (textureData.customTexture != null)
                    {
                        if (!textureData.customTexture.isReadable)
                        {
                            Debug.LogWarning($"{textureData.customTexture.name} is not readable! Skipping it. Please make sure every texture is readable before saving them!");

                            continue;
                        }
                    }
                    string emptyFolderBase64String = "";
                    string folderTextureBase64String = "";
                    string customTextureBase64String = "";

                    // Guid
                    string folderName = AssetDatabase.GUIDToAssetPath(guid);


                    if (textureData.emptyFolderTexture != null)
                    {
                        emptyFolderBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.emptyFolderTexture));
                    }
                    if (textureData.folderTexture != null)
                    {
                        folderTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.folderTexture));
                    }
                    if (textureData.customTexture != null)
                    {
                        customTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.customTexture));
                    }

                    JsonTextureData packedJsonData = new JsonTextureData();

                    if (textureData.emptyFolderTexture != null)
                    {
                        packedJsonData.emptyFolderTextureName = textureData.emptyFolderTexture.name;
                        packedJsonData.folderTextureName = textureData.folderTexture.name;
                    }
                    if (textureData.customTexture != null)
                        packedJsonData.customTextureName = textureData.customTexture.name;

                    packedJsonData.folderName = folderName;
                    packedJsonData.color = new Vector4(textureData.color.r, textureData.color.g, textureData.color.b, textureData.color.a);

                    packedJsonData.emptyFolderTextureBase64 = emptyFolderBase64String;
                    packedJsonData.folderTextureBase64 = folderTextureBase64String;
                    packedJsonData.customTextureBase64 = customTextureBase64String;

                    finalPackedJsonList.Add(packedJsonData);
                }
            }
            // If icon sets are enabled user's icons is in the temp guid list
            else
            {
                for (int i = 0; i < persistentFolderIconsData.tempGuidTextureListForIconSets.Count; i++)
                {
                    string guid = persistentFolderIconsData.tempGuidTextureListForIconSets[i].guid;
                    TextureData textureData = persistentFolderIconsData.tempGuidTextureListForIconSets[i].textureData;

                    if (textureData.emptyFolderTexture != null)
                    {
                        if (!textureData.emptyFolderTexture.isReadable)
                        {
                            Debug.LogWarning($"{textureData.emptyFolderTexture.name} is not readable! Skipping it. Please make sure every texture is readable before saving them!");

                            continue;
                        }
                        else if (!textureData.folderTexture.isReadable)
                        {
                            Debug.LogWarning($"{textureData.folderTexture.name} is not readable! Skipping it. Please make sure every texture is readable before saving them!");

                            continue;
                        }
                    }
                    else if (textureData.customTexture != null)
                    {
                        if (!textureData.customTexture.isReadable)
                        {
                            Debug.LogWarning($"{textureData.customTexture.name} is not readable! Skipping it. Please make sure every texture is readable before saving them!");

                            continue;
                        }
                    }

                    string emptyFolderBase64String = "";
                    string folderTextureBase64String = "";
                    string customTextureBase64String = "";

                    // Guid
                    string folderName = AssetDatabase.GUIDToAssetPath(guid);


                    if (textureData.emptyFolderTexture != null)
                    {
                        emptyFolderBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.emptyFolderTexture));
                    }
                    if (textureData.folderTexture != null)
                    {
                        folderTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.folderTexture));
                    }
                    if (textureData.customTexture != null)
                    {
                        customTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.customTexture));
                    }

                    JsonTextureData packedJsonData = new JsonTextureData();

                    if (textureData.emptyFolderTexture != null)
                    {
                        packedJsonData.emptyFolderTextureName = textureData.emptyFolderTexture.name;
                        packedJsonData.folderTextureName = textureData.folderTexture.name;
                    }
                    if (textureData.customTexture != null)
                        packedJsonData.customTextureName = textureData.customTexture.name;

                    packedJsonData.folderName = folderName;
                    packedJsonData.color = new Vector4(textureData.color.r, textureData.color.g, textureData.color.b, textureData.color.a);

                    packedJsonData.emptyFolderTextureBase64 = emptyFolderBase64String;
                    packedJsonData.folderTextureBase64 = folderTextureBase64String;
                    packedJsonData.customTextureBase64 = customTextureBase64String;

                    finalPackedJsonList.Add(packedJsonData);
                }
            }

            ProjectWideFolderIcons projectWideFolderIcons = new ProjectWideFolderIcons();
            projectWideFolderIcons.iconSetListDataList = finalPackedIconSetData;
            projectWideFolderIcons.jsonTextureDataList = finalPackedJsonList;

            JsonHelper.SaveJson<ProjectWideFolderIcons>(selectedFile, projectWideFolderIcons);
            AssetDatabase.Refresh();
            // ----------------------------------------------- Save Icons ------------------------------------------------------------------------------------------------------
        }
    }
}