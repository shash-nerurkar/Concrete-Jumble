using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum GameStates {
    InMenu,
    InGame
}

public class Main : MonoBehaviour
{
    public static bool IsMuted;
    public static float ScreenHeightHalved;
    public static GameStates GameState;
    public static int distanceFromPTTeacherText;
    public static float currentPlaythroughTime;
    public static Vector2 throwDirection;
    public static bool IsWeaponMidAir;

    

    public SoundsMain soundsMain;
    public HUD hud;
    public GameObject GameEntities;
    public Player player;
    public Player2 player2;
    public Player2 referee;
    public Weapon weapon;


    private bool shouldDialogPlay;
    private bool IsDialogPlaying;
    public Dictionary<Entity, string>[] dialogsList;
    public int currentDialogIndex = 0;
    Timer tutorialShowTimer;

    void AddDialogs () {
        dialogsList = new Dictionary<Entity, string>[] {
            new Dictionary<Entity, string> {
                { player2.Entity, "Yo man, CHILL" },
                { referee.Entity, "Woah, he just said \"Yo mama's filled\", you gonna take that bro?" },
                { player.Entity, "COME HERE, YOU LITTLE PRICK" },
                { player2.Entity, "That's NOT WHAT I SAID" },
            },
        };
    }

    async void PlayNextDialog () {
        IsDialogPlaying = true;
        
        Dictionary<Entity, string> dialogsDict = dialogsList [ currentDialogIndex ];

        foreach ( KeyValuePair<Entity, string> dialog in dialogsDict ) {
            dialog.Key.ShowDialog ( dialogText: dialog.Value, millisecondsDelay: 3000 );
            await Task.Delay ( millisecondsDelay: 3000 );
        }

        currentDialogIndex = ( currentDialogIndex < dialogsList.Length - 1 ) ? ( currentDialogIndex + 1 ) : 0;

        IsDialogPlaying = false;
    }

    void Awake () {
        shouldDialogPlay = true;
        IsMuted = false;
        ScreenHeightHalved = ( Camera.main.orthographicSize * Screen.width / Screen.height ) / 2;

        Entity.OnEntityHitAction += ChangeThrowDirection;

        tutorialShowTimer = gameObject.AddComponent<Timer> ();

        AddDialogs ();
        
        SetNewGameParams ();

        // StartGame();
        StartMenu ();
    }

    void Update () {
        if ( GameState == GameStates.InGame ) {
            currentPlaythroughTime += Time.deltaTime;

            if( !IsDialogPlaying ) {
                if ( shouldDialogPlay )
                    PlayNextDialog ();
                
                shouldDialogPlay = Random.Range ( 0, 1 ) > 0.95f;
            }
        }
    }

    void SetNewGameParams () {
        distanceFromPTTeacherText = 5;
        currentPlaythroughTime = 0;
        throwDirection = Vector2.right;
        IsDialogPlaying = false;
        IsWeaponMidAir = false;
    }

    public void StartGame () {
        GameState = GameStates.InGame;

        soundsMain.uiMusic.Stop ();
        soundsMain.inGameMusic.Play ();

        hud.InGameHUD.SetActive ( true );
        hud.PausePanel.SetActive ( false );
        hud.EndgamePanel.SetActive ( false );
        hud.Tutorial.SetActive ( true );
        hud.MainMenu.SetActive ( false );
        GameEntities.SetActive ( true );

        tutorialShowTimer.StartTimer ( maxTime: 10, onTimerFinish: OnTutorialTimerFinished );

        SetNewGameParams ();

        player.EnableActions ();
        hud.SetScrolling ( scrollSpeed: throwDirection  / 10 );
    }

    public void StartMenu () {
        GameState = GameStates.InMenu;

        soundsMain.uiMusic.Play ();
        soundsMain.inGameMusic.Stop ();

        ResetGameEntities ();

        hud.InGameHUD.SetActive ( false );
        hud.PausePanel.SetActive ( false );
        hud.EndgamePanel.SetActive ( false );
        hud.Tutorial.SetActive ( false );
        GameEntities.SetActive ( false );
        hud.MainMenu.SetActive ( true );

        player.DisableActions ();
        hud.SetScrolling ( scrollSpeed: Vector2.zero );
    }

    void ResetGameEntities () {
        player.transform.SetParent ( GameEntities.transform );
        player.Entity.ChangeState ( EntityState.Attacking );
        player.Entity.Canvas.SetActive ( false );
        player.transform.position = new Vector3 ( -2, 0, 0 );

        weapon.transform.SetParent ( player.Entity.HeldPivot.transform );
        weapon.Entity.ChangeState ( EntityState.Held );
        player.Entity.Canvas.SetActive ( false );
        weapon.transform.localPosition = new Vector3 ( 0, 0, 0 );

        player2.transform.SetParent( GameEntities.transform );
        player2.Entity.ChangeState ( EntityState.Defending );
        player.Entity.Canvas.SetActive ( false );
        player2.transform.position = new Vector3 ( 2, 0, 0 );

        referee.transform.SetParent( GameEntities.transform );
        referee.Entity.ChangeState ( EntityState.Refereeing );
        player.Entity.Canvas.SetActive ( false );
        referee.transform.position = new Vector3 ( 0, 1, 0 );
    }

    public void RetryGame () {
        player.DisableActions ();
        hud.SetScrolling ( scrollSpeed: Vector2.zero );

        ResetGameEntities ();

        StartGame ();
    }

    void OnTutorialTimerFinished () => hud.Tutorial.SetActive ( false );

    void ChangeThrowDirection () {
        throwDirection = throwDirection != Vector2.right ? Vector2.right : Vector2.left;

        hud.SetScrolling ( scrollSpeed: throwDirection );
    }

    void OnDestroy () {
        Entity.OnEntityHitAction -= ChangeThrowDirection;
    }
}
