using LccModel;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace LccEditor
{
    public class PreprocessUtil : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get; set;
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            
        }
    }
}