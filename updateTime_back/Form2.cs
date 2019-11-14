using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace updateTime_back
{
    public partial class Form2 : Form
    {
        #region 變數宣告

        //資料庫
        string selectCmd, selectCmd2, selectCmd3, selectCmd_30;
        SqlConnection conn, conn2, conn3, conn_30;
        SqlCommand cmd, cmd2, cmd3, cmd_30;
        SqlDataReader reader, reader2, reader3, reader_30;

        //資料庫
        string myConnectionString15;
        string myConnectionString30;

        #endregion  

        public Form2()
        {
            InitializeComponent();

            //資料庫路徑與位子
            myConnectionString15 = "Server=192.168.0.15;database=amsys;uid=sa;pwd=ams.sql;";
            myConnectionString30 = "Server=192.168.0.30;database=AMS2;uid=sa;pwd=Ams.sql;";
        }

        //Form2 load事件(載入datagridview資料)
        private void Form2_Load(object sender, EventArgs e)
        {
            RefreshDatagridview1();
        }

        //新增資料至表單做更新
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("你有一個輸入框沒有值");
            }
            else
            {
                conn = new SqlConnection(myConnectionString15);
                conn.Open();
                selectCmd = "IF EXISTS(SELECT * FROM [CaculateWorkTime] WHERE [StationName] = '" + textBox1.Text + "') UPDATE [CaculateWorkTime] SET [LoginTimeTableName] = '" + textBox2.Text + "',[WorkTimeTableName] = '" + textBox3.Text + "' WHERE [StationName] = '" + textBox1.Text + "' ELSE INSERT INTO [CaculateWorkTime]([StationName], [LoginTimeTableName], [WorkTimeTableName]) VALUES('" + textBox1.Text + "', '" + textBox2.Text + "','" + textBox3.Text + "');";
                cmd = new SqlCommand(selectCmd, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            //刷新datagridview
            RefreshDatagridview1();
        }

        //***刷新datagridview1方法(表單)***
        private void RefreshDatagridview1()
        {
            //初始化
            dataGridView1.Rows.Clear();

            //查詢每一筆時間
            conn = new SqlConnection(myConnectionString15);
            conn.Open();
            selectCmd = "SELECT  [Id],[StationName],[LoginTimeTableName],[WorkTimeTableName] FROM [CaculateWorkTime]";
            cmd = new SqlCommand(selectCmd, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dataGridView1.Rows.Add(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
            }
            conn.Close();
            reader.Close();
        }
    }
}
