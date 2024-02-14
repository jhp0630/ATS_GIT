using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;

namespace ats
{
    public partial class Form1 : Form
    {
        string g_user_id = null;
        string g_accnt_no = null;

        int g_scr_no = 0;

        int g_is_thread = 0;
        Thread thread1 = null;

        int g_flag_1 = 0;
        int g_flag_2 = 0;
        int g_flag_3 = 0;
        int g_flag_4 = 0;
        int g_flag_5 = 0;

        int g_is_next = 0; 

        string g_rqname = null;
        int g_ord_amt_possible = 0;

        public Form1()
        {
            InitializeComponent();
            this.axKHOpenAPI1.OnReceiveTrData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEventHandler(this.axKHOpenAPI1_OnReceiveTrdata);
            this.axKHOpenAPI1.OnReceiveMsg += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEventHandler(this.axKHOpenAPI1_OnReceiveMsg);
            this.axKHOpenAPI1.OnReceiveChejanData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEventHandler(this.axKHOpenAPI1_OnReceiveChejanData);
        }

        private void axKHOpenAPI1_OnReceiveTrdata(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (g_rqname.CompareTo(e.sRQName) == 0)
            {
                ;
            }
            else
            {
                write_err_log("요청한 TR  : [" + g_rqname + "]\n", 0);
                write_err_log("응답받은 TR  : [" + e.sRQName + "]\n", 0);

                switch(g_rqname)
                {
                    case "증거금세부내역조회요청":
                        g_flag_1 = 1;
                        break;

                    case "계좌평가현황요청":
                        g_flag_2 = 1;
                        break;

                    default: break;
                }
                return;
            }

            if(e.sRQName == "증거금세부내역조회요청")
            {
                g_ord_amt_possible = int.Parse(axKHOpenAPI1.CommGetData(e.sTrCode, "", e.sRQName, 0, "100주문가능금액").Trim());
                axKHOpenAPI1.DisconnectRealData(e.sScrNo);
                g_flag_1 = 1;
            }
            if(e.sRQName == "계좌평가현황요청")
            {
                int repeat_cnt = 0;
                int ii = 0;
                String user_id = null;
                String jongmok_cd = null;
                String jongmok_nm = null;
                int own_stock_cnt = 0;
                int buy_price = 0;
                int own_amt = 0;

                repeat_cnt = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                write_msg_log("TB_ACCNT_INFO 테이블 설정 시작\n", 0);
                write_msg_log("보유종목수 : " + repeat_cnt.ToString() + "\n", 0);

                for(ii=0; ii<repeat_cnt; ii++)
                {
                    user_id = "";
                    jongmok_cd = "";
                    own_stock_cnt = 0;
                    buy_price = 0;
                    own_amt = 0;

                    user_id = g_user_id;
                    jongmok_cd = axKHOpenAPI1.CommGetData(e.sTrCode, "", e.sRQName, ii, "종목코드").Trim().Substring(1, 6);
                    jongmok_nm = axKHOpenAPI1.CommGetData(e.sTrCode, "", e.sRQName, ii, "종목명").Trim();
                    own_stock_cnt = int.Parse(axKHOpenAPI1.CommGetData(e.sTrCode, "", e.sRQName, ii, "보유수량").Trim());
                    buy_price = int.Parse(axKHOpenAPI1.CommGetData(e.sTrCode, "", e.sRQName, ii, "평균단가").Trim());
                    own_amt = int.Parse(axKHOpenAPI1.CommGetData(e.sTrCode, "", e.sRQName, ii, "매입금액").Trim());

                    write_msg_log("종목코드 : " + jongmok_cd + "\n", 0);
                    write_msg_log("종목명 : " + jongmok_nm + "\n", 0);
                    write_msg_log("보유주식수 : " + own_stock_cnt.ToString() + "\n", 0);

                    if(own_stock_cnt == 0)
                    {
                        continue;
                    }

                    insert_tb_accnt_info(jongmok_cd, jongmok_nm, buy_price, own_stock_cnt, own_amt);

                }

                write_msg_log("TB_ACCNT_INFO 테이블 설정 완료\n", 0);
                axKHOpenAPI1.DisconnectRealData(e.sScrNo);

                if(e.sPrevNext.Length == 0)
                {
                    g_is_next = 0;
                }
                else
                {
                    g_is_next = int.Parse(e.sPrevNext);
                }
                g_flag_2 = 1;
            }


        }
        private void axKHOpenAPI1_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            if(e.sRQName == "매수주문")
            {
                write_msg_log("\n========매수주문 원장 응답정보 출력 시작========\n", 0);
                write_msg_log("sScrNo : [" + e.sScrNo + "]" + "\n", 0);
                write_msg_log("sRQName : [" + e.sRQName + "]" + "\n", 0);
                write_msg_log("sTrCode : [" + e.sTrCode + "]" + "\n", 0);
                write_msg_log("sMsg : [" + e.sMsg + "]" + "\n", 0);
                write_msg_log("========매수주문 원장 응답정보 출력 종료========\n", 0);
                g_flag_3 = 1;
            }

