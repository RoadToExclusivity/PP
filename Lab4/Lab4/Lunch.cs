using System.Threading;

namespace Lab4
{
    class Lunch
    {
        private static readonly int PHILOSOPHERS_COUNT = 5;
        private static readonly int FORKS_COUNT = PHILOSOPHERS_COUNT;

        private Semaphore[] _forkSemaphores;
        private Thread[] _threads;
        private Philosopher[] _philosophers;
        private bool[] _forks;

        public Lunch()
        {
            _forkSemaphores = new Semaphore[FORKS_COUNT];
            _threads = new Thread[PHILOSOPHERS_COUNT];
            _philosophers = new Philosopher[PHILOSOPHERS_COUNT];
            _forks = new bool[FORKS_COUNT];

            for (int i = 0; i < PHILOSOPHERS_COUNT; ++i)
            {
                _forks[i] = false;
                _forkSemaphores[i] = new Semaphore(1, 1);
                _philosophers[i] = new Philosopher(this, i);
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

        private int GetLeftForkPosition(int position)
        {
            int forkPosition = position + 1;
            if (forkPosition == FORKS_COUNT)
            {
                forkPosition = 0;
            }

            return forkPosition;
        }

        private int GetRightForkPosition(int position)
        {
            int forkPosition = position - 1;
            if (forkPosition < 0)
            {
                forkPosition = FORKS_COUNT - 1;
            }

            return forkPosition;
        }

        private bool CheckFork(int forkPosition)
        {
            _forkSemaphores[forkPosition].WaitOne();
            if (!_forks[forkPosition])
            {
                return true;
            }
            else
            {
                _forkSemaphores[forkPosition].Release();
                return false;
            }
        }

        private void SetFork(int forkPosition)
        {
            _forks[forkPosition] = true;
            _forkSemaphores[forkPosition].Release();
        }

        private void PutDownFork(int forkPosition)
        {
            _forkSemaphores[forkPosition].WaitOne();
            _forks[forkPosition] = false;
            _forkSemaphores[forkPosition].Release();
        }

        public bool CanTakeBothForks(int position, int priorFork)
        {
            int leftFork = GetLeftForkPosition(position);
            int rightFork = GetRightForkPosition(position);

            _forkSemaphores[leftFork].WaitOne();
            _forkSemaphores[rightFork].WaitOne();

            bool result = (!_forks[leftFork]) && (!_forks[rightFork]);
            if (!result)
            {
                _forkSemaphores[leftFork].Release();
                _forkSemaphores[rightFork].Release();
            }
            else
            {
                int releaseFork = priorFork == 0 ? rightFork : leftFork;
                _forkSemaphores[releaseFork].Release();
            }

            return result;
        }

        public bool CanTakeFork(int position, int forkIndex)
        {
            int forkPosition = forkIndex == 0 ? GetLeftForkPosition(position) : GetRightForkPosition(position);
            _forkSemaphores[forkPosition].WaitOne();

            bool result = !_forks[forkPosition] || (!_philosophers[position].IsHungry());
            if (!result)
            {
                _forkSemaphores[forkPosition].Release();
            }

            return result;
        }

        public bool CheckLeftFork(int position)
        {
            int forkPosition = GetLeftForkPosition(position);
            return CheckFork(forkPosition);
        }

        public void PutDownLeftFork(int position)
        {
            int forkPosition = GetLeftForkPosition(position);
            PutDownFork(forkPosition);
        }

        public void SetLeftFork(int position)
        {
            int forkPosition = GetLeftForkPosition(position);
            SetFork(forkPosition);
        }

        public bool CheckRightFork(int position)
        {
            int forkPosition = GetRightForkPosition(position);
            return CheckFork(forkPosition);
        }

        public void PutDownRightFork(int position)
        {
            int forkPosition = GetRightForkPosition(position);
            PutDownFork(forkPosition);
        }

        public void SetRightFork(int position)
        {
            int forkPosition = GetRightForkPosition(position);
            SetFork(forkPosition);
        }
    }
}
