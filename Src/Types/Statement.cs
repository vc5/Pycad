namespace NFox.Pycad.Types
{
    public class Statement : TypeBase
    {
        public override string ToolTipTitle
        {
            get { return $"{Name}语句"; }
        }

        public override string ToolTipText
        {
            get { return $"输入?{Name}以获取更多信息"; }
        }

        public Statement(string name)
        {
            Name = name;
            ImageIndex = 0;
        }
    }
}
