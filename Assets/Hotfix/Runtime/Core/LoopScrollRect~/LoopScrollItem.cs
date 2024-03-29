﻿using LccModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class LoopScrollItem : AObjectBase
    {
        public int index = -1;
        public GameObject gameObject => GetComponent<GameObjectComponent>().gameObject;

        public ILoopScrollSelect loopScrollSelect => (ILoopScrollSelect)Parent;

        public GameObject selectGo;
        public GameObject normalGo;
        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            ((GameObject)(object)p1).ConvertComponent(this);

            Button button = gameObject.GetComponent<Button>();
            if (button == null)
            {
                gameObject.AddComponent<Button>().onClick.AddListener(OnClick);
            }
            else
            {
                button.onClick.AddListener(OnClick);
            }
        }
        public virtual void UpdateData(object obj)
        {
            OnItemSelect(loopScrollSelect.CurSelect);
        }
        public virtual void OnClick()
        {
            loopScrollSelect.SetSelect(index);
        }

        public virtual void OnItemSelect(int index)
        {
            UpdateSelectSpriteVisible(this.index == index);
        }

        private void UpdateSelectSpriteVisible(bool visible)
        {
            if (selectGo != null && selectGo.activeSelf != visible)
            {
                selectGo.SetActive(visible);
            }
            if (normalGo != null && normalGo.activeSelf == visible)
            {
                normalGo.SetActive(!visible);
            }
        }
    }
}