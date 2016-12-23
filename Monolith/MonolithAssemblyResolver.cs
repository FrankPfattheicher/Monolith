using System.Diagnostics;

namespace Monolith
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;

    public static class MonolithAssemblyResolver
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);

        public static MethodInfo EntryPoint;

        public static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Debugger.Break();

            //MessageBox.Show("private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)");
            OutputDebugString("private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)");

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();

            var name = assemblyName.Name + "." + args.Name.Split(new[] { ',' })[0] + ".dll";

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    return null;

                using (var reader = new System.IO.BinaryReader(stream))
                {
                    var data = reader.ReadBytes((int)stream.Length);
                    return Assembly.Load(data);
                }
            }
        }

        public static void Init(string[] args)
        {
            //OutputDebugString("static void MonolithAssemblyResolver.Init()");
            MessageBox.Show("static void MonolithAssemblyResolver.Init()");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            //var entryArgs = new object[args.Length];
            //Array.Copy(args, entryArgs, args.Length);
            //EntryPoint?.Invoke(null, entryArgs);
        }

    }
}
