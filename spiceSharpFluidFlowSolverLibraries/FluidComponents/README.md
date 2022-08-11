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

Getting it the other way round sometimes requires iteration however. 
And that slows things down. A typical iteration of a 
FluidParallelSubCircuit containing three isothermal pipes within
a FluidSeriesCircuit containing only this FluidParalleSubCircuit
takes about 1 minute to solve.

## nested iteration algorithm slows things down

Each isothermal pipe contains an explcit function to derive a pressure 
drop given a mass flowrate. To obtain mass flowrate from a given 
pressure loss term (excluding hydrostatic pressure changes), a mass
flowrate is guessed, and a pressure drop is obtained. The mass flowrate
is changed iteratively using MathNet's algorithm library until the 
desired pressure drop is reached. It is a hybrid algorithm that is 
based on newton raphson and other methods but falls back to bisection
so that if a solution exists within the bounds, convergence is 
guaranteed.

With hydrostatic pressure change however, we then need to add the
$\Delta h \rho g$ value to the dynamic pressure loss term obtained
to get the total pressure change from start to end of the pipe.

When connected in series, and one wants to find the pressure loss terms
one can guess a mass flowrate through the series of pipes, and then
iteratively find a pressure loss value that matches the desired pressure
drop across this pipe system using the same mathnet algorithm.

Now when it comes to a parallel pipe system having multiple flow paths
each branch would have at least one fluid component. And each component
would use an iterative algorithm to find mass flowrate from pressure 
drop. In this case using MathNet's C\# libraries. 

Now parallel subsystems or subcircuits would be part of a larger series
circuit. And the larger series circuit would normally iteratively guess
the mass flowrate values to obtain a specified pressure drop. 

The algorithm would be:

1. Take the specified pressure added by some pump
2. Add the hydrostatic contributions by summing $\Delta h \rho g$ 
around the loop
3. Use this total pressure change as the specified pressure, and 
obtain the mass flowrate using iterations of guessing mass flowrate
until specified pressure is achieved.

Now when parallel circuits are involved in this series circuit, 
the overall fluid series circuit solver would need to know the pressure
loss for a parallel subcircuit given a mass flowrate. Moreover, we need
to know the hydrostatic contribution $\Delta h \rho g$ of this 
parallel subcircuit so that we can use that to obtain a suitable overall
mass flowrate of the loop.

The problem is for a non isothermal case, densities will differ across
each branch, and so the $\Delta h \rho g$ will differ depending on 
which branch one is looking at. So this is one major issue to solve.


Assuming we do solve it, the next problem would then be to guess
a pressure drop given a specified external mass flowrate.

The parallel subcircuit algorithm would be:

1. Take the specified mass flowrate specified by the external series 
circuit
2. Guess presusre drop by guessing a pressure change term across each 
branch obtain mass flowrates of each branch, 
and keep iterating the pressure change the until the sum of mass flowrates equals the external specified mass flowrate

Now each parallel subcircuit would not only contain one pipe within 
each branch, but possibly a series of pipes and components. Each of
which guesses mass flowrate given a specified pressure drop iteratively
as well.

The series subcircuit within the parallel subcircuit algorithm will be:


1. Take the specified pressure change across the branch 
2. Add the hydrostatic contributions by summing $\Delta h \rho g$ 
around the loop
3. Use this total pressure change as the specified pressure, and 
obtain the mass flowrate using iterations of guessing mass flowrate
until specified pressure is achieved.

This would mean we would have nested iterative methods to obtain a
simple pressure loss term over a series of pipes with a parallel branch.
If we have multiple parallel subcircuits nested within the main parallel
subcircuits, the iteration time would increase exponentially.

## time requirements for a realtime simulation

Thus, even for a simple of three isothermal pipes within a parallel 
subcircuit, and that within a series circuit, it took 1 minute to
calculate pressure loss term given a mass flowrate value. However
for a simulation expected to run in real time, where we expect mass 
flowrate to be calculated in millesecond intervals, this is not 
acceptable. For such a simulation, both mass flowrate and heat transfer
calculations should occur within <50 ms, because should we want 
mass flowrates to be updated realistically, eg. every 90-100 ms, we 
would not only need time for calculating mass flowrates, we would 
also need to calculate heat transfer rates and temperature distributions
for each component. Not only that, we would then need to take time to
upload these results to some server, and broadcast it to a piece of 
control or Supervisory Contorl and Data Acquisition (SCADA) software
via OPCUA.

All of this needs to happen every 100 ms or 0.1 s at the very most.

Thus, we will need some methods to speed up these iterative calculations
significantly so that this time requirement can be met.

## interpolation methods

To help with this, we might try to map out a relationship of getting a 
nondimensionalised pressure drop $Be_D$ with respect to Re. This would be in 
a similar manner we want to get explicit relationship of getting a Re from
Be in a pipe scenario. 

For a singular pipe component, we can take out the iterative part
of obtaining mass flowrate from a given pressure drop (taking into 
account buoyancy).

This is because given a pressure loss term, whether or not buoyancy
is a part of it, the following relationship for the fluid component
would remain the same regardless of temperature.

$$ Be_D = 0.5(f_{darcy} \frac{L}{D} + K) Re^2
$$

The only temperature dependent terms are Re and $Be_D$. So we can 
essentially build a database of $Be_D$ vs Re prior to setting the
simulation up, and then when the pressure loss term is
requested given a mass flowrate, we can obtain a reynold's number, 
and interpolate a Bejan number given this mass flowrate. And 
so we can obtain a pressure loss or pressure drop term given Re.

This is done programatically by:


1. We first map out Be vs Re by sampling points of Re and getting Be. 
2. we use those points to construct an interpolation object, using the
MatNet interpolation library
3. When pressure loss is requested Be will be interpolated from Re

This is essentially using the program to draw a graph of Be vs Re 
before the simulation starts, and reading Be from Re programmatically
during simulation. In that regard, all the iterations are done before
the simulation starts, not during the simulation.

