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


### Does Current Correspond to Mass Flowrate or Volumetric Flowrate?

Now, suppose we have current flow $i$, and voltage $V$. Current Flow is $\frac{Charge}{Time}$ while Voltage is $\frac{energy}{Charge}$.


Take the Product of both and we get

$$Power (watts) = iV = \frac{Charge}{Time}*\frac{Energy}{Charge}$$ 

Converting current into a mass or volumetric flowrate for fluids is a matter of replacing charge with volume or charge with mass.

Take Q = volumetric flowrate in $m^3/s$ and $\Delta P$ as pressure drop.

$$Power (watts) = Q*\Delta P = \frac{Volume}{Time}*\frac{Energy}{Volume}$$ 

We can see here that the pressure drop $\Delta P$ can be interpreted as loss 
in energy over the pipe per unit volume.

This is okay for a truly incompressible fluid, because one can 
simply replace Kirchov's current law, a current balance, 
with a volumetric flowrate balance.

But for a slightly compressible fluid, such that we want to have small expansions 
and contractions of liquids due to temperature differentials, those will not do.

Only a mass balance will work across the nodes no matter what. 
To convert the volumetric flowrate into mass flowrate,

We can multiple the numerator and denominator with density.
$$Power (watts) = Q\rho*\frac{\Delta P}{\rho} 
= \frac{mass}{Time}*\frac{Energy}{mass}$$ 

$$Power (watts) = \dot{m}*\frac{\Delta P}{\rho} = 
\frac{mass}{Time}*\frac{Energy}{mass}$$ 

The result is that current $i$ will correspond directly 
to mass flowrate $\dot{m} (kg/s)$ 
and voltage would correspond directly to kinematic pressure 
$\frac{\Delta P}{\rho}(m^2/s^2)$

Mass balance will always be consistent no matter the density, 
so this is physically more accurate and more intuitive mathematically.

Therefore, we will be doing mass balance equations over each node in place of current balance. It is a one to one replacement that does not require change of source code.




## Pipe Equations

In Perry's chemical engineering handbook, the formula used for 
fanning's friction factor by Churchill is:

$$f = \frac{\Delta P}{ (\frac{4L}{D}) \  \frac{1}{2} \rho u^2 }$$

$$\Delta P = f  (\frac{4L}{D})  \frac{1}{2} \rho u^2 $$

$$f = 2 \left[\\
\left( \frac{8}{Re} \right)^{12} + \\
\left( \frac{1}{A+B}\right)^{3/2} \\
\right]^{1/12} $$


The first term on the left represents the contribution by laminar flow regimes. 
For low reynolds numbers, B and A become really big. 
And so the right term becomes negligible.
The fanning friction factor reduces to $\frac{16}{Re}$ for Low reynolds numbers.


 
Where:

$$A = \left[ 2.457 \ln \frac{1}{\left( \frac{1}{(7/Re)^{0.9}} + \\
0.27 \frac{\varepsilon}{D} \right)} \\
\right]\ \ ; \ \ \\
B = \left( \frac{37530}{Re} \\ 
\right)^{16} $$


$$Re = \frac{ux}{\nu} = \frac{\rho u D}{\mu}$$ 

$$Re = \frac{\dot{m}D}{\mu A_{xs}}$$

Where $A_{XS}$ represents cross sectional area.
We can see that this is strongly non linear with respect to volumetric flowrate.

### From resistor current balance to pipe mass balance
![Resistor](resistor.png)
note: i copied the image from 
[here](https://spicesharp.github.io/SpiceSharp/articles/custom_components/example_resistor.html)

In spicesharp, we perform current balance over node A, 
essentially the inlet and node B, essentially the outlet.

The voltage at A and B can represent the kinematic pressure at the inlet 
and outlet respectively.

For a relationship between mass flowrate and kinematic pressure respectively,
at least for circuits, Ohm's law is used. It is a neat, explicit linear
relationship.

$$\frac{v_A-v_B}{R}=i_R$$

For the relationship between mass flowrate and pressure, an equivalent
equation which, if Ohm's law is obeyed, should look like this

$$\frac{p_A (m^2/s^2) - p_B (m^2/s^2)}{R_{flow}} = \dot{m}_{pipe} (kg/s)$$

Unfortunately, the relationship between kinematic pressure and mass flowrate 
is hardly ever that simple.

For a laminar flow regime for example, f is conveniently $\frac{16}{Re}$

$$f = \frac{\Delta P}{ (\frac{4L}{D}) \  \frac{1}{2} \rho u^2 }$$

Now let's substitute this value and use kinematic pressure (lowercase $p$) instead of dynamic pressure
$$\frac{16}{Re} = \frac{\Delta p}{ (\frac{4L}{D}) \  \frac{1}{2} u^2 }$$


$$Re = \frac{\dot{m}D}{\mu A_{xs}} = \frac{\rho u D}{\mu}$$
$$\frac{16\mu}{\rho u D} = \frac{\Delta p}{ (\frac{4L}{D}) \  \frac{1}{2} u^2 }$$
$$\frac{16\mu}{\rho  D} = \frac{\Delta p}{ (\frac{4L}{D}) \  \frac{1}{2} u }$$
$$\frac{64\mu (\frac{L}{D}) }{\rho  D} = \frac{\Delta p}{   \frac{1}{2} u }$$
$$\frac{64\mu L }{\rho  D^2} = \frac{\Delta p}{   \frac{1}{2} u }$$

We then relate $\dot{m} = \rho u A_{xs}\ ; u = \frac{\dot{m}}{\rho A_{xs}}$

$$\frac{64\mu L }{\rho  D^2} = \frac{\Delta p}{   \frac{1}{2} \frac{\dot{m}}{\rho A_{xs}} }$$

$$\frac{64\mu L\frac{1}{\rho A_{xs}}   \frac{1}{2}}{\rho  D^2} = \frac{\Delta p}{ \dot{m}  }$$

$$\frac{32\mu L   }{\rho^2 A_{xs}  D^2} = \frac{\Delta p}{ \dot{m}  }$$

For laminar flow essentially, Ohm's law is essentially obeyed and there is 
an explicit relationship.

For transitional and turbulent flow regimes, this law isn't obeyed.
![Moody Diagram](https://nuclear-power.com/wp-content/uploads/2016/05/Moody-chart-min.jpg)
[^Moody Diagram]

## Bibiliography
[^Moody Diagram]:
Fanning friction factor. Nuclear Power. (2021, October 25). Retrieved June 3, 2022, from https://www.nuclear-power.com/nuclear-engineering/fluid-dynamics/major-head-loss-friction-loss/fanning-friction-factor/ 













