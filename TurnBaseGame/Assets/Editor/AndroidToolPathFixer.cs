using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class AndroidToolPathFixer : IPostGenerateGradleAndroidProject
{
    private const string AndroidDir = @"H:\UnityAndroid6000.6.2f1";

    public int callbackOrder => 0;

    static AndroidToolPathFixer()
    {
        ApplyPaths();
    }

    [MenuItem("Tools/Android/Fix Tool Paths")]
    public static void ApplyPaths()
    {
        string ndk = Path.Combine(AndroidDir, "NDK");
        string sdk = Path.Combine(AndroidDir, "SDK");
        string jdk = Path.Combine(AndroidDir, "OpenJDK");
        string gradle = Path.Combine(AndroidDir, "Gradle");

        if (!Directory.Exists(AndroidDir))
        {
            Debug.LogWarning($"[AndroidToolPathFixer] Directory {AndroidDir} does not exist.");
            return;
        }

        AndroidExternalToolsSettings.jdkRootPath = jdk;
        AndroidExternalToolsSettings.sdkRootPath = sdk;
        AndroidExternalToolsSettings.ndkRootPath = ndk;
        AndroidExternalToolsSettings.Gradle.path = gradle;

        EditorPrefs.SetBool("JdkUseEmbedded", false);
        EditorPrefs.SetString("Jdk17Path", jdk);

        EditorPrefs.SetBool("SdkUseEmbedded", false);
        EditorPrefs.SetString("AndroidSdkRoot", sdk);

        EditorPrefs.SetBool("NdkUseEmbedded", false);
        EditorPrefs.SetString("AndroidNdkRootR27C", ndk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);

        EditorPrefs.SetBool("GradleUseEmbedded", false);
        EditorPrefs.SetString("GradlePath", gradle);

        Debug.Log("[AndroidToolPathFixer] Configured Android SDK/NDK/JDK/Gradle paths successfully.");
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string gradleProjectRoot = Directory.GetParent(path)?.FullName;
        if (string.IsNullOrEmpty(gradleProjectRoot)) return;

        string gradleProperties = Path.Combine(gradleProjectRoot, "gradle.properties");
        if (File.Exists(gradleProperties))
        {
            string content = File.ReadAllText(gradleProperties);
            if (!content.Contains("org.gradle.java.home"))
            {
                content += "\r\norg.gradle.java.home=H:/UnityAndroid6000.6.2f1/OpenJDK\r\n";
                File.WriteAllText(gradleProperties, content);
            }
        }

        string localProperties = Path.Combine(gradleProjectRoot, "local.properties");
        if (File.Exists(localProperties))
        {
            string localContent = File.ReadAllText(localProperties);
            localContent = localContent.Replace("H:\\Unity`s\\6000.6.2f1\\Editor\\Data\\PlaybackEngines\\AndroidPlayer\\SDK", @"H:\UnityAndroid6000.6.2f1\SDK");
            localContent = localContent.Replace("H:/Unity`s/6000.6.2f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK", @"H:/UnityAndroid6000.6.2f1/SDK");
            File.WriteAllText(localProperties, localContent);
        }
    }
}
