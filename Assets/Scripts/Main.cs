using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum GameStates {
    InMenu,
    InGame
}

public class Main : MonoBehaviour
{
    public static bool IsMuted;
    public static float ScreenHeightHalved;
    public static GameStates GameState;
    public static float PlayerHealth;
    public static float currentPlaythroughTime;
    public static Vector2 throwDirection;

    

    public Animator CameraAnimator;
    public ParticleSystem ParticleSystem;
    public SoundsMain soundsMain;
    public HUD hud;
    public GameObject GameEntities;
    public Player player;
    public Player2 player2;
    public Player2 referee;
    public Weapon weapon;


    private bool shouldExchangePlay;
    private bool IsExchangePlaying;
    public String[][] exchangesList;
    public int currentExchangeIndex;
    public int currentDialogueIndex;
    Timer tutorialShowTimer;
    Timer dialogTimer;


    void AddDialogs () {
        exchangesList = new String[][] {
            new String[] {
                "2:Yo Brown, CHILL, MAN, don't pass it to me",
                "3:Haha",
                "1:GET THIS THING OFF OF ME BLUE",
                "2:NO",
                "1:I DON'T WANT NOTHING TO DO WITH NO INFECTED BOOMERANGS, MAN",
                "2:PLEASE man, TAKE the L, and KEEP THE INFECTION AWAY FROM ME",
                "3:Finally some energy on the pavement, I LOVE it"
            },
            new String[] {
                "3:ahahaha these goons",
            },
            new String[] {
                "1:WE'VE KNOWN EACH OTHER FOR 7 YEARS, MAN",
                "2:SO YOU'D RATHER LET ME DIE OVER YOU?"
            },
            new String[] {
                "3:asli hai *hums*",
            },
            new String[] {
                "3:heuheuheuheueheu",
            },
            new String[] {
                "2:WHY ARE YOU RUNNING WITH US ANYWAY, GREEN?",
                "3:I'm the referee, obviously",
                "1:THIS IS A GAME TO YOU?",
            },
            new String[] {
                "3:khikhikhikhi",
            },
            new String[] {
                "3:bhag bhag bhag aaya sher aaya sher",
                "2:ENOUGH WITH THE OBSCURE RAP REFERENCES"
            },
            new String[] {
                "3:You guys ever heard \"We gunna run this town toniiight\" by Jay-Z?",
                "1: PLEASE SHUT UP"
            },
        };
    }

    void PlayNextDialog () {
        IsExchangePlaying = true;
        
        String dialog = exchangesList [ currentExchangeIndex ] [ currentDialogueIndex ];
        String[] subparts;
        Entity entity;


        if ( currentDialogueIndex > 0 ) {
            subparts = exchangesList [ currentExchangeIndex ] [ currentDialogueIndex - 1 ].Split( ":" );
            switch ( subparts[ 0 ] )  {
                case "1":
                    entity = player.Entity;
                    break;
                
                case "2":
                    entity = player2.Entity;
                    break;
                
                default:
                case "3":
                    entity = referee.Entity;
                    break;
            }
            entity.ToggleDialog ( shouldShow: false, dialogText: null );
        }


        subparts = dialog.Split(":");
        switch ( subparts[ 0 ] )  {
            case "1":
                entity = player.Entity;
                break;
            
            case "2":
                entity = player2.Entity;
                break;
            
            default:
            case "3":
                entity = referee.Entity;
                break;
        }
        entity.ToggleDialog ( shouldShow: true, dialogText: subparts[ 1 ] );


        if ( currentDialogueIndex == exchangesList [ currentExchangeIndex ].Length - 1 ) {
            currentDialogueIndex = 0;
            currentExchangeIndex = ( currentExchangeIndex < exchangesList.Length - 1 ) ? ( currentExchangeIndex + 1 ) : 1;
            IsExchangePlaying = false;
        }
        else {
            currentDialogueIndex++;
            dialogTimer.StartTimer ( maxTime: 7, onTimerFinish: PlayNextDialog );
        }
    }


    void Awake () {
        shouldExchangePlay = true;
        IsMuted = false;
        ScreenHeightHalved = ( Camera.main.orthographicSize * Screen.width / Screen.height ) / 2 - 0.3f;

        tutorialShowTimer = gameObject.AddComponent<Timer> ();
        dialogTimer = gameObject.AddComponent<Timer> ();

        AddDialogs ();
        
        SetNewGameParams ();

        soundsMain.inGameMusic.Play ();
        StartMenu ();
    }

    void Update () {
        if ( GameState == GameStates.InGame ) {
            currentPlaythroughTime += Time.deltaTime;

            SetDifficulty ();

            if( !IsExchangePlaying ) {
                if ( shouldExchangePlay )
                    PlayNextDialog ();
                
                shouldExchangePlay = Random.Range ( 0, 1 ) > 0.75f;
            }
        }
    }


    public void StartGame () {
        GameState = GameStates.InGame;

        if ( !soundsMain.inGameMusic.isPlaying )
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

        ResetGameEntities ();

        hud.InGameHUD.SetActive ( false );
        hud.PausePanel.SetActive ( false );
        hud.EndgamePanel.SetActive ( false );
        hud.Tutorial.SetActive ( false );
        GameEntities.SetActive ( false );
        hud.MainMenu.SetActive ( true );

        player.DisableActions ();
    }

