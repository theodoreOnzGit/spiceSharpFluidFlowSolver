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

Lastly, what are the parameters we need to use to nondimensionalise pressure
drop and mass flowrate?

1. hydraulic diameter $D_H$
2. cross sectional area $A_{XS}$
3. dynamic viscosity $\mu$
4. density $\rho$

With this, we can always guess the pressure drop given a mass flowrate.


For a series of parallel components, we have:

$$Be_{Di} = 0.5 Re_i^2 (f_i \frac{L_i}{D_i} +K_i)$$



Let's get those dimensionless parameters out so we can expose the mass flowrate
$$\frac{\Delta P D_H^2}{\nu \mu}= 0.5 (f \frac{L}{D} + K) Re^2$$


$$\Delta P = 0.5 \frac{\nu^2 \rho}{D_H^2} (f \frac{L}{D} + K) Re^2$$

Now let's perform some cancellations noting that:

$$Re = \frac{\dot{m} D_H}{A_{XS} \mu}$$


$$\Delta P = 0.5 \frac{\nu^2 \rho}{D_H^2} (f \frac{L}{D} + K)
\frac{\dot{m}^2 D_H^2}{A_{XS}^2 \mu^2}$$

Let's cancel out the diameters:
$$\Delta P = 0.5 \frac{\nu^2 \rho}{\mu^2} (f \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

And now the  viscosities as well:

$$\Delta P = 0.5 \frac{1}{\rho} (f \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

Now before we carry on, we should note that this equation pertains to a
pressure drop rather than a pressure change across the pipe. This is because
we neglect hydrostatic pressure. So in actuality,




$$\frac{\dot{m}^2}{A_{XS}^2} = 
\frac{(\Delta P_{totalChange} +\rho g \Delta H) 
\rho}{0.5 (f \frac{L}{D} + K)}$$

Square root:
$$\frac{\dot{m}^2}{A_{XS}^2} = \frac{(\Delta P_{totalChange} +\rho g \Delta H)
 \rho}{0.5 (f \frac{L}{D} + K)}$$

$$\frac{\dot{m}}{A_{XS}} =\sqrt{ \frac{\rho(\Delta P_{totalChange} +\rho g
 \Delta H)}{0.5 (f \frac{L}{D} + K)}}$$

$$\dot{m}= 
A_{XS}\sqrt{ \frac{\rho(\Delta P_{totalChange} +\rho g \Delta H)}{0.5 (f \frac{L}{D} + K)}}$$

In general for any branch eg. branch-i that can be represented as an fLDK 
component, we have the correlation as follows:

$$\dot{m_i}= 
A_{XSi}\sqrt{ \frac{\rho(\Delta P_{totalChange i} +\rho_i g \Delta H_i)}
{0.5 (f_i \frac{L_i}{D_i} + K_i)}}$$



Suppose now that we have the total mass flowrate $\dot{m}_{total}$

$$\dot{m}_{total} = \sum_i^n \dot{m}_i$$

$$\dot{m}_{total} = \sum_i^n A_{XSi}
\sqrt{ \frac{\rho(\Delta P_{totalChange i} +\rho_i g \Delta H_i)}
{0.5 (f_i \frac{L_i}{D_i} + K_i)}}$$

Now mass balance is already looking pretty scary and complicated.

Thankfully though, we are coming at this problem already knowing
the total mass flowrate. The whole idea is to iterate the pressure change
from the mass flowrate.

To help us. we use another property of parallel pipe systems, that
the sum of pressure change across each pipe section is equal, including
hydrostatic pressure and pressure drop etc.


$$\Delta P_{totalChange}  = 0.5 \frac{1}{\rho} 
(f \frac{L}{D} + K) \frac{\dot{m}^2 }{A_{XS}^2 } - \rho g \Delta H$$

$$constant = 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } - \rho_i g \Delta H_i$$

This gives us an easy way of finding the mass flowrates in the 
respective branches if we know the friction factors and k within each branch.


#### Fundamental correlation to find parameters for parallel circuit
And now suppose that this parallel setup should be represented by an fLDK
type component

$$ \dot{m}_{total} = \sum_i^n A_{XSi}
\sqrt{ \frac{\rho_i(\Delta P_{totalChange i} +\rho_i g \Delta H_i)}{0.5 (f_i \frac{L_i}{D_i} + K_i)}}$$



This is essence helps us guess the correct properties via mass balance equation


We can also take advantage of the fact that pressure changes across each
branch are equal:

$$\Delta P_{totalChange}  = 0.5 \frac{1}{\rho} 
(f \frac{L}{D} + K) \frac{\dot{m}^2 }{A_{XS}^2 } - \rho g \Delta H$$


$$\Delta P_{totalChange} = 0.5 \frac{1}{\rho_i} 
(f_i \frac{L_i}{D_i} + K_i) 
\frac{\dot{m}_i^2 }{A_{XSi}^2 } - \rho_i g \Delta H_i$$

$$\Delta P_{totalChange} = 0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
 - \rho_{parallel} g \Delta H_{parallel}$$

Now supposing again that the parallel pipe system is meant to be represented by
a single representative fLDK component:




$$\dot{m}_{total} = \sum_i^n A_{XSi}
\sqrt{ \frac{\rho_i(\Delta P_{totalChange i} +\rho_i g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$


$$\dot{m}_{total} = \sum_i^n A_{XSi}
\sqrt{ \frac{\rho_i(0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{parallel}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )g \Delta H_i)}
{ 0.5(f_i \frac{L_i}{D_i} + K_i)}}$$

Here we know

$$H_{parallel} = H_i$$

Which for a realistic pipe system in parallel, the change in height at the
final and initial point must be in the same physical space!

$$\dot{m}_{total} = \sum_i^n A_{XSi}
\sqrt{ \frac{0.5 \frac{1}{\rho_{parallel}} 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{total}^2 }{A_{XS{parallel}}^2 }
+ (- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 \frac{1}{\rho_i}(f_i \frac{L_i}{D_i} + K_i)}}$$

Now we still don't know that the component mass flowrates are in each branch,
but we know the total flowrates, so that is good!

Let's tidy up before going further.

We make the boussinesq approximation, that for pressure loss terms, density
changes so little that it can be neglected.

So for convenience sake, we mutliply top and bottom of the square root
term by $\rho_i$ since it can be explicitly calculated.
The only thing is that the difference between $\rho_{parallel}$ and $\rho_i$
cannot be neglected


$$\dot{m}_{total} = \sum_i^n A_{XSi} 
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}_{total}^2 }{A_{XS{parallel}}^2 }
+ \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i)}}$$


