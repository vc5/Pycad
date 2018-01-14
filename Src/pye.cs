using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using IronPython.Runtime;
using System.Linq;

namespace NFox.Pycad
{
    public class pye
    {

        public static void run(string name)
        {
            Engine.Run(name);
        }

        public static object invoke(string name, ResultBuffer args)
        {
            return Engine.Invoke(name, args);
        }

        public static void addcommand(dynamic cmd)
        {
            Engine.AddCommand(cmd);
        }

        public static void addlisp(dynamic lisp)
        {
            Engine.AddLisp(lisp);
        }

        public static void debug(string name)
        {
            Engine.Debug(name);
        }

        public static object debug(string name, List args)
        {
            return Engine.Debug(name, args);
        }

        public static dynamic execute(string code)
        {
            return Engine.Exec(code);
        }

        public static void build(PythonModule module)
        {
            Engine.Build(module);
        }

        public static string findfile(string modulename, string filename)
        {
            if (modulename.Contains('.'))
                modulename = modulename.Substring(0, modulename.IndexOf('.'));
            return Package.Root.GetPackage(modulename).FindFile(filename);
        }

        public static void hide()
        {
            ConsoleStream.HideForm();
        }

        public static void show()
        {
            ConsoleStream.ShowForm();
        }

    }
}
