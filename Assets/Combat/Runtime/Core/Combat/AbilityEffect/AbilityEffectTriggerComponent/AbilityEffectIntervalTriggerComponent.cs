namespace LccModel
{
    /// <summary>
    /// 间隔触发组件
    /// </summary>
    public class AbilityEffectIntervalTriggerComponent : Component, IUpdate
    {

        public string intervalValueFormula;
        public GameTimer intervalTimer;


        public override void Awake()
        {
            base.Awake();

            GetParent<AbilityEffect>().ParseParams();

            var intervalExpression = intervalValueFormula;


            var expression = ExpressionHelper.TryEvaluate(intervalExpression);
            if (expression.Parameters.ContainsKey("技能等级"))
            {
                expression.Parameters["技能等级"].Value = GetParent<AbilityEffect>().GetParent<StatusAbility>().GetComponent<AbilityLevelComponent>().level;
            }

            var interval = (int)expression.Value / 1000f;
            intervalTimer = new GameTimer(interval);
        }

        public void Update()
        {
            if (intervalTimer != null)
            {
                intervalTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, GetParent<AbilityEffect>().TryAssignEffectToOwner);
            }
        }
    }
}