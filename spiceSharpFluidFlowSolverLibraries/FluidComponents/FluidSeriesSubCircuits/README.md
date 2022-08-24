# FluidSeriesSubCircuits


## Goals

The goal of the fluidSeriesSubCircuit is mainly to contain a collection
of fluid entities or components in series.

The main quantities it should supply are:

1. Pressure losses given a mass flowrate
2. mass flowrate given a fixed pressure loss term
3. source terms eg. pumps
4. buoyancy terms
5. height change terms or vectors

The main reason is so that this can be used inside a parallel circuit
or subcircuit.

We will want to calculate mass flowrate across each branch or each
fluidSeriesSubCircuit given a pressure change across it.

The pressure change will be equal to the sum of contributions by pressure
loss terms, hydrostatic pressure terms, and source terms.

$$\Delta P_{changeTotal} = -\Delta P_{loss} + \rho_{series} g \Delta H
+\sum_i^n P_{source}$$

## Nested iteration issue

Now specifically when we request mass flowrate given a pressure change 
term, we normally use implicit methods such as bisection or newton 
raphson in order to obtain the mass flowrate.

This isn't usually a problem except that when we have parallel 
subcircuits within this system, and one requests a pressure loss term
given a mass flowrate, that too is implicit. And nested iterations
are extremely costly. Even a simple three pipe setup took one minute
to solve for the flow distribution because of this issue.

### Time Requirements

Now we would like the circuit to solve within 0.1s or less. This is
the bare minimum should we want to provide fresh values for the control
system every 0.1s.

A more important time requirement is also needed however, it is the 
Courant number.

