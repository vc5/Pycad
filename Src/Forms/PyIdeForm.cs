
using CCWin.SkinControl;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NFox.Pycad
{
    public partial class PyIdeForm : Form
    {

        public PyIdeForm()
        {
            InitializeComponent();
            ConsoleStream.SetTo(this, consoleTextBox1);
            Engine.Exec("ed.getcurr()");
            treeView1.Nodes.Add(Package.Root.Node);
            treeView1.ExpandAll();
            treeView1.HideSelection = false;

            XElement xe =
                XElement.Load(
                    AutoRegAssem.GetFileFullName(
                        Engine.SupportPath, "Config.xml"));
            XElement xtemplates = xe.Element("Templates");
            foreach (var e in xtemplates.Elements())
            {
                var name = e.Attribute("Name").Value;
                var content = e.Value;
                var mnu = mnuNew.DropDownItems.Add(name);
                mnu.Tag = e.Value;
                mnu.Click += mnuNewModule_Click;
            }
        }

        private void frmPyIde_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConsoleStream.SetTo(null, null);
            treeView1.Nodes.Clear();
            Engine.PythonReBulid();
        }

        #region TreeView

        private void skinTabControl1_TabePageClosing(object sender, TabPageEventArgs e)
        {
            SaveFile(e.ColseTabPage);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

            var m = e.Node.Tag as Module;
            if (m.Parent == null)
                return;

            FileInfo file = m.File;

            SkinTabPage page = null;
            foreach (SkinTabPage p in skinTabControl1.TabPages)
            {
                if (p.Tag == m)
                {
                    page = p;
                    break;
                }
            }

            CodeTextBox code;
            if (page == null)
            {
                code = new CodeTextBox();
                code.ImageList = imageList2;
                code.Text = File.ReadAllText(file.FullName);
                page = new SkinTabPage(e.Node.Text);
                page.Tag = m;
                page.Controls.Add(code);
                skinTabControl1.TabPages.Add(page);
            }
            else
            {
                code = page.Controls[0] as CodeTextBox;
            }
            skinTabControl1.Focus();
            skinTabControl1.SelectedTab = page;
        }

        Module _edited;

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {

                treeView1.SelectedNode = e.Node;

                ContextMenuStrip mnu;
                if (e.Node == Package.Root.Node)
                {
                    mnu = mnuRoot;
                }
                else
                {

                    mnu = mnuModule;

                    if (e.Node.Tag is Package)
                    {
                        mnuNewPackage2.Enabled = true;
                        mnuNew.Enabled = true;
                        mnuPaste.Enabled = _edited != null;
                    }
                    else
                    {
                        mnuNewPackage2.Enabled = false;
                        mnuNew.Enabled = false;
                        mnuPaste.Enabled = false;
                    }
                }

                mnu.Show(treeView1.PointToScreen(e.Location));

            }
        }

        private void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            e.CancelEdit = e.Node == Package.Root.Node;
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            var m = e.Node.Tag as Module;
            try
            {
                m.Parent.Validate(e.Label, m is Package);
                m.ChangeName(e.Label);
                foreach (TabPage page in skinTabControl1.TabPages)
                {
                    if (page.Tag == m)
                    {
                        page.Text = e.Label;
                        break;
                    }
                    
                }
            }
            catch(System.Exception ex)
            {
                e.CancelEdit = true;
                MessageBox.Show(this, ex.Message, "验证错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Menu

        bool _cut;

        private void mnuCut_Click(object sender, System.EventArgs e)
        {
            _cut = true;
            _edited = treeView1.SelectedNode.Tag as Module;
        }

        private void mnuCopy_Click(object sender, System.EventArgs e)
        {
            _cut = false;
            _edited = treeView1.SelectedNode.Tag as Module;
        }

        //要改，是否覆盖
        private void mnuPaste_Click(object sender, System.EventArgs e)
        {
            var p = treeView1.SelectedNode.Tag as Package;
            if (_edited.IsSubOf(p) || p.IsPartOf(_edited))
                return;
            _edited.CopyTo(p);
            p.Node.ExpandAll();
            if (_cut)
            {
                ClosePage(_edited);
                _edited.Remove();
                _cut = false;
            }
            _edited = null;
        }

        private void mnuDel_Click(object sender, System.EventArgs e)
        {
            var m = treeView1.SelectedNode.Tag as Module;
            ClosePage(m);
            m.Remove();
            _cut = false;
            _edited = null;
        }

        private void ClosePage(Module module)
        {
            if (module is Package)
            {
                var package = module as Package;
                foreach (TabPage page in skinTabControl1.TabPages)
                {
                    Module m = page.Tag as Module;
                    if (m.IsPartOf(package))
                        skinTabControl1.TabPages.Remove(page);
                }
            }
            else
            {
                foreach (TabPage page in skinTabControl1.TabPages)
                {
                    Module m = page.Tag as Module;
                    if (m== module)
                        skinTabControl1.TabPages.Remove(page);
                }
            }
        }

        private void mnuNewPackage_Click(object sender, System.EventArgs e)
        {
            var package = treeView1.SelectedNode.Tag as Package;
            InputBoxValidation validation = s => package.Validate(s, true);
            string name = "abc";
            if (InputBox.Show("输入包名", "名称:", ref name, validation) == DialogResult.OK)
            {
                package.CreateSubPackage(name);
                package.Node.ExpandAll();
            }

        }

        private void mnuNewModule_Click(object sender, System.EventArgs e)
        {
            var package = treeView1.SelectedNode.Tag as Package;
            InputBoxValidation validation = s => package.Validate(s, false);
            string name = "abc.py";
            if (InputBox.Show("输入模块名", "名称:", ref name, validation) == DialogResult.OK)
            {
                var mnu = sender as ToolStripItem;
                var content = string.Format(mnu.Tag as string, name.Substring(0, name.Length - 3));
                package.CreateModule(name, content);
                package.Node.Expand();
            }
        }

        #endregion

        #region Console

        private void consoleTextBox1_CommandStartting(object sender, ConsoleTextBox.CommandEventArgs e)
        {
            string cmd = e.Command;
            var patts =
                new Dictionary<string, string>
                {
                    { @"^\?(.*?)$", "help('$1')"},
                    { @"^\@(.*?)\((.*?)\)$", "pye.debug('$1',$2)" } ,
                    { @"^\@(.*?)$", "pye.debug('$1')" },
                    { @"^\$(.*?)$", "import pdb;pdb.set_trace();$1"},
                };
            foreach (var kv in patts)
            {
                if(Regex.IsMatch(cmd, kv.Key))
                    cmd = Regex.Replace(cmd, kv.Key, kv.Value);
            }
            e.Result = Engine.Exec(cmd);
        }

        private void consoleTextBox1_CommandEnded(object sender, ConsoleTextBox.CommandEventArgs e)
        {
            if (e.Result != null)
                Engine.Print(e.Result);
        }

        #endregion

        #region Toolbar

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            var page = skinTabControl1.SelectedTab;
            if (page != null)
                    SaveFile(page);
        }

        private void btnSaveAll_Click(object sender, System.EventArgs e)
        {
            foreach (SkinTabPage page in skinTabControl1.TabPages)
                SaveFile(page);
        }

        private void btnConfig_Click(object sender, System.EventArgs e)
        {
            ConfigForm frm = new ConfigForm();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
        }

        private void SaveFile(TabPage page)
        {
            var m = page.Tag as Module;
            FileInfo file = m.File; 
            var code = page.Controls[0] as CodeTextBox;
            using (var sw = new StreamWriter(file.FullName, false, Encoding.UTF8))
                sw.Write(code.Text);
        }

        #endregion

    }
}
