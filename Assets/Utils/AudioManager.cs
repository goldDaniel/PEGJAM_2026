using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel
{
	SFX,
	Music,
	UI
}

[Serializable]
public struct AudioBankEntry
{
	public string name;
	public AudioClip clip;
	public AudioChannel channel;
	public bool loop;
	public float defaultVolume;
}

public class AudioManager : MonoSingleton<AudioManager>
{
	[Header("Audio Source Pool")]
	[SerializeField, Min(1)] private int _sfxPoolSize = 16;
	[SerializeField, Min(2)] private int _musicPoolSize = 2;
	[SerializeField, Min(1)] private int _uiPoolSize = 8;

	[Header("Mixer Groups")]
	[SerializeField] private AudioMixerGroup _sfxMixer;
	[SerializeField] private AudioMixerGroup _musicMixer;
	[SerializeField] private AudioMixerGroup _uiMixer;

	[Header("Audio Bank")]
	[SerializeField] private AudioBankEntry[] _audioBank;

	private readonly Dictionary<string, AudioBankEntry> _bankMap = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<AudioChannel, AudioSource[]> _pools = new();
	private readonly Dictionary<AudioChannel, int> _poolIndices = new();

	private int _currentMusicIndex;
	private int _musicTransitionId;
	private Coroutine _musicTransitionCoroutine;

	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);

		_bankMap.Clear();
		_pools.Clear();
		_poolIndices.Clear();
		_currentMusicIndex = 0;
		_musicTransitionId = 0;
		_musicTransitionCoroutine = null;

		BuildBankMap();
		CreatePool(AudioChannel.SFX, _sfxPoolSize, _sfxMixer);
		CreatePool(AudioChannel.Music, _musicPoolSize, _musicMixer);
		CreatePool(AudioChannel.UI, _uiPoolSize, _uiMixer);

        Instance.PlayMusicCrossfade("MainMenu");
    }

	private void BuildBankMap()
	{
		if (_audioBank == null)
			return;

		foreach (var entry in _audioBank)
		{
			if (string.IsNullOrWhiteSpace(entry.name))
			{
				Debug.LogWarning("Audio bank contains an entry with an empty name.");
				continue;
			}

			if (entry.clip == null)
			{
				Debug.LogWarning($"Audio bank entry '{entry.name}' has no clip assigned.");
				continue;
			}

			if (!_bankMap.TryAdd(entry.name, entry))
				Debug.LogWarning($"Duplicate audio bank entry '{entry.name}'. Keeping the first one.");
		}
	}

	private void CreatePool(AudioChannel channel, int size, AudioMixerGroup mixer)
	{
		static AudioSource CreateAudioSource(AudioChannel channel, AudioMixerGroup mixer, Transform parent)
		{
			var obj = new GameObject($"{channel}AudioSource");
			obj.transform.parent = parent;
			var src = obj.AddComponent<AudioSource>();
			src.playOnAwake = false;
			src.outputAudioMixerGroup = mixer;
			src.spatialBlend = 0f;
			return src;
		}

		var sources = new AudioSource[size];
		for (int i = 0; i < size; ++i)
			sources[i] = CreateAudioSource(channel, mixer, this.transform);

		_pools[channel] = sources;
		_poolIndices[channel] = 0;
	}

	public void Play(string soundName, float? volumeOverride = null, float pitch = 1f)
	{
		if (!TryGetEntry(soundName, out var entry))
			return;

		var source = GetNextSource(entry.channel);
		source.clip = entry.clip;
		source.loop = entry.loop;
		source.volume = Mathf.Clamp01(volumeOverride ?? entry.defaultVolume);
		source.pitch = pitch;
		source.Play();
	}

	public void PlayMusicCrossfade(string musicName, float fadeDuration = 1f)
	{
		if (!_pools.TryGetValue(AudioChannel.Music, out var musicSources) || musicSources.Length == 0)
			return;

		fadeDuration = Mathf.Max(0f, fadeDuration);
		var currentSource = musicSources[_currentMusicIndex];

		if (string.IsNullOrWhiteSpace(musicName))
		{
			StartMusicTransition(null, fadeDuration, 0f);
			return;
		}

		if (!TryGetEntry(musicName, out var entry) || entry.channel != AudioChannel.Music)
		{
			Debug.LogWarning($"Missing or invalid music track: {musicName}");
			return;
		}

		if (currentSource.isPlaying && currentSource.clip == entry.clip)
			return;

		var nextIndex = (_currentMusicIndex + 1) % musicSources.Length;
		var toSource = musicSources[nextIndex];
		toSource.Stop();
		toSource.clip = entry.clip;
		toSource.loop = entry.loop;
		toSource.volume = 0f;
		toSource.pitch = 1f;
		toSource.Play();

		_currentMusicIndex = nextIndex;
		StartMusicTransition(toSource, fadeDuration, Mathf.Clamp01(entry.defaultVolume));
	}

	public void FadeOutAllMusic(float duration = 1f)
	{
		StartMusicTransition(null, Mathf.Max(0f, duration), 0f);
	}

	public void StopAll(AudioChannel channel)
	{
		if (!_pools.TryGetValue(channel, out var sources))
			return;

		if (channel == AudioChannel.Music)
			CancelMusicTransition();

		foreach (var source in sources)
			source.Stop();
	}

	private bool TryGetEntry(string soundName, out AudioBankEntry entry)
	{
		if (string.IsNullOrWhiteSpace(soundName))
		{
			entry = default;
			Debug.LogWarning("AudioManager received an empty sound name.");
			return false;
		}

		if (!_bankMap.TryGetValue(soundName, out entry))
		{
			Debug.LogWarning($"Missing audio clip: {soundName}");
			return false;
		}

		if (entry.clip == null)
		{
			Debug.LogWarning($"Audio bank entry '{soundName}' has no clip assigned.");
			return false;
		}

		return true;
	}

	private AudioSource GetNextSource(AudioChannel channel)
	{
		var pool = _pools[channel];
		var index = _poolIndices[channel];
		var source = pool[index];
		_poolIndices[channel] = (index + 1) % pool.Length;
		return source;
	}

	private void StartMusicTransition(AudioSource toSource, float duration, float targetVolume)
	{
		if (!_pools.TryGetValue(AudioChannel.Music, out var musicSources))
			return;

		CancelMusicTransition();
		_musicTransitionId++;
		_musicTransitionCoroutine = StartCoroutine(FadeMusicRoutine(_musicTransitionId, musicSources, toSource, duration, targetVolume));
	}

	private void CancelMusicTransition()
	{
		if (_musicTransitionCoroutine == null)
			return;

		StopCoroutine(_musicTransitionCoroutine);
		_musicTransitionCoroutine = null;
	}

	private IEnumerator FadeMusicRoutine(int transitionId, AudioSource[] musicSources, AudioSource targetSource, float duration, float targetVolume)
	{
		var sourceCount = musicSources.Length;
		var startVolumes = new float[sourceCount];
		for (var i = 0; i < sourceCount; i++)
			startVolumes[i] = musicSources[i].volume;

		if (duration <= 0f)
		{
			for (var i = 0; i < sourceCount; i++)
			{
				var source = musicSources[i];
				if (source == targetSource)
				{
					source.volume = targetVolume;
				}
				else
				{
					source.volume = 0f;
					source.Stop();
				}
			}

			_musicTransitionCoroutine = null;
			yield break;
		}

		var elapsed = 0f;
		while (elapsed < duration)
		{
			if (transitionId != _musicTransitionId)
				yield break;

			var t = elapsed / duration;
			for (var i = 0; i < sourceCount; i++)
			{
				var source = musicSources[i];
				if (source == targetSource)
					source.volume = Mathf.Lerp(startVolumes[i], targetVolume, t);
				else
					source.volume = Mathf.Lerp(startVolumes[i], 0f, t);
			}

			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}

		if (transitionId != _musicTransitionId)
			yield break;

		for (var i = 0; i < sourceCount; i++)
		{
			var source = musicSources[i];
			if (source == targetSource)
			{
				source.volume = targetVolume;
			}
			else
			{
				source.volume = 0f;
				source.Stop();
			}
		}

		_musicTransitionCoroutine = null;
	}
}
