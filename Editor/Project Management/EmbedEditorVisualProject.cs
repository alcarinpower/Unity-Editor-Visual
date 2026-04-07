using UnityEditor.PackageManager;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using System.Linq;
using System.IO;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Debug = UnityEngine.Debug;

namespace CodeDestroyer.Editor.EditorVisual
{
    public class EmbedEditorVisualProject
    {
        private static ListRequest listRequest;
        private static EmbedRequest Request;
        private static readonly string thisScriptPath = Path.GetFullPath(GlobalVariables.ProjectManagerPath) + Path.DirectorySeparatorChar + "EmbedEditorVisualProject.cs";


        [InitializeOnLoadMethod]
        private static void InitEmbeddingEditorVisualProject()
        {
            Debug.Log(GlobalVariables.ProjectTempInstalledFilePath);
            if (File.Exists(GlobalVariables.ProjectTempInstalledFilePath)) return;

            PackageInfo packageInfo = PackageInfo.FindForPackageName(GlobalVariables.UnityEditorVisualPackageName);
            PackageSource packageSource = PackageSource.Unknown;

            if (packageInfo != null)
            {
                packageSource = packageInfo.source;
            }

            if (packageSource != PackageSource.Embedded && packageSource != PackageSource.Local && packageSource != PackageSource.LocalTarball)
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
            Debug.Log("Inside TrySearchEmbeddedPackage Function");
            listRequest = Client.List();
            EditorApplication.update += ListProgress;
        }
        static void ListProgress()
        {
            Debug.Log("Inside ListProgress Function");

            if (listRequest.IsCompleted)
            {
                Debug.Log("Inside ListProgress Function " + listRequest.Result);

                if (listRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Inside ListProgress Function " + listRequest.Status);

                    if (listRequest.Result.Any(pkg => pkg.name == GlobalVariables.UnityEditorVisualPackageName))
                    {
                        Debug.Log("Package Found");

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
            Debug.Log("EmbedProject ");

            Request = Client.Embed(inTarget);
            EditorApplication.update += EmbedProgress;
        }

        static void EmbedProgress()
        {
            Debug.Log("Inside EmbedProgress");

            if (Request.IsCompleted)
            {
                Debug.Log("iscompleted embedprogress");

                if (Request.Status == StatusCode.Success)
                {
                    Debug.Log(Request.Status);

                    File.Delete(thisScriptPath);
                    File.Delete(thisScriptPath + ".meta");
                    AssetDatabase.Refresh();
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    Debug.Log(Request.Status);

                }

                EditorApplication.update -= EmbedProgress;
            }
        }
    }
}