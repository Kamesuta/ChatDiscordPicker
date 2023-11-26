using ChatPresetTool;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatDiscordPicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Regex _regex = new Regex(@"^Minecraft\*? 1\.\d+?(?:\.\d+?)?(.*)$", RegexOptions.Compiled);
        private readonly Stopwatch _timer = Stopwatch.StartNew();
        private IntPtr minecraftWindowHandle;

        public MainWindow()
        {
            InitializeComponent();

            GlobalHook.EnableHook();
            GlobalHook.KeyDownEvent += OnKeyPress;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // 画面サイズ読み込み
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;

            // URL読み込み
            widgetUrl.Text = Properties.Settings.Default.Url;
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            GlobalHook.DisableHook();

            // 画面サイズ/位置を保存
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Width = this.Width;

            // URLを保存
            Properties.Settings.Default.Url = widgetUrl.Text;

            // 保存
            Properties.Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private enum Keys : byte
        {
            VK_RETURN = 0x0D,
            VK_ESCAPE = 0x1B,
            VK_T = 0x54,
            VK_V = 0x56,
            VK_LCONTROL = 0xA2,
        }

        private void OnKeyPress(int vkCode)
        {
            // その他キー
            if (vkCode != (byte)Keys.VK_T && vkCode != (byte)Keys.VK_RETURN && vkCode != (byte)Keys.VK_ESCAPE)
            {
                return;
            }

            // マイクラのウィンドウか判定
            IntPtr handle = ActiveWindow.GetActiveWindow();
            string className = ActiveWindow.GetClassName(handle);
            if (className != "LWJGL" && className != "GLFW30")
            {
                return;
            }

            string title = ActiveWindow.GetWindowTitle(handle);
            if (!_regex.IsMatch(title))
            {
                return;
            }

            // Tが押されたら
            if (vkCode == (byte)Keys.VK_T)
            {
                // マイクラのウィンドウを記憶
                minecraftWindowHandle = ActiveWindow.GetActiveWindow();

                // オーバーレイモード開始
                StartOverlayMode();
            }
            else if (vkCode == (byte)Keys.VK_RETURN || vkCode == (byte)Keys.VK_ESCAPE)
            {
                // 背景色を透明化
                backImage.Opacity = 0.0;
                mainBorder.BorderBrush = null;
            }
        }

        // オーバーレイモード開始
        private void StartOverlayMode()
        {
            // 背景色の透明解除
            backImage.Opacity = 0.01;
            mainBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));

            // チャット画面を表示
            if (webView.Visibility == Visibility.Hidden)
            {
                try
                {
                    webView.Source = new Uri(widgetUrl.Text);
                    webView.Visibility = Visibility.Visible;
                    webSetup.Visibility = Visibility.Hidden;
                }
                catch
                {
                    // URLが不正
                }
            }
        }

        // マイクラにチャットを送信
        private async Task SendTextToMinecraft(string text)
        {
            // 入力
            if (text.Length > 0)
            {
                try
                {
                    await Task.Delay(10);

                    //using var backup = new ClipboardBackup();
                    Clipboard.SetText(text);

                    await Task.Delay(10);

                    KeySimulator.ReleaseKey((byte)Keys.VK_LCONTROL);
                    KeySimulator.ReleaseKey((byte)Keys.VK_V);
                    await Task.Delay(10);
                    KeySimulator.PressKey((byte)Keys.VK_LCONTROL);
                    KeySimulator.PressKey((byte)Keys.VK_V);
                    await Task.Delay(10);
                    KeySimulator.ReleaseKey((byte)Keys.VK_LCONTROL);
                    KeySimulator.ReleaseKey((byte)Keys.VK_V);
                }
                catch (COMException)
                {
                    // コピペの競合
                }
            }
        }

        // WebViwe2読み込み時
        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            // ページ読み込み完了時の処理
            webView.NavigationCompleted += WebView_NavigationCompleted;
            //Webページからのメッセージ受信時の処理
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        // 短い間隔を弾く
        private bool IsCooldown()
        {
            if (_timer.Elapsed.Milliseconds < 30)
            {
                return true;
            }

            _timer.Restart();
            return false;
        }

        // JavaScriptからのコマンド
        public class JSCommand
        {
            public string? Type { get; set; }
            public string? Data { get; set; }
        }

        //Webページからのメッセージ受信時の処理
        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // 短い間隔を弾く
            if (IsCooldown())
            {
                return;
            }

            // 送信文字を取得。今回はURLが格納されている
            string command = e.TryGetWebMessageAsString();
            if (command == null) return;

            // JSONをパース
            var jsCommand = JsonSerializer.Deserialize<JSCommand>(command);
            if (jsCommand == null) return;

            // コマンドを実行
            switch (jsCommand.Type)
            {
                case "chat":
                    {
                        // マイクラに送信
                        if (minecraftWindowHandle != IntPtr.Zero && jsCommand.Data != null)
                        {
                            ActiveWindow.SetActiveWindow(minecraftWindowHandle);
                            await SendTextToMinecraft(jsCommand.Data);
                        }
                    }
                    break;
            }
        }

        async void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // CSSを適用するためのJavaScriptコードを作成する
            string script = @"
                // CSS追加
                var style = document.createElement('style');
                style.innerHTML = `
                    div[class^=Chat_chatContainer] {
                        width: auto;
                        height: 100vh;
                        display: flex;
                        flex-direction: column;
                    }

                    div[class^=Chat_channelName] {
                        display: none;
                    }

                    ul[class^=Chat_messages] {
                        height: 100% !important;
                    }

                    li[class^=Chat_message]:hover {
                        background-color: rgba(0, 0, 0, 0.5);
                    }
                `;
                document.head.appendChild(style);

                // メッセージをクリックしたときの処理
                document.querySelector('#root').onclick = e =>
                {
                    // メッセージDOMまで辿って取得
                    const message = e.target.closest('li[class^=Chat_message]');
                    // メッセージDOMが取得できなかった場合は処理を終了
                    if (!message) return;

                    // メッセージDOMからメッセージテキストDOMを取得
                    const messageText = message.querySelector('span[class^=Chat_messageText]');
                    // 出力
                    console.log('マイクラに送信: ' + messageText.textContent);
                    window.chrome.webview.postMessage(JSON.stringify({Type:'chat',Data:messageText.textContent}));
                };
            ";

            // ExecuteScriptAsyncメソッドでJavaScriptコードを実行する
            await webView.ExecuteScriptAsync(script);
        }

        // ボーダードラッグ
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            var url = link.NavigateUri.AbsoluteUri.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void StartForce_Click(object sender, RoutedEventArgs e)
        {
            StartOverlayMode();
        }
    }
}
