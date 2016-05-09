using System;
using System.IO.Pipes;

namespace Lab5
{
    enum Request
    {
        GetPhilosopherPosition = 0,
        PutFork,
        TakeFork,
        Eat,
        Think,
        Rest
    };

    enum Response
    {
        PhilosopherPosition = 0,
        InvalidPhilosopherPosition,
        OkPutFork,
        FailedPutFork,
        OkTakeFork,
        FailedTakeFork,
        OkEat,
        OkThink,
        OkRest
    };

    class LunchServer
    {
        static public readonly string SERVER_PIPE_NAME = "lunchPipe";
        static public readonly byte FORKS_PER_PHILOSOPHER = 2;

        private Fork[] _forks;
        private byte _forksCount;
        private byte _curForkCounter;
        private int _connectedPhilosopherCount;
        private NamedPipeServerStream _pipeServer;

        static private NamedPipeServerStream CreateServerPipe()
        {
            return new NamedPipeServerStream(SERVER_PIPE_NAME, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, 
                                            PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        }

        private byte GetCorrectForkIndex(byte philosopherPosition, int offset)
        {
            int forkPosition = philosopherPosition + offset;
            if (forkPosition < 0)
            {
                forkPosition = _forksCount + forkPosition;
            }
            else
            {
                if (forkPosition >= _forksCount)
                {
                    forkPosition -= _forksCount;
                }
            }

            return (byte)forkPosition;
        }

        public LunchServer(byte forksCount)
        {
            _forksCount = forksCount;
            _curForkCounter = 0;
            _connectedPhilosopherCount = 0;
            _forks = new Fork[_forksCount];
            for (byte i = 0; i < _forksCount; ++i)
            {
                _forks[i] = new Fork();
            }
        }

        public void Start()
        {
            Console.WriteLine("Обед философов начался ! Всего {0} столовых приборов !", _forksCount);
            bool hasStarted = false;
            while (_connectedPhilosopherCount > 0 || !hasStarted)
            {
                _pipeServer = CreateServerPipe();
                _pipeServer.WaitForConnection();

                var request = (Request)_pipeServer.ReadByte();
                switch(request)
                {
                    case Request.GetPhilosopherPosition:
                        AssignPhilosopherPosition();
                        hasStarted = true;
                        break;
                    case Request.Eat:
                        Eat();
                        break;
                    case Request.Think:
                        Think();
                        break;
                    case Request.Rest:
                        Rest();
                        break;
                    case Request.TakeFork:
                        TakeFork();
                        break;
                    case Request.PutFork:
                        PutDownFork();
                        break;
                    default:
                        break;
                }
                _pipeServer.WaitForPipeDrain();
            }

            Console.WriteLine("Все ушли !");
            Console.ReadLine();
        }

        private void AssignPhilosopherPosition()
        {
            _pipeServer.ReadByte();
            if (_curForkCounter < _forksCount)
            {
                _pipeServer.WriteByte((byte)Response.PhilosopherPosition);
                _pipeServer.WriteByte(_curForkCounter++);
                _connectedPhilosopherCount++;
            }
            else
            {
                _pipeServer.WriteByte((byte)Response.InvalidPhilosopherPosition);
            }
        }

        private void Eat()
        {
            byte philPos = (byte)_pipeServer.ReadByte();
            byte[] indexes = GetNormalizedForksIndexes(philPos, FORKS_PER_PHILOSOPHER);
            for (int i = 0; i < FORKS_PER_PHILOSOPHER; ++i)
            {
                _forks[indexes[i]].SetForPut();
            }
            Console.WriteLine("Философ {0} обедает", philPos);
            _pipeServer.WriteByte((byte)Response.OkEat);
        }

        private void Think()
        {
            byte philPos = (byte)_pipeServer.ReadByte();
            byte[] indexes = GetNormalizedForksIndexes(philPos, FORKS_PER_PHILOSOPHER);
            for (int i = 0; i < FORKS_PER_PHILOSOPHER; ++i)
            {
                _forks[indexes[i]].SetForEat();
            }
            Console.WriteLine("Философ {0} размышляет", philPos);
            _pipeServer.WriteByte((byte)Response.OkThink);
        }

        private void Rest()
        {
            byte philPos = (byte)_pipeServer.ReadByte();
            Console.WriteLine("Философ {0} уходит", philPos);
            _connectedPhilosopherCount--;
            _pipeServer.WriteByte((byte)Response.OkRest);
        }

        private byte[] GetNormalizedForksIndexes(byte position, byte count)
        {
            byte[] res = new byte[count];
            int offset = 0;
            for (int i = 0; i < count; ++i)
            {
                res[i] = GetCorrectForkIndex(position, offset--);
            }

            return res;
        }

        private bool CanTakeFork(byte[] indexes, byte forkIndex, byte curForks, byte philPos)
        {
            if (_forks[forkIndex].IsTaken())
            {
                return false;
            }

            if (curForks == FORKS_PER_PHILOSOPHER - 1)
            {
                return true;
            }

            bool ok = true;
            for (int i = 0; i < FORKS_PER_PHILOSOPHER; ++i)
            {
                if (indexes[i] != forkIndex)
                {
                    ok = ok && (!_forks[indexes[i]].IsTaken() || ((_forks[indexes[i]].GetOwner() != philPos) && !_forks[indexes[i]].IsForEat()));
                }
            }

            return ok;
        }

        private void TakeFork()
        {
            byte philPos = (byte)_pipeServer.ReadByte();
            byte position = (byte)(philPos - 1);
            byte[] indexes = GetNormalizedForksIndexes(position, FORKS_PER_PHILOSOPHER);
            byte curForks = 0;
            for (int i = 0; i < FORKS_PER_PHILOSOPHER; ++i)
            {
                if (_forks[indexes[i]].GetOwner() == philPos)
                {
                    curForks++;
                }
            }
            bool ok = false;
            for (int i = 0; i < FORKS_PER_PHILOSOPHER && !ok; ++i)
            {
                if (CanTakeFork(indexes, indexes[i], curForks, philPos))
                {
                    ok = true;
                    Console.WriteLine("Философ {0} берет {1} вилку", philPos, i == 0 ? "левую" : "правую");
                    _forks[indexes[i]].Take();
                    _forks[indexes[i]].SetOwner(philPos);
                }
            }

            if (!ok)
            {
                _pipeServer.WriteByte((byte)Response.FailedTakeFork);
                return;
            }

            _pipeServer.WriteByte((byte)Response.OkTakeFork);
        }

        private void PutDownFork()
        {
            byte philPos = (byte)_pipeServer.ReadByte();
            byte position = (byte)(philPos - 1);
            byte[] indexes = GetNormalizedForksIndexes(position, FORKS_PER_PHILOSOPHER);
            bool ok = false;
            for (int i = 0; i < FORKS_PER_PHILOSOPHER && !ok; ++i)
            {
                if (_forks[indexes[i]].IsTaken() && _forks[indexes[i]].GetOwner() == philPos)
                {
                    ok = true;
                    Console.WriteLine("Философ {0} кладет {1} вилку", philPos, i == 0 ? "левую" : "правую");
                    _forks[indexes[i]].PutDown();
                    _forks[indexes[i]].SetOwner(-1);
                }
            }
            if (!ok)
            {
                _pipeServer.WriteByte((byte)Response.FailedPutFork);
                return;
            }

            _pipeServer.WriteByte((byte)Response.OkPutFork);
        }
    }
}
