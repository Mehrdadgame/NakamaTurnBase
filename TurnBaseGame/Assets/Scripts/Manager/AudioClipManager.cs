using NinjaBattle.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// this class for manager sound 
/// </summary>
public class AudioClipManager : MonoBehaviour
{
    public AudioClip WinAudioClip;
    public AudioClip LoosAudioClip;
    public AudioClip drawAudioClip;

    public static AudioClipManager instance;

    private void Start()
    {
        instance = this;
    }
    public void PlayAudioClip(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }

  /// <summary>
  /// It plays a sound based on the result of the game
  /// </summary>
  /// <param name="ResultGame">is an enum that contains the result of the game (win, loose,
  /// draw)</param>
    public void PlaySoundResultGame(ResultGame result)
    {
        switch (result)
        {
            case ResultGame.win:
                AudioSource.PlayClipAtPoint(WinAudioClip, Camera.main.transform.position);
                break;
            case ResultGame.loose:
                AudioSource.PlayClipAtPoint(LoosAudioClip, Camera.main.transform.position);
                break;
            case ResultGame.draw:
                AudioSource.PlayClipAtPoint(drawAudioClip, Camera.main.transform.position);
                break;
            default:
                break;
        }
    }
}
