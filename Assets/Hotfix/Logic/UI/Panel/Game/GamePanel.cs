using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class UIGameModel : ViewModelBase
    {
    }
    public class UIGamePanel : UIPanel<UIGameModel>
    {
        public GameObject joystick;
        public Combat Player => CombatContext.Instance.GetCombatListByTag(TagType.Player)[0];
        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);

        }
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.NormalNavigation;
        }



        public override void OnShow(Panel panel, AObjectBase contextData = null)
        {
            base.OnShow(panel, contextData);


            InitJoystick();
            InitSmoothFoolow2D();
        }
        public void InitJoystick()
        {
            Joystick joystack = ViewModel.selfPanel.AddComponent<Joystick>(150f, 1f, joystick);
            joystack.Bind(Player.GetComponent<JoystickComponent>().Move);
        }
        public void InitSmoothFoolow2D()
        {
            ViewModel.selfPanel.AddComponent<SmoothFoolow2D>(1, true, Vector2.zero, new Vector3(-500, -500, 0), new Vector3(500, 500, 0), Player.TransformComponent, GlobalManager.Instance.MainCamera);
        }
    }
}