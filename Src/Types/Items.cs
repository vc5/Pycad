using AutocompleteMenuNS;
using System;

namespace NFox.Pycad.Types
{

    public class TopItem : AutocompleteItem
    {
        TypeBase Type { get; }

        public TopItem(TypeBase type)
            : base(type.Name)
        {
            Type = type;
            ImageIndex = type.ImageIndex;
        }

        public override string ToolTipTitle
        {
            get { return Type.ToolTipTitle; }
            set { throw new NotImplementedException(); }
        }

        public override string ToolTipText
        {
            get { return Type.ToolTipText; }
            set { throw new NotImplementedException(); }
        }

    }

    public class SubItem : MethodAutocompleteItem
    {
        TypeBase Type { get; }

        public SubItem(TypeBase type)
            : base(type.Name)
        {
            Type = type;
            ImageIndex = type.ImageIndex;
        }

        public override string ToolTipTitle
        {
            get { return Type.ToolTipTitle; }
            set { throw new NotImplementedException(); }
        }

        public override string ToolTipText
        {
            get { return Type.ToolTipText; }
            set { throw new NotImplementedException(); }
        }


    }

}
