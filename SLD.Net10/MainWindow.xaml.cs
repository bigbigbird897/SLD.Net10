using System.ComponentModel;
using System.Windows;

namespace SLD.Net10
{
    public partial class MainWindow : Window
    {
        // Web服务监听地址，和appsettings.json的Urls保持一致
        private const string SwaggerUrl = "https://localhost:9666/swagger/index.html";

        //private const string SwaggerUrl = "https://localhost:9666/";

        // 托盘实例
        private NotifyIcon _trayIcon;

        // 托盘右键菜单
        private readonly ContextMenuStrip _trayMenu;

        public MainWindow()
        {
            InitializeComponent();
            LoadSwaggerPage();

            // 初始化托盘菜单
            _trayMenu = new ContextMenuStrip();
            InitTrayIcon();

            // 窗口最小化事件
            this.StateChanged += Window_StateChanged;
        }

        /// <summary>
        /// WebView2初始化并加载本地Swagger
        /// </summary>
        private async void LoadSwaggerPage()
        {
            // 初始化WebView2运行环境
            await WebViewSwagger.EnsureCoreWebView2Async(null);
            // 加载swagger页面
            WebViewSwagger.CoreWebView2.Navigate(SwaggerUrl);
        }

        #region 托盘初始化（修复ContextMenu报错，使用ContextMenuStrip）

        private void InitTrayIcon()
        {
            // 菜单条目
            var itemShow = new ToolStripMenuItem("显示Swagger窗口");
            var itemExit = new ToolStripMenuItem("彻底退出程序");

            // 绑定点击事件
            itemShow.Click += (s, e) => RestoreWindow();
            itemExit.Click += TrayMenu_ExitApp;

            // 添加到菜单
            _trayMenu.Items.Add(itemShow);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(itemExit);

            // 托盘配置
            _trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(@"Image\app.ico"),
                Text = "SLD.Net10 API后台服务",
                Visible = true,
                // 关键：赋值 ContextMenuStrip，不再使用ContextMenu
                ContextMenuStrip = _trayMenu
            };

            // 双击托盘图标恢复窗口
            _trayIcon.DoubleClick += TrayIcon_DoubleClick;
        }

        #endregion 托盘初始化（修复ContextMenu报错，使用ContextMenuStrip）

        #region 窗口最小化/关闭拦截

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                _trayIcon.ShowBalloonTip(1000, "提示", "程序已后台托盘常驻，API持续运行", ToolTipIcon.Info);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            _trayIcon.ShowBalloonTip(1000, "提示", "程序已最小化至系统托盘", ToolTipIcon.Info);
        }

        #endregion 窗口最小化/关闭拦截

        #region 托盘事件

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void TrayMenu_ExitApp(object sender, EventArgs e)
        {
            // 释放托盘资源
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayMenu.Dispose();
            // 退出应用
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 恢复窗口
        /// </summary>
        private void RestoreWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        #endregion 托盘事件

        #region 释放资源

        protected override void OnClosed(EventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            _trayMenu?.Dispose();
            base.OnClosed(e);
        }

        #endregion 释放资源
    }
}