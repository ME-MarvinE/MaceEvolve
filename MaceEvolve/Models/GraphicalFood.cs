﻿using MaceEvolve.Core.Models;
using MaceEvolve.Interfaces;
using System.Drawing;

namespace MaceEvolve.Models
{
    public class GraphicalFood : Food, IGraphical
    {
        #region Fields
        private Color _color = Color.Black;
        #endregion

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
    }
}
