using System;

namespace LccHotfix
{
    public class StatePipeline
    {
        public string target;
        public Func<bool> condition;

        public StatePipeline(string target, Func<bool> condition)
        {
            this.target = target;
            this.condition = condition;
        }
    }
}