using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hcl.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Models.PresetCommand _preset = new Models.PresetCommand();

        public MainWindow()
        {
            InitializeComponent();
            _preset.Initialize();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            _textBox.Text = "";
            Keyboard.Focus(_textBox);
        }

        private void _textBox_KeyUp(object sender, KeyEventArgs e)
        {
            _textBox.Text = _textBox.Text.TrimStart();

            if (e.Key == Key.Enter)
            {
                if (_textBox.Text.Length > 0)
                {
                    var items = _textBox.Text.Trim().Split(' ');
                    var command = items[0];
                    var args = items.Skip(1).ToArray();

                    var type = _preset.GetCommandType(command);
                    if (type != Models.PresetCommand.Type.None)
                    {
                        ExecutePresetCommand(type, args);
                    }
                    else
                    {
                        Models.Command.Instance.Execute(command, args);
                    }

                    if (Dispatcher.CheckAccess()) this.Hide();
                    else Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate() { this.Hide(); });
                }
            }
        }

        private void ExecutePresetCommand(Models.PresetCommand.Type type, string[] args)
        {
            switch (type)
            {
                case Models.PresetCommand.Type.SwitchEnvSet:
                    new Models.EnvSet(args[0]);
                    break;

                case Models.PresetCommand.Type.EditConfig:
                    new EditConfigWindow().Show();
                    break;

                default:
                    return;
            }
        }
    }
}