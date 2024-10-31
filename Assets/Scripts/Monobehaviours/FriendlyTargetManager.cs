using System.Collections.Generic;
using UnityEngine;
public class FriendlyTargetManager : MonoBehaviour
{
    public List<FriendlyTarget> friendlyTargets;

    private void Awake()
    {
        //Add all targets to a list
        friendlyTargets = new List<FriendlyTarget>(FindObjectsByType<FriendlyTarget>(FindObjectsSortMode.None));
    }
}