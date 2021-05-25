using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using System.Linq;

public class DropdownHandler : MonoBehaviour
{
    public Dictionary<int, string> monthsPool = new Dictionary<int, string>(); //dictionary with int,string pairs of index and full month name
    public List<string> months; //list of months to add to the first dropdown

    public Dictionary<int, State> statesPool = new Dictionary<int, State>(); //collection of State type objects using int, State key/value pairs
    public List<string> statesList; //list of strings to load dropdown_states with

    public Text onScreenDisplay_date;
    public Text onScreenDisplay_stats;
    public MonthDictionary dictionary;
    public JsonFileReader jsonReader;

    [SerializeField]
    private Dropdown dropdown_months; //container for dropdown object to display available months
    [SerializeField]
    private Dropdown dropdown_states; //container for dropdown object to display states depending on selected month

    public GameObject dropdown_controller; //control SetActive() for second dropdown dropdown_states
    public GameObject osd_date; //control SetActive() for text box displaying statistics
    public GameObject osd_stats; //control SetActive() for text box displaying month/year
    public Transform allSeeingEye;

    public GameObject tempNPC; //prefab
    public GameObject instanceObj; //instance of gameobject for destroy()
    [SerializeField] private Material myMaterial;
    //[SerializeField] private Color color;
    public List<Vector3> spawnTotal = new List<Vector3>();
    public List<GameObject> colorObjects = new List<GameObject>();
    public List<GameObject> activeObjects = new List<GameObject>();

    public int monthMenuIndex; //tracks index for first dropdown
    public int stateMenuIndex; //tracks index for second dropdown

    void Awake()
    {
        PopulateMonths();
        dropdown_controller.SetActive(false);
        osd_date.SetActive(false);
        osd_stats.SetActive(false);
        osd_date.SetActive(false);
    }

    void PopulateMonths() //add all 10 months dynamically to the first dropdown menu (dropdown_months)
    {
        dropdown_months.options.Clear();
        GenerateMonths();
        dropdown_months.AddOptions(months);
    }

    void GenerateMonths() //shows the documented months from April 2020-January 2021 in the dropdown menu using the DateTime class
    {
        DateTime monthsFromApr = new DateTime(); //declares DateTime object set at default minimum value (01/01/0001 + min time). For this, we only need to dynamically fill the dropdown with months.
        monthsFromApr = monthsFromApr.AddMonths(3); //starts at April '20, ends in Jan '21 (10 months obs)

        string courteousSelection = "Please select a month."; //default selection inserted at runtime instead of hard "coded" in unity's inspector
        months.Add(courteousSelection);

        for (int i = 0; i < 10; i++)
        {
            string monthToString = monthsFromApr.ToString("MMMM"); //set the string to the current DateTime object's established month (in the form of int) in full month format.
            monthsPool.Add(i, monthToString);
            monthsFromApr = monthsFromApr.AddMonths(1); //increase the month by 1.
        }

        foreach (KeyValuePair<int, string> entry in monthsPool)
        {
            string temp = entry.Value;
            months.Add(temp);
        }
    }

    public void Dropdown_Month_IndexChanged(int location) //When dropdown_months selection changes, if index > 0, generate the states list with JSON data and load the states into the dropdown menu dynamically.
    {
        monthMenuIndex = location;
        SetDropdownActive();
        LoadDropdownOnValueChanged(monthMenuIndex);

        State currentState = new State();

        int adjustedIndex = stateMenuIndex - 1;
        int monthIndex = monthMenuIndex - 1;
        if(adjustedIndex >= 0 && monthIndex >= 0)
        {
            currentState = statesPool[adjustedIndex];
            SetOnScreenDisplay(currentState);
            ClearSpawnLists();
            BuildSpawnListsAndGrid();
            AdjustCameraPos();
        }
        else
        {
            ClearSpawnLists();
            osd_date.SetActive(false);
            osd_stats.SetActive(false);
        }
    }

