/********************************************************************
*  CustomLogicGenInfo 外部使用方 构造自定义逻辑的初始数据
*  创建CustomLogic的运行时初始化信息
*  接口类，多个项目使用，不要直接修改
*  业务扩展可以新加一个 CustomLogicGenInfo 的继承类
*********************************************************************/

namespace LccHotfix
{
    /// <summary>
    /// 运行时初始化信息（不应被修改）
    /// </summary>
    public class ICustomLogicGenInfo : ICanRecycle
    {
        //逻辑配置ID
        protected int _logicConfigID = 0;

        //初始数据黑板
        protected VarEnv _preEnv = null;

        //配置组名
        protected string _configContainerName;

        public int LogicConfigID
        {
            get { return _logicConfigID; }
            set { _logicConfigID = value; }
        }

        public VarEnv PreEnv
        {
            get { return _preEnv; }
            set { _preEnv = value; }
        }

        public string ConfigContainerName
        {
            get { return _configContainerName; }
            set { _configContainerName = value; }
        }

        public bool IsInPool { get; private set; } = false;

        public void Construct()
        {
            IsInPool = false;
        }

        public virtual void Destroy()
        {
            IsInPool = true;
            _logicConfigID = 0;
            _preEnv = null;
            _configContainerName = null;
        }
    }
}