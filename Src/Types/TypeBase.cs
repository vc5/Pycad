using System;
using System.Collections.Generic;

namespace NFox.Pycad.Types
{
    public abstract class TypeBase : IComparable<TypeBase>
    {

        public string Name { get; protected set; }

        public virtual string ToolTipTitle { get; }

        public virtual string ToolTipText { get; }

        public int ImageIndex { get; protected set; }


        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(TypeBase other)
        {

            if (Name[0] == '_')
            {
                if (other.Name[0] == '_')
                    return Name.CompareTo(other.Name);
                else
                    return 1;
            }
            else
            {
                if (other.Name[0] == '_')
                    return -1;
                else
                    return Name.CompareTo(other.Name);
            }
        }

    }
}
