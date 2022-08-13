// See https://aka.ms/new-console-template for more information
using SharpFluids;
using EngineeringUnits;
using EngineeringUnits.Units;
using System;

// make a new therminol-VP1 fluid object

Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);

// set temperature and pressure

Pressure atmosphericPressure = new Pressure(1.1013e5, PressureUnit.Pascal);
EngineeringUnits.Temperature roomTemperature 
= new EngineeringUnits.Temperature(293, TemperatureUnit.Kelvin);

// update PT of therminol
// updates the temperature and pressure of therminol

therminol.UpdatePT(atmosphericPressure, roomTemperature);

// obtain prandtl number
Console.WriteLine(therminol.Prandtl);
