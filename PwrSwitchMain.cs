using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;

namespace PwrSwitch
{
    class PwrSwitchMain : Form
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new PwrSwitchMain());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private List<PowerPlan> pwrNames;
        public const string version = "v1.0";

        // Main init
        public PwrSwitchMain()
        {
            RegenTrayMenu();

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Power Switcher";
            trayIcon.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("PwrSwitch.power.ico"));

            trayIcon.ContextMenu = trayMenu;
            trayIcon.MouseClick += MouseClicked;
            trayIcon.Visible = true;
        }

        protected void RegenTrayMenu()
        {
            pwrNames = PwrList.getPlansList();
            PowerPlan active = PwrList.getCurrActivePlan();

            trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Power Plans:").Enabled = false;

            MenuItem curr;

            foreach (PowerPlan p in pwrNames)
            {
                curr = trayMenu.MenuItems.Add(p.name, ChangePlan);
                if (p.name == active.name)
                {
                    curr.Checked = true;
                } else
                {
                    
                }
            }

            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Refresh Plans", OnRefresh);
            trayMenu.MenuItems.Add("Open Control panel", OpenCtrlPanel);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("About", ShowAbout);
            trayMenu.MenuItems.Add("Exit", OnExit);
        }

        // Util functions
        protected void ClearChecked()
        {
            foreach (MenuItem mi in trayMenu.MenuItems)
            {
                mi.Checked = false;
            }
        }

        // TRIGGERED
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void MouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(trayIcon, null);
            }
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show("PwrSwitch " + version +
                " by ohnx\nPwrSwitch is a simple program that lets you toggle your power settings from the taskbar.\nQuestions? Problems? Suggestions? Please feel free to send me an email (me@masonx.ca).\nThanks for using PwrSwitch!",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OpenCtrlPanel(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("powercfg.cpl");
        }


        private void ChangePlan(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            // find the plan
            foreach (PowerPlan p in pwrNames)
            {
                if (mi.Text == p.name)
                {
                    // found the plan! now set it
                    if (PwrList.setPlan(p))
                    {
                        ClearChecked();
                        mi.Checked = true;
                    }
                    else
                    {
                        Console.WriteLine("Plan change failed.");
                    }
                    break;
                }
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            trayMenu.Dispose();
            RegenTrayMenu();
            trayIcon.ContextMenu = trayMenu;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                trayMenu.Dispose();
                trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
