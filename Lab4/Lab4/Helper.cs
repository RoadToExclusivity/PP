using System;
using System.Threading;

namespace Lab4
{
    static class ForkRandom
    {
        static private Semaphore _rndLock = new Semaphore(1, 1);
        static private Random _rnd = new Random();

        static public int GetRandomFork(int maxForkCount)
        {
            _rndLock.WaitOne();
            int result = _rnd.Next(maxForkCount);
            _rndLock.Release();

            return result;
        }
    }

    static class Logger
    {
        static private Semaphore _output = new Semaphore(1, 1);

        static public void Log(string msg)
        {
            _output.WaitOne();
            Console.WriteLine(msg);
            _output.Release();
        }
    }
}