This was done for a single pipes. We could do the same for series 
circuits and series subcircuits. The question is can we apply this 
same method for a paralle subcircuit as well?

This would mean assuming that the paralle subcircuit can be represented
by a singular fluid component and obtaining for this component:

$$ Be_D = 0.5(f_{darcy} \frac{L}{D} + K) Re^2
$$

We then assume that this nondimensionalised relationship 
remains the same for all values of Be and Re.

### challenges for using the interpolation methods with parallel subscircuits

To do this properly, we will need to overcome these challenges:

#### Obtaining a representative hydrostatic pressure change term 

As discussed previously, we will need to obtain a representative 
hydrostatic pressure change term across the branch of parallel 
branches.

The problem is how do we find a suitable average across each of these
branches if the buoyancy forces are unequal?


Buoyancy is quantified in hydrostatic pressure

$$\Delta h \rho g $$

the change in height for this system is obvious, the height difference
between entrance an exit regions is the same for all parallel branches.
Gravitional Acceleration is also constant.

The question is how to detemrine an average density?


#### Temperature distributions will affect pressure loss terms
The next challenge for doing this for 
parallel circuits is because buoyancy
affects each branch differently for a differing temperature 
distribution. Thus for a given mass flowrate, the apparent 
pressure loss terms across each branch may differ 
for a differing temperature distribution.

In this regard, fluid flowing up an isothermal pipe may feel more 
resistance compared to fluid flowing up a similar pipe with buoyancy
forces aiding the flow. This is known as aiding mixed convection.

In opposing mixed convection flow, where buoyancy forces oppose the 
direction of forced flow, then an opposite phenomena occurs. Thus the
net pressure loss terms over each pipe will differ. 

Similarly for pipe systems in parallel, a 
nondimensionalised relationship could be thought up.



#### Choosing a consistent way to nondimensionalise mass flowrate and pressure loss terms

Not only that, even if we could sort out the problem of differing 
temperature distributions, we still have to nondimensionalise our
terms properly. 

We have Re and Be defined as follows:

$$Re = \frac{\dot{m}}{ A_{XS}} \frac{D_H}{\mu}$$
$$Be_D = \frac{\Delta p D_H^2}{\nu^2} = \frac{\Delta P D_H^2}{\mu \nu}$$

While some of these parameters are obvious, eg.

1. mass flowrate of the parallel pipe system is sum of mass flowrates 
through each branch
2. presusre loss term is the pressure change over each branch (which 
must be the same), minus the hydrostatic pressure change of the
entire pipe system, should we solve for a representative hydrostatic
pressure change 
3. Cross sectional area is the total cross sectional area of the 
entrance region of the pipe. Or if reverse flow is of concern, we can
take the average of the sum of cross sectional areas in both entrance
and exit regions


However, we need to find the following:

1. We have multiple lengthscales $D_H$ and L to choose from. We can 
use $A_{XS}$, the cross sectional area to find $D_H$. But finding a 
suitable L may also be of importance as we may find out later.
How can we pick one that represents our case? 


2. Dynamic Viscosity $\mu$ needs to be averaged out as well

3. Density needs to be averaged out too so we can find kinematic 
viscosity $\nu$

Thankfully for us, in mainly liquid phase flow, density doesn't change
so much. So the boussinesq approximation can apply. The representative
density of the system should be the same as the representative density
used to calculate hydrostatic pressure contributions of this parallel
pipe system. 

The main issue then is how dynamic viscosity $\mu$ is calculated.

## Problem 1: Determining Hydrostatic Pressure Contributions

Now suppose we had a normal pipe which was slightly elevated,
if we were to have fluid flowing through the pipe at a fixed mass
flowrate, we would have some pressure drop due to frictional losses
and some pressure increase due to elevation.

The total change in pressure will be the sum of the pressure loss 
term and the pressure increase

$$\Delta P_{change} =  - 0.5 \frac{\dot{m}^2}{A_{XS}^2 \rho} 
(f_{darcy}\frac{L}{D_H}  + K) + \rho g \Delta h
$$

The friction term i will just refer to as fLDK from now on.
The fLDK term is the pressure loss term which is nondimensionalised 
using $Be_D$

$$Be_D = 0.5 Re^2 (f_{darcy}\frac{L}{D_H}  + K)$$

In counting hydrostatic pressure for a column of fluid,
 we would think about the weight 
exerted by a column of fluid and divide that by the area of contact.

If we want to establish the hydrostatic pressure of the fluid upon
the fluid below the pipe system, we can think about the weight of
the fluid and divide it accordingly by the cross sectional area.

How can we do so?

Let us first write down a force balance. We can say that the 
hydrostatic force exerted by the fluid within a parallel subcircuit
on the fluid below is the sum of all the hydrostatic forces exerted
by each of the branches.

$$F_{hydrostaticParallel} = \sum_i^n F_{hydrostatic i}$$

Hydrostatic forces can be thought of as the product of 
hydrostatic pressure multiplied by the area:

$$P_{hydrostaticParallel}A_{hydrostaticParallel} 
= \sum_i^n P_{hydrostatic i}A_{hydrostatici}$$


$$\rho_{Parallel} g \Delta H A_{hydrostaticParallel} 
= \sum_i^n \rho_i g \Delta H A_{hydrostatici}$$

given that g and $\Delta H$ are the same

$$\rho_{Parallel}  A_{hydrostaticParallel} 
= \sum_i^n \rho_i  A_{hydrostatici}$$

We can then see that the weighted average of fluid density
should be by the cross sectional area

$$\rho_{Parallel}   
= \frac{\sum_i^n \rho_i  A_{hydrostatici}}{A_{hydrostaticParallel}}$$

$$\rho_{Parallel}   
= \frac{\sum_i^n \rho_i  A_{XSi}}{\sum_i^n A_{XSi}}$$

This of course assumes that the area of contact exerted by each pipe is
the cross sectional area.

