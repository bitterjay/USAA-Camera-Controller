using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class NDIPostBuild
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // Windows Standalone
        if (target == BuildTarget.StandaloneWindows64)
        {
            string buildDir = Path.GetDirectoryName(pathToBuiltProject);
            string ndiDllSource = @"/Users/shelbyklein/Unity/PTZCamController/Library/PackageCache/jp.keijiro.klak.ndi@2.1.4/Plugin/Windows/Processing.NDI.Lib.x64.dll";
            string ndiDllDest = Path.Combine(buildDir, "Processing.NDI.Lib.x64.dll");

            if (File.Exists(ndiDllSource))
            {
                File.Copy(ndiDllSource, ndiDllDest, true);
                UnityEngine.Debug.Log("NDI DLL copied to build output.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("NDI DLL not found at: " + ndiDllSource);
            }
        }

        // macOS Standalone
        if (target == BuildTarget.StandaloneOSX)
        {
            // Path to your .app bundle
            string appPath = pathToBuiltProject;
            if (appPath.EndsWith(".app"))
            {
                string frameworksDir = Path.Combine(appPath, "Contents", "Frameworks");
                if (!Directory.Exists(frameworksDir))
                    Directory.CreateDirectory(frameworksDir);

                string ndiDylibSource = "/Users/shelbyklein/Unity/PTZCamController/Library/PackageCache/jp.keijiro.klak.ndi@2.1.4/Plugin/macOS/libndi.dylib"; // Adjust if your NDI dylib is elsewhere
                string ndiDylibDest = Path.Combine(frameworksDir, "libndi.4.dylib");

                if (File.Exists(ndiDylibSource))
                {
                    File.Copy(ndiDylibSource, ndiDylibDest, true);
                    UnityEngine.Debug.Log("NDI dylib copied to app bundle.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("NDI dylib not found at: " + ndiDylibSource);
                }
            }
        }
    }
} 