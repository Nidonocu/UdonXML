
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UdonXMLAsyncRunner : UdonSharpBehaviour
{
    [SerializeField]
    UdonXML UdonXML;

    [SerializeField]
    float ProgressMilestoneSize = 5f;

    object[] StateObject;

    string CurrentJobString = string.Empty;

    public object[] ResultObject;

    UdonSharpBehaviour CallbackBehaviour;

    string CallbackFunctionName = string.Empty;

    string ProgressCallbackFunctionName = string.Empty;

    int CurrentProgress;

    float CurrentProgressPC;
    float NextProgressPCMilestone;

    public void _InitAsync(string input, UdonSharpBehaviour callbackBehaviour, string callbackFunctionName, string progressCallbackFunctionName)
    {
        StateObject = null;
        ResultObject = null;
        CurrentProgress = 0;
        CurrentProgressPC = 0f;
        NextProgressPCMilestone = ProgressMilestoneSize;
        CurrentJobString = input;
        CallbackBehaviour = callbackBehaviour;
        CallbackFunctionName = callbackFunctionName;
        ProgressCallbackFunctionName = progressCallbackFunctionName;
    }
    
    public float _GetCurrentProgressPC()
    {
        return CurrentProgressPC;
    }

    void Update()
    {
        if (CurrentJobString != string.Empty)
        {
#if DEBUG
            //Debug.Log("[UDON-XML ASYNC RUNNER] ASYNC Load Progress:" + CurrentProgress);
#endif
            var result = UdonXML.Parse(true, CurrentJobString.ToCharArray(), StateObject, CurrentProgress);
            CurrentProgressPC = CurrentProgress / (float)CurrentJobString.Length * 100f;
            if (ProgressCallbackFunctionName != string.Empty)
            {
                if (CurrentProgressPC > NextProgressPCMilestone)
                {
                    NextProgressPCMilestone += ProgressMilestoneSize;
                    CallbackBehaviour.SendCustomEvent(ProgressCallbackFunctionName);
                }
            }
            if (CurrentProgress == CurrentJobString.Length)
            {
                ResultObject = result;
                CurrentJobString = string.Empty;
            }
        }
        else if (ResultObject != null)
        {
            if (CallbackFunctionName != string.Empty)
            {
                CallbackBehaviour.SendCustomEvent(CallbackFunctionName);
            }
            gameObject.SetActive(false);
        }
    }

    public void _SaveStates(object[] stateObject, int currentProgress)
    {
        StateObject = stateObject;
        CurrentProgress = currentProgress;
    }
}
