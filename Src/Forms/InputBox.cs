using System;
using System.Drawing;
using System.Windows.Forms;

namespace NFox.Pycad
{

    public delegate void InputBoxValidation(string errorMessage);

    public class InputBox
    {
        public static DialogResult Show(string title, string promptText, ref string value)
        {
            return Show(title, promptText, ref value, null);
        }

        public static DialogResult Show(string title, string promptText, ref string value,
                                        InputBoxValidation validation)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            textBox.SelectionLength = 0;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 16, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            if (validation != null)
            {
                form.FormClosing += delegate (object sender, FormClosingEventArgs e)
                {
                    if (form.DialogResult == DialogResult.OK)
                    {
                        try
                        {
                            validation(textBox.Text);
                        }
                        catch(System.Exception ex)
                        {
                            e.Cancel = true;
                            MessageBox.Show(form, ex.Message, "验证错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };
            }
            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}
