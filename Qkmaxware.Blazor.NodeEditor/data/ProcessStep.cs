using System;
using System.Linq;
using System.Collections.Generic;

namespace Qkmaxware.Blazor.NodeEditor {

public class NodePort {
    public string Name;
    public bool HasValue {get; private set;}
    private object value;
    protected virtual bool CanStore(object value) {
        return true;
    }
    public virtual Type GetStorageType() => typeof(object); 
    public virtual bool CanStore (Type t) {
        return true;
    }
    public void Store(object value) {
        if (CanStore(value)) {
            this.value = value;
            this.HasValue = true;
        }
    }
    public object Fetch() {
        return value;
    }
    public R Fetch<R>() {
        if (!HasValue)
            return default(R);
        
        if (value is R rvalue) {
            return rvalue;
        } else {
            return default(R);
        }
    }
    public void Clear() {
        this.HasValue = false;
        this.value = null;
    }
}

public class NodePort<T> : NodePort {
    protected override bool CanStore(object o) {
        return o is T;
    }
    public override Type GetStorageType() => typeof(T);
    public override bool CanStore (Type t) {
        return typeof(T).IsAssignableFrom(t);
    }
}

public class NodePortCollection {
    public int Size => ports.Count;
    private List<NodePort> ports = new List<NodePort>();
    public NodePort this[string name] => ports.Where(port => port.Name == name).FirstOrDefault();
    public NodePort this[int index] => ports[index];

    public NodePortCollection() {}
    public NodePortCollection(IEnumerable<NodePort> ports) {
        this.ports = ports.ToList();
    }
    public int IndexOf(string port) {
        for (var i = 0; i < ports.Count; i++) {
            if (ports[i].Name == port)
                return i;
        }
        return -1;
    }
    public IEnumerable<NodePort> Enumerate() {
        foreach (var port in ports)
            yield return port;
    }
}

public abstract class ProcessStep : BaseNode {
    public NodePortCollection Inputs;
    public NodePortCollection Outputs;

    /// <summary>
    /// Check if this node has already computed and cached its outputs
    /// </summary>
    /// <returns>True if the outputs are already computed, false otherwise</returns>
    public virtual bool HasCachedOutput() {
        if (Outputs != null) {
            foreach (var output in Outputs.Enumerate()) {
                if (!output.HasValue)
                    return false;
            } 
        }
        return true;
    }
    /// <summary>
    /// Reset the inputs and outputs clearing all computed values
    /// </summary>
    public void Reset() {
        if (Inputs != null) {
            foreach (var input in Inputs.Enumerate()) {
                input.Clear();
            }
        }
        if (Outputs != null) {
            foreach (var output in Outputs.Enumerate()) {
                output.Clear();
            }
        }
    }
    /// <summary>
    /// Force the node to recalculate all of its outputs
    /// </summary>
    public abstract void Recalculate();
}

public abstract class ParametereizedProcessStep : ProcessStep {
    public abstract object GetParametres();
}
public abstract class ParametreizedProcessStep<ParamType> : ParametereizedProcessStep where ParamType:class {
    public ParamType Parametres {get; set;}
    public override object GetParametres() => Parametres;
}

}