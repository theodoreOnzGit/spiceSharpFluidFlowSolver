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

