using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace OfficeHelp
{
    public partial class MainForm : Form
    {
        private WebView2 webView;
        private EmailScraper emailScraper;
        private System.Windows.Forms.Timer checkTimer;
        private NotifyIcon notifyIcon;
        private ToolStripStatusLabel statusLabel;
        private Button refreshButton;
        private Button hideButton;

        public MainForm()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Office Help - Outlook 邮件提醒";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.White;

            // 创建顶部工具栏
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15, 10, 15, 10)
            };

            // 标题标签
            Label titleLabel = new Label
            {
                Text = "Office Help",
                Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(15, 17)
            };
            topPanel.Controls.Add(titleLabel);

            // 按钮容器（右对齐）
            Panel buttonPanel = new Panel
            {
                Height = 40,
                Width = 180,
                Location = new Point(topPanel.Width - 195, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            topPanel.Controls.Add(buttonPanel);

            // 刷新按钮 - 现代扁平设计
            refreshButton = CreateModernButton("🔄 刷新", 0, Color.FromArgb(13, 110, 253));
            refreshButton.Click += RefreshButton_Click;
            buttonPanel.Controls.Add(refreshButton);

            // 最小化按钮 - 现代扁平设计
            hideButton = CreateModernButton("━", 95, Color.FromArgb(108, 117, 125));
            hideButton.Font = new Font("Arial", 12F, FontStyle.Bold);
            hideButton.Click += HideButton_Click;
            buttonPanel.Controls.Add(hideButton);

            // 添加分隔线
            Panel separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(222, 226, 230)
            };
            topPanel.Controls.Add(separator);

            this.Controls.Add(topPanel);

            // 创建状态栏
            StatusStrip statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            statusLabel = new ToolStripStatusLabel
            {
                Text = "⚡ 正在初始化...",
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            // 创建 WebView2 控件
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);

            // 系统托盘图标
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = false,
                Text = "Office Help - Outlook 邮件提醒"
            };
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("显示窗口", null, ShowWindow_Click);
            contextMenu.Items.Add("退出", null, Exit_Click);
            notifyIcon.ContextMenuStrip = contextMenu;

            // 定时检查邮件
            checkTimer = new System.Windows.Forms.Timer
            {
                Interval = 60000 // 每分钟检查一次
            };
            checkTimer.Tick += CheckTimer_Tick;

            // 窗口关闭事件
            this.FormClosing += MainForm_FormClosing;
        }

        private Button CreateModernButton(string text, int x, Color backgroundColor)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 0),
                Size = new Size(85, 40),
                BackColor = backgroundColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular)
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backgroundColor, 0.2f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backgroundColor, 0.1f);

            // 添加圆角效果（通过重绘）
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            };

            // 鼠标悬停效果
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = ControlPaint.Light(backgroundColor, 0.2f);
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = backgroundColor;
            };

            return btn;
        }

        private async void InitializeAsync()
        {
            try
            {
                statusLabel.Text = "🔄 正在初始化浏览器...";

                // 初始化 WebView2
                await webView.EnsureCoreWebView2Async(null);

                // 配置 WebView2 设置
                webView.CoreWebView2.Settings.IsScriptEnabled = true;
                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

                // 导航到 Outlook
                webView.CoreWebView2.Navigate("https://outlook.office.com/mail/");

                // 监听导航完成事件
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

                // 初始化邮件抓取器
                emailScraper = new EmailScraper(webView);
                emailScraper.OnNewEmail += EmailScraper_OnNewEmail;
                emailScraper.OnNewReminder += EmailScraper_OnNewReminder;

                // 启动定时器
                checkTimer.Start();

                statusLabel.Text = "✅ 就绪 - 已连接到 Outlook";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "❌ 初始化失败";
            }
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                statusLabel.Text = $"✅ 已加载: {webView.CoreWebView2.Source}";
            }
            else
            {
                statusLabel.Text = "❌ 页面加载失败";
            }
        }

        private async void CheckTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (emailScraper != null)
                {
                    await emailScraper.CheckForUpdatesAsync();
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"⚠️ 检查更新失败: {ex.Message}";
            }
        }

        private void EmailScraper_OnNewEmail(object? sender, EmailEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                NotificationForm notification = new NotificationForm(
                    "新邮件",
                    $"发件人: {e.Sender}\n主题: {e.Subject}",
                    NotificationType.Email
                );
                notification.Show();

                // 显示托盘通知
                if (!this.Visible)
                {
                    notifyIcon.ShowBalloonTip(3000, "新邮件", $"{e.Sender}: {e.Subject}", ToolTipIcon.Info);
                }

                statusLabel.Text = $"📧 收到新邮件: {e.Subject}";
            });
        }

        private void EmailScraper_OnNewReminder(object? sender, ReminderEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                NotificationForm notification = new NotificationForm(
                    "提醒",
                    $"标题: {e.Title}\n时间: {e.Time}",
                    NotificationType.Reminder
                );
                notification.Show();

                // 显示托盘通知
                notifyIcon.ShowBalloonTip(3000, "提醒", $"{e.Title} - {e.Time}", ToolTipIcon.Warning);

                statusLabel.Text = $"⏰ 新提醒: {e.Title}";
            });
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            if (webView?.CoreWebView2 != null)
            {
                webView.CoreWebView2.Reload();
                statusLabel.Text = "🔄 正在刷新...";
            }
        }

        private void HideButton_Click(object? sender, EventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(2000, "Office Help", "程序已最小化到系统托盘", ToolTipIcon.Info);
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void ShowWindow_Click(object? sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show(
                    "是否最小化到系统托盘?\n点击'是'最小化到托盘,点击'否'退出程序",
                    "确认",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.Hide();
                    notifyIcon.Visible = true;
                    notifyIcon.ShowBalloonTip(2000, "Office Help", "程序已最小化到系统托盘", ToolTipIcon.Info);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                checkTimer?.Dispose();
                notifyIcon?.Dispose();
                emailScraper?.Dispose();
                webView?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
