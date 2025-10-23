using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;

namespace OfficeHelp
{
    /// <summary>
    /// 邮件和提醒抓取器
    /// </summary>
    public class EmailScraper : IDisposable
    {
        private readonly WebView2 webView;
        private HashSet<string> processedEmailIds = new HashSet<string>();
        private HashSet<string> processedReminderIds = new HashSet<string>();

        public event EventHandler<EmailEventArgs>? OnNewEmail;
        public event EventHandler<ReminderEventArgs>? OnNewReminder;

        public EmailScraper(WebView2 webView)
        {
            this.webView = webView;
        }

        /// <summary>
        /// 检查新邮件和提醒
        /// </summary>
        public async Task CheckForUpdatesAsync()
        {
            if (webView?.CoreWebView2 == null)
                return;

            try
            {
                // 检查未读邮件
                await CheckUnreadEmailsAsync();

                // 检查提醒
                await CheckRemindersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查更新时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查未读邮件
        /// </summary>
        private async Task CheckUnreadEmailsAsync()
        {
            try
            {
                // JavaScript 代码来获取未读邮件信息
                string script = @"
                    (function() {
                        try {
                            const emails = [];

                            // 尝试多种选择器以适应不同的 Outlook 版本
                            const selectors = [
                                'div[role=""listitem""][aria-label*=""未读""]',
                                'div[role=""row""][aria-label*=""未读""]',
                                'div.customScrollBar div[draggable=""true""]',
                                '[data-convid]'
                            ];

                            let messageElements = [];
                            for (let selector of selectors) {
                                const elements = document.querySelectorAll(selector);
                                if (elements.length > 0) {
                                    messageElements = Array.from(elements);
                                    break;
                                }
                            }

                            // 获取未读邮件列表项
                            const unreadItems = Array.from(document.querySelectorAll('[aria-label*=""未读""]')).slice(0, 5);

                            unreadItems.forEach((item, index) => {
                                try {
                                    const ariaLabel = item.getAttribute('aria-label') || '';

                                    // 尝试从 aria-label 中提取信息
                                    let sender = '';
                                    let subject = '';
                                    let time = '';

                                    // 查找发件人
                                    const senderElement = item.querySelector('[title]');
                                    if (senderElement) {
                                        sender = senderElement.getAttribute('title') || senderElement.textContent || '';
                                    }

                                    // 查找主题
                                    const subjectElement = item.querySelector('[id*=""Subject""]') ||
                                                          item.querySelector('span[title]');
                                    if (subjectElement) {
                                        subject = subjectElement.textContent || subjectElement.getAttribute('title') || '';
                                    }

                                    // 查找时间
                                    const timeElement = item.querySelector('[id*=""Time""]') ||
                                                       item.querySelector('span[class*=""time""]');
                                    if (timeElement) {
                                        time = timeElement.textContent || '';
                                    }

                                    // 生成唯一ID
                                    const id = item.getAttribute('data-convid') ||
                                              item.id ||
                                              `email_${sender}_${subject}_${index}`.replace(/\s/g, '_');

                                    if (sender || subject) {
                                        emails.push({
                                            id: id,
                                            sender: sender.trim(),
                                            subject: subject.trim(),
                                            time: time.trim(),
                                            isUnread: ariaLabel.includes('未读') || item.className.includes('unread')
                                        });
                                    }
                                } catch (e) {
                                    console.error('解析邮件项时出错:', e);
                                }
                            });

                            return JSON.stringify(emails);
                        } catch (error) {
                            return JSON.stringify({ error: error.message });
                        }
                    })();
                ";

                string result = await webView.CoreWebView2.ExecuteScriptAsync(script);

                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    // 移除 JSON 字符串外的引号
                    result = result.Trim('"').Replace("\\\"", "\"").Replace("\\n", "").Replace("\\r", "");

                    var emails = JsonConvert.DeserializeObject<List<EmailInfo>>(result);

                    if (emails != null)
                    {
                        foreach (var email in emails)
                        {
                            if (!processedEmailIds.Contains(email.Id))
                            {
                                processedEmailIds.Add(email.Id);

                                OnNewEmail?.Invoke(this, new EmailEventArgs
                                {
                                    Sender = email.Sender,
                                    Subject = email.Subject,
                                    Time = email.Time,
                                    IsUnread = email.IsUnread
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查邮件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查提醒
        /// </summary>
        private async Task CheckRemindersAsync()
        {
            try
            {
                // JavaScript 代码来获取提醒信息
                string script = @"
                    (function() {
                        try {
                            const reminders = [];

                            // 查找提醒弹窗或提醒列表
                            const reminderSelectors = [
                                '[role=""dialog""][aria-label*=""提醒""]',
                                '[class*=""reminder""]',
                                '[data-reminder-id]',
                                '.ms-Dialog[aria-label*=""提醒""]'
                            ];

                            let reminderElements = [];
                            for (let selector of reminderSelectors) {
                                const elements = document.querySelectorAll(selector);
                                if (elements.length > 0) {
                                    reminderElements = Array.from(elements);
                                    break;
                                }
                            }

                            reminderElements.forEach((item, index) => {
                                try {
                                    const title = item.querySelector('[class*=""title""]')?.textContent ||
                                                 item.querySelector('h2')?.textContent ||
                                                 item.getAttribute('aria-label') || '';

                                    const time = item.querySelector('[class*=""time""]')?.textContent ||
                                                item.querySelector('[class*=""date""]')?.textContent || '';

                                    const id = item.getAttribute('data-reminder-id') ||
                                              item.id ||
                                              `reminder_${title}_${index}`.replace(/\s/g, '_');

                                    if (title) {
                                        reminders.push({
                                            id: id,
                                            title: title.trim(),
                                            time: time.trim()
                                        });
                                    }
                                } catch (e) {
                                    console.error('解析提醒项时出错:', e);
                                }
                            });

                            return JSON.stringify(reminders);
                        } catch (error) {
                            return JSON.stringify({ error: error.message });
                        }
                    })();
                ";

                string result = await webView.CoreWebView2.ExecuteScriptAsync(script);

                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    result = result.Trim('"').Replace("\\\"", "\"").Replace("\\n", "").Replace("\\r", "");

                    var reminders = JsonConvert.DeserializeObject<List<ReminderInfo>>(result);

                    if (reminders != null)
                    {
                        foreach (var reminder in reminders)
                        {
                            if (!processedReminderIds.Contains(reminder.Id))
                            {
                                processedReminderIds.Add(reminder.Id);

                                OnNewReminder?.Invoke(this, new ReminderEventArgs
                                {
                                    Title = reminder.Title,
                                    Time = reminder.Time
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查提醒时出错: {ex.Message}");
            }
        }

        public void Dispose()
        {
            processedEmailIds.Clear();
            processedReminderIds.Clear();
        }

        // 内部类用于 JSON 反序列化
        private class EmailInfo
        {
            public string Id { get; set; } = "";
            public string Sender { get; set; } = "";
            public string Subject { get; set; } = "";
            public string Time { get; set; } = "";
            public bool IsUnread { get; set; }
        }

        private class ReminderInfo
        {
            public string Id { get; set; } = "";
            public string Title { get; set; } = "";
            public string Time { get; set; } = "";
        }
    }

    /// <summary>
    /// 邮件事件参数
    /// </summary>
    public class EmailEventArgs : EventArgs
    {
        public string Sender { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Time { get; set; } = "";
        public bool IsUnread { get; set; }
    }

    /// <summary>
    /// 提醒事件参数
    /// </summary>
    public class ReminderEventArgs : EventArgs
    {
        public string Title { get; set; } = "";
        public string Time { get; set; } = "";
    }
}
