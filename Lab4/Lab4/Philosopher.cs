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
        private bool[] _ownForks;
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

            _ownForks = new bool[_allForksCount];
            for (int i = 0; i < _allForksCount; ++i)
            {
                _ownForks[i] = false;
            }
        }

        private void Eat()
        {
            _isHungry = false;
            _eatCount++;

            for (int i = 0; i < _allForksCount; ++i)
            {
                _forks[i].SetForPut();
            }
        }

        private void Think()
        {
            _isHungry = true;
            _thinkCount++;
        }

        private void LogTakeFork(int forkPosition)
        {
            string msg = "Философ " + (_pos + 1).ToString();
            if (forkPosition == 0)
            {
                msg = msg + " берет левую вилку";
            }
            else
            {
                msg = msg + " берет правую вилку";
            }
            Logger.Log(msg);
        }

        private void LogPutFork(int forkPosition)
        {
            string msg = "Философ " + (_pos + 1).ToString();
            if (forkPosition == 0)
            {
                msg = msg + " кладет левую вилку";
            }
            else
            {
                msg = msg + " кладет правую вилку";
            }
            Logger.Log(msg);
        }

        private bool CanTakeFork(int priorFork)
        {
            _forks[priorFork].Lock();
            if (_forks[priorFork].IsTaken())
            {
                _forks[priorFork].Release();
                return false;
            }

            if (_curForksCount == _allForksCount - 1)
            {
                return true;
            }

            for (int i = 0; i < _allForksCount; ++i)
            {
                if (i != priorFork)
                {
                    _forks[i].Lock();
                }
            }

            bool res = true;
            for (int i = 0; i < _allForksCount && res; ++i)
            {
                if (i != priorFork)
                {
                    res = res && (!_forks[i].IsTaken() || (!_ownForks[i] && _forks[i].IsForPut()));
                }
            }

            for (int i = 0; i < _allForksCount; ++i)
            {
                if (i != priorFork) //обязательно будет забрана вилка priorFork
                {
                    _forks[i].Release();
                }
            }

            if (!res)
            {
                _forks[priorFork].Release();
            }

            return res;
        }

        private bool TakeRowForks()
        {
            bool hasTaken = false;
            for (int i = 0; i < _allForksCount && !hasTaken; ++i)
            {
                if (CanTakeFork(i))
                {
                    LogTakeFork(i);
                    TakeFork(i);
                    hasTaken = true;
                }
            }

            if (!hasTaken)
            {
                Logger.Log("Философ " + (_pos + 1).ToString() + " хочет взять вилку, но не может, т.к. может возникнуть ситуация голода");
            }

            return hasTaken;
        }

        private void TakeFork(int forkRelativePosition)
        {
            _forks[forkRelativePosition].Take();
            _curForksCount++;
            _ownForks[forkRelativePosition] = true;
        }

        private void PutDownFork(int forkRelativePosition)
        {
            _forks[forkRelativePosition].PutDown();
            _curForksCount--;
            _ownForks[forkRelativePosition] = false;
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

                        if (CanTakeFork(rndFork))
                        {
                            LogTakeFork(rndFork);
                            TakeFork(rndFork);
                        }
                        else
                        {
                            TakeRowForks();
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
                            if (TakeRowForks())
                            {
                                if (_curForksCount == _allForksCount)
                                {
                                    Logger.Log("Философ " + (_pos + 1).ToString() + " готовится обедать");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_curForksCount == 0)
                    {
                        Think();
                        Logger.Log("Философ " + (_pos + 1).ToString() + " размышляет");
                    }
                    else
                    {
                        if (_curForksCount == _allForksCount)
                        {
                            int rndFork = ForkRandom.GetRandomFork(_allForksCount);

                            PutDownFork(rndFork);
                            LogPutFork(rndFork);
                        }
                        else
                        {
                            int rndFork = ForkRandom.GetRandomFork(_curForksCount);
                            int counter = -1;

                            for (int i = 0; i < _allForksCount && counter != rndFork; ++i)
                            {
                                if (_ownForks[i])
                                {
                                    counter++;
                                    if (counter == rndFork)
                                    {
                                        PutDownFork(i);
                                        LogPutFork(i);
                                    }
                                }
                            }

                            if (_curForksCount == 0)
                            {
                                Logger.Log("Философ " + (_pos + 1).ToString() + " готовится размышлять");
                            }
                        }
                    }
                }
            }
            Logger.Log("Философ " + (_pos + 1).ToString() + " заканчивает работу");
        }
    }
}