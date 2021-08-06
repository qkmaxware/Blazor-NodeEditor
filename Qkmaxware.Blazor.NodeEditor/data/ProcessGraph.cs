using System;
using System.Collections.Generic;
using System.Linq;

namespace Qkmaxware.Blazor.NodeEditor {

public class NodePortReference {
    public string FromPortName;
    public string ToPortName;
}

public class ProcessGraph : Graph<ProcessStep, NodePortReference> {
    public IEnumerable<ProcessStep> GetOutputs() {
        return this.GetNodes().Where(node => node.Outputs == null || node.Outputs.Size == 0);
    }
    public IEnumerable<T> GetOutputsOfType<T>() {
        return this.GetOutputs().OfType<T>();
    }   

    public bool Connect(ProcessStep start, NodePort outputPort, ProcessStep end, NodePort inputPort) {
        return Connect(start, end, new NodePortReference {
            FromPortName = outputPort.Name,
            ToPortName = inputPort.Name,
        });
    }
    public override bool Connect (ProcessStep start, ProcessStep end, NodePortReference data) {
        // Validation
        if (data == null) {
            // Verify port mapping exists
            throw new ArgumentException("Missing port connection references");
        }
        if (string.IsNullOrEmpty(data.FromPortName)) {
            // Verify port mapping is valid
            throw new ArgumentException("Missing start node's output port name");
        }
        if (string.IsNullOrEmpty(data.ToPortName)) {
            // Verify port mapping is valid
            throw new ArgumentException("Missing end node's input port name");
        }
        var outputPort = start.Outputs[data.FromPortName];
        if (outputPort == null) {
            // Verify port exists
            throw new ArgumentException($"Start node doesn't have an output port named '{data.FromPortName}'");
        }
        var inputPort = end.Inputs[data.ToPortName]; 
        if (inputPort == null) {
            // Verify port exists
            throw new ArgumentException($"End node doesn't have an input port named '{data.ToPortName}'");
        }
        if (!inputPort.CanStore(outputPort.GetStorageType())) {
            // Verify output port is compatible with input port
            throw new ArgumentException($"Cannot connect port output of type {outputPort.GetStorageType()} to one accepting {inputPort.GetStorageType()}");
        }

        // Clean up extra connections to the same port
        this.DisconnectAll((linkStart, linkEnd, mapping) => linkEnd == end && mapping.ToPortName == data.ToPortName);

        // Return
        return base.Connect(start, end, data);
    }

    private void recompute(ProcessStep node) {
        if (node == null)
            return;
        
        // Compute this node's inputs (recursively)
        var inputs = this.GetNodesFromIncomingEdges(node);
        foreach (var input in inputs) {
            if (!input.HasCachedOutput()) {
                recompute(input);
            }
        }

        // Set the inputs of the current node
        foreach (var input in inputs) {
            foreach (var edge in this.GetAllEdgeData(input, node)) {
                var inputPort = node.Inputs[edge.ToPortName];
                var outputPort = input.Outputs[edge.FromPortName];
                if (inputPort != null && outputPort != null) {
                    inputPort.Store(outputPort.Fetch());
                }
            }
        }

        // Compute the node's value
        node.Recalculate();
    }

    public void Recalculate(ProcessStep step) {
        // Clear old results
        foreach (var node in this.GetNodes()) {
            node.Reset();
        }
        
        recompute(step);
    }
    public void RecalculateAll() {
        // Clear old results
        foreach (var node in this.GetNodes()) {
            node.Reset();
        }

        // Travel graph computing new results
        foreach (var node in GetOutputs()) {
            recompute(node);
        }
    }
}

}