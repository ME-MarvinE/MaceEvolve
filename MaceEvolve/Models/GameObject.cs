using MaceEvolve.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public abstract class GameObject
    {
        #region Fields
        protected static Random _Random = new Random();
        private Rectangle _Rectangle = new Rectangle(0,0,0,0);
        public Color _Color = Color.Black;
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
                return X - Size / 2;
            }
        }
        public double MY
        {
            get
            {
                return Y - Size / 2;
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
                return _Color;
            }
            set
            {
                _Color = value;
            }
        }
        public Rectangle Rectangle
        {
            get
            {
                return _Rectangle;
            }
            set
            {
                _Rectangle = value;
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
                e.Graphics.FillEllipse(Brush, (float)MX, (float)MY, (float)Size, (float)Size);
            }
        }
        public virtual void Update()
        {
        }
        #endregion
    }
}
