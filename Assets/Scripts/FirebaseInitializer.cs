using Firebase; // importe les fonctions de base de Firebase
using Firebase.Auth;
using Firebase.Database; // manipuler la base de donn�es Firebase (�criture/lecture)
using Firebase.Extensions;
using UnityEngine;
using System;  // pour utiliser des types comme Action
using System.Threading.Tasks; // Task pour g�rer les actions asynchrones qui s'ex�cutent en arri�re-plan

public class FirebaseInitializer : MonoBehaviour // une classe h�rite de MonoBehaviour, donc elle peut �tre attach�e � un GameObject
{
    // cr�e un singleton
    private static FirebaseInitializer _instance;
    public static FirebaseInitializer Instance => _instance; //propri�t� en lecture seule qui retourne la valeur de la variable priv�e _instance.

    public FirebaseAuth Auth { get; private set; } // , Je peux lire Auth depuis n�importe o�  mais je ne peux pas modifier sa valeur depuis l�ext�rieur
    public DatabaseReference DbReference { get; private set; }
    public bool IsFirebaseInitialized { get; private set; }  // bool�en  indique si Firebase est bien initialis� ou pas.


    private Task<DependencyStatus> initTask; //initTask contient  une t�che asynchrone qui verifie que toutes les d�pendances Firebase sont bien install�es.


    //m�thode est appel�e automatiquement d�s que l�objet est cr�� (avant m�me Start())
    private void Awake()
    {

        // Si une autre instance existe d�j�, on d�truit celle-ci (on garde UNE SEULE INSTANCE !).
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // si n'existe pas dea une instance on  enregistre cette instance comme l�unique (singleton)
        _instance = this;
        DontDestroyOnLoad(gameObject); // pour garde cet objet m�me quand on change de sc�ne ( utile pour garder les donn�es Firebase actives).
    }


    // methode qui initialise Firebase
    public void InitializeFirebase(Action onInitialized)  // onInitialized est une fonction qu�on ex�cutera une fois que Firebase est pr�t.

    {
        if (IsFirebaseInitialized) // Si d�j� initialis�
        {
            onInitialized?.Invoke(); // Si onInitialized n�est pas nul, appelle-le
            return; // on termine 
        }


        // Si on n�a pas encore commenc� l'initialisation (initTask == null), alors on lance tache qui v�rifie que tout est pr�t c�t� Firebase.


        if (initTask == null)
        {
            initTask = FirebaseApp.CheckAndFixDependenciesAsync();
        }
        // Quand la t�che est termin�e

        initTask.ContinueWithOnMainThread(task =>
        {
            //  Si tout est OK 
            if (task.Result == DependencyStatus.Available)
            {

                // D�sactiver le cache local AVANT d�utiliser la base de donn�es
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
