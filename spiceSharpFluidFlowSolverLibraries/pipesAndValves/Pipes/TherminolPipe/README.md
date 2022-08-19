# Therminol Pipe

The therminol pipe class is a pipe which will form the foundation
of all therminol based components

$$Be_D = 0.5 Re_D^2 (f_{darcy} \frac{L}{D} + K)$$

The pipe will form the basis of all therminol based components.

The only things the user must define are

1. Fluid Temperature (from this all fluid properties are calculated)
2. Pipe or component dimensions, Length, hydraulic diameter and 
incline angle
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


### Methodology of Nodalisation

How can we now ensure nodalisation is done correctly so that 
the correct pressure loss is achieved?

Suppose we have a list of 10 nodes or volumes within the pipe,
and each node has a unique temperature, and each node within itself
behaves like an isothermal pipe.

We have already derived from our FluidSeriesSubCircuit how to obtain
the averaged dynamic viscosity, densities, lengthscales etc. 

Our objective now is to write this into code, and then test it by
implementing it as a default means of nodalisation.

From the FluidSeriesSubCircuits derivations, we find that we can
weigh viscosity such that it will yield the correct pressure loss
terms for stokes flow regimes:

$$\frac{\mu_{series} 
L_{series} }{\rho_{series} A_{XS{series}}^2} 
= \sum_i^n  \frac{\mu_i L_i }{\rho_i A_{XSi}^2} 
   $$

Where:
$$A_{XSi} = \frac{\pi D_i^2}{4}$$

$$L_{Series} = \sum_i^n L_i $$

And the density is weighted according to ensuring the correct
hydrostatic pressure changes (boussinesq approximation):
$$\Delta H_{series} \rho_{series}  = \sum_i^n \Delta H_i \rho_i $$

Where:

$$\Delta H_{series}   = \sum_i^n \Delta H_i $$

And for entrance and exit regions of different areas, I can
simply linearly interpolate the hydraulic mean diameter from its distance
from the start and end points.

$$D_i = \frac{D_{exit} - D_{0}}{L_{series} - L_{0}} (L_i - L_{0})
+D_{0}$$

$L_0$ refers to the length at the start of the pipe, which is essentially
0m.

$$D_i = \frac{D_{exit} - D_{0}}{L_{series}} (L_i)
+D_{0}$$

In the program, i can keep the formula as such, and just set $L_0$ = 0.

$$D_i = \frac{D_{exit} - D_{0}}{L_{series} - L_{0}} (L_i - L_{0})
+D_{0}$$

However this will necessitate us to have a list of lengths by default.
I do not need fluidEntities to enforce this, but it is essential for
heat transfer fluid entities to have this list so that the calculation
can be done.

Also, the length $L_i$ being used to interpolate the diameter must
be the midpoint of each segment.

So i will have a list of lengthscales, and then a list of diameters
and respective cross sectional areas will be calculated.

I will also need a start and end diameter or cross sectional area
which is set by the user. As diameter is more commonly used to 
measure pipes, i will then use diameter as the default method
of setting areas, rather than the other way round.

So basically, i need a list of temperatures, for which it will 
automatically calculate a list of densities, viscosities and
thermal conductivities and heat capacities also.

But one thing at a time though!

So for segement lengths, tests must be as follows:

1. Create a list of lengths within therminolpipe by default, 
IHeatTransferFluidEntity need not implement this interface. Because
i can simply use a for loop to cycle through each component for
a FluidSeriesSubCircuit. But for each entity, i need a way of splitting 
it up. Nevertheless, it may be a good idea anyhow so that it will help
structure the code easier. Ie. patterns of coding are more consistent.
2. By default, each of the lengths must equal 1/n times of the
original length. 
3. When summed up, the segement lengths must equal the original
length
4. When zero segments are given, throw an error.
5. When one segment given, it must be such that the length is
the same as the original pipe.

And then for segment diameters, test must be as follows:

1. Segement Diameters must be autocalculated whenever the 
list of lengths is set
2. Segment diameter interpolation should be based on the midpoint
of the list of lengths.

#### pipe segment Length code and tests
##### 1 .List of Lengths
Under TherminolPipe.cs, we have:
```csharp
public virtual IList<Length> lengthList { get; set; }
```

It's virtual in case you want to do other stuff with it.

##### 2. By default, each length is equal to 1/n times original length

