using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using IronPython.Runtime;
using System.Collections.Generic;

namespace NFox.Pycad
{
    public class command : lisp
    {

        public int Flags { get; protected set; }

        public command()
        {
            Flags = -1;
        }

        public command(string name)
        {
            Flags = -1;
        }

        public command(CommandFlags flags)
        {
            Flags = (int)flags;
        }

        public command(string name, CommandFlags flags)
        {
            Name = name;
            Flags = (int)flags;
        }

        public void __call__()
        {
            Func();
        }

    }





}
