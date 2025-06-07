using Firebase.Database;
using UnityEngine.UI; // Pour manipuler les éléments UI
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using ZXing; // Librairie pour lire les QR codes
using System.Linq; // Permet d’utiliser des méthodes LINQ (comme FirstOrDefault)
using TMPro;
using System;



public class QRAuthenticator : MonoBehaviour
{
    public GameObject authenticationPanel;
    public Text statusText; // Texte affichant les messages (ex: "Vérification en cours...")
    public RawImage cameraFeed;   // Pour afficher la caméra dans l'UI

    private DatabaseReference dbReference;

    private WebCamTexture backCameraTexture; //   lit la vidéo de la caméra.
    private IBarcodeReader barcodeReader; // scanner le QR code à partir des images caméra.

    // uv  est utilisé pour corriger l’orientation du feed caméra.
    private readonly Rect uvRectFlipped = new(1f, 0f, -1f, 1f);
    private readonly Rect uvRectNormal = new(0f, 0f, 1f, 1f);


    //ajoute pour tester sans qr code 
    public TMP_InputField uidInput;
    public TMP_InputField pinInput;
    public Button manualLoginButton;



    void Start()
    {


        //  initialiser Firebase.
        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized)
            {
                dbReference = FirebaseInitializer.Instance.DbReference; // référence vers la base de données.
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

        // ajoutee pour login auth mannuel 
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
        statusText.text = "Initializing camera...";

        string backCamName = WebCamTexture.devices.FirstOrDefault(device => !device.isFrontFacing).name;

        if (string.IsNullOrEmpty(backCamName))
        {
            statusText.text = "No back camera found.";
            return;
        }

        if (WebCamTexture.devices.Length == 0)
        {
            statusText.text = "No cameras found on device.";
            return;
        }

        backCameraTexture = new WebCamTexture(backCamName, 960, 960);

        backCameraTexture.requestedFPS = 30;
        backCameraTexture.filterMode = FilterMode.Bilinear;

        cameraFeed.texture = null; // Clear first
        cameraFeed.texture = backCameraTexture;
        cameraFeed.material = null;
        cameraFeed.color = Color.white;

        backCameraTexture.Play();

        // Wait until the camera starts updating
        StartCoroutine(AdjustCameraOrientation());

        barcodeReader = new BarcodeReader
        {
            AutoRotate = false,
            Options = new ZXing.Common.DecodingOptions
            {
                TryHarder = true,
                TryInverted = true,
                PossibleFormats = new[] { BarcodeFormat.QR_CODE }
            }
        };

        Debug.Log("Camera playing: " + backCameraTexture.isPlaying);
        Debug.Log("Camera frame size: " + backCameraTexture.width + "x" + backCameraTexture.height);


        StartCoroutine(ScanQRCode());
    }

    private IEnumerator AdjustCameraOrientation()
    {
        yield return new WaitUntil(() => backCameraTexture.width > 100);

        float angle = backCameraTexture.videoRotationAngle;
        if (!WebCamTexture.devices[0].isFrontFacing)
        {
            angle = -angle;
        }
        if (backCameraTexture.videoVerticallyMirrored)
        {
            angle += 180f;
        }

        cameraFeed.rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        bool needsFlip = (backCameraTexture.videoVerticallyMirrored && !WebCamTexture.devices[0].isFrontFacing)
                      || (!backCameraTexture.videoVerticallyMirrored && WebCamTexture.devices[0].isFrontFacing);

        cameraFeed.uvRect = needsFlip ? uvRectFlipped : uvRectNormal;
    }

    private IEnumerator ScanQRCode()
    {
        while (true)
        {
            if (backCameraTexture.didUpdateThisFrame)
            {
                // Convert the camera frame to a color array
                Color32[] colors = backCameraTexture.GetPixels32();
                var barcodeResult = barcodeReader.Decode(colors, backCameraTexture.width, backCameraTexture.height);

                if (barcodeResult != null)
                {
                    string uid = barcodeResult.Text;
                    if (!string.IsNullOrEmpty(uid))
                    {
                        AuthenticateUser(uid);
                        break;
                    }
                }
            }
            yield return null;
        }
    }


    //Authentification d’un utilisateur via QR
    private void AuthenticateUser(string rawData)
    {
        statusText.text = "Verifying QR Code...";

        try
        {
            var qrPayload = JsonUtility.FromJson<PlayerQRPayload>(rawData);
            // On lit les données du QR code (qui doivent contenir un uid et un pin).
            if (qrPayload == null || string.IsNullOrEmpty(qrPayload.uid) || string.IsNullOrEmpty(qrPayload.pin))
            {
                statusText.text = "Invalid QR Code format.";
                return;
            }

            string uid = qrPayload.uid;
            string enteredPin = qrPayload.pin;


            // on compare le pin avec celui stocké dans Firebase
            var pinRef = dbReference.Child("users").Child(uid).Child("password");
            pinRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    string storedPin = task.Result.Value.ToString();

                    if (enteredPin == storedPin)
                    {
                        statusText.text = "Authentication successful!";
                        LoadPlayerData(uid);
                    }
                    else
                    {
                        statusText.text = "Invalid PIN.";
                    }
                }
                else
                {
                    statusText.text = "Player not found.";
                }
            });
        }
        catch
        {
            statusText.text = "QR Code data unreadable.";
        }
    }

    private void LoadPlayerData(string uid)
    {
        statusText.text = "Welcome back! Loading your profile...";

        DatabaseReference playerRef = dbReference.Child("users").Child(uid);
        playerRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var playerData = task.Result;
                Debug.Log("Player data loaded.");

                // Proceed to the main game scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestScene");
            }
            else
            {
                statusText.text = "Failed to load player data.";
            }
        });
    }



    // fct pour authentifier manellemnt sans qrcode 
    private void AuthenticateUserManual(string uid, string enteredPin)
    {

        try
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(enteredPin))
            {
                statusText.text = "UID and PIN are required.";
                return;
            }

            statusText.text = "Verifying credentials...";

            var pinRef = dbReference.Child("users").Child(uid).Child("password");
            pinRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {


                dbReference.Child("users").GetValueAsync().ContinueWith(task2 => {
                    if (task2.IsCompleted && task2.Result.Exists)
                    {
                        DataSnapshot snapshot = task2.Result;
                        foreach (var user in snapshot.Children)
                        {
                            string userId = user.Key;
                            string role = user.Child("role").Value?.ToString();
                            string pass = user.Child("password").Value?.ToString();

                            Debug.Log("UID: " + userId + ", Nom: " + role + ", Email: " + pass);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Impossible de lire les utilisateurs ou aucun utilisateur trouvé.");
                    }
                });


                Debug.Log("Référence Firebase: " + dbReference);
                Debug.Log("UID entré: '" + uid + "'");
                Debug.Log("PIN entré: '" + enteredPin + "'");
                if (task.IsCompleted && task.Result.Exists)
                {
                    string storedPin = task.Result.Value.ToString();

                    if (enteredPin == storedPin)
                    {
                        statusText.text = "Authentication successful!";
                        LoadPlayerData(uid);
                    }
                    else
                    {
                        statusText.text = "Invalid PIN.";
                    }
                }
                else
                {
                    statusText.text = "User not found.";
                }
            });

        }

        catch (Exception ex)
        {
            Debug.LogError("Exception dans le bloc Firebase: " + ex.Message + "\n" + ex.StackTrace);
        }

    }

}
