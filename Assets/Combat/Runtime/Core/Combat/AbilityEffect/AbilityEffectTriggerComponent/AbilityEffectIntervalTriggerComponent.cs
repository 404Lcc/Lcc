namespace LccModel
{
    /// <summary>
    /// 间隔触发组件
    /// </summary>
    public class AbilityEffectIntervalTriggerComponent : Component, IUpdate
    {
        public override bool DefaultEnable { get; set; } = false;
        public string IntervalValue { get; set; }
        public GameTimer IntervalTimer { get; set; }


        public void Update()
        {
            if (IntervalTimer != null)
            {
                IntervalTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, GetParent<AbilityEffect>().TryAssignEffectToParent);
            }
        }

        public override void OnEnable()
        {
            var intervalExpression = IntervalValue;
            var expression = ExpressionHelper.TryEvaluate(intervalExpression);
            if (expression.Parameters.ContainsKey("技能等级"))
            {
                expression.Parameters["技能等级"].Value = GetParent<AbilityEffect>().GetParent<StatusAbility>().GetComponent<AbilityLevelComponent>().Level;
            }

            var interval = (int)expression.Value / 1000f;
            IntervalTimer = new GameTimer(interval);
        }
    }
}