namespace LccHotfix
{
    public class LogBhvCfg : ICustomNodeCfg
    {
        public string LogStr { get; set; }

        public System.Type NodeType()
        {
            return typeof(LogBhv);
        }

        public LogBhvCfg()
        {
            LogStr = "";
        }

        public LogBhvCfg(string str)
        {
            LogStr = str;
        }
    }

    /// <summary>
    /// 运行时节点:  打印Log
    /// </summary>
    public class LogBhv : BehaviorNodeBase
    {
        private string _logStr;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            var theCfg = cfg as LogBhvCfg;
            CLHelper.Assert(theCfg != null);
            _logStr = theCfg.LogStr;
        }

        public override void Destroy()
        {
            _logStr = null;
            base.Destroy();
        }

        protected override void OnBegin()
        {
            if (_logStr == null)
                return;
            CLHelper.LogInfo(this, _logStr);
        }
    }
}