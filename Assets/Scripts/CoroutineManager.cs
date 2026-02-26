using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    // This MonoBehaviour will be used to run coroutines
    public void StartNewCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