However, when pipes merge, it is not usually done at a flat angle. 
Usually pipes are bent:

![pipe photo](https://images.pexels.com/photos/3626605/pexels-photo-3626605.jpeg?cs=srgb&dl=pexels-daria-shevtsova-3626605.jpg&fm=jpg)
photo credits: pexels royalty free image usable without attribution

In these cases, we can see a main pipe branch splitting off into several branches. How then can we consider the hydrostatic pressure
contributions in such a way?

If a pipe is angled such that it is parallel to the ground, we need
not really worry about pressure drop contributions as much of the weight
of the fluid is supported by the pipe itself, not the fluid below it.

To the contribution of pressure at 90 degrees is:

$$\Delta h = D_H \sin \theta = D_H$$

$\theta$ is the angle of the pipe compared to the normal.

How should the hydrostatic area vector be considered? For the case
of the column of fluid lying flat on the ground, the cross sectional
area would be zero as we need only consider the pressure exerted by the 
fluid above the pipe junction.

So the cross sectional area varies like the cosine of inclined angle


$$A_{parallelBranch} = A_{XS} \cos \theta$$

And the total height of the said branch should be 

$$\Delta h = D_H \sin \theta + L cos \theta$$

The force is then:

$$F_{hydrostaticBranch} = \Delta h \rho g A_{parallelBranch}$$
$$F_{hydrostaticBranch} = (D_H \sin \theta + L cos \theta) 
\rho g A_{XS} \cos \theta$$


$$F_{hydrostaticBranch} = (D_H \sin \theta \cos \theta 
+ L cos^2 \theta) 
\rho g A_{XS} $$

$$F_{hydrostaticBranch} = (D_H \sin \theta \cos \theta 
+ L cos^2 \theta) 
\rho g A_{XS} $$

$$F_{hydrostaticBranch} = 0.5(D_H \sin 2 \theta  
+ L (cos 2 \theta +1))
\rho g A_{XS} $$

We can simply things noting that unless the parallel branch is really
short,

$$L >> D_H$$

Our equation then simplifies to:

$$F_{hydrostaticBranch} = 0.5( L(cos 2 \theta +1))
\rho g A_{XS} $$

$$F_{hydrostaticBranch} =  L(cos^2 \theta)
\rho g A_{XS} $$


$$F_{hydrostaticBranch} =  Lcos \theta
\rho g A_{XS} \cos \theta$$

$$F_{hydrostaticBranch} =  h
\rho g A_{XS} \cos \theta$$

Now this means that in general we should always weigh our cross 
sectional area by the angle at which contact is made


$$\rho_{Parallel}   
= \frac{\sum_i^n \rho_i  A_{XSi} \cos \theta_i}
{\sum_i^n A_{XSi} \cos \theta_i}$$

Now of course, if we want to be exact, then we must also include 
pressure contributions at the tee of the pipe, and depending how the
tee is shaped, we have to apply different formulas.

For simplicity, i opt to ignore the tee area as we might just consider 
it in a separate component.

However, suppose we have a 90 degree bend at the pipe junction,
and the parallel pipe segment bends upwards at 90 degrees.

![pipe photo](https://images.pexels.com/photos/3626605/pexels-photo-3626605.jpeg?cs=srgb&dl=pexels-daria-shevtsova-3626605.jpg&fm=jpg)
photo credits: pexels royalty free image usable without attribution

The formula above suggests we should ignore such a pipe because at the
junction, the angle of contact is 90 degrees. Thus, this formula can
only apply for special cases of pipes and tees.

To avoid such cases, we can simply use:

$$\rho_{Parallel}  A_{XSparallel} 
= \sum_i^n \rho_i  A_{XSi}$$

Such a case would account for all the possible bends within the pipe,
even if such bends go below and then above the tee junction area.



And now for the next part, what if the piping system is asymmetric?
And the cross sectional area at the top and bottom of the pipe would 
differ? For hydrostatic pressure, we are not considering the pressure
of the fluid column above the pipe, but rather below,

we should be using the cross sectional areas which are closer to the
ground for reference, regardless of flow path.


#### General principle for obtaining cross sectional areas
In general
, even if we have several pipe bends, and the pipes differ in
cross sectional area throughout the system, the general principle
if one were to use this kind of force balance is to insert a plane
parallel to the ground, and then take the cross sectional areas
of the pipes projected on such a plane. We shall then weigh the 
hydrostatic pressures by those cross sectional areas.


$$\rho_{Parallel}  A_{XSparallel} 
= \sum_i^n \rho_i  A_{XSparalleli}$$

In general 

$$A_{XSparallel} \neq A_{XS}$$


By convention we use entrance cross sectional area to calculate a 
representative reynold's number. So for a general pipe or component
we need to use two different cross sectional areas if possible.

#### Simplifying assumption: Constant cross section area in pipe branch.
Otherwise, a simplifying assumption would be to assume these areas
are the same. As long as we use the same area to weight our density always, shouldn't be too much of an issue.


So this way to weighting densities should in theory capture
the effects of individual hydrostatic pressures on the net hydrostatic
pressure exerted by this parallel assembly of components.

If anything else, the user should then judiciously choose the correct
hydrostatic cross sectional area. But it should minimise the tee 
height and the user must use a plane with the normal parallel to 
direction of gravitational acceleration, and cut the pipe using that 
plane. The projected areas upon the plane will provide the area of 
weightage for this method.

Fortunately, this only has to be done once and only once for a parallel
pipe branch. After a single calculation, perhaps by hand, then 
the user can move on to using other parts of the code.

If anything else, and if there are pumps in the parallel setup, one
can again use a force balance and cross sectional
areas to give weights for the average density.

### Program design implications
Programatically speaking, each parallel branch must be able to supply
a hydrostatic pressure and a corresponding hydrostatic area with which
to weight density.

The code will then perform averaging using this formula:

$$\rho_{Parallel}  A_{XSparallel} 
= \sum_i^n \rho_i  A_{XSparalleli}$$

To supply the average density of this system.



## Problem 2: Solving the issue of Aiding and Opposing Mixed Convection

Now we have decided to weight density by the cross sectional area.
In a simplifying assumption, that cross sectional area would be
the hydraulic cross sectional area, but more often than not, it will
not be the case. And it's up to the user to judiciously decide.

However, we shall not consider that issue here.

The scenario now is that we have determined the driving force supplied
by this parallel pipe section due to buoyancy factors. Once that driving
force is determined, we can use the fLDK factors for the parallel
pipe system to estimate a total mass flowrate.

This is assuming we can even estimate fLDK for the parallel subcircuit
in the first place. We shall discuss that in another section.


And we now want to determine a pressure loss term for 
this given a mass flowrate.

For purposes of speed, we want to represent the pressure loss terms
of this parallel fluid subcircuit using this singular correlation.


$$\Delta P_{totalChange} = 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 - \rho_{parallel} g \Delta H_{parallel}$$

The trouble is, depending on the relative temperatures of the 
branches in parallel, the fLDK factor can change. For the same
$\rho_{parallel}$ using the aforementioned weighting method,
the different branches can be experiencing aiding or opposing mixed 
convection depending on the temperature distribution. As such,
fLDK for each branch will be different in those scenarios, and 
fLDK for the parallel system in total will also change.

We can of course obtain this iteratively, but that is too time consuming
and we want to avoid is as far as possible. We would however like to
obtain a pressure loss term perhaps within 1% of the iteration methods
using another method to guestimate this overall pressure loss term.

However, the challenge is we cannot solve for this pressure drop 
iteratively using a while loop. Otherwise the simulation would be too
computationally resource heavy and time consuming.

### Buoyancy Correction Step
For a pipe system we consider:

$$Be_{Di} = 0.5 Re_i^2 (f_i \frac{L_i}{D_i} +K_i)$$

In effect it is easy for us to derive this correlation for a single
pipe because we can decouple the effects of buoyancy and pressure loss
in these equations.

Thus if we were to ignore buoyancy altogether, we could also obtain:

$$Be_{Dparallel} = 0.5 Re_{parallel}^2 (
	f_{parallel }  \frac{L_{parallel } }
	{D_{parallel } } +K_{parallel } )$$

Thus, we could still obtain fLDK parallel without buoyancy, and perhaps
perform a few steps to correct for buoyancy effects within the branch.

The only question is how?

We shall have to take advantage of the fact that pressure change
across each branch is equal.

$$ -\Delta P_{totalChange}= 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } - \rho_i g \Delta H_i$$

If we were to equate this to the components of the system:

$$ 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 - \rho_{parallel} g \Delta H_{parallel}
$$
$$ = 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } - \rho_i g \Delta H_i$$

