using Xunit;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;
using System;
using System.IO;
using System.Numerics;
using solverDiagnostics;

namespace xunitSandboxAndTests;

public class TestPrintMatrixAndVectors
{
	public IprintMatrixAndVectors _printObj { get; set; }
	public TestPrintMatrixAndVectors()
	{
		this._printObj = new printMatrixAndVectors();
	}

	// this test is here to check out the solverDiagnosticsTools
	[Fact]
	public void Test_shouldPrintDoubleArray(){
		
		Console.WriteLine("printMatrixAndVectors double[][] test \n");
		double[][] matrix =
		{
			new double[] { 0, 0, 1, 0 },
			new double[] { 1, 12, 1, 1 },
			new double[] { 0, 0, 0, 1 },
			new double[] { 1, 0, 0, 0 }
		};

		_printObj.print(matrix);
	}

	[Fact]
	public void Test_shouldPrintDoubleEnumerable(){
		
		Console.WriteLine("printMatrixAndVectors double[] test \n");
		double[] vector = new double[] { 0, 0, 1, 0 };

		_printObj.print(vector);
	}

	[Fact]
    public void Test_shouldPrintMatrixAndRHSVectorWithinSolver()
    {

		// here we have a matrix and a double vector
		// both of them i want to load into the solver
		double[][] matrix =
		{
			new double[] { 0, 0, 1, 0 },
			new double[] { 1, 12, 1, 1 },
			new double[] { 0, 0, 0, 1 },
			new double[] { 1, 0, 0, 0 }
		};

		double[] rhs = { 0, 1, 133, 0 };


		// so first i load the solver object
		var solver = new SparseRealSolver();

		// then i load the matrix and double values into the solver matrix
		// and RHS vector

		for (var r = 0; r < matrix.Length; r++)
		{
			for (var c = 0; c < matrix[r].Length; c++)
			{
				if (!matrix[r][c].Equals(0.0)){
					solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
				}

			}
			if (!rhs[r].Equals(0.0))
				solver.GetElement(r + 1).Value = rhs[r];
		}


		_printObj.print(solver);


    }

	[Fact]
    public void Test_WhatIfYmatrixNotDefined()
    {

		Console.WriteLine("This test is if the solver object is missing YMatrix ");
		// here we have a matrix and a double vector
		// both of them i want to load into the solver

		double[] rhs = { 0, 1, 133, 0 };


		// so first i load the solver object
		var solver = new SparseRealSolver();

		// then i load the double vector values into the solver 
		// and RHS vector

		for (var r = 0; r < rhs.Length; r++)
		{
			solver.GetElement(r + 1).Value = rhs[r];
		}


		_printObj.print(solver);


    }

	[Fact]
    public void Test_WhatIfRHSVectorNotDefined()
    {

		Console.WriteLine("this is a test if the solver is missing RHS Vector ");
		// here we have a matrix and a double vector
		// both of them i want to load into the solver


		double[][] matrix =
		{
			new double[] { 0, 0, 1, 0 },
			new double[] { 1, 12, 1, 1 },
			new double[] { 0, 0, 0, 1 },
			new double[] { 1, 0, 0, 0 }
		};

		// so first i load the solver object
		var solver = new SparseRealSolver();

		// then i load the double vector values into the solver 
		// and RHS vector

		for (var r = 0; r < matrix.Length; r++)
		{
			for (var c = 0; c < matrix[r].Length; c++)
			{
				if (!matrix[r][c].Equals(0.0)){
					solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
				}

			}
		}



		_printObj.print(solver);


    }

}
