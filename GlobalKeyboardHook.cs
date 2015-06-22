using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestForm {
    //修改自:https://github.com/merttasci/keylogger/blob/master/WindowsFormsApplication1/GlobalKeyBoardHook.cs
    public class GlobalKeyboardHook {
        #region 載入系統函式庫
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr hhk ,int code ,int wParam ,ref keyBoardHookStruct lParam);
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook ,LLKeyboardHook callback ,IntPtr hInstance ,uint theardID);
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        #endregion

        #region 隱含參數
        //宣告System Event Handler方法型別(委派)
        delegate int LLKeyboardHook(int Code ,int wParam ,ref keyBoardHookStruct lParam);

        //宣告系統回傳資料結構
        struct keyBoardHookStruct {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;
        
        LLKeyboardHook llkh;//System Event Handler
        bool enable = false;
        /// <summary>
        /// Windows API Hook Id
        /// </summary>
        IntPtr HookTarget = IntPtr.Zero;
        #endregion
        
        /// <summary>
        /// 受監聽按鍵集合
        /// </summary>
        public List<Keys> HookedKeys{get;set;}

        /// <summary>
        /// 表示是否啟用掛勾
        /// </summary>
        public bool Enable {
            get {
                return this.enable;
            }
            set {
                if(value && value != enable) {
                    Hook();
                } else if(!value && value != enable){
                    Unhook();
                }
                enable = value;
            }
        }

        

        /// <summary>
        /// 鍵盤按下後觸發的事件
        /// </summary>
        public event KeyEventHandler KeyDown;
        /// <summary>
        /// 鍵盤按鍵鬆開時的事件
        /// </summary>
        public event KeyEventHandler KeyUp;

        
        public GlobalKeyboardHook(bool NewAndHook = false) {
            llkh = new LLKeyboardHook(HookProc);//設定Handler
            HookedKeys = new List<Keys>();//初始化集合
            if(NewAndHook) {//如果允許建構物件後立刻開始監聽
                Hook();
            }
        }

        public GlobalKeyboardHook(bool NewAndHook ,params Keys[] HookKeys):this() {
            this.HookedKeys.AddRange(HookKeys);
            if(NewAndHook) {
                Hook();
            }
        }

        public GlobalKeyboardHook(bool NewAndHook ,bool HookAllKeys):this() {
            Keys[] HookKeys = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray();
            this.HookedKeys.AddRange(HookKeys);
            if(NewAndHook) {
                Hook();
            }
        }

        ~GlobalKeyboardHook() { Unhook(); }//解構子，釋放監聽掛勾

        /// <summary>
        /// 啟用掛勾
        /// </summary>
        public void Hook() {
            if(enable) {//已經啟用則先關閉在啟用
                Unhook();
            }
            IntPtr hInstance = LoadLibrary("User32");//將監聽目標掛在User32，達到全域效果
            HookTarget = SetWindowsHookEx(WH_KEYBOARD_LL ,llkh ,hInstance ,0);
            enable = true;
        }

        /// <summary>
        /// 停用掛勾
        /// </summary>
        public void Unhook() {
            if(!enable) {//尚未啟用則不能關閉
                return;
            }
            UnhookWindowsHookEx(HookTarget);
            enable = false;
        }

        private int HookProc(int Code ,int wParam ,ref keyBoardHookStruct lParam) {
            if(Code >= 0) {
                Keys key = (Keys)lParam.vkCode;
                if(HookedKeys.Contains(key)) {
                    KeyEventArgs kArg = new KeyEventArgs(key);
                    if((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                        KeyDown(this ,kArg);
                    else if((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                        KeyUp(this ,kArg);
                    if(kArg.Handled)
                        return 1;
                }
            }
            return CallNextHookEx(HookTarget ,Code ,wParam ,ref lParam);
        }
    }
}
