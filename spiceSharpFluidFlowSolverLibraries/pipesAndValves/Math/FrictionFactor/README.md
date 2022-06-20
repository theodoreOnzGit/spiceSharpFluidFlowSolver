# FrictionFactor Readme

## Design

This friction factor class is based on churchill friction factor
correlation.

However, since there are many ways of finding friction factor
I have written an IFrictionFactor interface so that we can
have many ways of implementing the friction factor.

Eg. Colebrook formula.

# IFrictionFactor

The IFrictionFactor interface just returns the fanning, moody
and darcy friction factor given a Re and roughness Ratio 
$\frac{\varepsilon}{D}$.

Note that moody and darcy friction factor are basically the same
thing. 

But I put it there for ease of use.

```csharp
public interface IFrictionFactor
{
	double fanning(double ReynoldsNumber, double roughnessRatio);
	double moody(double ReynoldsNumber, double roughnessRatio);
	double darcy(double ReynoldsNumber, double roughnessRatio);
}
```
# ChurchHillFrictionFactor.cs
Churchill friction factor is defined by:


$$f_{fanning} = 2 \left[\\
\left( \frac{8}{Re} \right)^{12} + \\
\left( \frac{1}{A+B}\right)^{3/2} \\
\right]^{1/12} $$

$$f_{Darcy} = 8 \left[\\
\left( \frac{8}{Re} \right)^{12} + \\
\left( \frac{1}{A+B}\right)^{3/2} \\
\right]^{1/12} $$

$$A = \left[ 2.457 \ln \frac{1}{\left( (7/Re)^{0.9} + \\
0.27 \frac{\varepsilon}{D} \right)} \\
\right]^{16}\ \ ; \ \ \\
B = \left( \frac{37530}{Re} \\ 
\right)^{16} $$


## Defining A and B

$$A = \left[ 2.457 \ln \frac{1}{\left( (7/Re)^{0.9} + \\
0.27 \frac{\varepsilon}{D} \right)} \\
\right]^{16}\ \ ; \ \ \\$$

A is defined in code as the following:

```csharp
private double A(double Re, double roughnessRatio){
	// first i need the logarithm of a number

	double reynoldsTerm =  Math.Pow( (7.0/Re), 0.9);
	double roughnessTerm = 0.27*roughnessRatio;

	double logFraction = 1.0/(reynoldsTerm+roughnessTerm);
	double innerBracketTerm = 2.457*Math.Log(logFraction);
	double A = Math.Pow(innerBracketTerm,16);
	
	return A;
}

```


$$B = \left( \frac{37530}{Re} \\ 
\right)^{16} $$

```csharp

private double B(double Re){
	double numerator = Math.Pow(37530,16);
	double denominator = Math.Pow(Re,16);
	return numerator/denominator;
}


```
##  intermediate calculation

$$innerTerm =  \left[\\
\left( \frac{8}{Re} \right)^{12} + \\
\left( \frac{1}{A+B}\right)^{3/2} \\
\right] $$

```csharp

private double churchillInnerTerm(double Re, double roughnessRatio){

	double laminarTerm;
	laminarTerm = Math.Pow(8.0/Re, 12);

	double turbulentTerm;
	double Aterm = this.A(Re,roughnessRatio);
	double Bterm = this.B(Re);

	turbulentTerm = Math.Pow( 1.0/(Aterm + Bterm), 3.0/2);

	return laminarTerm + turbulentTerm;


}


```
## fanning friction factor

So to calculate fanning friction factor,

```csharp
public double fanning(double ReynoldsNumber, double roughnessRatio){

	double fanningFrictionFactor;
	fanningFrictionFactor = 2 * Math.Pow(this.churchillInnerTerm(ReynoldsNumber,roughnessRatio), 1.0/12);
	return fanningFrictionFactor;
}
```

$$f_{fanning} = 2 \left[\\
\left( \frac{8}{Re} \right)^{12} + \\
\left( \frac{1}{A+B}\right)^{3/2} \\
\right]^{1/12} $$

