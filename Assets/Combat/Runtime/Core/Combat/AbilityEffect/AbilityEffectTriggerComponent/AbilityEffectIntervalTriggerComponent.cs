namespace LccModel
{
    /// <summary>
    /// 间隔触发组件
    /// </summary>
    public class AbilityEffectIntervalTriggerComponent : Component, IUpdate
    {
        public override bool DefaultEnable => false;
        public string intervalValueFormula;
        public GameTimer intervalTimer;


        public void Update()
        {
            if (intervalTimer != null)
            {
                intervalTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, GetParent<AbilityEffect>().TryAssignEffectToOwner);
            }
        }

        public override void OnEnable()
        {
            var intervalExpression = intervalValueFormula;
            var expression = ExpressionHelper.TryEvaluate(intervalExpression);
            if (expression.Parameters.ContainsKey("技能等级"))
            {
                expression.Parameters["技能等级"].Value = GetParent<AbilityEffect>().GetParent<StatusAbility>().GetComponent<AbilityLevelComponent>().level;
            }

            var interval = (int)expression.Value / 1000f;
            intervalTimer = new GameTimer(interval);
        }
    }
}