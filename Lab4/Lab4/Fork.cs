using System.Threading;

namespace Lab4
{
    class Fork
    {
        private Semaphore _forkLock;
        private bool _isTaken;
        private bool _isForEat;
        private int _index;

        public Fork(int index)
        {
            _index = index;
            _forkLock = new Semaphore(1, 1);
            _isTaken = false;
            _isForEat = true;
        }

        public void Take()
        {
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
            return !_isForEat;
        }

        public void Lock()
        {
            _forkLock.WaitOne();
        }

        public void Release()
        {
            _forkLock.Release();
        }
    }
}