Under TherminolPipe.cs, the numberOfSegments property by default
will automatically create a new lengthList with segments of
appropriate length:

```csharp
private int _numberOfSegments;
public virtual int numberOfSegments { 
    get {
        return this._numberOfSegments;
    }
    set {
        if(value <= 0)
            throw new DivideByZeroException("numberOfSegments <= 0");
        this._numberOfSegments = value;
        this.setLengthListUniform(value);
    }
}
```

This is a virtual function, meaning to say you can override it.
But don't have to if you don't want to change the default behavior.

The method setLengthListUniform is:

```csharp
private void setLengthListUniform(int numberOfSegments){
    // this function helps to evenly split a pipe into
    // n segements given a number of segments
    // so that the user doesn't have to manually set the
    // list of lengths

    Length segmentLength = this.getComponentLength()/ 
        numberOfSegments;

    List<Length> tempLengthList = new List<Length>();


    for (int i = 0; i < numberOfSegments; i++)
    {
        tempLengthList.Add(segmentLength);
    }
    this.lengthList = tempLengthList;

    return;
}
```

This is not overrideable, because you don't really need to override
this function. If you want to change the default behavior, change
the property numberOfSegments instead.

As a quality of life improvement however, i'd like to have a compile
time warning to ensure the user sets the number of segments, 
or at least it defaults to something. eg. 1.

##### 3. When summed up segment lengths must equal original length

I basically forced this feature in whenever setting the lengthList.

```csharp
private IList<Length> _lengthList;
public virtual IList<Length> lengthList { 
    get{
        return this._lengthList;
    }

    set{

        // let's do a null check first:
        if (value is null){
            throw new NullReferenceException("lengthList set to null");
        }

        // first i want to check if the 
        // total segment length is equal to the componentLength

        Length totalLength = new Length(0.0, 
                LengthUnit.Meter);
        foreach (Length segmentLength in value)
        {
            totalLength += segmentLength;
        }
        if(totalLength.As(LengthUnit.Meter) !=
                this.getComponentLength().As(LengthUnit.Meter)
                ){
            string errorMsg = "The total length in your lengthList \n";
            errorMsg += totalLength.ToString() + "\n";
            errorMsg += "is not equal to the pipe length: \n";
            errorMsg += this.getComponentLength().ToString() + "\n";
            throw new InvalidOperationException(errorMsg);
        }

        this._lengthList = value;
    }
}
```
The lengthList can never be set to null or a value where the sum of 
lengths isn't equal to the total length.


Basically the code throws an error if the lengthList



##### 4. When zero segments are given, throw an error:

When the numberOfSegments is set by default, it will throw an 
exception if the value is 0 or less.

A divide by zero error will be thrown.
```csharp
public virtual int numberOfSegments { 
    get {
        return this._numberOfSegments;
    }
    set {
        if(value <= 0)
            throw new DivideByZeroException("numberOfSegments <= 0");
        this._numberOfSegments = value;
        this.setLengthListUniform(value);
    }
}
```

##### 5. When one segment is given, the segment Length must equal component Length

This is a special case of the general set of cases where i test if
the length of the pipe is equal to the sum total of the segment lengths.
Or else an exception is thrown.

I will not code an entirely new exception for this, but rather, i'll
test it in a unit test.

Here is the unit test:

```csharp
[Theory]
[InlineData(5,1.0)]
[InlineData(50,10.0)]
[InlineData(70,1.0)]
[InlineData(1,2.0)]
public void WhenNumberOfNodesSetLengthEqual1dividebyNoOfSegments(
        int numberOfSegments, double componentLength){

    // Setup
    TherminolPipe testPipe = 
        new mockTherminolPipe("mockTherminolPipe", "0","out");

    testPipe.componentLength = new Length(componentLength, LengthUnit.Meter);
    testPipe.numberOfSegments = numberOfSegments;
    Length expectedLength = testPipe.componentLength/numberOfSegments;
    // now let's retrieve the length list

    IList<Length> testPipeLengthList = new List<Length>();

    foreach (Length segmentLength in testPipe.lengthList)
    {
        testPipeLengthList.Add(segmentLength);
    }

    // so let me just get the first length off this list
    Length firstLength = testPipeLengthList[0];

    // Act

    // then i'll go through a for loop whether the legnths are
    // equal, if equal i will add to an integer known as the checksum
    // if the interger in the checksum is equal to the 
    // number of nodes, then the test passes
    //

    foreach (Length segmentLength in testPipeLengthList)
    {
        // now i know for each length i'm not supposed to use
        // so many assert.Equal in one test
        // but i want the test to fail if even one of the lengths 
        // isn't equal, so that's why i do it this way
        // the lazy way
        Assert.Equal(expectedLength.As(LengthUnit.Meter),
                segmentLength.As(LengthUnit.Meter));
    }
    // Assert
    //
}
```

