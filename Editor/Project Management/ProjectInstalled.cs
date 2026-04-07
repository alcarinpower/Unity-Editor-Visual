using System.IO;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager;
using System.Linq;
using CodeDestroyer.Editor;
using UnityEngine;

[InitializeOnLoad]
public class ProjectInstalled
{

    static ProjectInstalled()
    {
        CreateInstalledTempFile();

        Events.registeringPackages += DeleteInstalledTempFile;
    }

    private static void CreateInstalledTempFile()
    {
        if (!Directory.Exists(GlobalVariables.ProjectTempDirectoryPath))
        {
            Directory.CreateDirectory(GlobalVariables.ProjectTempDirectoryPath);
        }

        if (!File.Exists(GlobalVariables.ProjectTempInstalledFilePath))
        {
            PackageSource packageInfo = PackageInfo.FindForPackageName(GlobalVariables.UnityEditorVisualPackageName).source;

            if (packageInfo == PackageSource.Embedded || packageInfo == PackageSource.Local || packageInfo == PackageSource.LocalTarball)
            {
                File.WriteAllText(GlobalVariables.ProjectTempInstalledFilePath, "Already Embedded Package!");
            }
        }
    }


    private static void DeleteInstalledTempFile(PackageRegistrationEventArgs args)
    {
        PackageInfo packageInfo = args.removed.First((x) => x.name == GlobalVariables.UnityEditorVisualPackageName);
        Debug.Log("Remove Editor Visual again in order to remove it!");
        if (packageInfo != null)
        {
            SessionState.EraseBool("isCheckedFolderEmptiness");

            File.Delete(GlobalVariables.ProjectTempInstalledFilePath);
            Events.registeringPackages -= DeleteInstalledTempFile;
        }
    }
}