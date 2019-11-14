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
    public partial class Form1 : Form
    {
        #region 變數宣告

        //計時器
        DateTime dtNext;

        //資料庫
        string selectCmd, selectCmd2, selectCmd3, selectCmd4, selectCmd5, selectCmd_30;
        SqlConnection conn, conn2, conn3, conn4, conn5, conn_30;
        SqlCommand cmd, cmd2, cmd3, cmd4, cmd_30;
        SqlDataReader reader, reader2, reader3, reader_30;
        string myConnectionString15;
        string myConnectionString30;

        #endregion

        //按鈕事件(設定時間)
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Enabled == true)
            {
                button4.Text = "解鎖";
                textBox1.Enabled = false;
                dtNext = DateTime.Now.AddMinutes(int.Parse(textBox1.Text));
            }
            else
            {
                button4.Text = "設定";
                textBox1.Enabled = true;
            }
        }

        //上方選單事件(執行全部更新)
        private void 全部資料更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllProcessUpdateWorkTime_WithoutT();
        }

        //初始化Form事件(刷新combobox、啟動計時器、設定計時器初始時間30分)
        private void Form1_Load(object sender, EventArgs e)
        {
            dtNext = DateTime.Now.AddMinutes(30);
            ComboBoxRefresh(); //刷新combobox
            timer1.Start();//啟動計時器
        }

        //按鈕事件(刷新combobox)
        private void button2_Click(object sender, EventArgs e)
        {
            ComboBoxRefresh();//刷新combobox
        }

        //按鈕事件(所有工時載入計算)
        private void button3_Click(object sender, EventArgs e)
        {
            AllProcessUpdateWorkTime();
        }

        //計時器(時間為0時執行工時計算，並再次更新時間)
        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan MySpan = dtNext.Subtract(DateTime.Now);
            string diffHour = Convert.ToString(MySpan.Hours);
            string diffMin = Convert.ToString(MySpan.Minutes);
            string diffSec = Convert.ToString(MySpan.Seconds);
            label7.Text = "還有" + diffHour + " 時 " + diffMin + " 分 " + diffSec + " 秒 ";

            //如果倒數時間為0，執行工時計算
            if (Math.Round(MySpan.TotalSeconds)-1 == 0)
            {
                AllProcessUpdateWorkTime();
                dtNext = DateTime.Now.AddMinutes(int.Parse(textBox1.Text));
            }
        }

        //上方選單開啟輸入表單
        private void 新增表單項目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 Form2 = new Form2();
            Form2.ShowDialog();
        }

        //初始化資料庫位置
        public Form1()
        {
            InitializeComponent();

            //資料庫路徑與位子
            myConnectionString15 = "Server=192.168.0.15;database=amsys;uid=sa;pwd=ams.sql;";
            myConnectionString30 = "Server=192.168.0.30;database=AMS2;uid=sa;pwd=Ams.sql;";
        }

        //***所有工時載入計算方法(不含等於T的)***
        private void AllProcessUpdateWorkTime()
        {
            DateTime start_time;
            DateTime end_time;

            //查詢所有登入表單
            conn4 = new SqlConnection(myConnectionString15);
            conn4.Open();
            selectCmd4 = "SELECT [LoginTimeTableName],[WorkTimeTableName] FROM [CaculateWorkTime]";
            cmd = new SqlCommand(selectCmd4, conn4);
            reader3 = cmd.ExecuteReader();
            while (reader3.Read())
            {
                //查詢每個登入時間內時間
                conn = new SqlConnection(myConnectionString15);
                conn.Open();
                selectCmd = "SELECT [OperatorId],[LoginTime], [LogoutTime],[ID]  FROM [" + reader3.GetString(0) + "] WHERE ([IsUpdate] is null or [IsUpdate] ='' or [IsUpdate]<>'T')";
                cmd = new SqlCommand(selectCmd, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    start_time = reader.GetDateTime(1);
                    end_time = reader.GetDateTime(2);
                    TimeSpan worktime = new TimeSpan(end_time.Ticks - start_time.Ticks);

                    //查詢該筆工時紀錄需分攤(計算分母)之數量
                    conn2 = new SqlConnection(myConnectionString15);
                    conn2.Open();
                    selectCmd2 = "SELECT COUNT([Id]) FROM [" + reader3.GetString(1) + "] WHERE [OperatorId] = '" + reader.GetString(0) + "' and [AddTime] >= '" + reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and [AddTime] <= '" + reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    cmd2 = new SqlCommand(selectCmd2, conn2);
                    reader2 = cmd2.ExecuteReader();
                    if (reader2.Read())
                    {
                        if (reader2.GetInt32(0) != 0)
                        {
                            //分攤工時
                            conn3 = new SqlConnection(myConnectionString15);
                            conn3.Open();
                            selectCmd3 = "UPDATE [" + reader3.GetString(1) + "] SET [WorkTime] = '" + ((decimal)(worktime.TotalMinutes) / (decimal)(reader2.GetInt32(0))) + "' WHERE [OperatorId] = '" + reader.GetString(0) + "' and [AddTime] >= '" + reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and [AddTime] <= '" + reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            cmd3 = new SqlCommand(selectCmd3, conn3);
                            cmd3.ExecuteNonQuery();
                            conn3.Close();
                        }
                    }
                    reader2.Close();
                    conn2.Close();

                    //不管有沒有紀錄，每筆執行過後加一個標記，之後不再查詢此筆(減輕負擔)
                    conn5 = new SqlConnection(myConnectionString15);
                    conn5.Open();
                    selectCmd5 = "UPDATE  [" + reader3.GetString(0) + "] SET [IsUpdate] = 'T' WHERE [ID] = '" + reader.GetInt64(3) + "'";
                    cmd4 = new SqlCommand(selectCmd5, conn5);
                    cmd4.ExecuteNonQuery();
                    conn5.Close();
                }
                reader.Close();
                conn.Close();
            }
            reader3.Close();
            conn4.Close();
            toolStripStatusLabel2.Text = DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss");
        }

        //***所有工時載入計算方法(意義上的全部連資料為T都更新)***
        private void AllProcessUpdateWorkTime_WithoutT()
        {
            DateTime start_time;
            DateTime end_time;

            //查詢所有登入表單
            conn4 = new SqlConnection(myConnectionString15);
            conn4.Open();
            selectCmd4 = "SELECT [LoginTimeTableName],[WorkTimeTableName] FROM [CaculateWorkTime]";
            cmd = new SqlCommand(selectCmd4, conn4);
            reader3 = cmd.ExecuteReader();
            while (reader3.Read())
            {
                //查詢每個登入時間內時間
                conn = new SqlConnection(myConnectionString15);
                conn.Open();
                selectCmd = "SELECT [OperatorId],[LoginTime], [LogoutTime],[ID]  FROM [" + reader3.GetString(0) + "]";
                cmd = new SqlCommand(selectCmd, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    start_time = reader.GetDateTime(1);
                    end_time = reader.GetDateTime(2);
                    TimeSpan worktime = new TimeSpan(end_time.Ticks - start_time.Ticks);

                    //查詢該筆工時紀錄需分攤(計算分母)之數量
                    conn2 = new SqlConnection(myConnectionString15);
                    conn2.Open();
                    selectCmd2 = "SELECT COUNT([Id]) FROM [" + reader3.GetString(1) + "] WHERE [OperatorId] = '" + reader.GetString(0) + "' and [AddTime] >= '" + reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and [AddTime] <= '" + reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    cmd2 = new SqlCommand(selectCmd2, conn2);
                    reader2 = cmd2.ExecuteReader();
                    if (reader2.Read())
                    {
                        if (reader2.GetInt32(0) != 0)
                        {
                            //分攤工時
                            conn3 = new SqlConnection(myConnectionString15);
                            conn3.Open();
                            selectCmd3 = "UPDATE [" + reader3.GetString(1) + "] SET [WorkTime] = '" + ((decimal)(worktime.TotalMinutes) / (decimal)(reader2.GetInt32(0))) + "' WHERE [OperatorId] = '" + reader.GetString(0) + "' and [AddTime] >= '" + reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and [AddTime] <= '" + reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            cmd3 = new SqlCommand(selectCmd3, conn3);
                            cmd3.ExecuteNonQuery();
                            conn3.Close();
                        }
                    }
                    reader2.Close();
                    conn2.Close();

                    //不管有沒有紀錄，每筆執行過後加一個標記，之後不再查詢此筆(減輕負擔)
                    conn5 = new SqlConnection(myConnectionString15);
                    conn5.Open();
                    selectCmd5 = "UPDATE  [" + reader3.GetString(0) + "] SET [IsUpdate] = 'T' WHERE [ID] = '" + reader.GetInt64(3) + "'";
                    cmd4 = new SqlCommand(selectCmd5, conn5);
                    cmd4.ExecuteNonQuery();
                    conn5.Close();
                }
                reader.Close();
                conn.Close();
            }
            reader3.Close();
            conn4.Close();
            MessageBox.Show("所有工時更新完成!");
        }

        //按鈕事件計算工時
        private void button1_Click(object sender, EventArgs e)
        {
            if (label5.Text == "NULL" || label6.Text == "NULL" || comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("請確實選擇站別，身為MIS不用我在說了吧?");
            }
            else
            {
                CaculateWorkTime(label5.Text, label6.Text);//計算工時
            }
            ComboBoxRefresh();//刷新combobox
        }

        //***個別工時載入計算方法(不含等於T的)***
        private void CaculateWorkTime(string LoginTimeTableName, string WorkTimeTableName)
        {
            DateTime start_time;
            DateTime end_time;

            //查詢每一筆時間
            conn = new SqlConnection(myConnectionString15);
            conn.Open();
            selectCmd = "SELECT [OperatorId],[LoginTime], [LogoutTime],[ID]  FROM [" + LoginTimeTableName + "] WHERE ([IsUpdate] is null or [IsUpdate] ='' or [IsUpdate]<>'T')";
            cmd = new SqlCommand(selectCmd, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                start_time = reader.GetDateTime(1);
                end_time = reader.GetDateTime(2);
                TimeSpan worktime = new TimeSpan(end_time.Ticks - start_time.Ticks);

                //查詢該筆工時紀錄需分攤(計算分母)之數量
                conn2 = new SqlConnection(myConnectionString15);
                conn2.Open();
                selectCmd2 = "SELECT COUNT([Id]) FROM [" + WorkTimeTableName + "] WHERE [OperatorId] = '" + reader.GetString(0) + "' and [AddTime] >= '" + reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and [AddTime] <= '" + reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                cmd2 = new SqlCommand(selectCmd2, conn2);
                reader2 = cmd2.ExecuteReader();
                if (reader2.Read())
                {
                    if (reader2.GetInt32(0) != 0)
                    {
                        //分攤工時
                        conn3 = new SqlConnection(myConnectionString15);
                        conn3.Open();
                        selectCmd3 = "UPDATE [" + WorkTimeTableName + "] SET [WorkTime] = '" + ((decimal)(worktime.TotalMinutes) / (decimal)(reader2.GetInt32(0))) + "' WHERE [OperatorId] = '" + reader.GetString(0) + "' and [AddTime] >= '" + reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and [AddTime] <= '" + reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                        cmd3 = new SqlCommand(selectCmd3, conn3);
                        cmd3.ExecuteNonQuery();
                        conn3.Close();
                    }
                }
                reader2.Close();
                conn2.Close();

                //不管有沒有紀錄，每筆執行過後加一個標記，之後不再查詢此筆(減輕負擔)
                conn5 = new SqlConnection(myConnectionString15);
                conn5.Open();
                selectCmd5 = "UPDATE  [" + LoginTimeTableName + "] SET [IsUpdate] = 'T' WHERE [ID] = '" + reader.GetInt64(3) + "'";
                cmd4 = new SqlCommand(selectCmd5, conn5);
                cmd4.ExecuteNonQuery();
                conn5.Close();
            }
            reader.Close();
            conn.Close();
            toolStripStatusLabel2.Text = DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss");
        }

        //***刷新讀取combobox包含內容方法***
        private void ComboBoxRefresh()
        {
            comboBox1.Items.Clear();
            //查詢每一筆
            conn = new SqlConnection(myConnectionString15);
            conn.Open();
            selectCmd = "SELECT [StationName],[LoginTimeTableName],[WorkTimeTableName] FROM [CaculateWorkTime]";
            cmd = new SqlCommand(selectCmd, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                comboBox1.Items.Add(reader.GetString(0));
            }
            reader.Close();
            conn.Close();
        }

        //站別更換時查詢資料庫更新站別名稱 提供使用者確認錯誤
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //顯示站別
            label2.Text = comboBox1.SelectedItem.ToString();
            conn = new SqlConnection(myConnectionString15);
            conn.Open();
            selectCmd = "SELECT [LoginTimeTableName],[WorkTimeTableName] FROM [CaculateWorkTime] WHERE [StationName] = '" + comboBox1.SelectedItem.ToString() + "'";
            cmd = new SqlCommand(selectCmd, conn);
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                label5.Text = reader.GetString(0);
                label6.Text = reader.GetString(1);
            }
            reader.Close();
            conn.Close();
        }
    }
}
