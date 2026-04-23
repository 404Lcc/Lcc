using System.Xml;

namespace LccHotfix
{
    /// <summary>
    /// CustomLogic相关一些调试、辅助代码
    /// </summary>
    public static class CLHelper
    {
        public static void AssertBreak()
        {
            LogWrapper.LogError("KaHotUpdate.CoreGameLogic Has ERROR! ");
            UnityEngine.Debug.Break();
        }

        public static bool Assert(bool condition, object logMsg = null)
        {
            if (condition)
                return true;
            if (logMsg != null)
            {
                LogWrapper.LogError(logMsg.ToString());
            }

            AssertBreak();
            return false;
        }

        /// Node Helper
        public static void LogError(this CustomNode node, string logMsg)
        {
            int id = node.GenInfo.LogicConfigID;
            LogWrapper.LogError($"Logic[ {id} ]({node.CreationIndex}) : {logMsg}");
        }

        public static void LogInfo(this CustomNode node, string logMsg)
        {
            int id = node.GenInfo.LogicConfigID;
            LogWrapper.LogInfo($"Logic[ {id} ]({node.CreationIndex}): {logMsg}");
        }

        public static bool IsNodeCanStop(this CustomNode node)
        {
            if (node != null && node is INeedStopCheck check)
            {
                return check.CanStop();
            }

            return true;
        }
    }
}