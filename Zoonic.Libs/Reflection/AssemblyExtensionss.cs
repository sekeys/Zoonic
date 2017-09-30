

namespace Zoonic
{
    public class AssemblyExtensionss
    {
        public static string RootPath;
        public static System.Reflection.Assembly Load(string assembly)
        {
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly);
        }

        public static System.Type Type(string assembly, string type)
        {
            var ass = Load(assembly);
            if (ass == null)
            {
                return null;
            }
            return ass.GetType(type);
        }
    }
}