            if (e.sRQName == "매도주문")
            {
                write_msg_log("\n========매도주문 원장 응답정보 출력 시작========\n", 0);
                write_msg_log("sScrNo : [" + e.sScrNo + "]" + "\n", 0);
                write_msg_log("sRQName : [" + e.sRQName + "]" + "\n", 0);
                write_msg_log("sTrCode : [" + e.sTrCode + "]" + "\n", 0);
                write_msg_log("sMsg : [" + e.sMsg + "]" + "\n", 0);
                write_msg_log("========매도주문 원장 응답정보 출력 종료========\n", 0);
                g_flag_4 = 1;
            }

            if (e.sRQName == "매도취소주문")
            {
                write_msg_log("\n========매도취소주문 원장 응답정보 출력 시작========\n", 0);
                write_msg_log("sScrNo : [" + e.sScrNo + "]" + "\n", 0);
                write_msg_log("sRQName : [" + e.sRQName + "]" + "\n", 0);
                write_msg_log("sTrCode : [" + e.sTrCode + "]" + "\n", 0);
                write_msg_log("sMsg : [" + e.sMsg + "]" + "\n", 0);
                write_msg_log("========매도취소주문 원장 응답정보 출력 종료========\n", 0);
                g_flag_5 = 1;
            }
        }
        private void axKHOpenAPI1_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if(e.sGubun == "0")
            {
                String chejan_gb = "";
                chejan_gb = axKHOpenAPI1.GetChejanData(913).Trim();

                if(chejan_gb == "접수")
                {
                    String user_id = null;
                    String jongmok_cd = null;
                    String jongmok_nm = null;
                    String ord_gb = null;
                    String ord_no = null;
                    String org_ord_no = null;
                    string ref_dt = null;
                    int ord_price = 0;
                    int ord_stock_cnt = 0;
                    int ord_amt = 0;
                    String ord_dtm = null;

                    user_id = g_user_id;
                    jongmok_cd = axKHOpenAPI1.GetChejanData(9001).Trim().Substring(1, 6);
                    jongmok_nm = get_jongmok_nm(jongmok_cd);
                    ord_gb = axKHOpenAPI1.GetChejanData(907).Trim();
                    ord_no = axKHOpenAPI1.GetChejanData(9203).Trim();
                    org_ord_no = axKHOpenAPI1.GetChejanData(904).Trim();
                    ord_price = int.Parse(axKHOpenAPI1.GetChejanData(901).Trim());
                    ord_stock_cnt = int.Parse(axKHOpenAPI1.GetChejanData(900).Trim());

                    DateTime CurTime;
                    String CurDt;
                    CurTime = DateTime.Now;
                    CurDt = CurTime.ToString("yyyy") + CurTime.ToString("MM") + CurTime.ToString("dd");

                    ref_dt = CurDt;
                    ord_dtm = CurDt + axKHOpenAPI1.GetChejanData(908).Trim();

                    write_msg_log("종목코드 : [" + jongmok_cd + "]" + "\n", 0);
                    write_msg_log("종목명 : [" + jongmok_nm + "]" + "\n", 0);
                    write_msg_log("주문구분 : [" + ord_gb + "]" + "\n", 0);
                    write_msg_log("주문번호 : [" + ord_no + "]" + "\n", 0);
                    write_msg_log("원주문번호 : [" + org_ord_no + "]" + "\n", 0);
                    write_msg_log("주문가 : [" + ord_price.ToString() + "]" + "\n", 0);
                    write_msg_log("주문주식수 : [" + ord_stock_cnt.ToString() + "]" + "\n", 0);
                    write_msg_log("주문금액 : [" + ord_amt.ToString() + "]" + "\n", 0);
                    write_msg_log("주문일시 : [" + ord_dtm + "]" + "\n", 0);

                    insert_tb_ord_lst(ref_dt, jongmok_cd, jongmok_nm, ord_gb, ord_no, org_ord_no, ord_price, ord_stock_cnt, ord_amt, ord_dtm);

                    if(ord_gb == "2") // 매수 주문일 경우 매수가능금액 조정 
                    {
                        update_tb_accnt(ord_gb, ord_amt);
                    }
                }

                else if(chejan_gb == "체결")
                {
                    String user_id = null;
                    String jongmok_cd = null;
                    String jongmok_nm = null;
                    String chegyul_gb = null;
                    int chegyul_no = 0;
                    int chegyul_price = 0;
                    int chegyul_cnt = 0;
                    int chegyul_amt = 0;
                    String chegyul_dtm = null;          
                    String ord_no = null;
                    String org_ord_no = null;
                    string ref_dt = null;

                    user_id = g_user_id;
                    jongmok_cd = axKHOpenAPI1.GetChejanData(9001).Trim().Substring(1, 6);
                    jongmok_nm = get_jongmok_nm(jongmok_cd);
                    chegyul_gb = axKHOpenAPI1.GetChejanData(907).Trim(); //2:매수 1:매도
                    chegyul_no = int.Parse(axKHOpenAPI1.GetChejanData(909).Trim());
                    chegyul_price = int.Parse(axKHOpenAPI1.GetChejanData(910).Trim());
                    chegyul_cnt = int.Parse(axKHOpenAPI1.GetChejanData(901).Trim());
                    chegyul_amt = chegyul_price * chegyul_cnt;
                    org_ord_no = axKHOpenAPI1.GetChejanData(904).Trim();

                    DateTime CurTime;
                    String CurDt;
                    CurTime = DateTime.Now;
                    CurDt = CurTime.ToString("yyyy") + CurTime.ToString("MM") + CurTime.ToString("dd");
                    ref_dt = CurDt;
                    chegyul_dtm = CurDt + axKHOpenAPI1.GetChejanData(908).Trim();
                    ord_no = axKHOpenAPI1.GetChejanData(9203).Trim();

                    write_msg_log("종목코드 : [" + jongmok_cd + "]" + "\n", 0);
                    write_msg_log("종목명 : [" + jongmok_nm + "]" + "\n", 0);
                    write_msg_log("체결구분 : [" + chegyul_gb + "]" + "\n", 0);
                    write_msg_log("체결번호 : [" + chegyul_no.ToString() + "]" + "\n", 0);
                    write_msg_log("체결가 : [" + chegyul_price.ToString() + "]" + "\n", 0);
                    write_msg_log("체결주식수 : [" + chegyul_cnt.ToString() + "]" + "\n", 0);
                    write_msg_log("체결금액 : [" + chegyul_amt.ToString() + "]" + "\n", 0);
                    write_msg_log("체결일시 : [" + chegyul_dtm + "]" + "\n", 0);
                    write_msg_log("주문번호 : [" + ord_no + "]" + "\n", 0);
                    write_msg_log("원주문번호 : [" + org_ord_no + "]" + "\n", 0);

                    insert_tb_chegyul_lst(ref_dt, jongmok_cd, jongmok_nm, chegyul_gb, chegyul_no, chegyul_price, chegyul_cnt, chegyul_amt, chegyul_dtm, ord_no, org_ord_no);

                    if(chegyul_gb == "1")
                    {
                        update_tb_accnt(chegyul_gb, chegyul_amt);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public string get_cur_tm()
        {
            DateTime l_cur_time;
            string l_cur_tm;

            l_cur_time = DateTime.Now;
            l_cur_tm = l_cur_time.ToString("HHmmss");
            return l_cur_tm;
        }

        public string get_jongmok_nm(string i_jongmok_cd)
        {
            string l_jongmok_nm = null;
            l_jongmok_nm = axKHOpenAPI1.GetMasterCodeName(i_jongmok_cd);
            return l_jongmok_nm;
        }

        private OracleConnection connect_db()
        {
            String conninfo = "User ID = ats;" +
            "Password = 1234;" +
            "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(Host = localhost)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = xe)));";

            OracleConnection conn = new OracleConnection(conninfo);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("connect_db() FAIL! " + ex.Message, "오류 발생");
                conn = null;
            }

            return conn;
        }

        public void write_msg_log(String text, int is_clear)
        {
            DateTime l_cur_time;
            String l_cur_dt;
            String l_cur_tm;
            String l_cur_dtm;

            l_cur_dt = "";
            l_cur_tm = "";

            l_cur_time = DateTime.Now;
            l_cur_dt = l_cur_time.ToString("yyyy-") + l_cur_time.ToString("MM-") + l_cur_time.ToString("dd");
            l_cur_tm = l_cur_time.ToString("HH:mm:ss");
            l_cur_dtm = "[" + l_cur_dt + " " + l_cur_tm + "]";

            if (is_clear == 1)
            {
                if(this.textBox_msg_log.InvokeRequired)
                {
                    textBox_msg_log.BeginInvoke(new Action(() => textBox_msg_log.Clear()));
                }
                else
                {
                    this.textBox_msg_log.Clear();
                }
            }

            else
            {
                if(this.textBox_msg_log.InvokeRequired)
                {
                    textBox_msg_log.BeginInvoke(new Action(() => textBox_msg_log.AppendText(l_cur_dtm + text)));
                }
                else
                {
                    this.textBox_msg_log.AppendText(l_cur_dtm + text);
                }
            }
        }

        public void write_err_log(String text, int is_clear)
        {
            DateTime l_cur_time;
            String l_cur_dt;
            String l_cur_tm;
            String l_cur_dtm;

            l_cur_dt = "";
            l_cur_tm = "";

            l_cur_time = DateTime.Now;
            l_cur_dt = l_cur_time.ToString("yyyy-") + l_cur_time.ToString("MM-") + l_cur_time.ToString("dd");
            l_cur_tm = l_cur_time.ToString("HH:mm:ss");
            l_cur_dtm = "[" + l_cur_dt + " " + l_cur_tm + "]";

            if (is_clear == 1)
            {
                if (this.textBox_err_log.InvokeRequired)
                {
                    textBox_err_log.BeginInvoke(new Action(() => textBox_err_log.Clear()));
                }
                else
                {
                    this.textBox_err_log.Clear();
                }
            }

            else
            {
                if (this.textBox_err_log.InvokeRequired)
                {
                    textBox_err_log.BeginInvoke(new Action(() => textBox_err_log.AppendText(l_cur_dtm + text)));
                }
                else
                {
                    this.textBox_err_log.AppendText(l_cur_dtm + text);
                }
            }
        }


        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public DateTime delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWard = ThisMoment.Add(duration);

            while (AfterWard >= ThisMoment)
            {
                try
                {
                    unsafe
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch(AccessViolationException ex)
                {
                    write_err_log("delay() ex.Messsage : [" + ex.Message + "]\n", 0);
                }
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        private string get_scr_no()
        {
            if (g_scr_no < 9999)
                g_scr_no++;
            else
                g_scr_no = 1000;
            return g_scr_no.ToString();
        }

        private void 로그인ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int ret = 0;
            int ret2 = 0;

            String l_accno = null;
            String l_accno_cnt = null;
            String[] l_accno_arr = null;

            ret = axKHOpenAPI1.CommConnect();

            if(ret == 0)
            {
                toolStripStatusLabel1.Text = "로그인 중...";

                for (;;)
                {
                    ret2 = axKHOpenAPI1.GetConnectState();
                    if(ret2==1)
                    {
                        break;
                    }
                    else
                    {
                        delay(1000);
                    }
                }

                toolStripStatusLabel1.Text = "로그인 완료";

                g_user_id = "";
                g_user_id = axKHOpenAPI1.GetLoginInfo("USER_ID").Trim();
                textBox1.Text = g_user_id;

                l_accno_cnt = "";
                l_accno_cnt = axKHOpenAPI1.GetLoginInfo("ACCOUNT_CNT").Trim();
                l_accno_arr = new String[int.Parse(l_accno_cnt)];

                l_accno = "";
                l_accno_arr = l_accno.Split(';');

                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(l_accno_arr);
                comboBox1.SelectedIndex = 0;
                g_accnt_no = comboBox1.SelectedItem.ToString().Trim();

            }
        }

        private void 로그아웃ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axKHOpenAPI1.CommTerminate();
            toolStripStatusLabel1.Text = "로그아웃 완료되었습니다.";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            g_accnt_no = comboBox1.SelectedItem.ToString().Trim();
            write_msg_log("사용할 증권계좌번호는 : [" + g_accnt_no + "] 입니다. \n", 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OracleCommand cmd;
            OracleConnection conn;
            OracleDataReader reader = null;

            string sql;
            string l_jongmok_cd;
            string l_jongmok_nm;
            int l_priority;
            int l_buy_amt;
            int l_buy_price;
            int l_target_price;
            int l_cut_loss_price;
            string l_buy_trd_yn;
            string l_sell_trd_yn;
            int l_seq = 0;
            string[] l_arr = null;
            conn = null;
            conn = connect_db();
            cmd = null;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            sql = null;
            sql = "SELECT       " + 
                "       JONGMOK_CD  ,       " +
                "       JONGMOK_NM  ,       " +
                "       PRIORITY    ,       " +
                "       BUY_AMT     ,       " +
                "       BUY_PRICE   ,       " +
                "       TARGET_PRICE    ,       " +
                "       CUT_LOSS_PRICE     ,        " +
                "       BUY_TRD_YN  ,       " +
                "       SELL_TRD_YN ,       " +
                " FROM                  " +
                "       TB_TRD_JONGMOK  " +
                "       WHERE USER_ID = " + "'" + g_user_id + "' order by PRIORITY ";
            cmd.CommandText = sql;

            this.Invoke(new MethodInvoker(
            delegate ()
            {
                dataGridView1.Rows.Clear();
            }));

            try
            {
                reader = cmd.ExecuteReader();
            }
            catch(Exception ex)
            {
                write_err_log("SELECT TB_TRD_JONGMOK ex.Message : [" + ex.Message + "]\n", 0);
            }

            l_jongmok_cd = "";
            l_jongmok_nm = "";
            l_priority = 0;
            l_buy_amt = 0;
            l_buy_price = 0;
            l_target_price = 0;
            l_cut_loss_price = 0;
            l_buy_trd_yn = "";
            l_sell_trd_yn = "";

            while(reader.Read())
            {
                l_seq++;
                l_jongmok_cd = "";
                l_jongmok_nm = "";
                l_priority = 0;
                l_buy_amt = 0;
                l_buy_price = 0;
                l_target_price = 0;
                l_cut_loss_price = 0;
                l_buy_trd_yn = "";
                l_sell_trd_yn = "";
                l_seq = 0;

                l_jongmok_cd = reader[0].ToString().Trim();
                l_jongmok_nm = reader[1].ToString().Trim();
                l_priority = int.Parse(reader[2].ToString().Trim());
                l_buy_amt = int.Parse(reader[3].ToString().Trim());
                l_buy_price = int.Parse(reader[4].ToString().Trim());
                l_target_price = int.Parse(reader[5].ToString().Trim());
                l_cut_loss_price = int.Parse(reader[6].ToString().Trim());
                l_buy_trd_yn = reader[7].ToString().Trim(); 
                l_sell_trd_yn = reader[8].ToString().Trim();

                l_arr = null;
                l_arr = new String[]
                {
                    l_seq.ToString(),
                    l_jongmok_cd,
                    l_jongmok_nm,
                    l_priority.ToString(),
                    l_buy_amt.ToString(),
                    l_buy_price.ToString(),
                    l_target_price.ToString(),
                    l_cut_loss_price.ToString(),
                    l_buy_trd_yn,
                    l_sell_trd_yn
                };

                this.Invoke(new MethodInvoker(
                delegate ()
                {
                    dataGridView1.Rows.Add(l_arr);
                }));
            }

            write_msg_log("TB_TRD_JONGMOK 테이블이 조회되었습니다.\n", 0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OracleCommand cmd;
            OracleConnection conn;
         
            string sql;
            string l_jongmok_cd;
            string l_jongmok_nm;
            int l_priority;
            int l_buy_amt;
            int l_buy_price;
            int l_target_price;
            int l_cut_loss_price;
            string l_buy_trd_yn;
            string l_sell_trd_yn;

            foreach(DataGridViewRow Row in dataGridView1.Rows)
            {
                if(Convert.ToBoolean(Row.Cells[check.Name].Value) != Capture)
                {
                    continue;
                }
                if (Convert.ToBoolean(Row.Cells[check.Name].Value) == Capture)
                {
                    l_jongmok_cd = Row.Cells[1].Value.ToString();
                    l_jongmok_nm = Row.Cells[2].Value.ToString();
                    l_priority = int.Parse(Row.Cells[3].Value.ToString());
                    l_buy_amt = int.Parse(Row.Cells[4].Value.ToString());
                    l_buy_price = int.Parse(Row.Cells[5].Value.ToString());
                    l_target_price = int.Parse(Row.Cells[6].Value.ToString());
                    l_cut_loss_price = int.Parse(Row.Cells[7].Value.ToString());
                    l_buy_trd_yn = Row.Cells[8].Value.ToString();
                    l_sell_trd_yn = Row.Cells[9].Value.ToString();

                    conn = null;
                    conn = connect_db();
                    cmd = null;
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    sql = null;
                    sql = @"insert into TB_TRD_JONGMOK values " +
                        "(" +
                        "'" + l_jongmok_cd + "'" + "," +
                        "'" + l_jongmok_nm + "'" + "," +
                              l_priority + "'" +
                              l_buy_amt + "'" +
                              l_buy_price + "'" +
                              l_target_price + "'" +
                              l_cut_loss_price + "'" +
                        "'" + l_buy_trd_yn + "'" + "," +
                        "'" + l_sell_trd_yn + "'" + "," +
                        "'" + g_user_id + "'" + "," +
                        "sysdate " + "," +
                        "NULL" + "," +
                        "NULL" +
                        ")";

                    cmd.CommandText = sql;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        write_err_log("insert TB_TRD_JONGMOK ex.Message : [" + ex.Message + "]\n", 0);
                    }
                    write_msg_log("종목코드 : [" + l_jongmok_cd + "]" + "가 삽입되었습니다.\n", 0);
                    
                    conn.Close();


                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OracleCommand cmd;
            OracleConnection conn;

            string sql;
            string l_jongmok_cd;
            string l_jongmok_nm;
            int l_priority;
            int l_buy_amt;
            int l_buy_price;
            int l_target_price;
            int l_cut_loss_price;
            string l_buy_trd_yn;
            string l_sell_trd_yn;

            foreach (DataGridViewRow Row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(Row.Cells[check.Name].Value) != Capture)
                {
                    continue;
                }
                if (Convert.ToBoolean(Row.Cells[check.Name].Value) == Capture)
                {
                    l_jongmok_cd = Row.Cells[1].Value.ToString();
                    l_jongmok_nm = Row.Cells[2].Value.ToString();
                    l_priority = int.Parse(Row.Cells[3].Value.ToString());
                    l_buy_amt = int.Parse(Row.Cells[4].Value.ToString());
                    l_buy_price = int.Parse(Row.Cells[5].Value.ToString());
                    l_target_price = int.Parse(Row.Cells[6].Value.ToString());
                    l_cut_loss_price = int.Parse(Row.Cells[7].Value.ToString());
                    l_buy_trd_yn = Row.Cells[8].Value.ToString();
                    l_sell_trd_yn = Row.Cells[9].Value.ToString();

                    conn = null;
                    conn = connect_db();
                    cmd = null;
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    sql = null;
                    sql = @" UPDATE TB_TRD_JONGMOK
                        SET
                            JONGMOK_NM = " + "'" + l_jongmok_nm + "'" + "," +
                            "PRIORITY = " + l_priority + "." +
                            "BUY_AMT = " + l_buy_amt + "." +
                            "BUY_RPICE = " + l_buy_price + "." +
                            "TARGET_PRICE = " + l_target_price + "." +
                            "CUT_LOSS_PRICE = " + l_cut_loss_price + "." +
                            "BUY_TRD_YN = " + "'" + l_buy_trd_yn + "'" + "." +
                            "SELL_TRD_YN = " + "'" + l_sell_trd_yn + "'" + "." +
                            "UPDT_ID = " + "'" + g_user_id + "'" + "," +
                            "UPDT_DTM = SYSDATE " +
                        " WHERE JONGMOK_CD + " + "'" + l_jongmok_cd + "'" +
                        " AND USER_ID = " + "'" + g_user_id + "'";


                    cmd.CommandText = sql;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        write_err_log("UPDATE TB_TRD_JONGMOK ex.Message : [" + ex.Message + "]\n", 0);
                    }
                    write_msg_log("종목코드 : [" + l_jongmok_cd + "]" + "가 수정되었습니다.\n", 0);

                    conn.Close();


                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OracleCommand cmd;
            OracleConnection conn;

            string sql;
            string l_jongmok_cd = null;

            foreach (DataGridViewRow Row in dataGridView1.Rows)
            {
                if(Convert.ToBoolean(Row.Cells[check.Name].Value) != true)
                {
                    continue;
                }
                if (Convert.ToBoolean(Row.Cells[check.Name].Value) == true)
                {
                    l_jongmok_cd = Row.Cells[1].Value.ToString();

                    conn = null;
                    conn = connect_db();

                    cmd = null;
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    sql = null;
                    sql = @" DELETE FROM TB_TRD_JONGMOK " +
                        " WHERE JONGMOK_CD = " + "'" + l_jongmok_cd + "'" +
                        " AND USER_ID = " + "'" + g_user_id + "'";

                    cmd.CommandText = sql;

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        write_err_log("DELETE TB_TRD_JONGMOK ex.Message : [" + ex.Message + "]\n", 0);
                    }
                    write_msg_log("종목코드 : [" + l_jongmok_cd + "]" + "가 삭제되었습니다.\n", 0);

                    conn.Close();

                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(g_is_thread == 1)
            {
                write_msg_log("Auto Trading이 이미 시작되었습니다. \n", 0);
                return;
            }
            g_is_thread = 1;
            thread1 = new Thread(new ThreadStart(m_thread1));
            thread1.Start();
        }

        public void m_thread1()
        {
            string l_cur_tm = null;
            int l_set_tb_accnt_flag = 0;
            int l_set_tb_accnt_info_flag = 0;
            
            if(g_is_thread == 0)
            {
                g_is_thread = 1;
                write_msg_log("자동매매가 시작되었습니다.\n", 0);
            }
            for(;;)
            {
                l_cur_tm = get_cur_tm();
                if(l_cur_tm.CompareTo("083001") >= 0)
                {
                    // 계좌조회, 계좌정보조회, 보유종목 매도주문 수행
                    if(l_set_tb_accnt_flag == 0)
                    {
                        l_set_tb_accnt_flag = 1;
                        set_tb_accnt();
                    }
                    if(l_set_tb_accnt_info_flag == 0)
                    {
                        set_tb_accnt_info();
                        l_set_tb_accnt_info_flag = 1;
                    }
                }
                if(l_cur_tm.CompareTo("090001") >= 0)
                {
                    for(;;)
                    {
                        l_cur_tm = get_cur_tm();
                        if (l_cur_tm.CompareTo("153001") >= 0)
                        {
                            break;
                        }
                        delay(200);
                    }
                }
                delay(200);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            write_msg_log("\n자동매매 중지 시작\n", 0);

            try
            {
                thread1.Abort();
            }
            catch(Exception ex)
            {
                write_err_log("자동매매 중지 ex.Message : " + ex.Message + "\n", 0);
            }

            this.Invoke(new MethodInvoker(() =>
            {
                if (thread1 != null)
                {
                    thread1.Interrupt();
                    thread1 = null;
                }
            }));
            g_is_thread = 0;

            write_msg_log("\n자동매매 중지 완료\n", 0);
        }

        public void set_tb_accnt()
        {
            int l_for_cnt = 0;
            int l_for_flag = 0;

            write_msg_log("TB_ACCNT 테이블 세팅 시작\n", 0);
            
            g_ord_amt_possible = 0;

            l_for_flag = 0;
            for(;;)
            {
                axKHOpenAPI1.SetInputValue("계좌번호", g_accnt_no);
                axKHOpenAPI1.SetInputValue("비밀번호", "");

                g_rqname = "";
                g_rqname = "증거금세부내역조회요청";
                g_flag_1 = 0;

                String l_scr_no = null;
                l_scr_no = "";
                l_scr_no = get_scr_no();
                axKHOpenAPI1.CommRqData("증거금세부내역조회요청", "opw00013", 0, l_scr_no);

                l_for_cnt = 0;
                for(;;)
                {
                    if(g_flag_1 == 1)
                    {
                        delay(1000);
                        axKHOpenAPI1.DisconnectRealData(l_scr_no);
                        l_for_flag = 1;
                        break;
                    }
                    else
                    {
                        write_msg_log("'증거금세부내역조회요청' 완료 대기 중...\n", 0);
                        delay(1000);
                        l_for_cnt++;
                        if(l_for_cnt == 1)
                        {
                            l_for_flag = 0;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                axKHOpenAPI1.DisconnectRealData(l_scr_no);

                if(l_for_flag == 1)
                {
                    break;
                }
                else if(l_for_flag == 0)
                {
                    delay(1000);
                    break;
                }
                delay(1000);
            }
            write_msg_log("주문가능금액 : [" + g_ord_amt_possible.ToString() + "]\n", 0);

            merge_tb_accnt(g_ord_amt_possible); 
        }

        public void merge_tb_accnt(int g_ord_amt_possible)
        {
            OracleCommand cmd = null;
            OracleConnection conn = null;
            String l_sql = null;
            l_sql = null;
            cmd = null;
            conn = null;
            conn = connect_db();

            if (conn != null)
            {
                cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;

                l_sql = @"merge into tb_accnt a using(select nvl(max(user_id), ' ') user_id, nvl(max(accnt_no), ' ') accnt_no, nvl(max(ref_dt), ' ') ref_dt " +
                     " from tb_accnt" +
                     " where user_id = '" + g_user_id + "'" +
                     "and accnt_no = " + "'" + g_accnt_no + "'" +
                     "and ref_dt = to_char(sysdate, 'yyyymmdd') " +
                     " ) b " +
                     " on ( a.user_id = b.user_id and a.accntno = b.accnt_no and a.ref_dt = b.ref_dt) " +
                     " when matche then update  " +
                     " set ord_possible_amt = " + g_ord_amt_possible + "," +
                     " updt_dtm = SYSDATE" + "," +
                     " updt_id = 'ats'" +
                     " when not matched then insert (a.user_id, a.accnt_no, a.ref_dt, a.ord_possible_amt, a.inst_dtm, a.inst_id) values ( " +
                     "'" + g_user_id + "'" + "," +
                     "'" + g_accnt_no + "'" + "," +
                     " to_char(sysdate, 'yyyymmdd')" + "," + g_ord_amt_possible + "," + "SYSDATE, " + "'ats'" + " )";

                cmd.CommandText = l_sql;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    write_err_log("merge_tb_accnt() ex : [" + ex.Message + "]\n", 0);
                }
                finally
                {
                    conn.Close();
                }
                   
            }
            else
            {
                write_msg_log("db connection check!\n", 0);
            }
        }

        public void set_tb_accnt_info()
        {
            OracleCommand cmd = null;
            OracleConnection conn = null;
            String sql = null;
            int l_for_cnt = 0;
            int l_for_flag = 0;
            conn = connect_db();
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            sql = @"delete from tb_accnt_info where ref_dt  = to_cahr(sysdate, 'yyyymmdd') and user id = " + "'" + g_user_id + "'";

            cmd.CommandText = sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                write_err_log("delete tb_accnt_info ex.Message : [" + ex.Message + "]\n", 0);
            }

            conn.Close();

            g_is_next = 0;

            for(;;)
            {
                l_for_flag = 0;
                for(;;)
                {
                    axKHOpenAPI1.SetInputValue("계좌번호", g_accnt_no);
                    axKHOpenAPI1.SetInputValue("비밀번호", "");
                    axKHOpenAPI1.SetInputValue("상장폐지조회구분", "1");
                    axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");

                    g_flag_2 = 0;
                    g_rqname = "계좌평가현황요청";

                    String l_scr_no = get_scr_no();

                    axKHOpenAPI1.CommRqData("계좌평가현황요청", "OPW00004", g_is_next, l_scr_no);

                    l_for_cnt = 0;
                    for(;;)
                    {
                        if(g_flag_2 == 1)
                        {
                            delay(1000);
                            axKHOpenAPI1.DisconnectRealData(l_scr_no);
                            l_for_flag = 1;

                            break;
                        }
                        else
                        {
                            delay(1000);
                            l_for_cnt++;
                            if(l_for_cnt == 5)
                            {
                                l_for_flag = 0;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    delay(1000);
                    axKHOpenAPI1.DisconnectRealData(l_scr_no);

                    if(l_for_flag == 1)
                    {
                        break;
                    }
                    else if(l_for_flag == 0)
                    {
                        delay(1000);
                        continue;
                    }

                }

                if(g_is_next == 0)
                {
                    break;
                }
                delay(1000);
            }
        }

        public void insert_tb_accnt_info(string i_jongmok_cd, string i_jongmok_nm, int i_buy_price, int i_own_stock_cnt, int i_own_amt)
        {
            OracleCommand cmd = null;
            OracleConnection conn = null;
            String l_sql = null;
            conn = connect_db();
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            l_sql = @" insert into tb_accnt_info values ( " +
                    "'" + g_user_id + "'" + "," +
                    "'" + g_accnt_no + "'" + "," +
                    "to_char(sysdate, 'yyyymmdd')" + "," +
                    "'" + i_jongmok_cd + "'" + "," +
                    "'" + i_jongmok_nm + "'" + "," +
                    +i_buy_price + "," +
                    +i_own_stock_cnt + "," +
                    +i_own_amt + "," +
                    "'ats'" + "," +
                    "SYSDATE" + "," +
                    "null" + "," +
                    "null" + ") ";

            cmd.CommandText = l_sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                write_err_log("insert tb_accnt_info() insert tb_accnt_info ex.Message : [" + ex.Message + "]\n", 0);
            }
            conn.Close();
        }

        public void insert_tb_ord_lst(string i_ref_dt, String i_jongmok_cd, String i_jongmok_nm, String i_ord_gb, String i_ord_no, String i_org_ord_no, int i_ord_price, int i_ord_stock_cnt, int i_ord_amt, String i_ord_dtm)
        {
            OracleCommand cmd = null;
            OracleConnection conn = null;
            String l_sql = null;

            conn = connect_db();
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            l_sql = @"insert into tb_ord_lst values ( " + 
                "'" + g_user_id + "'" + "," +
                "'" + g_accnt_no + "'" + "," +
                "'" + i_ref_dt + "'" + "," +
                "'" + i_jongmok_cd + "'" + "," +
                "'" + i_jongmok_nm + "'" + "," +
                "'" + i_ord_gb + "'" + "," +
                "'" + i_ord_no + "'" + "," +
                "'" + i_org_ord_no + "'" + "," +
               +i_ord_price + "." +
               +i_ord_stock_cnt + "." +
               +i_ord_amt + "." +
               "'" + i_ord_dtm + "'" + "," +
               "'ats'" + "," +
               "SYSDATE " + "," +
               "NULL" + "," +
               "NULL" + ")";

            cmd.CommandText = l_sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                write_err_log("insert tb_ord_lst ex : [" + ex.Message + "] \n", 0);
            }
            conn.Close();
        }

        public void update_tb_accnt(String i_chegyul_gb, int i_chegyul_amt)
        {
            OracleCommand cmd = null;
            OracleConnection conn = null;
            String l_sql = null;

            conn = connect_db();
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            if(i_chegyul_gb == "2")
            {
                l_sql = @" update TB_ACCNT set ORD_POSSIBLE_AMT = ord_possible_amt - " + i_chegyul_amt + ", updt_dtm = SYSDATE, updt_id = 'ats'" +
                    "where user_id = " + "'" + g_user_id + "'" +
                    "and accnt_no = " + "'" + g_accnt_no + "'" +
                    "and ref_dt = to_char(sysdate, 'yyyymmdd') ";
            }
            else if(i_chegyul_gb == "1")
            {
                l_sql = @" update TB_ACCNT set ORD_POSSIBLE_AMT = ord_possible_amt + " + i_chegyul_amt + ", updt_dtm = SYSDATE, updt_id = 'ats'" +
                   "updt_dtm = SYSDATE, updt_id = 'ats'" +
                   "where user_id = " + "'" + g_user_id + "'" +
                   "and accnt_no = " + "'" + g_accnt_no + "'" +
                   "and ref_dt = to_char(sysdate, 'yyyymmdd') ";
            }
            cmd.CommandText = l_sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                write_err_log("update TB_ACCNT ex.Message : [" + ex.Message + "]\n", 0);
            }
            conn.Close();
        }

        public void insert_tb_chegyul_lst(string i_ref_dt, String i_jongmok_cd, String i_jongmok_nm, String i_chegyul_gb, int i_chegyul_no, int i_chegyul_price, int i_chegyul_stock_cnt, int i_chegyul_amt, String i_chegyul_dtm, String i_ord_no, String i_org_ord_no)
        {
            OracleCommand cmd = null;
            OracleConnection conn = null;
            String l_sql = null;

            conn = connect_db();
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            l_sql = @"insert into tb_chegyul_lst values ( " +
                "'" + g_user_id + "'" + "," +
                "'" + g_accnt_no + "'" + "," +
                "'" + i_ref_dt + "'" + "," +
                "'" + i_jongmok_cd + "'" + "," +
                "'" + i_jongmok_nm + "'" + "," +
                "'" + i_chegyul_gb + "'" + "," +
                "'" + i_ord_no + "'" + "," +
                "'" + i_org_ord_no + "'" + "," +
               +i_chegyul_no + "." +
               +i_chegyul_price + "." +
               +i_chegyul_stock_cnt + "." +
               +i_chegyul_amt + "." +
               "'" + i_chegyul_dtm + "'" + "," +
               "'ats'" + "," +
               "SYSDATE " + "," +
               "NULL" + "," +
               "NULL" + ")";

            cmd.CommandText = l_sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                write_err_log("insert tb_chegyul_lst ex : [" + ex.Message + "] \n", 0);
            }
            conn.Close();
        }


    }
}
