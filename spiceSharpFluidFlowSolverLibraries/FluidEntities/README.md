# FluidEntities


## FluidSeriesCircuit

A Fluid Series Circuit is a series connection of fluid entities or fluid components,
meaning to say pipes, valves, or even a parallelSubCircuit.

How i manage parallel pipes is this:

If i have two parallel paths, i will contain these paths into an object called
a parallelSubCircuit. This parallelSubCircuit will be treated like a single 
component when it comes to a pressure drop.

Such components are then put in series, and the pressure drop of the entire system
is then the sum of pressure drops of these components.

# Calculation strategy

Suppose we have 10 pipes in series. Obtaining pressure drop is rather easy task,
just put in a fixed mass flowrate, they get converted into a Reynold's number of 
each pipe. The pipe objects will return a pressure drop using the Churchill 
correlation. And that's it.

However, getting mass flowrate is different. If we have a fixed pressure drop across
this system, then we will have to guess what the mass flowrate is and iterate 
ourselves till we get the flowrate with the correct pressure drop. With MathNet's 
RootFinding libraries which use a mixture of newton raphson and bisection, a 
solution is guaranteed if a root exists.

Now this is okay for simple systems. That is if we can explicitly get pressure drop
using a mass flowrate all the time.

However, the problem starts once we have a parallel pipe system. For this system
we have to iteratively solve for pressure drop given a mass flowrate. This is because
for parallel setups, the pressure change should be the same across each branch.

And pressure change has two parts (1) pressure drop or losses due to friction, 
and (2) pressure drop or gain due to hydrostatic effects. For a parallel pipe system
we assume that the start and end points are the same, so hydrostatic effects should
be the same on both branches in an isothermal case. And therefore pressure drop
due to friction across both branches must be equal. This is for isothermal case,
for natural convection, we have to think about the problem differently.

So for pipes in series, to solve for mass flowrate given a pressure drop, we have
to calculate this iteratively. However, add a parallel subsystem inside, and we
have nested iteration loops. (1) that we have to solve for mass flowrate iteratively
systemwide (2) that we have to solve for pressure drop iteratively given a total mass
flowrate across both parallel branches, and (3) that we have to solve for mass 
flowrate iteratively given a pressure drop across each branch.

If we have several nested parallel branches, then the number of iterations would
increase exponentially.

Suppose we need twenty iterations to solve for the FluidSeriesCircuit, and another
twenty to solve for the parallel subsystem, and another twenty to solve for each
branch in the parallel subsystem. We would then need about 8,000 iterations to 
solve for the steady state flow in one time step.

In practice, i tested with a simple setup of three pipes in parallel, nested within
a fluidSeriesCircuit, the calculations are stable and they work, but they took
about one minute to solve. For a more complex system, this could easily extend to
5 mins or more.

How then can we calculate these pressure drops in real time as needed for a digital
twin? We need these calculations to be done in 100 ms or less, or better yet
10ms or less.

## Preplotting the system curve

Well suppose we had a simple pipe, how can we prevent this long set of iterations
from occuring? 

No matter how fluid conditions change, as long as the pipe dimensions and surface
roughness remain constant, and the form losses don't change, we can actually plot 
a dimensionless pressure drop Be against the Reynold's number. This only needs to be
done once, and never again. This is done using cubic spline interpolation.

While fluid temperatures change, or fluid properties may change, the relationship
between Re and dimensionless pressure drop Be does not change. Thus, the code will
perform interpolation to get Re (dimensionless mass flowrate) from Be (dimensionless
pressure drop).

For one pipe, or one component, this is easy enough.

How about a series of pipes? We will surely have a temperature profile of some sort.

We will have to take into account that at different sections, the pipe series will
have differing Reynold's numbers.

We'll come back to this later.

First i applied this strategy to the parallel subcircuits. And replaced the 
iteration algorithm with the interpolation algorthim i wanted to see if
there was a significant speed increase.

The time to beat was 1 minute.

## Nested tests with FluidSeriesCircuits

Now after the interpolation strategy, things seem to be working out for the 
most part. The code runs a lot faster. And the only significant time taken
to solve the FluidSeriesCircuit is the time taken to build the fluidSystemCurves.

