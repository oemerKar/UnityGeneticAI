using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GenCounter : MonoBehaviour
{
    //Singleton
    public static GenCounter instance;

    public int gen = 1;


    [SerializeField] TMP_Text distanceText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

    }

    void Update()
    {
        UIDarstellung();
    }

    void UIDarstellung()
    {
        distanceText.text = (gen.ToString() + ". Generation");
    }
}
