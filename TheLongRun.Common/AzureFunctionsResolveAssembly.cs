using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TheLongRun.Common
{
    public class AzureFunctionsResolveAssembly : IDisposable
    {
        public AzureFunctionsResolveAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        void IDisposable.Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs arguments)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().FullName == arguments.Name);

            if (assembly != null)
            {
                return assembly;
            }

            // try to load assembly from file
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyName = new AssemblyName(arguments.Name);
            var assemblyFileName = assemblyName.Name + ".dll";
            string assemblyPath;

            if (assemblyName.Name.EndsWith(".resources"))
            {
                var resourceDirectory = Path.Combine(assemblyDirectory, assemblyName.CultureName);
                assemblyPath = Path.Combine(resourceDirectory, assemblyFileName);
            }
            else
            {
                assemblyPath = Path.Combine(assemblyDirectory, assemblyFileName);
            }

            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }
    }
}
