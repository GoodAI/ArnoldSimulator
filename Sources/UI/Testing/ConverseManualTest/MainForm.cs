using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConverseManualTest
{
    public partial class MainForm : Form
    {
        private ChatClient m_chat;

        public MainForm()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (m_chat == null)
            {
                try
                {
                    m_chat = new ChatClient("localhost", int.Parse(portTextBox.Text));
                }
                catch (Exception ex)
                {
                    logListBox.Items.Add($"Init error: {ex.Message}");
                }

                portTextBox.Enabled = false;  // Change of port after init is not supported.
            }

            logListBox.Items.Add($"Sending: '{messageText.Text}'");

            string reply = m_chat.SendQuery(messageText.Text);

            logListBox.Items.Add($"Reply: '{reply}'");
        }
    }
}
