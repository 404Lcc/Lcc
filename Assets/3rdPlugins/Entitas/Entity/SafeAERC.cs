using System;
using System.Collections.Generic;

namespace Entitas
{
    /// Automatic Entity Reference Counting (AERC)
    /// is used internally to prevent pooling retained entities.
    /// If you use retain manually you also have to
    /// release it manually at some point.
    /// SafeAERC checks if the entity has already been
    /// retained or released. It's slower than UnsafeAERC, but you keep the information
    /// about the owners.
    public sealed class SafeAERC : IAERC
    {
        public static readonly Func<Entity, IAERC> Delegate = entity => new SafeAERC(entity);

        public int RetainCount => _owners.Count;
        public HashSet<object> Owners => _owners;

        readonly Entity _entity;
        readonly HashSet<object> _owners = new HashSet<object>();

        public SafeAERC(Entity entity)
        {
            _entity = entity;
        }

        public void Retain(object owner)
        {
            if (!Owners.Add(owner))
                throw new EntityIsAlreadyRetainedByOwnerException(_entity, owner);
        }

        public void Release(object owner)
        {
            if (!Owners.Remove(owner))
                throw new EntityIsNotRetainedByOwnerException(_entity, owner);
        }
    }
}
