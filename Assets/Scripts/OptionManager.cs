using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    public Slider sfxSlider;
    public Slider bgmSlider;

    public enum OptionType
    {
        SFX,
        BGM
    }

    private void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.wholeNumbers = true;
            sfxSlider.minValue = 0;
            sfxSlider.maxValue = 4;
            sfxSlider.onValueChanged.AddListener((value) =>
            {
                AdjustOption(OptionType.SFX, (int)value);
            });
        }

        if (bgmSlider != null)
        {
            bgmSlider.wholeNumbers = true;
            bgmSlider.minValue = 0;
            bgmSlider.maxValue = 4;
            bgmSlider.onValueChanged.AddListener((value) =>
            {
                AdjustOption(OptionType.BGM, (int)value);
            });
        }
    }

    /// <summary>
    /// 슬라이더 값에 따라 볼륨 조절
    /// </summary>
    public void AdjustOption(OptionType type, int level)
    {
        float volume = Mathf.Clamp01(level / 4f);

        switch (type)
        {
            case OptionType.SFX:
                AudioManager.Instance.SetSFXVolume(volume);
                break;

            case OptionType.BGM:
                AudioManager.Instance.SetBGMVolume(volume);
                break;
        }
    }
}
