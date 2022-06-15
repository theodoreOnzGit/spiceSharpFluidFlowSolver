using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace spiceSharpFluidFlowSolverLibraries;
public class PipeFactory : BasePipeFactory
{

	// this pipefactory is just a class to help concretise the
	// base pipeFactory
	// the basePipeFactory is a scaffold to build other pipeFactories
	public PipeFactory(String pipeName) : base(pipeName)
	{
	}

	public PipeFactory(String pipeName,String inletName, String outletName)
		: base(pipeName,inletName,outletName)
	{
	}

}
