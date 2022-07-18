﻿using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public interface ISteadyStateFlowSimulation : 
		IBiasingSimulation
    {
		public double simulationResult { get; set; }
		public string simulationMode { get; set ; }
    }
}
