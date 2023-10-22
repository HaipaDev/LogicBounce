using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Boombox : MonoBehaviour{
    bool startedPlaying;
    bool disabled;
    bool disabledFxPlayed;
    void Update(){
        if(startedPlaying&&!GetComponent<AudioSource>().isPlaying){
            Disable();
        }
        if(GetComponent<AudioSource>().isPlaying){
            if(Jukebox.instance!=null){
                Jukebox.instance.PauseMusic();
                // Jukebox.instance.MuteMusic();
            }
        }else{
            if(Jukebox.instance!=null){
                Jukebox.instance.UnPauseMusic();
                // Jukebox.instance.UnmuteMusic();
            }
        }
    }
    void OnDestroy(){
        if(Jukebox.instance!=null){
            Jukebox.instance.UnPauseMusic();
            // Jukebox.instance.UnmuteMusic();
        }
    }
    void OnMouseDown(){
        if(!disabled){
            Play();
        }
    }
    void Play(){
        if(!GetComponent<AudioSource>().isPlaying){
            GetComponent<AudioSource>().Play();
            transform.GetChild(1).GetComponent<ParticleSystem>().Play();
            transform.GetChild(2).GetComponent<ParticleSystem>().Play();
            transform.GetChild(3).GetComponent<ParticleSystem>().Stop();
            startedPlaying=true;
        }
    }
    public void Enable(){
        startedPlaying=false;
        disabled=false;
        disabledFxPlayed=false;
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void EnableAndPlay(){
        Enable();
        Play();
    }
    public void Disable(){
        startedPlaying=false;
        disabled=true;
        GetComponent<AudioSource>().Stop();
        if(!disabledFxPlayed){
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
            transform.GetChild(2).GetComponent<ParticleSystem>().Stop();
            transform.GetChild(3).GetComponent<ParticleSystem>().Play();
            disabledFxPlayed=true;
        }
    }
}