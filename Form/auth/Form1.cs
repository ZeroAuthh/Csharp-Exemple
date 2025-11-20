using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace auth
{
    public partial class Form1 : Form
    {

        public static ZeroAUTH ZeroAuth = new ZeroAUTH(
            Application: "teste",  // AppID
            OwnerID: "lczxydll-akv5u99c"   // Database
        );



        public Form1()
        {
            InitializeComponent();
        }

        // Verifica o status do AppID assim que o Formulário é carregado
        private async void Form1_Load(object sender, EventArgs e)
        {
            await ZeroAuth.Init();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string key = textBox1.Text; 

            var response = await ZeroAuth.LoginWithKey(key);

            if (response.Success)
            {
                string expirationText = await ZeroAuth.GetExpiration(key, "Expiry in {d} Days {h} Hours {m} Minutes", isKey: true);
                Form3.ExpirationText = expirationText;

                Form3.Keytext = key;

                Form3 nextForm = new Form3();
                nextForm.Show();

                this.Hide();
            }
            else
            {
                MessageBox.Show(response.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async void button2_Click(object sender, EventArgs e)
        {
            string username = textBox2.Text;
            string password = textBox3.Text;

            var response = await ZeroAuth.LoginWithUser(username, password);

            if (response.Success)
            {
                string expirationText = await ZeroAuth.GetExpiration(username, "Expiry in {d} Days {h} Hours {m} Minutes", isKey: false);
                Form2.ExpirationText = expirationText;

                Form2.Usertext = username;

                Form2 nextForm = new Form2();
                nextForm.Show();

                this.Hide();
            }
            else
            {
                MessageBox.Show(response.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string username = textBox5.Text;
            string password = textBox4.Text;
            string key = textBox6.Text;

            var response = await ZeroAuth.RegisterUserWithKey(username, password, key);

            if (response.Success)
            {
            }
            else
            {
                MessageBox.Show(response.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
