using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeDestroyer._Editor.EditorVisual
{

    // We need any type of persistent data that will be remain same between unity sessions Alternatively we can use PlayerPrefs, JsonUtility
    // but these would be horrible way to because we are using textures continually
    [Serializable]
    internal class PersistentFolderIconsData : ScriptableObject
    {
        // Main guidTexture Data
        [SerializeField] internal List<GUIDTextureData> guidTextureList = new List<GUIDTextureData>();
        [SerializeField] internal List<GUIDTextureData> tempGuidTextureListForIconSets = new List<GUIDTextureData>();
        [SerializeField] internal List<FolderEmptinessData> folderEmptinessList = new List<FolderEmptinessData>();
        [SerializeField] internal List<string> allFoldersPathList = new List<string>();
        // Icon sets
        [SerializeField] internal List<IconSetDataListWrapper> iconSetDataList = new();
        [SerializeField] internal int currentIconSetIndex;


        [SerializeField] internal Texture2D emptyDefaultFolderIcon;
        [SerializeField] internal Texture2D defaultFolderIcon;

        [SerializeField] internal Texture2D buttonBackgroundTexture;
        [SerializeField] internal Texture2D buttonHoverTexture;

        [SerializeField] internal bool packageIsInstalledLocally;
    }
}