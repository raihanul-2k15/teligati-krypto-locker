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
    public partial class frmMain : Form
    {
        private string username;
        private BigInteger e, d, n;
        private byte[] currentFileContents;
        private string currentFilePath;
        private BindingList<Tuple<string, string>> lockedFiles = new BindingList<Tuple<string, string>>();

        public frmMain()
        {
            InitializeComponent();
            lstLockedFiles.DataSource = lockedFiles;
            lstLockedFiles.DisplayMember = "Item1";
        }

        private void logInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ld = new frmLogin();
            if (ld.ShowDialog() == DialogResult.OK)
                this.logInUser(ld.Username, ld.E, ld.D, ld.N);
        }

        private void registerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rd = new frmRegister();
            if (rd.ShowDialog() == DialogResult.OK)
                this.logInUser(rd.Username, rd.E, rd.D, rd.N);

        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.logOutUser();
        }

        private void lstLockedFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstLockedFiles.SelectedIndex == -1) return;

            string encFile = Path.Combine(Config.AppDataFolderPath,
                username,
                (lstLockedFiles.SelectedItem as Tuple<string, string>).Item1);

            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            btnOpen.Enabled = false;
            lstLockedFiles.Enabled = false;
            stsStatus.Text = "Opening...";
            stsProgress.Value = 0;
            stsProgress.Style = ProgressBarStyle.Marquee;

            bgwFileOpener.RunWorkerAsync(new string[] { encFile, "encrypted file" } as object);
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            lstLockedFiles.Enabled = false;
            lstLockedFiles.ClearSelected();
            btnUnlock.Enabled = false;
            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            stsStatus.Text = "Locking...";

            bgwCrypto.RunWorkerAsync(true);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select file";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.InitialDirectory = Environment.CurrentDirectory;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            
            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            btnOpen.Enabled = false;
            lstLockedFiles.Enabled = false;
            stsStatus.Text = "Opening...";
            stsProgress.Value = 0;
            stsProgress.Style = ProgressBarStyle.Marquee;

            bgwFileOpener.RunWorkerAsync(new string[] { ofd.FileName, "plain file" } as object);
        }

        private void bgwFileOpener_DoWork(object sender, DoWorkEventArgs e)
        {
            string filename = ((string[])e.Argument)[0];
            string plainOrEncrypted = ((string[])e.Argument)[1];
            currentFileContents = File.ReadAllBytes(filename);
            currentFilePath = filename;
            e.Result = plainOrEncrypted as object;
        }

        private void bgwFileOpener_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            bool plainFile = (e.Result as string) == "plain file" ? true : false;
            btnLock.Enabled = plainFile;
            btnUnlock.Enabled = !plainFile;
            btnOpen.Enabled = true;
            lstLockedFiles.Enabled = true;
            if (plainFile)
                lstLockedFiles.ClearSelected();
            stsStatus.Text = "Ready";
            stsProgress.Value = 0;
            stsProgress.Style = ProgressBarStyle.Continuous;
            
            if (currentFileContents.Length <= 65530)
            {
                StringBuilder sb = new StringBuilder(currentFileContents.Length);
                foreach (byte b in currentFileContents)
                {
                    if (b == 0)
                        sb.Append("�");
                    else
                        sb.Append((char)b);
                }
                txtPreview.Text = sb.ToString();
            }
            else
                txtPreview.Text = "File size too big. Cannot preview";
        }

        private void bgwCrypto_DoWork(object sender, DoWorkEventArgs eventArgs)
        {
            bool encTask = (bool)eventArgs.Argument;
            
            if (encTask) // encryption
            {
                List<byte> bytes = new List<byte>();
                int block_size = (n.BitLength() - 1) / 8;
                int nIter = currentFileContents.Length / block_size;
                byte[] subArr = new byte[block_size];
                for (int i=0; i < nIter; i++)
                {
                    Buffer.BlockCopy(currentFileContents, i * block_size, subArr, 0, block_size);
                    BigInteger m = Util.ArrToUnsignedBigInt(ref subArr);
                    BigInteger c = MyRSA.Encrypt(m, e, n);
                    byte[] cip = Util.BigIntToFixedLenArr(ref c, block_size + 1);
                    bytes.AddRange(cip);
                    (sender as BackgroundWorker).ReportProgress(100 * i / nIter);
                }
                int rem = currentFileContents.Length % block_size;
                if (rem > 0)
                {
                    subArr = new byte[rem];
                    Buffer.BlockCopy(currentFileContents, nIter * block_size, subArr, 0, rem);
                    BigInteger m = Util.ArrToUnsignedBigInt(ref subArr);
                    BigInteger c = MyRSA.Encrypt(m, e, n);
                    byte[] cip = Util.BigIntToFixedLenArr(ref c, block_size + 1);
                    bytes.AddRange(cip);
                }
                bytes.AddRange(Encoding.UTF8.GetBytes(new BigInteger(rem).ToString()));
                currentFileContents = bytes.ToArray();
                (sender as BackgroundWorker).ReportProgress(100);
            }
            else // decrypt
            {
                List<byte> bytes = new List<byte>();
                int block_size = (n.BitLength() - 1) / 8 + 1;
                int nIter = currentFileContents.Length / block_size;
                byte[] subArr = new byte[block_size];
                for (int i = 0; i < nIter - 1; i++)
                {
                    Buffer.BlockCopy(currentFileContents, i * block_size, subArr, 0, block_size);
                    BigInteger c = Util.ArrToUnsignedBigInt(ref subArr);
                    BigInteger m = MyRSA.Decrypt(ref c, ref d, ref n);
                    byte[] msg = Util.BigIntToFixedLenArr(ref m, block_size - 1);
                    bytes.AddRange(msg);
                    (sender as BackgroundWorker).ReportProgress(100 * i / nIter);
                }
                {
                    int rem = currentFileContents.Length % block_size;
                    subArr = new byte[rem];
                    Buffer.BlockCopy(currentFileContents, nIter * block_size, subArr, 0, rem);
                    int lastIterByteCount = (int)BigInteger.Parse(Encoding.UTF8.GetString(subArr));
                    subArr = new byte[block_size];
                    Buffer.BlockCopy(currentFileContents, (nIter - 1) * block_size, subArr, 0, block_size);
                    BigInteger c = Util.ArrToUnsignedBigInt(ref subArr);
                    BigInteger m = MyRSA.Decrypt(ref c, ref d, ref n);
                    byte[] msg = Util.BigIntToFixedLenArr(ref m, lastIterByteCount);
                    bytes.AddRange(msg);
                }

                currentFileContents = bytes.ToArray();
                (sender as BackgroundWorker).ReportProgress(100);
            }

            eventArgs.Result = encTask;
        }

        private void bgwCrypto_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            stsProgress.Value = e.ProgressPercentage;
        }

        private void bgwCrypto_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool wasEncTask = (bool)e.Result;
            lstLockedFiles.Enabled = true;
            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            btnOpen.Enabled = true;
            stsStatus.Text = "Ready";
            stsProgress.Value = 0;

            if (wasEncTask)
            {
                string filename = Path.GetFileName(currentFilePath);

                string encFile = Path.Combine(Config.AppDataFolderPath,
                    username,
                    filename);
                File.WriteAllBytes(encFile, currentFileContents);
                File.Delete(currentFilePath);

                string safeFileName = filename.Replace(" ", "%20");
                string safeFilePath = currentFilePath.Replace(" ", "%20");
                string line = safeFileName + " " + safeFilePath;
                File.AppendAllLines(Config.GetAppDataUserFile(username), new string[] { line });
                lockedFiles.Add(new Tuple<string, string>(filename, currentFilePath));
                lstLockedFiles.SetSelected(lockedFiles.Count - 1, true);
            }
            else
            {
                string saveFile = (lstLockedFiles.SelectedItem as Tuple<string, string>).Item2;
                File.WriteAllBytes(saveFile, currentFileContents);
                File.Delete(Path.Combine(Config.AppDataFolderPath,
                    username,
                    (lstLockedFiles.SelectedItem as Tuple<string, string>).Item1));
                txtPreview.Text = "";
                lockedFiles.RemoveAt(lstLockedFiles.SelectedIndex);
                string[] lines = new string[lockedFiles.Count];
                for (int i=0; i< lockedFiles.Count; i++)
                {
                    string safeFile = lockedFiles[i].Item1.Replace(" ", "%20");
                    string safePath = lockedFiles[i].Item2.Replace(" ", "%20");
                    lines[i] = safeFile + " " + safePath;
                }
                File.WriteAllLines(Config.GetAppDataUserFile(username), lines);
                lstLockedFiles.ClearSelected();

            }
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            lstLockedFiles.Enabled = false;
            btnUnlock.Enabled = false;
            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            stsStatus.Text = "Unlocking...";

            bgwCrypto.RunWorkerAsync(false);
        }

        private void aboutTeligatiKryptoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout abtD = new frmAbout();
            abtD.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void logInUser(string username, BigInteger e, BigInteger d, BigInteger n)
        {
            this.username = username;
            this.e = e;
            this.d = d;
            this.n = n;
            string[] lines = File.ReadAllLines(Config.GetAppDataUserFile(username));
            foreach (string line in lines)
            {
                string[] parts = line.Split(new char[] { ' ' });
                parts[0] = parts[0].Replace("%20", " ");
                parts[1] = parts[1].Replace("%20", " ");
                lockedFiles.Add(new Tuple<string, string>(parts[0], parts[1]));
            }
            stsStatus.Text = "Ready";
            stsUser.Text = username;
            logInToolStripMenuItem.Enabled = false;
            logOutToolStripMenuItem.Enabled = true;
            registerToolStripMenuItem.Enabled = false;
            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            btnOpen.Enabled = true;
            lstLockedFiles.Enabled = true;
            lstLockedFiles.ClearSelected();
        }

        private void logOutUser()
        {
            this.username = null;
            this.e = 0;
            this.d = 0;
            this.n = 0;
            currentFileContents = null;
            currentFilePath = null;
            stsStatus.Text = "Ready";
            stsUser.Text = "Please Log In";
            logInToolStripMenuItem.Enabled = true;
            logOutToolStripMenuItem.Enabled = false;
            registerToolStripMenuItem.Enabled = true;
            btnLock.Enabled = false;
            btnUnlock.Enabled = false;
            btnOpen.Enabled = false;
            lockedFiles.Clear();
            lstLockedFiles.ClearSelected();
            lstLockedFiles.Enabled = false;
            txtPreview.Text = "";
        }
    }
}
