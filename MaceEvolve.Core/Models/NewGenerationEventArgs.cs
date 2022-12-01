using System;

namespace MaceEvolve.Core.Models
{
    public class NewGenerationEventArgs : EventArgs
    {
        #region Properties
        public int OldGenerationNumber { get; set; }
        public int NewGenerationNumber { get; set; }
        #endregion

        #region Constructors
        public NewGenerationEventArgs(int oldGenerationNumber, int newGenerationNumber)
        {
            OldGenerationNumber = oldGenerationNumber;
            NewGenerationNumber = newGenerationNumber;
        }
        #endregion
    }
}
