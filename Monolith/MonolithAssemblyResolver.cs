using System;
using System.Reflection;

namespace Monolith
{
    public static class MonolithAssemblyResolver
    {
        static MonolithAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();

            var name = assemblyName.Name + "." + args.Name.Split(new[] { ',' })[0] + ".dll";
            //var resNames = assembly.GetManifestResourceNames();
            //var resName = resNames.FirstOrDefault(n => n == name);
            //if (resName == null)
            //  return null;

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

    }
}
