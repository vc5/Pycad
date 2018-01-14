using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NFox.Pycad.Types
{
    public class Variant : TypeBase
    {

        protected dynamic _obj;

        public static dynamic Clr
        {
            get { return Engine.Exec("clr"); }
        }

        public bool Callable
        {
            get { return Engine.Exec("callable")(_obj); }
        }

        public bool HasAttr(string key)
        {
            return Engine.Exec("hasattr")(_obj, key);
        }

        public dynamic GetAttr(string key)
        {
            return Engine.Exec("getattr")(_obj, key);
        }

        public dynamic Type
        {
            get { return Engine.Exec("type")(_obj); }
        }

        public dynamic ClrType
        {
            get { return Clr.GetClrType(Type); }
        }

        public override string ToolTipTitle
        {
            get { return $"变量:{Name}"; }
        }

        public override string ToolTipText
        {
            get { return $"类型:{ClrType.FullName}"; }
        }

        public Variant(string text, object obj)
        {
            Name = text;
            _obj = obj;
            ImageIndex = 1;
        }

        public static Variant GetValue(string key, object obj)
        {
            

            if (obj is System.Enum)
            {
                return new Variant(key, obj);
            }
            else if (obj is PythonType)
            {
                var t = Clr.GetClrType(obj);
                if (t.IsEnum)
                    return new Enum(key, obj);
                return new Type(key, obj);
            }

            if (obj is NamespaceTracker)
                return new Namespace(key, obj);
            else if (obj is System.Enum)
                return new Enum(key, obj);
            else if (obj is ReflectedEvent.BoundEvent)
                return new Event(key, obj);
            else if (obj is PythonModule)
                return new Module(key, obj);
            else if (obj is OldClass)
                return new Class(key, obj);
            else if (obj is PythonProperty)
                return new Property(key, obj);
            else if (obj is Method)
                return new Function(key, obj);
            else if (obj is BuiltinFunction)
                return new Function(key, obj);
            else if (obj is PythonFunction)
                return new Function(key, obj);
            else if (Engine.Exec("callable")(obj))
                return new Function(key, obj);
            else
                return new Variant(key, obj);

        }

        public Variant GetItem(string key)
        {
            if (Marshal.IsComObject(_obj))
                return null;
            try { return Variant.GetValue(key, GetAttr(key)); }
            catch { return null; }
        }

        public virtual IEnumerable<Variant> GetItems()
        {
            foreach (string key in Clr.Dir(_obj))
            {
                dynamic obj = null;
                try { obj = GetAttr(key); }
                catch { }
                if (obj != null)
                    yield return Variant.GetValue(key, obj);
            }
        }

    }

    public class Property : Variant
    {
        public override string ToolTipTitle
        {
            get { return $"属性:{Name}"; }
        }
        
        public override string ToolTipText
        {
            get { return null; }
        }

        public Property(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 2;
        }
    }


    public class Function : Variant
    {
        public override string ToolTipTitle
        {
            get { return $"函数:{Name}"; }
        }

        public override string ToolTipText
        {
            get
            {
                var doc = _obj.__doc__ as string;
                if (!string.IsNullOrEmpty(doc))
                {
                    var patt = $@"^({Name}\(.*?\)){{2,}}$";
                    if (Regex.IsMatch(doc, patt))
                    {
                        var ms = Regex.Matches(doc, $@"({Name}\(.*?\))");
                        return string.Join("\n", ms.Cast<Match>().Select(m => m.Value));
                    }
                }
                return doc;
            }
        }

        public Function(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 3;
        }
    }

    public class Module : Variant
    {

        public override string ToolTipTitle
        {
            get { return $"模块:{_obj.__name__}"; }
        }

        public override string ToolTipText
        {
            get { return _obj.__doc__ ?? _obj.__file__; }
        }

        public Module(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 4;
        }
    }

    public class Class : Variant
    {

        public override string ToolTipTitle
        {
            get { return $"类:{_obj.__name__}"; }
        }

        public override string ToolTipText
        {
            get { return _obj.__file__; }
        }

        public Class(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 5;
        }
    }

    public class Event : Variant
    {
        public override string ToolTipTitle
        {
            get { return $"事件:{Name}"; }
        }

        public override string ToolTipText
        {
            get { return $"类型:{_obj.Event.Info.EventHandlerType.FullName}"; }
        }

        public Event(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 6;
        }
    }

    public class Namespace : Variant
    {

        public override string ToolTipTitle
        {
            get { return $"命名空间"; }
        }

        public override string ToolTipText
        {
            get { return _obj.Name; }
        }

        public Namespace(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 7;
        }
    }

    public class Type : Variant
    {
        public override string ToolTipTitle
        {
            get { return $"类型:{Name}"; }
        }

        public override string ToolTipText
        {
            get
            {
                try
                {
                    if (HasAttr("__doc__"))
                        return _obj.__doc__;
                    else if (Callable)
                        return _obj.__call__.__doc__;
                }
                catch { }
                return ClrType.FullName;
            }
        }

        public Type(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 5;
        }
    }

    public class Enum : Type
    {

        public override string ToolTipTitle
        {
            get { return $"枚举:{Name}"; }
        }

        public override string ToolTipText
        {
            get
            {
                var names = System.Enum.GetNames(_obj);
                var values = System.Enum.GetValues(_obj);
                var res = new List<string>();
                for (int i = 0; i < names.Length; i++)
                    res.Add($"{names[i]}({(int)values[i]})");
                return string.Join("\n", res);
            }
        }

        public Enum(string text, object obj)
            : base(text, obj)
        {
            ImageIndex = 8;
        }

    }

}
