using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PrivacyMonitor
{
    public partial class AlertWindow : Form
    {
        public AlertWindow()
        {
            InitializeComponent();
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Top = Screen.PrimaryScreen.Bounds.Top;
            this.Left = Screen.PrimaryScreen.Bounds.Left;
            timer1.Interval = 1000;
            timer1.Enabled = true;
            lbl_counter.Text = "该窗口将在 " + counter + " 秒后关闭";
        }
        int counter = 3;
        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            lbl_counter.Text = "该窗口将在 " + (counter).ToString() + " 秒后关闭";
            
            if(counter == 0)
            {
                this.Close();
            }
            
        }
    }
}
