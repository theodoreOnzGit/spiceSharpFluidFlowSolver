# pipeFactory Readme

## Design

This class is for the user to select a pipe type using a string
and then the returnPipe method will give the pipe object
as type Component.

# BasePipeFactory

Basepipefactory is a class that contains my "official" list
of pipes. If you want to add your own pipes, create another
PipeFactory class and inherit from BasePipeFactory.

See PipeFactory.cs for a template.

## Underlying workings


### Dictionary
BasePipeFactory is dependent on a Dictionary.

```csharp
private Dictionary<String,Component> pipeDictionary;
````

What the user will do is supply a string. And the 
BasePipeFactory class will access the correct pipe
from the dictionary.

### Constructor

The dictionary is constructed in the constructor class.

```csharp
public BasePipeFactory(String pipeName)
{
	this.pipeDictionary = new Dictionary<String,Component>();
	this._pipeName = pipeName;
	
	// now we start adding components
	this.AddMockPipeCustomResistor();
	this.AddBasePipe();

}

```

All you need to do is supply the pipe name.


There is another overload of the constructor
so that you can construct the pipe if you know
the inlet and outlet names.

```csharp
public BasePipeFactory(String pipeName,String inletName, String outletName)
{
	this.pipeDictionary = new Dictionary<String,Component>();
	this._pipeName = pipeName;

	// now we start adding components
	this.AddMockPipeCustomResistor(inletName,outletName);
	this.AddBasePipe(inletName,outletName);

}

```

To add a pipe to the Dictionary, we have AddPipe Methods, eg.

```csharp

private void AddMockPipeCustomResistor(){
	MockPipeCustomResistor mockPipe;
	mockPipe = new MockPipeCustomResistor(this._pipeName);
	pipeDictionary.Add("MockPipeCustomResistor",mockPipe);
	return;
}
```

The pipeDictionary.Add is where we add the string and pipe component
to the dictionary.


### Accessing components

```csharp

foreach (var keyValuePair in this.pipeDictionary)
{
	if (String.Equals(keyValuePair.Key.ToLower(), pipeType.ToLower()))
	{
		return keyValuePair.Value;
	}
}
```

The ToLower() function is to make the method non case sensitive.


### Exceptions

I designed pipeFactory to return a list of valid pipeTypes.

If the user provides a wrong pipeType, 
this class will tell you what pipeTypes are valid:

```csharp
string listOfComponents;
listOfComponents = this.generateList();
string errorMsg;
errorMsg = "";
errorMsg += "\n";
errorMsg += "Your pipeType :" + pipeType + " doesn't exist \n";
errorMsg += "Please consider using pipeTypes \n from the following list";
errorMsg += listOfComponents;
throw new InvalidOperationException(errorMsg);
```

How does the generateList method work?

```csharp

foreach (var keyValuePair in this.pipeDictionary)
{
	listOfComponents += keyValuePair.Key + "\n";
}

```

# PipeFactory

PipeFactory just inherits from basePipeFactory

## constructors
```csharp

public PipeFactory(String pipeName) : base(pipeName)
{
}

```
The constructor in pipeFactory just calls the basePipeFactory
Constructor for both overloads.

If you want to have your own method to add in new dictionary 
entries, simply copy the templates

```csharp

private void AddMockPipeCustomResistor(){
	MockPipeCustomResistor mockPipe;
	mockPipe = new MockPipeCustomResistor(this._pipeName);
	pipeDictionary.Add("MockPipeCustomResistor",mockPipe);
	return;
}

// here is a constructor overload if the person desires to
// construct a custom resistor using already known inlet
// and outlet names
private void AddMockPipeCustomResistor(String inletName, String outletName){
	MockPipeCustomResistor mockPipe;
	mockPipe = new MockPipeCustomResistor(this._pipeName, inletName, outletName);
	pipeDictionary.Add("MockPipeCustomResistor",mockPipe);
	return;
}
```
Change the pipeTypes to include your own custom pipe classes.

