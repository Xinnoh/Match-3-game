using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EnemyHealthManager : MonoBehaviour
{
    private EnemyManager enemyManager;

    public int maxHealth = 1000;

    public int currentHealth;

    public TextMeshProUGUI healthText;
    public Slider healthSlider;


    [SerializeField] float sliderLerpSpeed = 8f;
    float targetHealth;

    Coroutine sliderRoutine;

    // Start is called before the first frame update
    void Start()
    {
        enemyManager = GetComponent<EnemyManager>();
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;

        if (sliderRoutine != null)
            StopCoroutine(sliderRoutine);

        sliderRoutine = StartCoroutine(LerpSlider(currentHealth));

        if (currentHealth <= 0)
            enemyManager.Die();
    }



    public void WriteHealth()
    {

    }

    private IEnumerator LerpSlider(int target)
    {
        float start = healthSlider.value;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * sliderLerpSpeed;
            healthSlider.value = Mathf.Lerp(start, target, t);
            yield return null;
        }

        healthSlider.value = target;
    }


}
