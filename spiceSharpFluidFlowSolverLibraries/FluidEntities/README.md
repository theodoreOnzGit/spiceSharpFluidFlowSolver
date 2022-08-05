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

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_1)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

These will be weighting factors in determining our series density.

How shall this condition be achieved?

The friction factor indeed is quite complex in itself. So for a start
we go with bounding cases:

1. laminar
2. fully turbulent

### laminar restrictions on finding series parameters

suppose we are in a fully laminar regime, and all our pipes are in
a more or less laminar regime

$$f_{darcy} = \frac{64}{Re}$$

$$f_{darcy} = \frac{64 A_{XS}\mu}{\dot{m} D_H}$$

we can substitute this in here:

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_1)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

so that:
$$\frac{(\frac{64 A_{XS}\mu}{\dot{m} D_H} \frac{L_i}{D_i} + K_1)}
{(\frac{64 A_{XSseries}\mu_{series}}{\dot{m} D_{series}} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

Now for our weighting factors to be constant, we only need the darcy
depdendent terms to be constant, so we can get rid of the K bits

$$\frac{(\frac{64 A_{XS}\mu}{\dot{m} D_H} \frac{L_i}{D_i} )}
{(\frac{64 A_{XSseries}\mu_{series}}{\dot{m} D_{series}} 
\frac{L_{series}}{D_{series}} )} 
\approx constant $$

we can cancel out the 64 and mass flowrate since mass flowrate across
a series of fluid components ought to be constant

$$\frac{(\frac{ A_{XS}\mu}{ D_H} \frac{L_i}{D_i} )}
{(\frac{ A_{XSseries}\mu_{series}}{ D_{series}} 
\frac{L_{series}}{D_{series}} )} 
\approx constant $$

We note that cross sectional area

$$A_{XS} = \frac{\pi}{4} D_H^2$$

So we can effectively cancel out the effect of hydraulic diameter.

$$\frac{\mu_i L_i} {\mu_{series}L_{series}} 
\approx constant $$

Now one correlation which satisfies this condition is:


$$\mu_{series} = \frac{1}{L_{series}} \sum_{i=1}^n L_i \mu_i$$

Where

$$L_{series} = \sum_i^n L_i$$
$$\mu_{series} = \frac{\sum_{i=1}^n L_i \mu_i}{\sum_i^n L_i} $$

Effectively the series lengthscale should be the sum of all the pipes
in that series. And the dynamic viscosity of the series is the length
weighted average of all the viscosities. Which should make sense,

a longer pipe section has a larger kinematic viscosity effect.

### turbulent flow restrictions in finding series parameters

For fully turbulent flow, $f_{darcy} =  k_{darcy} $

The constant will depend on the roughness ratio of the pipe. This
is a constant property of each pipe.

This is towards the right hand side of the moody chart. We can 
substitute this in here:

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_1)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

$$\frac{(k_{darcyi} \frac{L_i}{D_i} + K_1)}
{(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

We can cancel out the other form losses and the darcy friction
factors which at this point are more or less constant.


$$\frac{( \frac{L_i}{D_i} )}{ (\frac{L_{series}}{D_{series}} )} = constant$$

What is a suitable correlation which satisfies this condition?

I could say that the length to diameter ratio is either the sum
of all constitutent length to diameter ratios or an ensemble mean.

Since i have already determined that the series component lengthscale
is the sum of all constituent lengthscales, i can just say that the
series length to diameter ratio is a simple average of all length to
diameter ratios. This would ensure that the hydraulic diameter is s
somewhat an average of all the diameters here.


$$\frac{L_{series}}{D_{series}} = \frac{1}{n} \sum_{i=1}^n 
\frac{L_i}{D_i}$$

So in fact the way to find the hydraulic diameter of the series is:

$$\frac{L_{series}}{\frac{1}{n} \sum_{i=1}^n 
\frac{L_i}{D_i}} = D_{series}$$

$$\frac{L_{series}n}{ \sum_{i=1}^n 
\frac{L_i}{D_i}} = D_{series}$$

n here is the number of components or pipes in series. This is of course
provided that the darcy friction factor actually provides some friction
factor, and it is not some valve where there is no pipe friction factor.

In that case, we can just set L/D to 0. But that means we should not 
count those components in the weighting of the hydraulic diameter for 
the series. The same goes for the weighting of $\mu$. 

However, the exact procedure should not matter too much as long as
the above conditions are satisfied.


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

So now we need to find a representative value for $K_{series}$ and 
$f_{darcySeries}$

To save us work however, we can just note:

$$Be = 0.5 Re^2 (\frac{L}{D} f_{darcySeries} +K_{series})$$

This may not be so helpful for now, because we will need to iterate
a flowrate to find the values. That again defeats the purpose of
interpolation.

Let's first solve the easier of the two:

We assumed just now that

$$\frac{K_{i}}{K_{series}} = constant$$

An easy way to get this correlation is:

$$K_{series} = \sum_{i=1}^n K_i$$

This would make intuitive sense as well.

The last challenge would be to get $f_{darcySeries}$ from 
the $f_{darcyi}$. We could simply use a weighted sum like before:

$$f_{darcySeries} \frac{L_{series}}{D_{series}} $$
$$ = \sum_{i=1}^n f_{darcyi} \frac{L_i}{D_i}$$

However the issue is we don't know what $f_{darcyi}$ is, we need a 
Reynold's number to figure that out.

One way around this is to build more interpolations, with respective 
Bejan numbers, we would then interpolate the individual $f_{darcyi}$.

However, it would mean taking up a lot of RAM and such. Nevertheless
this is a surefire way to get things done.

A second way is to assume that the weighting factors:

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_1)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

Would stay constant regardless of laminar or turbulent flow. This was
our assumption from the beginning.

For laminar flow, we note this weighting factor is quite dependent upon
mass flowrate. This is flow so slow that it is essentially creeping.


so that:
$$\frac{(\frac{64 A_{XS}\mu}{\dot{m} D_H} \frac{L_i}{D_i} + K_1)}
{(\frac{64 A_{XSseries}\mu_{series}}{\dot{m} D_{series}} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

Though an argument could be made that for laminar flow, Re is small, 
so K is negligible in general. This is true if most of the system is
comprised of pipes.

#### laminar weighting factors (flow independent)
Under this assumption:

$$\frac{(\frac{64 A_{XS}\mu}{\dot{m} D_H} \frac{L_i}{D_i})}
{(\frac{64 A_{XSseries}\mu_{series}}{\dot{m} D_{series}} 
\frac{L_{series}}{D_{series}} )} 
\approx constant = w_i$$

We can cancel out the mass flowrates and calculate the weighting factors
directly

$$\frac{(\frac{64 A_{XSi}\mu}{ D_{Hi}} \frac{L_i}{D_{Hi}})}
{(\frac{64 A_{XSseries}\mu_{series}}{ D_{series}} 
\frac{L_{series}}{D_{series}} )} 
\approx constant = w_i$$

Whereas for fully turbulent flow, this is mass flowrate independent.

#### turbulent weighting factors (flow independent)


$$\frac{(k_{darcyi} \frac{L_i}{D_i} + K_i)}
{(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$


A user can choose between both depending on the kind of flow he or she 
might expect.


Over here, we calculate $k_{darcyi}$ as the friction factor when
the flow is totally turbulent.

We can use the function in churchill's correlation to determine this.
And just set the Reynold's number to 1e8.


Now for $k_{darcySeries}$ we can use the following correlation


$$\sum (k_{darcyi} \frac{L_i}{D_i} + K_i) =
(k_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})
$$

We note that

$$K_{series} = \sum K_i$$

So those cancel out

$$\sum (k_{darcyi} \frac{L_i}{D_i} ) =
(k_{darcySeries} \frac{L_{series}}{D_{series}} )
$$


$$\frac{D_{series}}{L_{series}}\sum (k_{darcyi} \frac{L_i}{D_i} ) =
(k_{darcySeries}  )
$$

$$k_{darcySeries} =\frac{D_{series}}{L_{series}}
\sum (k_{darcyi} \frac{L_i}{D_i} ) 
$$

Those should be okay for functions to calculate. So we calculate
$k_{darcyi}$ first using churchill's correlation and only do it once
and then we can calculate the L and D of the series
circuit. Then from that arrive at the answer.

The advantage is that this is Reynold's number indepdenent, and also
temperature independent.

And as the flow goes up to higher Reynold's numbers, the solution becomes
exact. It will not do as well for lower reynold's numbers but a meager
guess for the weighting factors is better than no weighting factors at
all. Or an ensemble average.

Furthermore, liquid density of therminol hardly changes from 20C to 120C.
It is 1064 $kg/m^3$ at 20 C, and 982 $kg/m^3$ at 120C. 
Taking 120C as the reference temperature, that is at most an 8.4% change over that wide range. So no matter how much
effort we spend to do density weightage, the flow is mostly 
incompressible anyhow and this won't matter too much.



#### dyanmic weighting factors (don't bother too much here)

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_i)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant = w_i$$

Now we know this correlation holds


$$Be_{Di} = 0.5 Re_i^2 (f_{darcyi} \frac{L_i}{D_i} + K_i)$$


$$(f_{darcyi} \frac{L_i}{D_i} + K_i) = \frac{Be_{Di}}{0.5 Re_i^2}$$

$$\frac{(f_{darcyi} \frac{L_i}{D_i} + K_i)}
{(f_{darcySeries} \frac{L_{series}}{D_{series}} + K_{series})} 
\approx constant $$

$$ =\frac{Be_{Di}}{0.5 Re_i^2} \frac{0.5 Re_{Dseries}}{Be_{Dseries}} $$

$$ =\frac{Be_{Di}}{Re_i^2} \frac{Re^2_{Dseries}}{Be_{Dseries}} $$

If we can find the ratios of the bejan numbers and Reynold's numbers
, that would be a great estimate for us.

Suppose first that i have a Reynold's number with appropriate scaling:


$$Re_{series} = \frac{\dot{m}_{Series} D_{Hseries}}
{A_{XSseries} \mu_{series}}$$

$$Re_{i} = \frac{\dot{m}_{Series} D_{Hi}}
{A_{XSi} \mu_{i}}$$

$$\frac{Re_{Dseries}^2}{Re_i^2} = \frac{D_{Hseries}^2}{D_{Hi}^2}
\frac{A_{XSi}^2 \mu_i^2}{A_{XSseries}^2\mu_{series}^2}$$


Again if we have 

$$A_{XS} = \frac{\pi}{4}D^2$$

$$\frac{Re_{Dseries}^2}{Re_i^2} 
= \frac{D_{i}^2 \mu_i^2}{D_{series}^2\mu_{series}^2}$$

This is temperature dependent

$$\frac{Be_{Di}}{Be_{Dseries}} = \frac{\Delta P_i D_i^2}{\mu_i \nu_i}
\frac{\mu_{series}\nu_{series}}{\Delta P_{total} D_{series}^2}
$$

Multiply them together:


$$\frac{Be_{Di}}{Be_{Dseries}} \frac{Re_{Dseries}^2}{Re_i^2}=
 \frac{\Delta P_i D_i^2}{\mu_i \nu_i}
\frac{\mu_{series}\nu_{series}}{\Delta P_{total} D_{series}^2}
\frac{D_{i}^2 \mu_i^2}{D_{series}^2\mu_{series}^2}
$$


$$\frac{Be_{Di}}{Be_{Dseries}} \frac{Re_{Dseries}^2}{Re_i^2}=
 \frac{\rho_{i}\Delta P_i D_i^4}
 {\rho_{series}\Delta P_{total} D_{series}^4}
$$


That's as much as we can simplify it.

Now we'll have to make some assumptions. Under constant flowrate,
the flow regimes should not change so much as to impact the value of
$\frac{\Delta P_i}{\Delta P_{total}}$ even when temperature changes.

Thus, where weighting is concerned,

$$\frac{\Delta P_i}{\Delta P_{total}} \approx constant$$

While in truth, this ratio is Reynold's number dependent, this is just
here to help us estimate weights for density. So that the average
density produces the correct pressure drop.

We can just take this ratio at one temperature and Reynold's number
and hope it stays constant.

We also assume that there are a large number of components, so that
one component having a density change doesn't affect $\rho_{series}$
too much.

Either way i do this, it's not very much bang for buck. I'd rather go 
with the fixed turbulent regime weights.



