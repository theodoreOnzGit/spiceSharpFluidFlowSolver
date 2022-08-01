# FluidSubCircuits

These FluidComponents were meant to copy the SubCircuit definitions, but now
add methods to extend its capability, ie, to return pressureDrop for a given
mass flowrate. 

## implementation

### Getting a template

To create a FluidSubCircuit, i just copied the SubCircuit code and renamed the class
in order to give myself freedom to meddle with the class.

MockFluidSubCircuit is one such class where i change nothing except the 
class name, constructors and cloning methods from SubCircuit to 
MockFluidSubCircuit.

### Subcircuit definitions

FluidSubCircuits, both series and parallel, are meant to return pressureDrops
for a given mass flowrate. And they inherit from FluidEntity rather than Entity.

The entities for subcircuits are stored in the subcircuit definition. 
The definitions are stored in the SubCircuit Parameters class. 

Subcircuit definitions in turn take in IEntityCollections in their constructor. 


## ISubCircuitDefinition and SubCircuitDefinition

Unfortunately, ISubcircuitDefinition and SubCircuitDefinition do not use
FluidEntityCollections. They use EntityCollections by default.

Hence i will need to use FluidEntityCollections in IFluidSubcircuitDefinition.

However, when i try to change the Entities into FluidEntities, i find that
the code is so tightly integrated that i cannot change it freely without
wrecking other parts of the code.

Eg. when i change the Entities into IFluidEntityCollection, it will complain
that the clone method for Entities isn't implemented. 

It's almost no use to do this =/

What i can do for the FluidSubcircuit is to check if each entity is castable as
a IFluidEntity just like before, and then sum up their pressureDrops. 

Or for parallel case, currents. 

# FluidParallelSubCircuit

To handle fluid flow in parallel pipes, i use the FluidParallelSubCircuit class.

For such circuits, we can easily obtain mass flowrates given a pressure drop since
the solver will just apply the same pressure drop to each branch and obtain
the mass flowrate.

Getting it the other way round sometimes requires iteration however. And that slows
things down.

## interpolation methods

To help with this, we might try to map out a relationship of getting a 
nondimensionalised pressure drop $Be_D$ with respect to Re. This would be in 
a similar manner we want to get explicit relationship of getting a Re from
Be in a pipe scenario. 

We first map out Be vs Re by sampling points of Re and getting Be. And then
we use those points to construct an interpolation object, a graph if you will
in the programming sense. 

This would help us get a graph of:

$$2 Be_D = (f_{darcy} \frac{L}{D} + K) Re^2
$$

We assume that this nondimensionalised relationship remains the same for all 
values of Be and Re.

Similarly for pipe systems in parallel, a nondimensionalised relationship
could be thought up

$$Be_D = f(Re)$$

or 

$$Re = f^{-1} (Be_D) $$
$$Re = \frac{ flowrate}{ A_{XS}} \frac{D_H}{\mu}$$
$$Be_D = \frac{\Delta p D_H^2}{\nu^2} = \frac{\Delta P D_H^2}{\mu \nu}$$

the question is, how can we nondimensionalise it such that the relationship
remains constant regardless of Re and $Be_D$?

We have multiple lengthscales and velocities to choose from. How can we pick
one that represents our case?

We normally have massflowrates and pipe cross sectional areas to help us
compute reynold's numbers.

It is quite intuitive that mass flowrates should be added up, and likewise
cross sectional areas can be added up as well.

The question is that for hydraulic diameter $D_H$. 

Thing is i already developed an method before to sample Be from Re in pipes.

I noticed that at high reynolds number, the pressure drop goes as the square
of mass flowrate. Or in terms of nondimensionalised variables, Be goes as 
$Re^2$ for high Re. So at high Re, a polynomial interpolation method should
suffice even for two points.

Less points should be used for high Re, and more points should be used for
interpolating the transition region.

That led me to think of using cubic spline interpolation for the entire Re range
and logarithmically spaced sampling points. So that at high Re, less points
are used, so as to save on RAM and calculation time, and at low Re and transition
Re, more sampling points are used.

