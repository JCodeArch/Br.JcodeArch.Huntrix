using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
namespace HuntrX.Editor.Optimization
{
    public static class SpriteAtlasPipeline
    {
        private const string ProfilePath="Assets/Data/Optimization/AtlasBuildProfile_Prototype.asset";
        [MenuItem("HUNTR/X/Optimization/Build authored sprite atlas")]
        public static void Build()
        {
            AtlasBuildProfile profile=AssetDatabase.LoadAssetAtPath<AtlasBuildProfile>(ProfilePath);
            if(profile==null||!AssetDatabase.IsValidFolder(profile.sourceFolder)||
                !profile.outputAtlas.StartsWith("Assets/Resources/Art/",StringComparison.Ordinal)||profile.outputAtlas.Contains("..")||
                !profile.outputAtlas.EndsWith(".spriteatlas",StringComparison.Ordinal)||
                profile.padding<2||profile.padding>16||profile.maximumTextureSize<256||profile.maximumTextureSize>4096||
                !Mathf.IsPowerOfTwo(profile.maximumTextureSize))throw new InvalidOperationException("Invalid authored atlas profile.");
            string[] paths=AssetDatabase.FindAssets("t:Sprite",new[]{profile.sourceFolder}).Select(AssetDatabase.GUIDToAssetPath).Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToArray();
            Sprite[] sprites=paths.SelectMany(AssetDatabase.LoadAllAssetsAtPath).OfType<Sprite>().ToArray();
            if(sprites.Length==0)throw new InvalidOperationException("No authored imported sprites are available; no empty atlas success is reported.");
            SpriteAtlas atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(profile.outputAtlas);
            if(atlas==null)
            {
                string parent=Path.GetDirectoryName(profile.outputAtlas);if(!Directory.Exists(parent))Directory.CreateDirectory(parent);
                AssetDatabase.Refresh();atlas=new SpriteAtlas();AssetDatabase.CreateAsset(atlas,profile.outputAtlas);
            }
            UnityEngine.Object[] old=atlas.GetPackables();if(old.Length>0)atlas.Remove(old);
            atlas.Add(sprites.Cast<UnityEngine.Object>().ToArray());
            SpriteAtlasPackingSettings packing=atlas.GetPackingSettings();packing.padding=profile.padding;
            packing.enableRotation=false;packing.enableTightPacking=false;atlas.SetPackingSettings(packing);
            SpriteAtlasTextureSettings texture=atlas.GetTextureSettings();texture.readable=false;texture.generateMipMaps=false;
            texture.filterMode=profile.filterMode;texture.sRGB=true;atlas.SetTextureSettings(texture);
            atlas.SetIncludeInBuild(true);
            ConfigurePlatform(atlas,"DefaultTexturePlatform",profile.maximumTextureSize,TextureImporterFormat.Automatic,false);
            ConfigurePlatform(atlas,"Android",profile.maximumTextureSize,TextureImporterFormat.ASTC_6x6,true);
            ConfigurePlatform(atlas,"iPhone",profile.maximumTextureSize,TextureImporterFormat.ASTC_6x6,true);
            EditorUtility.SetDirty(atlas);AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget,false);
            AssetDatabase.SaveAssets();
            Debug.Log("Packed authored sprite atlas. Device draw calls, overdraw and texture memory still require measured validation.");
        }
        private static void ConfigurePlatform(SpriteAtlas atlas,string name,int size,TextureImporterFormat format,bool overridden)
        {
            TextureImporterPlatformSettings platform=atlas.GetPlatformSettings(name);platform.name=name;
            platform.maxTextureSize=size;platform.overridden=overridden;platform.format=format;atlas.SetPlatformSettings(platform);
        }
    }
}
