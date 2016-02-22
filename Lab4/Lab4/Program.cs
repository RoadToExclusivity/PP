using System;
using System.Threading;

namespace Lab4
{
    static class ForkRandom
    {
        static private Semaphore _rndLock = new Semaphore(1, 1);
        static private Random _rnd = new Random();

        static public int GetRandomFork()
        {
            _rndLock.WaitOne();
            int result = _rnd.Next(1);
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

    class Philosopher
    {
        static private readonly int CYCLE_END_BORDER = 10000;
        public enum State
        {
            NO_FORKS,
            LEFT_FORK,
            RIGHT_FORK,
            BOTH_FORKS
        };

        private bool _isHungry;
        private Lunch _lunch;

        private State _state;
        private int _pos;
        private int _eatCount;
        private int _thinkCount;

        public Philosopher(Lunch lunch, int position)
        {
            _lunch = lunch;
            _pos = position;
            _state = State.NO_FORKS;
            _isHungry = true;
            _eatCount = 0;
            _thinkCount = 0;
        }

        public void Eat()
        {
            _isHungry = false;
            _eatCount++;
        }

        public void Think()
        {
            _isHungry = true;
            _thinkCount++;
        }

        public void Activate()
        {
            while (!(_thinkCount > CYCLE_END_BORDER && _eatCount > CYCLE_END_BORDER))
            {
                if (_isHungry)
                {
                    switch (_state)
                    {
                        case State.LEFT_FORK:
                            {
                                if (_lunch.CheckRightFork(_pos))
                                {
                                    _lunch.SetRightFork(_pos);
                                    _state = State.BOTH_FORKS;
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " берет правую вилку и готовится обедать");
                                }
                                break;
                            }
                        case State.RIGHT_FORK:
                            {
                                if (_lunch.CheckLeftFork(_pos))
                                {
                                    _lunch.SetLeftFork(_pos);
                                    _state = State.BOTH_FORKS;
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " берет левую вилку и готовится обедать");
                                }
                                break;
                            }
                        case State.BOTH_FORKS:
                            {
                                Eat();
                                Logger.Log("Философ " + (_pos + 1).ToString() + " обедает");
                                break;
                            }
                        case State.NO_FORKS:
                            {
                                int rndFork = ForkRandom.GetRandomFork();

                                if (_lunch.CanTakeBothForks(_pos, rndFork))
                                {
                                    if (rndFork == 0)
                                    {
                                        _lunch.SetLeftFork(_pos);
                                        _state = State.LEFT_FORK;
                                        Logger.Log("Философ " + (_pos + 1).ToString() + " берет левую вилку");
                                    }
                                    else
                                    {
                                        _lunch.SetRightFork(_pos);
                                        _state = State.RIGHT_FORK;
                                        Logger.Log("Философ " + (_pos + 1).ToString() + " берет правую вилку");
                                    }
                                }
                                else
                                {
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " хочет взять вилку, но не может, т.к. одна из вилок занята");
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    switch (_state)
                    {
                        case State.LEFT_FORK:
                            {

                                _lunch.PutDownLeftFork(_pos);
                                _state = State.NO_FORKS;
                                Logger.Log("Философ " + (_pos + 1).ToString() + " кладет левую вилку, готовится размышлять");

                                break;
                            }
                        case State.RIGHT_FORK:
                            {
                                _lunch.PutDownRightFork(_pos);
                                _state = State.NO_FORKS;
                                Logger.Log("Философ " + (_pos + 1).ToString() + " кладет правую вилку, готовится размышлять");

                                break;
                            }
                        case State.BOTH_FORKS:
                            {
                                int rndFork = ForkRandom.GetRandomFork();

                                if (rndFork == 0)
                                {
                                    _lunch.PutDownLeftFork(_pos);
                                    _state = State.RIGHT_FORK;
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " кладет левую вилку");
                                }
                                else
                                {
                                    _lunch.PutDownRightFork(_pos);
                                    _state = State.LEFT_FORK;
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " кладет правую вилку");
                                }
                                break;
                            }
                        case State.NO_FORKS:
                            {
                                Think();
                                Logger.Log("Философ " + (_pos + 1).ToString() + " размышляет");
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            Logger.Log("Философ " + (_pos + 1).ToString() + " заканчивает работу");
        }
    }

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

    class Program
    {
        static void Main(string[] args)
        {
            Lunch lunch = new Lunch();
            lunch.Work();
        }
    }
}
