using System;
using Qkmaxware.Blazor.NodeEditor;
using Test.Shared;

namespace Test.Data {

public class SimpleNodeGenerator : INodeGenerator<ProcessStep> {
    private string name;
    private Func<ProcessStep> constructor;
    public SimpleNodeGenerator(string name, Func<ProcessStep> constructor) {
        this.name = name;
        this.constructor = constructor;
    }
    public string GeneratorName() => name;
    public ProcessStep Generate() => constructor();
}

public class InputNode : ParametreizedProcessStep<InputNode.ParameterSet> {
    public class ParameterSet {
        public float Value {get; set;}
    }
    public InputNode() {
        this.Name = "Input";
        this.Parametres = new ParameterSet();
        
        this.Inputs = null;
        this.Outputs = new NodePortCollection(new NodePort[]{
            new NodePort<float> {
                Name = "value"
            }
        });
    }
    public override void Recalculate() { 
        this.Outputs["value"].Store(this.Parametres.Value);
        Console.WriteLine("Pushing " + this.Parametres.Value);
    }
}

public class OutputNode : ParametreizedProcessStep<OutputNode.ParameterSet> {
    public class ParameterSet {
        [CustomPropertyDrawer(typeof(OutputDrawer))]
        public float Result {get; set;}
    }

    public OutputNode() {
        this.Parametres = new ParameterSet();
        this.Name = "Output";
        this.Inputs = new NodePortCollection(new NodePort[]{
            new NodePort {
                Name = "value"
            }
        });
    }
    public override void Recalculate() {
        this.Parametres.Result = this.Inputs["value"].Fetch<float>();
        Console.WriteLine("Computed Result " + this.Parametres.Result);
    }
}

public class AddNode : ProcessStep {
    public AddNode() {
        this.Name = "Addition";
        this.Inputs = new NodePortCollection(new NodePort[]{
            new NodePort<float> {
                Name = "first"
            },
            new NodePort<float> {
                Name = "second"
            }
        });
        this.Outputs = new NodePortCollection(new NodePort[]{
            new NodePort<float> {
                Name = "value"
            }
        });
    }

    public override void Recalculate() {
        var t1 = this.Inputs["first"].Fetch<float>();
        var t2 = this.Inputs["second"].Fetch<float>();

        this.Outputs["value"].Store(t1 + t2);
        Console.WriteLine("Adding " + t1 + " + " + t2 );
    }
}

}