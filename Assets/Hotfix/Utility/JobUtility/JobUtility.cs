using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace LccHotfix
{
    public interface IFreeChunk
    {
    }

    public class FreeChunk<T> : IFreeChunk where T : struct
    {
        private NativeArray<T> dataArray;
        private Queue<int> freeIndices;
        private int nextIndex = 0;
        private readonly int max;

        public FreeChunk(int max)
        {
            this.max = max;

            dataArray = new NativeArray<T>(this.max, Allocator.Persistent);
            freeIndices = new Queue<int>();

            // 初始化所有组件
            for (int i = 0; i < max; i++)
            {
                dataArray[i] = new T();
            }
        }

        public int Create()
        {
            int index;
            if (freeIndices.Count > 0)
            {
                index = freeIndices.Dequeue();
            }
            else if (nextIndex < max)
            {
                index = nextIndex++;
            }
            else
            {
                return -1;
            }

            return index;
        }

        public void Destroy(int index)
        {
            if (index < 0 || index >= max)
            {
                return;
            }

            freeIndices.Enqueue(index);
        }

        public T GetData(int index)
        {
            return dataArray[index];
        }

        public void SetData(int index, T data)
        {
            dataArray[index] = data;
        }

        public NativeArray<T> GetArray()
        {
            return dataArray;
        }

        public int GetActiveCount()
        {
            return nextIndex - freeIndices.Count;
        }

        public void Dispose()
        {
            if (dataArray.IsCreated)
            {
                dataArray.Dispose();
            }

            freeIndices.Clear();
            nextIndex = 0;
        }
    }

    public interface IChunkJob<TChunk> where TChunk : struct
    {
        public float DeltaTime { get; set; }
        void Execute(ref TChunk chunk);
    }

    [BurstCompile]
    public struct ChunkJob<TJob, TChunk> : IJobParallelFor where TJob : IChunkJob<TChunk> where TChunk : struct
    {
        public NativeArray<TChunk> array;
        public TJob job;

        public void Execute(int index)
        {
            TChunk item = array[index];
            job.Execute(ref item);
            array[index] = item;
        }

        public void Run(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Execute(i);
            }
        }
    }

    public enum JobScheduleMode
    {
        Run,
        Parallel,
    }

    public interface IJobSystem
    {
        void Update(IFreeChunk freeChunk);
    }

    public class JobSystem<TJob, TChunk> : IJobSystem where TJob : IChunkJob<TChunk> where TChunk : struct
    {
        private TJob _job;

        public JobSystem(TJob job)
        {
            _job = job;
        }

        public void Update(IFreeChunk iFreeChunk)
        {
            var freeChunk = iFreeChunk as FreeChunk<TChunk>;
            _job.DeltaTime = Time.deltaTime;
            JobUtility.Schedule<TJob, TChunk>(JobScheduleMode.Parallel, freeChunk.GetArray(), freeChunk.GetActiveCount(), _job);
        }
    }

    public static class JobUtility
    {
        public static void Schedule<TJob, TChunk>(JobScheduleMode mode, NativeArray<TChunk> array, int count, TJob job, int batchSize = 32) where TJob : IChunkJob<TChunk> where TChunk : struct
        {
            var chunkJob = new ChunkJob<TJob, TChunk>
            {
                array = array,
                job = job,
            };

            switch (mode)
            {
                case JobScheduleMode.Run:
                    chunkJob.Run(count);
                    break;
                case JobScheduleMode.Parallel:
                    chunkJob.Schedule(count, batchSize).Complete();
                    break;
            }
        }
    }
}