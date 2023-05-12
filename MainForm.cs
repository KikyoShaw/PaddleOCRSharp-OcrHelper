using PaddleOCRSharp;
using System.Runtime.InteropServices;

namespace OcrHelper
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndlnsertAfter, int X, int Y, int cx, int cy, int flags);

        OCRStructureResult _ocrResult = new OCRStructureResult();
        PaddleOCREngine _ocrEngine;
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.Red;
            this.TransparencyKey = Color.Red;
            SetWindowPos(this.Handle, new IntPtr(-1),this.Left,this.Top,this.Width,this.Height,1|2);
            OCRModelConfig config = new OCRModelConfig();
            string? root = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string modelPathroot = root + @"\inference";
            config.det_infer = modelPathroot + @"\ch_PP-OCRv3_det_infer";
            config.cls_infer = modelPathroot + @"\ch_ppocr_mobile_v2.0_cls_infer";
            config.rec_infer = modelPathroot + @"\ch_PP-OCRv3_rec_infer";
            config.keys = modelPathroot + @"\ppocr_keys.txt";
            _ocrEngine = new PaddleOCREngine(config, new OCRParameter());
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        public void StartOcr(bool auto = false)
        {
            Image image = new Bitmap(panel_Image.Width, panel_Image.Height);
            Graphics graph = Graphics.FromImage(image);
            graph.CopyFromScreen(new Point(this.Left, this.Top + (this.Height - this.ClientSize.Height)), new Point(0, 0), new Size(panel_Image.Width, panel_Image.Height - (this.Height - this.ClientSize.Height)));
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            _ocrResult = _ocrEngine.DetectStructure(image);
            List<TextBlock> textBlocks = _ocrResult.TextBlocks;
            string result = "";
            foreach (var t in textBlocks)
                result += t.Text + "\r\n";
            this.BeginInvoke(new Action(() =>
            {
                richTextBox_Result.Text = result;
            }));
        }

        private void button_StartOcr_Click(object sender, EventArgs e)
        {
            StartOcr(false);
        }
    }
}