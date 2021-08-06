using System;
using System.Collections.Generic;
using System.Linq;

namespace Qkmaxware.Blazor.NodeEditor {

public class Graph<Node, Edge> where Edge:class {
    private class EdgeLink {
        public int EndpointIndex;
        public Edge Data;
    }
    private List<Node> nodes = new List<Node>();
    private List<List<EdgeLink>> edges = new List<List<EdgeLink>>();

    public void Clear() {
        this.nodes = new List<Node>();
        this.edges = new List<List<EdgeLink>>();
    }
    public void AddNode(Node node) {
        // Add the node
        this.nodes.Add(node);
        // Add an empty list of edges
        this.edges.Add(new List<EdgeLink>());
    }

    public void RemoveNode(Node node){
        var index = this.nodes.IndexOf(node);
        if (index >= 0) {
            // Delete this node and its outgoing edges
            this.nodes.RemoveAt(index);
            this.edges.RemoveAt(index);
            
            // Delete all inbound edges from other nodes (updating the indexes of subsequent connections)
            for (var startNodeIndex = 0; startNodeIndex < edges.Count; startNodeIndex++) {
                var outboundEdges = edges[startNodeIndex];
                outboundEdges.RemoveAll(edge => edge.EndpointIndex == index);
                foreach (var edge in outboundEdges) {
                    if (edge.EndpointIndex > index) {
                        edge.EndpointIndex--;
                    }
                }
            }
        }
    }
    public IEnumerable<T> GetNodesOfType<T>() {
        return this.nodes.OfType<T>();
    }
    public IEnumerable<Node> GetNodes() {
        return nodes.AsReadOnly();
    }
    public IEnumerable<Node> GetNodesFromOutgoingEdges(Node node) {
        var index = nodes.IndexOf(node);
        if (index >= 0) {
            return edges[index].Select(edge => nodes[edge.EndpointIndex]);
        } else {
            return Enumerable.Empty<Node>();
        }
    }
    public IEnumerable<Node> GetNodesFromIncomingEdges(Node node) {
        var index = nodes.IndexOf(node); 
        if (index >= 0) {
            return edges.SelectMany(
                (outbound, outboundIndex) => 
                    outbound
                    .Where(edge => edge.EndpointIndex == index)
                    .Select(edge => nodes[outboundIndex])
                );
        } else {
            return Enumerable.Empty<Node>();
        }
    }
    public virtual bool Connect (Node start, Node end, Edge data = null) {
        var startIndex = nodes.IndexOf(start);
        var endIndex = nodes.IndexOf(end);
        if (startIndex >= 0 && endIndex >= 0) {
            var edge = new EdgeLink {
                EndpointIndex = endIndex,
                Data = data,
            };
            this.edges[startIndex].Add(edge);
            return true;
        } else {
            return false;
        }
    }
    public bool Disconnect(Node start, Node end) {
        var startIndex = nodes.IndexOf(start);
        var endIndex = nodes.IndexOf(end);
        if (startIndex >= 0 && endIndex >= 0) {
            var removed = edges[startIndex].RemoveAll(edge => edge.EndpointIndex == endIndex);
            return removed > 0;            
        } else {
            return false;
        }
    }
    public void DisconnectAll(Func<Node, Node, Edge, bool> matcher) {
        for (var i = 0; i < edges.Count; i++) {
            edges[i].RemoveAll((link) => matcher(nodes[i], nodes[link.EndpointIndex], link.Data));
        }
    }
    public Edge GetEdgeData(Node start, Node end) {
        return GetAllEdgeData(start, end).FirstOrDefault();
    }
    public IEnumerable<Edge> GetAllEdgeData(Node start, Node end) {
        var startIndex = nodes.IndexOf(start);
        var endIndex = nodes.IndexOf(end);
        if (startIndex >= 0 && endIndex >= 0) {
            return this.edges[startIndex].Where(edge => edge.EndpointIndex == endIndex).Select(edge => edge.Data);
        } else {
            return Enumerable.Empty<Edge>();
        }
    }
}

public class Graph<Node> : Graph<Node, object> {}

}