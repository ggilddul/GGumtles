using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.UI;

namespace GGumtles.UI
{
    /// <summary>
    /// 재사용 가능한 버튼 컴포넌트
    /// 다양한 스타일과 기능을 지원하는 범용 버튼
    /// </summary>
    public class ReusableButton : UIBase
    {
        [Header("버튼 컴포넌트")]
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Image iconImage;

        [Header("버튼 스타일")]
        [SerializeField] private ButtonStyle buttonStyle = ButtonStyle.Normal;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.gray;
        [SerializeField] private Color disabledColor = Color.gray;
        [SerializeField] private Color highlightedColor = Color.yellow;

        [Header("애니메이션 설정")]
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private float scaleMultiplier = 0.95f;
        [SerializeField] private float scaleAnimationDuration = 0.1f;

        [Header("사운드 설정")]
        [SerializeField] private AudioManager.SFXType clickSound = AudioManager.SFXType.Button;
        [SerializeField] private AudioManager.SFXType hoverSound = AudioManager.SFXType.Button;

        // 버튼 스타일 열거형
        public enum ButtonStyle
        {
            Normal,
            Primary,
            Secondary,
            Danger,
            Success,
            Warning,
            Ghost
        }

        // 이벤트 정의
        public delegate void OnButtonClicked(ReusableButton button);
        public delegate void OnButtonHovered(ReusableButton button);
        public delegate void OnButtonPressed(ReusableButton button);
        
        public event OnButtonClicked OnButtonClickedEvent;
        public event OnButtonHovered OnButtonHoveredEvent;
        public event OnButtonPressed OnButtonPressedEvent;

        // 상태 관리
        private bool isPressed = false;
        private bool isHovered = false;
        private Vector3 originalScale;
        private Coroutine scaleAnimationCoroutine;

        protected override void AutoFindComponents()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (buttonImage == null)
                buttonImage = GetComponent<Image>();
            if (buttonText == null)
                buttonText = GetComponentInChildren<TMP_Text>();
            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();
        }

        protected override void SetupDefaultSettings()
        {
            originalScale = transform.localScale;
            
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        protected override void SetupUI()
        {
            ApplyButtonStyle();
        }

        /// <summary>
        /// 버튼 스타일 적용
        /// </summary>
        private void ApplyButtonStyle()
        {
            if (buttonImage == null) return;

            switch (buttonStyle)
            {
                case ButtonStyle.Normal:
                    buttonImage.color = normalColor;
                    break;
                case ButtonStyle.Primary:
                    buttonImage.color = new Color(0.2f, 0.6f, 1f);
                    break;
                case ButtonStyle.Secondary:
                    buttonImage.color = new Color(0.6f, 0.6f, 0.6f);
                    break;
                case ButtonStyle.Danger:
                    buttonImage.color = new Color(1f, 0.3f, 0.3f);
                    break;
                case ButtonStyle.Success:
                    buttonImage.color = new Color(0.3f, 0.8f, 0.3f);
                    break;
                case ButtonStyle.Warning:
                    buttonImage.color = new Color(1f, 0.8f, 0.2f);
                    break;
                case ButtonStyle.Ghost:
                    buttonImage.color = new Color(1f, 1f, 1f, 0.1f);
                    break;
            }
        }

        /// <summary>
        /// 버튼 클릭 이벤트
        /// </summary>
        private void OnClick()
        {
            try
            {
                // 사운드 재생
                if (enableSound)
                {
                    AudioManager.Instance?.PlaySFX(clickSound);
                }

                // 스케일 애니메이션
                if (enableScaleAnimation)
                {
                    StartScaleAnimation();
                }

                // 이벤트 발생
                OnButtonClickedEvent?.Invoke(this);
                OnButtonPressedEvent?.Invoke(this);

                LogDebug($"[ReusableButton] 버튼 클릭: {buttonText?.text ?? "Unknown"}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReusableButton] 버튼 클릭 처리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 스케일 애니메이션 시작
        /// </summary>
        private void StartScaleAnimation()
        {
            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
            }
            scaleAnimationCoroutine = StartCoroutine(ScaleAnimationCoroutine());
        }

        /// <summary>
        /// 스케일 애니메이션 코루틴
        /// </summary>
        private System.Collections.IEnumerator ScaleAnimationCoroutine()
        {
            // 축소
            Vector3 targetScale = originalScale * scaleMultiplier;
            float elapsed = 0f;

            while (elapsed < scaleAnimationDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (scaleAnimationDuration * 0.5f);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                yield return null;
            }

            transform.localScale = targetScale;

            // 복원
            elapsed = 0f;
            while (elapsed < scaleAnimationDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (scaleAnimationDuration * 0.5f);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
                yield return null;
            }

            transform.localScale = originalScale;
            scaleAnimationCoroutine = null;
        }

        /// <summary>
        /// 버튼 텍스트 설정
        /// </summary>
        public void SetText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }

        /// <summary>
        /// 버튼 아이콘 설정
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }
        }

        /// <summary>
        /// 버튼 스타일 설정
        /// </summary>
        public void SetButtonStyle(ButtonStyle style)
        {
            buttonStyle = style;
            ApplyButtonStyle();
        }

        /// <summary>
        /// 버튼 색상 설정
        /// </summary>
        public void SetColors(Color normal, Color pressed, Color disabled, Color highlighted)
        {
            normalColor = normal;
            pressedColor = pressed;
            disabledColor = disabled;
            highlightedColor = highlighted;
            ApplyButtonStyle();
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }

            if (buttonImage != null)
            {
                buttonImage.color = interactable ? normalColor : disabledColor;
            }
        }

        /// <summary>
        /// 스케일 애니메이션 활성화/비활성화
        /// </summary>
        public void SetScaleAnimationEnabled(bool enabled)
        {
            enableScaleAnimation = enabled;
        }

        /// <summary>
        /// 클릭 사운드 설정
        /// </summary>
        public void SetClickSound(AudioManager.SFXType sound)
        {
            clickSound = sound;
        }

        /// <summary>
        /// 호버 사운드 설정
        /// </summary>
        public void SetHoverSound(AudioManager.SFXType sound)
        {
            hoverSound = sound;
        }

        /// <summary>
        /// 버튼 정보 반환
        /// </summary>
        public string GetButtonInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[ReusableButton 정보]");
            info.AppendLine($"텍스트: {buttonText?.text ?? "없음"}");
            info.AppendLine($"스타일: {buttonStyle}");
            info.AppendLine($"상호작용 가능: {button?.interactable ?? false}");
            info.AppendLine($"스케일 애니메이션: {(enableScaleAnimation ? "활성화" : "비활성화")}");
            info.AppendLine($"클릭 사운드: {clickSound}");

            return info.ToString();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
            }

            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }

            // 이벤트 구독 해제
            OnButtonClickedEvent = null;
            OnButtonHoveredEvent = null;
            OnButtonPressedEvent = null;
        }
    }
}
