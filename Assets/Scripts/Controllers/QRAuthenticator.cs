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

        cameraFeed.texture = null;
        cameraFeed.texture = backCameraTexture;
        cameraFeed.material = null;
        cameraFeed.color = Color.white;

        backCameraTexture.Play();
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

    private void AuthenticateUser(string rawData)
    {
        statusText.text = "Verifying QR Code...";

        try
        {
            var qrPayload = JsonUtility.FromJson<PlayerQRPayload>(rawData);
            if (qrPayload == null || string.IsNullOrEmpty(qrPayload.uid) || string.IsNullOrEmpty(qrPayload.pin))
            {
                statusText.text = "Invalid QR Code format.";
                return;
            }

            string uid = qrPayload.uid;
            string enteredPin = qrPayload.pin;

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
            statusText.text = "Authentication error.";
        }
    }

    private void LoadPlayerData(string uid)
    {
        statusText.text = "Welcome back! Loading your profile...";

        dbReference.Child("users").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var playerData = task.Result;
                string schoolGrade = playerData.Child("schoolGrade").Value.ToString();
                Debug.Log($"Grade de l'étudiant {uid} : {schoolGrade}");

                // Vérifier si l'étudiant est en grade 5 ou 6
                if (schoolGrade != "5" && schoolGrade != "6")
                {
                    statusText.text = "Seuls les étudiants de grade 5 ou 6 peuvent jouer.";
                    Debug.LogError($"Grade non autorisé : {schoolGrade}");
                    return;
                }

                // Vérifier les tests pour vertical_operations avec Multiplication
                dbReference.Child("tests").GetValueAsync().ContinueWithOnMainThread(testTask =>
                {
                    if (testTask.IsCompleted && testTask.Result.Exists)
                    {
                        bool hasValidTest = false;
                        int maxNumberRange = 3; // Valeur par défaut
                        int numOperations = 5; // Valeur par défaut
                        float requiredCorrectAnswersMinimumPercent = 75f; // Valeur par défaut

                        foreach (DataSnapshot test in testTask.Result.Children)
                        {
                            string testId = test.Key;
                            string testGrade = test.Child("grade").Value.ToString();
                            Debug.Log($"Analyse du test {testId} pour le grade {testGrade}");

                            // Vérifier que le test est pour grade 5 ou 6
                            if (testGrade != "5" && testGrade != "6")
                            {
                                Debug.Log($"Test {testId} ignoré : grade {testGrade} non autorisé.");
                                continue;
                            }

                            // Vérifier explicitement si le test contient vertical_operations
                            if (!test.HasChild("miniGameConfigs/vertical_operations"))
                            {
                                Debug.Log($"Test {testId} ignoré : pas de miniGameConfigs/vertical_operations.");
                                continue;
                            }

                            // Vérifier que groupsMiniGameOrder contient vertical_operations
                            if (!test.HasChild("groupsMiniGameOrder/o") || !test.Child("groupsMiniGameOrder/o").Children.Any(child => child.Value.ToString() == "vertical_operations"))
                            {
                                Debug.Log($"Test {testId} ignoré : vertical_operations non inclus dans groupsMiniGameOrder/o.");
                                continue;
                            }

                            DataSnapshot verticalOpsConfig = test.Child("miniGameConfigs/vertical_operations");
                            bool isMultiplication = false;

                            // Vérifier groupsConfig pour s'assurer que l'étudiant est assigné
                            if (verticalOpsConfig.HasChild("groupsConfig"))
                            {
                                foreach (DataSnapshot group in verticalOpsConfig.Child("groupsConfig").Children)
                                {
                                    string groupId = group.Key;
                                    if (group.HasChild("studentIds"))
                                    {
                                        foreach (DataSnapshot sid in group.Child("studentIds").Children)
                                        {
                                            if (sid.Value.ToString() == uid)
                                            {
                                                // Étudiant trouvé dans ce groupe, vérifier operationsAllowed
                                                string operationsAllowed = group.Child("config/operationsAllowed").Value.ToString().ToLower();
                                                if (operationsAllowed.Contains("multiplication"))
                                                {
                                                    isMultiplication = true;
                                                    maxNumberRange = int.Parse(group.Child("config/maxNumberRange").Value.ToString());
                                                    numOperations = int.Parse(group.Child("config/numOperations").Value.ToString());
                                                    requiredCorrectAnswersMinimumPercent = float.Parse(group.Child("config/requiredCorrectAnswersMinimumPercent").Value.ToString());
                                                    Debug.Log($"GroupsConfig trouvé pour test {testId}, groupe {groupId}, étudiant {uid} : maxNumberRange={maxNumberRange}, numOperations={numOperations}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // Si groupsConfig n'a pas donné de résultat, vérifier gradeConfig
                            if (!isMultiplication && verticalOpsConfig.HasChild("gradeConfig/config/operationsAllowed"))
                            {
                                string operationsAllowed = verticalOpsConfig.Child("gradeConfig/config/operationsAllowed").Value.ToString().ToLower();
                                if (operationsAllowed.Contains("multiplication"))
                                {
                                    isMultiplication = true;
                                    maxNumberRange = int.Parse(verticalOpsConfig.Child("gradeConfig/config/maxNumberRange").Value.ToString());
                                    numOperations = int.Parse(verticalOpsConfig.Child("gradeConfig/config/numOperations").Value.ToString());
                                    requiredCorrectAnswersMinimumPercent = float.Parse(verticalOpsConfig.Child("gradeConfig/config/requiredCorrectAnswersMinimumPercent").Value.ToString());
                                    Debug.Log($"GradeConfig trouvé pour test {testId} : maxNumberRange={maxNumberRange}, numOperations={numOperations}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
                                }
                            }

                            if (isMultiplication)
                            {
                                hasValidTest = true;
                                // Créer ou accéder au GameManager et définir les paramètres
                                if (GameManager.Instance == null)
                                {
                                    GameObject gmObject = new GameObject("GameManager");
                                    gmObject.AddComponent<GameManager>();
                                    Debug.LogWarning("GameManager créé dynamiquement dans QRAuthenticator.");
                                }
                                GameManager.Instance.SetTestParameters(maxNumberRange, numOperations, requiredCorrectAnswersMinimumPercent, uid);
                                break;
                            }
                        }

                        if (hasValidTest)
                        {
                            statusText.text = "Test trouvé ! Chargement de VerticalOperationScene...";
                            Debug.Log("Chargement de VerticalOperationScene avec les paramètres définis.");
                            UnityEngine.SceneManagement.SceneManager.LoadScene("VerticalOperationsScene");
                        }
                        else
                        {
                            statusText.text = "Aucun test de multiplication verticale trouvé.";
                            Debug.LogError("Aucun test valide trouvé pour l'étudiant.");
                        }
                    }
                    else
                    {
                        statusText.text = "Échec du chargement des tests.";
                        Debug.LogError("Échec de la récupération des tests depuis Firebase.");
                    }
                });
            }
            else
            {
                statusText.text = "Échec du chargement des données du joueur.";
                Debug.LogError($"Utilisateur {uid} non trouvé dans Firebase.");
            }
        });
    }
}