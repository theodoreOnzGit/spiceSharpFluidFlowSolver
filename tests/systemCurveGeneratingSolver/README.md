# systemCurveGeneratingSolver README


This is the test suite for systemCurveGeneratingSolver.

## premise

Basically when i tried joining isothermal pipes together in 
series, I would get solver instablity whenever i tried to 
use voltage sources or current sources.

Nevertheless, i am able to work on pipes in parallel. 
I've tested running a voltage source on three pipes in parallel.
And that doesn't have any issue at all converging.

In other words, when i pass a voltage (or pressureDrop) to a pipe, 
i can immediately get a current or mass flowrate.

When i repeat this several times, i am able to get the system curve
of the pipe. A system curve states that for a set flowrate, expect
this much pressure drop.

If i can generate system curves for several pipes at a time, i can
basically stack them one on top of another. 

## some math

suppose i have a system curve for a pipe

$$pressureDrop = f (massFlowRate) $$

Now if i have five pipes in series, i can use:


$$pressureDrop_{total} = f_{pipe1} (massFlowRate) 
+f_{pipe2} (massFlowRate) 
+f_{pipe3} (massFlowRate) 
+f_{pipe4} (massFlowRate) 
+f_{pipe5} (massFlowRate) 
$$

The same can be applied in genearl for components in series.

For components in parallel, suppose we have five components.

i would sum the mass flowrates up instead of pressureDrops.

Suppose i could get a correlation as follows

$$massFlowRate = g (pressureDrop) $$

$$massFlowrate_{total} = g_{pipe1} (pressureDrop) 
+g_{pipe2} (pressureDrop) 
+g_{pipe3} (pressureDrop) 
+g_{pipe4} (pressureDrop) 
+g_{pipe5} (pressureDrop) 
$$

With that we can essentially sum the operating curves together to get pressure profiles at
various points of the system.

## obtaining pressure profiles

Suppose i wanted the pressure profile across a series of pipes.

I would either specify a total pressure drop or a specified mass flowrate.

The total pressure drop could be converted into a mass flowrate by
iteration. After that, knowing the system curves of each pipe,
I would feed this mass flowrate into the pipe. And there we go.


## obtaining mass flowrate from pressure drop across system

The first step would be to generate a system curve by summing up all the
system curves as before. 

We would then set a numerical iterator to use some bisection algorithm
so that mass flowrate is steadily adjusted until the total pressure drop
converges on the fixed pressure drop.

Once that is found, we have the mass flowrate.

## obtaining system level curves

We could place all the pipes in parallel. And set a fixed pressure across
all of them using a common voltage source.

Then observe the mass flowrate across each branch. Log that as one data point.
Repeat it for all the desired pressure drops.

We should have lots of data once we do this. And them sum the curves as 
described before.  

# issues

Now when i put pipes in parallel, i seem to get a reading of 0.0 mass flowrate
through them. Which is quite curious and frankly nonsensical. 

