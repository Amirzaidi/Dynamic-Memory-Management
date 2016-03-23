using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System;

namespace DynamicMemoryAllocation
{
    public class MemoryMaster
    {
        private ulong TotalMemoryUse = 0;
        private ulong _MemoryLimit;
        private ulong _CleanPoint;
        private ConcurrentBag<BaseMemoryObject> Loaded = new ConcurrentBag<BaseMemoryObject>();

        public MemoryMaster(ulong MemoryLimit, ulong CleanPoint)
        {
            if (CleanPoint > MemoryLimit)
            {
                CleanPoint = MemoryLimit;
            }

            _MemoryLimit = MemoryLimit;
            _CleanPoint = CleanPoint;
        }

        public MemoryObject<T> Add<T>(MemoryDataLoader<T> Loader)
        {
            var MemoryObject = new MemoryObject<T>(this, Loader);
            Loaded.Add(MemoryObject);
            return MemoryObject;
        }

        internal void Reserve(uint NecessaryAmount)
        {
            if (NecessaryAmount > _MemoryLimit)
            {
                throw new System.Exception("Can't allocate the requested memory");
            }
            
            lock (this)
            {
                BaseMemoryObject NextUnload = null;
                if (TotalMemoryUse + NecessaryAmount > _MemoryLimit)
                {
                    while (TotalMemoryUse > _CleanPoint)
                    {
                        NextUnload = Loaded.Where(x => x.Unloadable).OrderBy(x => (ulong)(DateTime.Now - x.LastUse).Ticks * x.ObjectMemoryUse).LastOrDefault();
                        if (NextUnload == null)
                        {
                            //Nothing to unload :(
                            Thread.Sleep(1);
                        }
                        else
                        {
                            TotalMemoryUse -= NextUnload.ObjectMemoryUse;
                            NextUnload.Unload();
                        }
                    }
                }

                TotalMemoryUse += NecessaryAmount;
            }
        }
    }
}
