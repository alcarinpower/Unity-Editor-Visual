using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeDestroyer.Editor
{
    public static class GlobalVariables
    {
        // Project Paths
        public const string NickName = "Code Destroyer";
        public const string DomainName = "com.codedestroyer";
        public const string UnityEditorVisualPackageName = "com.codedestroyer.unityeditorvisual";
        public const string ProjectsPath = "Packages/com.codedestroyer.unityeditorvisual/Editor/Projects";
        public const string ProjectManagerPath = "Packages/com.codedestroyer.unityeditorvisual/Editor/Project Management";

        public const string FolderIconsPath = ProjectsPath + "/Folder Icons";


        // Custom Editor GUIStyle Names
        public const string HeaderLabelGUIStyleName = "Header Label";
        public const string FoldoutHeaderGUIStyleName = "Foldout Header";

        // Project Variables
        public static readonly string ProjectTempDirectoryPath = Path.GetDirectoryName(Application.dataPath) + Path.DirectorySeparatorChar + DomainName;
        public static readonly string ProjectTempInstalledFilePath = ProjectTempDirectoryPath + Path.DirectorySeparatorChar + "EditorVisual" + ".installed";


        private static readonly Color defaultLineDarkColor = new Color(0.1215686f, 0.1215686f, 0.1215686f, 1f);
        private static readonly Color defaultLineWhiteColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        public static Color DefaultLineColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return defaultLineDarkColor;
                }
                else
                {
                    return defaultLineWhiteColor;
                }
            }
        }

        private static readonly Color defaultBackgroundDarkColor = new Color(0.2352941f, 0.2352941f, 0.2352941f, 1f);
        private static readonly Color defaultBackgroundWhiteColor = new Color(0.7843138f, 0.7843138f, 0.7843138f, 1f);
        public static Color DefaultBackgroundColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return defaultBackgroundDarkColor;
                }
                else
                {
                    return defaultBackgroundWhiteColor;
                }
            }
        }

    }
}
