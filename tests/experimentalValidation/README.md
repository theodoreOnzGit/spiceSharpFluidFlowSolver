# validationData readme


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


# Bibliogrpahy

Zweibaum, N. (2015). Experimental Validation of Passive Safety System Models: Application to Design and Optimization of Fluoride-Salt-Cooled, High-Temperature Reactors. University of California, Berkeley.