I then bring the total mass flowrate out, 


$$\dot{m}_{total} = \sum_i^n A_{XSi} \dot{m}_{total}
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}^2_{total} }{A_{XS{parallel}}^2 }
+ \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2}}$$

Now divide throughout by $\dot{m}_{total}$

$$1 = \sum_i^n A_{XSi} 
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel}) 
\frac{\dot{m}^2_{total} }{A_{XS{parallel}}^2 }
+ \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2}}$$

$$1 = \sum_i^n A_{XSi} 
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 A_{XS{parallel}}^2}}$$

Now let's take the parallel areas out...

$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$

This in fact becomes our weighting factor with which to find the flowrate 
distribution amongs all the flows.

And now just a smple dimensions check... are top and bottom in units of mass
flowrate squared?

The hydrostatic term is:

$$A^2 \rho^2 gz = m^4 kg^2 m^{-6} m s^{-2} m$$
$$A^2 \rho^2 gz = kg^2   s^{-2} $$

Units look ok!


The challenge now then is to find $\rho_{parallel}$, $f_{parallel}$ and
$L_{parallel}$ as well as $\mu_{parallel}$

We have to work with this equation, which means the sum of the total
mass flowrate fractions is the sum of the individual mass flowrate
fractions through the branches.

$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$

It is evident the first term in the numerator pertains to the pressure
loss term over the whole parallel pipe system, whereas the second term
describes the extra contribution due to buoyancy forces. 

The term in the denominator represents the friction factor terms for
that singular pipe branch.

### simplification: reference point is pipe system at isothermal condition
Suppose we chose a $\rho_{parallel}$ which describes the system at 
isothermal conditions, 

$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
+ A_{XS{parallel}}^2 \rho_i(- \rho_{parallel}+\rho_i )g \Delta H_i}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$


In this case 

$$\rho_{parallel}= \rho_i (T_{ref})$$

And we are left with:
$$1 = \sum_i^n \frac{A_{XSi}}{A_{XS{parallel}} }
\sqrt{ \frac{0.5 
(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
+ K_{parallel}) \dot{m}^2_{total}
}
{ 0.5 (f_i \frac{L_i}{D_i} + K_i) \dot{m}_{total}^2 }}$$

In this regard, we have chosen a descriptive reference density which
is the density of a fluid at a chosen temperature, rather than some
weighted average of the pipe.

If there are changes to the buoyancy forces, we will then factor that
in by calculating the buoyancy term separately and without iteration.

Next we shall set some constraints which amke intuitive sense:

We shall define that the area of this parallel component is
the sum of the constituent areas, for convenience. Plus it would make intuitive
sense.

$$A_{XSparallel} = \sum_i^n A_{XSi}$$

Also, because of this, we can get a proper lengthscale for a representative
hydraulic mean diameter.

$$D_{parallel}^2 \frac{\pi}{4} = A_{XSparallel}$$


The next part would have us make some assumptions:

Firstly that the friction factor doesn't change too much under 
the influence of buoyancy forces.


