# Office Help - Outlook 邮件提醒程序

这是一个基于 C# WinForm 的桌面应用程序，用于监控 Outlook 网页版邮件和提醒，并通过桌面通知窗口提醒用户。

## 功能特性

- ✉️ **邮件监控**: 自动检测新邮件并显示桌面通知
- ⏰ **提醒监控**: 检测 Outlook 提醒事项并弹出通知
- 🌐 **WebView2 集成**: 使用 Microsoft Edge WebView2 浏览器控件
- 🔔 **桌面通知**: 美观的桌面通知窗口，支持淡入淡出动画
- 📌 **系统托盘**: 最小化到系统托盘，不占用任务栏空间
- 🔄 **自动刷新**: 每分钟自动检查新邮件和提醒
- 🎨 **现代化界面**: 简洁美观的用户界面

## 系统要求

- Windows 10 或更高版本
- .NET 6.0 或更高版本
- Microsoft Edge WebView2 Runtime（首次运行会自动提示安装）

## 安装与运行

### 方法一：从源码构建

1. 克隆或下载项目代码
2. 安装 .NET 6.0 SDK
3. 在项目目录下执行：

```bash
dotnet restore
dotnet build
dotnet run
```

### 方法二：发布为独立程序

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

生成的可执行文件位于 `bin/Release/net6.0-windows/win-x64/publish/` 目录下。

## 使用说明

1. **首次登录**
   - 启动程序后会自动打开 Outlook 网页版
   - 使用你的 Microsoft 账户登录
   - 登录信息会被 WebView2 保存，下次启动无需重新登录

2. **查看邮件**
   - 程序会在主窗口中显示完整的 Outlook 界面
   - 你可以像使用网页版一样操作

3. **接收通知**
   - 当有新邮件时，会弹出桌面通知显示发件人和主题
   - 当有提醒事项时，会弹出提醒通知
   - 通知窗口会在 5 秒后自动关闭

4. **最小化到托盘**
   - 点击"最小化到托盘"按钮或关闭窗口时选择最小化
   - 双击系统托盘图标可恢复窗口
   - 右键托盘图标可以显示窗口或退出程序

5. **刷新页面**
   - 点击"刷新"按钮重新加载 Outlook 页面

## 项目结构

```
OfficeHelp/
├── Program.cs              # 程序入口
├── MainForm.cs             # 主窗体（包含 WebView2 和托盘功能）
├── EmailScraper.cs         # 邮件和提醒抓取逻辑
├── NotificationForm.cs     # 桌面通知窗口
├── OfficeHelp.csproj       # 项目配置文件
└── README.md               # 项目说明文档
```

## 技术实现

### WebView2 集成

使用 Microsoft Edge WebView2 控件嵌入 Chromium 浏览器内核，提供完整的网页浏览体验。

### JavaScript 注入

通过 `ExecuteScriptAsync` 方法向页面注入 JavaScript 代码，抓取邮件列表和提醒信息：

- 使用 DOM 选择器查找未读邮件元素
- 提取发件人、主题、时间等信息
- 监测提醒弹窗和提醒列表

### 数据去重

使用 `HashSet` 存储已处理的邮件和提醒 ID，避免重复通知。

### 桌面通知

自定义 Form 实现桌面通知窗口：
- 无边框、置顶窗口
- 淡入淡出动画效果
- 自动定位到屏幕右下角
- 根据通知类型使用不同颜色

## 注意事项

1. **登录状态**: 首次使用需要登录 Microsoft 账户，登录信息会被保存
2. **网络连接**: 需要稳定的网络连接才能访问 Outlook
3. **检查频率**: 默认每分钟检查一次，可以根据需要修改 `checkTimer.Interval`
4. **页面变化**: Outlook 网页版可能会更新，导致选择器失效，需要更新 JavaScript 代码

## 自定义配置

### 修改检查间隔

在 `MainForm.cs` 中修改定时器间隔：

```csharp
checkTimer = new System.Windows.Forms.Timer
{
    Interval = 60000 // 毫秒，60000 = 1分钟
};
```

### 修改通知显示时间

在 `NotificationForm.cs` 中修改自动关闭时间：

```csharp
closeTimer = new System.Windows.Forms.Timer
{
    Interval = 5000 // 毫秒，5000 = 5秒
};
```

### 修改通知窗口样式

在 `NotificationForm.cs` 的 `InitializeComponent` 方法中修改窗口大小、颜色、字体等。

## 故障排除

### WebView2 初始化失败

- 确保已安装 Microsoft Edge WebView2 Runtime
- 从官方下载：https://developer.microsoft.com/microsoft-edge/webview2/

### 无法检测到邮件

- 检查网络连接
- 确保已成功登录 Outlook
- 打开浏览器控制台（F12）查看 JavaScript 是否执行成功
- Outlook 页面结构可能已更新，需要调整选择器

### 通知不显示

- 检查 Windows 通知设置
- 确保程序有通知权限
- 查看事件订阅是否正确绑定

## 开发计划

- [ ] 添加声音提醒
- [ ] 支持自定义通知样式
- [ ] 添加邮件快捷操作（标记已读、删除等）
- [ ] 支持多账户切换
- [ ] 添加配置文件保存用户设置
- [ ] 优化内存占用

## 许可证

本项目仅供学习和个人使用。

## 贡献

欢迎提交 Issue 和 Pull Request！

## 联系方式

如有问题或建议，请创建 Issue。
