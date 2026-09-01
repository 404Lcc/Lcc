using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Entitas
{
    public static class GroupEntityBufferExtensions
    {
        private sealed class VersionState
        {
            public int Version = 1;
        }

        private static readonly ConditionalWeakTable<object, VersionState> s_versions = new();

        public static List<TEntity> GetEntities<TEntity>(this IGroup<TEntity> group, List<TEntity> buffer, ref int bufferVersion) where TEntity : class, IEntity
        {
            var state = s_versions.GetValue(group, _ =>
            {
                var created = new VersionState();
                group.OnEntityAdded += (_, _, _, _) => created.Version++;
                group.OnEntityRemoved += (_, _, _, _) => created.Version++;
                group.OnEntityUpdated += (_, _, _, _, _) => created.Version++;
                return created;
            });

            if (bufferVersion == state.Version)
                return buffer;

            bufferVersion = state.Version;
            return group.GetEntities(buffer);
        }
    }
}