using Xunit;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;
using System;
using System.IO;
using System.Numerics;

namespace xunitSandboxAndTests;

public class Sandbox
{
    [Fact]
    public void Test1()
    {
		Console.WriteLine("hi");

    }

	[Fact]
	public void When_SingletonPivoting_Expect_NoException()
	{
		// Build the solver with only the singleton pivoting
		var solver = new SparseRealSolver();
		solver.Parameters.Strategies.Clear();
		solver.Parameters.Strategies.Add(new MarkowitzSingleton<double>());

		// Build the matrix that should be solvable using only the singleton pivoting strategy
		double[][] matrix =
		{
			new double[] { 0, 0, 1, 0 },
			new double[] { 1, 1, 1, 1 },
			new double[] { 0, 0, 0, 1 },
			new double[] { 1, 0, 0, 0 }
		};
		double[] rhs = { 0, 1, 0, 0 };
		for (var r = 0; r < matrix.Length; r++)
		{
			for (var c = 0; c < matrix[r].Length; c++)
			{
				if (!matrix[r][c].Equals(0.0))
					solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
			}
			if (!rhs[r].Equals(0.0))
				solver.GetElement(r + 1).Value = rhs[r];
		}

		// This should run without throwing an exception
		Assert.Equal(solver.Size, solver.OrderAndFactor());
	}

	[Fact]
	public void When_ExampleComplexMatrix1_Expect_MatlabReference()
	{
		// Build the example matrix
		Complex[][] matrix =
		{
			new Complex[] { 0, 0, 0, 0, 1, 0, 1, 0 },
			new Complex[] { 0, 0, 0, 0, -1, 1, 0, 0 },
			new[] { 0, 0, new Complex(0.0, 0.000628318530717959), 0, 0, 0, -1, 1 },
			new Complex[] { 0, 0, 0, 0.001, 0, 0, 0, -1 },
			new Complex[] { 1, -1, 0, 0, 0, 0, 0, 0 },
			new Complex[] { 0, 1, 0, 0, 0, 0, 0, 0 },
			new Complex[] { 1, 0, -1, 0, 0, 0, 0, 0 },
			new[] { 0, 0, 1, -1, 0, 0, 0, new Complex(0.0, -1.5707963267949) }
		};
		Complex[] rhs = { 0, 0, 0, 0, 0, 24.0 };
		Complex[] reference =
		{
			new Complex(24, 0),
			new Complex(24, 0),
			new Complex(24, 0),
			new Complex(23.999940782519708, -0.037699018824477),
			new Complex(-0.023999940782520, -0.015041945718407),
			new Complex(-0.023999940782520, -0.015041945718407),
			new Complex(0.023999940782520, 0.015041945718407),
			new Complex(0.023999940782520, -0.000037699018824)
		};

		// build the matrix
		var solver = new SparseComplexSolver();
		for (var r = 0; r < matrix.Length; r++)
		{
			for (var c = 0; c < matrix[r].Length; c++)
			{
				if (!matrix[r][c].Equals(Complex.Zero))
					solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
			}
		}

		// Add some zero elements
		solver.GetElement(new MatrixLocation(7, 7));
		solver.GetElement(5);

		// Build the Rhs vector
		for (var r = 0; r < rhs.Length; r++)
		{
			if (!rhs[r].Equals(Complex.Zero))
				solver.GetElement(r + 1).Value = rhs[r];
		}

		// Solver
		Assert.Equal(solver.Size, solver.OrderAndFactor());
		var solution = new DenseVector<Complex>(solver.Size);
		solver.Solve(solution);

		// Check!
		for (var r = 0; r < reference.Length; r++)
		{
			Assert.Equal(reference[r].Real, solution[r + 1].Real, 12);
			Assert.Equal(reference[r].Imaginary, solution[r + 1].Imaginary, 12);
		}
	}
}