Each parallel subcircuit takes about 20s to build the respective Re vs Be graph
but after this calculation, there is no more need to calculate and we can just
use the graph (or actually the interpolation object). 20s was much much faster
than 1 minute. The time savings will only show once i calculate multiple timesteps
or more.

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

## Nondimensionalising a series of fluid components

Now it appears we can't really run away with nondimensionalising a 
fluidSeriesCircuit. Whether we use iteration or interpolation, we will have
to do this. How can we do this?

How can we pick characteristic length scales, dynamic viscosities, etc?

For the parallel subcircuits, we started with the conservation of mass
across the branches.

$$\dot{m}_{total} = \dot{m}_1 + \dot{m}_2$$

This means if we have two branches, the mass flowrate across the parallel setup
the the sum of both smaller mass flowrates.

In a similar fashion, for series setups, we have to say that the sum of
pressure drops (dynamic) is equal to the total dynamic pressure drop across the
series of components.

Suppose for a two pipe in series system:


$$\Delta P_{total} = \Delta P_1 + \Delta P_2$$

What are the appropriate expressions for each pressure drop?

We shall assume fluid components follow this equation:

$$Be_D = 0.5 (f \frac{L}{D} + K) Re^2$$

$$Be_D = \frac{\Delta P D_H^2}{\nu \mu}$$

So we can find that:


$$\frac{\Delta P D_H^2}{\nu \mu}= 0.5 (f \frac{L}{D} + K) Re^2$$


$$\Delta P = 0.5 \frac{\nu^2 \rho}{D_H^2} (f \frac{L}{D} + K) Re^2$$

Now let's perform some cancellations noting that:

$$Re = \frac{\dot{m} D_H}{A_{XS} \mu}$$


$$\Delta P = 0.5 \frac{\nu^2 \rho}{D_H^2} (f \frac{L}{D} + K)
\frac{\dot{m}^2 D_H^2}{A_{XS}^2 \mu^2}$$

Let's cancel out the diameters:
$$\Delta P = 0.5 \frac{\nu^2 \rho}{\mu^2} (f \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

We note also that if $\mu$ is dyanmic viscosity, $\rho$ is fluid density and
$\nu$ is fluid kinematic viscosity:

$$\mu^2 = \rho^2 \nu^2$$

$$\Delta P = 0.5 \frac{\nu^2 \rho}{\nu^2 \rho^2} (f \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

$$\Delta P = 0.5 \frac{1}{\rho} (f_{darcy} \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

So far we see that no matter how we get an average kinematic viscosity, it just
cancels out. I also denote f as the darcy friciton factor for clarity.

Suppose now i substitute these back into our correlation for series pipes:

$$\Delta P_{total} = \Delta P_1 + \Delta P_2$$

$$\Delta P_{total} = 0.5 \frac{1}{\rho_1} (f_{darcy1} \frac{L_1}{D_1} + K_1)
\frac{\dot{m_1}^2 }{A_{XS1}^2 } + 0.5 \frac{1}{\rho_2} (f_{darcy2} 
\frac{L_2}{D_2} + K_2) \frac{\dot{m_2}^2 }{A_{XS2}^2 }$$

Now suppose that the series of pipes can be represented by one long component.

$$\Delta P_{total} = 0.5 \frac{1}{\rho_{avg}} (f_{darcyAvg} \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }$$

So now if we substitute this in, we get one long equation:

$$ 0.5 \frac{1}{\rho_{avg}} (f_{darcyAvg} \frac{L}{D} + K)
\frac{\dot{m}^2 }{A_{XS}^2 }
$$
$$= 0.5 \frac{1}{\rho_1} (f_{darcy1} \frac{L_1}{D_1} + K_1)
\frac{\dot{m_1}^2 }{A_{XS1}^2 } + 0.5 \frac{1}{\rho_2} (f_{darcy2} 
\frac{L_2}{D_2} + K_2) \frac{\dot{m_2}^2 }{A_{XS2}^2 }$$

Now for series pipes, we know that the mass flowrate through each component is equal,
we will cancel it out. Also we will cancel out the factor of 0.5


$$ \frac{1}{\rho_{avg}} (f_{darcyAvg} \frac{L}{D} + K)
\frac{1 }{A_{XS}^2 }=  $$
$$\frac{1}{\rho_1} (f_{darcy1} \frac{L_1}{D_1} + K_1)
\frac{1}{A_{XS1}^2 } + \frac{1}{\rho_2} (f_{darcy2} 
\frac{L_2}{D_2} + K_2) \frac{1}{A_{XS2}^2 }$$

Let's now do some tidying up

### Dimensionless Pressure Drop Correlation for components in Series

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}{\rho_{series}A_{XSseries}^2 } 
=  \frac{f_{darcy1} \frac{L_1}{D_1} + K_1}{\rho_1 A_{XS1}^2} 
 + \frac{f_{darcy2} \frac{L_2}{D_2} + K_2}{\rho_2 A_{XS2}^2 }  $$

In general

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}{\rho_{series}A_{XSseries}^2 }$$

$$=  \sum_{i=1}^n\frac{f_{darcyi} \frac{L_i}{D_1} + K_i}{\rho_i A_{XSi}^2}$$

This will become our fundamental correlation as to how to find the average
of these quantities for the whole pipe such that the total pressure drop doesn't
change under influence of changing temperature or temperature profile across the
pipes. 

### Required quantities to nondimensionalise a series of pipes

To build a dimensionless correlation of Be vs Re to interpolate,


$$Re = \frac{\dot{m} D_H}{A_{XS} \mu}$$
$$Be = \frac{\Delta P D_H^2}{\mu \nu}$$

We only need to find the representative kinematic and dyanmic 
viscosity, or dynamic viscosity and density.  As well as hydraulic mean
diameter. 

$$A_{XS} = K D_H^2$$

We don't really need to worry about cross sectional area since it is
taken care of by $D_H$ scaling. Mass flowrate is constant through the 
pipes and so we don't really need to worry either, last of all
dyanmic pressure drop across the whole pipe series is already known:
it is our dependent variable.

So we need only find $\rho_{series}$, $D_{Hseries}$ and $\mu_{series}$.
Such that the above correlation holds for all temperatures as far as 
possible.

### finding the representative density for the series of pipes

The density of the series can be found by rearranging the following 
equation

$$ \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}{\rho_{series}A_{XSseries}^2 }$$

