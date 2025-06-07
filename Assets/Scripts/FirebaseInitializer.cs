using Firebase; // importe les fonctions de base de Firebase
using Firebase.Auth;
using Firebase.Database; // manipuler la base de données Firebase (écriture/lecture)
using Firebase.Extensions;
using UnityEngine;
using System;  // pour utiliser des types comme Action
using System.Threading.Tasks; // Task pour gérer les actions asynchrones qui s'exécutent en arrière-plan

public class FirebaseInitializer : MonoBehaviour // une classe hérite de MonoBehaviour, donc elle peut être attachée à un GameObject
{
    // crée un singleton
    private static FirebaseInitializer _instance;
    public static FirebaseInitializer Instance => _instance; //propriété en lecture seule qui retourne la valeur de la variable privée _instance.

    public FirebaseAuth Auth { get; private set; } // , Je peux lire Auth depuis n’importe où  mais je ne peux pas modifier sa valeur depuis l’extérieur
    public DatabaseReference DbReference { get; private set; }
    public bool IsFirebaseInitialized { get; private set; }  // booléen  indique si Firebase est bien initialisé ou pas.


    private Task<DependencyStatus> initTask; //initTask contient  une tâche asynchrone qui verifie que toutes les dépendances Firebase sont bien installées.


    //méthode est appelée automatiquement dès que l’objet est créé (avant même Start())
    private void Awake()
    {

        // Si une autre instance existe déjà, on détruit celle-ci (on garde UNE SEULE INSTANCE !).
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // si n'existe pas dea une instance on  enregistre cette instance comme l’unique (singleton)
        _instance = this;
        DontDestroyOnLoad(gameObject); // pour garde cet objet même quand on change de scène ( utile pour garder les données Firebase actives).
    }


    // methode qui initialise Firebase
    public void InitializeFirebase(Action onInitialized)  // onInitialized est une fonction qu’on exécutera une fois que Firebase est prêt.

    {
        if (IsFirebaseInitialized) // Si déjà initialisé
        {
            onInitialized?.Invoke(); // Si onInitialized n’est pas nul, appelle-le
            return; // on termine 
        }


        // Si on n’a pas encore commencé l'initialisation (initTask == null), alors on lance tache qui vérifie que tout est prêt côté Firebase.


        if (initTask == null)
        {
            initTask = FirebaseApp.CheckAndFixDependenciesAsync();
        }
        // Quand la tâche est terminée

        initTask.ContinueWithOnMainThread(task =>
        {
            //  Si tout est OK 
            if (task.Result == DependencyStatus.Available)
            {

                // Désactiver le cache local AVANT d’utiliser la base de données
                FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);

                Auth = FirebaseAuth.DefaultInstance;
                DbReference = FirebaseDatabase.DefaultInstance.RootReference;
                IsFirebaseInitialized = true;
                Debug.Log("Firebase initialized.");
                onInitialized?.Invoke();
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
    }
}