    public void SetDropdownActive() //toggles the rendering of the second dropdown based on the index of the first
    {
        bool setDropdownActive;
        setDropdownActive = indexGreaterThanZero(monthMenuIndex);
        if (setDropdownActive)
        {
            dropdown_controller.SetActive(true);
        }
        else
        {
            dropdown_controller.SetActive(false);
        }
    }

    void LoadDropdownOnValueChanged(int value) //loads the dictionary based on the index from the first dropdown
    {

        int indexOffset = adjustIndexForDropdown(value); //compensates for months offset by index (index 1 = April but DateTime int value is 4, DateTime defaults at 1.)
        DateTime indexAsMonth = new DateTime(); //new DateTime object initalized at default value (01/01/01 at 00:00:01)
        indexAsMonth = indexAsMonth.AddMonths(indexOffset); //sets months to value + 2;

        if (indexGreaterThanZero(value))
        {
            jsonReader = new JsonFileReader();
            dictionary = JsonUtility.FromJson<MonthDictionary>(jsonReader.LoadJsonAsResource("Items/MonthDictionary.json")); //load the array of json file paths
            string indexAsStr = indexAsMonth.ToString("MMM"); //returns month as abbreviated three letter string

            foreach (string dictionaryMonth in dictionary.months)
            {
                //sets variable to abbreviated month value as a string based on int value
                string noFilePath = removeFilePath(dictionaryMonth);
                bool doStringsMatch = doPathsMatch(noFilePath, indexAsStr);

                if (doStringsMatch)
                {
                    SetOnScreenMonth(monthMenuIndex);
                    LoadStates(dictionaryMonth);
                }
            }
        }
    }

    int adjustIndexForDropdown(int index) //adjusts the selected value of the first dropdown menu to return the proper month using DateTime
    {
        int value = index;
        value += 2; //default datetime initializes at 1 so we only need to add 2 here.
        return value;
    }

    private bool indexGreaterThanZero(int index) //takes an int and returns true if greater than zero
    {
        bool greaterThanZero;
        if (index > 0)
        {
            greaterThanZero = true;
        }
        else
        {
            greaterThanZero = false;
        }
        return greaterThanZero;
    }

    string removeFilePath(string path) //removes entire file path for comparison
    {
        string monthWithFilepath = path;
        string monthHalfFilepath = monthWithFilepath.Replace("Items/Months/", "");
        string monthNoFilepath = monthHalfFilepath.Replace(".json", "");
        return monthNoFilepath;
    }

    bool doPathsMatch(string path1, string path2) //checks the strings for a match and returns a boolean
    {
        bool pathsMatch = path1.Equals(path2);
        return pathsMatch;
    }

    void SetOnScreenMonth(int index)
    {
        int adjustedIndex = index - 1;
        if (adjustedIndex >= 0)
        {
            if (adjustedIndex == 9)
            {
                string fullMonthName = monthsPool[adjustedIndex];
                onScreenDisplay_date.text = "Date: " + fullMonthName + " 2021";
                osd_date.SetActive(true);
            }
            else
            {
                string fullMonthName = monthsPool[adjustedIndex];
                onScreenDisplay_date.text = "Date: " + fullMonthName + " 2020";
                osd_date.SetActive(true);
            }
        }
        else
        {
            osd_date.SetActive(false);
        }
    }

    void LoadStates(string path)
    {
        statesPool.Clear();
        dropdown_states.options.Clear();
        statesList = new List<string>();
        jsonReader = new JsonFileReader();

        string myLoadedStates = jsonReader.LoadJsonAsResource(path);
        States statesFromJson = JsonUtility.FromJson<States>(myLoadedStates);

        int dictionaryIndex = 0;

        foreach (State state in statesFromJson.states)
        {
            statesPool.Add(dictionaryIndex, state);
            dictionaryIndex++;
        }

        string courteousSelection = "Please choose a state.";
        statesList.Add(courteousSelection);

        foreach (KeyValuePair<int, State> entry in statesPool)
        {
            State temp = entry.Value;
            statesList.Add(temp.Province_State);
        }

        dropdown_states.AddOptions(statesList);
    }

