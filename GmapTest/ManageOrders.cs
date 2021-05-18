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
    public partial class ManageOrders : Form
    {
        public ManageOrders()
        {
            InitializeComponent();
        }

        private void ManageOrders_Load(object sender, EventArgs e)
        {
            comboBox2.Items.Add("Нет");
            for (int i = 0; i < Constants.MASTERS.Count; i++)
            {               
                comboBox2.Items.Add(Constants.MASTERS[i].Name);
            }
            comboBox2.SelectedIndex = 0;

            RefreshTable();
        }
        
        private void RefreshTable()
        {
            DBHandlerMySQL.GetOrders();
            dataGridView1.Rows.Clear();
            for (int i = 0; i < Constants.ORDERS.Count; i++)
            {
                if (dateTimePicker2.Value.ToShortDateString().Equals(Constants.ORDERS[i].DateOrder.ToShortDateString()))
                {
                    dataGridView1.Rows.Add(Constants.ORDERS[i].Id,
                        Constants.ORDERS[i].DateCreate.ToShortDateString(),
                        Constants.ORDERS[i].Name,
                        Constants.ORDERS[i].Phone1,
                        Constants.ORDERS[i].DateOrder.ToShortDateString(),
                        Constants.ORDERS[i].Description,
                        Constants.ORDERS[i].City,
                        Constants.ORDERS[i].Street,
                        Constants.ORDERS[i].House,
                        Constants.ORDERS[i].Flat,
                        Constants.ORDERS[i].Office,
                        Constants.ORDERS[i].Porch,
                        Constants.ORDERS[i].Floor,
                        Constants.ORDERS[i].Intercom,
                        Constants.ORDERS[i].Phone2,
                        Constants.ORDERS[i].TimeBeg,
                        Constants.ORDERS[i].Lat,
                        Constants.ORDERS[i].Lon,
                        Constants.ORDERS[i].Master
                        );
                }
            }
        }
        

        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            textBox12.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value.ToString();
            textBox15.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();
            if (dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[19].Value == null)
                checkBox12.Checked = false;
            else
                checkBox12.Checked = Convert.ToBoolean(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[19].Value.ToString());

            dateTimePicker2.Value = Convert.ToDateTime(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[4].Value);
            maskedTextBox1.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[15].Value.ToString();
            comboBox2.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[18].Value.ToString();
            textBox1.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[6].Value.ToString();
            textBox2.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[7].Value.ToString();
            textBox3.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[8].Value.ToString();
            textBox4.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[10].Value.ToString();
            textBox5.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[9].Value.ToString();
            textBox6.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[11].Value.ToString();
            textBox7.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[12].Value.ToString();

            if (dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[13].Value == null)
                checkBox11.Checked = false;
            else
                checkBox11.Checked = Convert.ToBoolean(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[13].Value.ToString());
            textBox8.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[3].Value.ToString();
            textBox9.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[14].Value.ToString();
            textBox10.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[5].Value.ToString();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                string id = textBox12.Text;
                string name = textBox15.Text;
                bool completed = checkBox12.Checked;
                string dateOrder = dateTimePicker2.Value.ToShortDateString();
                string timeBeg = maskedTextBox1.Text;
                string master = comboBox2.Text;
                string city = textBox1.Text;
                string street = textBox2.Text;
                string house = textBox3.Text;
                string office = textBox4.Text;
                string flat = textBox5.Text;
                string porch = textBox6.Text;
                string floor = textBox7.Text;
                bool intercom = checkBox11.Checked;
                string phone1 = textBox8.Text;
                string phone2 = textBox9.Text;
                string description = textBox10.Text;                                                                

                if (dateTimePicker2.Value.Date < DateTime.Now.Date)
                {
                    MessageBox.Show("Проверьте дату выполнения заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (name.Length == 0)
                {
                    MessageBox.Show("Не указано название заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DBHandlerMySQL.UpdateOrder(id, name, description, city, street, house, flat, office, porch, intercom, floor, dateOrder,
                    timeBeg, phone1, phone2, master, completed);
                RefreshTable();                
                
                MessageBox.Show("Информация о заказе " + name + " успешно обновлена!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (textBox12.Text != "")
            {
                if (MessageBox.Show("Вы действительно хотите удалить заказ " + textBox15.Text + "?", "", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    DBHandlerMySQL.DeleteOrder(textBox12.Text);
                    RefreshTable();                    
                    MessageBox.Show("Заказ " + textBox15.Text + " успешно удален!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    textBox5.Clear();
                    textBox6.Clear();
                    textBox7.Clear();
                    textBox8.Clear();
                    textBox9.Clear();
                    textBox10.Clear();
                    textBox12.Clear();
                    textBox15.Clear();

                    checkBox12.Checked = false;
                    checkBox11.Checked = false;

                    maskedTextBox1.Clear();
                    comboBox2.Text = "Нет";                                        
                }
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            RefreshTable();
        }
    }
}
