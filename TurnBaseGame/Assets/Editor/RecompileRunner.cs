using UnityEditor;

public static class RecompileRunner
{
    public static void Run()
    {
        AssetDatabase.ImportAsset("Assets/Scripts/Game/ActionEndGame.cs", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Scripts/UI/GameResultPresentation.cs", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