If i were to constrain myself to this same method, then i would need to find
a way that the transition from laminar regime, where Be and Re relate linearly
or non quadratically, would happen at 2300 < Re < 4000. This way, i can ensure
that the transition region has sufficient points to help interpolate Be from
Re in that region, or vice versa.


For this we shall have a thought experiment:

Suppose we have two identical pipes in parallel,

using the above definitions, and if we were to take the overall hydraulic
diameter as the sum of the components, we would get the following:

$$Re = \frac{2 flowrate}{2 A_{XS}} \frac{2 D_H}{\mu}$$
$$Re = \frac{ flowrate}{ A_{XS}} \frac{2 D_H}{\mu}$$

Now for this case, the reynold's numbers for the parallel circuit would
be twice as high as the Reynold's number for the single pipe. Thus the
transition region would be from 4600 to 8000. This means either a new 
interpolation scheme should be used, or to keep the transition region
in a similar Re range, then we can define the hydraulic diameter as
the average hydraulic diameter of both pipes.

This is not good,

So we can instead take the hydraulic diameter to be the average of the hydraulic
diameter of both pipes. This way, we have:

$$Re = \frac{ flowrate}{ A_{XS}} \frac{ D_H}{\mu}$$

Thus, the transition region would more or less happen in a similar region.

Now suppose we have two pipes, one about 10x the diameter of the other:


The average hydraulic diameter is then 0.55D. Where D is the hydraulic
diameter of the larger pipe.

The cross sectional area is 1.01 $A_{XS}$ where $A_{XS}$ is the area of the
larger pipe. We can now see most of the flow goes through the pipe of higher
hydraulic diameter. This is by virtue of the larger pipe having so little flow
resistance. And i assume the pipe are of equal length.

Given that the smaller pipe adds a negligible amount of flowrate to the setup,
we can then see

$$Re \approx \frac{ flowrate}{ A_{XS}} \frac{0.55 D_H}{\mu}$$

Now in this case we see that the transition region might take place in 
Re about 1200-1300 to about 2200. This is just estimation.

Nevertheless, using logarithmically spaced points, the sampling interval in this
region is also twice as small as the region where Re is 2300 to 4000.

We can repeat this as many times as we'd like even up to Re = 120 due to the nature
of how we sample our logarithmically spaced points.

Now suppose we have another two pipes, one is about 1.5 times the diameter
of the other:

Again, the mass flowrates and cross sectional areas should scale more or less
equally provided the density and velocities are constant. They may not be..

But assume that isn't a problem yet (we can test this assumption later).

The average hydraulic diameter should be around 0.67 times of the larger pipe.

$$Re \approx \frac{ flowrate}{ A_{XS}} \frac{0.67 D_H}{\mu}$$

Again, the transition region here would happen below Re = 2300 due to the way
the Re scales.

This wouldn't be a problem either.

It is likely however, that more flowrate goes through the larger pipe, and we
can have potential situations where one pipe is in a laminar regime
and the other in a parallel regime. 

Here is where we consider the next case.


Suppose we have a pressure drop such that one pipe is laminar and the other
is turbulent. And unlike the above case, the flowrate through neither is 
negligible. Will this interpolation scheme then work?


For one pipe:

$$Be = k_1 Re$$

For the other:


$$Be = k_2 Re^2$$
$$Re = \sqrt{\frac{Be}{k_2}}$$

Now if we had a common pipe Re, we would be able to get Re from Be like so:

$$Re = \frac{Be}{k_1} + \sqrt{\frac{Be}{k_2}}$$

For this case, a polynomial spline interpolation method still seems feasible.


Now suppose we have a fLDK component with no clear transition region:

We have the following correlation

$$Be_D = k_1 Re + k_2 Re^2$$

For this, a cubic spline interpolation would also work regardless of the points
supplied, since this function is clearly quadratic.


It looks like the bases are reasonably checked and covered. I probably haven't 
turned over every stone, perhaps there are multiple pipes, or one pipe is in
a transition regime and the other is not, or one pipe is in a not fully turbulent
(smooth pipe) regime and the other is not.

Regardless, i think for a first iteration, it should do okay. There can be future
work done to adapt this for other systems. But for CIET, where flow is mostly
laminar, and we have pipes, this should be fine.