## Darcy and Moody Friction factor methods
Darcy friction  factor just multiples fanning friction factor
by 4..

```csharp
public double darcy(double ReynoldsNumber, double roughnessRatio){

	// darcy friction factor is 4x fanning friction factor
	// https://neutrium.net/fluid-flow/pressure-loss-in-pipe/
	double darcyFrictionFactor;
	darcyFrictionFactor = 4 * this.fanning(ReynoldsNumber,roughnessRatio);
	return darcyFrictionFactor;
}
```

Moody Friction factor just calls the Darcy friction factor method.

```csharp

public double moody(double ReynoldsNumber, double roughnessRatio){

	// apparently the moody friciton factor is same as the darcy friction factor

	return this.darcy(ReynoldsNumber,roughnessRatio);
}
```
## Usage 

Just instantiate the object and use the fanning friction factor term
straightaway.


# finding Re from pressure drop (nondimensional pressure drop Be)


For pressure drop, we have the explicit correlation
$$f_{fanning}(Re,\frac{\varepsilon}{D})* Re^2 = \frac{32 Be}{ (\frac{4L}{D})^3 
}$$

We want to use the Mathnet Numerics library

so in our csproj file we have:

```xml
<PackageReference Include="MathNet.Numerics" Version="5.0.0" />
```

I'm using the findRoots.cs [file](https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/FindRoots.cs):

```csharp

namespace MathNet.Numerics
{
    public static class FindRoots
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Example: 1e-14.</param>
        /// <param name="maxIterations">Maximum number of iterations. Example: 100.</param>
        public static double OfFunction(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            if (!ZeroCrossingBracketing.ExpandReduce(f, ref lowerBound, ref upperBound, 1.6, maxIterations, maxIterations*10))
            {
                throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
            }

            if (Brent.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out var root))
            {
                return root;
            }

            if (Bisection.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
        }

```

So I'll be using the namespace MathNet.Numerics and use the static class
FindRoots.

The way to call it inline is:

```csharp

FindRoots.OfFunction(func<double,double> f, lowerBound, upperBound);
```


Unfortunately, we don't quite have one input and one output. So we'll
have to change types.

So we'll have to resort to some object oriented trickery in order to do this.

Meaning to say that all else constant, the function is such that
Reynold's number is the input and the LHS-RHS is the output.

Which doesn't quite give us pure functions so to speak, since we 
are using properties. But I'll try as far as i can.

Setting the equation to zero is


$$f_{fanning}(Re,\frac{\varepsilon}{D})* Re^2 - \frac{32 Be}{ (\frac{4L}{D})^3 
} = 0$$

So I'll need to create a function that takes Reynold's number as an input,
takes the Bejan number, roughness ratio and lengthToDiameter ratio
as constants. Then return the LHS-RHS as the output.


Here's the result:

```csharp

double pressureDropRoot(double Re){

	// fanning term
	//
	double fanningTerm;
	fanningTerm = this.fanning(Re, this.roughnessRatio);
	fanningTerm *= Math.Pow(Re,2.0);


	//  BejanTerm
	//
	double bejanTerm;
	bejanTerm = 32.0 * this.bejanNumber;
	bejanTerm *= Math.Pow(4.0*this.lengthToDiameter,-3);

	// to set this to zero, we need:
	//
	return fanningTerm - bejanTerm;

}
```

The LHS is the fanningTerm. Which is

$$f_{fanning}(Re,\frac{\varepsilon}{D})* Re^2 $$

To calculate this i use the fanning friction factor function
within the churchill correlation class. (I'm adding methods to 
the same class)

Then I'm multiplying $Re^2$ to the fanning friction factor.

The RHS is the bejan term


$$ \frac{32 Be}{ (\frac{4L}{D})^3} $$

So I set the bejan term to 32.0*Be, the user's given Bejan number.
Then i multiplied that by four times the L/D ratio to the power
of -3 to obtain the 4L/D term in the denominator.

