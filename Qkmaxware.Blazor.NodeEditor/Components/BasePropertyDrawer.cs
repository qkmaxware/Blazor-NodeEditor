using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Qkmaxware.Blazor.NodeEditor.Components {

public abstract class BasePropertyDrawer : ComponentBase {
    [Parameter] public object Instance {get; set;}
    [Parameter] public PropertyInfo Property {get; set;}
}

}