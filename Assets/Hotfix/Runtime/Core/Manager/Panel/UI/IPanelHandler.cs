namespace LccHotfix
{
    public interface IPanelHandler
    {
        /// <summary>
        /// ��ʼ��UI���
        /// </summary>
        /// <param name="panel"></param>
        void OnInitComponent(Panel panel);
        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="panel"></param>
        void OnInitData(Panel panel);

        /// <summary>
        /// ע��UIҵ���߼��¼�
        /// </summary>
        /// <param name="panel"></param>
        void OnRegisterUIEvent(Panel panel);

        /// <summary>
        /// ��ʾUI����
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="contextData"></param>
        void OnShow(Panel panel, AObjectBase contextData = null);

        /// <summary>
        /// ����UI����
        /// </summary>
        /// <param name="panel"></param>
        void OnHide(Panel panel);

        /// <summary>
        /// ���ý���
        /// </summary>
        /// <param name="panel"></param>
        void OnReset(Panel panel);

        /// <summary>
        /// ���ٽ���֮ǰ
        /// </summary>
        /// <param name="panel"></param>
        void OnBeforeUnload(Panel panel);

        /// <summary>
        /// �ж��Ƿ񷵻�
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        bool IsReturn(Panel panel);
    }
}