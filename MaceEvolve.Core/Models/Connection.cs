﻿namespace MaceEvolve.Core.Models
{
    public class Connection
    {
        #region Properties
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public float Weight { get; set; }
        #endregion
    }
}
