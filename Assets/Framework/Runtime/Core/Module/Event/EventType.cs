namespace LccModel
{
    public enum EventType : int
    {
        InitializeFailed,//��������ʼ��ʧ��
        PatchStatesChange,//�������̲���ı�
        FoundUpdateFiles,//���ָ����ļ�
        DownloadProgressUpdate,//���ؽ��ȸ���
        PackageVersionUpdateFailed,//��Դ�汾�Ÿ���ʧ��
        PatchManifestUpdateFailed,//�����嵥����ʧ��
        WebFileDownloadFailed,//�����ļ�����ʧ��
    }
}