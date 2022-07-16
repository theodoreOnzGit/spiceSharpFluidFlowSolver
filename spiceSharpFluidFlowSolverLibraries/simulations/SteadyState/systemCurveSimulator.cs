using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public class systemCurveSimulator : 
		BiasingSimulation, ISystemCurveSimulator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OP"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public systemCurveSimulator(string name)
            : base(name)
        {
        }

		public IList<double> simulationResult { get; set; }
		= new List<double>();

		string simulationMode = "vanilla";

        /// <inheritdoc/>
        protected override void Execute()
        {
			// these two lings of code are for me to force source stepping
			//BiasingParameters.NoOperatingPointIterate = true;
			//BiasingParameters.GminSteps = 0;
            base.Execute();

			switch (simulationMode)
			{
				case "vanilla":
					Op(BiasingParameters.DcMaxIterations);
					break;

				case "sourceStepping":
					int maxiterations = 100;
					int sourceSteps = 10;
					IterateSourceStepping(maxiterations,sourceSteps);
					break;
			
				default:
					break;
			}
            var exportargs = new ExportDataEventArgs(this);
            OnExport(exportargs);
        }



    }
}
