using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource music;
    public Slider volumeSlider;
    private float volume;

    private void Start()
    {
        volume = NetworkManager.instance.volume;
        if (volumeSlider != null)
            volumeSlider.value = volume;
        music.volume = volume;
    }

    public void HandleSliderValueChanged (float value)
    {
        volume = value;
        NetworkManager.instance.volume = volume;
        music.volume = volume;
    }
}
