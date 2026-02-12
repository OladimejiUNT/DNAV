using UnityEngine;

public class DestinationTrigger : MonoBehaviour
{
    [Tooltip("Unique ID used by NavigationManager. Example: Radiology, ER, ICU")]
    public string destinationId = "Radiology";
}
