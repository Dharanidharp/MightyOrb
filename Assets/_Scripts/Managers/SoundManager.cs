using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip coinCollectSFX;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip landSFX;

    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private float coinSoundCooldown = 0.1f;
    private bool canPlayCoinSound = true;

    [Header("Music Tracks")]
    [SerializeField] private List<AudioClip> musicTracks;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicCrossfadeTime = 1.5f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f; // Add a settable music volume

    private int currentMusicTrackIndex = -1;
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        // --- STEP 1: Implement Robust Singleton Pattern ---
        if (Instance != null)
        {
            // If another instance exists, and it's not THIS one, destroy this duplicate.
            if (Instance != this)
            {
                Destroy(gameObject);
            }
            // If it's the same instance (re-Awake due to scene load/unload, or DontDestroyOnLoad), just return.
            return;
        }

        Instance = this;
        // IMPORTANT: Decide if you truly need DontDestroyOnLoad.
        // If your game is a single scene, DO NOT use this.
        // If you have a main menu scene and then load the game scene, then use it.
        DontDestroyOnLoad(gameObject); // Uncomment if needed for multi-scene games

        // --- STEP 2: Ensure AudioSources are properly configured and created if missing ---
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("Music AudioSource not assigned to SoundManager, adding one automatically.");
        }
        musicSource.playOnAwake = false; // Always force off for music
        musicSource.loop = false;       // Always force off, we handle looping in script
        musicSource.volume = musicVolume; // Set initial volume

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("SFX AudioSource not assigned to SoundManager, adding one automatically.");
        }
        sfxSource.playOnAwake = false;  // Always force off for SFX
        sfxSource.loop = false;         // Always force off
    }

    private void Start()
    {
        // --- STEP 3: Start music ONLY if tracks exist ---
        if (musicTracks != null && musicTracks.Count > 0 && musicSource != null)
        {
            // If music isn't already playing (e.g., from DontDestroyOnLoad)
            if (!musicSource.isPlaying)
            {
                PlayNextMusicTrack();
            }
        }
        else
        {
            Debug.LogWarning("SoundManager: No music tracks assigned or musicSource is null. Music will not play.");
        }
    }

    private void Update()
    {
        // --- STEP 4: Check for music finished only if not fading ---
        // Also ensure we have tracks to play and a source.
        if (musicSource != null && !musicSource.isPlaying && musicTracks != null && musicTracks.Count > 0 && musicFadeCoroutine == null)
        {
            PlayNextMusicTrack();
        }
    }

    // --- Public methods to play sounds ---

    public void PlayCoinCollectSFX()
    {
        if (canPlayCoinSound)
        {
            PlaySFX(coinCollectSFX);
            StartCoroutine(CoinSoundCooldownRoutine());
        }
    }

    public void PlayJumpSFX()
    {
        PlaySFX(jumpSFX);
    }

    public void PlayLandSFX()
    {
        PlaySFX(landSFX);
    }

    private IEnumerator CoinSoundCooldownRoutine()
    {
        canPlayCoinSound = false;
        yield return new WaitForSeconds(coinSoundCooldown);
        canPlayCoinSound = true;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (sfxSource == null || clip == null) return;

        // ADD THIS LINE FOR DEBUGGING
        Debug.Log("PLAYING SFX: " + clip.name + " at time: " + Time.time);

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    // --- Music management ---

    private void PlayNextMusicTrack()
    {
        if (musicTracks == null || musicTracks.Count == 0 || musicSource == null)
        {
            Debug.LogWarning("SoundManager: No music tracks assigned or musicSource is null. Cannot play music.");
            return;
        }

        // If a fade is already in progress, stop it to start a new one
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null; // Mark as null so Update can re-trigger if needed
            musicSource.volume = 0; // Ensure it starts from silent for the new fade if interrupted
            musicSource.Stop(); // Stop any currently playing sound abruptly
        }

        int nextTrackIndex;
        if (musicTracks.Count == 1)
        {
            nextTrackIndex = 0;
        }
        else
        {
            do
            {
                nextTrackIndex = Random.Range(0, musicTracks.Count);
            } while (nextTrackIndex == currentMusicTrackIndex);
        }
        currentMusicTrackIndex = nextTrackIndex;

        AudioClip nextClip = musicTracks[currentMusicTrackIndex];
        Debug.Log($"Starting fade to music track: {nextClip.name}");
        musicFadeCoroutine = StartCoroutine(FadeMusic(nextClip, musicCrossfadeTime));
    }

    private IEnumerator FadeMusic(AudioClip newClip, float fadeTime)
    {
        // Initial volume for fade out (if current track is playing)
        float startFadeOutVolume = musicSource.volume;
        float fadeOutDuration = fadeTime / 2f; // Half the time to fade out, half to fade in

        // Fade out current music (if playing and not already silent)
        if (musicSource.isPlaying && startFadeOutVolume > 0.01f) // Small epsilon to avoid fading from already silent
        {
            float timer = 0f;
            while (timer < fadeOutDuration)
            {
                musicSource.volume = Mathf.Lerp(startFadeOutVolume, 0f, timer / fadeOutDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            musicSource.Stop();
        }

        // Play and fade in new music
        musicSource.clip = newClip;
        musicSource.loop = false;
        musicSource.volume = 0f; // Start silent
        musicSource.Play();

        float targetVolume = musicVolume; // Use the settable musicVolume for fade in
        float fadeInDuration = fadeTime / 2f;

        float timer2 = 0f;
        while (timer2 < fadeInDuration && musicSource.volume < targetVolume)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, timer2 / fadeInDuration);
            timer2 += Time.deltaTime;
            yield return null;
        }
        musicSource.volume = targetVolume; // Ensure full target volume at end

        musicFadeCoroutine = null; // Mark fade as complete
    }

    public void StopMusic()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        if (musicSource != null) musicSource.Stop();
        musicSource.volume = musicVolume; // Reset volume to default
    }

    public void PauseMusic()
    {
        if (musicSource != null) musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null) musicSource.UnPause();
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicVolume = volume; // Update the target volume
            musicSource.volume = volume;
        }
    }
}