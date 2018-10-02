using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public abstract class FrameworkController : MonoBehaviour
{
    protected Dictionary<string, GameObject> gameObjectresources = new Dictionary<string, GameObject>();
    protected Dictionary<string, Resource> resources = new Dictionary<string, Resource>();

    DatabaseReference resourcesReference = FirebaseDatabase.DefaultInstance.GetReference("resources").Child(Project.id);
    DatabaseReference interactionsReference = FirebaseDatabase.DefaultInstance.GetReference("interactions").Child(Project.id);
    DatabaseReference previewsResourcesReference = FirebaseDatabase.DefaultInstance.GetReference("previews").Child(Project.previewId).Child("resources");

    void OnEnable()
    {
        resourcesReference.ChildAdded += ResourceAdded;
        interactionsReference.ValueChanged += InteractionsChanged;
    }

    void Start()
    {
        /**
         * Locking the preview
         **/
        FirebaseDatabase.DefaultInstance
            .GetReference("previews").Child(Project.previewId).Child("locked").SetValueAsync(true);

        Setup();
    }

    void ResourceAdded(object sender, ChildChangedEventArgs args)
    {
        if(args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        ResourceCreated(Resource.FromDataSnapshot(args.Snapshot));
        // TODO: Write preview resource to true
    }
    
    void InteractionsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        //Debug.Log((string) args.Snapshot.Key);
    }

    /**
     * Every class which implements this should call this method when the
     * framework is ready
     **/
    //public void SendReadyMessage()
    //{
    //    //Controller.eventManager.Send(new ViewerEvent.FrameworkReady(ViewerEvent.FRAMEWORK_READY));
    //}

    public abstract void Setup();

    //public void ResourcesSetup(Resource[] resources)
    //{
    //    foreach (Resource r in resources)
    //    {
    //        ResourceCreated(r);
    //    }
    //    //Controller.eventManager.Send(new ViewerEvent.ResourcesInitialized(ViewerEvent.RESOURCES_INITIALIZED));
    //}

    public void InteractionsApproved(Interaction[] interactions)
    {
        RestoreResources();
        foreach (Interaction i in interactions)
        {
            switch (i._event)
            {
                case 1:
                    switch (i._action)
                    {
                        case 1:
                            MarkerIsDetected_AugmentResource(i.event_inputs, i.action_inputs);
                            break;
                    }
                    break;
            }
        }
    }

    /**
     * Restore method can be overrided or edited. It should restore to the
     * initial state of resources, this is without connections or interactions added
     **/
    public void RestoreResources()
    {
        foreach (KeyValuePair<string, GameObject> resource in gameObjectresources)
        {
            resource.Value.transform.parent = null;
        }
    }

    public abstract void MarkerIsDetected_AugmentResource(string[] event_inputs, string[] action_inputs);

    public abstract void InteractionsRejected();

    public void ResourceCreated(Resource resource)
    {
        // TODO: Use static strings or enums
        switch (resource.type)
        {
            case "pfmarker":
                AddPredefinedFiducialMarkerResource(resource);
                break;
            case "pnmarker":
                AddPredefinedNaturalMarkerResource(resource);
                break;
            case "poly":
                AddPolyResource(resource);
                break;
        }
    }

    public abstract void AddPredefinedFiducialMarkerResource(Resource resource);

    public abstract void AddPredefinedNaturalMarkerResource(Resource resource);

    public void AddPolyResource(Resource resource)
    {
        PolyUtil.GetPolyResult(resource.content, (polyResult) =>
        {
            resources.Add(resource.id, resource);
            gameObjectresources.Add(resource.id, polyResult);
        });
    }

    void OnDisable()
    {
        resourcesReference.ChildAdded -= ResourceAdded;
        interactionsReference.ValueChanged -= InteractionsChanged;
        // TODO: Set locked to false
    }

}