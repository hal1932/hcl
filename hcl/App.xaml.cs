using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using Livet;
using System.IO;

namespace hcl
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon = new System.Windows.Forms.NotifyIcon();
        private KeyHook _keyHook = new KeyHook();

        private Window _mainWindow;
        private double _acceptKeyHookInterval = 200.0;
        private double _lastKeyHookAccepted = 0.0;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Initialize();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Dispose();
        }


        private void Initialize()
        {
            Models.Command.Instance.Initialize(@"dic.txt");

            var menu = new System.Windows.Forms.ContextMenuStrip();
            {
                var configItem = new System.Windows.Forms.ToolStripMenuItem();
                configItem.Text = "設定";
                configItem.Click += configItem_Click;
                menu.Items.Add(configItem);

                var exitItem = new System.Windows.Forms.ToolStripMenuItem();
                exitItem.Text = "終了";
                exitItem.Click += exitItem_Click;
                menu.Items.Add(exitItem);
            }
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.Icon = new System.Drawing.Icon(@"test.ico");

            _keyHook.Hook += _keyHook_Hook;

            _mainWindow = new Views.MainWindow();

            _notifyIcon.Visible = true;
        }

        void _keyHook_Hook(uint vkCode, bool control, bool alt, bool lshift, bool rshift)
        {
            if (vkCode == (uint)System.Windows.Forms.Keys.Space && control)
            {
                var now = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;
                if(now - _lastKeyHookAccepted < _acceptKeyHookInterval)
                {
                    return;
                }
                _lastKeyHookAccepted = now;

                if (Dispatcher.CheckAccess())
                {
                    SwitchMainWindow();
                }
                else
                {
                    DispatcherHelper.BeginInvoke(() => SwitchMainWindow());
                }
            }
        }

        private void SwitchMainWindow()
        {
            if (_mainWindow == null) return;

            if (_mainWindow.IsVisible) _mainWindow.Hide();
            else _mainWindow.Show();
        }
        
        private void Dispose()
        {
            _keyHook.Dispose();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            if (_mainWindow != null)
            {
                _mainWindow.Close();
            }

            base.Shutdown();
        }

        void configItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("hoge");
        }

        void exitItem_Click(object sender, EventArgs e)
        {
            Dispose();
        }


        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO:ロギング処理など
            MessageBox.Show(
                "不明なエラーが発生しました。アプリケーションを終了します。",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(1);
        }
    }
}
