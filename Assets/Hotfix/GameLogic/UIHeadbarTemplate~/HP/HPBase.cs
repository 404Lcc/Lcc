using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class HPBase : UIHeadbarTemplate
    {
        public Image downFrame;
        public Image upFrame;
        public TextMeshProUGUI number;

        private bool showHPValue; //是否显示血量值
        private bool showHPDown; //有的血条是两层，是否显示下方
        private float lastHp; //上次的血量
        private bool enableDown; //开启动画

        private float targetDownFillAmount;
        private float duration = 0.25f; // 动画持续时间（秒）
        private float timer = 0f;

        public override void Init(LogicEntity entity, float offsetY)
        {
            base.Init(entity, offsetY);

            showHPValue = false;
            showHPDown = true;
            lastHp = 0;
            enableDown = false;

            if (number != null)
            {
                number.gameObject.SetActive(showHPValue);
            }

            UpdatePos();
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Update()
        {
            base.Update();

            if (enableDown)
            {
                // 更新动画计时器
                timer += Time.deltaTime;

                // 计算当前进度（0~1）
                float progress = Mathf.Clamp01(timer / duration);

                // 使用Lerp平滑过渡
                downFrame.fillAmount = Mathf.Lerp(downFrame.fillAmount, targetDownFillAmount, progress);

                // 动画完成时重置状态
                if (progress >= 1f)
                {
                    enableDown = false;
                }
            }
        }

        //血条
        public void SetHp(float hp, float maxHp)
        {
            if (Mathf.Abs(lastHp - hp) <= 0.01f)
            {
                return;
            }

            bool isReduce = lastHp - hp > 0;
            lastHp = hp;

            float currentFill = hp / maxHp;
            upFrame.fillAmount = currentFill;

            if (showHPValue)
            {
                number.text = Mathf.RoundToInt(hp).ToString();
            }

            if (showHPDown)
            {
                if (isReduce)
                {
                    // 启动自定义动画
                    targetDownFillAmount = currentFill;
                    timer = 0f;
                    enableDown = true;
                }
                else
                {
                    // 血量增加时立即更新
                    downFrame.fillAmount = currentFill;
                    enableDown = false; //停止可能存在的动画
                }
            }

            AfterSetHp();

            if (isReduce)
            {
                PlayLoseHpAnim();
            }
        }

        public virtual void AfterSetHp()
        {
        }

        public virtual void PlayLoseHpAnim()
        {
        }
    }
}