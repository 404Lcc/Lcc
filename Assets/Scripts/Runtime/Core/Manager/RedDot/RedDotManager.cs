using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class RedDot : MonoBehaviour
    {
        public bool isRedDotActive;
        private GameObject redDot;
        private Text redDotCount;

        public Vector3 scale = Vector3.one;
        public Vector2 offset = Vector2.zero;

        void Start()
        {
            redDot = AssetManager.Instance.InstantiateAsset("RedDot", AssetType.Tool);
            redDotCount = redDot.GetComponentInChildren<Text>();


            redDot.transform.SetParent(transform, false);
            redDot.transform.localScale = scale;
            redDot.transform.GetComponent<RectTransform>().anchoredPosition = offset;

            Hide();

        }
        public void Show()
        {
            isRedDotActive = true;
            redDotCount.text = string.Empty;
            redDot.SetActive(isRedDotActive);
        }
        public void Hide()
        {
            isRedDotActive = false;
            redDotCount.text = string.Empty;
            redDot.SetActive(isRedDotActive);


        }

        public void RefreshRedDotCount(int count)
        {
            if (!isRedDotActive)
            {
                return;
            }
            redDotCount.text = count <= 0 ? string.Empty : count.ToString();
        }


        public void OnDestroy()
        {
            Hide();
            Destroy(redDot);


        }

    }

    public class RedDotManager : Singleton<RedDotManager>
    {
        public Dictionary<string, List<string>> parentDict = new Dictionary<string, List<string>>();//key���ڵ� value�ӽڵ��б�
        public HashSet<string> needShowParent = new HashSet<string>();//��Ҫ��ʾ�ĸ��ڵ� key���ڵ�
        public Dictionary<string, int> redDotCountDict = new Dictionary<string, int>();//key�ӽڵ� value������
        public Dictionary<string, string> childToParentDict = new Dictionary<string, string>();//key�ӽڵ� value���ڵ�
        public Dictionary<string, int> nodeCountDict = new Dictionary<string, int>();//key�ӽڵ� value�ڵ����
        public Dictionary<string, RedDot> redDotDict = new Dictionary<string, RedDot>();//key�ӽڵ� value���


        /// <summary>
        /// ���Ӻ��ڵ�
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShow"></param>
        public void AddRedDotNode(string parent, string target, bool isNeedShow)
        {
            if (!string.IsNullOrEmpty(parent) && !parentDict.ContainsKey(parent))
            {
                LogUtil.LogWarning("���ڵ����½ڵ㣺" + parent);
            }



            if (string.IsNullOrEmpty(target))
            {
                LogUtil.LogError($"Ŀ�겻��Ϊ��");
                return;
            }
            if (string.IsNullOrEmpty(parent))
            {
                LogUtil.LogError($"���ڵ㲻��Ϊ��");
                return;
            }
            if (childToParentDict.ContainsKey(target))
            {
                LogUtil.LogError($"{target} �Ѵ���");
                return;
            }



            childToParentDict.Add(target, parent);


            if (!nodeCountDict.ContainsKey(target))
            {
                nodeCountDict.Add(target, 0);
            }



            if (!needShowParent.Contains(parent) && isNeedShow)
            {
                needShowParent.Add(parent);
            }


            if (!redDotCountDict.ContainsKey(target))
            {
                redDotCountDict.Add(target, 0);
            }



            if (!nodeCountDict.ContainsKey(parent))
            {
                nodeCountDict.Add(parent, 0);
            }



            if (parentDict.TryGetValue(parent, out List<string> list))
            {
                list.Add(target);
                return;
            }
            list = new List<string>();
            list.Add(target);
            parentDict.Add(parent, list);
        }
        /// <summary>
        /// ���Ӻ��ڵ�
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShow"></param>
        /// <param name="redDot"></param>
        public void AddRedDotNode(string parent, string target, bool isNeedShow, RedDot redDot)
        {
            AddRedDotNode(parent, target, isNeedShow);
            AddRedDot(target, redDot);
        }
        /// <summary>
        /// �Ƴ����ڵ�
        /// </summary>
        /// <param name="target"></param>
        public void RemoveRedDotNode(string target)
        {
            if (!childToParentDict.TryGetValue(target, out string parent))
            {
                return;
            }

            if (!IsLeafNode(target))
            {
                LogUtil.LogError("����ɾ�����ڵ�");
                return;
            }

            //���ٺ�����
            UpdateNodeCount(target, false);


            //�Ƴ��ڵ�
            childToParentDict.Remove(target);
            if (!string.IsNullOrEmpty(parent))
            {
                parentDict[parent].Remove(target);
                if (parentDict[parent].Count <= 0)
                {
                    parentDict[parent].Clear();
                    parentDict.Remove(parent);
                    needShowParent.Remove(parent);
                }
            }
            nodeCountDict.Remove(target);
        }
        /// <summary>
        /// �Ƴ����ڵ�
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRemoveRedDot"></param>
        public void RemoveRedDotNode(string target, bool isRemoveRedDot)
        {
            RemoveRedDotNode(target);
            if (isRemoveRedDot)
            {
                RemoveRedDot(target, out RedDot redDot);
            }
        }
        /// <summary>
        /// ���Ӻ��
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public void AddRedDot(string target, RedDot redDot)
        {
            if (!nodeCountDict.TryGetValue(target, out int nodeCount))
            {
                LogUtil.LogError($"�ڵ㲻���� {target} ��������");
                return;
            }

            redDotDict[target] = redDot;

            if (nodeCount == 0)
            {
                return;
            }
            redDot.Show();
        }
        /// <summary>
        /// �Ƴ����
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public void RemoveRedDot(string target, out RedDot redDot)
        {
            if (redDotDict.TryGetValue(target, out redDot))
            {
                redDotDict.Remove(target);
            }

            if (redDot == null || !redDot.isRedDotActive)
            {
                return;
            }
            Object.Destroy(redDot);
        }



        /// <summary>
        /// ��ʾ���ڵ�
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ShowRedDotNode(string target)
        {
            if (IsAlreadyShow(target))
            {
                return false;
            }

            if (!IsLeafNode(target))
            {
                LogUtil.LogError("������ʾ���ڵ� " + target);
                return false;
            }

            UpdateNodeCount(target, true);
            return true;
        }
        /// <summary>
        /// ���غ��ڵ�
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool HideRedDotNode(string target)
        {
            if (!IsLeafNode(target))
            {
                LogUtil.LogError("�������ظ��ڵ� " + target);
                return false;
            }

            UpdateNodeCount(target, false);
            return true;
        }
        /// <summary>
        /// ˢ�º�����
        /// </summary>
        /// <param name="target"></param>
        /// <param name="Count"></param>
        public void RefreshRedDotCount(string target, int Count)
        {
            if (!IsLeafNode(target))
            {
                LogUtil.LogError("����ˢ�¸��ڵ�");
                return;
            }

            redDotDict.TryGetValue(target, out RedDot redDot);

            redDotCountDict[target] = Count;

            if (needShowParent.Contains(target) && redDot != null)
            {
                redDot.RefreshRedDotCount(redDotCountDict[target]);
            }


            bool isParentExist = childToParentDict.TryGetValue(target, out string parent);

            while (isParentExist)
            {
                var viewCount = 0;

                foreach (var childNode in parentDict[parent])
                {
                    viewCount += redDotCountDict[childNode];
                }


                redDotCountDict[parent] = viewCount;

                if (redDotDict.TryGetValue(parent, out redDot))
                {
                    if (needShowParent.Contains(parent))
                    {
                        redDot.RefreshRedDotCount(redDotCountDict[parent]);
                    }
                }
                isParentExist = childToParentDict.TryGetValue(parent, out parent);
            }
        }




        /// <summary>
        /// ���½ڵ����
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRaiseCount"></param>
        private void UpdateNodeCount(string target, bool isRaiseCount)
        {
            if (!nodeCountDict.ContainsKey(target))
            {
                LogUtil.LogError($"{target} �ڵ㲻����");
                return;
            }

            if (!IsLeafNode(target))
            {
                LogUtil.LogError($"{target} �����Ǹ��ڵ�");
                return;
            }
            //��߼���
            if (isRaiseCount)
            {
                if (nodeCountDict[target] == 1)
                {
                    LogUtil.LogError($"{target} �ڵ�����Ѿ���1��");
                    return;
                }

                nodeCountDict[target] += 1;
                if (nodeCountDict[target] != 1)
                {
                    LogUtil.LogError($"{target} �ڵ�������� RetainCount = {nodeCountDict[target]}");
                    return;
                }
            }
            else
            {
                if (nodeCountDict[target] != 1)
                {
                    LogUtil.LogError($"{target} �ڵ��ǲ���ʾ״̬ RetainCount = {nodeCountDict[target]}");
                    return;
                }
                nodeCountDict[target] += -1;
            }


            int curr = nodeCountDict[target];
            if (curr < 0 || curr > 1)
            {
                LogUtil.LogError("���������󣬺���߼�������");
                return;
            }
            //��ʾ���
            if (redDotDict.TryGetValue(target, out RedDot redDot))
            {
                if (isRaiseCount)
                {
                    redDot.Show();
                }
                else
                {
                    redDot.Hide();
                }
            }



            //��ȡ���ڵ�
            bool isParentExist = childToParentDict.TryGetValue(target, out string parent);
            //ѭ�������ϲ�ڵ�
            while (isParentExist)
            {
                nodeCountDict[parent] += isRaiseCount ? 1 : -1;

                if (nodeCountDict[parent] >= 1 && isRaiseCount)
                {
                    if (redDotDict.TryGetValue(parent, out redDot))
                    {
                        if (!redDot.isRedDotActive)
                        {
                            redDot.Show();
                        }
                    }
                }

                if (nodeCountDict[parent] == 0 && !isRaiseCount)
                {
                    if (redDotDict.TryGetValue(parent, out redDot))
                    {
                        redDot.Hide();
                    }
                }
                isParentExist = childToParentDict.TryGetValue(parent, out parent);
            }
        }
        /// <summary>
        /// �ж��Ƿ���Ҷ�ӽڵ�
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsLeafNode(string target)
        {
            return !parentDict.ContainsKey(target);
        }
        /// <summary>
        /// ����Ƿ��Ѿ�������ʾ״̬
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsAlreadyShow(string target)
        {
            if (!nodeCountDict.ContainsKey(target))
            {
                return false;
            }
            return nodeCountDict[target] >= 1;
        }



        public override void OnDestroy()
        {
            base.OnDestroy();


            foreach (var item in parentDict.Values)
            {
                item.Clear();
            }
            parentDict.Clear();
            childToParentDict.Clear();
            nodeCountDict.Clear();
            redDotDict.Clear();
            needShowParent.Clear();
            redDotCountDict.Clear();
        }

    }
}