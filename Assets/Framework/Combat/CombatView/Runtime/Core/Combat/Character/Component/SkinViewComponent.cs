using System.Collections.Generic;

namespace LccModel
{
    public class SkinViewComponent : Component
    {
        public CombatView CombatView => GetParent<CombatView>();

        public void ApplySkin(List<string> list)
        {

        }
    }
}