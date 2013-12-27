using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
// using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;

namespace DeviceNotifier
{
    // ReSharper disable once RedundantNameQualifier
   // [System.ComponentModel.DesignerCategory("")]
    internal class MessagesForm : Form
    {
        private IContainer components;


        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._lstMessages = new DeviceNotifier.ReadOnlyListBox();
            this._closeTimer = new System.Windows.Forms.Timer(this.components);
            this._fadeTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // _lstMessages
            // 
            this._lstMessages.BackColor = System.Drawing.SystemColors.Control;
            this._lstMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._lstMessages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lstMessages.ItemHeight = 15;
            this._lstMessages.Location = new System.Drawing.Point(12, 12);
            this._lstMessages.Name = "_lstMessages";
            this._lstMessages.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this._lstMessages.Size = new System.Drawing.Size(120, 90);
            this._lstMessages.TabIndex = 0;
            this._lstMessages.TabStop = false;
            this._lstMessages.UseTabStops = false;
            // 
            // _closeTimer
            // 
            this._closeTimer.Interval = 5000;
            this._closeTimer.Tick += new System.EventHandler(this._closeTimer_Tick);
            // 
            // _fadeTimer
            // 
            this._fadeTimer.Interval = 33;
            this._fadeTimer.Tick += new System.EventHandler(this._fadeTimer_Tick);
            // 
            // MessagesForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 269);
            this.Controls.Add(this._lstMessages);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MessagesForm";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ResumeLayout(false);

        }
    
        // ReSharper disable once InconsistentNaming
        private const int CS_DROPSHADOW = 0x00020000;
        private ReadOnlyListBox _lstMessages;
        private Timer _closeTimer;
        private Timer _fadeTimer;
        private readonly StringWriter _messages = new StringWriter();
        private int _fadeStep;
        private int _totalSteps;
        private const int FADE_TIME = 1000;

        public MessagesForm()
        {
            InitializeComponent();
        }

        // ReSharper disable once InconsistentNaming

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
        
        public void AddMessage(string message)
        {
            // check if this message is already present
            if (_lstMessages.Items.Count > 0 && _lstMessages.Items.Cast<string>().Last() == message)
                return; // No op

            _closeTimer.Stop();

            if (Visible)
            {
                _fadeTimer.Stop();
                Opacity = 1.0;
            }
            else
            {
                Show();
            }

            _messages.WriteLine(message);

            _lstMessages.Items.Add(message);
            ResizeToFit(_lstMessages, message);
            Reposition();

            _closeTimer.Start();
        }

        private void ResizeToFit(ListBox list, string message)
        {
            using (var gfx = list.CreateGraphics())
            {
                var size = gfx.MeasureString(message, list.Font);
                var w = (int)Math.Ceiling(size.Width);
                list.Width = Math.Max(list.Width, w);
                list.Height = list.PreferredHeight;
            }

            Width = list.Width + (list.Left * 2);
            Height = list.Height + (list.Top * 2);
        }

        private void Reposition()
        {
            var dw = Screen.PrimaryScreen.WorkingArea;

            Left = dw.Right - (Width + 5);
            Top = dw.Bottom - (Height + 5);
        }

        private void _closeTimer_Tick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            BeginFade();
        }

        private void BeginFade()
        {
            Debug.Assert(_fadeTimer != null, "_fadeTimer != null");
            Opacity = 1.0;
            // ReSharper disable once PossibleLossOfFraction
            _totalSteps = FADE_TIME / _fadeTimer.Interval;
            _fadeStep = _totalSteps;
            _fadeTimer.Start();
        }

        private void _fadeTimer_Tick(object sender, EventArgs e)
        {
            Opacity = _fadeStep / (double)_totalSteps;
            _fadeStep -= 1;

            if (!(Opacity <= 0.0)) return;
            Hide();
            _fadeTimer.Stop();
            // Clear out
            _lstMessages.Items.Clear();
        }
    }
}