In computational fluid dynamics, a [Courant number](https://www.simscale.com/knowledge-base/what-is-a-courant-number/)
below 1 is necessary
for numerical stability and convergence. We should also follow this
criteria even for nodalised calculation involving heat transfer.

Therefore we may wish to do simple calculations to obtain the largest
timestep possible to achieve Co below 1, ideally 0.7 and below.

The formula is as follows:

$$Co = u \frac{\Delta t}{\Delta x}$$

We can calculate velocity using mass flowrate of 0.18 kg/s (typical
experimental flowrate in CIET), and use the smallest area in CIET,
about 3.64e-4 $m^2$, and the lowest density possible for Therminol VP1,
950 $kg/m^3$ at about 155 &deg;C. Then we multiply that by a safety
factor of about 2.

$$u = \frac{0.18 kg/s}{950 kg/m^3 3.64e-4} *2 \approx 1.05 m/s$$

The smallest segment length in the CTAH and Heater branch is about 
0.1526 m.

If we were to use a timestep of 0.1s, we could attain:

$$Co = 1.05 \frac{0.1}{0.1526} \approx 0.688$$


Thus, it is absolutely crucial for us to maintain this courant number 
for forced circulation. By having minimum segment length 
$\approx 0.1526 m$. Should we increase minimum segment length to 0.15m,
we can get:


$$Co = 1.05 \frac{0.1}{0.15} \approx 0.7$$

This is well below 1.

Other notable regions of concern for CIET are the heater top head 
where segment length is 0.0891 m. Flow area here is also 3.64e-4:

$$u = \frac{0.18 kg/s}{950 kg/m^3 3.64e-4} *2 \approx 1.05  m/s$$
$$Co = 1.05 m/s * \frac{0.1}{0.0889} = 0.697 \approx 1.18$$

Now of course, this is here because i introduced a safety factor of 2.
If i reduce that same safety factor to 1.5:


$$Co = 1.05 m/s * \frac{0.1}{0.0889} *1.5/2 = 0.878 $$

All this means is that the maximum mass flowrate of the simulation has
a safe upper bound of about 0.27 kg/s (1.5 times 0.18 kg/s).
As long as segment lengths are at minimum 0.0889m and timestep is about
0.1 s.



All in all, it seems that meeting the timestep criteria of 0.1s is
not just important for ensuring enough data flows from the simulation
into the control system, but also it ensures that the simulation is
numerically stable within the range of mass flowrates prescribed at
these segment lengths of 0.0889m at flowrate of 0.27kg/s.

Therefore it's absolutely crucial for the timestep to be at 0.1s and not
any more. This means that the calculations must be done well below this
time threshold in order to have numerical stability for this system.



## Precalculation Strategy

To combat this issue, I wanted to have a nondimensionalised function
for each pipe or component, so that i could request a mass flowrate
given a pressure loss over each pipe term.

$$Be_D = 0.5 Re^2 (f_{darcy}\frac{L}{D} +K)$$

The values for Be and Re are calculated beforehand, and then interpolated
during the simulation. So that during the simulation, minimal iterations
are required.

The nondimensionalised form is advantageous because the temperature
dependence of the loss terms is contained within Be, Re and the
darcy friction factor which is also depdendent on Re. Thus if 
temperature changes, one can simply adjust Re, and use that
Re to obtain a new Be and therefore pressure loss term.

I wish to apply this to the FluidSeriesSubCircuit as a whole also
and assume that the pressure losses across it can be captured
in a correlation which is quite invariant with temperature.

$$Be_{Dseries} = 0.5 Re_{series}^2 (f_{darcySeries}
\frac{L_{series}}{D_{series}} +K_{series})$$

This would require us to judiciously find values to nondimensionalise
the flow well.

## Summary of how to find characteristic quantities for series pipes

For the nondimensionalised precalculation strategy, we wish to plot:

$$Be_D = \frac{\Delta P D_H^2}{\nu \mu}$$

$$Re = \frac{\dot{m} D_H}{A_{XS} \mu}$$


### 1. Density

Assumption: Boussinesq approximation, density averaged by summing
buoyancy forces.

$$\Delta H_{series} \rho_{series}  = \sum_i^n \Delta H_i \rho_i $$

$$\Delta H_{series}   = \sum_i^n \Delta H_i $$

### 2. Cross sectional area and Hydraulic mean diameter

Assumptions: fully turbulent flow bounding case, density calculated
using boussinesq approximation

$$ \frac{\sum_i^n (k_{darcyi} \frac{L_i}{D_i} + K_i)}
{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{k_{darcyi} \frac{L_i}{D_i} + K_i}{\rho_i A_{XSi}^2} 
   $$
Where:
$$\frac{1}{\sqrt{k_{darcyi}}} = 
-2 \log_{10} (\frac{\varepsilon_i/D_i}{3.7})$$

$$A_{XSSeries} = \frac{\pi}{4}D_{series}^2$$

Note: for asymmetric components, we can use average of entrance and 
exit cross sectional areas so that correlation is ambivalent to flow
direction.

### 3. Dynamic Viscosity

Assumptions: Stokes regime bounding case, density calculated using
boussinesq approximation, area weighted according to fully turbulent
flow assumption

$$\frac{\mu_{series} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\mu_i L_i }{\rho_i A_{XSi}^2} 
   $$
Where:
$$L_{series} = \sum_i^n L_i$$

## Derivations for finding a nondimensionalised correlation which holds at different temperature distribution

### requirements:
So we begin by looking at the terms Be and Re to see which terms
are required to be nondimensionalised.

$$Be_D = \frac{\Delta P D_H^2}{\nu \mu}$$

$$Re = \frac{\dot{m} D_H}{A_{XS} \mu}$$

From these, we need effective ways of finding system viscosity $\mu$,
density $\rho$, cross sectional area $A_{XS}$ and hydraulic diameter
$D_H$ at the very minimum.

We also need to define system pressure losses and mass flowrates as well.

#### 1. Mass flowrates

For mass flowrate, it is straightforward for series circuits, the
mass flowrate through all components is equal.

$$\dot{m}_i = \dot{m}_{total}$$

#### 2. Pressure Loss terms

For pressure losses, the terms are simply a summation of all the
pressure losses accumulated in the fluid series circuit
$$\Delta P_{totalLoss} = \sum_i^n \Delta P_{loss-i}$$

#### 3. Density

The case I'm dealing with is mostly incompressible liquid flow,
so that density doesn't change very much. So i will apply the
boussinesq approximation and weigh density by the following:


$$\Delta H_{series} \rho_{series} g = \sum_i^n \Delta H_i \rho_i g$$

$$\Delta H_{series} \rho_{series}  = \sum_i^n \Delta H_i \rho_i $$

The height contributions are as follows:


$$\Delta H_{series}   = \sum_i^n \Delta H_i $$

So in essence, the density of the system will be averaged by height 
changes.


####  Viscosity, Length, Cross Sectional Area and Diameter

Averaging viscosity in such a way that the system Reynold's number
produces an accurate estimate of the pressure loss term will require
us to look at some fundamental equations.

For a series of fluid components,
We shall assume each fluid components follow this equation:

$$Be_D = 0.5 (f \frac{L}{D} + K) Re^2$$


So we can find that after redimnesionalising:


$$\frac{\Delta P D_H^2}{\nu \mu}= 0.5 (f \frac{L}{D} + K) Re^2$$




$$\Delta P = 0.5 \frac{\nu^2 \rho}{D_H^2} (f \frac{L}{D} + K)
\frac{\dot{m}^2 D_H^2}{A_{XS}^2 \mu^2}$$

Let's cancel out the diameters and viscosities:


$$\Delta P = 0.5 \frac{1}{\rho} (f_{darcy} \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

From this correlation, we can apply the fact that the pressure loss
of the entire system is equal to pressure losses of each component:

Suppose now i substitute these back into our correlation for series of two pipes:

$$\Delta P_{total} = \Delta P_1 + \Delta P_2$$

$$\Delta P_{total} = 0.5 \frac{1}{\rho_1} (f_{darcy1} \frac{L_1}{D_1} + K_1)
\frac{\dot{m_1}^2 }{A_{XS1}^2 } + 0.5 \frac{1}{\rho_2} (f_{darcy2} 
\frac{L_2}{D_2} + K_2) \frac{\dot{m_2}^2 }{A_{XS2}^2 }$$

$$\Delta P_{totalLoss} = \sum_i^n 
0.5 \frac{1}{\rho_i} (f_{darcyi} \frac{L_i}{D_i} + K_i)
\frac{\dot{m_i}^2 }{A_{XSi}^2 }$$


Now suppose that the series 
of pipes can be represented by one equivalent long component.

$$\Delta P_{totalLoss} =  
0.5 \frac{1}{\rho_{series}} (f_{darcySeries} 
\frac{L_{series}}{D_{series}} + K_{series})
\frac{\dot{m}_{series}^2 }{A_{XS{series}}^2 }$$


So now if we substitute this in, we get one long equation:

$$0.5 \frac{1}{\rho_{series}} (f_{darcySeries} 
\frac{L_{series}}{D_{series}} + K_{series})
\frac{\dot{m}_{series}^2 }{A_{XS{series}}^2 } = \sum_i^n 
0.5 \frac{1}{\rho_i} (f_{darcyi} \frac{L_i}{D_i} + K_i)
\frac{\dot{m_i}^2 }{A_{XSi}^2 }$$




##### Dimensionless Pressure Drop Correlation for components in Series

Let's now do some tidying up noting that mass flowrate is constant in
the series of pipes:

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{f_{darcyi} \frac{L_i}{D_i} + K_i}{\rho_i A_{XSi}^2} 
   $$

This will become our fundamental correlation as to how to find average
viscosity, cross sectional area and hydraulic diameter.

However, there are other quantities there that must be defined
in order to know how to find suitable averages of $\mu$, $A_{XS}$
and $D_H$.

For length, it just makes intutitive sense that the system length
should be the sum of all the constituent pipe lengths

$$L_{series} = \sum_i^n L_i$$

However, for other cases, such as form losses, it isn't as 
straightforward as addition.


However, cross sectional area and hydraulic mean diameter can
be correlated by the following:


$$A_{XS} = \frac{\pi}{4} D_H^2$$

For averaging viscosity, and other factors, we can consider the 
following bounding cases:

1. Stokes regime 
2. fully turbulent

##### Stokes Regime Assumption (Creeping flow)

This is the extreme case where Re is so low we can ignore the effects
of K. Thus viscous effects are dominant here. It should make sense
that viscosity should be weighted such that flow losses are
to be accurate in this bounding case.

So we have this equation:
$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{f_{darcyi} \frac{L_i}{D_i} + K_i}{\rho_i A_{XSi}^2} 
   $$

And we can use the stokes regime formula for darcy friction factor:


$$f_{darcy} = \frac{64}{Re}$$

$$f_{darcy} = \frac{64 A_{XS}\mu}{\dot{m} D_H}$$

We also have

$$f_{darcyi} \frac{L_i}{D_i} + K_i \approx
f_{darcyi} \frac{L_i}{D_i} $$
$$f_{darcy{series}} \frac{L_{series}}{D_{series}} + K_{series} \approx
f_{darcy{series}} \frac{L_{series}}{D_{series}} $$

we can substitute this in here:

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} 
}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{f_{darcyi} \frac{L_i}{D_i} }{\rho_i A_{XSi}^2} 
   $$


$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} 
}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{\frac{64 A_{XSi}\mu_i}{\dot{m}_i D_i} \frac{L_i}{D_i} }{\rho_i A_{XSi}^2} 
   $$

