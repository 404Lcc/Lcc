using cfg;
using System.Collections.Generic;

namespace LccHotfix
{
    internal partial class WindowManager : Module
    {
        /// <summary>
        /// 跳转至指定UI，校验功能是否开启
        /// </summary>
        /// <param name="gotoID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool JumpToWindowByID(int gotoID, params object[] args)
        {
            var config = ConfigManager.Instance.Tables.TBEmptyGoTo.Get(gotoID);
            if (config == null)
                return false;

            if (!CheckFuncIsOpenAndShowTip(config))
                return false;

            var list = new List<object>();

            // 参数顺序
            // param3  >  param1  > param2  >  args

            if (!string.IsNullOrEmpty(config.Param3))
            {
                list.Add(config.Param3);
            }

            if (config.Param1 != -1)
            {
                list.Add(config.Param1);

                if (config.Param2 != -1)
                {
                    list.Add(config.Param2);
                }
            }

            if (args != null)
            {
                list.AddRange(args);
            }
            object[] nowArg = list.Count > 0 ? list.ToArray() : null;

            int scene = config.SceneID;

            WNode.TurnNode turn = new WNode.TurnNode
            {
                nodeName = config.WindowName,
                nodeParam = nowArg,
                nodeType = (NodeType)config.WindowType
            };

            ChangeWindowNode(turn);

            var curState = (int)SceneManager.Instance.curState;
            //同场景跳转
            if ((curState & scene) > 0)
            {
                if (OpenSpecialWindow(turn))
                {
                    return true;
                }
                if (turn.nodeType == NodeType.ROOT)
                {
                    OpenRoot(turn.nodeName, turn.nodeParam);
                }
                else
                {
                    OpenWindow(turn.nodeName, turn.nodeParam);
                }
                return true;
            }
            return JumpWindowCrossScene(scene, turn);

        }

        /// <summary>
        /// 跨场景跳转
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="panelName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool JumpWindowCrossScene(int scene, WNode.TurnNode turn)
        {
            LoadSceneHandler handler = null;

            //目前只能跳主场景
            if ((scene & (int)SceneType.Main) > 0)
            {
                handler = SceneManager.Instance.GetScene(SceneType.Main);
                handler.turnNode = turn;
                SceneManager.Instance.ChangeScene(handler);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 调整界面和参数
        /// </summary>
        /// <param name="turnNode"></param>
        private void ChangeWindowNode(WNode.TurnNode turnNode)
        {
            switch (turnNode.nodeName)
            {
                default:
                    break;
            }
        }
        /// <summary>
        /// 有些面板不能直接打开，打开方式比较特殊
        /// </summary>
        /// <param name="turnNode"></param>
        /// <returns></returns>
        public bool OpenSpecialWindow(WNode.TurnNode turnNode)
        {
            switch (turnNode.nodeName)
            {
                default:
                    break;
            }
            return false;
        }

        private bool CheckFuncIsOpenAndShowTip(EmptyGoTo config)
        {
            if (config == null)
                return false;
            if (config.FuncType == 0)
            {
                //判断功能是否开启
                if (!FunctionOpenManager.Instance.IsFunctionOpenedAndShowTips(config.FuncId))
                    return false;
            }
            return true;
        }
    }
}