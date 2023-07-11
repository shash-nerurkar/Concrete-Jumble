using UnityEngine;

public class SoundsMain : MonoBehaviour
{
    [Header ( "Music" )]
    public AudioSource inGameMusic;

    [Header ( "Ambient Sounds" )]
    public AudioSource onButtonClick;
    public AudioSource onEntityHit;
    public AudioSource onEntityThrow;


    public void VolumeToggle () {
        inGameMusic.mute = Main.IsMuted;
        onButtonClick.mute = Main.IsMuted;
        onEntityHit.mute = Main.IsMuted;
        onEntityThrow.mute = Main.IsMuted;
    }
}
