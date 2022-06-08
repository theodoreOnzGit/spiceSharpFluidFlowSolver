# pipesAndValves Readme

## Code Design Philosophy

For this pipes and valves code
its purpose is ultimately
to act as a component that can be put into any 
regular circuit which is an IEntityCollection.

### Solid Principles

As far as possible, i will be using solid principles,
the dependency injection pattern and loose coupling
as well as the factory pattern to instantiate 
objects.

### Procedural and Functional Programming

However, solid principles are sometimes 
[overkill](https://youtu.be/IRTfhkiAqPw)
for code that does not need to change. In fact
overdoing it can 
[obfuscate](https://youtu.be/QM1iUe6IofM) the code.

Therefore it's necessary to assess the strengths
and weaknesses of each programming style to suit
them for the task at hand.

#### Strengths and Weaknesses of Each Programming Style
I paraphrase a lot from this site:

[https://scoutapm.com/blog/
functional-vs-procedural-vs-oop](https://scoutapm.com/blog/functional-vs-procedural-vs-oop)

For Object Oriented Programming(OOP):

What it's good for
1. Reusability: Classes and objects can be reused 
regularly without rewriting the same code over
and over, eg. inheritance
2. Security: using access modifiers in classes
eg. private, we can prevent the user from
altering certain variables that break the code
3. Maintenance: supposing that you are already 
familiar with the code, it's easy to swap out
one dependency for another using dependency
injection and loose coupling

What it's bad for
1. Extensive planning for classes, interfaces and 
how they depend on each other
2. OOP takes much longer to read if you keep
needing to jump between 
parts of the code. (spicesharp is a good example)


For Procedural Programming

What it's good for
1. intuitive to read
2. easy to track program flow

what it's bad for:
1. you cannot really reuse the code here
2. therefore hard to scale up
3. maintenance is a nightmare if you need to 
change parts of the code

For functional programming, we minimise the use of 
classes, and we grant immutability to variables as 
far as possible. This means that instead of 
changing the value of a variable, we try to declare
new ones so as to make debugging easier.


what it's good for:

1. Reliability: pure functions are easy to test
it doesn't rely on global variables, just inputs
and outputs.
2. information is explicitly passed through
into the function, not as the result of some
event or whatsoever.
3. variables are calculated only when needed
you don't pre-calculate values you don't use
like for objects etc.


What it's bad for:

1. recursion (ie executing a function as long as a 
condition is met) is pretty unituitive
2. immutability has reduced performance especially
since we keep allocating memory for new variables
to be created
3. immutability is not quite compatible with 
continuously running processes. eg. servers
and realtime digital twin simulations.
















