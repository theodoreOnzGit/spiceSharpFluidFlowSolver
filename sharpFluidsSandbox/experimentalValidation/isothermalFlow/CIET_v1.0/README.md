# isothermal flow CIET v1.0 validationData readme


This folder contains data for the compact integral effects test
(CIET).

## thermophysical data
The data is plotted as manometer height (m) against mass flowrate
in the loop (kg/s).

The flowrate range is from 0 (kg/s) to 0.18 (kg/s) and
the height range is from approximately 0.2 m to 1.75m. 


The fluid used in the loop is Dowtherm A, or Therminol VP-1 which
has a very similar chemical composition and therefore 
thermophysical properties. It is used at room temperature
and the temperature in the loop is constant approximately.

I would have to guess the temperature is about 21 C. 
(EDIT: ambient temp is stated at 20 C in thesis, page 31)

## methods
Sample data is pulled from a paper by Dr Nicolas Zweibaum's PhD 
dissertation, Experimental Validation of Passive Safety System 
Models.

I used the [graphreader tool](http://www.graphreader.com/) to extract
the relevant data and save them in json and csv format.

## interpretation

For pressure drop across the entire loop, M-41, M-42 and M-43 are the
manometers to take note of.

M-42 is located just before the pump whereas M-43 is located right 
after the pump.

For fluid going into the pump, the fluid pressure will drop just
before the suction region. I posit that M-42 has a lower pressure
than M-41 for this reason.

The other possible and more likely reason is because there is a 
strainer S-40 between M-42 and M-41, causing a slight pressure drop
between the two components.

I'm also not sure of the valve lineup for this dataset. I'll have to
assume for now that it is the normal flow pattern without bypass or
flow reversal.

## stability testing

So far, i have noted that for this setup, whenever i connect two
or more pipe systems in series, the solver is unable to converge
to a suitable value. I suspected it is because of the transition
region in turbulence.

Therefore I tried taking a shortcut: CIET mostly had flow regimes
in the laminar region, therefore just use the components with 
fLDK correlations (ie Be = (f *L/D + K)*Re^2).

Those don't seem to have transition regions, and do not seem to 
blow up when connected in series with source stepping. However, 
this is only true for
lower Reynold's numbers eg 1100 and below. Any higher and the solver
fails to converge even with source stepping. 

I therefore conclude that the system curve generating method would be 
best to help either solve the system of equations or get so close
to the solution that the curve has sort of a linear jacobian
within the vicinity of the solution.

This leaves us with two problems.

1. get an algorithm to help us automatically call methods within
each component to help generate the Bejan number given a Re

2. supply the pressure drops here into an initial guess for the nodes
so as to help the solver converge.


# Bibliogrpahy
Zweibaum, N. (2015). Experimental Validation of Passive Safety System 
Models: Application to Design and Optimization of 
Fluoride-Salt-Cooled, High-Temperature Reactors. University of .
California, Berkeley.


