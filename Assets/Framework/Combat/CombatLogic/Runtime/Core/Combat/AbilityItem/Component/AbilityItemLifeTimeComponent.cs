﻿namespace LccModel
{
    public class AbilityItemLifeTimeComponent : Component, IUpdate
    {
        public GameTimer lifeTimer;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            lifeTimer = new GameTimer((float)(object)p1);
        }

        public void Update()
        {
            if (lifeTimer.IsRunning)
            {
                lifeTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, Destroy);
            }
        }

        private void Destroy()
        {
            Parent.Dispose();
        }
    }
}