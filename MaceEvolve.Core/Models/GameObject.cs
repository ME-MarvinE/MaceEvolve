using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class GameObject : IGameObject
    {
        #region Properties
        public float X { get; set; }
        public float Y { get; set; }
        public float MX
        {
            get
            {
                return X + Size / 2;
            }
        }
        public float MY
        {
            get
            {
                return Y + Size / 2;
            }
        }
        public float Size { get; set; }
        #endregion
    }
}