Thus, hydraulic mean diameter and cross sectional area of the pipe
system are now determined.

This would leave us with:

$$
\sqrt{ \frac{1}
{ (f_{parallel} \frac{L_{parallel}}{D_{parallel}} + K_{parallel})}}
 =\frac{1}{\sum_i^n A_{XSi}} \sum_i^n A_{XSi}
\sqrt{ \frac{1}{ (f_i \frac{L_i}{D_i} + K_i)}}$$

So far, all the terms on the right hand side are known, except for the 
individual $f_i$ which are the constituent darcy friction factors.

Remember, that we have to average density, dynamic viscosity
and lengthscales. So far we already have done diameter.

So let's have a way to weight our density:

$$
\sqrt{\rho_{parallel}}
 =\frac{\sqrt{(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})}}{\sum_i^n A_{XSi}} \sum_i^n A_{XSi}
\sqrt{ \frac{ \rho_i}{ (f_i \frac{L_i}{D_i} + K_i)}}$$


We note that to help us simplify our equation, we can define weighting factors

$$w_{XSareai} = \frac{A_{XSi}}{\sum_i^n A_{XSi}} $$

With this, we can see:


$$
\sqrt{\rho_{parallel}}
 =\sqrt{(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})} \sum_i^n w_{XSareai}
\sqrt{ \frac{ \rho_i}{ (f_i \frac{L_i}{D_i} + K_i)}}$$

We see that the weighting factor for area will sum to equal 1. And it should be the case.

Now we see another weighting factor, namely the fLDK weighting factor:

$$w_{fLDKi} = \sqrt{\frac{(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})}{ (f_i \frac{L_i}{D_i} + K_i)}} $$

So that:


$$ \sqrt{\rho_{parallel}} = \sum_i^n w_{XSareai} \ w_{fLDKi} \ \sqrt{\rho_i}$$

Now, if we apply the boussinesq approximation, ie that density doesn't change
so much, we will get:


$$1 = \sum_i^n w_{XSareai} \ w_{fLDKi} $$

However, even if densities are not that constant, we would still like the
weighting factors to sum up to one any how. This is the only way it makes
physical sense.

Now with this we can start looking at how to scale viscosity and density.
$$ \sum_i^n w_{XSareai} \ w_{fLDKi}  = 1$$

Let's substitute back in:
$$w_{fLDKi} = \sqrt{\frac{(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})}{ (f_i \frac{L_i}{D_i} + K_i)}} $$
 
$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{(f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})}{ (f_i \frac{L_i}{D_i} + K_i)}}  = 1$$

$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (f_i \frac{L_i}{D_i} + K_i)}}  = (f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})^{-0.5}$$

With this we have a correlation for our respective friction factors. It appears
that they are weighted by the cross sectional areas.

$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (f_i \frac{L_i}{D_i} + K_i)}}  = (f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel})^{-0.5}$$

 Here, everything on the left hand side is assumed known except for $f_i$.
 We have not found ways to find $K_{parallel}$ just yet. Because while we
 explictly would know the mass flowrate (we are using that to solve for pressure
 drop), we have not scaled the viscosity just yet.

 But let's take a look if we make the fLDK of the parallel circuit the subject.


$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (f_i \frac{L_i}{D_i} + K_i)}} \right)^{-2}  = f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel}$$

To help us with how to handle $K_{parallel}$ and how to scale viscosity,
we first can go to the bounding case of fully laminar flow such that K is
not even important.

$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (f_i \frac{L_i}{D_i} + K_i)}} \right)^{-2}  = f_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel}$$

#### bounding cases to help us guess how to average parameters

1. Fully turbulent flow regime

Under fully turbulent flow regime

$$\frac{1}{\sqrt{f_{darcy}}} = -2 \log_{10} (\frac{\varepsilon/D}{3.7})$$

$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (k_i \frac{L_i}{D_i} + K_i)}} \right)^{-2}  = k_{parallel} \frac{L_{parallel}}{D_{parallel}} 
 + K_{parallel}$$

So here, every term on the left hand side is known. However, we still need
other equations for us to find the appropriate length scaling and appropriate 
K scaling

2. laminar flow regime

Here we consider the other case where flowrate is so small that K is
neglected.

$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (f_i \frac{L_i}{D_i} )}} \right)^{-2}  = f_{parallel} \frac{L_{parallel}}{D_{parallel}} $$


Let's consider that 

$$f_{darcy} = \frac{64}{Re} = \frac{64 A_{XS} \mu}{\dot{m} D_H}$$


$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (\frac{64 A_{XSi} \mu}{\dot{m}_i D_i} 
\frac{L_i}{D_i} )}} \right)^{-2}  
= \frac{64 A_{XS} \mu}{\dot{m} D_{parallel}} 
\frac{L_{parallel}}{D_{parallel}} $$

