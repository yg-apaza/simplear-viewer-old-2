using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerController : MonoBehaviour {

	void Start ()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("projects/" + Project.id)
            .GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    // TODO: Handle the error...
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    switch ((string)snapshot.Child("framework").Value)
                    {
                        case "vuforia":
                            SceneManager.LoadScene("VuforiaScene");
                            break;
                        case "artoolkit":
                            Debug.Log("Load Artoolkit scene");
                            break;
                    }
                }
            });
    }
}
