using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeDestroyer.Editor.EditorVisual
{
    public sealed class FolderIconsPostproccesor : AssetPostprocessor
    {
        // Handle updating dictionary that checks if a folder is empty or not and Handle applying icons to the newly created folders with icon sets
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                IconManager.InitializeFolderIcons();
            }
            if (importedAssets.Length > 0)
            {
                for (int i = 0; i < importedAssets.Length; i++)
                {
                    string importedAsset = importedAssets[i];
                    if (!IconManager.persistentFolderIconsData.allFoldersPathList.Contains(importedAsset))
                    {
                        IconManager.persistentFolderIconsData.allFoldersPathList.Add(importedAsset);
                    }

                    string parentAsset = Path.GetDirectoryName(importedAsset).Replace("\\", "/");

                    UtilityFunctions.UpdateFolderEmptyDict(parentAsset, ref IconManager.isFolderFilledDict);

                    if (!AssetDatabase.IsValidFolder(importedAsset)) continue;

                    
                    UtilityFunctions.UpdateCurrentFolderIconWithIconSet(importedAsset);
                }

                if (IconManager.isFolderFilledDict.Count > 0)
                {
                    UtilityFunctions.ReDrawFolders();
                }
            }
            if (deletedAssets.Length > 0)
            {
                for (int i = 0; i < deletedAssets.Length; i++)
                {
                    string deletedAsset = deletedAssets[i];

                    if (deletedAsset == ProjectConstants.persistentDataPath)
                    {
                        UtilityFunctions.CheckAndCreateIconFolders();
                        Debug.LogWarning("You can't delete FolderIconsData.asset, Folder Icons project is depends on it! If you dont wan't it you can remove it.");
                    }
                    if (IconManager.persistentFolderIconsData.allFoldersPathList.Contains(deletedAsset))
                    {
                        IconManager.persistentFolderIconsData.allFoldersPathList.Remove(deletedAsset);
                    }

                    string deletedAssetParent = null;
                    try
                    {
                        deletedAssetParent = Path.GetDirectoryName(deletedAsset).Replace("\\", "/");
                    }
                    catch
                    {
                        return;
                    }

                    UtilityFunctions.UpdateFolderEmptyDict(deletedAssetParent, ref IconManager.isFolderFilledDict);

                    int folderEmptinessIndex = IconManager.persistentFolderIconsData.folderEmptinessList.FindIndex(x => x.folderPath == deletedAssetParent);


                    bool isFolderFilled = UtilityFunctions.IsFolderFilled(ref deletedAsset);
                    if (folderEmptinessIndex != -1)
                    {
                        IconManager.persistentFolderIconsData.folderEmptinessList[folderEmptinessIndex].isFolderEmpty = isFolderFilled;
                    }
                    IconManager.isFolderFilledDict[deletedAssetParent] = isFolderFilled;

                    string guid = AssetDatabase.AssetPathToGUID(deletedAsset);

                    for (int guidTextureIndex = 0; guidTextureIndex < IconManager.persistentFolderIconsData.guidTextureList.Count; guidTextureIndex++)
                    {
                        if (IconManager.persistentFolderIconsData.guidTextureList[guidTextureIndex].guid == guid)
                        {
                            IconManager.persistentFolderIconsData.guidTextureList.RemoveAt(guidTextureIndex);
                            break;
                        }
                    }

                }

                if (IconManager.isFolderFilledDict.Count > 0)
                {
                    UtilityFunctions.ReDrawFolders();
                }
            }

            if (movedAssets.Length > 0)
            {
                for (int i = 0; i < movedAssets.Length; i++)
                {
                    string movedAsset = movedAssets[i];
                    string movedParentAsset = Path.GetDirectoryName(movedAsset).Replace("\\", "/");

                    UtilityFunctions.UpdateFolderEmptyDict(movedParentAsset, ref IconManager.isFolderFilledDict);
                    if (!AssetDatabase.IsValidFolder(movedParentAsset)) continue;

                    UtilityFunctions.UpdateCurrentFolderIconWithIconSet(movedAsset);
                }

                if (IconManager.isFolderFilledDict.Count > 0)
                {
                    UtilityFunctions.ReDrawFolders();
                }
            }
            if (movedFromAssetPaths.Length > 0)
            {

                for (int i = 0; i < movedFromAssetPaths.Length; i++)
                {
                    string movedFromAssetPath = movedFromAssetPaths[i];
                    if (IconManager.persistentFolderIconsData.allFoldersPathList.Contains(movedFromAssetPath))
                    {
                        IconManager.persistentFolderIconsData.allFoldersPathList.Remove(movedFromAssetPath);
                    }

                    string parentMovedAsset = Path.GetDirectoryName(movedFromAssetPath).Replace("\\", "/");

                    UtilityFunctions.UpdateFolderEmptyDict(parentMovedAsset, ref IconManager.isFolderFilledDict);
                }

                if (IconManager.isFolderFilledDict.Count > 0)
                {
                    UtilityFunctions.ReDrawFolders();
                }
            }
        }
    }
}
