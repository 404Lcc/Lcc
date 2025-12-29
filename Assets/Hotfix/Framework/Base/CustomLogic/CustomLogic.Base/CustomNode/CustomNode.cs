using System.Collections.Generic;

//CustomLogic中的节点概念，用于组织逻辑对象的结构关联

namespace LccHotfix
{
    //逻辑内流通，接口性质的上下文结构，（不应被修改）
    public struct CustomNodeContext
    {
        public ICustomLogicGenInfo GenInfo;
        public CustomLogic Logic;

        public VarEnv VarEnvImp;

        //模板配置库（静态配置get），支持通过ID复用一整个配置当模板
        public ILogicConfigContainer ConfigContainer;

        //逻辑节点工厂（运行时逻辑节点get）
        public CustomLogicFactory Factory;

        public CustomNodeContext(ICustomLogicGenInfo genInfo, CustomLogic logic, VarEnv varEnvImp, ILogicConfigContainer container, CustomLogicFactory factory)
        {
            GenInfo = genInfo;
            Logic = logic;
            VarEnvImp = varEnvImp;
            ConfigContainer = container;
            Factory = factory;
        }

        public void Clear()
        {
            GenInfo = null;
            Logic = null;
            VarEnvImp = null;
            ConfigContainer = null;
            Factory = null;
        }
    }

    //自定义节点、条件节点、行为节点、结构容器节点，都继承自它
    public class CustomNode : ICustomNode
    {
        private static int sCreationIndexAcc = 0;
        private bool mIsActive = false;
        protected CustomNodeContext mContext;

        public int CreationIndex { get; private set; }

        //运行时变量环境（黑板）
        public VarEnv VarEnvRef => mContext.VarEnvImp;

        //运行时初始数据
        public ICustomLogicGenInfo GenInfo => mContext.GenInfo;

        public CustomLogic RootLogic => mContext.Logic;

        public bool IsInPool { get; private set; } = false;

        public bool IsActive
        {
            get { return mIsActive; }
        }

        public void Construct()
        {
            IsInPool = false;
        }

        public virtual void Destroy()
        {
            IsInPool = true;
            Deactivate();
            mContext.Clear();
        }


        public T GetRootLogic<T>() where T : CustomLogic
        {
            if (mContext.Logic is T theLogic)
            {
                return theLogic;
            }

            CLHelper.LogError(this, $"CustomNode.GetOwnerLogic logic({mContext.Logic.GetType()}) is not {typeof(T)}");
            return null;
        }

        public T GetGenInfo<T>(bool logError = true) where T : ICustomLogicGenInfo
        {
            if (mContext.GenInfo is T theGenInfo)
            {
                return theGenInfo;
            }

            if (logError)
            {
                CLHelper.LogError(this, $"CustomNode.GetGenInfo genInfo({mContext.GenInfo.GetType()}) is not {typeof(T)}");
            }

            return null;
        }

        public virtual void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            CreationIndex = ++sCreationIndexAcc;
            mContext = context;
            Activate();
        }

        /// <summary>
        /// Active概念比较纯粹， 只有IsActive的Node才能够响应外部的输入驱动、通知、查询
        /// </summary>
        public virtual void Activate()
        {
            mIsActive = true;
        }

        public virtual void Deactivate()
        {
            mIsActive = false;
        }

        /// <summary>
        /// Reset的设计语义是：节点内部状态恢复到InitializeNode之后的样子（主要用于可以重复多次执行的Node）
        /// </summary>
        public virtual void Reset()
        {
        }


        public virtual void CollectInterface<T>(ref List<T> interfaceList) where T : class
        {
            T notify = this as T;
            if (notify != null)
            {
                interfaceList.Add(notify);
            }
        }

        public virtual void CollectInterfaceInChildren<T>(ref List<T> interfaceList) where T : class
        {
            //如果有子节点，重载实现这个方法
        }

        /// <summary>
        /// 遍历收集所有interface
        /// </summary>
        /// <param name="interfaceList"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        protected static void TraverseCollectInterface<T>(ref List<T> interfaceList, object obj) where T : class
        {
            if (obj == null)
                return;
            ICustomNode node = obj as ICustomNode;
            if (node == null)
            {
                T notify = obj as T;
                if (notify != null)
                    interfaceList.Add(notify);
                return;
            }

            node.CollectInterface<T>(ref interfaceList);
            node.CollectInterfaceInChildren<T>(ref interfaceList);
        }

        public void SetVar<T>(string key, T value)
        {
            VarEnvRef.WriteVar(key, value);
        }

        public bool ClearVar<T>(string key)
        {
            return VarEnvRef.ClearVar<T>(key);
        }

        public T GetVar<T>(string key, T defaultV = default)
        {
            if (VarEnvRef.ReadVar<T>(key, out var value))
                return value;
            return defaultV;
        }

        public bool HasVar<T>(string key)
        {
            return VarEnvRef.HasVar<T>(key);
        }
    }
}