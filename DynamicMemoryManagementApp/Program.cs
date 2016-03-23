using DynamicMemoryAllocation;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DynamicMemoryManagementApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("RAM MB: ");
            ulong Ram = Convert.ToUInt64(Console.ReadLine());

            Console.Write("CLEANPOINT MB: ");
            ulong CleanPoint = Convert.ToUInt64(Console.ReadLine());

            Console.Write("OBJECTS: ");
            ushort ObjectCount = Convert.ToUInt16(Console.ReadLine());

            Console.Write("OBJECT SIZE MB: ");
            ushort ObjectSize = Convert.ToUInt16(Console.ReadLine());

            var MemoryMaster = new MemoryMaster(Ram * 1024 * 1024, CleanPoint * 1024 * 1024);
            var Objects = new MemoryObject<byte[]>[ObjectCount];

            Parallel.For(0, Objects.Length, Index =>
            {
                Objects[Index] = MemoryMaster.Add((MemoryObject<byte[]> Obj) =>
                {
                    byte[] Data = new byte[Obj.Reserve(ObjectSize * 1024 * 1024)];
                    new Random().NextBytes(Data);

                    byte[] String = Encoding.ASCII.GetBytes("Test String");
                    Array.Copy(String, Data, String.Length);

                    return Data;
                });
            });

            Console.WriteLine("Start");

            var Stopwatch = new Stopwatch();
            Stopwatch.Start();

            for (int j = 0; j < 10; j++)
            {
                Parallel.For(0, Objects.Length, i =>
                {
                    using (Objects[i].Use())
                    {
                        Encoding.ASCII.GetString(Objects[i].Data, 0, 32).Trim();
                        //Console.WriteLine(Encoding.ASCII.GetString(Objects[i].Data, 0, 32));
                    }
                });

                Console.WriteLine("Time " + Stopwatch.ElapsedMilliseconds);
            }

            Stopwatch.Stop();
            Console.ReadKey();
        }
    }
}
