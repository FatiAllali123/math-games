using Firebase.Database;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using ZXing;
using System.Linq;
using TMPro;
using System;

public class QRAuthenticator : MonoBehaviour
{
    public GameObject authenticationPanel;
    public Text statusText;
    public RawImage cameraFeed;
    public TMP_InputField uidInput;
    public TMP_InputField pinInput;
    public Button manualLoginButton;

    private DatabaseReference dbReference;
    private WebCamTexture backCameraTexture;
    private IBarcodeReader barcodeReader;
    private readonly Rect uvRectFlipped = new(1f, 0f, -1f, 1f);
    private readonly Rect uvRectNormal = new(0f, 0f, 1f, 1f);

    void Start()
    {
        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized)
            {
                dbReference = FirebaseInitializer.Instance.DbReference;
                statusText.text = "Checking camera permissions...";

                if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
                {
                    StartCoroutine(RequestCameraPermission());
                }
                else
                {
                    SetupCamera();
                }
            }
        });

        manualLoginButton.onClick.AddListener(() =>
        {
            string uid = uidInput.text.Trim();
            string pin = pinInput.text.Trim();
            AuthenticateUserManual(uid, pin);
        });
    }

    private IEnumerator RequestCameraPermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            SetupCamera();
        }
        else
        {
            statusText.text = "Camera permission denied.";
        }
    }

    private void SetupCamera()
    {
        string backCamName = WebCamTexture.devices.FirstOrDefault(device => !device.isFrontFacing).name;
        backCameraTexture = new WebCamTexture(backCamName, 960, 960);
        cameraFeed.texture = backCameraTexture;
        backCameraTexture.Play();

        StartCoroutine(AdjustCameraOrientation());

        barcodeReader = new BarcodeReader
        {
            Options = new ZXing.Common.DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new[] { BarcodeFormat.QR_CODE }
            }
        };

        StartCoroutine(ScanQRCode());
    }

    private IEnumerator AdjustCameraOrientation()
    {
        yield return new WaitUntil(() => backCameraTexture.width > 100);

        float angle = backCameraTexture.videoRotationAngle;
        if (!WebCamTexture.devices[0].isFrontFacing) angle = -angle;
        if (backCameraTexture.videoVerticallyMirrored) angle += 180f;

        cameraFeed.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        cameraFeed.uvRect = backCameraTexture.videoVerticallyMirrored ? uvRectFlipped : uvRectNormal;
    }

    private IEnumerator ScanQRCode()
    {
        while (true)
        {
            if (backCameraTexture.didUpdateThisFrame)
            {
                var result = barcodeReader.Decode(backCameraTexture.GetPixels32(),
                                                 backCameraTexture.width,
                                                 backCameraTexture.height);
                if (result != null) AuthenticateUser(result.Text);
            }
            yield return null;
        }
    }

    private void AuthenticateUser(string rawData)
    {
        try
        {
            var qrPayload = JsonUtility.FromJson<PlayerQRPayload>(rawData);
            if (qrPayload == null) return;

            dbReference.Child("users").Child(qrPayload.uid).Child("password")
                      .GetValueAsync().ContinueWithOnMainThread(task =>
                      {
                          if (task.Result.Exists && task.Result.Value.ToString() == qrPayload.pin)
                          {
                              LoadPlayerData(qrPayload.uid);
                          }
                          else
                          {
                              statusText.text = "Authentication failed";
                          }
                      });
        }
        catch
        {
            statusText.text = "Invalid QR Code";
        }
    }

    private void LoadPlayerData(string uid)
    {
        dbReference.Child("users").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                var data = task.Result;
                PlayerPrefs.SetString("StudentGrade", data.Child("schoolGrade").Value.ToString());
                PlayerPrefs.SetString("StudentUID", uid);
                UnityEngine.SceneManagement.SceneManager.LoadScene("ChoiceScene");
            }
        });
    }

    private void AuthenticateUserManual(string uid, string pin)
    {
        dbReference.Child("users").Child(uid).Child("password")
                  .GetValueAsync().ContinueWithOnMainThread(task =>
                  {
                      if (task.Result.Exists && task.Result.Value.ToString() == pin)
                      {
                          LoadPlayerData(uid);
                      }
                      else
                      {
                          statusText.text = "Invalid credentials";
                      }
                  });
    }

    [System.Serializable]
    private class PlayerQRPayload
    {
        public string uid;
        public string pin;
    }
}