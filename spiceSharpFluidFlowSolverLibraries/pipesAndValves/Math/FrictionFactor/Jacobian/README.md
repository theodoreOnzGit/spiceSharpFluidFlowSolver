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