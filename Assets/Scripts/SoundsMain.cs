using UnityEngine;

public class SoundsMain : MonoBehaviour
{
    [Header ( "Music" )]
    public AudioSource uiMusic;
    public AudioSource inGameMusic;

    [Header ( "Ambient Sounds" )]
    public AudioSource onButtonClick;


    public void VolumeToggle () {
        uiMusic.mute = Main.IsMuted;
        inGameMusic.mute = Main.IsMuted;
        onButtonClick.mute = Main.IsMuted;
    }
}
