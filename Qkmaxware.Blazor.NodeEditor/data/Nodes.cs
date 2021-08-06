using System;
using System.Linq;

namespace Qkmaxware.Blazor.NodeEditor {

public struct Point {
    public int X;
    public int Y;
}

public class BaseNode {
    public string Name;
    public string Description;
    public bool Collapsed = false;
    public Point Position;
}

}