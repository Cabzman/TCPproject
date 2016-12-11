using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace tcpproj
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            string temp = testDurationTextBox.Text;
            int timeDuration = Convert.ToInt32(temp);
            temp = testNrTextBox.Text;
            int testNr = Convert.ToInt32(temp);
            DRIVER.StartTest(testNr, timeDuration);
        }
    }
}
