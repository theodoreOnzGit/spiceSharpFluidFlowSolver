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

### Calculation strategy

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

### Preplotting the system curve

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

### Nondimensionalising a series of fluid components

TBD
## FluidParallelCircuits

### issues with fluidseriesCircuits and FluidParallelSubCircuits

#### 1. nested iteration
Now the problem with fluidSeriesCircuits is that one has to employ 
nested iteration strategies to deal with parallel flow. This is 
problematic because it is time consuming to solve, and also
that parallel sub circuits are not easily nondimensionalised when
buoyancy forces come into play.

#### 2. preplotting interpolation strategy difficult to employ

For parallel subcircuits, buoyancy forces are not easily decoupled
from their the respective branch's loss terms because those are
quite Re dependent.


#### Inspiration to avoid nested iteration

However, each branch can essentially be one pipe or a series of fluid
components. And obtaining a total mass flowrate from a pressure drop
across these series of pipes is quite okay given that a fluidSeries
Subcircuit is coded properly.

##### zero flow through parallel subcircuits
If the flow through a parallel subcircuit is zero, then we have a special
case: flow can upwards through one branch and downwards through other 
branches.

However, this special case essentially allows us to solve very simple
parallel flow setups.


### Converting a simple parallel and series circuit into a fully parallel circuit

Suppose we have a pipe reigme like similar to a power plant:

1. we have a main pipe branch which pumps coolant 
through a heat exchanger
2. we branch off into three separate parallel branches, one perhaps 
contains a heater, and another contains a secondary cooling loop and
a bypass flow
3. These three branches combine and go through a pump.


This could be thought of as a series circuit with the main branch 
splitting into three parallel branches and back.

However, if one were to take the tees as the points of reference,
this circuit could essentially be converted into a parallel circuit
with four branches. Thus, we can use the parallel circuit setup to solve
for the flow through the four branches.

The pressure loss calculations could be easily calculated for each branch
provided they can be represented by a series of fluid components.

So suppose we have a pump in one of these branches, with natural 
convection, and some components. In another branch we have natural
convection sources also.

The Be vs Re graph for each could be decoupled 
from the driving forces within the circuit.


To solve for the flow within each branch, one should apply the same
pressure change to each branch, and then iterate accordingly until
the sum of flowrates across the tees become zero.

The program setup could be as follows:

1. Construct Four FluidSeriesSubCircuits, 
precalculate Be vs Re for each
2. Place these four fluidSeriesSubCircuits into the FluidParallelCircuit
3. Iterate the pressure changes across each branch (which must be the
same) the sum of mass flowrates across each branch becomes zero, use 
MathNet's algorithms.


In this way, we have only one iterative solver solving for mass flowrate
equals to zero across this loop. The iterative bits for each fluidSeries
Circuit would have already been precalculated via nondimensionalisation.

Thus, solving for the flow through each branch now is very fast and
computationally efficient. However, this solver is only applicable
to a specialised subset of cases, eg. series circuits and series circuits
with one set of pipes.




















