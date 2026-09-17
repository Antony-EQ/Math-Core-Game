using UnityEditor;

// Any texture placed under Assets/Art/UI is guaranteed to import as a UI-ready Sprite,
// regardless of the project's default texture import settings.
public class UITexturePostprocessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').Contains("/Art/UI/"))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        // No forzamos spriteImportMode: si ya se recorto como "Multiple" en el Sprite Editor
        // (por ejemplo un sprite sheet en una carpeta "recursos"), no lo pisamos en cada reimport.
        if (importer.spriteImportMode == SpriteImportMode.None)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
        }
        importer.mipmapEnabled = false;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
    }
}
