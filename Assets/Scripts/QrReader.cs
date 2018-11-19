using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;
using ZXing;

public class QrReader : MonoBehaviour
{
    private bool cameraInitialized;
    private IBarcodeReader barCodeReader;

    void Start()
    {
        // Vuforia
        VuforiaBehaviour.Instance.enabled = true;
        VuforiaRuntime.Instance.InitVuforia();

        // Zxing
        barCodeReader = new BarcodeReader();
        StartCoroutine(InitializeCamera());

        // Firebase
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://simplear-e9683.firebaseio.com");
    }

    private IEnumerator InitializeCamera()
    {
        // Waiting a little seem to avoid the Vuforia's crashes.
        yield return new WaitForSeconds(1.25f);
        
        // Force autofocus.
        bool isAutoFocus = CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        if (!isAutoFocus)
        {
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
        }
        cameraInitialized = true;
    }

    private void Update()
    {
        if (cameraInitialized)
        {
            try
            {
                //Image cameraFeed = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.GRAYSCALE);
                if (CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true))
                {
                    Debug.Log("Successfully registered pixel format ");
                }
                else
                {
                    Debug.LogError("Failed to register pixel format:");
                    return;
                }

                Image cameraFeed = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.GRAYSCALE);
                if (cameraFeed == null)
                {
                    return;
                }
                Result data = barCodeReader.Decode(cameraFeed.Pixels, cameraFeed.BufferWidth, cameraFeed.BufferHeight, RGBLuminanceSource.BitmapFormat.Gray8);
                if (data != null)
                {
                    // TODO: Handle when preview doesn't exists or an invalid text from QR Code was detected
                    FirebaseDatabase.DefaultInstance
                        .GetReference("previews/" + data.Text)
                        .GetValueAsync().ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                // TODO: Handle the error...
                            }
                            else if (task.IsCompleted)
                            {
                                DataSnapshot snapshot = task.Result;
                                Project.id = (string) snapshot.Child("projectId").Value;
                                Project.previewId = data.Text;
                                SceneManager.LoadScene("ManagerScene");
                            }
                        });
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);
            }
        }
    }
}