After that i use the FindRoots.OfFunction method
I set the minimum Re to be 1 (otherwise we'll get infinity in the
laminar region)

And then 1e8 as the maximum. That's the upper limit of the moody
chart.

```csharp
double ReynoldsNumber;
ReynoldsNumber = FindRoots.OfFunction(pressureDropRoot, 1, 1e8);
```

Unfortunately this means i have to use class parameters to share
variables within the function as constants.

```csharp

private double roughnessRatio;
private double lengthToDiameter;
private double bejanNumber;

```

I first use the function's variables to instantiate the values:

```csharp

this.roughnessRatio = roughnessRatio;
this.lengthToDiameter = lengthToDiameter;
this.bejanNumber = Be;
```

Then I perform calculations.
After I'm done, i set them all to zero.

```csharp

// once I'm done, i want to clean up all terms
this.roughnessRatio = 0.0;
this.lengthToDiameter = 0.0;
this.bejanNumber = 0.0;


// then let's return Re

return ReynoldsNumber;
```

This is to ensure that the object state doesn't really affect
the calculation, so it functions like a pure method or function.

And I also make sure to clean up every number after i'm done,
so that the memory is cleared.


The only thing that remains is to test it!



# IFrictionFactorDerivatives

Now we will need to calculate jacobians a lot!

So we will need to calculate a lot of derivatives.

For the KISS principle's sake, I am going to stuff things in the same
class. However, to organise the Churchill Friction factor to be
distinct from its derivatives, I will use partial classes.

Meaning to say, the partial derivative functions will be part
of a separate file, but in the same class.

I also have a separate interface which fulfils the SOLID principle
interface segregation since there are at least two ways of
performing derivatives.

One is to use numerical derivatives, ie, my own central difference
code. (Or the MathNet implementation)

The other is to use analytical derivatives as shown in the readme.

## ChurchHill Friction factor Partial Derivative class

```csharp

public double calculateFanningPartialDerivative(double Re, 
		double roughnessRatio){
	//
	// firstly i need to use the derivative object

	IDerivative derivativeObj;
	derivativeObj = new CentralDifference();

	// secondly i need a function with a double
	// in and out
	// this is the function that returns the Reynold's number
	// however, the roughness ratio is kept constant

	this.roughnessRatio = roughnessRatio;

	double constantRoughnessFanning(double Re){

		double fanning = this.fanning(Re, this.roughnessRatio);

		return fanning;
	}

	// now let's calculate the derivative at a specific Re

	double derivativeResult;
	derivativeResult = derivativeObj.calc(
			constantRoughnessFanning, Re);

	// after i'm done, clean up the roughness ratio
	// variable within the class

	this.roughnessRatio = 0.0;


	return derivativeResult;

}

```

The idea here is to take partial derivatives of the fanning 
friction factor if constant roughness ratio is assumed.

I again use the class variable roughnessRatio to help in this
area. In that the local function constantRoughnessFanning uses
this class variable roughnessRatio assumed to be constant during
the time of this calculation.


Note that this only works if the object instance is being 
synchronously. It is not good for async since roughnessRatio
can be changed.

However, since we don't really care about speed at this point
in time, I'm not going to bother yet.

### testing
The partial derivatives should be benchmarked against something.

Unfortunately, there are no straightforward ways of getting
reference values other than the analytical solution.

And yet, the analytical solution itself is an untested class
at this point.

We can only assume that there is not a common systematic error
between them, such that if the test passes, it's not because
both analytical and numerical solution are equally wrong.

But we have some confidence in the numerical result, since 
the fanning friction factor and the derivative term have been
tested extensively in unit testing.


#### Testing: Laminar Region

In the laminar region, the numerical derivative was benchmarked
against the derivative of 16/Re which became $-16/Re^2$. With that
in mind, the only challenge was in the turbulent region

#### Testing: Turbulent Region

For testing in the turbulent region, we want an expression easily
differentiable but reasonably accurate compared to colebrook.