We use to cancel out the diameters:

$$A_{XSi} = \frac{\pi D_i^2}{4}$$

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} 
}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{\frac{64 A_{XSi}\mu_i}{\dot{m}_i D_i} \frac{L_i}{D_i} }{\rho_i A_{XSi}^2} 
   $$

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} 
}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{\frac{16\mu_i}{\dot{m}_i \pi} L_i }{\rho_i A_{XSi}^2} 
   $$


Doing the same thing for the series circuit:
$$\frac{\frac{16\mu_{series}}
{\dot{m}_{series} \pi} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\frac{16\mu_i}{\dot{m}_i \pi} L_i }{\rho_i A_{XSi}^2} 
   $$

We can cancel out the mass flowrates and constants to obtain:

$$\frac{\mu_{series} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\mu_i L_i }{\rho_i A_{XSi}^2} 
   $$

$$\frac{\mu_{series} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\mu_i L_i }{\rho_i A_{XSi}^2} 
   $$

Thus, we can find a way to weight viscosity using cross sectional
area, density, and characteristic component lengthscales.

$$\frac{\mu_{series} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\mu_i L_i }{\rho_i A_{XSi}^2} 
   $$

Unfortunately, we still don't have a way to find a weighted
average of cross sectional areas; we need another correlation.


##### turbulent flow restrictions in finding series parameters

