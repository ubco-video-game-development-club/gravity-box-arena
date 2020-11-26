using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    private const string MASTER_VOL_PREF = "settings.mastervolume",
        MUSIC_VOL_PREF = "settings.musicvolume",
        SFX_VOL_PREF = "settings.sfxvolume",
        UI_VOL_PREF = "settings.uivolume";
    
    private const string MASTER_MIX_VOL = "MasterVolume",
        MUSIC_MIX_VOL = "MusicVolume",
        SFX_MIX_VOL = "SFXVolume",
        UI_MIX_VOL = "UIVolume";

    [SerializeField] private TMPro.TextMeshProUGUI highscoreText;
    [SerializeField] private string playScene = "Scenes/Game";
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;

    void Start()
    {
        int highScore = PlayerPrefs.GetInt(ScoreSystem.HIGH_SCORE_PREF);
        highscoreText.text = $"High Score: {highScore}";

        SetVolumeSliders();
    }

    public void OnPlayButtonClicked()
    {
        //TODO: Load the scene in a more elegant way (?)
        SceneManager.LoadScene(playScene);
    }

    public void OnMasterVolumeSliderChanged(float value)
    {
        masterMixer.SetFloat(MASTER_MIX_VOL, value);
        PlayerPrefs.SetFloat(MASTER_VOL_PREF, value);
    }

    public void OnMusicVolumeSliderChanged(float value)
    {
        masterMixer.SetFloat(MUSIC_MIX_VOL, value);
        PlayerPrefs.SetFloat(MUSIC_VOL_PREF, value);
    }

    public void OnSFXVolumeSliderChanged(float value)
    {
        masterMixer.SetFloat(SFX_MIX_VOL, value);
        PlayerPrefs.SetFloat(SFX_VOL_PREF, value);
    }

    public void OnUIVolumeSliderChanged(float value)
    {
        masterMixer.SetFloat(UI_MIX_VOL, value);
        PlayerPrefs.SetFloat(UI_VOL_PREF, value);
    }

    private void SetVolumeSliders()
    {
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOL_PREF);
        masterVolumeSlider.value = masterVolume;
        masterMixer.SetFloat(MASTER_MIX_VOL, masterVolume);

        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_PREF);
        musicVolumeSlider.value = musicVolume;
        masterMixer.SetFloat(MUSIC_MIX_VOL, musicVolume);

        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_PREF);
        sfxVolumeSlider.value = sfxVolume;
        masterMixer.SetFloat(SFX_MIX_VOL, sfxVolume);

        float uiVolume = PlayerPrefs.GetFloat(UI_VOL_PREF);
        uiVolumeSlider.value = uiVolume;
        masterMixer.SetFloat(UI_MIX_VOL, uiVolume);
    }
}
