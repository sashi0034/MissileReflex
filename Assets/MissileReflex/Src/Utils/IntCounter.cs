namespace MissileReflex.Src.Utils
{
    public class IntCounter
    {
        private int _value = 0;
        public int Value => _value;

        public void Reset(int value = 0)
        {
            _value = value;
        }

        public void CountNext()
        {
            _value++;
        }
    }
}