Let's use

$$A_{XS} = \frac{\pi}{4} D^2$$

$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{1}{ (\frac{16 \pi \mu L_i}{\dot{m}_i } 
)}} \right)^{-2}  
= \frac{16 \pi \mu L_{parallel}}{\dot{m} } 
$$

$$ \left(\sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}} \right)^{-2}  
= \frac{16 \pi \mu_{parallel} L_{parallel}}{\dot{m_{total}} } 
$$

Or so to help me see the correlations better:

$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{\dot{m_{total}}}{16 \pi \mu_{parallel} L_{parallel} } }
$$


Now we may not know the individual mass flowrates for this case, but
in stokes regime or fully laminar regime, Ohm's law actually applies
for each branch,


$$\Delta P = 0.5 \frac{1}{\rho} (f \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

When we apply darcy friction factor is 64/Re and neglect K:

$$\Delta P = 0.5 \frac{1}{\rho} (\frac{64}{Re} \frac{L}{D} )
\frac{\dot{m}^2 }{A_{XS}^2 }$$

$$\Delta P = 0.5 \frac{1}{\rho} (\frac{64 A_{XS} \mu}{\dot{m} D_H} \frac{L}{D_H})
\frac{\dot{m}^2 }{A_{XS}^2 }$$

Let's tidy up the equation first by cancelling mass flowrate and cross sectional 
area.


$$\Delta P = 0.5 \frac{1}{\rho} (\frac{64 A_{XS} \mu}{ D_H}\frac{L}{D_H})
\frac{\dot{m} }{A_{XS}^2 }$$

$$\Delta P = 0.5 \frac{1}{\rho} (\frac{64 A_{XS} \mu}{ D_H}\frac{L}{D_H})
\frac{\dot{m} }{A_{XS}^2 }$$

Substiute cross sectional area is $\frac{\pi}{4} D^2$

$$\Delta P = 0.5 \frac{1}{\rho} (16\pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$


$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$

Now we don't know that the pressure drop is at this point, but we
want to iterate it out.

However, what we do know is that the pressure drop over the whole system
is in essence equal to the pressure drop over each branch, of course 
for the sake of guessing the mass flowrate distributions to estimate
the weight of our viscosity, we will just pretend for a moment 
that buoyancy forces don't exist.

$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$

$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$

Now this is true for each and every branch, so we can substitute
this value of mass flowrate in for branch i.

However, that would cancel out any chance of trying to weight 
the viscosities for us.


$$ \sum_i^n w_{XSareai} 
\sqrt{\frac{\dot{m}_i}{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{\dot{m_{total}}}{16 \pi \mu_{parallel} L_{parallel} } }
$$

#### using Ohm's law assumption to help us weight mass flowrates to weight viscosity

Suppose we apply a small pressure drop that we have a small mass 
flowrate in each branch, we also ignore contributions of buoyancy
for now.

We would then know $m_i$ and $m_{total}$ for this mini experiment

So the pressure losses across these series of pipes becomes:

$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$

And since the pressure drops across each branch is the same, and Ohm's
law is obeyed, the ratios of mass flowrates in each of the branches 
will be equal

$$\Delta P = \frac{1}{\rho} (8 \pi \mu L)
\frac{\dot{m} }{A_{XS}^2 }$$
So the mass distributions of the 

$$\Delta P = \frac{1}{\rho_i} (8 \pi \mu L)_i
\frac{\dot{m}_i }{A_{XSi}^2 }
=\frac{1}{\rho_{parallel}} (8 \pi \mu L)_{parallel}
\frac{\dot{m}_{parallel} }{A_{XSparallel}^2 }
$$

Removing pressure from the equation, density (via boussinesq 
approximation)
and the factor of 
$8\pi$

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

Let's condense the area ratios

$$ \sum_i^n w_{XSareai}^2 
\sqrt{\frac{\frac{(\mu L)_{parallel}}{(\mu L )_i} }{ 16 \pi \mu_i L_i}}   
= \sqrt{\frac{1}{16 \pi \mu_{parallel} L_{parallel} } }
$$

And now the viscosities and lengthscales


$$ \sum_i^n w_{XSareai}^2 \frac{1}{\mu_i L_i}
\sqrt{\frac{1}
{ 16 \pi }}   
= \sqrt{\frac{1}{16 \pi  } }
\frac{1}{\mu_{parallel} L_{parallel}}
$$


And removing the 16 $\pi$ term:
$$ \sum_i^n w_{XSareai}^2 \frac{1}{\mu_i L_i} = 
\frac{1}{\mu_{parallel} L_{parallel}}
$$

And removing the 16 $\pi$ term:
$$ \sum_i^n w_{XSareai}^2 \frac{1}{\mu_i } \frac{1}{L_i} = 
\frac{1}{\mu_{parallel} L_{parallel}}
$$

We thus come up with a very simple way of weighting our lengths
and viscosities. 

We can view the lengthscales and area weighting ratios as sort
of using system dimensions to help in weighting our average viscosity:

And removing the 16 $\pi$ term:
$$ \sum_i^n  \frac{w_{XSareai}^2}{L_i} \frac{1}{\mu_i }  = 
\frac{1}{\mu_{parallel} L_{parallel}}
$$

So if we were to intrepret this as such, then we will have to 
normalise our weighting factors like so:

$$\sum_i^n  \frac{w_{XSareai}^2}{L_i}   = 
\frac{1}{ L_{parallel}}
$$

In this regard, we have found our weighted average of Length

$$ L_{parallel} =    
\frac{1}{\sum_i^n  \frac{w_{XSareai}^2}{L_i} }
$$

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


#### using a hydrostatic force balance to weight density 
So now we have weighting methods for our average viscosity, 
hydraulic diameter and density. Or at least some methods to reference
the density.


While our density was chosen at some arbitrary isothermal reference 
temperature, it may not make sense if we were to set it at a fixed 
value for our representative fLDK component.




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

### estimation methods for Be

upon writing the code, i know i have a simple way for finding evenly spaced
Reynold's number points. However, i don't know a simple way for finding evenly
spaced Bejan number points... 

Bejan number is usually the dependent variable, not independent variable. So
there aren't usually Bejan number ranges i can think off the top of my head.

However, i know for pipes, there are bejan numbers corresponding to Reynold's
numbers. And i can assume the system behaves like a pipe in terms of distribution.

I will use the log spaced Reynold's numbers to guess the corresponding Bejan
numbers, and will use the Bejan numbers as guess points to get the corresponding
Reynold's numbers.

```csharp


for (int i = 0; i < 500; i++)
{
	// first i decide on a number of values to give
	double ReLogSpacing = 0.02;
	double ReGuessValue = Math.Pow(10,ReLogSpacing * i);

	// once we have a suitable Re, we need to get a Be
	// value,
	// so we convert Re into mass flowrate
	//
	// Now unfortunately, for the first round of iteration,
	// we cannot guess Re from Be non iteratively.
	// We have to guess Be first, and then from that get Re.
	//
	// Problem is i don't know what a typical Be range should be!
	// Nor where the transtion region should be for any typical
	// graph.
	//
	// Well one method we can try is this:
	//
	// we have a guess Reynold's number value, which we then
	// feed into a pipe equation like churchill
	// from that, we can guess a Bejan number to go with
	// And from this Bejan number, we can guess the actual
	// Reynold's number value

	IFrictionFactor _frictionFactorObj = 
		new ChurchillFrictionFactor();

	// so bejan number is:
	// Be = 0.5 *Re^2 (f L/D  + K)
	//
	// Now herein lines a problem,
	// if we were to  use this method to guess,
	// we need a proper L/D ratio too.
	// To keep it once more in a similar order of
	// magnitude, i would rather have L be the average
	// of both branch lengths
	//
	// I assume the cubic spline would take care of 
	// any variation due to K, what i'm watching out for
	// so i assume K = 0
	//
	// more importantly is modelling the transition region
	// or interpolating it with sufficient points
	// to get L/D ratio, we need the average branch lengths
	// and average hydraulic diameters

	double lengthToDiameter = this.getComponentLength().
		As(LengthUnit.Meter) / 
		this.getHydraulicDiameter().
		As(LengthUnit.Meter);

	// my roughness ratio here is guessed based on 
	// assuming cast iron pipes, just a guestimation
	// so not so important

	Length absoluteRoughness = new Length(
			0.15, LengthUnit.Millimeter);


	double roughnessRatio = absoluteRoughness.As(LengthUnit.Meter)/ 
		this.getHydraulicDiameter().As(LengthUnit.Meter);


	double darcyFrictionFactor = _frictionFactorObj.
		darcy(ReGuessValue, roughnessRatio);

	// i shall now shove these values in to obtain my Bejan number
	// Be_d = 0.5*Re(guess)^2 *f * L/D
	double bejanNumber = 0.5 * 
		Math.Pow(ReGuessValue,2.0) *
		lengthToDiameter *
		darcyFrictionFactor;
	// once we have this we can add the bejan number
	BeValues.Add(bejanNumber);

}
```

So this is how i will gues bejan number.

### Guessing Pressure Drop from Bejan number (parallel case)

Now we have a bejan number, we will need to apply a pressure drop across
this parallel array. So we will need an average kinematic viscosity so to speak
from this array of pipes and components.

If the pipes were isothermal, great, we only have one kinematic viscosity
with which to weight our Bejan numbers.

$$Be_D = \frac{\Delta p D^2}{\nu^2}$$

To understand how we got here, we should visit the underlying equations
This is for the pipe:

$$Be_D = 0.5 (\frac{L}{D} f_{darcy} + K) Re^2$$

$$Be_D = 0.5 (\frac{L}{D} f_{darcy} + K) Re^2$$

$$ \frac{\Delta p D^2}{\nu^2} = 0.5 (\frac{L}{D} f_{darcy} + K) Re^2$$

$$ \frac{\Delta p D^2}{\nu^2} = 0.5 (\frac{L}{D} f_{darcy} + K} 
\frac{\dot{m}^2}{A_{xs}^2} \frac{D_H^2}{\mu^2}$$

$$ \frac{\Delta p }{\nu^2} = 0.5 (\frac{L}{D} f_{darcy} + K} 
\frac{\dot{m}^2}{A_{xs}^2} \frac{1}{\mu^2}$$

$$ \frac{\Delta p }{1} = 0.5 (\frac{L}{D} f_{darcy} + K} 
\frac{\dot{m}^2}{A_{xs}^2} \frac{1}{\rho^2}$$

For this case, we can see when it comes to viscosity, as long as the terms
can cancel out like this, it doesn't matter.

If the components were in series, we would simply sum up pressre drops across
all of them. However, we cannot since they are in parallel.

Instead, a single bejan number would give rise to multiple mass flowrates.

Now suppose we have two fLDK type components in parallel, which follow this
correlation:

$$Be_D = k_1 Re + k_2 Re^2$$
$$k_1 Re^2 + k_2  Re - Be_D = 0$$

If we were to use the quadratic formula

$$Re = \frac{1}{2k_1} (-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})$$

Since we expect Re to be positive (mostly), or rather, take the same sign
as $Be_D$, that when $Be_D$ is positive, Re is positive, and vice versa,
we shall just assume both of them are positive.

$$Re = \frac{1}{2k_1} (-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})$$

Now we see in this case, Re scales as the square root of (Be_D), at least
in terms of units, that is the case. For simplicity's sake, we shall use
this assumption to find a suitable average viscosity for parallel setup.


$$\frac{\dot{m}}{A_{xs}}  \frac{D_H}{\mu}
= \frac{1}{2k_1} (-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})$$


