using cfg;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class JumpNode
    {
        public string nodePanel;
        public object[] nodeParam;
        public JumpDependNode depend;//依赖只支持一层

        public JumpNode(string nodePanel)
        {
            this.nodePanel = nodePanel;

        }
        public JumpNode(string nodePanel, object[] nodeParam)
        {
            this.nodePanel = nodePanel;
            this.nodeParam = nodeParam;

        }
        public void SetDepend(JumpDependNode depend)
        {
            this.depend = depend;
        }
    }
    public class JumpDependNode
    {
        public string nodePanel;
        public object[] nodeParam;

        public JumpDependNode(string nodePanel)
        {
            this.nodePanel = nodePanel;

        }
    }

    internal class JumpManager : Module
    {
        public static JumpManager Instance { get; } = Entry.GetModule<JumpManager>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        /// <summary>
        /// 跳转至指定UI，校验功能是否开启
        /// </summary>
        /// <param name="gotoID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool JumpToPanleByID(int gotoID, params object[] args)
        {
            var config = ConfigManager.Instance.Tables.TBJump.Get(gotoID);
            if (config == null)
                return false;

            if (!CheckFuncOpen(config))
                return false;

            var list = new List<object>();

            // 参数顺序
            //  param3  >  param1  > param2  >  args

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

            int scene = config.SceneId;

            JumpNode jump = new JumpNode(config.PanelName, nowArg);

            ChangeWindowNode(jump);

            var curState = (int)SceneStateManager.Instance.CurState;
            //同场景跳转
            if ((curState & scene) > 0)
            {
                if (OpenSpecialWindow(jump))
                {
                    return true;
                }
                //var data = new ShowPanelData(false, true, jump.nodeParam, true, false, true);
                //PanelManager.Instance.ShowPanel(jump.nodeType, data);
                return true;
            }

            //todo 依赖的界面现在给不了参数
            jump.SetDepend(new JumpDependNode(config.ScenePanelName));
            return JumpPanelCrossScene(scene, jump);

        }

        private bool CheckFuncOpen(Jump config)
        {
            return true;
        }

        public bool OpenSpecialWindow(JumpNode jumpNode)
        {
            switch (jumpNode.nodePanel)
            {
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// 调整界面和参数
        /// </summary>
        /// <param name="jumpNode"></param>
        private void ChangeWindowNode(JumpNode jumpNode)
        {
            switch (jumpNode.nodePanel)
            {
                default:
                    break;
            }
        }

        /// <summary>
        /// 跨场景跳转
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="panelName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool JumpPanelCrossScene(int scene, JumpNode jumpNode)
        {
            //目前只能跳主场景
            if ((scene & (int)SceneStateType.Main) > 0)
            {
                SceneStateManager.Instance.GetState(SceneStateType.Main).jumpNode = jumpNode;
                SceneStateManager.Instance.ChangeScene(SceneStateType.Main);
                return true;
            }

            return false;
        }

    }
}