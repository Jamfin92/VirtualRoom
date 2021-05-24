using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public TextAsset jsonFile;

    void Start()
    {
        States statesInJson = JsonUtility.FromJson<States>(jsonFile.text);

        foreach (State state in statesInJson.states)
        {
            Debug.Log("State/Province: " + state.Province_State + " Confirmed Cases: " + state.Confirmed + " Confirmed Deaths: " + state.Deaths);
        }
    }
}