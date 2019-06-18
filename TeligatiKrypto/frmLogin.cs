using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

namespace TeligatiKrypto
{
    public partial class frmLogin : Form
    {
        public string Username;
        public BigInteger E;
        public BigInteger D;
        public BigInteger N;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string u = txtUsername.Text.Trim();
            string p = txtPassword.Text;
            string hp = Util.Hash(p);

            bool correct = false;
            if (File.Exists(Config.AppDataFilePath))
            {
                string[] lines = File.ReadAllLines(Config.AppDataFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] userData = lines[i].Split(new char[] { ' ' });
                    if (u == userData[0] && hp == userData[1])
                    {
                        this.Username = u;
                        this.E = new BigInteger(Util.XOREncDec(Convert.FromBase64String(userData[2])));
                        this.D = new BigInteger(Util.XOREncDec(Convert.FromBase64String(userData[3])));
                        this.N = new BigInteger(Util.XOREncDec(Convert.FromBase64String(userData[4])));
                        this.DialogResult = DialogResult.OK;
                        correct = true;
                        break;
                    }
                }
            }
            if (!correct)
                MessageBox.Show("Incorrect username or password. Please try again.", "Wrong credentials", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                Close();
        }
    }
}
