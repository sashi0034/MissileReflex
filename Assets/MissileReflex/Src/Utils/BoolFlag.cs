namespace MissileReflex.Src.Utils
{
    public interface IBoolFlagPeeker
    {
        public bool PeekFlag();
    }
    
    public class BoolFlag : IBoolFlagPeeker
    {
        private bool _flag = false;
        public bool Flag => _flag;

        public void UpFlag()
        {
            _flag = true;
        }

        public void Clear()
        {
            _flag = false;
        }

        public bool PeekFlag()
        {
            bool result = _flag;
            _flag = false;
            return result;
        }
    }
}