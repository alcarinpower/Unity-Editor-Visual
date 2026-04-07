using UnityEditor;
using UnityEngine;

namespace CodeDestroyer.Editor.EditorVisual
{
    internal static class ProjectConstants
    {
        internal const string packageFolderPath = GlobalVariables.ProjectsPath + "/Folder Icons";
        internal const string defaultUnityIconsPath = packageFolderPath + "/Default Icons/Default Unity Icons";

        internal const string dependencyPackages = "com.codedestroyer.editortools";
        internal const string dataPath = packageFolderPath + "/Data";


        internal const string persistentDataPath = dataPath + "/FolderIconsData.asset";
        // -----------------------------------------------------------------


        // Icon paths from "/Assets/Plugins/Folder-Icons"
        internal const string Plugins = "Plugins";
        internal const string UnityFolderIcons = "Folder-Icons";

        // Names for color folders
        internal const string EmptyColorFolderIcons = "Empty Color Folder Icons";
        internal const string ColorFolderIcons = "Color Folder Icons";
        internal const string LoadedIconSets = "Loaded Icon Sets";
        internal const string LoadedIcons = "Loaded Icons";
        // -----------------------------------------------------------------



        internal static readonly string emptyIconFolderPath = dataPath + "/" + EmptyColorFolderIcons;
        internal static readonly string iconFolderPath = dataPath + "/" + ColorFolderIcons;
        internal static readonly string loadedIconSetPath = dataPath + "/" + LoadedIconSets;
        internal static readonly string loadedIconsPath = dataPath + "/" + LoadedIcons;


        // Default icon names
        internal const string darkEmptyFolderName = "/d_DarkEmptyFolder Icon.png";
        internal const string darkFolderName = "/d_DarkFolder Icon.png";
        internal const string lightEmptyFolderName = "/d_LightEmptyFolder Icon.png";
        internal const string lightFolderName = "/d_LightFolder Icon.png";

        internal const string darkDefaultButtonName = "/darkDefaultbutton.png";
        internal const string darkHoverButtonName = "/darkHoverbutton.png";

        internal const string lightDefaultButtonName = "/lightDefaultbutton.png";
        internal const string lightHoverButtonName = "/lightHoverbutton.png";


        internal static readonly string darkEmptyFolderPath = defaultUnityIconsPath + darkEmptyFolderName;
        internal static readonly string darkFolderPath = defaultUnityIconsPath + darkFolderName;
        internal static readonly string lightEmptyFolderPath = defaultUnityIconsPath + lightEmptyFolderName;
        internal static readonly string lightFolderPath = defaultUnityIconsPath + lightFolderName;

        // Default unity icons path
        internal static readonly string defaultButtonPath = EditorGUIUtility.isProSkin ? defaultUnityIconsPath + darkDefaultButtonName : defaultUnityIconsPath + lightDefaultButtonName;
        internal static readonly string hoverButtonPath = EditorGUIUtility.isProSkin ? defaultUnityIconsPath + darkHoverButtonName : defaultUnityIconsPath + lightHoverButtonName;

        internal static readonly string dynamicDefaultEmptyFolderPath = EditorGUIUtility.isProSkin ? darkEmptyFolderPath : lightEmptyFolderPath;
        internal static readonly string dynamicDefaultFolderPath = EditorGUIUtility.isProSkin ? darkFolderPath : lightFolderPath;



        // Default unity buttons Hover Color
        internal static readonly Color32 darkHoverSkin = new Color32(103, 103, 103, 255);
        internal static readonly Color32 lightDefaultSkin = new Color32(203, 203, 203, 255);
        internal static readonly Color32 lightHoverSkin = new Color32(239, 239, 239, 255);


        // Project Background Color
        internal static readonly Color32 darkBackgroundSkin = new Color32(51, 51, 51, 255);
        internal static readonly Color32 lightBackgroundSkin = new Color32(190, 190, 190, 255);

        // Dynamically updated dark theme or light theme variables (Pro skin or not)
        internal static readonly Color32 buttonHoverColor = EditorGUIUtility.isProSkin ? darkHoverSkin : lightHoverSkin;
        internal static readonly Color32 projectBackgroundColor = EditorGUIUtility.isProSkin ? darkBackgroundSkin : lightBackgroundSkin;


        // Folder Color
        internal static readonly Color32 darkFolderSkin = new Color32(194, 194, 194, 255);
        internal static readonly Color32 lightFolderSkin = new Color32(86, 86, 86, 255);
    }
}