The last theory test asserts this case. 

#### Pipe Diameter Segment Code and tests

#### 1. Segement Diameters AutoCalculated when Length List is set
```csharp
set{

    // let's do a null check first:
    if (value is null){
        throw new NullReferenceException("lengthList set to null");
    }

    // first i want to check if the 
    // total segment length is equal to the componentLength

    Length totalLength = new Length(0.0, 
            LengthUnit.Meter);
    foreach (Length segmentLength in value)
    {
        totalLength += segmentLength;
    }
    if(totalLength.As(LengthUnit.Meter) !=
            this.getComponentLength().As(LengthUnit.Meter)
            ){
        string errorMsg = "The total length in your lengthList \n";
        errorMsg += totalLength.ToString() + "\n";
        errorMsg += "is not equal to the pipe length: \n";
        errorMsg += this.getComponentLength().ToString() + "\n";
        throw new InvalidOperationException(errorMsg);
    }

    this._lengthList = value;
    this.generateHydraulicDiameterList();
}
```

I have altered the lengthList set functions to invoke a script called
generateHydraulicDiameterList whenever set.

#### 2. Segment diameter interpolation 

The interpolation scheme now becomes kind of tricky; how can i do
the diameter interpolation?

I first need to find the diameter at both entrance and exit. These
are properties which should be implemented in therminol pipe, either
as abstract properties or virtual properties otherwise. 

Now once i have these entrance and exit hydraulic diameters, what do i 
do?

Let's say i have 10 segments, and want to find the length of the first.

$$L_{segment} = \frac{L_{total}}{n_{segments}}$$

At midpoint, i will be having the length from the last node:

$$L_{segmentI} = n_i*L_{segment} - 0.5 L_{segment}$$

I will have a function to return this, note that i factorised out
the segment Length.

```csharp
private Length getSegmentInterpolationLength(int segmentNumber){
    return this.getComponentLength()/this.numberOfSegments 
        *(segmentNumber - 0.5);
}
```

The next part is to implement this equation:
$$D_i = \frac{D_{exit} - D_{0}}{L_{series} - L_{0}} (L_i - L_{0})
+D_{0}$$

The following properties and methods implement this:
```csharp

public abstract Length entranceHydraulicDiameter { get; set; }

public abstract Length exitHydraulicDiameter { get; set; }

private Length getSegmentInterpolationLength(int segmentNumber){
    return this.getComponentLength()/this.numberOfSegments 
        *(segmentNumber - 0.5);
}

private Length getSegmentLinearInterpolatedDiameter(int segmentNumber){
    double interpolationSlope;
    interpolationSlope = (this.exitHydraulicDiameter 
            - this.entranceHydraulicDiameter)/(
                this.getComponentLength() - 
                this.entranceLengthValue);

    Length interpolationLength = this.getSegmentInterpolationLength(
            segmentNumber);

    return (interpolationLength - 
            this.entranceLengthValue)*interpolationSlope
        + entranceHydraulicDiameter;

}

public virtual Length entranceLengthValue { get; set; } = 
    new Length(0.0, LengthUnit.Meter);
    
```

I force the user to define the entrance and exit hydraulic diameter,
and then i use the method getSegmentLinearInterpolatedDiameter to help
calculate the diameter given a segment number.




#### Verification Methods

Suppose this method has been conceived, how then do we test it?

We must create a pipe object, split it up into 10 segments, and
then we have an uneven temperature profile across this pipe.
Then calculate the mass flow terms given a pressure loss iteratively
using the MathNet Libraries. 

This will be a reference flowrate.

The nodalised pipe will use the Be vs Re method at an isothermal 
temperature. However, it will then try to guess the pressure loss
term using the quantities to nondimensionalise and redimensionalise
the Be and Re.

This will be the equations used above.

I will include the full set of equations in here






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