$$\dot{m} = \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})$$

As a sanity check, we can note that when Be=0, $-k_2+ \sqrt{k_2^2} = 0$ and thus
the mass flowrate should be zero at zero pressure drop. This is the correct
expression.

Now this is for a single pipe so to speak. For two identical pipes, we can
express the total mass flow as:

$$\dot{m} = \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})
+ \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})
$$

Assuming they are at different temperature due to the different viscosities,
we can assign $\mu_1$ and $\mu_2$ to describe the differing viscosities of each
pipe.

Note that this equation is simply a mass balance type equation across
the parallel branches. Ie, total mass flowrate across the parallel branches
is the sum of flow between each branch.


$$\dot{m} = \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})
+ \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})
$$

in terms of units, so we can cancel the dimensionless terms out,

$$\dot{m} ~ \frac{A_{xs}\mu}{D_H} \sqrt{Be_D})
+ \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(\sqrt{Be_D})
$$

We already decided that for the area scaling, the sum of areas will be the
representative cross sectional area of the parallel setup, and the 
ensemble average hydraulic diameter will be the representative hydraulic diameter
of the mass flowrates. Then perhaps to keep Re in the same order of magnitude,
as individual pipe Re, i will use the ensemble average.



$$ \frac{A_{xsTotal} \mu_{avg}}{D_{Havg}} \sqrt{Be_avg}
~ \frac{A_{xs}\mu}{D_H} \sqrt{Be_D}
+ \frac{A_{xs}\mu}{D_H} \sqrt{Be_D}
$$

