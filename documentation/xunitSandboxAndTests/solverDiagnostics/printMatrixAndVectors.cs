using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;
using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;

namespace solverDiagnostics
{

	// the whole point of this class is to print matrices
	// and vectors
	// from the solver
	public interface IprintMatrixAndVectors
	{
		public void print(IEnumerable<double> array);
		public void print(double[][] array);
		public void print(ISolver<double> solver);
	}

	public class printMatrixAndVectors:IprintMatrixAndVectors
	{
	
		// first overload prints a double array matrix like
		public void print(double[][] array){
			string matrixRow;
			matrixRow = "";

			for (var r = 0; r < array.Length; r++)
			{
				for (var c = 0; c < array[r].Length; c++)
				{
					matrixRow += array[r][c].ToString() + " ";
				}
				// i'll write the matrixRow for each row, and set it to 0
				Console.WriteLine(matrixRow + "\n");
				matrixRow="";

			}

			return;
		}


		// second overload prints any IEnumerable<double>, which definitely includes
		// double arrays
		public void print(IEnumerable<double> array){

			string vectorRow;
			vectorRow = "";

			foreach (var item in array)
			{
				vectorRow = item.ToString();
				Console.WriteLine(vectorRow + "\n");
				vectorRow = "";
			}
		}

		// this code here the matrix AND Rhs Vector in the solver
		public void print(ISolver<double> solver){
			string matrixRow;
			matrixRow = "";

			Console.WriteLine("printing Ymatrix within the solver");

			// note that this works only for a square matrix
			for (var r = 0; r < solver.Size; r++)
			{
				for (var c = 0; c < solver.Size; c++)
				{
					matrixRow += solver[new MatrixLocation(r + 1, c + 1)].ToString()  + " ";
				}
				// i'll write the matrixRow for each row, and set it to 0
				Console.WriteLine(matrixRow + "\n");
				matrixRow="";

			}

			// next i want to print the RHS vector
			Console.WriteLine("printing RHS Vector within the solver");
			for (var r = 0; r < solver.Size; r++)
			{
				matrixRow += solver[r+1].ToString();
				Console.WriteLine(matrixRow + "\n");
				matrixRow="";
			}
		}

	}

	
}
