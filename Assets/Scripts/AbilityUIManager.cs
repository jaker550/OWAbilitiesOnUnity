using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIManager : MonoBehaviour
{
    public static AbilityUIManager Instance; // Singleton instance
    public PlayerAbilityController playerAbilityController;
    public Image canvImage1;
    public Image canvImage2;
    public Image canvImageBg1;
    public Image canvImageBg2;

    private void Start()
    {
        canvImage1 = GameObject.Find("AbilityIcon1").GetComponent<Image>();
        canvImage2 = GameObject.Find("AbilityIcon2").GetComponent<Image>();

        canvImageBg1 = GameObject.Find("AbilityBg1").GetComponent<Image>();
        canvImageBg2 = GameObject.Find("AbilityBg2").GetComponent<Image>();

        playerAbilityController = GameObject.FindWithTag("Player").GetComponent<PlayerAbilityController>();
        if(playerAbilityController != null)
        {
            canvImage1.sprite = playerAbilityController.ability1.abilityIcon;
            canvImage2.sprite = playerAbilityController.ability2.abilityIcon;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Ability1Press());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Ability2Press());
        }
    }

    private IEnumerator Ability1Press()
    {
        canvImageBg1.color = new Color(1, 0, 0);
        yield return new WaitForSeconds(playerAbilityController.ability1.abilityDuration);
        canvImageBg1.color = new Color(0.5f, 0.5f, 0.5f);
    }

    private IEnumerator Ability2Press()
    {
        canvImageBg2.color = new Color(1, 0, 0);
        yield return new WaitForSeconds(playerAbilityController.ability2.abilityDuration);
        canvImageBg2.color = new Color(0.5f, 0.5f, 0.5f);
    }
}