Note here that as we bring out the kinematic viscosity terms $Be_D$


$$ \frac{A_{xsTotal} \mu_{avg}}{D_{Havg}} \sqrt{Be_avg}
~ \frac{A_{xs}\rho}{1} \sqrt{\Delta p}
+ \frac{A_{xs}\rho}{1} \sqrt{\Delta p}
$$

It seems that, if we just use simple ensemble average for kinematic
viscosity, that would work since most of the terms cancel out anyhow.

$$ \frac{A_{xsTotal} \rho_{avg}}{1} \sqrt{\Delta p}
~ \frac{A_{xs}\rho1}{1} \sqrt{\Delta p}
+ \frac{A_{xs}\rho2j}{1} \sqrt{\Delta p}
$$

Now for parallel setups, i believe the dynamic pressures across each branch
will be the same

$$ A_{xsTotal} \sqrt{\rho_{avg}} \sqrt{\Delta P}
~ A_{xs}\sqrt{\rho1} \sqrt{\Delta P}
+ A_{xs}\sqrt{\rho2} \sqrt{\Delta P}
$$

For this to hold, such that the Be and Re numbers are of a similar order of 
magnitude, we have this such equation for parallel setup:

$$ A_{xsTotal} \sqrt{\rho_{avg}}
= A_{xs1}\sqrt{\rho1} 
+ A_{xs2}\sqrt{\rho2} 
$$

