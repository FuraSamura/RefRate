using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RefRate
{
    public partial class Form1 : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE1
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;

            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;

            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;

            public int dmPanningWidth;
            public int dmPanningHeight;
        };



        class User_32
        {
            [DllImport("user32.dll")]
            public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE1 devMode);
            [DllImport("user32.dll")]
            public static extern int ChangeDisplaySettings(ref DEVMODE1 devMode, int flags);

            public const int ENUM_CURRENT_SETTINGS = -1;
            public const int CDS_UPDATEREGISTRY = 0x01;
            public const int CDS_TEST = 0x02;
            public const int DISP_CHANGE_SUCCESSFUL = 0;
            public const int DISP_CHANGE_RESTART = 1;
            public const int DISP_CHANGE_FAILED = -1;
        }



        class CResolution
        {
            public CResolution(int a)
            {
                Screen screen = Screen.PrimaryScreen;


                int Frequency = a;


                DEVMODE1 dm = new DEVMODE1();
                dm.dmDeviceName = new String(new char[32]);
                dm.dmFormName = new String(new char[32]);
                dm.dmSize = (short)Marshal.SizeOf(dm);

                if (0 != User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref dm))
                {

                    dm.dmDisplayFrequency = Frequency;

                    int iRet = User_32.ChangeDisplaySettings(ref dm, User_32.CDS_TEST);

                    if (iRet == User_32.DISP_CHANGE_FAILED)
                    {
                        MessageBox.Show("Unable to process your request");
                        MessageBox.Show("Description: Unable To Process Your Request. Sorry For This Inconvenience.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        iRet = User_32.ChangeDisplaySettings(ref dm, User_32.CDS_UPDATEREGISTRY);

                        switch (iRet)
                        {
                            case User_32.DISP_CHANGE_SUCCESSFUL:
                                {
                                    break;

                                    //successfull change
                                }
                            case User_32.DISP_CHANGE_RESTART:
                                {

                                    MessageBox.Show("Description: You Need To Reboot For The Change To Happen.\n If You Feel Any Problem After Rebooting Your Machine\nThen Try To Change Resolution In Safe Mode.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    break;
                                    //windows 9x series you have to restart
                                }
                            default:
                                {

                                    MessageBox.Show("Description: Failed To Change The Resolution.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    break;
                                    //failed to change
                                }
                        }
                    }

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ShowInTaskbar = false;
            Form1 form1 = new Form1();
            notifyIcon1.Visible = false;
            //this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            //this.ShowInTaskbar = false;
            form1.Hide();
            try
            {
                RegistryKey CurrentUserKey = Registry.CurrentUser;

                string rate = "";
                try
                {
                    //RegistryKey localMachineKey = Registry.LocalMachine;
                    RegistryKey RefRateKey = CurrentUserKey.OpenSubKey("RefRateKey");
                    if (CurrentUserKey.OpenSubKey("RefRateKey") == null)
                    {
                        RegistryKey RefRateKeyC = CurrentUserKey.CreateSubKey("RefRateKey");
                        RefRateKeyC.SetValue("Rate", "60");
                        RefRateKeyC.Close();
                    }
                    else
                    {
                        rate = RefRateKey.GetValue("Rate").ToString();
                        RefRateKey.Close();
                    }
                }
                catch
                {
                    //RegistryKey localMachineKey = Registry.LocalMachine;
                    RegistryKey RefRateKey = CurrentUserKey.CreateSubKey("RefRateKey");
                    RefRateKey.SetValue("Rate", "60");
                    RefRateKey.Close();

                }
                if (rate == "" || rate == null)
                {
                    //RegistryKey localMachineKey = Registry.LocalMachine;
                    RegistryKey RefRateKey = CurrentUserKey.CreateSubKey("RefRateKey");
                    RefRateKey.SetValue("Rate", "60");
                    RefRateKey.Close();
                }
                while (true)
                {
                    RegistryKey RefRateKey = CurrentUserKey.OpenSubKey("RefRateKey");

                    rate = RefRateKey.GetValue("Rate").ToString();
                    RefRateKey.Close();
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Control\\UnitedVideo\\CONTROL\\VIDEO\\{A2E1D0F8-F4B4-11EC-8EB2-806E6F6E6963}\\0000"))
                    {
                        if (key != null)
                        {
                            Object o = key.GetValue("DefaultSettings.VRefresh");
                            if (o != null)
                            {
                                int RefRate = Convert.ToInt32(o);  //"as" because it's REG_SZ...otherwise ToString() might be safe(r)
                                if (RefRate != Convert.ToInt32(rate))
                                {
                                    CResolution ChangeRes = new CResolution(Convert.ToInt32(rate));
                                }                               
                            }
                        }
                    }
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            {
                //react appropriately
            }
        }

        

        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}
