using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private GameObject AudioObjects;

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("AttackHandler");
                    instance = obj.AddComponent<AudioManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        // ½Ì±ÛÅæÀ» À§ÇÑ ÄÚµå
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    public void PlayAudio(AudioClip audio)
    {
        GameObject soundObject = new GameObject();
        AudioSource sound = soundObject.AddComponent<AudioSource>();
        sound.clip = audio;
        soundObject.transform.position = AudioObjects.transform.position;
        soundObject.transform.SetParent(AudioObjects.transform);
        sound.Play();

        soundObject.name = audio.name;

        StartCoroutine(DestroySound(soundObject));

        IEnumerator DestroySound(GameObject obj)
        {
            yield return new WaitForSeconds(sound.clip.length + .1f);
            Destroy(obj);
            yield return null;
        }
    }
}
