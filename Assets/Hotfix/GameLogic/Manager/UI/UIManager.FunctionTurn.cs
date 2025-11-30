using cfg;
using System.Collections.Generic;

namespace LccHotfix
{
    public partial class UIManager : Module
    {
        /// <summary>
        /// 跳转至指定UI，校验功能是否开启
        /// </summary>
        /// <param name="gotoID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool JumpToPanelByID(int gotoID, params object[] args)
        {
            var config = Main.ConfigService.Tables.TBEmptyGoTo.Get(gotoID);
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

            int procedure = config.ProcedureID;

            TurnNode turn = new TurnNode
            {
                nodeName = config.PanelName,
                nodeParam = nowArg,
                nodeType = (NodeType)config.PanelType
            };

            ChangePanelNode(turn);

            var curState = (int)Main.ProcedureService.CurState;
            //同流程跳转
            if ((curState & procedure) > 0)
            {
                if (OpenSpecialPanel(turn))
                {
                    return true;
                }

                if (turn.nodeType == NodeType.ROOT)
                {
                    Main.WindowService.OpenRoot(turn.nodeName, turn.nodeParam);
                }
                else
                {
                    Main.WindowService.OpenWindow(turn.nodeName, turn.nodeParam);
                }

                return true;
            }

            return JumpPanelCrossProcedure(procedure, turn);

        }

        /// <summary>
        /// 跨流程跳转
        /// </summary>
        /// <param name="procedure"></param>
        /// <returns></returns>
        private bool JumpPanelCrossProcedure(int procedure, TurnNode turn)
        {
            //目前只能跳主流程
            if ((procedure & (int)ProcedureType.Main) > 0)
            {
                Main.ProcedureService.GetProcedure(ProcedureType.Main.ToInt()).turnNode = turn;
                Main.ProcedureService.ChangeProcedure(ProcedureType.Main.ToInt());
                return true;
            }

            return false;
        }

        /// <summary>
        /// 调整界面和参数
        /// </summary>
        /// <param name="turnNode"></param>
        private void ChangePanelNode(TurnNode turnNode)
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
        public bool OpenSpecialPanel(TurnNode turnNode)
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
            }

            return true;
        }
    }
}