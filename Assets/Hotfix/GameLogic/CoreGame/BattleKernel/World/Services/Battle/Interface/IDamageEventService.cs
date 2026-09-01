using System;

namespace LccHotfix
{
    public interface IDamageEventService
    {
        void AddDamageHandler(Action<EvtDamage> handler);
        void RemoveDamageHandler(Action<EvtDamage> handler);
        void AddHealHandler(Action<EvtHeal> handler);
        void RemoveHealHandler(Action<EvtHeal> handler);
        void AddAfterDamageHandler(Action<EvtAfterDamage> handler);
        void RemoveAfterDamageHandler(Action<EvtAfterDamage> handler);
        void DispatchDamage(EvtDamage evt);
        void DispatchHeal(EvtHeal evt);
        void DispatchAfterDamage(EvtAfterDamage evt);
    }
}
