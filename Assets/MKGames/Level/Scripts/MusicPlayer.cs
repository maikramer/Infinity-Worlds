using System;
using System.Collections.Generic;
using MkGames;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : Singleton<MusicPlayer>
{
	[HideInInspector] public List<AudioTrack> trackList;

	private AudioSource music;

	void OnEnable()
	{
		music = GetComponent<AudioSource>();
		SceneManager.sceneLoaded += OnSceneLoaded;
		DontDestroyOnLoad(this);
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		music.Stop();
		foreach (var audioTrack in trackList)
		{
			if (audioTrack.sceneName == scene.name)
			{
				music.clip = audioTrack.audioClip;
				music.loop = audioTrack.loop;
				
				if (!music.clip)
					return;
				
				if (audioTrack.playAtStart)
					music.Play();
				
				break;
			}
		}
	}

	[Serializable]
	public struct AudioTrack
	{
		public string clipName;
		public AudioClip audioClip;
		public string sceneName;
		public bool loop;
		public bool playAtStart;
	}
}