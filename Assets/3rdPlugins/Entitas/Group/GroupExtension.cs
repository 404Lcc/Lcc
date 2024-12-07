﻿namespace Entitas
{
    public static class GroupExtension
    {
        /// Creates a Collector for this group.
        public static ICollector<TEntity> CreateCollector<TEntity>(this IGroup<TEntity> group, GroupEvent groupEvent = GroupEvent.Added) where TEntity : Entity =>
            new Collector<TEntity>(group, groupEvent);
    }
}
