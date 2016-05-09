namespace Lab5
{
    class Fork
    {
        private bool _isTaken;
        private bool _isForEat;
        private int _owner;

        public Fork()
        {
            _isTaken = false;
            _isForEat = true;
            _owner = -1;
        }

        public void Take()
        {
            _isTaken = true;
            SetForEat();
        }

        public void PutDown()
        {
            _isTaken = false;
            SetForPut();
        }

        public bool IsTaken()
        {
            return _isTaken;
        }

        public void SetForEat()
        {
            _isForEat = true;
        }

        public void SetForPut()
        {
            _isForEat = false;
        }

        public bool IsForEat()
        {
            return _isForEat;
        }

        public void SetOwner(int index)
        {
            _owner = index;
        }

        public int GetOwner()
        {
            return _owner;
        }
    }
}