Rearranging to make the pressure loss terms across each branch labelled
branch i the subject:

$$ 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 - \rho_{parallel} g \Delta H_{parallel}+ \rho_i g \Delta H_i
$$
$$ = 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } $$

Note that the total change in height for each branch must also be the
same as that in parallel:

$$ 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 +(\rho_i- \rho_{parallel}) g \Delta H_{parallel}
$$
$$ = 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } $$

We note that at this stage, the pressure loss over branch i can be
explicitly calculated:

$$\Delta P_{lossBranch\ i} = 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } 
=
$$

$$ 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 +(\rho_i- \rho_{parallel})\rho_i g \Delta H_{parallel}
$$

Assuming that each parallel branch already has the Reynold's number
and Bejan number mapped out, we can easily obtain the mass flowrate
across that pipe branch:

$$\dot{m_i}= 
A_{XSi}\sqrt{ \frac{\rho_i(0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )\rho_i g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$

This assumes the fLDK term for each branch can be easily and quickly
calculated (we need a Re vs Be correlation for which we can use):


$$Be_{Di} = 0.5 Re_i^2 (f_i \frac{L_i}{D_i} +K_i)$$

One can use the interpolation for this parallel pipe branch, read off
the Reynold's number, and calculate fLDK for this branch using:

$$f_i \frac{L_i}{D_i} +K_i = \frac{Be_{Di}}{0.5 Re_i^2}$$

Given that the correlation is already there, this step is relatively
straightforward.

Of course, if the pressure drop is negative, we don't get imaginary
numbers here, we simply reverse the direction of flow for the mass 
flowrate.


Once that is done, we can compute a new total mass flowrate by
summing up the mass flowrate of the individual branches:

$$\dot{m}_{totalEstimated} = \sum_i^n A_{XSi}
\sqrt{ \frac{\rho_i(0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )\rho_i g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$

It is apparent that the total mass flowrate calculated from this step
will not be the same as the mass flowrate specified externally.

If the pipes are generally experiencing aiding mixed convection,
then the total estimated mass flowrate calculated here 
$\dot{m}_{totalEstimated}$
will be more than that of the mass flowrate without aiding mixed
convection. In effect, we have overestimated the fLDK term for 
the parallel setup.

Having an average $\rho_{parallel}$ partially corrects for this case,
but in terms of finding the branch flowrates accurately, one must go
through these steps of iteration. To perform additional correction.

If fLDK is overestimated, we can correct for this by assuming a lower
pressure loss term across the entire parallel pipe branch. Thus,
we can make corrections to the fLDK term by using a corrected mass
flowrate to estimate fLDK.


$$\dot{m}_{corrected} = \dot{m}_{parallel} *\frac{\dot{m}_{parallel}}
{\dot{m}_{totalEstimated}}$$


$$\dot{m}_{corrected} = \frac{\dot{m}^2_{parallel}}
{\dot{m}_{totalEstimated}}$$

Using this corrected mass flowrate, we can re-estimate fLDK parallel
using the Bejan number vs Reynold's number correlation,


$$Be_{Dparallel} = 0.5 Re_{corrected}^2 (
	f_{corrected }  \frac{L_{corrected } }
	{D_{corrected } } +K_{corrected } )$$

We can then use this new pressure loss term and assume this is the 
pressure loss for the specified flowrate because we already did one
correction step.

This would then be a quick way to estimate pressure loss terms for
the entire parallel setup when correcting for natural buoyancy.

It remains to be seen whether this step gets us sufficiently close.

The assumption here is that forced convection will dominate the flow,
rather than natural convection, and that the buoyancy corrections to 
obtain the pressure losses and fLDK factors are small. So that only
a single step is required to correct for reduced or additional losses
due to aiding or opposing mixed convection.

Of course, one could add two or three more steps, but the key here is
to avoid using a while loop so that calculation time can be minimised.


### Possible Numerical Instabilies

For such a calculation, the way we estimate the corrected mass flowrate
depends on:

$$\dot{m}_{corrected} = \frac{\dot{m}^2_{parallel}}
{\dot{m}_{totalEstimated}}$$

Any time division is in the picture, there would be trouble the 
denominator approaches zero.

Now if the total estimated mass flowrate is close to zero, and the 
stipulated mass flowrate is a significant nonzero value, then it is 
apparent that this method of calculating mass flowrates is not
suitable.

Major corrections are needed. And the underlying assumptions are 
not correct.

Programatically, we can just throw an exception if this occurs.

The condition to throw this exception is that:

$$\frac{\dot{m}_{totalEstimated}}{\dot{m}_{parallel}} < 0.01$$

This would then cover situations where the flow would have reversed
or we have zero flow.


#### zero external flow

When exactly zero external flow is expected, we have for the
branch flowrates:

$$\dot{m_i}= 
A_{XSi}\sqrt{ \frac{\rho_i(0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )\rho_i g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$

$$\dot{m_i}= 
A_{XSi}\sqrt{ \frac{ (- \rho_{parallel}+\rho_i )\rho_i g \Delta H_i}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$

One can only hope they sum up to zero within some instrument error
value.

Otherwise, an exception will have to be thrown.


### When natural convection dominates flow

When natural convection dominates flow, eg. 100\% natural circulation
where a heated area of the loop causes fluid to rise and a cooler
area of the loop causes fluid to fall,

We would still account for the natural convection by weighting our
density properly and sum it all over the loop.

However, the pressure loss across the parallel branches driving the flow
will be vastly different compared to an external force driving the flow.

If an external force drives a flow across a heat producing region with
bypass regions, all branches have some degree of flow. But if said heat
producing region is driving the flow, then the bypass regions would have
little flow going through those regions as there is little fluid going 
through them. They may not sink so much because the temperature of
those branches is high enough not to let it flow downwards,
but the flow through them may be minimal as the fluid in the tee region
is relatively similar in temperature to them.


One can only hope this algorithm works well enough to correct for the
fLDK factors. Programatically speaking, we can just throw an exception
and wait for further notice if this occurs, or perform a series of
tests in order to ascertain if the correction algorithm is good enough.

This will be the subject of a study in itself. 

In a sense, this method will have to be validated on a system by 
system basis.

The only boon to this step is that it can provide estimates rather
quickly and without iteration.

## Problem 2b: Estimating fLDK for the parallel subcircuit so we can obtain mass flowrate in the first place

Now, in order to calculate the pressure losses given a mass flowrate,
we have to calculate the mass flowrate first assuming a pressure loss
in the wider series circuit.

This involved the use of estimating fLDK for the branch in parallel when
natural convection is negligible. Thereafter we could plot this fLDK
and use it as an initial guess in order to apply some correction step
to estimate branch flowrates and also overall pressure loss within
the parallel subcircuit.

We assumed an fLDK for this parallel sub circuit and estimated an
external mass flowrate. 
However, correcting pressure loss terms for buoyancy and
therefore fLDK would cause the prevailing mass flowrate to change,
and which would then necessitate iterating for yet another
pressure loss and fLDK within this parallel subcircuit until
the mass flowrate and pressure losses somehow converge.

This is not ideal..

We then have to limit the capabilites of this algorithm, and limit
it to situations where forced flow dominates the parallel subcircuit,
the flow is not driven by natural convection. That is if we want
the flow estimates here to be accurate.

We should add a caveat that the flow through the loop cannot be driven
by a source within the parallel subcircuit; another algorithm must
be used to solve such problems.


## Problem 3: deriving how to average nondimensional parameters

Recall that we want to nondimensionalise this equation:

$$\Delta P_{totalChange} = 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 - \rho_{parallel} g \Delta H_{parallel}$$
into 

$$Be_{Dparallel} = 0.5 Re_{parallel}^2 (
	f_{parallel }  \frac{L_{parallel } }
	{D_{parallel } } +K_{parallel } )$$

These are how the Re and Be are defined:
$$Re = \frac{\dot{m}}{ A_{XS}} \frac{D_H}{\mu}$$
$$Be_D = \frac{\Delta p D_H^2}{\nu^2} = \frac{\Delta P D_H^2}{\mu \nu}$$

What are the parameters we need to use to nondimensionalise pressure
drop and mass flowrate?

1. hydraulic diameter $D_H$
2. cross sectional area $A_{XS}$
3. dynamic viscosity $\mu$
4. density $\rho$


### 3a. Mass flowrate

the mass flowrate of the parallel subcircuit is the sum of mass
flowrate through the branches


$$\dot{m}_{parallel} = \sum_i^n \dot{m}_i $$

### 3b. Characteristic hydraulic diameter and cross sectional area

For cross sectioal area and hydraulic diameter, 
 we shall define the 

We shall define that the area of this parallel component is
the sum of the constituent areas, for convenience. Plus it would make intuitive
sense.

$$A_{XSparallel} = \sum_i^n A_{XSi}$$

Also, because of this, we can get a proper lengthscale for a representative
hydraulic mean diameter.

$$D_{parallel}^2 \frac{\pi}{4} = A_{XSparallel}$$



### 3c. Density

I am only devleoping this solver for mainly single phase 
incompressible flow. The boussinseq approximation will therefore apply.

To save on doing extra work, i shall define the density as the area
weighted average density:

$$\rho_{Parallel}  A_{XSparallel} 
= \sum_i^n \rho_i  A_{XSparalleli}$$


### 3d. Viscosity $\mu$

To correctly find a weighted average of viscosity, we must first 
consider where its effects are felt the most.

We shall see that in the pressure loss equations, viscosity cancels
out when we redeimensionalise our terms.

For each branch in parallel we have:

$$Be_{Di} = 0.5 Re_i^2 (f_i \frac{L_i}{D_i} +K_i)$$

Can be reduced to:

$$\Delta P_{loss-i} = 0.5 \frac{\nu^2 \rho}{\mu^2} 
(f_i \frac{L_i}{D_i} +K)
\frac{\dot{m}_i^2 }{A_{XSi}^2 }$$

And now the  viscosities as well:

$$\Delta P_{loss-i} = 0.5 \frac{1}{\rho} (f_i \frac{L_i}{D_i} +K)
\frac{\dot{m}_i^2 }{A_{XSi}^2 }$$

Notice here that viscosity is not explicitly in the pressure loss
equation. It is however, implicitly found in the darcy friction factor
found here.

We can then consider two extremes, 

1. viscous forces dominate
2. turbulent forces dominate

#### Bounding case: full turbulence

Under fully turbulent flow regime

$$\frac{1}{\sqrt{f_{darcy}}} = -2 \log_{10} (\frac{\varepsilon/D}{3.7})$$

Essentially, no matter how we choose viscosity, it has no bearing
on the darcy friction factor.

In this case, getting a mass flowrate is relatively simple:


Now before we carry on, we should note that this equation pertains to a
pressure drop rather than a pressure 
change or loss term across the pipe. This is because
we neglect hydrostatic pressure. So in actuality,

$$ \Delta P_{totalChange} = \Delta H_i \rho_i g- \Delta P_{loss-i}  $$


$$\frac{\dot{m}^2}{A_{XS}^2} = 
\frac{(- \Delta P_{totalChange} +\rho_i g \Delta H_i) 
\rho_i}{0.5 (f_i \frac{L_i}{D_i} + K_i)}$$

Rearranging to find mass flowrate:
$$\dot{m}_i= 
A_{XSi}\sqrt{ \frac{\rho_i(\Delta P_{totalChange} 
+\rho_i g \Delta H_i)}{0.5 (f_i \frac{L_i}{D_i} + K_i)}}$$

Square root:
$$\frac{\dot{m}^2}{A_{XS}^2} = \frac{(-\Delta P_{totalChange} +\rho g \Delta H)
 \rho}{0.5 (f \frac{L}{D} + K)}$$

$$\frac{\dot{m}}{A_{XS}} =\sqrt{ 
	\frac{\rho(-\Delta P_{totalChange} +\rho g
 \Delta H)}{0.5 (f \frac{L}{D} + K)}}$$

Now, we assume this parallel setup can be represented as a 
single fLDK component.
we substitute this expression in, so we get:



$$-\Delta P_{totalChange} = 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 - \rho_{parallel} g \Delta H_{parallel}$$

Here we know

$$H_{parallel} = H_i$$

So we get:

$$\dot{m}_{i} =  A_{XSi}
\sqrt{ \frac{\rho_i(0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$

Summing up the components of each branch,

$$\dot{m}_{total} = \sum_i^n A_{XSi}
\sqrt{ \frac{\rho_i(0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$


Which for a realistic pipe system in parallel, the change in height at the
final and initial point must be in the same physical space!


Let's tidy up before going further. I will note that
$\dot{m}_{parallel}= \dot{m}_{total}$ and i will divide throughout
to get mass fractions.


$$1 = \sum_i^n A_{XSi} 
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 A_{XS{parallel}}^2}}$$

If i took the area of the entire parallel setup out, i'd get:

$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$

This in fact becomes our weighting factor with which to find the flowrate 
distribution amongs all the flows.

Here is a simple dimensions check to ensure our units match.
The hydrostatic term is:

$$A^2 \rho^2 gz = m^4 kg^2 m^{-6} m s^{-2} m$$
$$A^2 \rho^2 gz = kg^2   s^{-2} $$

Units look ok!

With units checking out we are left with:

$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$

If i define some weighting factor:

$$ w_{XSareai} =\frac{A_{XSi}}{A_{XS{parallel}} }
  $$

And for simplification i ignore buoyancy forces, i can get an expression
for fLDK in parallel.

$$k_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel} = \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (k_i \frac{L_i}{D_i} + K_i)}} \right)^{-2}  
$$

Every fLDK term in the branches is user defined, so we may find out
what fLDK is in totality for a turbulent regime.

In the simple case that form losses are the only contributing factor
or major contributing factor, 
$$ K_{parallel} = \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{  K_i}} \right)^{-2}  
$$

This will be how we weight form losses from now on.

If this were true, we would then find a way of finding $k_{parallel}$
based on this definition

$$k_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 = \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (k_i \frac{L_i}{D_i} + K_i)}} \right)^{-2}  -
\left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{  K_i}} \right)^{-2}  
$$

