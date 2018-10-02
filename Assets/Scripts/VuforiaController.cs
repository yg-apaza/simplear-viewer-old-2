using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class VuforiaController : FrameworkController
{
    public GameObject arCamera;
    private Dictionary<string, TrackableBehaviour> predefinedNaturalMarkers = new Dictionary<string, TrackableBehaviour>();

    public override void Setup()
    {
        arCamera.GetComponent<VuforiaBehaviour>().enabled = true;
        VuforiaRuntime.Instance.InitVuforia();
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(SetupTrackables);
    }

    public override void AddPredefinedFiducialMarkerResource(Resource resource)
    {
        Debug.Log("NOT SUPPORTED");
    }

    private void SetupTrackables()
    {
        // TODO: Avoid to load all trackables
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        DataSet dataSet = objectTracker.CreateDataSet();

        if (dataSet.Load("SimpleARDefaultMarkers"))
        {
            objectTracker.Stop();

            if (!objectTracker.ActivateDataSet(dataSet))
            {
                // Note: ImageTracker cannot have more than 1000 total targets activated
                Debug.Log("<color=yellow>Failed to Activate DataSet: SimpleARDefaultMarkers</color>");
            }

            if (!objectTracker.Start())
            {
                Debug.Log("<color=yellow>Tracker Failed to Start.</color>");
            }

            IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
            foreach (TrackableBehaviour tb in tbs)
            {
                predefinedNaturalMarkers.Add(tb.TrackableName, tb);
            }
        }
    }

    public override void AddPredefinedNaturalMarkerResource(Resource resource)
    {
        TrackableBehaviour tb = predefinedNaturalMarkers[resource.content];
        tb.gameObject.name = "DynamicImageTarget-" + tb.TrackableName;
        tb.gameObject.AddComponent<DefaultTrackableEventHandler>();
        tb.gameObject.AddComponent<TurnOffBehaviour>();

        resources.Add(resource.id, resource);
        gameObjectresources.Add(resource.id, tb.gameObject);
        return;
    }

    public override void MarkerIsDetected_AugmentResource(string[] event_inputs, string[] action_inputs)
    {
        string marker_name = event_inputs[0];
        string resource_name = action_inputs[0];
        if (resources.ContainsKey(marker_name) && resources.ContainsKey(resource_name))
        {
            gameObjectresources[resource_name].SetActive(true);
            gameObjectresources[resource_name].transform.parent = gameObjectresources[marker_name].transform;

            predefinedNaturalMarkers[resources[marker_name].content]
                .GetComponent<DefaultTrackableEventHandler>()
                .OnTrackableStateChanged(TrackableBehaviour.Status.TRACKED, TrackableBehaviour.Status.NO_POSE);
        }
    }
}
