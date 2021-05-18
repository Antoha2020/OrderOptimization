using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GmapTest
{
    public partial class ManageMasters : Form
    {
        public ManageMasters()
        {
            InitializeComponent();
        }
       
        private void ManageMasters_Load(object sender, EventArgs e)
        {            
            for(int i=0;i<Constants.MASTERS.Count;i++)
            {
                dataGridView1.Rows.Add(i+1, Constants.MASTERS[i].Name, Constants.MASTERS[i].StartLat,
                    Constants.MASTERS[i].StartLon, Constants.MASTERS[i].InWork);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DBHandlerMySQL.AddMaster(textBox1.Text, textBox2.Text, textBox3.Text, checkBox1.Checked);
            RefreshTable();
        }

        private void RefreshTable()
        {
            DBHandlerMySQL.GetMasters();
            dataGridView1.Rows.Clear();
            for (int i = 0; i < Constants.MASTERS.Count; i++)
            {
                dataGridView1.Rows.Add(i + 1, Constants.MASTERS[i].Name, Constants.MASTERS[i].StartLat,
                    Constants.MASTERS[i].StartLon, Constants.MASTERS[i].InWork);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DBHandlerMySQL.UpdateMaster(textBox1.Text, textBox2.Text, textBox3.Text, checkBox1.Checked);
            RefreshTable();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Вы действительно хотите удалить мастера " + textBox1.Text + "?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                DBHandlerMySQL.DeleteMaster(textBox1.Text);
                RefreshTable();
            }
        }
        
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox1.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString();
            textBox2.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();
            textBox3.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[3].Value.ToString();
            checkBox1.Checked = Convert.ToBoolean(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[4].Value) ? true : false;
        }
    }
}
