using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceNotifier
{
    internal class DeviceNotifierApplicationContext : ApplicationContext
    {
        private readonly Container _components;
        private readonly NotifyIcon _notifyIcon;

        public DeviceNotifierApplicationContext()
        {
            _components = new Container();
            _notifyIcon = new NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = SystemIcons.Application, // TODO: Fix
                Text = @"Device Notifications",
                Visible = true
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStripOnOpening;
            _notifyIcon.DoubleClick += (sender, args) => _notifyIcon.ContextMenuStrip.Show();

            var timer = new Timer(_components);
            var messagesForm = new MessagesForm();
            _components.Add(messagesForm);
            var mainForm = new MainForm(_notifyIcon, messagesForm, timer);
            _components.Add(mainForm);
            MainForm = mainForm;
        }

        private void ContextMenuStripOnOpening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
            _notifyIcon.ContextMenuStrip.Items.Add("E&xit", null, OnExitClick);
        }

        private void OnExitClick(object sender, EventArgs eventArgs)
        {
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _components != null)
            {
                _components.Dispose();
            }
        }

        protected override void ExitThreadCore()
        {
            _notifyIcon.Visible = false;
            base.ExitThreadCore();
        }
    }
}