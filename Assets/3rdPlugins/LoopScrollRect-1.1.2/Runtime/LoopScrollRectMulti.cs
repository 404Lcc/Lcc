using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class LoopScrollRectMulti : LoopScrollRectBase
    {
        //===修改
        //[HideInInspector]
        //[NonSerialized]
        //public LoopScrollMultiDataSource dataSource = null;
        //===修改

        protected override void ProvideData(Transform transform, int index)
        {
            //===修改
            //dataSource.ProvideData(transform, index);
            ProvideDataHandler(transform, index);
            //===修改
        }

        // Multi Data Source cannot support TempPool
        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            //===修改
            //RectTransform nextItem = prefabSource.GetObject(itemIdx).transform as RectTransform;

            RectTransform nextItem = GetObjectHandler(itemIdx).transform as RectTransform;

            //===修改
            nextItem.transform.SetParent(m_Content, false);
            nextItem.gameObject.SetActive(true);

            ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            Debug.Assert(m_Content.childCount >= count);
            if (fromStart)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    //===修改
                    //prefabSource.ReturnObject(m_Content.GetChild(i));
                    ReturnObjectHandler(m_Content.GetChild(i), itemTypeStart + i);
                    //===修改
                }
            }
            else
            {
                int t = m_Content.childCount - count;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    //===修改
                    //prefabSource.ReturnObject(m_Content.GetChild(i));
                    ReturnObjectHandler(m_Content.GetChild(i), itemTypeStart + i);
                    //===修改
                }
            }
        }

        protected override void ClearTempPool()
        {
        }
    }
}