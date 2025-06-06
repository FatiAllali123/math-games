using Firebase.Database;
using UnityEngine.UI; // Pour manipuler les  l ments UI
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using ZXing; // Librairie pour lire les QR codes
using System.Linq; // Permet d utiliser des m thodes LINQ (comme FirstOrDefault)
using TMPro;
using System;
using System.Collections.Generic;






public class QRAuthenticator : MonoBehaviour
{
    public GameObject authenticationPanel;
    public Text statusText; // Texte affichant les messages (ex: "V rification en cours...")
    public RawImage cameraFeed;   // Pour afficher la cam ra dans l'UI

    private DatabaseReference dbReference;

    private WebCamTexture backCameraTexture; //   lit la vid o de la cam ra.
    private IBarcodeReader barcodeReader; // scanner le QR code   partir des images cam ra.

    // uv  est utilis  pour corriger l orientation du feed cam ra.
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
                dbReference = FirebaseInitializer.Instance.DbReference; // r f rence vers la base de donn es.
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


    //Authentification d un utilisateur via QR
    private void AuthenticateUser(string rawData)
    {
        statusText.text = "Verifying QR Code...";

        try
        {
            var qrPayload = JsonUtility.FromJson<PlayerQRPayload>(rawData);
            // On lit les donn es du QR code (qui doivent contenir un uid et un pin).
            if (qrPayload == null || string.IsNullOrEmpty(qrPayload.uid) || string.IsNullOrEmpty(qrPayload.pin))
            {
                statusText.text = "Invalid QR Code format.";
                return;
            }

            string uid = qrPayload.uid;
            string enteredPin = qrPayload.pin;


            // on compare le pin avec celui stock  dans Firebase
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
    /*
    private void LoadPlayerData(string uid)
    {
        statusText.text = "Welcome back! Loading your profile...";

        DatabaseReference playerRef = dbReference.Child("users").Child(uid);
        playerRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                string json = task.Result.GetRawJsonValue();

                UserData userData = JsonUtility.FromJson<UserData>(json);

                if (userData != null)
                {
                    // Stocker dans le singleton
                    UserSession.Instance.SetUserData(userData);

                    // Ensuite charger la sc ne principale
                    UnityEngine.SceneManagement.SceneManager.LoadScene("WelcomeScene");
                }
                else
                {
                    statusText.text = "Error parsing user data.";
                }
            }
            else
            {
                statusText.text = "Failed to load player data.";
            }
        });
    }


    */

    private void LoadPlayerData(string uid)
    {
        statusText.text = "Welcome back! Loading your profile...";

        DatabaseReference playerRef = dbReference.Child("users").Child(uid);
        playerRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log("Raw Firebase data: " + task.Result.GetRawJsonValue());

                try
                {
                    // Cr er manuellement UserData   partir des donn es Firebase
                    UserData userData = ParseUserDataFromSnapshot(task.Result);

                    if (userData != null)
                    {
                        Debug.Log("User data loaded successfully: " + userData.firstName + " " + userData.lastName);

                        // V rifier que UserSession existe
                        if (UserSession.Instance == null)
                        {
                            Debug.LogError("UserSession.Instance is null! Make sure UserSession GameObject exists in the scene.");
                            statusText.text = "Session error. Please restart.";
                            return;
                        }

                        // Stocker dans le singleton
                        UserSession.Instance.SetUserData(userData);

                        // Attendre un frame avant de changer de sc ne
                        StartCoroutine(LoadSceneDelayed());
                    }
                    else
                    {
                        Debug.LogError("Failed to parse UserData from Firebase snapshot");
                        statusText.text = "Error parsing user data.";
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception while parsing user data: " + ex.Message + "\n" + ex.StackTrace);
                    statusText.text = "Error loading user data.";
                }
            }
            else
            {
                Debug.LogError("Firebase task failed or data doesn't exist. Task status: " + task.Status);
                if (task.Exception != null)
                {
                    Debug.LogError("Exception: " + task.Exception);
                }
                statusText.text = "Failed to load player data.";
            }
        });
    }


