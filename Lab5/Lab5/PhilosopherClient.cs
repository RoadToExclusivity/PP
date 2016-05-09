using System;
using System.Threading;
using System.IO.Pipes;

namespace Lab5
{
    class PhilosopherClient
    {
        static private readonly int CYCLE_END_BORDER = 2;

        private bool _isHungry;

        private int _pos;
        private int _eatCount;
        private int _thinkCount;

        private int _allForksCount;
        private int _curForksCount;

        private NamedPipeClientStream _pipeClient;

        static private NamedPipeClientStream CreateClientPipe()
        {
            return new NamedPipeClientStream(".", LunchServer.SERVER_PIPE_NAME, 
                                            PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public PhilosopherClient()
        {
            _isHungry = true;
            _eatCount = 0;
            _thinkCount = 0;
            _allForksCount = LunchServer.FORKS_PER_PHILOSOPHER;
            _curForksCount = 0;
            _pos = 0;
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

        public void Activate()
        {
            Response response = GetResponse(Request.GetPhilosopherPosition);
            switch (response)
            {
                case Response.PhilosopherPosition:
                    _pos = _pipeClient.ReadByte() + 1;
                    Console.WriteLine("Мое место за столом - {0}", _pos);
                    break;
                case Response.InvalidPhilosopherPosition:
                    Console.WriteLine("Мне не нашлось места ((((...");
                    return;
                default:
                    Console.WriteLine("Непонятный ответ от сервера на запрос о получении моего места");
                    return;
            }
            while (_thinkCount < CYCLE_END_BORDER || _eatCount < CYCLE_END_BORDER)
            {
                if (_isHungry)
                {
                    if (_curForksCount == _allForksCount)
                    {
                        response = GetResponse(Request.Eat);
                        if (response == Response.OkEat)
                        {
                            Eat();
                            Console.WriteLine("Обедаю");
                        }
                        else
                        {
                            Console.WriteLine("Неизвестный ответ от сервера - ожидался ответ на покушать");
                        }
                    }
                    else
                    {
                        response = GetResponse(Request.TakeFork);
                        switch (response)
                        {
                            case Response.OkTakeFork:
                                Console.WriteLine("Взял вилку, для обеда осталось еще {0} вилок", _allForksCount - ++_curForksCount);
                                break;
                            case Response.FailedTakeFork:
                                Console.WriteLine("Пока не удается взять вилку. Нужно еще {0} вилок", _allForksCount - _curForksCount);
                                break;
                            default:
                                Console.WriteLine("Несовпадающий ответ от сервера");
                                break;
                        }
                    }
                }
                else
                {
                    if (_curForksCount == 0)
                    {
                        response = GetResponse(Request.Think);
                        if (response == Response.OkThink)
                        {
                            Think();
                            Console.WriteLine("Размышляю");
                        }
                        else
                        {
                            Console.WriteLine("Неизвестный ответ от сервера - ожидался ответ на подумать");
                        }
                    }
                    else
                    {
                        response = GetResponse(Request.PutFork);
                        switch (response)
                        {
                            case Response.OkPutFork:
                                Console.WriteLine("Положил вилку, для размышлений осталось еще {0} вилок", --_curForksCount);
                                break;
                            case Response.FailedPutFork:
                                Console.WriteLine("Не удалось положить вилку. Осталось еще {0} вилок", _curForksCount);
                                break;
                            default:
                                Console.WriteLine("Несовпадающий ответ от сервера");
                                break;
                        }
                    }
                }

                Thread.Sleep(200);
            }

            response = GetResponse(Request.Rest);
            if (response == Response.OkRest)
            {
                Console.WriteLine("Заканчиваю свою работу");
            }
            else
            {
                Console.WriteLine("Неизвестный ответ от сервера - ожидался ответ на отойти");
            }

            Console.ReadLine();
        }

        private void TryConnect(NamedPipeClientStream server)
        {
            try
            {
                server.Connect();
            }
            catch (Exception)
            {
                Console.WriteLine("Не могу найти где все обедают !");
                Thread.Sleep(500);
                TryConnect(server);
            }
        }

        private Response GetResponse(Request request)
        {
            _pipeClient = CreateClientPipe();
            TryConnect(_pipeClient);

            _pipeClient.WriteByte((byte)request);
            _pipeClient.WriteByte((byte)_pos);
            _pipeClient.WaitForPipeDrain();

            Response response = (Response)_pipeClient.ReadByte();
            return response;
        }
    }
}
