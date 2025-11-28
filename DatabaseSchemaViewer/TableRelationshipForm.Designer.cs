namespace DatabaseSchemaViewer
{
    partial class TableRelationshipForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxStatistics = new System.Windows.Forms.GroupBox();
            this.lblStatistics = new System.Windows.Forms.Label();
            this.groupBoxFilter = new System.Windows.Forms.GroupBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.radioIsolated = new System.Windows.Forms.RadioButton();
            this.radioLinked = new System.Windows.Forms.RadioButton();
            this.radioAll = new System.Windows.Forms.RadioButton();
            this.btnFindPath = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBoxTables = new System.Windows.Forms.GroupBox();
            this.listViewTables = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnections = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBoxRelationships = new System.Windows.Forms.GroupBox();
            this.treeViewRelationships = new System.Windows.Forms.TreeView();
            this.groupBoxClusters = new System.Windows.Forms.GroupBox();
            this.listViewClusters = new System.Windows.Forms.ListView();
            this.columnHeaderClusterId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderClusterName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTableCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRelCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnExportJson = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxStatistics.SuspendLayout();
            this.groupBoxFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBoxRelationships.SuspendLayout();
            this.groupBoxClusters.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxStatistics
            // 
            this.groupBoxStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStatistics.Controls.Add(this.lblStatistics);
            this.groupBoxStatistics.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStatistics.Name = "groupBoxStatistics";
            this.groupBoxStatistics.Size = new System.Drawing.Size(860, 50);
            this.groupBoxStatistics.TabIndex = 0;
            this.groupBoxStatistics.TabStop = false;
            this.groupBoxStatistics.Text = "Статистика";
            // 
            // lblStatistics
            // 
            this.lblStatistics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatistics.Location = new System.Drawing.Point(3, 16);
            this.lblStatistics.Name = "lblStatistics";
            this.lblStatistics.Padding = new System.Windows.Forms.Padding(5);
            this.lblStatistics.Size = new System.Drawing.Size(854, 31);
            this.lblStatistics.TabIndex = 0;
            this.lblStatistics.Text = "Всего таблиц: 0  |  Связанных: 0  |  Изолированных: 0  |  Кластеров: 0  |  Всего " +
    "связей: 0";
            // 
            // groupBoxFilter
            // 
            this.groupBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFilter.Controls.Add(this.btnFindPath);
            this.groupBoxFilter.Controls.Add(this.txtSearch);
            this.groupBoxFilter.Controls.Add(this.lblSearch);
            this.groupBoxFilter.Controls.Add(this.radioIsolated);
            this.groupBoxFilter.Controls.Add(this.radioLinked);
            this.groupBoxFilter.Controls.Add(this.radioAll);
            this.groupBoxFilter.Location = new System.Drawing.Point(12, 68);
            this.groupBoxFilter.Name = "groupBoxFilter";
            this.groupBoxFilter.Size = new System.Drawing.Size(860, 50);
            this.groupBoxFilter.TabIndex = 1;
            this.groupBoxFilter.TabStop = false;
            this.groupBoxFilter.Text = "Фильтр";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(398, 19);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.TabIndex = 4;
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(352, 22);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(42, 13);
            this.lblSearch.TabIndex = 3;
            this.lblSearch.Text = "Поиск:";
            // 
            // radioIsolated
            // 
            this.radioIsolated.AutoSize = true;
            this.radioIsolated.Location = new System.Drawing.Point(220, 20);
            this.radioIsolated.Name = "radioIsolated";
            this.radioIsolated.Size = new System.Drawing.Size(107, 17);
            this.radioIsolated.TabIndex = 2;
            this.radioIsolated.Text = "Изолированные";
            this.radioIsolated.UseVisualStyleBackColor = true;
            this.radioIsolated.CheckedChanged += new System.EventHandler(this.RadioFilter_CheckedChanged);
            // 
            // radioLinked
            // 
            this.radioLinked.AutoSize = true;
            this.radioLinked.Location = new System.Drawing.Point(118, 20);
            this.radioLinked.Name = "radioLinked";
            this.radioLinked.Size = new System.Drawing.Size(81, 17);
            this.radioLinked.TabIndex = 1;
            this.radioLinked.Text = "Связанные";
            this.radioLinked.UseVisualStyleBackColor = true;
            this.radioLinked.CheckedChanged += new System.EventHandler(this.RadioFilter_CheckedChanged);
            // 
            // radioAll
            // 
            this.radioAll.AutoSize = true;
            this.radioAll.Checked = true;
            this.radioAll.Location = new System.Drawing.Point(15, 20);
            this.radioAll.Name = "radioAll";
            this.radioAll.Size = new System.Drawing.Size(91, 17);
            this.radioAll.TabIndex = 0;
            this.radioAll.TabStop = true;
            this.radioAll.Text = "Все таблицы";
            this.radioAll.UseVisualStyleBackColor = true;
            this.radioAll.CheckedChanged += new System.EventHandler(this.RadioFilter_CheckedChanged);
            // 
            // btnFindPath
            // 
            this.btnFindPath.Location = new System.Drawing.Point(620, 17);
            this.btnFindPath.Name = "btnFindPath";
            this.btnFindPath.Size = new System.Drawing.Size(200, 23);
            this.btnFindPath.TabIndex = 5;
            this.btnFindPath.Text = "Найти путь между таблицами...";
            this.btnFindPath.UseVisualStyleBackColor = true;
            this.btnFindPath.Click += new System.EventHandler(this.BtnFindPath_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 124);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxTables);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(860, 350);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.TabIndex = 2;
            // 
            // groupBoxTables
            // 
            this.groupBoxTables.Controls.Add(this.listViewTables);
            this.groupBoxTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTables.Location = new System.Drawing.Point(0, 0);
            this.groupBoxTables.Name = "groupBoxTables";
            this.groupBoxTables.Size = new System.Drawing.Size(300, 350);
            this.groupBoxTables.TabIndex = 0;
            this.groupBoxTables.TabStop = false;
            this.groupBoxTables.Text = "Таблицы";
            // 
            // listViewTables
            // 
            this.listViewTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderConnections,
            this.columnHeaderType});
            this.listViewTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewTables.FullRowSelect = true;
            this.listViewTables.GridLines = true;
            this.listViewTables.HideSelection = false;
            this.listViewTables.Location = new System.Drawing.Point(3, 16);
            this.listViewTables.Name = "listViewTables";
            this.listViewTables.Size = new System.Drawing.Size(294, 331);
            this.listViewTables.TabIndex = 0;
            this.listViewTables.UseCompatibleStateImageBehavior = false;
            this.listViewTables.View = System.Windows.Forms.View.Details;
            this.listViewTables.SelectedIndexChanged += new System.EventHandler(this.ListViewTables_SelectedIndexChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Имя таблицы";
            this.columnHeaderName.Width = 150;
            // 
            // columnHeaderConnections
            // 
            this.columnHeaderConnections.Text = "Связей";
            this.columnHeaderConnections.Width = 60;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Тип";
            this.columnHeaderType.Width = 80;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBoxRelationships);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBoxClusters);
            this.splitContainer2.Size = new System.Drawing.Size(556, 350);
            this.splitContainer2.SplitterDistance = 200;
            this.splitContainer2.TabIndex = 0;
            // 
            // groupBoxRelationships
            // 
            this.groupBoxRelationships.Controls.Add(this.treeViewRelationships);
            this.groupBoxRelationships.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxRelationships.Location = new System.Drawing.Point(0, 0);
            this.groupBoxRelationships.Name = "groupBoxRelationships";
            this.groupBoxRelationships.Size = new System.Drawing.Size(556, 200);
            this.groupBoxRelationships.TabIndex = 0;
            this.groupBoxRelationships.TabStop = false;
            this.groupBoxRelationships.Text = "Связи выбранной таблицы";
            // 
            // treeViewRelationships
            // 
            this.treeViewRelationships.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRelationships.Location = new System.Drawing.Point(3, 16);
            this.treeViewRelationships.Name = "treeViewRelationships";
            this.treeViewRelationships.Size = new System.Drawing.Size(550, 181);
            this.treeViewRelationships.TabIndex = 0;
            // 
            // groupBoxClusters
            // 
            this.groupBoxClusters.Controls.Add(this.listViewClusters);
            this.groupBoxClusters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxClusters.Location = new System.Drawing.Point(0, 0);
            this.groupBoxClusters.Name = "groupBoxClusters";
            this.groupBoxClusters.Size = new System.Drawing.Size(556, 146);
            this.groupBoxClusters.TabIndex = 0;
            this.groupBoxClusters.TabStop = false;
            this.groupBoxClusters.Text = "Кластеры связанных таблиц";
            // 
            // listViewClusters
            // 
            this.listViewClusters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderClusterId,
            this.columnHeaderClusterName,
            this.columnHeaderTableCount,
            this.columnHeaderRelCount});
            this.listViewClusters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewClusters.FullRowSelect = true;
            this.listViewClusters.GridLines = true;
            this.listViewClusters.HideSelection = false;
            this.listViewClusters.Location = new System.Drawing.Point(3, 16);
            this.listViewClusters.Name = "listViewClusters";
            this.listViewClusters.Size = new System.Drawing.Size(550, 127);
            this.listViewClusters.TabIndex = 0;
            this.listViewClusters.UseCompatibleStateImageBehavior = false;
            this.listViewClusters.View = System.Windows.Forms.View.Details;
            this.listViewClusters.DoubleClick += new System.EventHandler(this.ListViewClusters_DoubleClick);
            // 
            // columnHeaderClusterId
            // 
            this.columnHeaderClusterId.Text = "ID";
            this.columnHeaderClusterId.Width = 40;
            // 
            // columnHeaderClusterName
            // 
            this.columnHeaderClusterName.Text = "Центральная таблица";
            this.columnHeaderClusterName.Width = 200;
            // 
            // columnHeaderTableCount
            // 
            this.columnHeaderTableCount.Text = "Таблиц";
            this.columnHeaderTableCount.Width = 60;
            // 
            // columnHeaderRelCount
            // 
            this.columnHeaderRelCount.Text = "Связей";
            this.columnHeaderRelCount.Width = 60;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnClose);
            this.panelButtons.Controls.Add(this.btnExportJson);
            this.panelButtons.Controls.Add(this.btnExportCsv);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 480);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(884, 40);
            this.panelButtons.TabIndex = 3;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(797, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // btnExportJson
            // 
            this.btnExportJson.Location = new System.Drawing.Point(127, 8);
            this.btnExportJson.Name = "btnExportJson";
            this.btnExportJson.Size = new System.Drawing.Size(110, 23);
            this.btnExportJson.TabIndex = 1;
            this.btnExportJson.Text = "Экспорт в JSON";
            this.btnExportJson.UseVisualStyleBackColor = true;
            this.btnExportJson.Click += new System.EventHandler(this.BtnExportJson_Click);
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Location = new System.Drawing.Point(12, 8);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(100, 23);
            this.btnExportCsv.TabIndex = 0;
            this.btnExportCsv.Text = "Экспорт в CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.BtnExportCsv_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 520);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // TableRelationshipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 542);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBoxFilter);
            this.Controls.Add(this.groupBoxStatistics);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "TableRelationshipForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Анализ связей таблиц";
            this.Load += new System.EventHandler(this.TableRelationshipForm_Load);
            this.groupBoxStatistics.ResumeLayout(false);
            this.groupBoxFilter.ResumeLayout(false);
            this.groupBoxFilter.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxTables.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBoxRelationships.ResumeLayout(false);
            this.groupBoxClusters.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxStatistics;
        private System.Windows.Forms.Label lblStatistics;
        private System.Windows.Forms.GroupBox groupBoxFilter;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.RadioButton radioIsolated;
        private System.Windows.Forms.RadioButton radioLinked;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.Button btnFindPath;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBoxTables;
        private System.Windows.Forms.ListView listViewTables;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderConnections;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBoxRelationships;
        private System.Windows.Forms.TreeView treeViewRelationships;
        private System.Windows.Forms.GroupBox groupBoxClusters;
        private System.Windows.Forms.ListView listViewClusters;
        private System.Windows.Forms.ColumnHeader columnHeaderClusterId;
        private System.Windows.Forms.ColumnHeader columnHeaderClusterName;
        private System.Windows.Forms.ColumnHeader columnHeaderTableCount;
        private System.Windows.Forms.ColumnHeader columnHeaderRelCount;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnExportJson;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}
