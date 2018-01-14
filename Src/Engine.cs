using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.CSharp;
using Microsoft.Scripting.Hosting;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

[assembly: ExtensionApplication(typeof(NFox.Pycad.Engine))]
[assembly: CommandClass(typeof(NFox.Pycad.Engine))]

namespace NFox.Pycad
{

    /// <summary>
    /// Pycad引擎
    /// </summary>
    public class Engine : AutoRegAssem
    {

        internal static Types.KeywordList Keywords;

        #region Paths

        /// <summary>
        /// 当前目录
        /// </summary>
        public static DirectoryInfo Path { get; private set; }

        /// <summary>
        /// Autocad目录
        /// </summary>
        public static DirectoryInfo AcPath { get; private set; }

        /// <summary>
        /// IronPython目录
        /// </summary>
        public static FileInfo PythonLibPath { get; private set; }

        /// <summary>
        /// 支持文件目录
        /// </summary>
        public static DirectoryInfo SupportPath { get; private set; }


        public static DirectoryInfo ProjectPath { get; private set; }
        
        public static DirectoryInfo DebugPath { get; private set; }

        public static DirectoryInfo ReleasePath { get; private set; }

        #endregion

        #region ExtensionApplication

        public override void Initialize()
        {

            Path = CurrDirectory;
            var assembly = Assembly.GetAssembly(typeof(Entity));
            AcPath = GetDirectory(assembly);
            SupportPath = GetDirectory(Path, "Support");
            PythonLibPath = GetFile(SupportPath, "Lib.zip");
            ProjectPath = GetDirectory(Path, "Projects");
            DebugPath = GetDirectory(ProjectPath, "Debug");
            ReleasePath = GetDirectory(ProjectPath, "Release");

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            PythonReBulid();

        }

        static Assembly _ass = Assembly.GetExecutingAssembly();
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {

            if (args.Name == _ass.FullName)
                return _ass;
            return null;
        }

        public override void Terminate() { }

        #endregion

        #region Cmds