The correlation by Tsal is indeed easy to differentiate.

It is shown as [[1]](#frictionFactorApproximations):

$$C = 0.11 (\frac{68}{Re} + \frac{\varepsilon}{D})^{0.25}$$
$$if\ C \geq 0.018, f_{darcy} = C$$
$$if\ C < 0.018, f_{darcy} = 0.0028 + 0.85*C$$

While easy to differentiate, it has relatively high errors. 
I tested it in the test suite and it seems to only work well
in the smooth regions and regions of relatively low
surface roughness.

I again searched the [same paper](https://iwaponline.com/ws/article/20/4/1321/73330/Approximations-of-the-Darcy-Weisbach-friction)
trying to find another relatively accurate correlation that is
farily easy to differentiate:


I found the Filonenko correlation which was, according to the paper
found to have a relative error at most 0.5% and below. this is 
comparable to churchill correlation, but is much easier to 
differentiate. However, laminar region is not predicted by this
equation.

The Filonenko equation when adapted for rough pipes is as follows [[1]](#frictionFactorApproximations):

$$\frac{1}{\sqrt{f_{Darcy}}}=-2 \log_{10} (\frac{6.6069}{Re^{0.91}}
+ \frac{\varepsilon/D}{3.71})$$

To obtain the derviative $\frac{d f_{Darcy}}{dRe}$, we can use:

$$\frac{d}{dRe}\frac{1}{\sqrt{f_{Darcy}}}= 
- \frac{1}{2}\frac{1}{f_{Darcy}^{3/2}} 
\frac{d f_{Darcy}}{dRe} $$

$$\frac{d f_{Darcy}}{dRe} = -2 f_{Darcy}^{1.5}
\frac{d}{dRe}\frac{1}{\sqrt{f_{Darcy}}}$$


To obtain the derivative, 
we first transform the base of the logarithm:
$$\frac{1}{\sqrt{f_{Darcy}}}=-2 \frac{1}{\ln_{10}}\ln 
(\frac{6.6069}{Re^{0.91}}
+ \frac{\varepsilon/D}{3.71})$$

The derivative can be obtained analytically as:
$$\frac{d}{dRe}\frac{1}{\sqrt{f_{Darcy}}}= -2 \frac{1}{\ln 10}
\frac{6.6069*(-0.91)*Re^{-1.91}}{\frac{6.6069}{Re^{0.91}}
+\frac{\varepsilon/D}{3.71}}$$


## Analytical Derivative

The analytical derivative of f with respect to Re was calculated
in the README.md of fluid pipe theory.

What has been done is that the analytical derivative for $f(Re)Re^2$
has already been calculated. For constant roughness ratio.

$$\frac{d}{d(Re)} [f(Re) Re^2] = Re^2 \frac{d}{d(Re)} [f(Re)]
+ f(Re) * 2Re$$


Since this is the case, i can back calculate the derivative.

$$Re^2 \frac{d}{d(Re} [f(Re)] =  \frac{d}{d(Re)} [f(Re) Re^2]
-f(Re) * 2Re$$

Divide both sides by $Re^2$ and we should get the differential value.
All without using numerical methods, but rather analytical.

I can implement this function and just return this value.

```csharp

public override double calculateFanningPartialDerivative(
		double Re, double roughnessRatio){

	double derivativeResult;
	derivativeResult = this.partialDerivativeFanningReSquared(Re,
			roughnessRatio);

	derivativeResult -= 2.0 * Re * this.fanning(Re,roughnessRatio);

	derivativeResult /= Math.Pow(Re,2.0);

	return derivativeResult;
}
```

### terms and functions

To calculate the fanningDerivativeReSquared I used:

```csharp

public double partialDerivativeFanningReSquared(
		double Re, double roughnessRatio){

	double finalValue;
	finalValue = 1.0/6.0;
	finalValue *= this.dG1_dRe(Re, roughnessRatio);
	finalValue /= this.G1(Re,roughnessRatio);
	finalValue *= Math.Pow(this.G1(Re,roughnessRatio),1.0/12.0);
	return finalValue;
}
```

The first function used is dG1_dRe, and G1.

G1 is:
```csharp

public double G1(double Re, double roughnessRatio){

	double g1value;
	g1value = Math.Pow(8.0*Re,12.0);
	double g1RHSvalue = Math.Pow(Re,16.0*3.0/2.0);
	g1RHSvalue *= Math.Pow(
			this.A(Re,roughnessRatio)
			+this.B(Re),
			-3.0/2.0);

	g1value += g1RHSvalue;

	return g1value;
}
```

whereas dG1_dRe is:
```csharp

public double dG1_dRe(double Re, double roughnessRatio){

	double finalValue;
	finalValue = -Math.Pow(Re,16.0) *
		(this.dA_dRe(Re, roughnessRatio) + this.dB_dRe(Re));
	
	finalValue += 16.0 * Math.Pow(Re,15.0) *
		(this.A(Re,roughnessRatio) + this.B(Re));

	finalValue /= Math.Pow(this.A(Re,roughnessRatio)+
			this.B(Re),2.0);

	finalValue *= 3.0/2.0 * Math.Pow(Re,16.0/2.0);

	finalValue /= Math.Pow(this.A(Re,roughnessRatio) + 
			this.B(roughnessRatio),1.0/2.0);

	finalValue += 96.0 * Math.Pow(Re,11.0);


	return finalValue;

}
```

The A and B will be inherited from the Churchill Friction Factor class.

```csharp

public class ChurchillAnalyticalDerivative : 
	ChurchHillFrictionFactor,IFrictionFactorDerivatives

```

Whereas the derivatives are as follows:


I will need dB_dRe.

As derived before, it is 

$$\frac{dB}{dRe} = -B \frac{16}{Re}$$

So we would want to have B on standby.

I will hence make A and B public so that I can inherit them.

And I will also make the calculate fanning Partial Derivative
classes overwriteable

```csharp
public double dB_dRe(double Re){
	return -this.B(Re) * 16.0 / Re;
}
```


Then I would need dA_dRe also:

```csharp
public double dA_dRe(double Re, double roughnessRatio){
	double dAdReValue;
	dAdReValue = 16*this.A(Re,roughnessRatio);
	dAdReValue *= this.dG2_dRe(Re);
	dAdReValue /= this.G2(Re,roughnessRatio);
	dAdReValue /= Math.Log(this.G2(Re,roughnessRatio));

	return dAdReValue;
}
```

G2 is:

```csharp
public double G2(double Re, double roughnessRatio){

	double g2value;
	g2value = Math.Pow(7.0/Re, 0.9);
	g2value += 0.27*roughnessRatio;

	return g2value;

}
```

and dG2_dRe is:

This is of course assuming roughness ratio does not change with Re.
Should make physical sense.


```csharp


public double dG2_dRe(double Re){

	return -5.1859789 * Math.Pow(Re,-1.9);
}
```

Next thing is to just test this thing and see if any debugging is needed.

### Analytical expression: depracated till further notice

It turns out that after testing, i cannot pinpoint the 
source of the bug easily.

The churchill friction factor correlation is twice that
of the laminar correlation. And I have no easy way to tell
where I made a mistake in the derivation.

My solution forward is to use the numerical method.
However, I will use friction factor representations that
are both accurate (<1%  error compared to $f_{D\ colebrook}$
) and easy to differentiate.

This will become my benchmark for the numerical method in
the turbulent region.

The numerical solution was tested against the benchmark
in the laminar region already and results were satisfactory.


# Bibliography

<a id="frictionFactorApproximations">
[1]
Zeyu, Z., Junrui, C., Zhanbin, L., Zengguang, X., & Peng, L. (2020). Approximations of the Darcy–Weisbach friction factor in a vertical pipe with full flow regime. Water Supply, 20(4), 1321-1333.
</a>
