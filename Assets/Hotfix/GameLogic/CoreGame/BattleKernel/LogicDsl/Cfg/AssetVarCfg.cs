using HotUpdate.Framework.PbCfg;
using PBConfig;

namespace LccHotfix
{
    public struct AssetVarCfg
    {
        public string Var { get; private set; }
        public uint Tid { get; private set; }

        public AssetVarCfg(string varName)
        {
            Var = varName;
            Tid = 0;
        }

        public AssetVarCfg(uint assetTid)
        {
            Var = null;
            Tid = assetTid;
        }

        public string GetResPath(CustomNode node, bool logError = true)
        {
            if (Tid > 0)
            {
                return GetResPath(node, Tid, logError);
            }

            var path = node.GetVar<string>(Var, null);
            if (path != null)
            {
                return path;
            }

            var itid = node.GetVar<int>(Var, 0);
            if (itid > 0)
            {
                return GetResPath(node, (uint)itid, logError);
            }

            return Var;
        }

        private string GetResPath(CustomNode node, uint tid, bool logError = true)
        {
            var goCfg = PbCfg.GetData<TAssetGameObjectModel>(tid);
            if (goCfg == null)
            {
                if (logError)
                {
                    CLHelper.LogError(node, $"GetResPath assetTid={tid} AssetGameObject 配置有错误，请策划检查");
                }

                return null;
            }

            return goCfg.PathName;
        }
    }
}