using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Access;
using DiscordStats.Data;
using System.IO;
using System.Threading;

namespace DiscordStats
{
    class Export
    {
        public static void ToExcel(GuildStats stats)
        {
            using (CommonSaveFileDialog dialog = new CommonSaveFileDialog())
            {
                dialog.Filters.Add(new CommonFileDialogFilter("Excel file", ".xlsx"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Ookii.Dialogs.Wpf.TaskDialog progressDialog = new Ookii.Dialogs.Wpf.TaskDialog
                    {
                        Content = $"Exporting to \"{Path.GetFileName(dialog.FileName)}.xlsx\"",
                        AllowDialogCancellation = false,
                        WindowTitle = "Exporting to Excel"
                    };

                    progressDialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Cancel));
                    progressDialog.ProgressBarState = Ookii.Dialogs.Wpf.ProgressBarState.Normal;
                    progressDialog.ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle.MarqueeProgressBar;
                    progressDialog.ProgressBarMaximum = stats.MemberStats.Count() + stats.ChannelStats.Count();

                    new Thread(() =>
                    {
                        try
                        {
                            Application app = new Application();
                            Workbook workbook = app.Workbooks.Add();
                            Worksheet memberSheet = workbook.Sheets.Add();

                            try
                            {
                                App.Current.Dispatcher.Invoke(() => progressDialog.ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle.ProgressBar);

                                memberSheet.Name = "Members";

                                memberSheet.Cells[1, 1] = "Id";
                                memberSheet.Cells[1, 2] = "Username";
                                memberSheet.Cells[1, 3] = "Sent Messages";
                                memberSheet.Cells[1, 4] = "Mentions";
                                memberSheet.Cells[1, 5] = "Avg Messages / Day";
                                memberSheet.Cells[1, 6] = "Last Message";

                                for (int i = 0; i < stats.MemberStats.Count(); i++)
                                {
                                    MemberStats stat = stats.MemberStats.ElementAt(i);
                                    memberSheet.Cells[i + 2, 1] = stat.Id.ToString();
                                    memberSheet.Cells[i + 2, 2] = stat.Username.Trim('=');

                                    memberSheet.Cells[i + 2, 3] = stat.SentMessages;
                                    memberSheet.Cells[i + 2, 4] = stat.Mentions;
                                    memberSheet.Cells[i + 2, 5] = stat.AvgMessagesPerDay;
                                    memberSheet.Cells[i + 2, 6] = stat.LastMessage.ToString();

                                    if ((i % 3) == 0)
                                        App.Current.Dispatcher.Invoke(() => { try { progressDialog.ProgressBarValue = (progressDialog.ProgressBarValue +3); } catch { } });
                                }

                                Worksheet channelsSheet = workbook.Sheets.Add();
                                channelsSheet.Name = "Channels";



                                channelsSheet.Cells[1, 1] = "Id";
                                channelsSheet.Cells[1, 2] = "Name";
                                channelsSheet.Cells[1, 3] = "Message Count";
                                channelsSheet.Cells[1, 4] = "Avg Messages / Day";

                                for (int i = 0; i < stats.ChannelStats.Count(); i++)
                                {
                                    ChannelStats stat = stats.ChannelStats.ElementAt(i);
                                    channelsSheet.Cells[i + 2, 1] = stat.Id.ToString();
                                    channelsSheet.Cells[i + 2, 2] = stat.Name.Trim('=');

                                    channelsSheet.Cells[i + 2, 3] = stat.Messages;
                                    channelsSheet.Cells[i + 2, 4] = stat.AvgMessagesPerDay;

                                    App.Current.Dispatcher.Invoke(() => { try { progressDialog.ProgressBarValue = (progressDialog.ProgressBarValue + 1); } catch { } });
                                }
                            }
                            finally
                            {
                                workbook.SaveAs(dialog.FileName);
                                workbook.Close();
                                app.Quit();

                                App.Current.Dispatcher.Invoke(() => progressDialog.Dispose());
                            }
                        }
                        catch (Exception ex)
                        {
                            MainWindow.ShowTaskDialog($"Something went wrong exporting to Excel and an {ex.GetType().Name} occured.", Ookii.Dialogs.Wpf.TaskDialogIcon.Error);
                            App.Current.Dispatcher.Invoke(() => progressDialog.Dispose());
                        }
                    }).Start();

                    progressDialog.Show();
                }
            }
        }
    }
}