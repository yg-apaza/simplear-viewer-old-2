using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public abstract class FrameworkController : MonoBehaviour
{
    protected Dictionary<string, GameObject> gameObjectresources = new Dictionary<string, GameObject>();
    protected Dictionary<string, Resource> resources = new Dictionary<string, Resource>();

    DatabaseReference resourcesReference;
    DatabaseReference interactionsReference;
    DatabaseReference previewsReference;

    void OnEnable()
    {
        //resourcesReference.ChildAdded += ResourceAdded;
        //interactionsReference.ValueChanged += InteractionsChanged;
    }

    void Start()
    {
        /**
        * Locking the preview
        **/

        resourcesReference = FirebaseDatabase.DefaultInstance.GetReference("resources").Child(Project.id);
        interactionsReference = FirebaseDatabase.DefaultInstance.GetReference("interactions").Child(Project.id);
        previewsReference = FirebaseDatabase.DefaultInstance.GetReference("previews").Child(Project.previewId);

        resourcesReference.ChildAdded += ResourceAdded;
        interactionsReference.ValueChanged += InteractionsChanged;

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

        AddResource(Resource.FromDataSnapshot(args.Snapshot));
        // TODO: Write preview resource to true
    }
    
    void InteractionsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        Interactions interactions = new Interactions(null);
        JsonUtility.FromJsonOverwrite((string) args.Snapshot.Value, interactions);
        UpdateInteractions(interactions.interactions);
    }

    public abstract void Setup();

    public void AddResource(Resource resource)
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

    /**
     * Always call UpdatePreviewResource(resourceId) after adding a resource
     **/
    public abstract void AddPredefinedFiducialMarkerResource(Resource resource);

    public abstract void AddPredefinedNaturalMarkerResource(Resource resource);

    public void AddPolyResource(Resource resource)
    {
        PolyUtil.GetPolyResult(resource.content, (polyResult) =>
        {
            resources.Add(resource.id, resource);
            gameObjectresources.Add(resource.id, polyResult);
            UpdatePreviewResource(resource.id);
        });
    }

    public void UpdatePreviewResource(string resourceId)
    {
        previewsReference.Child("resources").Child(resourceId).SetValueAsync(true);
    }

    public void UpdateInteractions(Interaction[] interactions)
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

    void OnDisable()
    {
        resourcesReference.ChildAdded -= ResourceAdded;
        interactionsReference.ValueChanged -= InteractionsChanged;
        previewsReference.Child("locked").SetValueAsync(false);
    }
}