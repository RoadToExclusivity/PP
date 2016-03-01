using System.Threading;

namespace Lab4
{
    class Fork
    {
        private Semaphore _forkLock;
        private bool _isTaken;
        private bool _isForEat;

        public Fork()
        {
            _forkLock = new Semaphore(1, 1);
            _isTaken = false;
            _isForEat = false;
        }

        public void Take()
        {
            _forkLock.WaitOne();
            _isTaken = true;
            _isForEat = true;
            _forkLock.Release();
        }

        public void PutDown()
        {
            _forkLock.WaitOne();
            _isTaken = false;
            _isForEat = false; 
            _forkLock.Release();
        }

        public bool IsTaken()
        {
            return _isTaken;
        }

        public void SetForPut()
        {
            _forkLock.WaitOne();
            _isForEat = false;
            _forkLock.Release();
        }

        public bool IsForPut()
        {
            return _isForEat;
        }
    }
}
