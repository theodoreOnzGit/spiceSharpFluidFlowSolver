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
