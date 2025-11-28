using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;

namespace DatabaseSchemaViewer
{
    public partial class TableRelationshipForm : Form
    {
        private readonly DatabaseSchema _databaseSchema;
        private readonly TableRelationshipAnalyzer _analyzer;
        private List<DatabaseTable> _allTables;
        private List<DatabaseTable> _linkedTables;
        private List<DatabaseTable> _isolatedTables;
        private List<TableCluster> _clusters;

        public TableRelationshipForm(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema ?? throw new ArgumentNullException(nameof(databaseSchema));
            _analyzer = new TableRelationshipAnalyzer(databaseSchema);
            InitializeComponent();
        }

        private void TableRelationshipForm_Load(object sender, EventArgs e)
        {
            LoadData();
            UpdateStatistics();
            PopulateTableList();
            PopulateClusterList();
        }

        private void LoadData()
        {
            _allTables = _databaseSchema.Tables.OrderBy(t => t.Name).ToList();
            _linkedTables = _analyzer.GetLinkedTables();
            _isolatedTables = _analyzer.GetIsolatedTables();
            _clusters = _analyzer.GetTableClusters();
        }

        private void UpdateStatistics()
        {
            var stats = _analyzer.GetRelationshipStatistics();
            lblStatistics.Text = string.Format(
                "Всего таблиц: {0}  |  Связанных: {1}  |  Изолированных: {2}  |  Кластеров: {3}  |  Всего связей: {4}",
                stats.TotalTables,
                stats.LinkedTables,
                stats.IsolatedTables,
                stats.ClusterCount,
                stats.TotalRelationships);

            if (stats.MostConnectedTable != null)
            {
                lblStatistics.Text += string.Format("  |  Макс. связей: {0} ({1})",
                    stats.MaxConnectionsCount,
                    stats.MostConnectedTable.Name);
            }
        }

        private void PopulateTableList()
        {
            listViewTables.BeginUpdate();
            listViewTables.Items.Clear();

            var tables = GetFilteredTables();
            var searchText = txtSearch.Text.Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(searchText))
            {
                tables = tables.Where(t => t.Name.ToLowerInvariant().Contains(searchText)).ToList();
            }

            var linkedSet = new HashSet<DatabaseTable>(_linkedTables);

            foreach (var table in tables)
            {
                var connectionCount = table.ForeignKeys.Count + table.ForeignKeyChildren.Count;
                var isLinked = linkedSet.Contains(table);
                var item = new ListViewItem(table.Name)
                {
                    Tag = table
                };
                item.SubItems.Add(connectionCount.ToString());
                item.SubItems.Add(isLinked ? "Связанная" : "Изолированная");

                if (!isLinked)
                {
                    item.ForeColor = System.Drawing.Color.Gray;
                }

                listViewTables.Items.Add(item);
            }

            listViewTables.EndUpdate();
            toolStripStatusLabel1.Text = string.Format("Отображено таблиц: {0}", listViewTables.Items.Count);
        }

        private List<DatabaseTable> GetFilteredTables()
        {
            if (radioLinked.Checked)
                return _linkedTables;
            if (radioIsolated.Checked)
                return _isolatedTables;
            return _allTables;
        }

        private void PopulateClusterList()
        {
            listViewClusters.BeginUpdate();
            listViewClusters.Items.Clear();

            foreach (var cluster in _clusters)
            {
                var item = new ListViewItem(cluster.ClusterId.ToString())
                {
                    Tag = cluster
                };
                item.SubItems.Add(cluster.Name);
                item.SubItems.Add(cluster.TableCount.ToString());
                item.SubItems.Add(cluster.RelationshipCount.ToString());
                listViewClusters.Items.Add(item);
            }

            listViewClusters.EndUpdate();
        }

