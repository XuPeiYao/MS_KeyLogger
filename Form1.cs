using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
namespace TestForm {
    public partial class Form1 : Form {
        GlobalKeyboardHook keyhook;
        public Form1() {
            InitializeComponent();
        }


        void keyhook_KeyDown(object sender ,KeyEventArgs e) {
            this.textBox1.AppendText("," + e.KeyCode);//將輸入按鍵的名稱印出
            //這裡可以做些什麼...Ex:將值傳送至遠端或本地儲存
            Label Target = (from t in VKeyboard
                            where t.Text.ToUpper() == e.KeyCode.ToString().ToUpper()
                            select t).FirstOrDefault();
            if(Target != null)
                Target.BackColor = Color.Gray;
        }
        void keyhook_KeyUp(object sender ,KeyEventArgs e) {
            Label Target = (from t in VKeyboard
                            where t.Text.ToUpper() == e.KeyCode.ToString().ToUpper()
                            select t).FirstOrDefault();
            if(Target != null)
                Target.BackColor = this.BackColor;//恢復顏色
        }


        List<Label> VKeyboard = new List<Label>();

        private void Form1_Load(object sender ,EventArgs e) {
            #region 虛擬鍵盤顯示
            string[][] Keyboard =new string[][]{
                "D1,D2,D3,D4,D5,D6,D7,D8,D9,D0".Split(','),
                "q,w,e,r,t,y,u,i,o,p".Split(','),
                "a,s,d,f,g,h,j,k,l".Split(','),
                "z,x,c,v,b,n,m".Split(','),
                "space".Split(','),
                "789".ToCharArray().Select(input=>"NumPad" + input).ToArray(),
                "456".ToCharArray().Select(input=>"NumPad" + input).ToArray(),
                "1230".ToCharArray().Select(input=>"NumPad" + input).ToArray(),
            };

            for(int i = 0; i < Keyboard.Length;i++ ) {
                for(int j = 0; j < Keyboard[i].Length; j++) {
                    Label NEW = new Label();
                    NEW.Text = Keyboard[i][j];
                    NEW.Width = 30;
                    NEW.Height = 30;
                    NEW.Left = j * 30;
                    NEW.Top = i * 30;
                    NEW.TextAlign = ContentAlignment.MiddleCenter;
                    NEW.BorderStyle = BorderStyle.FixedSingle;
                    this.Controls.Add(NEW);
                    VKeyboard.Add(NEW);
                }
            }
            #endregion

            keyhook = new GlobalKeyboardHook(
                NewAndHook: false ,//建構完物件不立刻開始監聽
                HookAllKeys: true//監聽所有按鍵
            );
            keyhook.KeyDown += keyhook_KeyDown;//加入事件
            keyhook.KeyUp += keyhook_KeyUp;
            keyhook.Enable = true;//開始監聽
        }

        private void Form1_FormClosed(object sender ,FormClosedEventArgs e) {
            keyhook.Enable = false;
        }

        
    }
}