This helps us get the factor kL weighted. Now as long as we specify
the system length, we would get the system k value so to speak.


The challenge now then is to find  $f_{parallel}$ and
$L_{parallel}$ as well as $\mu_{parallel}$

For that we shall look at another bounding case: laminar flow




#### Bounding case: Creeping flow

Recall that we simplify the following equation:
$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$

By assuming it can be decoupled from the buoyancy forces

And we are left with:
$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) }
{ 0.5 (f_i \frac{L_i}{D_i} + K_i)  }}$$

To have us find out how viscosity is weighed, we should take into
account the case where viscous forces dominate. This is the creeping
flow assumption, where Re is so low that form losses are negligible.

That means $K_i \approx 0$ compared to the form losses. And also by
extension, that means $K_{parallel} \approx 0$ compared to the 
form losses of the parallel setup

This would leave us with:

$$
\sqrt{ \frac{1}
{ (f_{parallel} \frac{L_{parallel}}{D_{parallel}} )}}
 =\frac{1}{\sum_i^n A_{XSi}} \sum_i^n A_{XSi}
\sqrt{ \frac{1}{ (f_i \frac{L_i}{D_i} )}}$$

We again refer to our weighing factors:
$$w_{XSareai} = \frac{A_{XSi}}{\sum_i^n A_{XSi}} $$

