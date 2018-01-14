using Autodesk.AutoCAD.DatabaseServices;
using IronPython.Runtime;

namespace NFox.Pycad
{
    public class lisp
    {

        public string Name { get; protected set; }

        public dynamic Func { get; protected set; }

        public lisp()
        {
            Name = null;
        }

        public lisp(string name)
        {
            Name = name;
        }

        public lisp __call__(PythonFunction func)
        {
            if (Name == null)
                Name = func.__name__;
            Func = func;
            Engine.AddFunction(this);
            return this;
        }

        public object __call__(ResultBuffer args)
        {
            return 
                Engine.Conv.ToLispData(
                    __call__(Engine.Conv.FromLispData(args)));
        }

        public object __call__(List args)
        {
            return Func(args);
        }

    }
}
