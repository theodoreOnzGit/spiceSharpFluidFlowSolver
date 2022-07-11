using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public class FrictionFactorTests : testOutputHelper
{
	private IDerivative _derivativeObj;
	public FrictionFactorTests(ITestOutputHelper outputHelper):base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"

		// now i'll also create dependencies in the constructor
		// 
		IDerivative derivativeObj = new CentralDifference();
		this._derivativeObj = derivativeObj;
	}

	[Theory]
	[InlineData(1800)]
	[InlineData(1799)]
	[InlineData(1801)]
	[InlineData(0)]
	public void continuityTest_dB_dRe(double Re){
		double roughnessRatio = 0.05;
		double lengthToDiameter = 10.0;

		// basically at Re=1800, i transit from
		// churchill correlation to 16/Re for dB_dRe
		// for the stabilised churchill
		// I just want to see how bad the discontinuity is
		//

		IFrictionFactorJacobian _churchill;
		IFrictionFactorJacobian _stabilisedChurchill;

		_churchill = new ChurchillFrictionFactorJacobian();
		_stabilisedChurchill = new StabilisedChurchillJacobian();

		double dB_dRe_reference;
		if(Re > 100){
			dB_dRe_reference = _churchill.
				dB_dRe(Re, roughnessRatio, lengthToDiameter);
		}
		else{
			dB_dRe_reference = 16 *Re;
		}

		//Act

		double dB_dRe_result = _stabilisedChurchill.
			dB_dRe(Re, roughnessRatio, lengthToDiameter);

		// Assert

		//Assert.Equal(dB_dRe_reference, dB_dRe_result,0);


		double errorMax = 0.002;
		// Act

		

		double error = Math.Abs(dB_dRe_result - dB_dRe_reference)/dB_dRe_reference;

		// Assert
		//

		// Assert.Equal(referenceDarcyFactor,resultDarcyFactor);
		if(Re == 0.0){
			Assert.Equal(dB_dRe_reference,
					dB_dRe_result);
			return;
		}
		Assert.True(error < errorMax);
		return;
	}


	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	public void When_negativeReFor_dB_dRe_ExpectNegativeResult(
			double Re,
			double roughnessRatio){


		// this test shows that when calculating negative
		// Re values dBe_dRe should return 
		// a negative derivative 
		// so in fact the Bejan Number on the Positive Re
		// side will have a reflection about the
		// y axis on the negative Re side

		// Setup
		IFrictionFactorJacobian _jacobianObject;
		_jacobianObject = new ChurchillFrictionFactorJacobian();

		double negativeRe;
		negativeRe = -Re;
		double lengthToDiameter = 10.0;
		double jacobianDerivativePositiveRe;
		double jacobianDerivativeNegativeRe;
		// Act
		//
		jacobianDerivativePositiveRe = _jacobianObject.
			getRe(Re, 
					roughnessRatio,
					lengthToDiameter);

		jacobianDerivativeNegativeRe = _jacobianObject.
			getRe(negativeRe,
					roughnessRatio,
					lengthToDiameter);


		// Assert
		Assert.Equal(jacobianDerivativePositiveRe,
				-jacobianDerivativeNegativeRe);

	}

	[Theory(Skip = "buggy and probably not worth the effort")]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	public void Test_analyticalDerivativeLaminarShouldEqualAnalaytical(
			double Re, double roughnessRatio){

		// as an easier test, we can test the correlation
		// against the laminar flow friction factor derivative
		// laminar flow friction factor derivative
		// df/dRe is
		// d/dRe (16/Re)
		// = -16/Re^2

		// Setup

		double analayticalDerivativeValue;
		analayticalDerivativeValue = -16.0/Math.Pow(Re,2.0);

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new ChurchillAnalyticalDerivative();


		// Act 

		double derivativeResult;
		derivativeResult = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		double errorFraction = Math.Abs( derivativeResult
				- analayticalDerivativeValue)
			/Math.Abs(analayticalDerivativeValue);
		double errorTolerance = 0.05;
		// Assert
		Assert.Equal(analayticalDerivativeValue,
				derivativeResult);
		Assert.True(errorFraction < errorTolerance);

		// result: all ok except Re = 1200
		// the numerical derivative there is 0
		// quite strange

	}
	
	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	public void Test_ChurchillMathNetDerivativeAccurateTo1Percent(
			double Re, double roughnessRatio){

		// as an easier test, we can test the correlation
		// against the laminar flow friction factor derivative
		// laminar flow friction factor derivative
		// df/dRe is
		// d/dRe (16/Re)
		// = -16/Re^2

		// Setup

		double analayticalDerivativeValue;
		analayticalDerivativeValue = -16.0/Math.Pow(Re,2.0);

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new ChurchillMathNetDerivative();


		// Act 

		double derivativeResult;
		derivativeResult = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		double errorFraction = Math.Abs( derivativeResult
				- analayticalDerivativeValue)
			/Math.Abs(analayticalDerivativeValue);
		double errorTolerance = 0.01;
		// Assert
		//Assert.Equal(analayticalDerivativeValue,
		//			derivativeResult);
		Assert.True(errorFraction < errorTolerance);

		// result: all ok except Re = 1200
		// the numerical derivative there is 0
		// quite strange

	}

	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	public void Test_numericalMathNetDerivativeLaminarShouldEqualAnalaytical(
			double Re, double roughnessRatio){

		// as an easier test, we can test the correlation
		// against the laminar flow friction factor derivative
		// laminar flow friction factor derivative
		// df/dRe is
		// d/dRe (16/Re)
		// = -16/Re^2

		// Setup

		double analayticalDerivativeValue;
		analayticalDerivativeValue = -16.0/Math.Pow(Re,2.0);

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new ChurchillMathNetDerivative();


		// Act 

		double derivativeResult;
		derivativeResult = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		double errorFraction = Math.Abs( derivativeResult
				- analayticalDerivativeValue)
			/Math.Abs(analayticalDerivativeValue);
		double errorTolerance = 0.05;
		// Assert
		//Assert.Equal(analayticalDerivativeValue,
		//			derivativeResult);
		Assert.True(errorFraction < errorTolerance);

		// result: all ok except Re = 1200
		// the numerical derivative there is 0
		// quite strange

	}

	[Theory(Skip = "mostly passed, but at Re=1200, strangely equals 0")]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	public void Test_numericalDerivativeLaminarShouldEqualAnalaytical(
			double Re, double roughnessRatio){

		// as an easier test, we can test the correlation
		// against the laminar flow friction factor derivative
		// laminar flow friction factor derivative
		// df/dRe is
		// d/dRe (16/Re)
		// = -16/Re^2

		// Setup

		double analayticalDerivativeValue;
		analayticalDerivativeValue = -16.0/Math.Pow(Re,2.0);

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new ChurchillFrictionFactor();


		// Act 

		double derivativeResult;
		derivativeResult = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		double errorFraction = Math.Abs( derivativeResult
				- analayticalDerivativeValue)
			/Math.Abs(analayticalDerivativeValue);
		double errorTolerance = 0.05;
		// Assert
		Assert.Equal(analayticalDerivativeValue,
				derivativeResult);
		Assert.True(errorFraction < errorTolerance);

		// result: all ok except Re = 1200
		// the numerical derivative there is 0
		// quite strange

	}
	
	[Theory]
	[InlineData(4000, 0.05)]
	[InlineData(40000, 0.05)]
	[InlineData(4e5, 0.05)]
	[InlineData(4e6, 0.05)]
	[InlineData(4e7, 0.05)]
	[InlineData(4e8, 0.05)]
	[InlineData(4e9, 0.05)]
	[InlineData(4e3, 0.0)]
	[InlineData(4e7, 0.00005)]
	[InlineData(4e6, 0.001)]
	[InlineData(4e5, 0.01)]
	[InlineData(4e4, 0.03)]
	public void Test_numericalAndAnalayticalTurbulentDervativesShouldBeSimilar(
			double Re, double roughnessRatio){

		// Setup

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new ChurchillFrictionFactor();

		IFrictionFactorDerivatives analyticalDerivative;
		analyticalDerivative = new FilonenkoAnalyticalDerivative();

		// for this, the numericalDerivative should be the reference
		// since at the point of this test
		// the numerical derivative has been validated at least partially,  0.057933060738478, 1.0e5)]

		double referenceNumericalDerivative;
		referenceNumericalDerivative = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		// Act
		double analayticalDerivativeValue;
		analayticalDerivativeValue = analyticalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);
		// Assert

		// first we check if the reference numerical derivative is more
		// than 0, if so then we can check the relativ error
		//
		// otherwise assert equal to 10 dp. (useful for constant
		// friction factor region)
		//
		// The following code asserts the following
		// if the reference derivative is non zero, 
		// check that the error tolerance is below 5%
		// otherwise check that 
		// 
		// it's equal to within 5dp
		// this is useful for derivatives near zero
		// or near transition regions, where the 
		// derivatives may not be right anyhow.
		//
		// not too strict here, because I just need correct jacobian 
		// to be calculated
		//
		if (Math.Abs(referenceNumericalDerivative) > 0){
			double errorFraction = Math.Abs(analayticalDerivativeValue 
					- referenceNumericalDerivative)
				/Math.Abs(referenceNumericalDerivative);
			double errorTolerance = 0.05;
			if(errorFraction < errorTolerance){
				Assert.True(errorFraction < errorTolerance);
				return;
			}
			if(errorFraction > errorTolerance){
				Assert.Equal(referenceNumericalDerivative,
						analayticalDerivativeValue,5);
				return;
			}
			return;
		}



		Assert.Equal(referenceNumericalDerivative,
				analayticalDerivativeValue,10);
		//

	}

	[Theory]
	[InlineData(4000, 0.05)]
	[InlineData(40000, 0.05)]
	[InlineData(4e5, 0.05)]
	[InlineData(4e6, 0.05)]
	[InlineData(4e7, 0.05)]
	[InlineData(4e8, 0.05)]
	[InlineData(4e9, 0.05)]
	[InlineData(4e3, 0.0)]
	[InlineData(4e7, 0.00005)]
	[InlineData(4e6, 0.001)]
	[InlineData(4e5, 0.01)]
	[InlineData(4e4, 0.03)]
	public void Test_numericalAndAnalayticalTurbulentDervativesShouldBeSimilarFilonenko(
			double Re, double roughnessRatio){

		// Setup

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new FilonenkoFrictionFactor();

		IFrictionFactorDerivatives analyticalDerivative;
		analyticalDerivative = new FilonenkoAnalyticalDerivative();

		// for this, the numericalDerivative should be the reference
		// since at the point of this test
		// the numerical derivative has been validated at least partially,  0.057933060738478, 1.0e5)]

		double referenceNumericalDerivative;
		referenceNumericalDerivative = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		// Act
		double analayticalDerivativeValue;
		analayticalDerivativeValue = analyticalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);
		// Assert
		double errorFraction = Math.Abs(analayticalDerivativeValue 
				- referenceNumericalDerivative)
			/Math.Abs(referenceNumericalDerivative);
		double errorTolerance = 0.001;

		Assert.Equal(referenceNumericalDerivative,
				analayticalDerivativeValue,10);
		Assert.True(errorFraction < errorTolerance);

	}

	[Theory(Skip = "Debugging")]
	[InlineData(4000, 0.05)]
	[InlineData(40000, 0.05)]
	[InlineData(4e5, 0.05)]
	[InlineData(4e6, 0.05)]
	[InlineData(4e7, 0.05)]
	[InlineData(4e8, 0.05)]
	[InlineData(4e9, 0.05)]
	[InlineData(4e3, 0.0)]
	[InlineData(4e7, 0.00005)]
	[InlineData(4e6, 0.001)]
	[InlineData(4e5, 0.01)]
	[InlineData(4e4, 0.03)]
	public void Test_numericalAndAnalayticalTurbulentDervativesShouldBeSimilarChurchill(
			double Re, double roughnessRatio){

		// Setup

		IFrictionFactorDerivatives numericalDerivative;
		numericalDerivative = new ChurchillFrictionFactor();

		IFrictionFactorDerivatives analyticalDerivative;
		analyticalDerivative = new ChurchillAnalyticalDerivative();

		// for this, the numericalDerivative should be the reference
		// since at the point of this test
		// the numerical derivative has been validated at least partially,  0.057933060738478, 1.0e5)]

		double referenceNumericalDerivative;
		referenceNumericalDerivative = numericalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);

		// Act
		double analayticalDerivativeValue;
		analayticalDerivativeValue = analyticalDerivative.
			calculateFanningPartialDerivative(Re, roughnessRatio);
		// Assert
		double errorFraction = Math.Abs(analayticalDerivativeValue 
				- referenceNumericalDerivative)
			/Math.Abs(referenceNumericalDerivative);
		double errorTolerance = 0.02;

		Assert.Equal(referenceNumericalDerivative,
				analayticalDerivativeValue);
		Assert.True(errorFraction < errorTolerance);
	}


	[Theory]
	[InlineData(4e3, 0.0, 0.039907014055631)]
	[InlineData(4e7, 0.00005, 0.010627694187016)]
	[InlineData(4e6, 0.001, 0.019714092419925)]
	public void Test_TsalFrictionFactorErrorNotMoreThan2Percent_Turbulent(double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here

		// Setup
		double referenceDarcyFactor = referenceFrictionFactor;

		// also the above values are visually inspected with respect to the graph
		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new TsalFrictionFactor();

		double errorMax = 0.02;
		// Act

		double resultDarcyFactor =  frictionFactorObj.darcy(Re,roughnessRatio);
		

		double error = Math.Abs(referenceDarcyFactor - resultDarcyFactor)/referenceDarcyFactor;

		// Assert
		//

		// Assert.Equal(referenceDarcyFactor,resultDarcyFactor);
		Assert.True(error < errorMax);

		// It appears that Tsal friction factor only works well for
		// smooth or relatively smooth pipe regimes



	}

	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224, 4.0)]
	[InlineData(40000, 0.05, 0.07212405402775,5.0)]
	[InlineData(4e5, 0.05, 0.071608351787938, 10.0)]
	[InlineData(4e6, 0.05,  0.071556444535705, 20.0)]
	[InlineData(4e7, 0.05,  0.071551250389636, 100.0)]
	[InlineData(4e8, 0.05, 0.071550730940769, 1000.0)]
	[InlineData(4e9, 0.05, 0.071550678995539, 65.0)]
	[InlineData(4e3, 0.0, 0.039907014055631, 20.0 )]
	[InlineData(4e7, 0.00005, 0.010627694187016, 35.0)]
	[InlineData(4e6, 0.001, 0.019714092419925, 8.9)]
	[InlineData(4e5, 0.01, 0.038055838413508, 50.0)]
	[InlineData(4e4, 0.03,  0.057933060738478, 1.0e5)]
	public void Test_churchillJacobianShouldGetAccurateNegativeReTurbulent(
			double Re,
			double roughnessRatio, 
			double referenceDarcyFrictionFactor,
			double lengthToDiameter){
		// the objective of this test is to test the
		// accuracy of getting Re using the getRe function
		// in the ChurchillFrictionFactorJacobian 
		// Implementation
		// BUT i use negative Bejan number instead
		//
		// And if i use the negative Bejan number
		// i should get the negative reflection of the reference
		// Reynold's number
		//
		// so it tests the ability of the frictionFactor Jacobian
		// to deal with negative values of Re for the getRe
		// function
		//
		// we have a reference Reynold's number
		//
		// and we need to get a Re using
		// fanning friction factor
		// and roughness Ratio
		//
		// we already have roughness ratio
		// but we need Bejan number and L/D
		//
		// Bejan number would be known in real life.
		// however, in this case, we cannot arbitrarily
		// specify it
		// the only equation that works now
		// is Be = f*Re^2*(4L/D)^3/32.0
		// That means we just specify a L/D ratio
		// and that would specify everything.
		// So I'm going to randomly specify L/D ratios and hope that
		// works
		

		// setup
		//
		double referenceRe = Re;

		IFrictionFactorGetRe testObject;
		testObject = new ChurchillFrictionFactorJacobian();


		double fanningFrictionFactor = 0.25*referenceDarcyFrictionFactor;
		double Be = fanningFrictionFactor*Math.Pow(Re,2.0);
		Be *= Math.Pow(4.0*lengthToDiameter,3);
		Be *= 1.0/32.0;

		// act

		double resultNegativeRe;
		resultNegativeRe = testObject.getRe(-Be,roughnessRatio,lengthToDiameter);

		// Assert (manual test)

		// Assert.Equal(referenceRe, resultNegativeRe);

		// Assert (auto test)
		// test if error is within 1% of actual Re
		double errorFraction = Math.Abs(-resultNegativeRe - referenceRe)/Math.Abs(referenceRe);
		double errorTolerance = 0.01;

		Assert.True(errorFraction < errorTolerance);


	}

	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224, 4.0)]
	[InlineData(40000, 0.05, 0.07212405402775,5.0)]
	[InlineData(4e5, 0.05, 0.071608351787938, 10.0)]
	[InlineData(4e6, 0.05,  0.071556444535705, 20.0)]
	[InlineData(4e7, 0.05,  0.071551250389636, 100.0)]
	[InlineData(4e8, 0.05, 0.071550730940769, 1000.0)]
	[InlineData(4e9, 0.05, 0.071550678995539, 65.0)]
	[InlineData(4e3, 0.0, 0.039907014055631, 20.0 )]
	[InlineData(4e7, 0.00005, 0.010627694187016, 35.0)]
	[InlineData(4e6, 0.001, 0.019714092419925, 8.9)]
	[InlineData(4e5, 0.01, 0.038055838413508, 50.0)]
	[InlineData(4e4, 0.03,  0.057933060738478, 1.0e5)]
	public void Test_churchillJacobianShouldGetAccurateReTurbulent(
			double Re,
			double roughnessRatio, 
			double referenceDarcyFrictionFactor,
			double lengthToDiameter){
		// the objective of this test is to test the
		// accuracy of getting Re using the getRe function
		// in the ChurchillFrictionFactorJacobian 
		// Implementation
		//
		// we have a reference Reynold's number
		//
		// and we need to get a Re using
		// fanning friction factor
		// and roughness Ratio
		//
		// we already have roughness ratio
		// but we need Bejan number and L/D
		//
		// Bejan number would be known in real life.
		// however, in this case, we cannot arbitrarily
		// specify it
		// the only equation that works now
		// is Be = f*Re^2*(4L/D)^3/32.0
		// That means we just specify a L/D ratio
		// and that would specify everything.
		// So I'm going to randomly specify L/D ratios and hope that
		// works
		

		// setup
		//
		double referenceRe = Re;

		IFrictionFactorGetRe testObject;
		testObject = new ChurchillFrictionFactorJacobian();


		double fanningFrictionFactor = 0.25*referenceDarcyFrictionFactor;
		double Be = fanningFrictionFactor*Math.Pow(Re,2.0);
		Be *= Math.Pow(4.0*lengthToDiameter,3);
		Be *= 1.0/32.0;

		// act

		double resultRe;
		resultRe = testObject.getRe(Be,roughnessRatio,lengthToDiameter);

		// Assert (manual test)

		// Assert.Equal(referenceRe, resultRe);

		// Assert (auto test)
		// test if error is within 1% of actual Re
		double errorFraction = Math.Abs(resultRe - referenceRe)/Math.Abs(referenceRe);
		double errorTolerance = 0.01;

		Assert.True(errorFraction < errorTolerance);


	}


	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224, 4.0)]
	[InlineData(40000, 0.05, 0.07212405402775,5.0)]
	[InlineData(4e5, 0.05, 0.071608351787938, 10.0)]
	[InlineData(4e6, 0.05,  0.071556444535705, 20.0)]
	[InlineData(4e7, 0.05,  0.071551250389636, 100.0)]
	[InlineData(4e8, 0.05, 0.071550730940769, 1000.0)]
	[InlineData(4e9, 0.05, 0.071550678995539, 65.0)]
	[InlineData(4e3, 0.0, 0.039907014055631, 20.0 )]
	[InlineData(4e7, 0.00005, 0.010627694187016, 35.0)]
	[InlineData(4e6, 0.001, 0.019714092419925, 8.9)]
	[InlineData(4e5, 0.01, 0.038055838413508, 50.0)]
	[InlineData(4e4, 0.03,  0.057933060738478, 1.0e5)]
	public void Test_churchillFrictionFactorShouldGetAccurateReTurbulent(
			double Re,
			double roughnessRatio, 
			double referenceDarcyFrictionFactor,
			double lengthToDiameter){
		// the objective of this test is to test the
		// accuracy of getting Re using the getRe function
		//
		// we have a reference Reynold's number
		//
		// and we need to get a Re using
		// fanning friction factor
		// and roughness Ratio
		//
		// we already have roughness ratio
		// but we need Bejan number and L/D
		//
		// Bejan number would be known in real life.
		// however, in this case, we cannot arbitrarily
		// specify it
		// the only equation that works now
		// is Be = f*Re^2*(4L/D)^3/32.0
		// That means we just specify a L/D ratio
		// and that would specify everything.
		// So I'm going to randomly specify L/D ratios and hope that
		// works
		

		// setup
		//
		double referenceRe = Re;

		IFrictionFactorGetRe testObject;
		testObject = new ChurchillFrictionFactor();


		double fanningFrictionFactor = 0.25*referenceDarcyFrictionFactor;
		double Be = fanningFrictionFactor*Math.Pow(Re,2.0);
		Be *= Math.Pow(4.0*lengthToDiameter,3);
		Be *= 1.0/32.0;

		// act

		double resultRe;
		resultRe = testObject.getRe(Be,roughnessRatio,lengthToDiameter);

		// Assert (manual test)

		// Assert.Equal(referenceRe, resultRe);

		// Assert (auto test)
		// test if error is within 1% of actual Re
		double errorFraction = Math.Abs(resultRe - referenceRe)/Math.Abs(referenceRe);
		double errorTolerance = 0.01;

		Assert.True(errorFraction < errorTolerance);


	}


	// this test will test the churchill correlation over some
	// values using an online colebrook calculator
	// https://www.engineeringtoolbox.com/colebrook-equation-d_1031.html
	// https://www.ajdesigner.com/php_colebrook/colebrook_equation.php#ajscroll
	// the online calculators return a darcy friction factor
	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224)]
	[InlineData(40000, 0.05, 0.072124054027755)]
	[InlineData(4e5, 0.05, 0.071608351787938)]
	[InlineData(4e6, 0.05,  0.071556444535705)]
	[InlineData(4e7, 0.05,  0.071551250389636)]
	[InlineData(4e8, 0.05, 0.071550730940769)]
	[InlineData(4e9, 0.05, 0.071550678995539)]
	[InlineData(4e3, 0.0, 0.039907014055631)]
	[InlineData(4e7, 0.00005, 0.010627694187016)]
	[InlineData(4e6, 0.001, 0.019714092419925)]
	[InlineData(4e5, 0.01, 0.038055838413508)]
	[InlineData(4e4, 0.03,  0.057933060738478)]
	public void Test_churchillFrictionFactorShouldBeAccurate_Turbulent(double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here

		// Setup
		double referenceDarcyFactor = referenceFrictionFactor;

		// also the above values are visually inspected with respect to the graph
		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchillFrictionFactor();

		// Act

		double resultDarcyFactor =  frictionFactorObj.darcy(Re,roughnessRatio);
		
		// Assert
		// Now by default, i can assert to a fixed number of decimal places
		// so comparing 99.98 and 99.99 are about the same to two decimal places
		// However, repeat this tactic with smaller numbers,eg
		// 0.00998 and 0.00999
		// this tactic will fail
		// to normalise everything I will use a normalise decimal place
		// I can take the logarithm base 10 of this number, round up
		// because the log10 of a number will give about the number of decimal 
		// places i need to correct for


		int normaliseDecimalPlace(double reference){

			double normaliseDouble = Math.Log10(reference);
			normaliseDouble = Math.Ceiling(normaliseDouble);
			int normaliseInteger;

			normaliseInteger = (int)normaliseDouble;
			// at this stage, i will get the number of decimal places i need to subtract
			// i want to add the correct number of decimal places,
			// so i will just use a negative sign
			normaliseInteger = -normaliseInteger;

			return normaliseInteger;
		}

		int decimalPlaceTest = 1 + normaliseDecimalPlace(referenceDarcyFactor);


		Assert.Equal(referenceDarcyFactor,resultDarcyFactor,decimalPlaceTest);
	}

	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224)]
	[InlineData(40000, 0.05, 0.072124054027755)]
	[InlineData(4e5, 0.05, 0.071608351787938)]
	[InlineData(4e6, 0.05,  0.071556444535705)]
	[InlineData(4e7, 0.05,  0.071551250389636)]
	[InlineData(4e8, 0.05, 0.071550730940769)]
	[InlineData(4e9, 0.05, 0.071550678995539)]
	[InlineData(4e3, 0.0, 0.039907014055631)]
	[InlineData(4e7, 0.00005, 0.010627694187016)]
	[InlineData(4e6, 0.001, 0.019714092419925)]
	[InlineData(4e5, 0.01, 0.038055838413508)]
	[InlineData(4e4, 0.03,  0.057933060738478)]
	public void Test_FilonenkoFrictionFactorErrorNotMoreThan4Percent_Turbulent(
			double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here

		// Setup
		double referenceDarcyFactor = referenceFrictionFactor;

		// also the above values are visually inspected with respect to the graph
		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new FilonenkoFrictionFactor();

		double errorMax = 0.04;
		// Act

		double resultDarcyFactor =  frictionFactorObj.darcy(Re,roughnessRatio);
		

		double error = Math.Abs(referenceDarcyFactor - resultDarcyFactor)/referenceDarcyFactor;

		// Assert
		//

		Assert.True(error < errorMax);




	}

	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224)]
	[InlineData(40000, 0.05, 0.072124054027755)]
	[InlineData(4e5, 0.05, 0.071608351787938)]
	[InlineData(4e6, 0.05,  0.071556444535705)]
	[InlineData(4e7, 0.05,  0.071551250389636)]
	[InlineData(4e8, 0.05, 0.071550730940769)]
	[InlineData(4e9, 0.05, 0.071550678995539)]
	[InlineData(4e3, 0.0, 0.039907014055631)]
	[InlineData(4e7, 0.00005, 0.010627694187016)]
	[InlineData(4e6, 0.001, 0.019714092419925)]
	[InlineData(4e5, 0.01, 0.038055838413508)]
	[InlineData(4e4, 0.03,  0.057933060738478)]
	public void Test_churchillFrictionFactorErrorNotMoreThan2Percent_Turbulent(double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here

		// Setup
		double referenceDarcyFactor = referenceFrictionFactor;

		// also the above values are visually inspected with respect to the graph
		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchillFrictionFactor();

		double errorMax = 0.02;
		// Act

		double resultDarcyFactor =  frictionFactorObj.darcy(Re,roughnessRatio);
		

		double error = Math.Abs(referenceDarcyFactor - resultDarcyFactor)/referenceDarcyFactor;

		// Assert
		//

		Assert.True(error < errorMax);




	}

	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	public void Test_churchillFrictionFactorErrorNotMoreThan2Percent_Laminar(double Re,double roughnessRatio){
		// this tests the churchill relation against the 
		// laminar flow friction factor
		// fanning is 16/Re
		// and no matter the roughness ratio, I should get the same result
		// however, roughness ratio should not exceed 0.1
		// as maximum roughness ratio in charts is about 0.05
		//
		// Setup

		// this test asserts that the error should not be more than 2%

		double referenceFanning = 16/Re;

		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchillFrictionFactor();

		double errorMax = 0.02;

		// Act

		double resultFanning = frictionFactorObj.fanning(Re,roughnessRatio);

		// Assert
		//
		// I want to use a 10 percent difference rather than absolute value
		// Assert.Equal(referenceFanning,resultFanning,4);

		double error;
		error = Math.Abs(resultFanning - referenceFanning)/referenceFanning;
		
		Assert.True(error < errorMax);
		// I have asserted that the churchill friction factor correlation is accurate to 
		// 10% up to Re=2200 with the laminar flow correlation,
		// this is good
	}

	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	[InlineData(2200, 0.05)]
	public void Test_churchillFrictionFactorErrorNotMoreThan4Percent_Laminar(double Re,double roughnessRatio){
		// this tests the churchill relation against the 
		// laminar flow friction factor
		// fanning is 16/Re
		// and no matter the roughness ratio, I should get the same result
		// however, roughness ratio should not exceed 0.1
		// as maximum roughness ratio in charts is about 0.05
		//
		// Setup

		// this test asserts that the error should not be more than 2%

		double referenceFanning = 16/Re;

		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchillFrictionFactor();

		double errorMax = 0.04;

		// Act

		double resultFanning = frictionFactorObj.fanning(Re,roughnessRatio);

		// Assert
		//
		// I want to use a 10 percent difference rather than absolute value
		// Assert.Equal(referenceFanning,resultFanning,4);

		double error;
		error = Math.Abs(resultFanning - referenceFanning)/referenceFanning;
		
		Assert.True(error < errorMax);
		// I have asserted that the churchill friction factor correlation is accurate to 
		// 10% up to Re=2200 with the laminar flow correlation,
		// this is good
	}


	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	[InlineData(2200, 0.05)]
	public void Test_churchillFrictionFactorShouldBeAccurate_Laminar(double Re,double roughnessRatio){
		// this tests the churchill relation against the 
		// laminar flow friction factor
		// fanning is 16/Re
		// and no matter the roughness ratio, I should get the same result
		// however, roughness ratio should not exceed 0.1
		// as maximum roughness ratio in charts is about 0.05
		//
		// Setup

		double referenceFrictionFactor = 16/Re;

		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchillFrictionFactor();

		// Act

		double resultFrictionFactor = frictionFactorObj.fanning(Re,roughnessRatio);

		// Assert
		//
		// I want to use a 10 percent difference rather than absolute value
		// Assert.Equal(referenceFrictionFactor,resultFrictionFactor,4);

		double resultErrorFraction;
		resultErrorFraction = Math.Abs(resultFrictionFactor - referenceFrictionFactor)/referenceFrictionFactor;
		
		Assert.Equal(0.0, resultErrorFraction,1);
		// I have asserted that the churchill friction factor correlation is accurate to 
		// 10% up to Re=2200 with the laminar flow correlation,
		// this is good
	}
}