And we substitute the expression for friction factor under creeping flow
or laminar flow assumptions:
$$f_{darcy} = \frac{64}{Re} = \frac{64 A_{XS} \mu}{\dot{m} D_H}$$


So we get:



$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (\frac{64 A_{XSi} \mu}{\dot{m}_i D_i} 
\frac{L_i}{D_i} )}} \right)^{-2}  
= \frac{64 A_{XS} \mu}{\dot{m} D_{parallel}} 
\frac{L_{parallel}}{D_{parallel}} $$

Now we will consistently relate cross sectional area to hydraulic 
diameter here:

$$A_{XS} = \frac{\pi}{4} D_H^2$$

After substituting this expression in, we get:
$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (\frac{16 \pi \mu L_i}{\dot{m}_i } 
)}} \right)^{-2}  
= \frac{16 \pi \mu L_{parallel}}{\dot{m} } 
$$

$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}} \right)^{-2}  
= \frac{16 \pi \mu_{parallel} L_{parallel}}{\dot{m_{total}} } 
$$

Or if we don't want to see the inverse square:

$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{\dot{m_{total}}}{16 \pi \mu_{parallel} L_{parallel} } }
$$


Now we may not know the individual mass flowrates for this case, but
in stokes regime or fully laminar regime, Ohm's law actually applies
for each branch if we were to take the fluid pipe and treat it as if
it were a resistor.

