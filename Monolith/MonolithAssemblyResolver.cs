namespace Monolith
{
    using System;
    using System.Reflection;

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
