using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public abstract class Node<T>
    {
        public Type InputType { get; }
        public Node()
        {
            InputType = typeof(T);
        }
        public Func<T, double> Getter { get; }
    }
}
