using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DestinationGoal : MonoBehaviour
{
    public NavigationManager manager;

    private DestinationTrigger dest;

    void Awake()
    {
        dest = GetComponent<DestinationTrigger>();
    }

   void OnTriggerEnter(Collider other)
{
    if (manager == null || dest == null) return;

    if (other.CompareTag("player"))
    {
        manager.NotifyReachedDestination(dest);
    }
}


}