Therefore, pressure losses are linearly correlated to mass flowrate.

Let's revisit this equation:


$$\Delta P = 0.5 \frac{1}{\rho} (f \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

When we apply darcy friction factor is 64/Re and neglect K:

$$\Delta P = 0.5 \frac{1}{\rho} (\frac{64}{Re} \frac{L}{D} )
\frac{\dot{m}^2 }{A_{XS}^2 }$$

$$\Delta P = 0.5 \frac{1}{\rho} (\frac{64 A_{XS} \mu}{\dot{m} D_H} \frac{L}{D_H})
\frac{\dot{m}^2 }{A_{XS}^2 }$$

Let's tidy up the equation first by cancelling 
mass flowrate and cross sectional area.



Substiute 
$$A_{XS} = \frac{\pi}{4} D^2$$

$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$

If we don't consider buoyancy forces, then we can substitute this
expression back in:


$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{\dot{m_{total}}}{16 \pi \mu_{parallel} L_{parallel} } }
$$


$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$


We thus arrive at a way of finding mass flowrate from pressure losses.

If we ignore buoyancy and other source terms
, the pressure losses are equal to the total
pressure change in each branch:

$$\Delta P = \frac{1}{\rho_i} (8 \pi \mu L)_i
\frac{\dot{m}_i }{A_{XSi}^2 }
=\frac{1}{\rho_{parallel}} (8 \pi \mu L)_{parallel}
\frac{\dot{m}_{parallel} }{A_{XSparallel}^2 }
$$

Removing pressure from the equation, density (via boussinesq 
approximation)
and the factor of $8\pi$, we can obtain a convenient ratio with which
to aid our calculations:

$$ (\mu L)_i
\frac{\dot{m}_i }{A_{XSi}^2 }
= ( \mu L)_{parallel}
\frac{\dot{m}_{total} }{A_{XSparallel}^2 }
$$


$$\frac{m_i}{m_{total}} =
\frac{(\mu L)_{parallel}}{(\mu L )_i} w_{XSAreai}^2$$


Let's substitute this back here:
$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{\dot{m_{total}}}{16 \pi \mu_{parallel} L_{parallel} } }
$$

So we get:

$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{\frac{(\mu L)_{parallel}}{(\mu L )_i} w_{XSAreai}^2}{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{1}{16 \pi \mu_{parallel} L_{parallel} } }
$$

Let's condense the area ratios, remove the constant of $16\pi$,
and combine the viscosity and length terms:


$$ \sum_i^n  \frac{w_{XSareai}^2}{L_i} \frac{1}{\mu_i }  = 
\frac{1}{\mu_{parallel} L_{parallel}}
$$
We thus come up with a very simple way of weighting our lengths
and viscosities. 

$$ \sum_i^n  \frac{w_{XSareai}^2 L_{parallel}}{L_i} 
\frac{1}{\mu_i }  = 
\frac{1}{\mu_{parallel} }
$$

From this expression, it looks like viscosity is weighted by
length and cross sectional area ratios.

Moreover, it looks as if we have arrived at some new weighting
ratio here:

$$\frac{w_{XSareai}^2 L_{parallel}}{L_i}$$

So if we were to intrepret this as such, then we to 
normalise our weighting factors like so:

$$\sum_i^n  \frac{w_{XSareai}^2}{L_i}   = 
\frac{1}{ L_{parallel}}
$$

In this regard, we have found our weighted average of Length

$$ L_{parallel} =    
\frac{1}{\sum_i^n  \frac{w_{XSareai}^2}{L_i} }
$$

We can also use this to find the smaller k value when the system
reaches full turbulence.

With this length, we can use this to weight our viscosities:


$$ \sum_i^n  \frac{w_{XSareai}^2}{L_i} \frac{1}{\mu_i }  = 
\frac{\sum_i^n  \frac{w_{XSareai}^2}{L_i} }{\mu_{parallel} }
$$

