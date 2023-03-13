using System.Collections.Generic;

namespace LccModel
{
    public class SkinViewComponent : Component
    {
        public CombatView CombatViewEntity => GetParent<CombatView>();

        public void ApplySkin(List<string> list)
        {

        }
    }
}