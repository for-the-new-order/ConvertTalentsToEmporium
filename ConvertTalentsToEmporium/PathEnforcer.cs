using System;
using System.IO;

namespace ConvertTalentsToEmporium
{
    public class PathEnforcer
    {
        public (string[] sourcePaths, string destinationPath) FindPath(string[] args)
        {
            if (args.Length >= 2)
            {
                var sources = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                {
                    sources[i] = EnforceValidPath(args[i], PathType.Source);
                }
                var dest = EnforceValidPath(args[args.Length - 1], PathType.Destination);
                return (sources, dest);
            }
            else
            {
                var source = EnforceValidPath(null, PathType.Source);
                var dest = EnforceValidPath(null, PathType.Destination);
                return (new[] { source }, dest);
            }
        }

        private string EnforceValidPath(string path, PathType pathType)
        {
            if (path == null)
            {
                Console.WriteLine($"Please enter path for '{pathType}': ");
                path = Console.ReadLine();
            }
            if (pathType == PathType.Source && !File.Exists(path))
            {
                Console.WriteLine($"Path '{path}' does not exist.");
                path = EnforceValidPath(null, pathType);
            }
            else if (pathType == PathType.Destination)
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"Directory '{directory}' does not exist.");
                    path = EnforceValidPath(null, pathType);
                }
            }
            return path;
        }

        private enum PathType
        {
            Source,
            Destination
        }
    }
}
