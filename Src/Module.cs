using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NFox.Pycad
{
    public class Module
    {

        public TreeNode Node { get; protected set; }

        public Package Parent { get; set; }

        public string Name { get; protected set; }

        public FileInfo File { get; protected set; }

        public string Text
        {
            get { return Node.Text; }
            set { Node.Text = value; }
        }

        protected Module(string name)
        {
            Name = name;
        }

        public Module(FileInfo path)
            : this(path.Name)
        {
            File = path;
            Node = new TreeNode(Name, 2, 2);
            Node.Tag = this;
        }

        public virtual void ChangeName(string name)
        {
            File.MoveTo(Parent.Directory.FullName + "\\" + name);
            Node.Text = Name = name;
        }

        public virtual void Remove()
        {
            Parent.RemoveModule(this);
        }

        public static bool Over { get; set; }

        public virtual Module CopyTo(Package parent)
        {
            var m = parent.GetModule(Name);
            if (m == null)
            {
                var file = File.CopyTo(parent.Directory.FullName + "\\" + File.Name);
                m = new Module(file);
                parent.Add(m);
            }
            else if(Over)
            {
                m.File = File.CopyTo(parent.Directory.FullName + "\\" + File.Name);
            }
            return m;
        }

        public bool IsPartOf(Module other)
        {
            if (other is Package)
            {
                var p = this;
                while (p != null)
                {
                    if (p == other)
                        return true;
                    p = p.Parent; 
                }
            }
            return false;
        }

        public bool IsSubOf(Module other)
        {
            return Parent == other;
        }

    }
}