        [CommandMethod("CSTEST")]
        public static void Test()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var t = DateTime.Now;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var currspace = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                for (int i = 0; i < 100000; i++)
                {
                    var cir = new Circle(new Autodesk.AutoCAD.Geometry.Point3d(), Autodesk.AutoCAD.Geometry.Vector3d.ZAxis, 2);
                    currspace.AppendEntity(cir);
                    tr.AddNewlyCreatedDBObject(cir, true);
                }
                tr.Commit();
            }
            var a = DateTime.Now - t;
            doc.Editor.WriteMessage($"画100000个圆花费时间:{a.TotalMilliseconds}毫秒");

        }

        [CommandMethod("PYRL")]
        public static void PythonReLoad()
        {
            new Action<bool>(InitEngine).BeginInvoke(false, null, null);
        }

        [CommandMethod("PYRB")]
        public static void PythonReBulid()
        {
            new Action<bool>(InitEngine).BeginInvoke(true, null, null);
        }

        [CommandMethod("PYE")]
        public static void ShowPyIde()
        {
            PyIdeForm frm = new PyIdeForm();
            Application.ShowModalDialog(frm);
        }

        #endregion

        #region DynamicAssembly

        private static int _tempIndex;

        private static Dictionary<string, dynamic> _cmds;
        private static Dictionary<string, dynamic> _lisps;

        internal static void AddCommand(dynamic cmd)
        {
            if (_cmds.ContainsKey(cmd.name))
                throw new System.Exception($"重复的命令名({cmd.name}).");
            _cmds.Add(cmd.name, cmd);
        }

        internal static void AddLisp(dynamic lisp)
        {
            if (_lisps.ContainsKey(lisp.name))
                throw new System.Exception($"重复的函数名({lisp.name}).");
            _lisps.Add(lisp.name, lisp);
        }

        private static void MakeDynamicAssembly(bool build)
        {

            //加载模块并利用装饰器获取函数信息
            _cmds = new Dictionary<string, dynamic>();
            _lisps = new Dictionary<string, dynamic>();
            Package.ImportAllPackages();

            if (build)
            {

                try
                {

                    //按py文件的内容生成对应的动态程序集

                    //声明代码的部分
                    CodeCompileUnit compunit = new CodeCompileUnit();
                    CodeNamespace ns = new CodeNamespace($"NFox.Pycad{_tempIndex++}");
                    compunit.Namespaces.Add(ns);

                    //引用命名空间
                    ns.Imports.Add(new CodeNamespaceImport("System"));
                    ns.Imports.Add(new CodeNamespaceImport("NFox.Pycad"));
                    ns.Imports.Add(new CodeNamespaceImport("Autodesk.AutoCAD.Runtime"));
                    ns.Imports.Add(new CodeNamespaceImport("Autodesk.AutoCAD.DatabaseServices"));

                    //在命名空间下添加一个类
                    CodeTypeDeclaration cmdsclass = new CodeTypeDeclaration("Commands");
                    ns.Types.Add(cmdsclass);

                    //为每个py函数按装饰器生成命令
                    int i = 0;
                    foreach (var cmd in _cmds)
                    {
                        var method = MakeCmdMethod(cmd.Value, i++);
                        cmdsclass.Members.Add(method);
                    }

                    //为每个py函数按装饰器生成lisp函数
                    i = 0;
                    foreach (var lisp in _lisps)
                    {
                        var method = MakeLispMethod(lisp.Value, i++);
                        cmdsclass.Members.Add(method);
                    }

                    //生成驻留内存的动态程序集
                    CompilerParameters pars = new CompilerParameters();
                    pars.CompilerOptions = "/target:library /optimize";
                    pars.GenerateExecutable = false;
                    pars.GenerateInMemory = true;

                    //添加引用
                    var asslst = pars.ReferencedAssemblies;
                    asslst.Add("System.dll");
                    asslst.Add("System.Core.dll");
                    asslst.Add("Microsoft.CSharp.dll");
                    var file = GetFile(CurrDirectory, "IronPython.dll");
                    if (file != null)
                        asslst.Add(file.FullName);
                    asslst.Add(Location.FullName);

                    var mainmgds = _scope.GetVariable("mgds")[0];
                    foreach (string name in mainmgds)
                    {
                        string fullname = GetFile(AcPath, name)?.FullName;
                        if (fullname != null)
                            asslst.Add(fullname);
                    }

                    //编译并加载
                    CSharpCodeProvider cprovider = new CSharpCodeProvider();
                    CompilerResults cr =
                        cprovider.CompileAssemblyFromDom(pars, compunit);

                }
                catch(System.Exception ex)
                {
                    Application.ShowAlertDialog("Err:" + ex.Message);
                }
            }

        }


        private static CodeMemberMethod MakeCmdMethod(dynamic cmd, int index)
        {

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = $"Cmd_{index}";
            var args = new List<CodeAttributeArgument>();

            if ((int)cmd.flags != -1)
                args.Add(
                    new CodeAttributeArgument(
                        new CodeCastExpression(
                            "CommandFlags",
                            new CodePrimitiveExpression((int)cmd.flags))));

            //设置命令名
            args.Insert(0,
                new CodeAttributeArgument(
                    new CodePrimitiveExpression(cmd.name)));
            method.CustomAttributes.Add(
                new CodeAttributeDeclaration(
                    "CommandMethod",
                    args.ToArray()));
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;

            method.Statements.Add(
                new CodeSnippetStatement(
                    $"Engine.Run(\"{cmd.name}\");"));
            return method;

        }

        private static CodeMemberMethod MakeLispMethod(dynamic lisp, int index)
        {

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = $"Lisp_{index}";
            var args = new List<CodeAttributeArgument>();

            //设置命令名
            args.Add(
                new CodeAttributeArgument(
                    new CodePrimitiveExpression(lisp.name)));
            method.CustomAttributes.Add(
                new CodeAttributeDeclaration(
                    "LispFunction",
                    args.ToArray()));
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            method.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    "ResultBuffer", "rb"));
            method.ReturnType = new CodeTypeReference("Object");

            method.Statements.Add(
                new CodeSnippetStatement(
                    $"return Engine.Invoke(\"{lisp.name}\", rb);"));
            return method;

        }

        #endregion

        #region Engine

        static ScriptEngine _engine;
        static ScriptScope _scope;

        private static void InitEngine(bool build)
        {
            try
            {

                //生成py引擎
                var options = new Dictionary<string, object>();
                options["Frames"] = true;
                options["FullFrames"] = true;
                _engine = Python.CreateEngine(options);

                //设置搜索目录
                var paths = _engine.GetSearchPaths();
                paths.Add(AcPath.FullName);
                paths.Add(Path.FullName);
                paths.Add(PythonLibPath.FullName);
                paths.Add(SupportPath.FullName);
                paths.Add(DebugPath.FullName);
                paths.Add(ReleasePath.FullName);
                _engine.SetSearchPaths(paths);

                var runtime = _engine.Runtime;

                //输入输出重定向
                var cs = new ConsoleStream();
                runtime.IO.SetOutput(cs, Encoding.UTF8);
                runtime.IO.SetErrorOutput(cs, Encoding.UTF8);
                runtime.IO.SetInput(cs, Encoding.UTF8);

                //加载pye帮助类
                dynamic clr = runtime.GetClrModule();
                var builtin = runtime.GetBuiltinModule();
                builtin.SetVariable("pye", clr.GetPythonType(typeof(pye)));

                //从acad.py中获取初始化信息
                _scope = runtime.ExecuteFile(GetFile(SupportPath, "acad.py").FullName);
                
                //获取关键字列表
                Keywords = new Types.KeywordList();
                foreach (string key in Exec("keyword.kwlist"))
                    Keywords.AddStatement(key);

                foreach (var kv in builtin.GetItems())
                    Keywords.AddBuiltin(kv.Key, kv.Value);

                MakeDynamicAssembly(build);

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Err:" + ex.Message);
            }
        }

        public static void Run(string name)
        {
            _cmds[name]();
        }

        public static object Invoke(string name, ResultBuffer args)
        {
            return _lisps[name](args);
        }

        internal static Editor GetAcEditor()
        {
            return
                Autodesk.AutoCAD
                .ApplicationServices
                .Application
                .DocumentManager?
                .MdiActiveDocument?
                .Editor;
        }

        internal static void Debug(string name)
        {
            _cmds[name](true);
        }

        internal static object Debug(string name, List args)
        {
            return _lisps[name](args);
        }

        internal static dynamic GetValue(string name)
        {
            return _scope.GetVariable(name);
        }

        internal static void Print(object res)
        {
            _scope.SetVariable("TempValue", res);
            Exec("print TempValue");
            _scope.RemoveVariable("TempValue");
        }

        internal static dynamic Exec(string code)
        {
            ScriptSource script =
               _engine.CreateScriptSourceFromString(code);
            return script.Execute(_scope);
        }

        internal static void Build(dynamic module)
        {
            var name = module.__name__;
            var package = Package.Root.GetPackage(name);
            if (package.Node != null)
            {

                GetDirectory(ReleasePath, name)?.Delete(true);
                var dir = ReleasePath.CreateSubdirectory(name);
                if (package.DataDirectory != null)
                {
                    var newdatadir = dir.CreateSubdirectory("data");
                    foreach (var file in package.DataDirectory.GetFiles())
                        file.CopyTo($"{newdatadir.FullName}\\{file.Name}");
                }

                string dllname = $"'{dir.FullName}\\{name}.dll'";
                List<string> names = new List<string>();
                package.GetFileNames(names);
                var code = $"clr.CompileModules({dllname},{string.Join(",", names)});";
                Exec(code.Replace("\\", "\\\\"));

            }
        }

        #endregion

    }

}