    private UserData ParseUserDataFromSnapshot(DataSnapshot snapshot)
    {
        try
        {
            UserData userData = new UserData
            {
                uid = snapshot.Child("uid").Value?.ToString(),
                role = snapshot.Child("role").Value?.ToString(),
                firstName = snapshot.Child("firstName").Value?.ToString(),
                lastName = snapshot.Child("lastName").Value?.ToString(),
                birthday = snapshot.Child("birthday").Value?.ToString(),
                gender = snapshot.Child("gender").Value?.ToString(),
                linkedTeacherId = snapshot.Child("linkedTeacherId").Value?.ToString()
            };

            // Parse schoolGrade (niveau UserData)
            if (int.TryParse(snapshot.Child("schoolGrade").Value?.ToString(), out int schoolGradeInt) &&
                Enum.IsDefined(typeof(GradeLevel), schoolGradeInt))
            {
                userData.schoolGrade = (GradeLevel)schoolGradeInt;
            }
            else
            {
                userData.schoolGrade = GradeLevel.One; // Valeur par d faut
            }

            // Parse PlayerProfile
            if (snapshot.Child("playerProfile").Exists)
            {
                var profileSnapshot = snapshot.Child("playerProfile");
                userData.playerProfile = new PlayerProfile
                {
                    playerName = profileSnapshot.Child("playerName").Value?.ToString(),
                    coins = int.TryParse(profileSnapshot.Child("coins").Value?.ToString(), out int coins) ? coins : 0,
                    mathLevel = int.TryParse(profileSnapshot.Child("mathLevel").Value?.ToString(), out int mathLevel) ? mathLevel : 0,
                    questionsSolved = int.TryParse(profileSnapshot.Child("questionsSolved").Value?.ToString(), out int questionsSolved) ? questionsSolved : 0
                };

                // Parse schoolGrade dans PlayerProfile avec validation
                if (int.TryParse(profileSnapshot.Child("schoolGrade").Value?.ToString(), out int profileSchoolGradeInt) &&
                    Enum.IsDefined(typeof(GradeLevel), profileSchoolGradeInt))
                {
                    userData.playerProfile.schoolGrade = (GradeLevel)profileSchoolGradeInt;
                }
                else
                {
                    userData.playerProfile.schoolGrade = GradeLevel.One; // Valeur par d faut
                }

                // Parse skillsToImprove
                if (profileSnapshot.Child("skillsToImprove").Exists)
                {
                    userData.playerProfile.skillsToImprove = new List<string>();
                    foreach (var skill in profileSnapshot.Child("skillsToImprove").Children)
                    {
                        userData.playerProfile.skillsToImprove.Add(skill.Value.ToString());
                    }
                }

                // Parse rewardProfile
                if (profileSnapshot.Child("rewardProfile").Exists)
                {
                    var rewardSnapshot = profileSnapshot.Child("rewardProfile");
                    userData.playerProfile.rewardProfile = new RewardData
                    {
                        score = int.TryParse(rewardSnapshot.Child("score").Value?.ToString(), out int score) ? score : 0,
                        rank = int.TryParse(rewardSnapshot.Child("rank").Value?.ToString(), out int rank) ? rank : 0,
                        iScore = int.TryParse(rewardSnapshot.Child("iScore").Value?.ToString(), out int iScore) ? iScore : 0,
                        rewardCount = int.TryParse(rewardSnapshot.Child("rewardCount").Value?.ToString(), out int rewardCount) ? rewardCount : 0,
                        positives = int.TryParse(rewardSnapshot.Child("positives").Value?.ToString(), out int positives) ? positives : 0,
                        negatives = int.TryParse(rewardSnapshot.Child("negatives").Value?.ToString(), out int negatives) ? negatives : 0
                    };
                }
            }

            // Parse gameProgress (au niveau racine)
            if (snapshot.Child("gameProgress").Exists)
            {
                userData.gameProgress = new Dictionary<string, GameProgressEntry>();
                foreach (var game in snapshot.Child("gameProgress").Children)
                {
                    var gameEntry = new GameProgressEntry
                    {
                        lastScore = int.TryParse(game.Child("lastScore").Value?.ToString(), out int lastScore) ? lastScore : 0,
                        bestScore = int.TryParse(game.Child("bestScore").Value?.ToString(), out int bestScore) ? bestScore : 0,
                        completedAt = game.Child("completedAt").Value?.ToString()
                    };
                    userData.gameProgress[game.Key] = gameEntry;
                }
            }

            // Parse achievements (au niveau racine)
            if (snapshot.Child("achievements").Exists)
            {
                userData.achievements = new AchievementData();
                if (snapshot.Child("achievements").Child("badges").Exists)
                {
                    userData.achievements.badges = new List<string>();
                    foreach (var badge in snapshot.Child("achievements").Child("badges").Children)
                    {
                        userData.achievements.badges.Add(badge.Value.ToString());
                    }
                }
            }

            return userData;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing UserData: " + ex.Message);
            return null;
        }
    }


    private IEnumerator LoadSceneDelayed()
    {
        yield return null;
        Debug.Log("Loading WelcomeScene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("WelcomeScene");
    }











    // fct pour authentifier manellemnt sans qrcode 
    /*
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
                        Debug.LogWarning("Impossible de lire les utilisateurs ou aucun utilisateur trouv .");
                    }
                });


                Debug.Log("R f rence Firebase: " + dbReference);
                Debug.Log("UID entr : '" + uid + "'");
                Debug.Log("PIN entr : '" + enteredPin + "'");
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
    */

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
                Debug.Log("Firebase reference: " + dbReference);
                Debug.Log("UID entered: '" + uid + "'");
                Debug.Log("PIN entered: '" + enteredPin + "'");

                if (task.IsCompleted && task.Result.Exists)
                {
                    string storedPin = task.Result.Value.ToString();
                    Debug.Log("Stored PIN: '" + storedPin + "'");

                    if (enteredPin == storedPin)
                    {
                        statusText.text = "Authentication successful!";
                        LoadPlayerData(uid);
                    }
                    else
                    {
                        statusText.text = "Invalid PIN.";
                        Debug.Log("PIN mismatch - entered: '" + enteredPin + "', stored: '" + storedPin + "'");
                    }
                }
                else
                {
                    statusText.text = "User not found.";
                    Debug.Log("User not found in Firebase for UID: " + uid);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception in manual authentication: " + ex.Message + "\n" + ex.StackTrace);
            statusText.text = "Authentication error.";
        }
    }

}