using UnityEditor.PackageManager;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using System.Linq;
using System.IO;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Debug = UnityEngine.Debug;

namespace CodeDestroyer._Editor.EditorVisual
{
    public class EmbedEditorVisualProject
    {
        private static ListRequest listRequest;
        private static EmbedRequest Request;
        private static readonly string thisScriptPath = Path.GetFullPath(GlobalVariables.ProjectManagerPath) + Path.DirectorySeparatorChar + "EmbedEditorVisualProject.cs";


        [InitializeOnLoadMethod]
        private static void InitEmbeddingEditorVisualProject()
        {
            if (File.Exists(GlobalVariables.ProjectTempInstalledFilePath)) return;

            PackageSource packageInfo = PackageInfo.FindForPackageName(GlobalVariables.UnityEditorVisualPackageName).source;

            if (packageInfo != PackageSource.Embedded && packageInfo != PackageSource.Local && packageInfo != PackageSource.LocalTarball)
            {
                Debug.Log(GlobalVariables.UnityEditorVisualPackageName + " is embedding now!");
                TrySearchEmbeddedPackage();
            }
            else
            {
                File.Delete(thisScriptPath);
                File.Delete(thisScriptPath + ".meta");
                AssetDatabase.Refresh();
            }
        }

        private static void TrySearchEmbeddedPackage()
        {
            listRequest = Client.List();
            EditorApplication.update += ListProgress;
        }
        static void ListProgress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {

                    if (listRequest.Result.Any(pkg => pkg.name == GlobalVariables.UnityEditorVisualPackageName))
                    {
                        EmbedProject(GlobalVariables.UnityEditorVisualPackageName);
                    }
                }
                else
                {
                    Debug.Log(listRequest.Error.message);
                }

                EditorApplication.update -= ListProgress;
            }
        }

        static void EmbedProject(string inTarget)
        {
            Request = Client.Embed(inTarget);
            EditorApplication.update += EmbedProgress;
        }

        static void EmbedProgress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                {
                    File.Delete(thisScriptPath);
                    File.Delete(thisScriptPath + ".meta");
                    AssetDatabase.Refresh();
                }
                else if (Request.Status >= StatusCode.Failure)
                {

                }

                EditorApplication.update -= EmbedProgress;
            }
        }
    }
}