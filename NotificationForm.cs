using System;
using System.Drawing;
using System.Windows.Forms;

namespace OfficeHelp
{
    /// <summary>
    /// 通知类型
    /// </summary>
    public enum NotificationType
    {
        Email,
        Reminder,
        Info
    }

    /// <summary>
    /// 桌面通知窗口
    /// </summary>
    public partial class NotificationForm : Form
    {
        private System.Windows.Forms.Timer fadeTimer;
        private System.Windows.Forms.Timer closeTimer;
        private double opacity = 0.0;
        private bool isClosing = false;

        private Label titleLabel;
        private Label messageLabel;
        private Button closeButton;

        public NotificationForm(string title, string message, NotificationType type)
        {
            InitializeComponent(title, message, type);
            SetupAnimation();
        }

        private void InitializeComponent(string title, string message, NotificationType type)
        {
            // 窗体设置
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(350, 120);
            this.BackColor = Color.White;
            this.Opacity = 0;

            // 定位到屏幕右下角
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                workingArea.Right - this.Width - 10,
                workingArea.Bottom - this.Height - 10
            );

            // 根据通知类型设置颜色
            Color accentColor;
            switch (type)
            {
                case NotificationType.Email:
                    accentColor = Color.FromArgb(0, 120, 215); // 蓝色
                    break;
                case NotificationType.Reminder:
                    accentColor = Color.FromArgb(255, 140, 0); // 橙色
                    break;
                default:
                    accentColor = Color.FromArgb(0, 150, 136); // 青色
                    break;
            }

            // 顶部彩色条
            Panel accentPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor = accentColor
            };
            this.Controls.Add(accentPanel);

            // 主面板
            Panel mainPanel = new Panel
            {
                Location = new Point(0, 5),
                Size = new Size(350, 115),
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // 标题
            titleLabel = new Label
            {
                Text = title,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            mainPanel.Controls.Add(titleLabel);

            // 消息内容
            messageLabel = new Label
            {
                Text = message,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(15, 45),
                Size = new Size(300, 50),
                AutoEllipsis = true
            };
            mainPanel.Controls.Add(messageLabel);

            // 关闭按钮
            closeButton = new Button
            {
                Text = "✕",
                Font = new Font("Arial", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Size = new Size(25, 25),
                Location = new Point(315, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += CloseButton_Click;
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.Red;
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Color.Gray;
            mainPanel.Controls.Add(closeButton);

            this.Controls.Add(mainPanel);

            // 添加阴影效果 (模拟)
            this.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
                    Color.FromArgb(200, 200, 200), ButtonBorderStyle.Solid);
            };

            // 点击窗体任意位置关闭
            this.Click += (s, e) => Close();
            mainPanel.Click += (s, e) => Close();
            titleLabel.Click += (s, e) => Close();
            messageLabel.Click += (s, e) => Close();
        }

        private void SetupAnimation()
        {
            // 淡入动画
            fadeTimer = new System.Windows.Forms.Timer
            {
                Interval = 20
            };
            fadeTimer.Tick += FadeTimer_Tick;
            fadeTimer.Start();

            // 自动关闭定时器（5秒后）
            closeTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                StartClosing();
            };
            closeTimer.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (!isClosing)
            {
                // 淡入
                if (opacity < 1.0)
                {
                    opacity += 0.05;
                    this.Opacity = opacity;
                }
                else
                {
                    fadeTimer?.Stop();
                }
            }
            else
            {
                // 淡出
                if (opacity > 0)
                {
                    opacity -= 0.05;
                    this.Opacity = opacity;
                }
                else
                {
                    fadeTimer?.Stop();
                    this.Close();
                }
            }
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            StartClosing();
        }

        private void StartClosing()
        {
            isClosing = true;
            closeTimer?.Stop();
            if (fadeTimer == null || !fadeTimer.Enabled)
            {
                fadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
                fadeTimer.Tick += FadeTimer_Tick;
                fadeTimer.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadeTimer?.Dispose();
                closeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // 防止窗口获得焦点
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return cp;
            }
        }
    }
}
