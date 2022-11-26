using MaceEvolve.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace MaceEvolve.Models
{
    public class GameObject
    {
        #region Fields
        private Rectangle _rectangle = new Rectangle(0, 0, 0, 0);
        private Color _color = Color.Black;
        #endregion

        #region Properties
        public GameHost GameHost { get; set; }
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
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
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

        #region Constructors
        public GameObject()
        {
        }
        #endregion

        #region Methods
        public virtual void Draw(PaintEventArgs e)
        {
            using (SolidBrush Brush = new SolidBrush(Color))
            {
                e.Graphics.FillEllipse(Brush, (float)X, (float)Y, (float)Size, (float)Size);
            }
        }
        public virtual void Update()
        {
        }
        #endregion
    }
}
