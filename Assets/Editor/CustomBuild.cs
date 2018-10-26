using System.IO;
using UnityEditor;

public class CustomBuild {

    private static string DEMO_RESOURCES = "DemoRelease/Resources/";
    private static string FULL_RESOURCES = "FullRelease/Resources/";
    private static string TMP_RESOURCES = "Tmp/Resources/";
    private static string ASSETS = "Assets/Resources/";
    private static string[] REPLACE_FOLDERS = new string[] { "levels", "Images" };

    private static string[] levels = new string[]{"Assets/PerroElectrico.unity", "Assets/Intro.unity", "Assets/Main.unity", "Assets/GameScene.unity"};

    static void BuildLinux() {
        BuildPipeline.BuildPlayer(levels, "Linux", BuildTarget.StandaloneLinux, BuildOptions.None); 
    }

    private static void Move(string src, string dst)
    {
        System.Console.WriteLine("Moving resources from {0} to {1}", src, dst);
        foreach (var folder in REPLACE_FOLDERS)
        {
            System.Console.WriteLine("{0}{1} -> {2}{1}", src, folder, dst);
            Directory.Move(src + folder, dst + folder);
        }
    }

    static void BuildWebDemo() {

        // 1. move current resources to tmp location
        if (!File.Exists(TMP_RESOURCES))
        {
            Directory.CreateDirectory(TMP_RESOURCES);
        }
        Move(ASSETS, TMP_RESOURCES);

        // 2. put demo resources in place
        Move(DEMO_RESOURCES, ASSETS);

        // 3. build
        BuildPipeline.BuildPlayer(levels, "WebPlayerBuild", BuildTarget.WebGL, BuildOptions.None);

        // 4. move resources back to demo dir
        Move(ASSETS, DEMO_RESOURCES);

        // 5. move back tmp resources in place
        Move(TMP_RESOURCES, ASSETS);
    }

    static void BuildWebFull()
    {
        if (!File.Exists(TMP_RESOURCES))
        {
            Directory.CreateDirectory(TMP_RESOURCES);
        }

        // 1. move current resources to tmp location
        Move(ASSETS, TMP_RESOURCES);

        // 2. put demo resources in place
        Move(FULL_RESOURCES, ASSETS);

        // 3. build
        BuildPipeline.BuildPlayer(levels, "WebPlayerBuild", BuildTarget.WebGL, BuildOptions.None);

        // 4. move resources back to demo dir
        Move(ASSETS, FULL_RESOURCES);

        // 5. move back tmp resources in place
        Move(TMP_RESOURCES, ASSETS);
    }

    static void BuildAll() {
        BuildWebDemo();
        BuildWebFull();
        BuildLinux();
    }
}