    public void RetryGame () {
        ResetGameEntities ();

        StartGame ();
    }


    void SetDifficulty () {
        switch ( ( int ) currentPlaythroughTime ) {
            case 0:
                weapon.Entity.MoveSpeed = 2.0f;
                player2.Entity.MoveSpeed = 1.0f;
                referee.Entity.MoveSpeed = 1.5f;
                soundsMain.inGameMusic.pitch = 1.0f;
                break;
            
            case 30:
                weapon.Entity.MoveSpeed = 2.0f + 0.05f;
                player2.Entity.MoveSpeed = 1.0f + 0.05f;
                referee.Entity.MoveSpeed = 1.5f + 0.05f;
                soundsMain.inGameMusic.pitch = 1.0f + 0.05f;
                break;
            
            case 45:
                weapon.Entity.MoveSpeed = 2.0f + 0.05f + 0.05f;
                player2.Entity.MoveSpeed = 1.0f + 0.05f + 0.05f;
                referee.Entity.MoveSpeed = 1.5f + 0.05f + 0.05f;
                soundsMain.inGameMusic.pitch = 1.0f + 0.05f + 0.05f;
                break;

            case 60:
                weapon.Entity.MoveSpeed = 2.0f + 0.05f + 0.05f + 0.05f;
                player2.Entity.MoveSpeed = 1.0f + 0.05f + 0.05f + 0.05f;
                referee.Entity.MoveSpeed = 1.5f + 0.05f + 0.05f + 0.05f;
                soundsMain.inGameMusic.pitch = 1.0f + 0.05f + 0.05f + 0.05f;
                break;
            
            case 120:
                weapon.Entity.MoveSpeed = 2.0f + 0.05f + 0.05f + 0.05f + 0.05f;
                player2.Entity.MoveSpeed = 1.0f + 0.05f + 0.05f + 0.05f + 0.05f;
                referee.Entity.MoveSpeed = 1.5f + 0.05f + 0.05f + 0.05f + 0.05f;
                soundsMain.inGameMusic.pitch = 1.0f + 0.05f + 0.05f + 0.05f + 0.05f;
                break;
            
            case 180:
                weapon.Entity.MoveSpeed = 2.0f + 0.05f + 0.05f + 0.05f + 0.05f + 0.05f;
                player2.Entity.MoveSpeed = 1.0f + 0.05f + 0.05f + 0.05f + 0.05f + 0.05f;
                referee.Entity.MoveSpeed = 1.5f + 0.05f + 0.05f + 0.05f + 0.05f + 0.05f;
                soundsMain.inGameMusic.pitch = 1.0f + 0.05f + 0.05f + 0.05f + 0.05f + 0.05f;
                break;
            
            default:
                break;
        }
    }

    void SetNewGameParams () {
        currentExchangeIndex = 0;
        currentDialogueIndex = 0;
        PlayerHealth = 20;
        hud.HealthBarSlider.maxValue = PlayerHealth;
        currentPlaythroughTime = 0;
        ChangeThrowDirection ( Vector2.right );
        IsExchangePlaying = false;
    }

    void ResetGameEntities () {
        player.Entity.Reset (
            parent: GameEntities.transform,
            state: EntityState.Attacking,
            position: new Vector3 ( -2, 0, 0 ),
            CurrentHolder: null,
            CurrentHeld: weapon.Entity
        );
        
        weapon.Entity.Reset (
            parent: player.Entity.HeldPivot.transform,
            state: EntityState.Held,
            position: new Vector3 ( 0, 0, 0 ),
            CurrentHolder: player.Entity,
            CurrentHeld: null
        );
        
        player2.Entity.Reset (
            parent: GameEntities.transform,
            state: EntityState.Defending,
            position: new Vector3 ( 2, 0, 0 ),
            CurrentHolder: null,
            CurrentHeld: null
        );
        
        referee.Entity.Reset (
            parent: GameEntities.transform,
            state: EntityState.Refereeing,
            position: new Vector3 ( 0, 1, 0 ),
            CurrentHolder: null,
            CurrentHeld: null
        );
    }

    void OnTutorialTimerFinished () => hud.Tutorial.SetActive ( false );

    public void ChangeThrowDirection ( Vector2 throwDir ) {
        Vector3 xDir;

        if ( throwDir == Vector2.zero )
            throwDirection = throwDirection != Vector2.right ? Vector2.right : Vector2.left;
        else
            throwDirection = throwDir;
        
        if ( throwDirection == Vector2.right ) {
            ParticleSystem.transform.position = new Vector3 ( 3.5f, 0, 0 );
            ParticleSystem.transform.rotation = Quaternion.Euler ( 180, 90, -90 );

            xDir = new Vector3 ( 1, 1, 1 );
        }
        else {
            ParticleSystem.transform.position = new Vector3 ( -3.5f, 0, 0 );
            ParticleSystem.transform.rotation = Quaternion.Euler ( 0, 90, -90 );

            xDir = new Vector3 ( -1, 1, 1 );
        }

        hud.SetScrolling ( scrollSpeed: throwDirection / 10 );

        foreach ( Entity entity in new Entity[] { player.Entity, weapon.Entity, player2.Entity, referee.Entity } ) {
            entity.transform.localScale = xDir;
            entity.Canvas.transform.localScale = xDir;
        }
    }
}
