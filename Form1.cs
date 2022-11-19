/*
MIT License

Copyright (c) 2022 benjysugi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net;

namespace SkateLauncher {
    public partial class Form1 : Form {
        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Form1() {
            InitializeComponent();
        }

        static Form1 frm;

        struct LaunchData {
            public bool DisableWatermark;
            public bool MultiplayerEnabled;
            public bool PickedFromServerList;
            public bool IsServerHost;
            public bool PresenceEnabled;
            public string Username;
            public string ServerIP;
            public string AdditionalArgs;
        }

        struct Settings {
            public string SetUsername;
            public bool DisableWatermark;
            public bool Multiplayer;
            public string AdditionalSwitches;
            public bool UseAdditionalSwitches;
        }

        private string GetFile() {
            StackTrace st = new StackTrace(new StackFrame(true));
            StackFrame sf = st.GetFrame(0);
            return sf.GetFileName();
        }

        private void button1_Click(object sender, EventArgs e) {
            string executablePath = "";

            if(File.Exists(assemblyPath + "\\skate.exe")) {
                executablePath = assemblyPath + "\\skate.exe";
            }
            else {
                MessageBox.Show("Unable to find game executable in " + assemblyPath);
                return;
            }

            var sb = new StringBuilder();

            var launchSettings = new LaunchData();

            if(cbMultiplayer.Checked) {
                launchSettings.MultiplayerEnabled = true;

                launchSettings.PickedFromServerList = !checkBox3.Checked;

                if (checkBox3.Checked) {
                    launchSettings.ServerIP = $"{textBox1.Text}:{textBox2.Text}";
                }
                else {
                    launchSettings.ServerIP = "" // Original domain redacted;
                }

                launchSettings.Username = textBox4.Text;

                launchSettings.DisableWatermark = checkBox2.Checked;
                launchSettings.PresenceEnabled = checkBox1.Checked;
            }
            else {
                launchSettings.MultiplayerEnabled = false;
            }

            //sb.Append("-thinclient 0 -Render.Rc2BridgeEnable 1 -Rc2Bridge. DeviceBackend Rc2BridgeBackend_Vulkan -RenderDevice. RenderCore2Enable 1");

            if (launchSettings.DisableWatermark) { sb.Append(" -DelMarUI.EnableWatermark false"); }
            if (!launchSettings.PresenceEnabled) { sb.Append(" -Online.ClientIsPresenceEnabled false"); }

            if (launchSettings.MultiplayerEnabled) {
                sb.Append($" -   -Client.ServerIp {launchSettings.ServerIP}");
            }

            sb.Append($" -DelMar.LocalPlayerDebugName {launchSettings.Username}");

            if(checkBox5.Checked) {
                sb.Append($" {textBox3.Text}");
            }

            if(checkBox6.Checked) {
                sb.Append(" -WorldRender.ShadowmapsEnable false");
            }

            if(checkBox7.Checked) {
                sb.Append(" -DebugRender true");
            }

            if(checkBox8.Checked) {
                sb.Append(" -UI.DrawEnable false");
            }

            string args = sb.ToString();

            var proc = new Process();
            proc.StartInfo.FileName = executablePath;
            proc.StartInfo.Arguments = args;

            proc.Start();
        }

        private void Form1_Load(object sender, EventArgs e) {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            frm = this;
        }

        private void cbMultiplayer_CheckedChanged(object sender, EventArgs e) {
            //groupBox1.Enabled = cbMultiplayer.Checked;
        }

        static void OnProcessExit(object sender, EventArgs e) {
            //SaveData(frm.GetSaveable());

            Process.Start("taskkill.exe", "/f /im skate.exe /t");
        }

        private Settings GetSaveable() {
            var data = new Settings();

            data.SetUsername = textBox4.Text;
            data.DisableWatermark = checkBox2.Checked;
            data.Multiplayer = cbMultiplayer.Checked;
            data.AdditionalSwitches = textBox3.Text;
            data.UseAdditionalSwitches = checkBox5.Checked;

            return data;
        }
    }
}
