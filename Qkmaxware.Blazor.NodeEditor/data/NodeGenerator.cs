namespace Qkmaxware.Blazor.NodeEditor {

public interface INodeGenerator<NodeType> {
    string GeneratorName();
    NodeType Generate();
}

}