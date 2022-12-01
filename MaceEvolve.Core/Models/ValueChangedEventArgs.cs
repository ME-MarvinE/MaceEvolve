using System;

namespace MaceEvolve.Core.Models
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        #region Properties
        public T OldValue { get; }
        public T NewValue { get; }
        #endregion

        #region Constructors
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
        #endregion
    }
}
