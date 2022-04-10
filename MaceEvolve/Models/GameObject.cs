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
        private Rectangle _Rectangle = new Rectangle();
        public Color _Color = Color.Black;
        #endregion

        #region Properties
        public GameHost GameHost { get; set; }
        public int X
        {
            get
            {
                return Rectangle.X;
            }
            set
            {
                Rectangle = new Rectangle(value, Y, Size, Size);
            }
        }
        public int Y
        {
            get
            {
                return Rectangle.Y;
            }
            set
            {
                Rectangle = new Rectangle(X, value, Size, Size);
            }
        }
        public int Size
        {
            get
            {
                return Rectangle.Width;
            }
            set
            {
                Rectangle = new Rectangle(X, Y, value, value);
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
                e.Graphics.FillEllipse(Brush, Rectangle);
            }
        }
        public virtual void Update()
        {
        }
        #endregion
    }
}
