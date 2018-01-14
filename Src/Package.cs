using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NFox.Pycad
{
    public class Package : Module
    {

        const string InitPyFileName = "__init__.py";

        public static Package Root { get; private set; }

        public DirectoryInfo Directory { get; protected set; }

        public List<Package> Packages { get; }

        public DirectoryInfo DataDirectory
        {
            get { return AutoRegAssem.GetDirectory(Directory, "data"); }
        }

        public List<Module> Modules { get; }

        /// <summary>
        /// 获取所有项目
        /// </summary>
        public static void ImportAllPackages()
        {

            Root = new Package("所有项目");
            Root.Directory = Engine.DebugPath;
            Root.Node = new TreeNode(Root.Name, 0, 0);
            Root.Node.Tag = Root;

            foreach (var dir in Root.Directory.GetDirectories())
                Root.Add(new Package(dir));

            foreach (var dir in Engine.ReleasePath.GetDirectories())
            {
                if (!Root.Contains(dir.Name))
                    Root.Add(new Package(dir, false));
            }

            foreach (var p in Root.Packages)
            {
                Engine.Exec($"import {p.Name}");
                Engine.Keywords.AddBuiltin(p.Name, Engine.Exec(p.Name));
            }

        }

        public void GetFileNames(List<string> names)
        {
            names.Add($"'{File.FullName}'");
            foreach (var m in Modules)
                names.Add($"'{m.File.FullName}'");
            foreach (var p in Packages)
                p.GetFileNames(names);
        }

        protected Package(string name)
            : base(name)
        {
            Packages = new List<Package>();
            Modules = new List<Module>();
        }

        private Package(DirectoryInfo dir)
            : this(dir, true)
        { }

        private Package(DirectoryInfo dir, bool debug)
            : this(dir.Name)
        {
            Directory = dir;
            if (debug)
            {

                File = AutoRegAssem.GetFile(Directory, InitPyFileName);
                Node = new TreeNode(Name, 1, 1);
                Node.Tag = this;
                foreach (var f in Directory.GetFiles("*.py"))
                {
                    if (f.Name != InitPyFileName)
                        Add(new Module(f));
                }
                foreach (var d in Directory.GetDirectories())
                {
                    if (d.Name.ToLower() != "data")
                        Add(new Package(d));
                }
            }
            else
            {
                var dllname = $"{Directory.FullName}\\{Name}.dll";
                if (System.IO.File.Exists(dllname))
                {
                    var code = $"clr.AddReferenceToFileAndPath('{dllname}')";
                    Engine.Exec(code.Replace("\\", "\\\\"));
                }
            }
        }

        public string FindFile(string filename)
        {
            return AutoRegAssem.GetFileFullName(DataDirectory, filename);
        }

        public void Add(Module module)
        {
            if (module is Package)
                Packages.Add(module as Package);
            else
                Modules.Add(module);
            module.Parent = this;
            if(module.Node != null)
                Node.Nodes.Add(module.Node);
        }

        public override void Remove()
        {
            Parent.RemovePackage(this);
        }

        public void RemovePackage(Package package)
        {
            Packages.Remove(package);
            Node.Nodes.Remove(package.Node);
            package.Directory.Delete(true);
        }

        public void RemoveModule(Module module)
        {
            Modules.Remove(module);
            Node.Nodes.Remove(module.Node);
            module.File.Delete();
        }

        public Package GetPackage(string name)
        {
            foreach (var p in Packages)
            {
                if (p.Name == name)
                    return p;
            }
            return null;
        }

        public Module GetModule(string name)
        {
            foreach (var m in Modules)
            {
                if (m.Name == name)
                    return m;
            }
            return null;
        }

        public Package CreateSubPackage(string name)
        {
            var dir = Directory.CreateSubdirectory(name);
            var file = new FileInfo(dir.FullName + "\\" + InitPyFileName);
            using (var fs = file.Create())
                fs.Close();
            var p = new Package(dir);
            Add(p);
            return p;
        }

        public Module CreateModule(string name, string content)
        {
            var file = new FileInfo(Directory.FullName + "\\" + name);
            using (var fs = file.Create())
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(content);
                }
            }
            var m = new Module(file);
            Add(m);
            return m;
        }

        public bool Contains(string name)
        {
            foreach (var p in Packages)
            {
                if (p.Name.ToLower() == name.ToLower())
                    return true;
            }
            if (Modules != null)
            {
                foreach (var m in Modules)
                {
                    if (m.Name.ToLower() == name.ToLower())
                        return true;
                }
            }
            return false;
        }

        public bool Validate(string name, bool ispackage)
        {
            var patt = @"^[a-zA-Z][a-zA-Z0-9]*";
            if (!ispackage)
                patt += @"\.py";
            patt += "$";
            name = name.Trim();
            if (string.IsNullOrEmpty(name))
                throw new System.Exception("名字不允许为空");
            if (Regex.IsMatch(name, patt))
            {
                if (Contains(name))
                    throw new System.Exception("名字重复");
                return true;
            }
            else
            {
                throw new System.Exception("验证错误");
            }
        }

        //要改
        public override Module CopyTo(Package parent)
        {
            Package package  = parent.GetPackage(Name);
            if (package == null)
                package = parent.CreateSubPackage(Name);

            foreach (var m in Modules)
                m.CopyTo(package);
            foreach (var p in Packages)
                p.CopyTo(package);
            return package;

        }


        public override void ChangeName(string name)
        {
            Directory.MoveTo(Parent.Directory.FullName + "\\" + name);
            Node.Text = Name = name;
        }

    }
}
