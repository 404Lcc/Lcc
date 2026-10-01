using System.IO;
using LccHotfix;
using UnityEditor.AssetImporters;

namespace LccHotfix.Editor.KaAudio
{
    [ScriptedImporter(1, "kab")]
    public class AudioBankConfigImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = AudioConfigBase.Deserialize<AudioBankConfig>(File.ReadAllText(ctx.assetPath));
            var path = Path.GetFileName(ctx.assetPath);
            ctx.AddObjectToAsset(path, asset);
        }
    }
}