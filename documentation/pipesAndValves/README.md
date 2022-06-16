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


Functional and Procedural programming are sometimes simpler,
and for some users, preferred in comparison to OOP.

Therefore, it's necessary to assess the strengths
and weaknesses of each programming style to suit
them for the task at hand.

### Strengths and Weaknesses of Each Programming Style
Given these differing schools of thought,
it's necessary to assess the strengths
and weaknesses of each programming style to suit
them for the task at hand.

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


### KISS Principle

It stands for keep it simple, stupid. While I do not
 endorse calling people stupid like that, I do like the principle of 
keeping it simple.

#### Default: Procedural Programming
The simplest form of programming is procedural programming.

So it should be used by default.

#### Extensibility and Maintainability: OOP

However, I do expect that the code can be patched and extended. This
almost requires that object oriented by nature. 

I expect the code to go through several iterations.
So using interfaces here will be useful; i am just concerned with input
and output of my code.

The concretisation will differ from iteration to iteration.

#### Reusability and Modularity: Functional Programming
I also want certian parts
of my code to be reusable. This means that parts of my code can be 
functional.

#### Application: Iterative Programming with Interfaces and Factory Pattern

Since the code will be iterative, I will indeed have several different
versions of my pipe code.

Each pipe code will already scaffold off the existing customResistor
code structure.

And there is already an existing interface for the components, eg. IEntity.

So I do not create a new interface.

However, I make several new implementations of this interface. Thus, the
concretisation of IEntity is loosely coupled from IEntityCollection.

However, to make the concretisation of the entity, I will then use the
factory pattern to decide which implementation of the code i will
need to use. Thus the factory can create the object and decide
which implmentation is going to be used from some sort of dropdown
menu, enum or dictionary.

The factory class will not be abstracted unless necessary.

## Pipe Object Factory Class Design

For the factory class, I will allow some extension in case the user
wants to add custom pipes.

So I will have an abstract class which will be contained in the
OfficialPipeFactory.cs I call it BasePipeFactory. 
I will also have OfficialPipeFactory here It will inherit from
BasePipeFactory and call the base constructor, and do nothing more.

And also another class called TemplateCustomPipeFactory.cs. Which is
for the user to add their own  pipe types and models to this PipeFactory.

Later on, i will also want to construct various valves from this, bends
etc. So those classes will inherit from the BasePipeFactory.

Abstract classes can be used like interfaces. But they cannot be 
instantiated. I intend to use base abstract classes like interfaces.

### How the User will interact
I will have a few public methods:

1. Component returnPipe(String pipeType) 
2. getList()

returnPipe will just return the Component Type. It is an abstract class 
which functions almost like an interface. Except that it has actual methods
we can inherit from. 

getList will just return a list of valid pipeTypes.

The next thing we want the use to know is if we supply the wrong pipeType,
then an exception is thrown. And I want the exception message to tell the 
user, which pipeTypes are valid.

I also don't want the pipetype to be case sensitive. So that will be 
covered in how the if loop is designed.


### under the hood

#### Constructor
The moment you construct the pipe, a dictionary is instantiated.
Dictionaries will implement the IEnumerable interface so that For
each loops can be used.

This dictionary will be a private variable.

I will have the constructor call methods that add key value pairs to the
dictionary. Everytime a new implementation is added, i will just add
additional lines which add new key value pairs to this dictionary.

Thus, the factory will have a only a single responsibility.

It will not have open close principle here, rather, KISS principle 
overrides it. 

Nevertheless, to make it backwards compatible, i will only add lines to
this code or debug it. 

An alternative is to define an abstract class with the base constructor.
The abstract class provides the templates for the methods, whereas the user
is free to define the list however he or she likes.

For ease of readability, both abstract class and concrete class will be
placed in the same file.

The child class will call the base class constructor and that will add
a predefined set of key value pairs.

The user will be encouraged to extend the abstract class using child 
classes. But no more.




#### Add methods (both base class and inherited)

The constructor calls add methods to add String and Component Values to
the dictionary. The add method must first create the Component object 
using whatever dependnecies are needed. Then it will be called in
the constructor.

There won't be one add method, but let's say we have a simplePipe 
implementation (no entrance or eleveation effects etc).

We can then create an AddSimplePipe() method and create the object there.

#### getList() (base class only)

The getList method returns a list of available implementations the user
can select from the pipeAndValveFactory class.

It will use a ForEach loop to access keyValuePairs within the dictionary.
Then Console.Writeline those pairs out.

#### returnPipe(String pipeType) (base class only)

The method will just use the TryGetValue method within the dictionary and
return the pipe object.

Failing which, an exception will be thrown.

The exception message will tell the user a list of the valid inputs.
We can use the getList() function. Though that only does the console 
writeline.


#### generateList() (base class only, called in base constructor)
The alternative here is that a class called generateList will populate
a private string variable with the list of variables within the 
dictionary. 

Then both getList and the error message will use this private string 
variable to print to the user.


### Limitations

I am NOT designing this Officialfactory to be 
inherited or modified unless a new pipeType is given.


However, the abstract basePipeFactory can be inherited. And can be used
for templating valves and bend components in future.

### Tests

For this, i will create a mock pipe class. 
The mock pipe class will be exactly the same code as the custom resistor
class. 

I will put that mock pipe class into a circuit. Run the circuit in 
Operating Point mode. And then call it a day.

## Pipe Class Design

I intend my pipe class to be iteratively designed. Going from simple
pipe models to complex pipe models I intend to have many many
implementations of the pipe class.



























