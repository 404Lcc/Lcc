using System.Collections.Generic;

namespace LccHotfix
{
    public class GuideFinishCondBase
    {
        protected Guide _guide;
        protected List<string> _args;

        public GuideFinishCondBase(Guide guide, List<string> args)
        {
            _guide = guide;
            _args = args;
        }

        public virtual bool IsFinish()
        {
            return false;
        }

        public virtual void Release()
        {
        }
    }
}