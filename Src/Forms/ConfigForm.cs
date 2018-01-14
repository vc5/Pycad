using System.IO;
using System.Windows.Forms;

namespace NFox.Pycad
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
            var code = new CodeTextBox();
            code.Text = File.ReadAllText(AutoRegAssem.GetFile(Engine.SupportPath, "acad.py").FullName);
            Controls.Add(code);
        }
    }
}