        private void ShowTableRelationships(DatabaseTable table)
        {
            treeViewRelationships.BeginUpdate();
            treeViewRelationships.Nodes.Clear();

            if (table == null)
            {
                treeViewRelationships.EndUpdate();
                return;
            }

            var relationships = _analyzer.GetTableRelationships(table);

            // Родительские таблицы
            var parentNode = treeViewRelationships.Nodes.Add("Родительские таблицы (ссылается на):");
            if (relationships.ParentTables.Any())
            {
                foreach (var parent in relationships.ParentTables)
                {
                    var node = parentNode.Nodes.Add(string.Format("{0} (FK: {1})",
                        parent.Table.Name,
                        string.Join(", ", parent.ForeignKeyColumns)));
                    node.Tag = parent.Table;
                    node.ToolTipText = string.Format("Constraint: {0}\nColumns: {1} -> {2}",
                        parent.ForeignKeyName,
                        string.Join(", ", parent.ForeignKeyColumns),
                        string.Join(", ", parent.ReferencedColumns));
                }
            }
            else
            {
                parentNode.Nodes.Add("(нет)");
            }
            parentNode.Expand();

            // Дочерние таблицы
            var childNode = treeViewRelationships.Nodes.Add("Дочерние таблицы (ссылаются на эту):");
            if (relationships.ChildTables.Any())
            {
                foreach (var child in relationships.ChildTables)
                {
                    var node = childNode.Nodes.Add(string.Format("{0} (FK: {1})",
                        child.Table.Name,
                        string.Join(", ", child.ForeignKeyColumns)));
                    node.Tag = child.Table;
                    node.ToolTipText = string.Format("Constraint: {0}\nColumns: {1}",
                        child.ForeignKeyName,
                        string.Join(", ", child.ForeignKeyColumns));
                }
            }
            else
            {
                childNode.Nodes.Add("(нет)");
            }
            childNode.Expand();

            treeViewRelationships.EndUpdate();
        }

