using DiscordStats.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace DiscordStats
{
    /// <summary>
    /// Interaction logic for ResultsPage.xaml
    /// </summary>
    public partial class ResultsPage : Page
    {
        public ResultsPage(GuildStats stats)
        {
            InitializeComponent();
            DataContext = stats;

            if (stats.MemberStats.Any())
            {
                Dictionary<string, long> dictionary = new Dictionary<string, long>();
                foreach (var item in stats.MemberStats)
                {
                    dictionary.Add(item.Username, item.SentMessages);
                }
                
                ((PieSeries)leaderboardsPie.Series[0]).ItemsSource = dictionary.OrderByDescending(t => t.Value);
            }
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            switch (formatComboBox.SelectedIndex)
            {
                case 0:
                    Export.ToExcel(DataContext as GuildStats);
                    break;
                case 1:
                    MainWindow.ShowTaskDialog("Sorry! This feature is not yet implemented.", TaskDialogIcon.Information);
                    break;
                case 2:
                    using (CommonSaveFileDialog dialog = new CommonSaveFileDialog())
                    {
                        dialog.Filters.Add(new CommonFileDialogFilter("XML File", ".xml"));
                        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            using (FileStream str = File.Create(dialog.FileName))
                            using (StreamWriter stringwriter = new StreamWriter(str))
                            {
                                XmlSerializer serializer = new XmlSerializer(DataContext.GetType());
                                serializer.Serialize(stringwriter, DataContext);
                            }
                        }
                    }
                    break;
                case 3:
                    using (CommonSaveFileDialog dialog = new CommonSaveFileDialog())
                    {
                        dialog.Filters.Add(new CommonFileDialogFilter("JSON File", ".json"));
                        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(DataContext, Formatting.Indented));
                        }
                    }
                    break;
                case 4:
                    break;
                default:
                    break;
            }
        }

        private void grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
