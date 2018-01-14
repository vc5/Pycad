using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;


namespace NFox.Pycad
{

    [Serializable]
    public struct AssemInfo
    {
        /// <summary>
        /// 注册名
        /// </summary>
        public string Name;

        /// <summary>
        /// 程序集全名
        /// </summary>
        public string Fullname;

        /// <summary>
        /// 程序集路径
        /// </summary>
        public string Loader;

        /// <summary>
        /// 加载方式
        /// </summary>
        public AssemLoadType LoadType;

        /// <summary>
        /// 程序集说明
        /// </summary>
        public string Description;

    }
    public enum AssemLoadType
    {
        Startting = 2,
        ByCommand = 12,
        Disabled = 20
    }


    public abstract class AutoRegAssem : IExtensionApplication
    {

        private AssemInfo _info = new AssemInfo();

        public static FileInfo Location
        {
            get { return new FileInfo(Assembly.GetCallingAssembly().Location); }
        }

        public static DirectoryInfo CurrDirectory
        {
            get { return Location.Directory; }
        }

        public static DirectoryInfo GetDirectory(Assembly assem)
        {
            return new FileInfo(assem.Location).Directory;
        }

        public static DirectoryInfo GetDirectory(DirectoryInfo dir, string name)
        {
            var dirs = dir.GetDirectories(name);
            if (dirs.Length > 0)
                return dirs[0];
            return null;
        }

        public static DirectoryInfo GetDirectory(DirectoryInfo dir, params string[] names)
        {
            foreach (var name in names)
                dir = GetDirectory(dir, name);
            return dir;
        }

        public static DirectoryInfo GetDirectory(string name)
        {
            return GetDirectory(CurrDirectory, name);
        }

        public static DirectoryInfo GetDirectory(params string[] names)
        {
            return GetDirectory(CurrDirectory, names);
        }

        public static FileInfo GetFile(DirectoryInfo dir, string name)
        {
            var files = dir.GetFiles(name);
            if (files.Length > 0)
                return files[0];
            return null;
        }

        public static FileInfo GetFile(string name)
        {
            return GetFile(CurrDirectory, name);
        }

        public static string GetFileFullName(DirectoryInfo dir, string name)
        {
            string path = dir.FullName;
            if (path.Last() != '\\')
                path += "\\";
            return path + name;
        }

        public AutoRegAssem()
        {
            Assembly assem = Assembly.GetCallingAssembly();
            _info.Loader = assem.Location;
            _info.Fullname = assem.FullName;
            _info.Name = assem.GetName().Name;
            _info.LoadType = AssemLoadType.Startting;
            if (!SearchForReg())
                RegApp();
        }


        #region Reg

        private Microsoft.Win32.RegistryKey GetAcAppKey()
        {

            Microsoft.Win32.RegistryKey ackey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    HostApplicationServices.Current.MachineRegistryProductRootKey, true);

            return ackey.CreateSubKey("Applications");

        }

        private bool SearchForReg()
        {

            Microsoft.Win32.RegistryKey appkey = GetAcAppKey();
            var regApps = appkey.GetSubKeyNames();
            return regApps.Contains(_info.Name);

        }

        public void RegApp()
        {

            Microsoft.Win32.RegistryKey appkey = GetAcAppKey();
            Microsoft.Win32.RegistryKey rk = appkey.CreateSubKey(_info.Name);
            rk.SetValue("DESCRIPTION", _info.Fullname, RegistryValueKind.String);
            rk.SetValue("LOADCTRLS", _info.LoadType, RegistryValueKind.DWord);
            rk.SetValue("LOADER", _info.Loader, RegistryValueKind.String);
            rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            appkey.Close();

        }

        public abstract void Initialize();

        public abstract void Terminate();

        #endregion

    }
}