$$\frac{\sum_i^n  \frac{w_{XSareai}^2}{L_i} }
{ \sum_i^n  \frac{w_{XSareai}^2}{L_i} \frac{1}{\mu_i }} =
  \mu_{parallel} 
$$

With these, we have found weighting methods for our viscosity,
and that was really the point anyhow.


### estimation methods for Be

upon writing the code, i know i have 
a simple way for finding evenly spaced sampling points
Reynold's number points. However
, i don't know a simple way for finding evenly
spaced Bejan number points... 

Bejan number is usually the dependent 
variable, not independent variable. So
there aren't usually Bejan number ranges 
i can think off the top of my head.

However, i know for pipes, there 
are bejan numbers corresponding to Reynold's
numbers. And i can assume the system 
behaves like a pipe in terms of distribution.

I will use the log spaced Reynold's 
numbers to guess the corresponding Bejan
numbers, and will use the Bejan numbers 
as guess points to get the corresponding
Reynold's numbers.

For such a case, i will assume the system behaves like a component

$$(f \frac{L}{D} +K)_{parallel} = (k_{parallel} + \frac{64}{Re} )
\frac{L_{parallel}}{D_{parallel}} +K_{parallel} $$

Where: 
$$A_{XSparallel} = \sum_i^n A_{XSi}$$

$$D_{parallel}^2 \frac{\pi}{4} = A_{XSparallel}$$
$$ w_{XSareai} =\frac{A_{XSi}}{A_{XS{parallel}} }
  $$



$$\sum_i^n  \frac{w_{XSareai}^2}{L_i}   = 
\frac{1}{ L_{parallel}}
$$

$$ K_{parallel} = \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{  K_i}} \right)^{-2}  
$$


$$k_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 = \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (k_i \frac{L_i}{D_i} + K_i)}} \right)^{-2}  -
\left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{  K_i}} \right)^{-2}  
$$
I use this because of simplicity and it's ability to handle both
bounding cases of creeping flow and fully turbulent flow.


So this is how i will guess bejan number.

### Guessing Pressure Drop from Bejan number (parallel case)

Now that Bejan number is guessed, we have to obtain a pressure
drop with which to test the parallel pipe setup.


$$Be_D = \frac{\Delta P D_H^2}{\nu \mu}$$

From our derivations previously, and based on boussinesq approximation:

$$A_{XSparallel} = \sum_i^n A_{XSi}$$

$$D_{parallel}^2 \frac{\pi}{4} = A_{XSparallel}$$
$$ w_{XSareai} =\frac{A_{XSi}}{A_{XS{parallel}} }
  $$
$$\mu_{parallel} = \frac{\sum_i^n  \frac{w_{XSareai}^2}{L_i} }
{ \sum_i^n  \frac{w_{XSareai}^2}{L_i} \frac{1}{\mu_i }} 
$$

$$\rho_{Parallel}   
= \frac{\sum_i^n \rho_i  A_{XSi}}{\sum_i^n A_{XSi}}$$

Thus we obtain a series of pressure changes with which to test each 
branch

### Testing for ParallelSubCkts
Now for parallelSubCkts with this setup, what tests do we need?
Assume that each branch has been condensed into a single component.

0. basic functionality
a. if i use the nondimensionalised interpolation method across various Re
it should yield the same results as with a normal calculation (ie using the
bisection or whatever else implicit methods).
Meaning to say i'll want two classes of FluidParalleSubCircuit, one reference
class and one interpolated class. Otherwise, hook up a current source to two
IsothermalPipes in parallel. That will be the reference. Then test this against
the parallelSubCkts.

Unfortunately, i tried the current source to parallel setup, failed miserably.



1. Elevation tests, 
a. means if i elevate the branch i should expect the proper mass flowrate
given a pressure drop
b. it also means that if i elevate a branch, both branches should elevate
equally. Meaning to say both branches should have the same elevation change.
c. if i elevate branches and the temperatures change, i should expect a different
flowrate due to the temperature change (this is more of a density change however
probably do it later)

2. Density and viscosity change tests (more of temperature change)
a. if i calculate the mass flowrate from a pressure drop using the 
nondimensionalised correlation, and change density and viscosity halfway thru,
i should expect the massflowrate to be correct using the same nondimensionalised
correlation.


Also, i forgot a few unit tests for my IFluidEntity anyhow when using the 
interpolation thingy. Like the dynamic pressure tests are not done yet.
For Isothermal pipe it isn't implemented properly and for fluidparallel subckt
it's not even implemented.





## Nested tests with FluidSeriesCircuits (work in progress)

Now after the interpolation strategy, things seem to be working out for the 
most part. The code runs a lot faster. And the only significant time taken
to solve the FluidSeriesCircuit is the time taken to build the fluidSystemCurves.

Except when i try to use the getMassFlowRate within the FluidSeriesCircuits 
the iterations cause the solver to go beserk. And i get numerically unstable 
answers. Which to be fair, is kind of beyond interpolation range.

Now, my way of getting massFlowrate is to use an iterative root finding algorithm
which guesses pressureDrop by changing mass flowrate until the guessed 
pressureDrop matches that of the desired pressureDrop. My mass flowrates are tested
from -1e12 to 1e12 kg/s. Kind of too much tbh and it may be beyond the range of
interpolated values anyhow.


One possible way to change this is to change the range of roots of the mass flowrate
from maximum to minimum possible Reynold's number of 1e12 and -1e12 respectively.

Now to get a Reynold's number representative of the whole system, one must then
think of how to scale things properly. What is the hydraulic diameter, what is
the viscosity, what is the mass flowrate and what is the cross sectional area.

Can we use ensemble averages or must we use something more sophisticated?

I think we must develop some proper theory for FluidSeriesCircuits and
FluidSeriesSubCircuits on how to nondimensionalise things properly.

The methodology will be assume there is one representative component that produces
the same pressure drop for a given mass flowrate as a series of pipes and 
components. 






