using System.Collections.Generic;

namespace LccHotfix
{
    public class GuideCondBase
    {
        protected List<string> _args;

        public GuideCondBase(List<string> args)
        {
            _args = args;
        }

        public virtual bool Trigger()
        {
            return false;
        }

        public virtual void Release()
        {
        }
    }
}