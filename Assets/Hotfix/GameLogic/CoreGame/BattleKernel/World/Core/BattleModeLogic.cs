namespace LccHotfix
{
    public interface IGameDuration
    {
        public float GetGameDuration();
    }
    
    public class BattleModeLogic : CustomLogic, IGameDuration
    {
        private float _gameDuration;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _gameDuration = 0;
        }

        public override float Update(float dt)
        {
            base.Update(dt);
            _gameDuration += dt;
            return dt;
        }

        public float GetGameDuration()
        {
            return _gameDuration;
        }
    }
}
