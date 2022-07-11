# simulations

In this folder, you will find custom simulations which inherit
from Biasing Simulation class. 

This is because the standard operating point simulation
make it difficult to extract simulation data out of the class.

And other than printing data, it doesn't do all that much.

## PrototypeSteadyStateFlowSimulation

This is nothing but a copy of the operating point class. Just a different
class name.

## ISteadyStateFlowSimulation

This is a public interface which inherits from IBiasingSimulation.

However it will include interfaces by which one can extract data from
the simulation.
