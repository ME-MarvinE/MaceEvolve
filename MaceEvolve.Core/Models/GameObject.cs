using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class GameObject : IGameObject
    {
        #region Fields
        private Rectangle _rectangle = new Rectangle(0, 0, 0, 0);
        #endregion

        #region Properties
        public double X
        {
            get
            {
                return Rectangle.X;
            }
            set
            {
                Rectangle.X = value;
            }
        }
        public double Y
        {
            get
            {
                return Rectangle.Y;
            }
            set
            {
                Rectangle.Y = value;
            }
        }
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
        public double Size
        {
            get
            {
                return Rectangle.Width;
            }
            set
            {
                Rectangle.Width = value;
                Rectangle.Height = value;
            }
        }
        public Rectangle Rectangle
        {
            get
            {
                return _rectangle;
            }
            set
            {
                _rectangle = value;
            }
        }

        #endregion
    }
}
