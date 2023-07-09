using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    public SoundsMain soundsMain;
    public Main main;
    
    [Header ( "Ever-present" )]
    public RawImage backgroundImage;
    public Vector2 backgroundScrollSpeed;
    public Button volumeButton;
    public Sprite mutedSprite;
    public Sprite unmutedSprite;


    [Header ( "Main Menu" )]
    public GameObject MainMenu;


    [Header ( "In-Game HUD" )]
    public GameObject InGameHUD;
    public TextMeshProUGUI distanceFromPTTeacherText;
    public TextMeshProUGUI timerText;



    [Header ( "Pause Panel" )]
    public GameObject PausePanel;


    [Header ( "Endgame Panel" )]
    public GameObject EndgamePanel;
    public TextMeshProUGUI endGameTimeText;


    [Header ( "Tutorial" )]
    public GameObject Tutorial;


    [Header ( "Role Reversal" )]
    public RoleReversalPanel RoleReversalPanel;

    void Awake () {
        Entity.OnEntityHitAction += RoleReversalPanel.ReverseRoles;
    }

    void OnDestroy () {
        Entity.OnEntityHitAction -= RoleReversalPanel.ReverseRoles;
    }

    void Update () {
        backgroundImage.uvRect = new Rect ( backgroundImage.uvRect.position + backgroundScrollSpeed * Time.deltaTime, backgroundImage.uvRect.size );

        SetDistanceFromPETeacherText ();
        
        SetTimerText ();
    }

    public void SetScrolling ( Vector2 scrollSpeed ) => backgroundScrollSpeed = scrollSpeed;

    public void SetDistanceFromPETeacherText () => distanceFromPTTeacherText.text = "Distance from PE Teach: " + Main.distanceFromPTTeacherText.ToString () + " m";

    public void SetTimerText () {
        int min = ( int ) Main.currentPlaythroughTime / 60;
        int seconds = ( int ) Main.currentPlaythroughTime % 60;
        
        timerText.text = 
            ( ( min < 10 ? "0" : "" ) + min.ToString () ) + 
            ":" + 
            ( ( seconds < 10 ? "0" : "" ) + seconds.ToString () );
    }

    public void OnPlayGamePressed () {
        soundsMain.onButtonClick.Play ();

        main.StartGame ();
    }
    
    public void OnExitGamePressed () {
        soundsMain.onButtonClick.Play ();

        Application.Quit ();
    }

    public void OnPauseGamePressed () {
        soundsMain.onButtonClick.Play ();

        Time.timeScale = 0;

        PausePanel.SetActive ( true );

        Tutorial.SetActive ( true );
    }

    public void OnVolumeToggled () {
        Main.IsMuted = !Main.IsMuted;

        soundsMain.VolumeToggle ();

        volumeButton.image.sprite = Main.IsMuted ? mutedSprite : unmutedSprite;
    }

    public void OnResumeGamePressed () {
        soundsMain.onButtonClick.Play ();

        Time.timeScale = 1;

        PausePanel.SetActive ( false );

        Tutorial.SetActive ( false );
    }

    public void OnReturnToMenuPressed () {
        OnResumeGamePressed ();

        main.StartMenu ();
    }

    public void OnGameEnded () {
        soundsMain.onButtonClick.Play ();

        EndgamePanel.SetActive ( true );
    }

    public void OnRetryGamePressed () {
        OnResumeGamePressed ();

        main.RetryGame ();
    }
}