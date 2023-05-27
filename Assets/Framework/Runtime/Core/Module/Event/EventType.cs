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

        UserTryInitialize,//�û������ٴγ�ʼ����Դ��
        UserBeginDownloadWebFiles,//�û���ʼ���������ļ�
        UserTryUpdatePackageVersion,//�û������ٴθ��¾�̬�汾
        UserTryUpdatePatchManifest,//�û������ٴθ��²����嵥
        UserTryDownloadWebFiles,//�û������ٴ����������ļ�
    }
}