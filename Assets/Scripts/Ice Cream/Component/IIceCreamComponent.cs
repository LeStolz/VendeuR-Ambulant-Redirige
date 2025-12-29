using System.Collections.Generic;
using UnityEngine;

public interface IIceCreamComponent
{
    public void UpdateIceCreamComponentVisuals(List<GameObject> visuals);
    public void UpdateIceCreamUIVisuals(List<GameObject> uiVisuals, int index);
    public bool CanAdd(List<IIceCreamComponent> currentComponents);
    public void Consume(GameObject gameObject);
    public bool Equals(IIceCreamComponent other);
}
