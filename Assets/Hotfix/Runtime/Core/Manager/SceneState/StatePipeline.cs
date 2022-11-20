namespace LccHotfix
{
    public abstract class StatePipeline
    {
        public string sceneName;
        public string target;

        public StatePipeline(string sceneName, string target)
        {
            this.sceneName = sceneName;
            this.target = target;
        }
        public abstract bool CheckState();
    }
}