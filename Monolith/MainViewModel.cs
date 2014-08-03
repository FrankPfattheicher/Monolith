using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;
using IctBaden.Presentation;
using Microsoft.Win32;

namespace Monolith
{
    public class MainViewModel : ActiveViewModel
    {
        public string ExeFileName { get; set; }

        public List<string> AssemblyFiles { get; set; }
        public List<string> AssemblyResources { get; set; }


        public string SelectedFile { get; set; }
        public string SelectedResource { get; set; }


        [ActionMethod]
        public void OnBrowseExeFile()
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = "exe",
                Filter = "EXE Application|*.exe|All Files|*.*"
            };

            if (dlg.ShowDialog() != true)
                return;

            this["ExeFileName"] = dlg.FileName;
            if (File.Exists(ExeFileName))
            {
                var mgr = new AssemblyResourceManager(ExeFileName);
                this["AssemblyFiles"] = mgr.GetLocalAssemblies();
                this["AssemblyResources"] = mgr.GetResourceAssemblies();
            }
        }

        [DependsOn("SelectedFile")]
        [ActionMethod(EnabledPredicate = "CanAddResource")]
        public void OnAddResource()
        {
            var mgr = new AssemblyResourceManager(ExeFileName);
            var src = Path.Combine(mgr.AssemblyPath, SelectedFile);
            if (mgr.AddAssemblyResource(src, false))
            {
                this["AssemblyFiles"] = mgr.GetLocalAssemblies();
                this["AssemblyResources"] = mgr.GetResourceAssemblies();
            }
        }
        public bool CanAddResource()
        {
            return !string.IsNullOrEmpty(ExeFileName) && !string.IsNullOrEmpty(SelectedFile);
        }

        [DependsOn("SelectedResource")]
        [ActionMethod(EnabledPredicate = "CanRemoveResource")]
        public void OnRemoveResource()
        {
            var mgr = new AssemblyResourceManager(ExeFileName);
            if (mgr.RemoveAssemblyResource(SelectedResource, true))
            {
                this["AssemblyFiles"] = mgr.GetLocalAssemblies();
                this["AssemblyResources"] = mgr.GetResourceAssemblies();
            }
        }
        public bool CanRemoveResource()
        {
            return !string.IsNullOrEmpty(ExeFileName) && !string.IsNullOrEmpty(SelectedResource);
        }


        [ActionMethod]
        public void OnAddResolver()
        {
            var mgr = new AssemblyResourceManager(ExeFileName);
            mgr.AddAssemblyResolver();
        }

    }
}