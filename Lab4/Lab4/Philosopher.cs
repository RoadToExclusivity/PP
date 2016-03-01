namespace Lab4
{
    class Philosopher
    {
        static private readonly int CYCLE_END_BORDER = 2;

        private bool _isHungry;

        private int _pos;
        private int _eatCount;
        private int _thinkCount;

        private Fork[] _forks;
        private int _allForksCount;
        private int _curForksCount;

        public Philosopher(int position, Fork[] forks)
        {
            _pos = position;
            _forks = forks;
            _isHungry = true;
            _eatCount = 0;
            _thinkCount = 0;
            _allForksCount = _forks.Length;
            _curForksCount = 0;
        }

        private void Eat()
        {
            _isHungry = false;
            _eatCount++;
        }

        private void Think()
        {
            _isHungry = true;
            _thinkCount++;
        }


        private void TakeFork(int forkRelativePosition)
        {
            if (forkRelativePosition == 0)
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

        public bool IsHungry()
        {
            return _isHungry;
        }

        public void Activate()
        {
            while (!(_thinkCount > CYCLE_END_BORDER && _eatCount > CYCLE_END_BORDER))
            {
                if (_isHungry)
                {
                    if (_curForksCount == 0)
                    {
                        int rndFork = ForkRandom.GetRandomFork(_allForksCount);

                        if (_lunch.CanTakeBothForks(_pos, rndFork))
                        {
                            TakeFork(rndFork);
                        }
                        else
                        {
                            Logger.Log("Философ " + (_pos + 1).ToString() + " хочет взять вилку, но не может, т.к. в ближайшее время ее не отпустят");
                        }
                    }
                    else
                    {
                        if (_curForksCount == _allForksCount)
                        {
                            Eat();
                            Logger.Log("Философ " + (_pos + 1).ToString() + " обедает");
                        }
                        else
                        {

                        }
                    }
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
                                    TakeFork(rndFork);
                                }
                                else
                                {
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " хочет взять вилку, но не может, т.к. в ближайшее время ее не отпустят");
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
}
