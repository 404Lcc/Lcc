using System;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        public Type MainObjectViewType { get; private set; }

        public void SetMainObjectViewType(Type viewType)
        {
            MainObjectViewType = viewType;
        }
    }
}
