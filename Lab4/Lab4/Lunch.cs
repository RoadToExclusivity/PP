using System.Threading;

namespace Lab4
{
    class Lunch
    {
        private static readonly int PHILOSOPHERS_COUNT = 5;
        private static readonly int FORKS_COUNT = PHILOSOPHERS_COUNT;

        private Thread[] _threads;
        private Philosopher[] _philosophers;
        private Fork[] _forks;

        private int GetForkPosition(int philosopherPos, int relativeForkPos)
        {
            int forkPosition = philosopherPos + relativeForkPos;
            if (forkPosition < 0)
            {
                forkPosition = FORKS_COUNT + forkPosition;
            }
            else
            {
                if (forkPosition >= FORKS_COUNT)
                {
                    forkPosition -= FORKS_COUNT;
                }
            }

            return forkPosition;
        }

        public Lunch()
        {
            _threads = new Thread[PHILOSOPHERS_COUNT];
            _philosophers = new Philosopher[PHILOSOPHERS_COUNT];
            _forks = new Fork[FORKS_COUNT];
            for (int i = 0; i < FORKS_COUNT; ++i)
            {
                _forks[i] = new Fork(i);
            }

            for (int i = 0; i < PHILOSOPHERS_COUNT; ++i)
            {
                Fork[] curForks = new Fork[2] { _forks[GetForkPosition(i, +0)], _forks[GetForkPosition(i, -1)] };
                _philosophers[i] = new Philosopher(i, curForks);
                _threads[i] = new Thread(new ThreadStart(_philosophers[i].Activate));
            }
        }

        public void Work()
        {
            for (int i = 0; i < PHILOSOPHERS_COUNT; ++i)
            {
                _threads[i].Start();
            }
        }
    }
}
