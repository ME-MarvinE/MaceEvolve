using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class Apple : GameObject
    {
        #region Fields
        public int _Servings = 1;
        #endregion
        #region Properties
        public int Servings { get; set; }
        public int EnergyPerServing { get; set; } = 10;
        public double ServingDigestionCost { get; set; } = 0.05;
        #endregion

        #region Constructors
        public Apple()
        {
        }
        #endregion
    }
}
