# MixerFrictionFactor Readme

This folder is meant to contain friction factor classes for mixers,
considering entrance effects and exit effects also. 

The first of these mixers will be the the static mixer MX-10
from the compact integral effects test (CIET). 



## Design

MX-10 has a relation of the following:

$$K + f_{Darcy} \frac{L}{D} = 21 + \frac{4000}{Re}$$

Equivalently, if one were to write in terms of fanning friction factors

$$K + f_{Fanning} \frac{4L}{D} = 21 + \frac{4000}{Re}$$

In the laminar region since 

$$f_{Darcy} = \frac{64}{Re}$$
$$f_{Fanning} = \frac{16}{Re}$$
$$f_{Darcy} = 4 f_{fanning}$$

It has a hydraulic diameter of 2.79e-2 m and an area of 6.11e-4 $m^2$.
These numbers will be helpful in obtaining the Reynold's numbers
required for calculation here. 
[[1]](#zweibaumDissertation)

The generic component formula used is:

$$\Delta P = \frac{1}{2} \rho u^2 * (f_{fanning} \frac{4L}{D} +K)$$

Recall that for pipes we nondimensionalised the following:
$$f_{fanning} = \frac{\Delta P}{ (\frac{4L}{D}) \  \frac{1}{2} \rho u^2 }$$

$$f_{fanning} (Re) Re^2 = \frac{\Delta p}{ (\frac{4L}{D}) \  
\frac{1}{2}  (\frac{ \nu^2}{ D^2}) }$$

$$f_{fanning}(Re) Re^2 = \frac{32 Be}{ (\frac{4L}{D})^3 
}$$

Here we have the Bejan number defined as:

$$Be = \frac{\Delta p L^2}{\nu^2}$$

Now this means of nondimensionalising only applies well for pipes without
any entrance effects taken into account.

We'll have to make some adjustments to make this work for generic components.

For pipes we have:
$$f_{fanning} (Re) Re^2 = \frac{\Delta p}{ (\frac{4L}{D}) \  
\frac{1}{2}  (\frac{ \nu^2}{ D^2}) }$$

I'm now going to move the 4L/D term to the LHS:

$$f_{fanning} (Re) (\frac{4L}{D})Re^2 = \frac{\Delta p}{  \  
\frac{1}{2}  (\frac{ \nu^2}{ D^2}) }$$

And now i'm going to add back the K term so that

$$[f_{fanning} (Re) (\frac{4L}{D}) + K ]Re^2 = \frac{\Delta p}{  \  
\frac{1}{2}  (\frac{ \nu^2}{ D^2}) }$$


$$[f_{fanning} (Re) (\frac{4L}{D}) + K ]Re^2 = 2 \frac{\Delta p D^2}{  \  
\nu^2 }$$

We can also defined a Bejan number here as:

$$Be_D = \frac{\Delta p D^2}{\nu^2}$$

It is just convenient for us to work with nondimensionalised pressure drop
Be. The length scale is not the length of the pipe now, but rather the length
scale of hydraulic diameter.

$$[f_{fanning} (Re) (\frac{4L}{D}) + K ]Re^2 = 2 Be_D$$

From this we can once again obtain gradients and correlations for dB_dRe
and also find a specific Reynold's number given a Bejan number.

It would be good to make an interface which returns the fLDK term, which
i will just call fLDK in such an interface. 

The fLDK method will be independent of the Reynold's number used. 

There will be two methods:

The generic_fLDK, and fLDK. fLDK method will use the component specific
parameters, while generic fLDK will allow the user to specify all the
parameters. However, the fLDK has a friction factor dependent on the
churchill correlation of f.

Here for friction factor, we need Re, roughness ratio, L/D. ANd then we need 
extra K term. The interface will be known as IfLDKFactor.

# Bibliography

<a id=zweibaumDissertation">
[1]
Zweibaum, N. (2015). Experimental Validation of Passive Safety System Models: Application to Design and Optimization of Fluoride-Salt-Cooled, High-Temperature Reactors. University of California, Berkeley.
</a>
