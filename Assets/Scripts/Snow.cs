using System.Collections.Generic;
using UnityEngine;

public class Snow : MonoBehaviour
{
    private HashSet<Enemy> enemiesInSnow = new HashSet<Enemy>();

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemiesInSnow.Add(enemy);
            enemy.EnterSnow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemiesInSnow.Remove(enemy);
            enemy.ExitSnow();
        }
    }

    private void OnDestroy()
    {
        foreach (var enemy in enemiesInSnow)
        {
            if (enemy != null)
            {
                enemy.ExitSnow();
            }
        }
        enemiesInSnow.Clear();
    }
}
