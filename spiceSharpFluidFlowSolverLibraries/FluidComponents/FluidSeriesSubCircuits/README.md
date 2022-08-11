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


####  Viscosity, Length

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

we can substitute this in here:

$$w_i = \frac{(f_{darcyi} \frac{L_i}{D_i} + K_i)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
$$

so that:
$$w_i = \frac{(\frac{64 A_{XS}\mu}{\dot{m} D_H} \frac{L_i}{D_i} + K_i)}
{(\frac{64 A_{XSseries}\mu_{series}}{\dot{m} D_{series}} \frac{L_{series}}{D_{series}} + K_{series})} 
$$

In this limiting case we can ignore K.

$$w_i =\frac{(\frac{64 A_{XS}\mu}{\dot{m} D_H} \frac{L_i}{D_i} )}
{(\frac{64 A_{XSseries}\mu_{series}}{\dot{m} D_{series}} 
\frac{L_{series}}{D_{series}} )} 
$$

we can cancel out the 64 and mass flowrate since mass flowrate across
a series of fluid components ought to be constant

$$w_i = \frac{(\frac{ A_{XS}\mu}{ D_H} \frac{L_i}{D_i} )}
{(\frac{ A_{XSseries}\mu_{series}}{ D_{series}} 
\frac{L_{series}}{D_{series}} )} 
 $$

We note that cross sectional area

$$A_{XS} = \frac{\pi}{4} D_H^2$$

So we can effectively cancel out the effect of hydraulic diameter
and cross sectional area since the constant of difference between them
is $\pi/4$

We are then left with this simple setup for weighting factor:

$$w_i =\frac{\mu_i L_i} {\mu_{series}L_{series}} 
 $$

 This will be quite exact for very low Reynold's numbers.

Now this presumes we know $\mu_{series}$ and $L_{series}$.

One way to ensure the weighting factors sum up to 1 is this:

$$\mu_{series} = \frac{1}{L_{series}} \sum_{i=1}^n L_i \mu_i$$

Where

$$L_{series} = \sum_i^n L_i$$

Taking the lengthscale of the series to be the sum of constituent lengthscales
would make intuitive sense.

$$\mu_{series} = \frac{\sum_{i=1}^n L_i \mu_i}{\sum_i^n L_i} $$



This ensures that the weighting factors are normalised. Also it forces 
us to weigh viscosity using the relative lengthscales of the system.

Effectively the series lengthscale should be the sum of all the pipes
in that series. And the dynamic viscosity of the series is the length
weighted average of all the viscosities. Which should make sense,

a longer pipe section has a larger kinematic viscosity effect.
Now this is somewhat of a handwavy way of averaging kinematic viscosity. But
it would make intuitive sense.


A better way if we really want to take viscous forces into account at low
Re is this:


### turbulent flow restrictions in finding series parameters

For fully turbulent flow, $f_{darcy} =  k_{darcy}$


From the [colebrook correlation](https://www.sciencedirect.com/book/9781856178303/transmission-pipeline-calculations-and-simulations-manual):

For fully rough pipes:
$$\frac{1}{\sqrt{f_{darcy}}} = -2 \log_{10} (\frac{\varepsilon/D}{3.7})$$

This is very convenient for us since this is indepdenent of both
flowrates and temperatures (pipe expansion neglected).

The constant will depend on the roughness ratio of the pipe. This
is a constant property of each pipe.

This is towards the right hand side of the moody chart. We can 
substitute this in here:

$$w_i = \frac{(f_{darcyi} \frac{L_i}{D_i} + K_i)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
$$

$$w_i = \frac{(k_{darcyi} \frac{L_i}{D_i} + K_i)}
{(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
$$

We can cancel out the other form losses and the darcy friction
factors which at this point are more or less constant.

Now how shall we ensure that this is normalised?

$$\sum (k_{darcyi} \frac{L_i}{D_i} + K_i) =
(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})
$$

Now to start us off, we can use a very intuitive way of finding $K_{series}$

$$K_{series} = \sum_{i=1}^n K_i$$




Subtracting this term from both sides,
$$k_{darcySeries} \frac{L_{series}}{D_{series}} 
 = \sum_{i=1}^n k_{darcyi} \frac{L_i}{D_i}$$

Now we know how $L_{series}$ is calculated. What is left for us is to calculate
$D_{series}$.

Now it seems the friction factor of the series is also some weighted average
of the individual components.

We have yet another seti of weighting factors here:


$$w_{kDarcyi} =\frac{( \frac{L_i}{D_i} )}{ (\frac{L_{series}}{D_{series}} )} $$

To ensure that the weighting factors all sum up to one, we can use:
$$\frac{L_{series}}{D_{series}} =  \sum_{i=1}^n 
\frac{L_i}{D_i}$$



$$\frac{L_{series}}{ \sum_{i=1}^n 
\frac{L_i}{D_i}} = D_{series}$$

When we substiute the above values back into here, we shall get:
$$k_{darcySeries}  
 =\frac{D_{series}}{L_{series}} \sum_{i=1}^n k_{darcyi} \frac{L_i}{D_i}$$

This is of course
provided that the darcy friction factor actually provides some friction
factor, and it is not some valve where there is no pipe friction factor.

### Summary viscosity, area and hydraulic diameter scaling for series of components

For length:
$$L_{series} = \sum_i^n L_i$$

Dynamic viscosity:
$$\mu_{series} = \frac{\sum_{i=1}^n L_i \mu_i}{\sum_i^n L_i} $$

For hydraulic diameter:

$$D_{series} = \frac{L_{series}}{ \sum_{i=1}^n 
\frac{L_i}{D_i}} $$


### now back to our density (Liquid phase only, <10% thermal expansion)

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

Nevertheless, here's what i have.

What remains is now for us to find out how to weigh $\rho_{series}$ in
order to find an average.

$$ \rho_{series}A_{XSseries}^2 =  \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}
{\sum_{i=1}^n\frac{f_{darcyi} 
\frac{L_i}{D_1} + K_i}{\rho_i A_{XSi}^2}}$$

We have already shown two ways of weighting the density, one in a laminar bounding
case, and one in the fully turbulent bounding case.

All other cases in between are some mixture of both cases.

For laminar case:
$$w_i =\frac{\mu_i L_i} {\mu_{series}L_{series}} 
 $$

For turbulent case:

$$w_i = \frac{(k_{darcyi} \frac{L_i}{D_i} + K_i)}
{(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
$$

$$\sum (k_{darcyi} \frac{L_i}{D_i} + K_i) =
(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})
$$

Since the weighting factors for density don't matter as much (density doesn't
even change more than 8%). 

I can just take a ensemble average of these two weighting factors and it won't 
matter all that much.
