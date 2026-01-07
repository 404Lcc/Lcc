using UnityEngine;

namespace LccHotfix
{
    public interface ILogicCfgContainerRegister
    {
        void Register(ICustomLogicService service);
    }

    internal class CustomLogicManager : Module, ICustomLogicService
    {
        private CustomLogicFactory _factory;
        private ILogicCfgContainerRegister _register;

        public CustomLogicManager()
        {
            _factory = new CustomLogicFactory();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            _factory.Dispose();
        }

        public void SetRegister(ILogicCfgContainerRegister register)
        {
            _register = register;
            _register.Register(this);
        }

        public void AddConfigContainer(ILogicConfigContainer container)
        {
            _factory.AddConfigContainer(container);
        }

        public ILogicConfigContainer GetConfigContainer(string name)
        {
            return _factory.TryGetConfigContainer(name, out var container) ? container : null;
        }

        public CustomLogic CreateLogic(ICustomLogicGenInfo genInfo)
        {
            if (genInfo == null)
            {
                Debug.LogError("CreateLogic genInfo == null");
                return null;
            }

            return _factory.CreateLogic(genInfo);
        }

        public T CreateLogic<T>(ICustomLogicGenInfo genInfo) where T : CustomLogic
        {
            if (genInfo == null)
            {
                Debug.LogError($"CreateLogic<T> genInfo == null, T = {typeof(T)}");
                return null;
            }

            var logic = _factory.CreateLogic(genInfo);
            if (logic is T theLogic)
            {
                return theLogic;
            }

            Debug.LogError($"CreateLogic logic = {genInfo.LogicConfigID} logic({logic.GetType()}) is not {typeof(T)}");
            return null;
        }

        public void DestroyLogic(CustomLogic logic)
        {
            if (logic == null)
                return;
            _factory.DestroyCustomNode(logic);
        }


        public T NewGenInfo<T>() where T : ICustomLogicGenInfo, new()
        {
            return _factory.CreatePart<T>();
        }

        public VarEnv NewVarEnv()
        {
            return _factory.CreatePart<VarEnv>();
        }
    }
}