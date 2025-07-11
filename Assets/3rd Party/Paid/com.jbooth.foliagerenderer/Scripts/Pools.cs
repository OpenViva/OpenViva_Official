//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.FoliageRendering
{

    public class ObjectPool<T> where T : IDisposable, new()
    {
        public ObjectPool(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                _availableObjects.Enqueue(new T());
            }
        }

        private Queue<T> _availableObjects = new Queue<T>();

        public T GetObject()
        {
            if (_availableObjects.Count > 0)
            {
                return _availableObjects.Dequeue();
            }
            else
            {
                return new T();
            }
        }

        public void ReturnObject(T obj)
        {
            obj.Dispose();
            _availableObjects.Enqueue(obj);
        }
    }


    public class ObjectListPool<T, K> where T : List<K>, new() where K : IDisposable, new()
    {
        private Queue<T> _availableObjects = new Queue<T>();

        public T GetObject()
        {
            if (_availableObjects.Count > 0)
            {
                return _availableObjects.Dequeue();
            }
            else
            {
                return new T();
            }
        }

        public void ReturnObject(T obj)
        {
            foreach (var o in obj)
            {
                o.Dispose();
            }
            obj.Clear();
            _availableObjects.Enqueue(obj);
        }
    }

    public class GraphicsBufferPool
    {
        public struct Statistics
        {
            public int poolCount;
            public int reservedMemory;
            public int inuseMemory;
            public int currentFreeBuffers;
            public int currentUsedBuffers;
            public int resizeEvents;

        }
        Statistics stats = new Statistics();

        public Statistics GetStatistics()
        {
            stats.poolCount = pools.Count;
            stats.currentFreeBuffers = 0;
            stats.reservedMemory = 0;
            foreach (var v in pools.Values)
            {
                stats.currentFreeBuffers += v.Count;
                foreach (var gb in v)
                {
                    stats.reservedMemory += gb.count * gb.stride;
                }
            }
            return stats;
        }

        // target:stride -> buffer pool
        Dictionary<int, List<GraphicsBuffer>> pools = new Dictionary<int, List<GraphicsBuffer>>();
        public int maxFreeBuffers;

        public GraphicsBufferPool(int maxFreeBuffers = 2)
        {
            this.maxFreeBuffers = maxFreeBuffers;
        }

        public void ReleaseUnusedBuffers()
        {
            foreach (var pool in pools.Values)
            {
                foreach (var gb in pool)
                {
                    gb.Dispose();
                }
                pool.Clear();
            }
            stats = new Statistics();
        }

        // we don't handle strides bigger than 0.1 meg, but wtf would you be doing anyway?
        int GetPoolID(GraphicsBuffer.Target target, int stride)
        {
            return (int)target * 100000 + stride;
        }

        // if needed, release the smallest buffer
        void ReleaseSmallest(List<GraphicsBuffer> pool)
        {
            if (pool.Count > maxFreeBuffers)
            {
                GraphicsBuffer smallestBuffer = null;
                int smallestSize = int.MaxValue;

                for (int i = 0; i < pool.Count; ++i)
                {
                    var buffer = pool[i];
                    if (buffer.IsValid() == false)
                    {
                        buffer.Dispose();
                        pool.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (buffer.count < smallestSize)
                    {
                        smallestSize = buffer.count;
                        smallestBuffer = buffer;
                    }
                }

                if (smallestBuffer != null)
                {
                    pool.Remove(smallestBuffer);
                    smallestBuffer.Dispose();
                    stats.resizeEvents++;
                }
            }
        }

        // return a buffer at least big enough for size
        public GraphicsBuffer GetBuffer(GraphicsBuffer.Target target, int size, int stride)
        {
            List<GraphicsBuffer> pool;
            int poolID = GetPoolID(target, stride);
            if (pools.TryGetValue(poolID, out pool))
            {
                for (int i = 0; i < pool.Count; ++i)
                {
                    var buffer = pool[i];
                    if (buffer.count >= size)
                    {
                        pool.RemoveAt(i);
                        stats.currentUsedBuffers++;
                        stats.inuseMemory += buffer.count * buffer.stride;
                        return buffer;
                    }
                }
                stats.currentUsedBuffers++;
                stats.inuseMemory += size * stride;
                return new GraphicsBuffer(target, size, stride);
            }
            else
            {
                pools.Add(poolID, new List<GraphicsBuffer>());
                stats.currentUsedBuffers++;
                stats.inuseMemory += size * stride;
                return new GraphicsBuffer(target, size, stride);
            }
            
            
        }

        public void ReturnBuffer(GraphicsBuffer buffer)
        {
            if (buffer == null)
            {
                Debug.LogError("Null buffer returned to the pool");
                return;
            }
            // This seems to happen when we're cleaning up exiting play mode
            if (buffer.IsValid() == false)
            {
                stats.currentUsedBuffers--;
                stats.inuseMemory = 0;
                //stats.inuseMemory -= buffer.count * buffer.stride;
                buffer.Dispose();
                return;
            }
            int poolID = GetPoolID(buffer.target, buffer.stride);
            List<GraphicsBuffer> pool;
            if (pools.TryGetValue(poolID, out pool))
            {
                pool.Add(buffer);
                stats.currentUsedBuffers--;
                stats.inuseMemory -= buffer.count * buffer.stride;
                ReleaseSmallest(pool);
            }
            else
            {
                Debug.LogError("Releasing buffer that has no pool");
            }
                
            
        }

    }

}
