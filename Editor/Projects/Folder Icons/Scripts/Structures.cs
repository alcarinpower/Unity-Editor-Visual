using System;
using UnityEngine;
using System.Collections.Generic;

namespace CodeDestroyer.Editor.EditorVisual
{

    [Serializable]
    internal class FolderEmptinessData
    {
        [SerializeField] internal string folderPath;
        [SerializeField] internal bool isFolderEmpty;
    }

    [Serializable]
    internal struct TextureData
    {
        [SerializeField] internal Color color;
        [SerializeField] internal Texture2D emptyFolderTexture;
        [SerializeField] internal Texture2D folderTexture;

        [SerializeField] internal Texture2D customTexture;

        internal void Clear()
        {
            color = Color.clear;
            emptyFolderTexture = null;
            folderTexture = null;
            customTexture = null;
        }

    }


    [Serializable]
    internal struct GUIDTextureData
    {
        [SerializeField] internal string guid;
        [SerializeField] internal TextureData textureData;

        internal void Clear()
        {
            guid = "";
            textureData.Clear();
        }
    }






    [Serializable]
    internal sealed class IconSetData
    {
        [SerializeField] internal string folderName;
        [SerializeField] internal Texture2D icon;
    }


    // This is for serializable list
    [Serializable]
    internal sealed class IconSetDataListWrapper
    {
        [SerializeField] internal string iconSetName;
        [SerializeField] internal List<IconSetData> iconSetData;
    }



    [Serializable]
    internal sealed class HeaderContents
    {
        // Unity inspector header variables
        [SerializeField] internal GUIStyle headerIconGUIStyle;
        [SerializeField] internal GUIContent resetButtonGUIContent;
        [SerializeField] internal GUIContent openButton;
    }




    // Icon structure related to saving data with json
    [Serializable]
    internal struct JsonTextureData
    {
        [SerializeField] internal string folderName;
        [SerializeField] internal Vector4 color;

        [SerializeField] internal string emptyFolderTextureName;
        [SerializeField] internal string folderTextureName;
        [SerializeField] internal string customTextureName;

        [SerializeField] internal string emptyFolderTextureBase64;
        [SerializeField] internal string folderTextureBase64;
        [SerializeField] internal string customTextureBase64;

    }


    // Icon set structures related to saving data with json
    [Serializable]
    internal struct IconSetListData
    {
        [SerializeField] internal string iconSetName;
        [SerializeField] internal List<Base64IconSetData> iconSetData;
    }

    [Serializable]
    internal struct Base64IconSetData
    {
        [SerializeField] internal string folderName;
        [SerializeField] internal string iconName;
        [SerializeField] internal string iconBase64;
    }


    [Serializable]
    internal struct ProjectWideFolderIcons
    {
        [SerializeField] internal List<IconSetListData> iconSetListDataList;
        [SerializeField] internal List<JsonTextureData> jsonTextureDataList;
    }
}