using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace El_Hassab    // TO CONSIDER :          //  SCROLL - MENUSTRIP - EXPORTING 
{
    public partial class Form1 : Form
    {
        private SQLiteConnection connection;
        private SQLiteDataAdapter adapter;
        private SQLiteCommandBuilder commandBuilder;
        private SQLiteCommand cmd;
        private DataTable dataTable;
        private DataTable backupdataTable;
        private DataView view = new DataView();
        private readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trial.db");
        private int selectedRow;

        public Form1()
        {
            InitializeComponent();
            Init();
            comboBox3.Items.AddRange(new string[] { "Ascending", "Descending", "Amount", "Completed", "Uncompleted" });
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            AdjustControlSizes();
        }

        private void AdjustControlSizes()
        {

            dataGridView1.Width = 3060;
            dataGridView1.Height = 1550;
            dataGridView1.Location = new Point(1130, 178);
        }

        private void ConInit()
        {
            
            string query = "SELECT * FROM t1";            
            try
            {
                connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                connection.Open();
                adapter = new SQLiteDataAdapter(query, connection);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
                connection.Close();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error_log.txt", ex.ToString());
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void CreateBackup()
        {
            if (dataTable != null)
            {
                backupdataTable= dataTable.Copy();
            }
        }
        private void UndoChanges()
        {
            if (backupdataTable != null)
            {
                dataTable = backupdataTable.Copy();
                dataGridView1.DataSource = dataTable;
                UpdateRowCountLabel(dataGridView1.RowCount);
            }
        }
        private void DGV()
        {   ReplaceTextColumnWithCheckBox(2);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Cambria", 16, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            comboBox3.SelectedIndexChanged += ComboBox3_SelectedIndexChanged;
            textBox5.KeyDown += TextBox5_KeyDown;
            button3.Font = new Font("Segoe UI Symbol", 15);
            button3.Text = "⮪";
            button3.TextAlign = ContentAlignment.TopCenter;
            button3.UseVisualStyleBackColor = true;
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 14);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.CellClick += DataGridView1_CellClick;
            dataGridView1.ReadOnly = true;

        }
        private void Init()
        {
            ConInit();
            DGV();
            CreateBackup();
            UpdateRowCountLabel(dataGridView1.RowCount);
        }
        private void RefreshDataGridView()
        {
            Init();
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedRow = e.RowIndex;
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textbox1.Text = row.Cells[3].Value?.ToString() ?? string.Empty;
                textBox2.Text = row.Cells[4].Value?.ToString() ?? string.Empty;
                textBox3.Text = row.Cells[6].Value?.ToString() ?? string.Empty;
                textBox4.Text = row.Cells[8].Value?.ToString() ?? string.Empty;

                string comboBox1Value = row.Cells[7].Value.ToString();
                string comboBox2Value = row.Cells[5].Value.ToString();

                comboBox1.Text = comboBox1Value;
                comboBox2.Text = comboBox2Value;
                string dateString = row.Cells[1].Value?.ToString();
                DateTime dateValue;
                if (!string.IsNullOrEmpty(dateString) && DateTime.TryParseExact(dateString, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out dateValue))
                {
                    dateTimePicker1.Value = dateValue;
                }
                else
                {
                    dateTimePicker1.Value = DateTime.Today;
                }
                string checkBoxString = row.Cells[2].Value?.ToString();
                checkBox1.Checked = !string.IsNullOrEmpty(checkBoxString) && checkBoxString.Equals("1", StringComparison.OrdinalIgnoreCase);
            }
        }
        private void ReplaceTextColumnWithCheckBox(int textColumnIndex)
        {
            var originalColumn = dataGridView1.Columns[textColumnIndex] as DataGridViewTextBoxColumn;
            if (originalColumn == null) return;

            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = originalColumn.HeaderText,
                Name = originalColumn.Name,
                FlatStyle = FlatStyle.Popup,
                DataPropertyName = originalColumn.DataPropertyName
            };

            dataGridView1.Columns.Insert(textColumnIndex + 1, checkBoxColumn);
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string textValue = row.Cells[textColumnIndex].Value?.ToString();
                bool isChecked = false;

                switch (textValue)
                {
                    case "1":
                        isChecked = true;
                        break;

                    case "0":
                        isChecked = false;
                        break;
                }
                row.Cells[textColumnIndex + 1].Value = isChecked;
            }
            dataGridView1.Columns.RemoveAt(textColumnIndex);
        }
        private void Clear()
        {
            dateTimePicker1.Value = DateTime.Now;
            checkBox1.Checked = false;
            textbox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;
            comboBox1.Text = "";
            comboBox2.Text = "";
        }
                   //  CRUD and SEARCH BUTTONS

        private void ADD_Click(object sender, EventArgs e)
        {
            try
            {
                CreateBackup();
                Insert();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Adding a line to the table", "Description :" + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // ADD BUTTON
        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                CreateBackup();
                DeleteRow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error occured while updating this row(s) : {ex.Message}", "Delete Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }   // DELETE BUTTON

        private void Button6_Click(object sender, EventArgs e)
        {
            try
            {
                CreateBackup();
                UpdateRow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error occured while updating this row(s) : {ex.Message}", "Update Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // UPDATE BUTTON

        private void Button7_Click(object sender, EventArgs e)
        {
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error occured while saving changes : {ex.Message}", "Save Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // SAVE BUTTON

        private void SEARH_Click(object sender, EventArgs e)
        {
            string connectionString = $"Data Source = {dbPath};Version=3;";
            try
            {
                connection = new SQLiteConnection(connectionString);
                adapter = new SQLiteDataAdapter("SELECT * FROM t1;", connection);
                dataTable = new DataTable();    
                adapter.Fill(dataTable);
                string searchText = textBox5.Text.Trim();
                dataGridView1.DataSource = dataTable;
                if (dataTable == null)
                {
                    MessageBox.Show("No data available to search.", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DataTable filteredTable = dataTable.Clone(); // Create a copy of the original table structure

                if (string.IsNullOrEmpty(searchText))
                {
                    // If search text is empty, show the original table
                    dataGridView1.DataSource = dataTable;
                }
                else
                {
                    string[] searchTerms = searchText.Split(' '); // Split the search text into multiple words

                    foreach (DataRow row in dataTable.Rows)
                    {
                        bool matchFound = false;

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            foreach (string term in searchTerms)
                            {
                                if (row[column].ToString().IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    matchFound = true;
                                    break;
                                }
                            }
                            if (matchFound) break;
                        }

                        if (matchFound)
                        {
                            filteredTable.ImportRow(row); // Add matching rows to filteredTable
                        }
                    }

                    if (filteredTable.Rows.Count > 0)
                    {
                        dataGridView1.DataSource = filteredTable;
                        HighlightMatches(searchTerms); // Highlight matching results
                    }
                    else
                    {
                        MessageBox.Show($"No matching records found for '{searchText}'.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dataGridView1.DataSource = dataTable;
                    }
                }
                UpdateRowCountLabel(dataGridView1.Rows.Count);
                comboBox3.Text = "   Sort by";
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        } // SEARCH BUTTON 

        private void Button2_Click(object sender, EventArgs e)
        {
            string newItem = textBox7.Text;

            if (!string.IsNullOrWhiteSpace(newItem))
            {
                comboBox1.Items.Add(newItem);
                textBox7.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid item.");
            }
        } // ADD SPONSORS BUTTON

        private void Button1_Click(object sender, EventArgs e)  // REFRESH BUTTON
        {
            RefreshDataGridView();
            comboBox3.Text = "   Sort by";
            UpdateRowCountLabel(dataGridView1.RowCount);
        }

        private void Button5_Click(object sender, EventArgs e)  // CLEAR BUTTON
        {
            Clear();
        }

        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Load the data into the DataTable
                try
                {
                    dataTable = new DataTable();
                    string query = "SELECT * FROM t1;";
                    connection = new SQLiteConnection($"Data Source = {dbPath}; Version=3;");
                    
                    connection.Open();
                    this.adapter = new SQLiteDataAdapter(query, connection);
                        
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ERROR IMPORTING DATA IN COMBOBOX3: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                view = new DataView(dataTable);
                string selectedSort = comboBox3.SelectedItem.ToString();
                string sortExpression = string.Empty;
                string filterExpression = string.Empty;

                switch (selectedSort)
                {
                    case "Ascending":
                        sortExpression = "ID ASC";
                        break;
                    case "Descending":
                        sortExpression = "ID DESC";
                        break;
                    case "Amount":
                        sortExpression = "AMOUNT ASC";
                        break;
                    case "Completed":
                        filterExpression = "STATUS = 1";
                        break;
                    case "Uncompleted":
                        filterExpression = "STATUS = 0";
                        break;
                    default:
                        MessageBox.Show("Invalid sort option selected.", "Sort Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                if (!string.IsNullOrEmpty(filterExpression))
                {
                    view.RowFilter = filterExpression;
                }

                if (!string.IsNullOrEmpty(sortExpression))
                {
                    view.Sort = sortExpression;
                }

                dataGridView1.DataSource = view;
                DGV(); 
                UpdateRowCountLabel(dataGridView1.RowCount);  
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR FILTERING: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
    } // SORTING COMBOBOX

        private void HighlightMatches(string[] searchTerms)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    foreach (string term in searchTerms)
                    {
                        if (cell.Value != null && cell.Value.ToString().IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            cell.Style.BackColor = Color.LightGoldenrodYellow; // Highlight matching cell
                            cell.Style.ForeColor = Color.Black;
                            break;
                        }
                    }
                }
            }
        } // HIGHLIGHT FOUND TEXT
  
        private void TextBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SEARH.PerformClick();
                e.SuppressKeyPress = true;
            }
        } // ALLOWING ENTER KEY TO SEARCH
        private void UpdateRowCountLabel(int rowCount)          // COUNT ROWS
        {
            label8.Text = $"Displayed Rows :  {rowCount-1}";
        }

        // CRUD METHODS

        private void SaveChanges()
        {
            string connectionString = $"Data Source={dbPath}; Version=3;";
            
            connection = new SQLiteConnection(connectionString);
            connection.Open();

            SQLiteTransaction transaction = connection.BeginTransaction();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue; // Skip the new row placeholder

                    string id = row.Cells["ID"].Value?.ToString();
                    string date = row.Cells["DATE"].Value?.ToString();
                    int status = Convert.ToInt32(row.Cells["STATUS"].Value ?? 0);
                    string sender = row.Cells["SENDER"].Value?.ToString();
                    string receiver = row.Cells["RECEIVER"].Value?.ToString();
                    string type = row.Cells["TYPE"].Value?.ToString();
                    double amount = Convert.ToDouble(row.Cells["AMOUNT"].Value ?? 0);
                    string sponsors = row.Cells["SPONSORS"].Value?.ToString();
                    string description = row.Cells["DESCRIPTION"].Value?.ToString();

                    string query;

                    if (string.IsNullOrEmpty(id)) // New row, perform INSERT
                    {
                        query = @"INSERT INTO t1 (DATE, STATUS, SENDER, RECEIVER, TYPE, AMOUNT, SPONSORS, DESCRIPTION)
                            VALUES (@DATE, @STATUS, @SENDER, @RECEIVER, @TYPE, @AMOUNT, @SPONSORS, @DESCRIPTION)";
                    }
                    else // Existing row, perform UPDATE
                    {
                        query = @"UPDATE t1
                            SET DATE = @DATE, STATUS = @STATUS, SENDER = @SENDER, RECEIVER = @RECEIVER, TYPE = @TYPE, AMOUNT = @AMOUNT, SPONSORS = @SPONSORS, DESCRIPTION = @DESCRIPTION
                            WHERE ID = @ID";
                    }

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            command.Parameters.AddWithValue("@ID", id);
                        }

                        command.Parameters.AddWithValue("@DATE", date);
                        command.Parameters.AddWithValue("@STATUS", status);
                        command.Parameters.AddWithValue("@SENDER", sender);
                        command.Parameters.AddWithValue("@RECEIVER", receiver);
                        command.Parameters.AddWithValue("@TYPE", type);
                        command.Parameters.AddWithValue("@AMOUNT", amount);
                        command.Parameters.AddWithValue("@SPONSORS", sponsors);
                        command.Parameters.AddWithValue("@DESCRIPTION", description);

                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            MessageBox.Show("Data saved successfully!");
            connection.Close();
        }

        private void UpdateRow()
        {
            try
            {
                DataTable dt = new DataTable();
                string cs = $"Data Source = {dbPath}; Version=3;";
                dataTable = new DataTable();

                connection = new SQLiteConnection(cs);
                connection.Open();

                // Fetch data from the database into the DataTable
                adapter = new SQLiteDataAdapter("SELECT * FROM t1;", connection);
                commandBuilder = new SQLiteCommandBuilder(adapter);
                
                commandBuilder.DataAdapter.Fill(dataTable);

                // Check if any row is selected in the DataGridView
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    // Get the index of the selected row
                    int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;
                    DataRow selectedRow = dataTable.Rows[selectedRowIndex];

                    // Update the selected row's values based on the controls
                    selectedRow["DATE"] = dateTimePicker1.Value.ToString("dd-MM-yyyy");
                    selectedRow["STATUS"] = checkBox1.Checked ? 1 : 0;
                    selectedRow["SENDER"] = string.IsNullOrEmpty(textbox1.Text) ? DBNull.Value : (object)textbox1.Text;
                    selectedRow["RECEIVER"] = string.IsNullOrEmpty(textBox2.Text) ? DBNull.Value : (object)textBox2.Text;
                    selectedRow["TYPE"] = comboBox2.SelectedItem?.ToString() ?? DBNull.Value.ToString();
                    selectedRow["SPONSORS"] = comboBox1.SelectedItem?.ToString() ?? DBNull.Value.ToString();
                    selectedRow["AMOUNT"] = Convert.ToDouble(textBox3.Text);
                    selectedRow["DESCRIPTION"] = textBox4.Text;

                    // Update the database with the changes
                    adapter.Update(dataTable);
                   
                    // Refresh the DataGridView to show updated data
                    dataTable.Clear();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;

                    MessageBox.Show("Row updated successfully!");
                }
                else
                {
                    MessageBox.Show("Please select a row to update.", "No Row Selected", MessageBoxButtons.OK);
                }

                connection.Close();
                UpdateRowCountLabel(dataGridView1.RowCount);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Operation failed!", $"{ex.Message}", MessageBoxButtons.OK);
            }
        }
        private void DeleteRow()
        {
            try
            {
                if (dataGridView1.SelectedCells.Count > 0) // Ensure a cell is selected
                {
                    // Get the selected cell's row index
                    int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;

                    if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.Rows.Count)
                    {
                        // Get the ID value from the selected row
                        DataGridViewRow selectedRow = dataGridView1.Rows[selectedRowIndex];
                        int id = Convert.ToInt32(selectedRow.Cells["ID"].Value);

                        var confirmationResult = MessageBox.Show(
                            "Are you sure you want to delete this record?",
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmationResult == DialogResult.Yes)
                        {
                            string query = "DELETE FROM t1 WHERE ID = @ID";

                            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                            connection.Open();
                            cmd = new SQLiteCommand(query, connection);
                            cmd.Parameters.AddWithValue("@ID", id);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                dataGridView1.Rows.RemoveAt(selectedRowIndex);
                                UpdateRowCountLabel(dataGridView1.Rows.Count);
                                MessageBox.Show("The record has been successfully deleted.", "Delete Operation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No records were deleted. Please check if the ID exists.", "Delete Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            UpdateRowCountLabel(dataGridView1.RowCount);
                            connection.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a valid row to delete.", "Delete Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a cell to delete its corresponding row.", "Delete Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the record: {ex.Message}", "Delete Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Insert()
        {
            string cs = $"Data Source = {dbPath}; Version=3;";
            dataTable = new DataTable();
            try
            {
                connection = new SQLiteConnection(cs);
                connection.Open();
                adapter = new SQLiteDataAdapter("SELECT * FROM t1;", connection);
                commandBuilder = new SQLiteCommandBuilder(adapter);

                adapter.Fill(dataTable);
                try
                {
                    DataRow newRow = dataTable.NewRow();

                    newRow["DATE"] = dateTimePicker1.Value.ToString("dd-MM-yyyy");
                    newRow["STATUS"] = checkBox1.Checked ? 1 : 0;
                    newRow["SENDER"] = string.IsNullOrEmpty(textbox1.Text) ? DBNull.Value : (object)textbox1.Text;
                    newRow["RECEIVER"] = string.IsNullOrEmpty(textBox2.Text) ? DBNull.Value : (object)textBox2.Text;

                    // Check if comboBox2 and comboBox1 have selected items
                    newRow["TYPE"] = comboBox2.SelectedItem?.ToString() ?? DBNull.Value.ToString();
                    newRow["SPONSORS"] = comboBox1.SelectedItem?.ToString() ?? DBNull.Value.ToString();
                    newRow["AMOUNT"] = Convert.ToDouble(textBox3.Text);
                    newRow["DESCRIPTION"] = textBox4.Text;

                    dataTable.Rows.Add(newRow);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Missing Inputs Detected", $"Description {ex.Message}", MessageBoxButtons.OK);
                }
                adapter.Update(dataTable);

                dataTable.Clear();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
                UpdateRowCountLabel(dataGridView1.RowCount);
                MessageBox.Show("Row inserted successfully!");
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            UndoChanges();
        }

    }
}
