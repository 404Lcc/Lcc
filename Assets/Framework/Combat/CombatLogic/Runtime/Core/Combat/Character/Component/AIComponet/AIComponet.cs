namespace LccModel
{
    public class AIComponet : AObjectBase, IUpdate
    {
        public CombatScript aiScript;

        public override void Awake()
        {
            base.Awake();

            aiScript = CombatScriptManager.Instance.GetScript("LccModel.PlayerAIScript");
        }
        public void Update()
        {
            aiScript.Update();
        }
    }
}