    public void Dropdown_State_IndexChanged(int index) //when the dropdown of the list of states changes, update the accessible values for other classes to access
    {
        /** To-Do List:
         * 1. Cast index to inner variable (int indexDropdown)
         * 2. Get State object fields from statesPool using index (int indexDropdown) 
         * 3. Update OSD (textbox)
         * 4. Instantiate pools of objects for statistical representation
         * 5. Track objects instantiated
         * 6. Instantiate or Destroy if objects are lesser or greater than new State object fields.
         **/
        stateMenuIndex = index;
        int adjustedIndex = stateMenuIndex - 1;
        State activeState = new State();
        if(adjustedIndex >= 0)
        {
            activeState = statesPool[adjustedIndex];
            SetOnScreenDisplay(activeState);
            ClearSpawnLists();
            BuildSpawnListsAndGrid();
            AdjustCameraPos();
        }
        else
        {
            ClearSpawnLists();
            osd_stats.SetActive(false);
            osd_date.SetActive(false);
        }
    }

    void SetOnScreenDisplay(State activeState) //sets the OSD to the currently selected state on the second dropdown menu
    {
        string province_state = activeState.Province_State;
        int confirmed = activeState.Confirmed;
        int deaths = activeState.Deaths;
        onScreenDisplay_stats.text = "State: " + province_state + "\n" + "Confirmed: " + confirmed + "\n" + "Deceased: " + deaths + "\n" + "Ratio: 1:10";
        osd_stats.SetActive(true);
        osd_date.SetActive(true);
    }

    void ClearSpawnLists()
    {
            
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            Destroy(activeObjects[i], 0);
        }

        activeObjects.Clear();
        spawnTotal.Clear();
        colorObjects.Clear();
    }

    public void BuildSpawnListsAndGrid()
    {
        int count = 0;
        GameObject changeObject;
        State activeState = statesPool[stateMenuIndex - 1];
        int total = activeState.Confirmed / 10;
        int deceased = activeState.Deaths / 10;
        float length = Mathf.Sqrt(total)/2.0f; //get the dimensions of one side of the total spawn grid; divide that by 2
        int boundary = (int)length; //cast to int to prevent overshooting the actual statistic (within 3% margin of error)
        int positiveBoundary = boundary * 2;
        float posOffset = 1.8f;

        for(int x = -boundary; x < boundary; x++) //spawns objects from - 1/2 the length of a side to 1/2 the length of a side
        {
            for(int z = 0; z < positiveBoundary; z++)
            {
                Vector3 pos = new Vector3(x * posOffset, 0, z * posOffset);
                spawnTotal.Add(pos);
            }
        }

        for(int i = 0; i < spawnTotal.Count; i++)
        {
            count++;
            instanceObj = Instantiate(tempNPC, spawnTotal[i], Quaternion.identity);
            activeObjects.Add(instanceObj);
        }

        Debug.Log(count);

        for (int i = 0; i < deceased; i++) //randomly selected statistics marked as deceased
        {
            int rand = Random.Range(0, activeObjects.Count() - 1); //get random

            changeObject = activeObjects.ElementAt(rand);
            myMaterial = changeObject.GetComponent<MeshRenderer>().material;

            int attempts = 0;

            while (attempts < 20)
            {
                if (colorObjects.Contains(changeObject))
                {
                    changeObject = activeObjects.ElementAt(rand);
                    attempts++;
                }
                else
                {
                    myMaterial.color = new Color(1,0,0,0.5f);
                    colorObjects.Add(changeObject);
                    break;
                }
            }
        }
    }

    void AdjustCameraPos()
    {
        State stateArea = statesPool[stateMenuIndex - 1];
        int total = stateArea.Confirmed / 10;
        float dimensions = Mathf.Sqrt(total);

        float xpos = -1;
        float ypos = dimensions * 4 / 5;
        float zpos = -dimensions * 4 / 5;

        allSeeingEye.position = new Vector3(xpos, ypos, zpos);
        allSeeingEye.rotation = Quaternion.Euler(30, 0, 0);
    }
}