        private void ListViewTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewTables.SelectedItems.Count > 0)
            {
                var table = listViewTables.SelectedItems[0].Tag as DatabaseTable;
                ShowTableRelationships(table);
            }
            else
            {
                ShowTableRelationships(null);
            }
        }

        private void RadioFilter_CheckedChanged(object sender, EventArgs e)
        {
            PopulateTableList();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            PopulateTableList();
        }

        private void BtnFindPath_Click(object sender, EventArgs e)
        {
            var tables = _databaseSchema.Tables.Select(t => t.Name).OrderBy(n => n).ToArray();

            using (var dialog = new FindPathDialog(tables))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var path = _analyzer.FindPathBetweenTables(dialog.FromTable, dialog.ToTable);
                    if (path != null && path.Count > 0)
                    {
                        var pathString = string.Join(" -> ", path.Select(t => t.Name));
                        MessageBox.Show(
                            string.Format("Путь найден ({0} таблиц):\n\n{1}", path.Count, pathString),
                            "Путь между таблицами",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            string.Format("Путь между таблицами '{0}' и '{1}' не найден.",
                                dialog.FromTable, dialog.ToTable),
                            "Путь не найден",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ListViewClusters_DoubleClick(object sender, EventArgs e)
        {
            if (listViewClusters.SelectedItems.Count > 0)
            {
                var cluster = listViewClusters.SelectedItems[0].Tag as TableCluster;
                if (cluster != null)
                {
                    var tableNames = string.Join("\n", cluster.Tables.Select(t => "  - " + t.Name));
                    MessageBox.Show(
                        string.Format("Кластер '{0}' ({1} таблиц):\n\n{2}",
                            cluster.Name, cluster.TableCount, tableNames),
                        "Таблицы кластера",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.DefaultExt = ".csv";
                dialog.FileName = "table_relationships";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        ExportToCsv(dialog.FileName);
                        toolStripStatusLabel1.Text = "Экспортировано в " + Path.GetFileName(dialog.FileName);
                        MessageBox.Show("Экспорт успешно завершен.", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToCsv(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TableName,ConnectionCount,Type,ParentTables,ChildTables");

            foreach (var table in _allTables)
            {
                var relationships = _analyzer.GetTableRelationships(table);
                var connectionCount = relationships.TotalRelationships;
                var type = relationships.IsIsolated ? "Isolated" : "Linked";
                var parents = string.Join("; ", relationships.ParentTables.Select(p => p.Table.Name));
                var children = string.Join("; ", relationships.ChildTables.Select(c => c.Table.Name));

                sb.AppendLine(string.Format("\"{0}\",{1},\"{2}\",\"{3}\",\"{4}\"",
                    table.Name.Replace("\"", "\"\""),
                    connectionCount,
                    type,
                    parents.Replace("\"", "\"\""),
                    children.Replace("\"", "\"\"")));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void BtnExportJson_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.DefaultExt = ".json";
                dialog.FileName = "table_relationships";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        ExportToJson(dialog.FileName);
                        toolStripStatusLabel1.Text = "Экспортировано в " + Path.GetFileName(dialog.FileName);
                        MessageBox.Show("Экспорт успешно завершен.", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToJson(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"statistics\": {");
            var stats = _analyzer.GetRelationshipStatistics();
            sb.AppendLine(string.Format("    \"totalTables\": {0},", stats.TotalTables));
            sb.AppendLine(string.Format("    \"linkedTables\": {0},", stats.LinkedTables));
            sb.AppendLine(string.Format("    \"isolatedTables\": {0},", stats.IsolatedTables));
            sb.AppendLine(string.Format("    \"totalRelationships\": {0},", stats.TotalRelationships));
            sb.AppendLine(string.Format("    \"clusterCount\": {0}", stats.ClusterCount));
            sb.AppendLine("  },");

            sb.AppendLine("  \"tables\": [");
            for (int i = 0; i < _allTables.Count; i++)
            {
                var table = _allTables[i];
                var relationships = _analyzer.GetTableRelationships(table);
                sb.AppendLine("    {");
                sb.AppendLine(string.Format("      \"name\": \"{0}\",", EscapeJson(table.Name)));
                sb.AppendLine(string.Format("      \"connectionCount\": {0},", relationships.TotalRelationships));
                sb.AppendLine(string.Format("      \"isIsolated\": {0},", relationships.IsIsolated.ToString().ToLower()));
                sb.Append("      \"parentTables\": [");
                sb.Append(string.Join(", ", relationships.ParentTables.Select(p => "\"" + EscapeJson(p.Table.Name) + "\"")));
                sb.AppendLine("],");
                sb.Append("      \"childTables\": [");
                sb.Append(string.Join(", ", relationships.ChildTables.Select(c => "\"" + EscapeJson(c.Table.Name) + "\"")));
                sb.AppendLine("]");
                sb.AppendLine(i < _allTables.Count - 1 ? "    }," : "    }");
            }
            sb.AppendLine("  ],");

            sb.AppendLine("  \"clusters\": [");
            for (int i = 0; i < _clusters.Count; i++)
            {
                var cluster = _clusters[i];
                sb.AppendLine("    {");
                sb.AppendLine(string.Format("      \"id\": {0},", cluster.ClusterId));
                sb.AppendLine(string.Format("      \"name\": \"{0}\",", EscapeJson(cluster.Name)));
                sb.AppendLine(string.Format("      \"tableCount\": {0},", cluster.TableCount));
                sb.AppendLine(string.Format("      \"relationshipCount\": {0},", cluster.RelationshipCount));
                sb.Append("      \"tables\": [");
                sb.Append(string.Join(", ", cluster.Tables.Select(t => "\"" + EscapeJson(t.Name) + "\"")));
                sb.AppendLine("]");
                sb.AppendLine(i < _clusters.Count - 1 ? "    }," : "    }");
            }
            sb.AppendLine("  ]");

            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

    /// <summary>
    /// Диалог поиска пути между таблицами
    /// </summary>
    public class FindPathDialog : Form
    {
        private ComboBox cboFrom;
        private ComboBox cboTo;
        private Button btnOk;
        private Button btnCancel;
        private Label lblFrom;
        private Label lblTo;

        public string FromTable => cboFrom.SelectedItem?.ToString();
        public string ToTable => cboTo.SelectedItem?.ToString();

        public FindPathDialog(string[] tableNames)
        {
            InitializeComponents();
            cboFrom.Items.AddRange(tableNames);
            cboTo.Items.AddRange(tableNames);
        }

        private void InitializeComponents()
        {
            Width = 400;
            Height = 180;
            Text = "Найти путь между таблицами";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            lblFrom = new Label
            {
                Text = "От таблицы:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            cboFrom = new ComboBox
            {
                Location = new System.Drawing.Point(120, 17),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            lblTo = new Label
            {
                Text = "До таблицы:",
                Location = new System.Drawing.Point(20, 55),
                AutoSize = true
            };

            cboTo = new ComboBox
            {
                Location = new System.Drawing.Point(120, 52),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnOk = new Button
            {
                Text = "Найти",
                Location = new System.Drawing.Point(200, 100),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (s, e) =>
            {
                if (cboFrom.SelectedItem == null || cboTo.SelectedItem == null)
                {
                    MessageBox.Show("Выберите обе таблицы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new System.Drawing.Point(290, 100),
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[] { lblFrom, cboFrom, lblTo, cboTo, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
