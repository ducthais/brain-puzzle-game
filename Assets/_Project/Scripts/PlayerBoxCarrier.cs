using UnityEngine;

public class PlayerBoxCarrier : MonoBehaviour
{
    public bool isCarryingBox = false;
    public GameObject carriedBoxVisual; // Kéo object CarriedBox trên đầu vào đây

    public void PickUp()
    {
        isCarryingBox = true;
        carriedBoxVisual.SetActive(true);
    }

    public void Drop()
    {
        isCarryingBox = false;
        carriedBoxVisual.SetActive(false);
    }
}