$$  \sqrt{\rho_{avg}}
= \frac{1}{A_{xsTotal}} (A_{xs1}\sqrt{\rho1} 
+ A_{xs2}\sqrt{\rho2} )
$$

we have just found a way of weighting our average density. At least for
the more turbulent type regime. For laminar regimes, the square root
kind of disappears, so that we have a density weighted by areas.

Either way, we shouldn't be that far off.



Square the result and we should get our average density.

This will keep the average Re and hopefully Be as well to be in the same
order of magnitude.

With these scaling parameters, we should be able to get our pressure drops pretty
decently well. 


The area of the parallel setup is the sum of areas of each branch,
the mass flowrate is the sum  of mass flowrates of each branch,
hydraulic diameter is the ensemble average of hydraulic diameter of each branch,
kinematic viscosity is the ensemble average of kinematic viscosity of each branch,
finally, the density is weighted by area of each respective branch.

Therefore we need a way to return the areas, and densities of each pipe branch
or each fluid entity. So that the averaging can occur.

Also kinematic viscosity too.

I'll probably want to do a series of tests to confirm if the paralleSubCircuit
is obtaining the quantities as shown above. 

okay tests complete, here's one such example:


```csharp
[Theory]
[InlineData(0.5,0.1,0.3, 1.5,1.0,0.5)]
[InlineData(0.4,0.2,0.3, 0.9,0.88,1.3)]
public void WhenParallelSubCkt_getDensity_expectAreaWeightedSum(
		double diameter1,
		double diameter2,
		double diameter3,
		double kinVis1,
		double kinVis2,
		double kinVis3){


	// Setup
	//
	// First let's get the lengths and kinematic Viscosities
	//
	// Because my testpipe obtains density by finding ratios of 
	// kinematicViscosity to dynamic Viscosity
	// i will just randomly switch up the kinematic viscosity up

	Length hydraulicDiameter1 = new Length(diameter1, LengthUnit.Meter);
	Length hydraulicDiameter2 = new Length(diameter2, LengthUnit.Meter);
	Length hydraulicDiameter3 = new Length(diameter3, LengthUnit.Meter);

	KinematicViscosity kinViscosity1 = 
		new KinematicViscosity(kinVis1, 
				KinematicViscosityUnit.Centistokes);
	KinematicViscosity kinViscosity2 = 
		new KinematicViscosity(kinVis2, 
				KinematicViscosityUnit.Centistokes);
	KinematicViscosity kinViscosity3 = 
		new KinematicViscosity(kinVis3, 
				KinematicViscosityUnit.Centistokes);


	// next let's setup the pipes

	Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
	IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
	testPipe.Parameters.hydraulicDiameter = hydraulicDiameter1;
	testPipe.Parameters.fluidKinViscosity = kinViscosity1;
	testPipe.Connect("parallelIn","parallelOut");

	preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
	IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
	testPipe2.Parameters.hydraulicDiameter = hydraulicDiameter2;
	testPipe2.Parameters.fluidKinViscosity = kinViscosity2;
	testPipe2.Connect("parallelIn","parallelOut");

	preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
	IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
	testPipe3.Parameters.hydraulicDiameter = hydraulicDiameter3;
	testPipe3.Parameters.fluidKinViscosity = kinViscosity3;
	testPipe3.Connect("parallelIn","parallelOut");

	// let me get my total area from these pipes
	Area expectedTotalArea =
		(testPipe.getXSArea() + 
		 testPipe2.getXSArea() + 
		 testPipe3.getXSArea());

	// the above area will help me weight my densities
	// now let's get the expected density
	// were
	// sqrt(rho_avg) = 1/totalArea * (area1*sqrt(rho1) +
	// area2*sqrt(rho2) + area3*sqrt(rho3))

	Density _expectedAverageDensity;

	{
		double sqrtDensity;
		sqrtDensity = 
			testPipe.getXSArea().As(AreaUnit.SI) *
			Math.Pow(testPipe.getFluidDensity().As(DensityUnit.SI),0.5) +
			testPipe2.getXSArea().As(AreaUnit.SI) *
			Math.Pow(testPipe2.getFluidDensity().As(DensityUnit.SI),0.5) +
			testPipe3.getXSArea().As(AreaUnit.SI) *
			Math.Pow(testPipe3.getFluidDensity().As(DensityUnit.SI),0.5);

		sqrtDensity /= expectedTotalArea.As(AreaUnit.SI);
		double densityValue = 
			Math.Pow(sqrtDensity,2.0);

		_expectedAverageDensity = new Density(
				densityValue, DensityUnit.KilogramPerCubicMeter);
	}



	var subckt = new SubcircuitDefinition(new Circuit(
				testPipe,
				testPipe2,
				testPipe3),
			"parallelIn", "parallelOut");

	FluidParallelSubCircuit fluidSubCkt
		= new FluidParallelSubCircuit("X1", subckt);
	fluidSubCkt.Connect("out" , "0");

	//Act
	//
	Density _actualAverageDensity = 
		fluidSubCkt.getFluidDensity();


	// Assert

	Assert.Equal(_expectedAverageDensity.As(DensityUnit.SI),
			_actualAverageDensity.As(DensityUnit.SI),1
			);

}

```
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


