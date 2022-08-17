# Therminol Pipe

The therminol pipe class is a pipe which will form the foundation
of all therminol based components

$$Be_D = 0.5 Re_D^2 (f_{darcy} \frac{L}{D} + K)$$

The pipe will form the basis of all therminol based components.

The only things the user must define are

1. Fluid Temperature (from this all fluid properties are calculated)
2. Pipe dimensions
3. fLDK factors

This itself is an abstract class from which all therminol type pipes are
derived.

## Inner Nodalisation

For a first iteration of the pipe, only ONE temperature is defined.

In reality however, especially for longer pipes, the temperature will 
vary throughout the pipe. We will therefore need nodes to calculate the
temperature distribution. However, adding several nodes in this pipe
may add additional unnecessary computational burden to this class.

If we were to introduce nodalisation eg. with 10 nodes , 
then we will need to break this pipe into ten subcomponents. 
To ensure that computational expediency is achieved, the same fLDK
factor is just divided by 10 for a given mass flowrate.

Given that hydraulic diameter is the same, the only things that may 
change are viscosity and density of the fluid.

We would then store a list of temperatures at different points of the 
pipe. These temperatures will be used to retrieve the fluid properties
of the pipe. For which the same fLDK equation is used to calculate 
pressure losses.

Doing so, we can speed up calculations still using the precalculation 
technique.



## Interfaces

### IFluidEntity
The therminol pipe should fulfil the IFluidEntity interface, where 
pressure losses can be obtained from mass flow and vice versa.

In fact, a list of functions should be standardised to help us return 
these fluid quantities

1. pressure loss from mass flowrate
2. mass flowrate from pressure loss
3. fLDK factor given a pressure loss, or mass flowrate
4. Property: List of temperatures for flow calculations
5. accessing component length
6. Accessing component diameters
7. accessing area
8. accessing fluid density
9. accessing fluid kinematic viscosity
10. accessing fluid dynamic viscosity
11. accessing hydrostatic pressure change
12. accessing change in elevation z
13. accessing change in coordinates (dx,dy,dz) --> important for 
fluidParallelCircuit, where i want to ensure all the branches
end up in the same location
14. Access Bejan number given pressure loss/drop
15. Access Reynolds number given mass flowrate
16. obtain mass flowrate from Re
17. obtain Bejan number from Re

sounds like Be and Re should be more like properties with different
get and set functions. But i'd prefer a functional style of programming
to avoid calculations being dependent on object state as far as possible.

Now problem is for this, if i want to return a fluid property, i must
define a temperature, and one temperature may not be sufficient to describe
the entire system. So accessing fluid properties using a function must
come by supplying a temperature beforehand so that we don't have to always
average out the temperatures before supplying the fluid temperature.

Then again, a lot of legacy code has the interface of using a representative
value to get temperature. It's probably unwise to get have a function getting
fluid properties here based on temperature.

### IHeatTransferFluidEntity

The therminol pipe should also be able to return important properties for
heat transfer, eg. Prandtl number.

However, prandtl number access should not be put under FluidEntity because
for isothermal fluids, you don't really need that. So under interface 
segregation, i may make another interface for this.

This heat transfer fluid entity must also have a temperaturelist which
shows temperature distribution in the component.

1. returnPr(Temperature)
2. returnThermalConductivity(Temperature)
3. returnDensity(Temperature)
4. returnDynamicViscosity(Temperature)
5. returnSpecificHeatCapacity(Temperature)
6. temperatureList

## Definition of Pipe via inheriting from Abstract class

The abstract class of the pipe would define how the 
interpolation objects would be done in the base constructor.

Note that the base constructor must be called, otherwise the 
interpolation won't work.

### Common methods

TBC


## Tests



# Bibliography

<a id="frictionFactorApproximations">
[1]
Zeyu, Z., Junrui, C., Zhanbin, L., Zengguang, X., & Peng, L. (2020). Approximations of the Darcy–Weisbach friction factor in a vertical pipe with full flow regime. Water Supply, 20(4), 1321-1333.
</a>
