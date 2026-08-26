using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entitas;

namespace LccHotfix
{
    public interface IHasSourceEntity
    {
        LogicEntity SourceEntity { get; }
    }

    public interface IHasOwnerEntity
    {
        LogicEntity OwnerEntity { get; }
    }
    
    public interface IBuff
    {
        int Level { get; set; }
        int MaxLevel { get; set; }
        BuffGenInfo BuffGenInfo { get; }

        bool IsFinished();

        void UpdateBuff(float dt);

        void Upgrade();
    }

}
