using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace spiceSharpFluidFlowSolverLibraries;
public abstract class BasePipeFactory
{
	private Dictionary<String,Component> pipeDictionary;
	private String _pipeName;

	// constructor here
	// in order for this to work, we must specify the pipeName
	public BasePipeFactory(String pipeName)
	{
		this.pipeDictionary = new Dictionary<String,Component>();
		this._pipeName = pipeName;
		
		// now we start adding components
		this.AddMockPipeCustomResistor();

	}

	public BasePipeFactory(String pipeName,String inletName, String outletName)
	{
		this.pipeDictionary = new Dictionary<String,Component>();
		this._pipeName = pipeName;

		// now we start adding components
		this.AddMockPipeCustomResistor(inletName,outletName);

	}

	// here are my other methods
	// I use virtual on all of them because
	// I want to be able to overrride them
	// virtual means that classes can be overridden but need
	// not be

	public virtual string getList(){
		string listOfComponents;
		listOfComponents = this.generateList();
		Console.WriteLine(listOfComponents);
		return listOfComponents;
	}

	// this method generates a list of components that can be
	// used to return the list

	public string generateList(){
		string listOfComponents;
		// first i clean up the list
		listOfComponents = "";

		// second i give a welcome message
		//
		listOfComponents += "\n";
		listOfComponents += "***************************************\n";
		listOfComponents += "Here is a list of Valid pipeTypes \n";
		listOfComponents += "***************************************\n";
		listOfComponents += "\n";

		foreach (var keyValuePair in this.pipeDictionary)
		{
			listOfComponents += keyValuePair.Key + "\n";
		}

		listOfComponents += "\n";
		listOfComponents += "***************************************\n";
		listOfComponents += "***************************************\n";
		listOfComponents += "\n";

		return listOfComponents;

	}

	public virtual Component returnPipe(string pipeType){
		foreach (var keyValuePair in this.pipeDictionary)
		{
			if (String.Equals(keyValuePair.Key.ToLower(), pipeType.ToLower()))
			{
				return keyValuePair.Value;
			}
		}
		// if everything else fails, throw an error
		// I will generate a list of valid pipeTypes first
		string listOfComponents;
		listOfComponents = this.generateList();
		string errorMsg;
		errorMsg = "";
		errorMsg += "\n";
		errorMsg += "Your pipeType :" + pipeType + " doesn't exist \n";
		errorMsg += "Please consider using pipeTypes \n from the following list";
		errorMsg += listOfComponents;
		throw new InvalidOperationException(errorMsg);
	}	


	// here is where I start constructing pipes
	// oh dear, i'm trying to construct objects even before knowing
	// the name
	// i will just need to add the name at the constructor
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
}
