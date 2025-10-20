using System.Collections.Generic;

namespace LccHotfix
{
    public class JobSystems
    {
        private List<IJobSystem> systems = new List<IJobSystem>();

        public void AddSystem<TJob, TChunk>(TJob job) where TJob : IChunkJob<TChunk> where TChunk : struct
        {
            systems.Add(new JobSystem<TJob, TChunk>(job));
        }

        public void Update(IFreeChunk freeChunk)
        {
            foreach (var item in systems)
            {
                item.Update(freeChunk);
            }
        }
    }
}