### Hybrid Bejan number and buoyancy correction method

The trouble with the Bejan number method is that it becomes very 
confusing as to how to weigh density, friction factors and lengthscales.

When buoyancy forces come in, the equation becomes very messy.

The other way is to simply iterate our way through, seeing that our
setup is parallel. 

Problem is, with nested parallel loops, this will take a long time.

#### An initial guess for iteration

There are several ways to speed this up given that the pressure change
(not form losses) across each parallel branch are equal.

Suppose i have the $\dot{m}_{total}$ mass flowrate through the whole
pipe. Now i force it through onemass any one branch of the pipe.

I will then get an appropriate pressure drop pretty quickly.

I then apply this pressure drop across the whole parallel setup to
got a guess $\dot{m}_{totalGuess}$ guessed mass flowrate. Of course,
the guessed mass flowrate will be several times bigger than
the set total mass flowrate.

Since this is so, i have a ratio of the guessed mass flowrate to the
total mass flowrate. I will take the branch mass flowrate in the initial
pipe to be the ratio of the total mass flowrate to this initial guess
mass flowrate.

I will then obtain a new pressure change, and apply this uniformly 
across the entire pipe system. I then iterate until a set tolerance,
perhaps 1 or 2\% will suffice. The key here is speed. As the simulation
is planned to happen in almost realtime, but accuracy needs to be
sufficient but perhaps within instrument error (one standard deviation
of the flowmeter reading).


If we just leave things at one iteration, i can get an estimate of
a pressure drop very quickly. Neverthless, the relative flow 
ratios of the mass flowrates for the pipe may not apply 
at both of these flowrates.

Since that is the case, it may be better to have another way of 
guessing this initial mass flowrate.


Suppose again that i have a set mass flowrate $\dot{m}_{total}$.

I can map out the
pressure losses using the bejan number method described before and
completely ignore buoyancy forces. I would then have a set pressure
drop unaffected by buoyancy forces.

I can estimate the pressure change by adding this pressure drop to
the mean hydrostatic pressure across the pipes. 

$$\Delta P_{change} = \Delta P_{loss} + h\rho g$$


And again i take one
branch as a reference branch, and I check the mass flowrate 
$\dot{m}_{branch}$

Now i apply this pressure change $\Delta P_{change}$ to 
all branches of the pipe WITH
buoyancy forces. I would then get an iterated total mass flowrate.
$\dot{m}_{totalGuess}$
This value may be more or less than the total mass flowrate.

Suppose that natural convection is aiding flow for all branches, 
for a set pressure change across the branch, the branch flowrate with
natural convection will be more than branch flowrate neglecting 
natural convection so that:


$$\dot{m}_{totalGuess} > \dot{m}_{total}$$

In this setup we must then reduce the branch mass flowrate slightly
to correct for the natural convection aiding flow

What i then do is to correct the branch mass flowrate

$$\dot{m}_{branchCorrected} = \dot{m}_{branch} 
\frac{\dot{m}_{total}}{\dot{m}_{totalGuess}}$$

We would then get buoyancy corrected branch flowrate for this branch
and we can then estimate the pressure change across the branch. And
obtain a guessed total mass flowrate, which should me much closer 
to the set total mass flowrate.

We cna then return the pressure change to the environment, and use the
pre-exising derivaitons of fLDK formulas to calculate the net 
pressure drop. And then we subtract this pressure drop from the 
pressure change to obtain a net hydrostatic pressure increase or 
decrease.


This is one iteration. Here, so long as the forced convection is much
greater than natural convection, the ratios of the mass flowrate 
in the branch should. Otherwise we can keep iterating until we stop.


### what if natural convection dominates the flow?
For zero total mass flowrate however, we won't be doing good with this
method since $\dot{m}_{total} = 0$ and there is no means for us to
guess mass flowrate.

In this regard, we should set the pressure change in each branch as
the hydrostatic pressure drop across the whole parallel pipe system.

In general, there would be branches with posistive and negative flows,
they should all sum to zero however.

But that's another problem for another time.











## Nested tests with FluidSeriesCircuits

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






