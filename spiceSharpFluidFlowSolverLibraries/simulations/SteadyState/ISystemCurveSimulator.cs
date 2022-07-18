﻿using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public interface ISystemCurveSimulator : 
		IBiasingSimulation
    {
		public IList<double> simulationResult { get; set; }
    }
}