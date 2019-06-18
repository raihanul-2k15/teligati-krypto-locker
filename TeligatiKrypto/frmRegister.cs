using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Numerics;
using System.IO;
using System.Windows.Forms;

namespace TeligatiKrypto
{
    public partial class frmRegister : Form
    {
        public string Username;
        public BigInteger E;
        public BigInteger D;
        public BigInteger N;

        public frmRegister()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string n = txtName.Text.Trim();
            string u = txtUsername.Text.Trim();
            string p = txtPassword.Text;
            string r = txtRetype.Text;
            Regex uPattern = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");
            if (n.Length < 1 || 
                !uPattern.IsMatch(u) ||
                p.Length < 1 || 
                p != r)
            {
                MessageBox.Show("Please fill out form properly.", "Invalid Form Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Directory.Exists(Config.AppDataFolderPath))
                Directory.CreateDirectory(Config.AppDataFolderPath);
            if (!File.Exists(Config.AppDataFilePath))
            {
                var fs = File.Create(Config.AppDataFilePath);
                fs.Close();
            }
            if (!File.Exists(Path.Combine(Config.AppDataFolderPath, u)))
            {
                var fs = File.Create(Config.GetAppDataUserFile(u));
                fs.Close();
            }
            string[] lines = File.ReadAllLines(Config.AppDataFilePath);
            foreach (string line in lines)
            {
                if (u == line.Split(new char[] { ' ' })[0])
                {
                    MessageBox.Show("This username is already taken. Please use a different username.", "Username already taken", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return; 
                }
            }

            string hp = Util.Hash(p);

            BigInteger ke, kd, kn;
            MyRSA.KeyGen(Config.CryptoKeySize, out ke, out kd, out kn);
            string eStr = Convert.ToBase64String(Util.XOREncDec(ke.ToByteArray()));
            string dStr = Convert.ToBase64String(Util.XOREncDec(kd.ToByteArray()));
            string nStr = Convert.ToBase64String(Util.XOREncDec(kn.ToByteArray()));

            StringBuilder sb = new StringBuilder();
            sb.Append(u);
            sb.Append(" ");
            sb.Append(hp);
            sb.Append(" ");
            sb.Append(eStr);
            sb.Append(" ");
            sb.Append(dStr);
            sb.Append(" ");
            sb.Append(nStr);

            File.AppendAllLines(Config.AppDataFilePath, new string[] { sb.ToString() });

            if (!Directory.Exists(Path.Combine(Config.AppDataFolderPath, u)))
                Directory.CreateDirectory(Path.Combine(Config.AppDataFolderPath, u));

            this.Username = u;
            this.E = ke;
            this.D = kd;
            this.N = kn;
            this.DialogResult = DialogResult.OK;

            MessageBox.Show("Account created successfully. You are now logged in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
