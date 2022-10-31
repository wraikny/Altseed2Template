using System;
using System.Reflection;
using System.IO;

using Altseed2;

namespace Altseed2Template
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new Configuration()
            {
                ConsoleLoggingEnabled = true,
            };

            Engine.Initialize("Altseed2Template", 800, 600, config);

            LoadResources();

            while (Engine.DoEvents())
            {
                Engine.Update();
            }

            Engine.Terminate();
        }

        private static string LoadEmbeddedTextFile(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(filename);

            if (stream is null) throw new Exception($"Failed to GetManifestResourceStream({filename})");

            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        private static void LoadResources()
        {

#if DEBUG
            Engine.File.AddRootDirectory("Resources");
#else
            var password = LoadEmbeddedTextFile("Altseed2Template.ResourcesPassword.txt");
            Engine.File.AddRootPackageWithPassword("Resources.pack", password);
#endif
        }
    }
}
