using UnityEngine;

public class CountrySelector : MonoBehaviour
{
    public GameObject brFlag;
    public GameObject inFlag;
    public GameObject keFlag;
    public GameObject seFlag;
    public GameObject usFlag;

    public void SelectCountry(string country)
    {
        GameManager.Instance.country = country;

        brFlag.SetActive(country == "Brazil");
        inFlag.SetActive(country == "India");
        keFlag.SetActive(country == "Kenya");
        seFlag.SetActive(country == "Sweden");
        usFlag.SetActive(country == "USA");
    }
}
