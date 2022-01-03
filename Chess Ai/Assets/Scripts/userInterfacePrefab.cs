using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userInterfacePrefab : MonoBehaviour
{
    private userInterface userInterface;

    public int prefabPieceType;

    // Start is called before the first frame update
    void Start()
    {

        userInterface = GameObject.Find("PieceHandler").GetComponent<userInterface>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        userInterface.GetComponent<userInterface>().type = prefabPieceType;
        userInterface.GetComponent<userInterface>().destroyPromotionUI();
    }
}