Here we shall go to our other bounding case: full turbulence.


For fully turbulent flow, $f_{darcy} =  k_{darcy}$


From the [colebrook correlation](https://www.sciencedirect.com/book/9781856178303/transmission-pipeline-calculations-and-simulations-manual):


For fully turbulent flow through rough pipes:
$$\frac{1}{\sqrt{k_{darcyi}}} = 
-2 \log_{10} (\frac{\varepsilon_i/D_i}{3.7})$$


$$ \frac{k_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{k_{darcyi} \frac{L_i}{D_i} + K_i}{\rho_i A_{XSi}^2} 
   $$

In this equation, all terms on the RHS are assumed to be known. Whereas
the methods of averaging $k_{darcySeries}$, $K_{series}$ and 
$D_{series}$ are unknowns. 

$$A_{XSSeries} = \frac{\pi}{4}D_{series}^2$$

We shall require two other equations in 
order to fully specify our system.

We shall take them from intuition, in the sense that $A_{XSSeries}$ is
some weighted average of the areas. And form losses should of course
stack additively.

Knowing that, we can just define this constraint which suits our
intuition:


$$\sum_i^n (k_{darcyi} \frac{L_i}{D_i} + K_i) =
k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series}
$$

Knowing this, we can simply substitute out two of our current unknowns:

###### Weighting Cross Sectional Area:
$$ \frac{\sum_i^n (k_{darcyi} \frac{L_i}{D_i} + K_i)}
{\rho_{series}A_{XSseries}^2 } 
= \sum_i^n  \frac{k_{darcyi} \frac{L_i}{D_i} + K_i}{\rho_i A_{XSi}^2} 
   $$

In this way, we find that the areas are a weighted average
such that it will yield the correct loss coefficients in a fully
turbulent case.

Hydraulic diameter is to be found using:

###### Finding Hydraulic Mean Diameter
$$A_{XSSeries} = \frac{\pi}{4}D_{series}^2$$


With this cross sectional area, we can effectively weight our
viscosity so that it will yield the correct loss terms in stokes
regimes.

###### Weighting Viscosity:
$$\frac{\mu_{series} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\mu_i L_i }{\rho_i A_{XSi}^2} 
   $$

### Checking Boussinesq Approximation for Therminol from 20-120C

So far we have found out suitable ways of finding a representative
dynamic viscosity $\mu_{series}$ for a series system of pipes
and components and also a way to scale the hydraulic diameter.

This will ensure that under temperature changes, friction factor changes
for laminar and fully turbulent profiles are taken care of; no need to
recalculate the dimensionless correlation between Re and Be.

Now we should note that we need not spend too much effort here 
trying to find the appropriate ways to weight out density
since the density of the fluid doesn't change by more than 10% from 20C
to 120C, this is dowtherm i'm talking about.

Liquid phase flows in general do not change density too much. So
no matter how accurately i weigh the average densities, it won't yield
as much bang for buck as when i spend effort elsewhere.

## Code Validation Tests

To ensure this fluidSeriesSubCircuit is working correctly, I shall begin
by first designing a therminol based pipe. And test those things out.

Assuming that is done, I want to have a few validation cases:


1. Isothermal pressure loss validation test (is precalculation working
correctly?)
2. Isothermal pressure loss time tests, for a subcircuit of 20 components
my time per calculation for mass flow from pressure loss should be less
than 5 ms for the timestep calculation
3. Changing Temperature tests, after temperature changing perhaps 10 out
of 20 components within the temperature range, does my pressure loss term
equal that of the pressure loss term when found iteratively? At least within 1% error.
4. For a series of pipes check if the correct height change is supplied
5. For a series of pipes, check if the correct hydrostatic pressure is 
supplied

### Code basic function tests

At the function levels, i need to make sure that my code is returning
the correct averaged quantities for a series of non uniform temperature
pipes.

1. Density tests
2. cross section area tests
3. Hydraulic diameter tests
4. fLDK summation tests
5. nondimensionalisation tests for Re and Be
6. redimensionalisation tests
7. ICloneable tests, in case we need to make copies of the object
for use in thread safe parallel calculations.
















