# Fluid Pipe Theory

## Intro

This document is here to introduce and elaborate on the concept of bringing fluid pipes with (mostly) incompressible fluids and reintroducing it as a nonlinear resistor in spicesharp so that a HVAC type simulator can be done.

## Converting Electrical Circuit Equations Into Fluid Equations

### Traditional SpiceSharp Calculations

In spicesharp, modified nodal analysis (MNA) is done over each node. Nodes here refer to the positive and negative terminals of the resistor. 

Positive terminals, often marked as node A, can be considered the "inlet" of the pipe, whereas negative terminals or node B, can be considered the "outlet" of the pipe.

This can be seen from the 
[example](https://spicesharp.github.io/SpiceSharp/articles/custom_components/example_resistor.html) here.


For most of the resistors, a current balance is performed both at the inlet and outlet of the pipe. The equations may be redundant at the end of the day, but the Solver objects automatically figure that out for you.

The job of the programmer is just to Program the resistor by performing current balance over the nodes.

For most nodes, current balance is being done. 

If voltage sources are introduced as in modified nodal analysis, then a fourth equation is put into the system of equations stating what the value of the voltage source is.

Now to 
### Does Current Correspond to Mass Flowrate or Volumetric Flowrate?

## Pipe Equations

In Perry's chemical engineering handbook, the formula used for fanning's friction factor by Churchill is:

$$f = 2 \left[\\
\left( \frac{8}{Re} \right)^{12} + \\
\left( \frac{1}{A+B}\right)^{3/2} \\
\right]^{1/12} $$
 
Where:

$$A = \left[ 2.457 \ln \frac{1}{\left( \frac{1}{(7/Re)^{0.9}} + \\
0.27 \frac{\varepsilon}{D} \right)} \\
\right]\ \ ; \ \ \\
B = \left( \frac{37530}{Re} \\ 
\right)^{16} $$


$$Re = \frac{ux}{\nu} = \frac{\dot{V} x}{A_{XS} \nu}$$

Where $A_{XS}$ represents cross sectional area.
We can see that this is strongly non linear with respect to volumetric flowrate.
abcde