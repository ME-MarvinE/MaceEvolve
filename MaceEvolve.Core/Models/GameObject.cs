using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class GameObject : IGameObject
    {
        #region Properties
        public double X { get; set; }
        public double Y { get; set; }
        public double MX
        {
            get
            {
                return X + Size / 2;
            }
        }
        public double MY
        {
            get
            {
                return Y + Size / 2;
            }
        }
        public double Size { get; set; }
        #endregion
    }
}