$$=  \sum_{i=1}^n\frac{f_{darcyi} \frac{L_i}{D_1} + K_i}{\rho_i A_{XSi}^2}$$


$$ \rho_{series}A_{XSseries}^2 =  \frac{f_{darcySeries} \frac{L_{Series}}{D_{series}} + 
K_{series}}
{\sum_{i=1}^n\frac{f_{darcyi} 
\frac{L_i}{D_1} + K_i}{\rho_i A_{XSi}^2}}$$


This is a really concise equation, but the problem is that
we don't know what the individual darcy friction factors even though we 
already know the total pressure drop. In fact, we'd have to iterate
the pressure drop out, which kind of defeats the purpose of interpolating
in the first place.

In fact, we need to know the Reynold's number in order to guess the 
individual darcy friction factors. And right now we don't know how Re 
will relate to Be yet.


However, our life will be made easier if:



Now suppose i set the constraint that the ratio of the fLDK terms are approximately
1.

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_1)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx 1$$

This way we don't even need to care about the relative weights of each
of the density and cross sectional areas.

We will then have a very easy way of weighting our densities.

$$ \rho_{series}A_{XSseries}^2 =  \frac{1}
{\sum_{i=1}^n\frac{1}{\rho_i A_{XSi}^2}}$$


However, this will hardly be possible for every single component
because they are so varied. A more plausible approach is that
these weighting ratios are more or less constant


These will be weighting factors in determining our series density.
Now even this will be hard in general because at different temperatures
the Reynold's number at each pipe section will differ.

$$w_i = \frac{(f_{darcyi} \frac{L_i}{D_i} + K_i)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant $$

If this were doable, our life will be simple. Unfortunately it is not
the case in gneeral since the ratios may differ based on temperature and
flow regime.

The friction factor indeed is quite complex in itself. 
So to estimate these weighting factors we might 
go with bounding cases:

1. Stokes regime laminar
2. fully turbulent

### laminar restrictions on finding series parameters

suppose we are in a fully laminar regime, and all our pipes are in
a more or less laminar regime.

This is the extreme case where Re is so low we can ignore the effects
of K.

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