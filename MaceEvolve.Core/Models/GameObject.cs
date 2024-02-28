using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class GameObject : IGameObject
    {
        #region Fields
        private float _cachedX;
        private float _cachedY;
        private float _cachedSize;
        private float _cachedMX;
        private float _cachedMY;
        #endregion

        #region Properties
        public float X { get; set; }
        public float Y { get; set; }
        public float MX
        {
            get
            {
                if (!(X == _cachedX && Size == _cachedSize))
                {
                    _cachedX = X;
                    _cachedSize = Size;
                    _cachedMX = X + Size / 2;
                }

                return _cachedMX;
            }
        }
        public float MY
        {
            get
            {
                if (!(Y == _cachedY && Size == _cachedSize))
                {
                    _cachedY = Y;
                    _cachedSize = Size;
                    _cachedMY = Y + Size / 2;
                }

                return _cachedMY;
            }
        }
        public float Size { get; set; }
        public float Mass { get; set; }
        #endregion
    }
}
