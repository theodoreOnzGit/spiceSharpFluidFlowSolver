# IsothermalPipeNumericallyUnstable Readme

For this pipe, it is the same as the isothermalPipe. But before I started
altering it to experiment with numerical stability for the newton raphson
solver.

I'm using this class as a backup in case something goes wrong with
isothermalPipe.

## main issue to solve
Basically i get issues when putting several isothermalPipeNumericallyUnstable,
in series with either voltage or current sources.

In parallel or with single isothermalPipeNumericallyUnstable, the solver is
stable and converges to the right value. The issues come once the solver is 
in series. 

I probably need to look at some methods to help the solver converge, but
before i make modifications, here is the backup.
