using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace OJT___QR_Code_Generator
{
    public partial class Main_Menu : Form
    {
        public Main_Menu()
        {
            InitializeComponent();
        }
        private void OpenChildForm(Form childForm)
        {
            this.Hide();
            childForm.FormClosed += (s, e) => this.Show();
            childForm.Show();
        }
        private void Main_Menu_Load(object sender, EventArgs e)
        {

        }

        private void btnBinLocation_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Form1());
        }

        private void btnNamingPart_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Naming_Part_From());
        }

        private void btnFloorBin_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Floor_Bin());
        }

        private void btnFinishedGoods_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Finished_Goods());
        }

        private void btnForm5_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Form_5());
        }
    }
}
