using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EDIFileFilteringApp
{
    public partial class Form1 : Form
    {
        // Global Tanımlamalar
        private DataTable dataTable = new DataTable();
        private DataGridView dataGridView1;
        private TextBox ediTypeTextBox, senderTextBox, receiverTextBox, yearTextBox;
        private Button filterButton, clearButton;
        private Label ediTypeLabel, senderLabel, receiverLabel, yearLabel;

        public Form1()
        {
            // Bileşenleri Başlat ve Düzeni Oluştur
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            // Form Ayarları
            this.Text = "EDI File Filtering";
            this.Size = new Size(1200, 800);  // Form boyutu genişletildi
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // DataGridView
            dataGridView1 = new DataGridView
            {
                Width = 1150,
                Height = 500,
                Top = 20,
                Left = 20,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Font = new Font("Arial", 10),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    BackColor = Color.LightGray
                }
            };
            this.Controls.Add(dataGridView1);

            // EDI Type Label ve TextBox
            ediTypeLabel = new Label { Text = "EDI Type:", Top = 550, Left = 20, Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(ediTypeLabel);
            ediTypeTextBox = new TextBox { Top = 570, Left = 20, Width = 200 };
            this.Controls.Add(ediTypeTextBox);

            // Sender Label ve TextBox
            senderLabel = new Label { Text = "Sender:", Top = 550, Left = 250, Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(senderLabel);
            senderTextBox = new TextBox { Top = 570, Left = 250, Width = 200 };
            this.Controls.Add(senderTextBox);

            // Receiver Label ve TextBox
            receiverLabel = new Label { Text = "Receiver:", Top = 550, Left = 480, Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(receiverLabel);
            receiverTextBox = new TextBox { Top = 570, Left = 480, Width = 200 };
            this.Controls.Add(receiverTextBox);

            // Year Label ve TextBox
            yearLabel = new Label { Text = "Year:", Top = 550, Left = 710, Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(yearLabel);
            yearTextBox = new TextBox { Top = 570, Left = 710, Width = 100 };
            this.Controls.Add(yearTextBox);

            // Filter Button
            filterButton = new Button { Text = "Filter", Top = 620, Left = 850, Width = 100, Height = 40, BackColor = Color.LightBlue };
            filterButton.Click += new EventHandler(filterButton_Click);
            this.Controls.Add(filterButton);

            // Clear Button
            clearButton = new Button { Text = "Clear", Top = 620, Left = 980, Width = 100, Height = 40, BackColor = Color.LightCoral };
            clearButton.Click += new EventHandler(clearButton_Click);
            this.Controls.Add(clearButton);
        }

        private void LoadData()
        {
            string folderPath = @"C:\Users\DELL\OneDrive\Masaüstü\EDI Dosyaları";

            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath).Where(f => !f.Contains("desktop.ini")).ToList();

                dataTable.Clear();
                dataTable.Columns.Clear();
                dataTable.Columns.Add("File Name", typeof(string));
                dataTable.Columns.Add("Modification Time", typeof(string));
                dataTable.Columns.Add("EDI Type", typeof(string));
                dataTable.Columns.Add("Sender", typeof(string));
                dataTable.Columns.Add("Receiver", typeof(string));

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var modificationTime = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm:ss");

                    string ediType = "Unknown";
                    if (fileName.Contains("DELINS")) ediType = "DELINS";
                    else if (fileName.Contains("DELFOR")) ediType = "DELFOR";
                    else if (fileName.Contains("DELJIT")) ediType = "DELJIT";
                    else if (fileName.Contains("VDA4905")) ediType = "VDA4905";
                    else if (fileName.Contains("VDA4915")) ediType = "VDA4915";

                    string sender = "Sender not found";
                    string receiver = "Receiver not found";

                    var lines = File.ReadAllLines(file);
                    foreach (var line in lines)
                    {
                        var segments = line.Split('+');

                        if (ediType == "DELINS" && segments.Length > 3)
                        {
                            sender = segments[2].Trim();
                            receiver = segments[3].Trim();
                            break;
                        }
                        else if (ediType == "DELFOR" && segments[0] == "NAD")
                        {
                            if (segments[1] == "BY")
                                receiver = segments.Length > 4 ? segments[4].Trim() : "Receiver missing";
                            else if (segments[1] == "SE")
                                sender = segments.Length > 4 ? segments[4].Trim() : "Sender missing";
                        }
                        else if (ediType == "DELJIT" && line.StartsWith("UNB"))
                        {
                            var unbSegments = line.Split('+');
                            if (unbSegments.Length > 3)
                            {
                                sender = unbSegments[2].Split(':')[0].Trim();
                                receiver = unbSegments[3].Split(':')[0].Trim();
                                break;
                            }
                        }
                        else if ((ediType == "VDA4905" || ediType == "VDA4915"))
                        {
                            if (line.Length >= 23)
                            {
                                sender = line.Substring(5, 9).Trim();
                                receiver = line.Substring(14, 9).Trim();
                                if (!string.IsNullOrEmpty(sender) || !string.IsNullOrEmpty(receiver))
                                    break;
                            }
                        }
                    }

                    dataTable.Rows.Add(fileName, modificationTime, ediType, sender, receiver);
                }

                dataGridView1.DataSource = dataTable;
                dataGridView1.AutoResizeColumns();
                dataGridView1.Refresh();
            }
            else
            {
                MessageBox.Show("Folder path not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void filterButton_Click(object sender, EventArgs e)
        {
            var filteredRows = dataTable.AsEnumerable().Where(row =>
                (string.IsNullOrEmpty(ediTypeTextBox.Text) || row.Field<string>("EDI Type").IndexOf(ediTypeTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrEmpty(senderTextBox.Text) || row.Field<string>("Sender").IndexOf(senderTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrEmpty(receiverTextBox.Text) || row.Field<string>("Receiver").IndexOf(receiverTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrEmpty(yearTextBox.Text) || row.Field<string>("Modification Time").Contains(yearTextBox.Text))
            );

            dataGridView1.DataSource = filteredRows.Any() ? filteredRows.CopyToDataTable() : null;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            ediTypeTextBox.Text = string.Empty;
            senderTextBox.Text = string.Empty;
            receiverTextBox.Text = string.Empty;
            yearTextBox.Text = string.Empty;
            dataGridView1.DataSource = dataTable;
        }
    }
}
