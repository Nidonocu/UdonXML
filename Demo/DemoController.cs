
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class DemoController : UdonSharpBehaviour
{
    // Components

    [SerializeField]
    UdonXML UdonXML;

    // Data

    [SerializeField]
    string DatabaseXML;

    object RootNode;

    [UdonSynced]
    string ResultsData = string.Empty;
    string _prevResultsData = string.Empty;

    [UdonSynced]
    string StatusData = string.Empty;
    string _prevStatusData = string.Empty;

    // UI

    [SerializeField]
    Button StartButton;
    [SerializeField]
    Text StatusText;
    [SerializeField]
    Text ResultsText;
    [SerializeField]
    Image SpinnerImage;

    public void StartLoading()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        StatusData = "Loading Data - 0%";
        UpdateStatus();
        RequestSerialization();

        Debug.Log("[DemoController] Starting Load...");

        if (DatabaseXML == "")
        {
            Debug.LogError("[DemoController] No Input Data Provided");
            return;
        }

        var check = UdonXML._LoadXmlAsync(DatabaseXML, this, "_ReviewData", "_ShowCurrentStatus");
        if (!check)
        {
            Debug.LogError("[DemoController] Could not load data - XML operation already in progress");
            return;
        }
    }

    public void _ReviewData()
    {
        Debug.Log("[DemoController] Load Complete");

        RootNode = UdonXML.FetchAsyncResult();

        if (RootNode == null)
        {
            Debug.LogError("[DemoController] Invalid XML");
            return;
        }

        var NorthWindNode = UdonXML.GetChildNodeByName(RootNode, "Northwind");
        if (NorthWindNode == null)
        {
            Debug.LogError("[DemoController] Unable to find Database Root Node");
            return;
        }

        var recordsCount = UdonXML.GetChildNodesCount(NorthWindNode);

        var recordTypeNames = new string[0];
        var recordTypeCounts = new int[0];

        for (var i = 0; i != UdonXML.GetChildNodesCount(NorthWindNode); i++)
        {
            var recordNode = UdonXML.GetChildNode(NorthWindNode, i);
            var recordTypeName = UdonXML.GetNodeName(recordNode);
            var newName = true;
            for (int tnI = 0; tnI < recordTypeNames.Length; tnI++)
            {
                if (recordTypeNames[tnI] == recordTypeName)
                {
                    recordTypeCounts[tnI]++;
                    newName = false;
                    break;
                }
            }
            if (newName)
            {
                var tempNameArray = new string[recordTypeNames.Length + 1];
                var tempCountArray = new int[recordTypeNames.Length + 1];
                for (int n = 0; n < recordTypeNames.Length; n++)
                {
                    tempNameArray[n] = recordTypeNames[n];
                    tempCountArray[n] = recordTypeCounts[n];
                }
                tempNameArray[recordTypeNames.Length] = recordTypeName;
                tempCountArray[recordTypeNames.Length] = 1;
                recordTypeNames = tempNameArray;
                recordTypeCounts = tempCountArray;
            }
        }

        var outputString = "The following data was successfully loaded:\n";

        for (int i = 0; i < recordTypeNames.Length; i++)
        {
            outputString += recordTypeCounts[i] + " " + recordTypeNames[i] + "\n";
        }
        outputString += recordsCount + " Total Records";
        ResultsData = outputString;
        RequestSerialization();
        ShowResults();
    }

    public void _ShowCurrentStatus()
    {
        StatusData = "Loading Data - " + UdonXML.FetchAsyncProgress().ToString("N1") + "%";
        RequestSerialization();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        StartButton.gameObject.SetActive(false);
        StatusText.gameObject.SetActive(true);
        SpinnerImage.gameObject.SetActive(true);
        StatusText.text = StatusData;
    }

    private void ShowResults()
    {
        ResultsText.text = ResultsData;
        ResultsText.gameObject.SetActive(true);
        StatusText.gameObject.SetActive(false);
        SpinnerImage.gameObject.SetActive(false);
    }

    public override void OnDeserialization()
    {
        if (ResultsData != _prevResultsData)
        {
            _prevResultsData = ResultsData;
            ShowResults();
        }
        if (StatusData != _prevStatusData)
        {
            _prevStatusData = StatusData;
            UpdateStatus();
        }
    }
}
