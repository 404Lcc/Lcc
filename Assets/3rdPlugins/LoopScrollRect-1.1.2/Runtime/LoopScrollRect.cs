using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class LoopScrollRect : LoopScrollRectBase
    {
        //===修改
        //[HideInInspector]
        //[NonSerialized]
        //public LoopScrollDataSource dataSource = null;
        //===修改
        
        protected override void ProvideData(Transform transform, int index)
        {
            //===修改
            ProvideDataHandler(transform, index);
            //dataSource.ProvideData(transform, index);
            //===修改
        }

        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            //===修改
            //RectTransform nextItem = null;
            //if (deletedItemTypeStart > 0)
            //{
            //    deletedItemTypeStart--;
            //    nextItem = m_Content.GetChild(0) as RectTransform;
            //    nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
            //}
            //else if (deletedItemTypeEnd > 0)
            //{
            //    deletedItemTypeEnd--;
            //    nextItem = m_Content.GetChild(m_Content.childCount - 1) as RectTransform;
            //    nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
            //}
            //else
            //{
            //    nextItem = prefabSource.GetObject(itemIdx).transform as RectTransform;
            //    nextItem.transform.SetParent(m_Content, false);
            //    nextItem.gameObject.SetActive(true);
            //}

            RectTransform nextItem = GetObjectHandler(itemIdx).transform as RectTransform;
            nextItem.transform.SetParent(m_Content, false);
            nextItem.gameObject.SetActive(true);
            //===修改
            ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            //===修改
            //if (fromStart)
            //    deletedItemTypeStart += count;
            //else
            //    deletedItemTypeEnd += count;

            Debug.Assert(m_Content.childCount >= count);
            if (fromStart)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    ReturnObjectHandler(m_Content.GetChild(i), itemTypeStart + i);
                }
            }
            else
            {
                int t = m_Content.childCount - count;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    ReturnObjectHandler(m_Content.GetChild(i), itemTypeStart + i);
                }
            }

            //===修改
        }

        protected override void ClearTempPool()
        {
            //===修改
            //Debug.Assert(m_Content.childCount >= deletedItemTypeStart + deletedItemTypeEnd);
            //if (deletedItemTypeStart > 0)
            //{
            //    for (int i = deletedItemTypeStart - 1; i >= 0; i--)
            //    {
            //        prefabSource.ReturnObject(m_Content.GetChild(i));
            //    }
            //    deletedItemTypeStart = 0;
            //}
            //if (deletedItemTypeEnd > 0)
            //{
            //    int t = m_Content.childCount - deletedItemTypeEnd;
            //    for (int i = m_Content.childCount - 1; i >= t; i--)
            //    {
            //        prefabSource.ReturnObject(m_Content.GetChild(i));
            //    }
            //    deletedItemTypeEnd = 0;
            //}
            //===修改
        }
    }
}