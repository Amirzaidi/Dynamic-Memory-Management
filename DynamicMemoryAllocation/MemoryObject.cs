using System;

namespace DynamicMemoryAllocation
{
    public abstract class BaseMemoryObject
    {
        internal bool Unloadable
        {
            get
            {
                return (UseCount == 0 && ObjectMemoryUse != 0);
            }
        }

        protected byte UseCount = 0;
        internal DateTime LastUse = DateTime.Now;
        internal ulong ObjectMemoryUse = 0;
        internal abstract void Unload();
    }

    public class MemoryObject<T> : BaseMemoryObject, IDisposable
    {
        public T Data = default(T);

        private MemoryMaster _Master;
        private MemoryDataLoader<T> _Loader;

        internal MemoryObject(MemoryMaster Master, MemoryDataLoader<T> Loader)
        {
            _Master = Master;
            _Loader = Loader;
        }

        public MemoryObject<T> Use()
        {
            lock (this)
            {
                UseCount++;
                if (Data == null || Data.Equals(default(T)))
                {
                    Data = _Loader(this);
                }

                return this;
            }
        }

        public int Reserve(int NecessaryAmount)
        {
            return (int)Reserve((uint)NecessaryAmount);
        }

        public uint Reserve(uint NecessaryAmount)
        {
            _Master.Reserve(NecessaryAmount);
            ObjectMemoryUse += NecessaryAmount;
            return NecessaryAmount;
        }

        internal override void Unload()
        {
            Data = default(T);
            ObjectMemoryUse = 0;
        }

        public void Dispose()
        {
            LastUse = DateTime.Now;
            lock (this)
            {
                UseCount--;
            }
        }
    }
}
