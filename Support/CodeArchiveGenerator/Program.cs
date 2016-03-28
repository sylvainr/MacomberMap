using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeArchiveGenerator
{
    class Program
    {

       private static String[] ExtensionsToIgnore = ".tmp,.lnk,.zip".Split(',');
       private static String[] DirectoriesToIgnore = ".vs,bin,obj,packages".Split(',');

        [STAThread]
        static void Main(string[] args)
        {

            String RootDirectory = new DirectoryInfo(@"..\..\..\..\..").FullName;
            String TargetFile = Path.Combine(RootDirectory, "MacomberMapOpenSource-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".zip");
            if (File.Exists(TargetFile))
                File.Delete(TargetFile);
            using (ZipFile OutFile = new ZipFile(TargetFile))
            {
                ProcessDirectory(new DirectoryInfo(RootDirectory),OutFile,RootDirectory);
                OutFile.Save();
            }
            Clipboard.SetText(TargetFile);
        }

        /// <summary>
        /// Process an individiaul directory, adding its files to the archive
        /// </summary>
        /// <param name="CurrentDirectory"></param>
        /// <param name="OutFile"></param>
        /// <param name="RootDirectory"></param>
        private static void ProcessDirectory(DirectoryInfo CurrentDirectory, ZipFile OutFile, String RootDirectory)
        {
            foreach (FileInfo fI in CurrentDirectory.GetFiles("*.*"))
                    if (!ExtensionsToIgnore.Contains(fI.Extension.ToLower()))
                        OutFile.AddFile(fI.FullName, fI.DirectoryName.Replace(RootDirectory, ""));
            foreach (DirectoryInfo dI in CurrentDirectory.GetDirectories())
                if (!DirectoriesToIgnore.Contains(dI.Name.ToLower()))
                    ProcessDirectory(dI, OutFile, RootDirectory);


        }
    }
}