using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public TextAsset jsonFile; //variable to read from json file

    public Text TextBox; //variable to report additional information from States object in text box

    public int CurrentConfirmed;

    public int CurrentDeaths;


    // Start is called before the first frame update
    void Start()
    {
        var dropdown = transform.GetComponent<Dropdown>(); //instantiates variable for dropdown text

        //dropdown.options.Clear();

        States statesInJson = JsonUtility.FromJson<States>(jsonFile.text);

        foreach (State state in statesInJson.states)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = state.Province_State });
        }

        DropdownItemSelected(dropdown);

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); }); //delegate is invoked when dropdown menu is changed

    }

    void DropdownItemSelected(Dropdown dropdown) //displays the text when the dropdown item is selected
    {
        int index = dropdown.value;

        string dropdownLabel = dropdown.options[index].text; //stores the string value of the dropdown index

        SetCurrentSelections(dropdownLabel); //calls the SetCurrentSelections void method using a string variable

        if(dropdownLabel == "None") //hides the descriptive right-hand menu if nothing is selected
        {
            TextBox.text = null;
        } else { 
            TextBox.text = "Confirmed: " + CurrentConfirmed + "\n" + "Deaths: " + CurrentDeaths; //shows available totals if a selection besides "None" is made
        }
    }

    void SetCurrentSelections(string currentState) //takes a string and searches the States object for a match
    {
        States statesInJson = JsonUtility.FromJson<States>(jsonFile.text);

        foreach (State state in statesInJson.states)
        {
            if (currentState == state.Province_State)
            {
                CurrentConfirmed = state.Confirmed; //if a match is found, update public variables
                CurrentDeaths = state.Deaths;
            }
        }
    }
    
}
