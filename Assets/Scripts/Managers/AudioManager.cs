using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("오디오 소스")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    
    [Header("오디오 클립")]
    public AudioClip[] buttonClips;
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;
    
    [Header("오디오 풀링")]
    public int maxConcurrentSFX = 5;
    public GameObject audioSourcePrefab;
    
    [Header("설정")]
    public float defaultSFXVolume = 1f;
    public float defaultBGMVolume = 0.7f;
    public float fadeDuration = 1f;

    // 오디오 타입 열거형
    public enum AudioType
    {
        SFX,
        BGM,
        UI
    }

    // BGM 타입 열거형
    public enum BGMType
    {
        Main,
        Menu,
        Game,
        Victory,
        Defeat
    }

    // SFX 타입 열거형
    public enum SFXType
    {
        Button,
        Collect,
        Achievement,
        Evolve,
        Error,
        Success,
        ItemDrop,
        EarnItem
    }

    // 오디오 풀링용 큐
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    
    // 현재 재생 중인 BGM
    private AudioClip currentBGM;
    private Coroutine fadeCoroutine;
    
    // 설정 연동
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeAudioSystem();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSystem()
    {
        try
        {
            // 오디오 시스템 초기화 전에 잠시 대기
            StartCoroutine(InitializeAudioSystemCoroutine());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AudioManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private IEnumerator InitializeAudioSystemCoroutine()
    {
        // 오디오 시스템이 완전히 초기화될 때까지 대기
        yield return new WaitForEndOfFrame();
        
        ValidateAudioSources();
        InitializeAudioPool();
        LoadAudioSettings();
        isInitialized = true;
        
        Debug.Log("[AudioManager] 오디오 시스템 초기화 완료");
    }

    private void ValidateAudioSources()
    {
        if (sfxSource == null)
        {
            Debug.LogError("[AudioManager] SFX AudioSource가 설정되지 않았습니다.");
        }
        
        if (bgmSource == null)
        {
            Debug.LogError("[AudioManager] BGM AudioSource가 설정되지 않았습니다.");
        }
    }

    private void InitializeAudioPool()
    {
        if (audioSourcePrefab == null)
        {
            Debug.LogWarning("[AudioManager] AudioSource 프리팹이 설정되지 않았습니다.");
            return;
        }

        for (int i = 0; i < maxConcurrentSFX; i++)
        {
            GameObject audioObj = Instantiate(audioSourcePrefab, transform);
            AudioSource audioSource = audioObj.GetComponent<AudioSource>();
            
            if (audioSource != null)
            {
                audioSource.volume = defaultSFXVolume;
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        Debug.Log($"[AudioManager] 오디오 풀 초기화 완료 - {maxConcurrentSFX}개");
    }

    private void LoadAudioSettings()
    {
        try
        {
            if (GameSaveManager.Instance?.currentSaveData == null) return;

            var saveData = GameSaveManager.Instance.currentSaveData;
            
            // BGM 설정 로드
            float bgmVolume = GetBGMVolumeFromSettings(saveData.bgmOption);
            SetBGMVolume(bgmVolume);
            
            // SFX 설정 로드
            float sfxVolume = GetSFXVolumeFromSettings(saveData.sfxOption);
            SetSFXVolume(sfxVolume);
            
            Debug.Log($"[AudioManager] 오디오 설정 로드 완료 - BGM: {bgmVolume:F2}, SFX: {sfxVolume:F2}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[AudioManager] 오디오 설정 로드 중 오류: {ex.Message}");
            // 기본 설정 사용
            SetBGMVolume(defaultBGMVolume);
            SetSFXVolume(defaultSFXVolume);
        }
    }

    private float GetBGMVolumeFromSettings(GameSaveData.AudioOption bgmOption)
    {
        return bgmOption switch
        {
            GameSaveData.AudioOption.Off => 0f,
            GameSaveData.AudioOption.Low => 0.3f,
            GameSaveData.AudioOption.High => 0.7f,
            _ => defaultBGMVolume
        };
    }

    private float GetSFXVolumeFromSettings(GameSaveData.AudioOption sfxOption)
    {
        return sfxOption switch
        {
            GameSaveData.AudioOption.Off => 0f,
            GameSaveData.AudioOption.Low => 0.5f,
            GameSaveData.AudioOption.High => 1f,
            _ => defaultSFXVolume
        };
    }

    /// <summary>
    /// SFX 재생
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (!isInitialized || clip == null) return;

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            PlaySFXFromPool(clip);
        }
    }

    /// <summary>
    /// 풀에서 SFX 재생
    /// </summary>
    private void PlaySFXFromPool(AudioClip clip)
    {
        if (audioSourcePool.Count == 0)
        {
            Debug.LogWarning("[AudioManager] 오디오 풀이 비어있습니다.");
            return;
        }

        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.clip = clip;
        audioSource.Play();
        
        activeAudioSources.Add(audioSource);
        StartCoroutine(ReturnToPool(audioSource, clip.length));
    }

    private IEnumerator ReturnToPool(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeAudioSources.Contains(audioSource))
        {
            activeAudioSources.Remove(audioSource);
        }
        
        audioSourcePool.Enqueue(audioSource);
    }

    /// <summary>
    /// 버튼 사운드 재생
    /// </summary>
    public void PlayButtonSound(int soundIndex)
    {
        if (!isInitialized || buttonClips == null || soundIndex < 0 || soundIndex >= buttonClips.Length)
        {
            Debug.LogWarning($"[AudioManager] 잘못된 버튼 사운드 인덱스: {soundIndex}");
            return;
        }

        PlaySFX(buttonClips[soundIndex]);
    }

    /// <summary>
    /// SFX 타입별 재생
    /// </summary>
    public void PlaySFX(SFXType sfxType)
    {
        if (!isInitialized || sfxClips == null) return;

        int index = (int)sfxType;
        if (index >= 0 && index < sfxClips.Length)
        {
            PlaySFX(sfxClips[index]);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX 타입 '{sfxType}'에 해당하는 클립이 없습니다.");
        }
    }

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(AudioClip clip, bool fadeIn = true)
    {
        if (!isInitialized || clip == null) return;

        if (bgmSource == null)
        {
            Debug.LogError("[AudioManager] BGM AudioSource가 없습니다.");
            return;
        }

        if (currentBGM == clip && bgmSource.isPlaying) return;

        currentBGM = clip;
        bgmSource.clip = clip;
        bgmSource.loop = true;

        if (fadeIn)
        {
            StartCoroutine(FadeInBGM());
        }
        else
        {
            bgmSource.volume = GetBGMVolumeFromSettings(GameSaveManager.Instance?.currentSaveData?.bgmOption ?? GameSaveData.AudioOption.High);
            bgmSource.Play();
        }
    }

    /// <summary>
    /// BGM 타입별 재생
    /// </summary>
    public void PlayBGM(BGMType bgmType, bool fadeIn = true)
    {
        if (!isInitialized || bgmClips == null) return;

        int index = (int)bgmType;
        if (index >= 0 && index < bgmClips.Length)
        {
            PlayBGM(bgmClips[index], fadeIn);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] BGM 타입 '{bgmType}'에 해당하는 클립이 없습니다.");
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM(bool fadeOut = true)
    {
        if (!isInitialized || bgmSource == null) return;

        if (fadeOut)
        {
            StartCoroutine(FadeOutBGM());
        }
        else
        {
            bgmSource.Stop();
            currentBGM = null;
        }
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        if (!isInitialized || bgmSource == null) return;
        bgmSource.Pause();
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        if (!isInitialized || bgmSource == null) return;
        bgmSource.UnPause();
    }

    /// <summary>
    /// BGM 페이드 인
    /// </summary>
    private IEnumerator FadeInBGM()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        float targetVolume = GetBGMVolumeFromSettings(GameSaveManager.Instance?.currentSaveData?.bgmOption ?? GameSaveData.AudioOption.High);
        bgmSource.volume = 0f;
        bgmSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    /// <summary>
    /// BGM 페이드 아웃
    /// </summary>
    private IEnumerator FadeOutBGM()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
        currentBGM = null;
    }

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (!isInitialized) return;

        float clampedVolume = Mathf.Clamp01(volume);
        
        if (sfxSource != null)
        {
            sfxSource.volume = clampedVolume;
        }

        // 풀의 모든 AudioSource 볼륨 업데이트
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.volume = clampedVolume;
            }
        }

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null)
            {
                audioSource.volume = clampedVolume;
            }
        }

        Debug.Log($"[AudioManager] SFX 볼륨 설정: {clampedVolume:F2}");
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        if (!isInitialized || bgmSource == null) return;

        float clampedVolume = Mathf.Clamp01(volume);
        bgmSource.volume = clampedVolume;
        
        Debug.Log($"[AudioManager] BGM 볼륨 설정: {clampedVolume:F2}");
    }

    /// <summary>
    /// 모든 오디오 정지
    /// </summary>
    public void StopAllAudio()
    {
        if (!isInitialized) return;

        // BGM 정지
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        // SFX 정지
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }

        // 풀의 모든 AudioSource 정지
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        Debug.Log("[AudioManager] 모든 오디오 정지");
    }

    /// <summary>
    /// 현재 BGM 정보 반환
    /// </summary>
    public AudioClip GetCurrentBGM()
    {
        return currentBGM;
    }

    /// <summary>
    /// BGM 재생 중 여부
    /// </summary>
    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }

    /// <summary>
    /// 설정 저장
    /// </summary>
    public void SaveAudioSettings()
    {
        if (GameSaveManager.Instance?.currentSaveData == null) return;

        var saveData = GameSaveManager.Instance.currentSaveData;
        
        // 현재 볼륨을 설정값으로 변환
        saveData.sfxOption = ConvertVolumeToOption(sfxSource?.volume ?? defaultSFXVolume);
        saveData.bgmOption = ConvertVolumeToOption(bgmSource?.volume ?? defaultBGMVolume);
        
        GameSaveManager.Instance.SaveGame();
        
        Debug.Log($"[AudioManager] 오디오 설정 저장 완료 - SFX: {saveData.sfxOption}, BGM: {saveData.bgmOption}");
    }

    private GameSaveData.AudioOption ConvertVolumeToOption(float volume)
    {
        if (volume == 0f) return GameSaveData.AudioOption.Off;
        if (volume <= 0.5f) return GameSaveData.AudioOption.Low;
        return GameSaveData.AudioOption.High;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAudioSettings();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAudioSettings();
        }
    }

    private void OnDestroy()
    {
        SaveAudioSettings();
    }
}