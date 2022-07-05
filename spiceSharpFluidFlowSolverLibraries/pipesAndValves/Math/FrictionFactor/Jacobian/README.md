# Jacobian Readme


## Purpose

The jacobian set of classes is meant to calculate the jacobian
terms:

$$\frac{\partial \dot{m}_{pipe} (kg/s)}{\partial p_A (m^2/s^2)}$$
$$\frac{\partial \dot{m}_{pipe}}{\partial p_B}$$

And likewise to node b,
$$-\frac{\partial \dot{m}_{pipe}}{\partial p_A}$$
$$-\frac{\partial \dot{m}_{pipe}}{\partial p_B}$$

## Methodology
From chain rule we can obtain:
$$\frac{d \Delta p}{d Re} =  \frac{d \Delta p}{d p_A}* 
\frac{d p_A}{d \dot{m}_{pipe}} * \frac{d \dot{m}_{pipe}}{d Re} $$

$$\frac{d \dot{m}_{pipe}}{d p_A} = 
\frac{\frac{d \Delta p}{d p_A} 
\frac{d \dot{m}_{pipe}}{d Re}}{\frac{d \Delta p}{d Re}}$$

The top two derivatives are easy to obtain analytically.

What remains is the bottom derivative.

I wish to obtain the derivative using nondimensional numbers
as far as possible. And when dimensional numbers are given,
I want to use the EngineeringUnits package to dimensionalise
quantites properly.

$$\frac{d(Be)}{d(Re)}  = \frac{d(Be)}{d \Delta p} 
* \frac{d \Delta p}{d (Re)}$$

To obtain the $\frac{d(Be)}{d \Delta p}$:

$$Be = \frac{\Delta p L^2}{\nu^2}$$
$$ \frac{d(Be)}{d \Delta p} = \frac{L^2}{\nu^2}$$


With this we can now obtain the derivative 
$\frac{d \Delta p}{d (Re)}$:

$$\frac{d(Be)}{d(Re)}  = \frac{d(Be)}{d \Delta p} 
* \frac{d \Delta p}{d (Re)}$$


$$\frac{d(Be)}{d(Re)}  = \frac{L^2}{\nu^2}
* \frac{d \Delta p}{d (Re)}$$

$$\frac{d \Delta p}{d (Re)} = \frac{d(Be)}{d(Re)} 
* \frac{\nu^2}{L^2}$$

### Numerical Derivative

To obtain the numerical derivative $\frac{d(Be)}{d(Re)}$:
$$f_{fanning} (Re)* Re^2 = \frac{32 Be}{ (\frac{4L}{D})^3 }$$
$$\frac{d(Be)}{d(Re)} = \frac{(\frac{4L}{D})^3}{32}
\frac{d}{d(Re)} ( f_{fanning}(Re)*Re^2 ) $$

Now we have our equation being fully nondimensional.
From this we can find the differential.
This of course assumes that  roughness ratio is constant.

The derivative can be achieved by direct numerical estimation of
the derivative. This should be the most straightforward way.
I have little in the way of testing the validity however.

Nevertheless, I can simply copy the numerical code that performs 
the calculations for fanning derivatives and add in  $Re^2$ to the 
function.

```csharp
```

The only test to do here is an internal consistency check.
But given that it's so simple, i'd rather forgo it.

Unless errors come forth later on.

### dpdRe dimensioned derivative
Now we just need to calculate:
$$\frac{d \Delta p}{d (Re)} = \frac{d(Be)}{d(Re)} 
* \frac{\nu^2}{L^2}$$

```csharp
```


It is also very important to our computation algorithms
that one pressure drop value corresponds to one Re value.

So we don't have the problem of one friction factor value
corresponding to two Re values.
$$\frac{d(Be)}{d(Re)}  = \frac{d(Be)}{d \Delta p} * \frac{d \Delta p}{d (Re)}
$$

I can't really test too much on this, except to copy/paste
the inner working code and see if it works as intended.

Also unit checks.

##  dmdRe dimensioned derivative
$$\frac{d \dot{m}_{pipe}}{d p_A} = 
\frac{\frac{d \Delta p}{d p_A} 
\frac{d \dot{m}_{pipe}}{d Re}}{\frac{d \Delta p}{d Re}}$$


Now we have settled the denominator, we can look at the numerator

dmdRe is in the numerator, and it has units of mass flowrate.


$$Re_D = \frac{\dot{m}_{pipe}D_{pipe}}{A_{xs}\mu_{fluid}}$$

$$\dot{m}_{pipe} = \frac{Re_{D} A_{xs} \mu_{fluid}}{D_{pipe}}$$

so we have the derivative 

$$\frac{d\dot{m}_{pipe}}{d(Re)} = \frac{A_{xs} 
\mu_{fluid}}{D_{pipe}}$$

Only three parameters are required: 


1. pipe cross sectional area
2. fluid viscosity
3. pipe hydraulic diameter

the return type will be dimensioned in mass flowrate


```csharp
```

##  $\partial \Delta p / \partial p_A$ and  $\partial \Delta p / \partial p_B$ derivatives

We note:

$$\Delta p = p_A - p_B$$

We go by the potential difference convention for circuits rather
than pressure drop which has inital - final.

From this we note:

$$\frac{\partial \Delta p}{\partial p_A} = 1$$
$$\frac{\partial \Delta p}{\partial p_B} = -1$$

For the sake of having such functions explicitly declared
in code, i'm making functions for these:

```csharp
```

May be trivial, but at least it makes code somewhat easier to read.
If one understands these derivations that is.

## dm/dp jacobian

Now this derivative here will be in some 
weird unit: $(kg/s)/(m^2/s^2)$

I know not of any existing unit being of this sort of unit.
But no matter, the final return type is double, since
this is the type expected to go into load.

I expect units to all be SI converted. So I'd like to convert them
in my functions before anything else.

So far, the jacobian can be calculated if the Reynold's number is 
known.

If the Reynold's number is not known, we have to guess it from 
the pressure drop.

For that, we need to convert the kinematic pressure drop to Bejan 
number, then obtain the Reynold's number given the relative
roughness and lengthToDiameter Ratio.















