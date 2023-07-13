using System;
using System.IO;

namespace TechTonic.SharedProject
{
    public static class PathUtils
    {
        /// <summary>
        /// This combines an OS Native ProjectPath and a windows based relative Reference
        /// </summary>
        /// <param name="projectPath">This is native to the current OS</param>
        /// <param name="reference">This is a windows based relative path</param>
        /// <returns></returns>
        public static string CombineReference(string projectPath, string reference)
        {
            if (string.IsNullOrEmpty(projectPath)) throw new ArgumentNullException(projectPath);
            if (string.IsNullOrEmpty(reference)) throw new ArgumentNullException(reference);

            projectPath = Directory.GetParent(projectPath)!.FullName;

            return CombineRelative(projectPath, reference);
        }

        /// <summary>
        /// This combines an OS Native basePath and a windows based relative Path
        /// </summary>
        /// <param name="basePath">This is native to the current OS</param>
        /// <param name="relativePath">This is a windows based relative path</param>
        /// <returns></returns>
        public static string CombineRelative(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(basePath);
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(relativePath);

            while (relativePath.StartsWith("..\\", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring(3);
                basePath = Directory.GetParent(basePath)!.FullName;

            }
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                relativePath = relativePath.Replace('\\', '/');
            }

            return Path.GetFullPath(Path.Join(basePath, relativePath));
        }

        /// <summary>
        /// This checks the given path is the ACTUAL casing for this file
        /// This has only been  tested on windows - Unix can check casing out of the box
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>True if casing is the same</returns>
        public static bool CheckPathCasing(string filePath)
        {
            //A good start
            if (!File.Exists(filePath) && !Directory.Exists(filePath)) return false;

            //Check the case of the file
            var file = Path.GetFileName(filePath);
            var folder = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(folder)) return false;

            if (File.Exists(filePath) && !string.Equals(filePath, Directory.GetFiles(folder, file)[0], StringComparison.Ordinal)) return false;

            //Go up the tree checking each Directory
            while (!string.IsNullOrEmpty(folder))
            {
                string thisDirectory = Path.GetFileName(folder);
                if (string.IsNullOrEmpty(thisDirectory)) return true;
                if (!string.Equals(folder, Directory.GetParent(folder)!.GetDirectories(thisDirectory)[0].FullName, StringComparison.Ordinal)) return false;
                folder = Directory.GetParent(folder)!.FullName;
            }

            return true;
        }
    }
}
