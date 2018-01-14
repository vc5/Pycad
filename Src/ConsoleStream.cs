using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Autodesk.AutoCAD.EditorInput;
using System;

namespace NFox.Pycad
{
    public class ConsoleStream : MemoryStream
    {

        static ConsoleTextBox _textBox;
        static PyIdeForm _form;

        public static void ShowForm()
        {
            _form?.Show();
        }

        public static void HideForm()
        {
            _form?.Hide();
        }

        public static void SetTo(PyIdeForm form, ConsoleTextBox textBox)
        {
            _form = form;
            _textBox = textBox;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string message = Encoding.UTF8.GetString(buffer, offset, count);
            if (_textBox == null)
                Engine.GetAcEditor()?.WriteMessage(message);
            else
                _textBox.Write(message);
        }


        public static string Buffer;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_textBox == null)
            {
                PromptStringOptions opts = new PromptStringOptions("");
                opts.AllowSpaces = true;
                var res = Engine.GetAcEditor()?.GetString(opts);
                Buffer = Environment.NewLine;
                if (res.Status == PromptStatus.OK)
                    Buffer = res.StringResult.Trim() + Buffer;
            }
            else
            {
                _textBox.IsRead = true;
                while (Buffer == null)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
            }
            var buf = Encoding.UTF8.GetBytes(Buffer);
            Buffer = null;
            for (int i = 0; i < buf.Length; i++)
                buffer[i] = buf[i];
            return buf.Length;
        }

    }
}
