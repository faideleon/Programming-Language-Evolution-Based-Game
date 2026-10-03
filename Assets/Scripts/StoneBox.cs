using UnityEngine;

// Put this on a pushable box to make it a stone box.
// The altar checks for it in phase 3, and stone boxes are heavier to carry.
public class StoneBox : MonoBehaviour
{
    // How fast the player walks while carrying this box (wooden boxes: 2)
    public float carrySpeed = 1.2f;
}
