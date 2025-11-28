using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer.Controls
{
    /// <summary>
    /// Panel for displaying entity details
    /// </summary>
    public class EntityDetailsPanel : UserControl
    {
        private Label _nameLabel;
        private Label _typeLabel;
        private Label _statusLabel;
        private Label _createdLabel;
        private Label _lastModifiedLabel;
        private ListView _dependenciesListView;
        private ListView _referencedByListView;
        private TextBox _sourceCodeTextBox;
        private TabControl _tabControl;
        private DatabaseEntity _currentEntity;

        /// <summary>
        /// Event raised when a related entity is clicked
        /// </summary>
        public event EventHandler<EntitySelectedEventArgs> RelatedEntityClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDetailsPanel"/> class
        /// </summary>
        public EntityDetailsPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Header panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10)
            };

            _nameLabel = new Label
            {
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font(Font.FontFamily, 12, FontStyle.Bold)
            };

            _typeLabel = new Label
            {
                Location = new Point(10, 35),
                AutoSize = true
            };

            _statusLabel = new Label
            {
                Location = new Point(10, 55),
                AutoSize = true
            };

            _createdLabel = new Label
            {
                Location = new Point(10, 75),
                AutoSize = true
            };

            _lastModifiedLabel = new Label
            {
                Location = new Point(10, 95),
                AutoSize = true
            };

            headerPanel.Controls.AddRange(new Control[] 
            { 
                _nameLabel, 
                _typeLabel, 
                _statusLabel, 
                _createdLabel, 
                _lastModifiedLabel 
            });

            // Tab control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Dependencies tab
            var dependenciesTab = new TabPage("Dependencies (Uses)");
            _dependenciesListView = CreateListView();
            _dependenciesListView.DoubleClick += OnDependencyDoubleClick;
            dependenciesTab.Controls.Add(_dependenciesListView);
            _tabControl.TabPages.Add(dependenciesTab);

            // Referenced by tab
            var referencedByTab = new TabPage("Referenced By (Used by)");
            _referencedByListView = CreateListView();
            _referencedByListView.DoubleClick += OnReferencedByDoubleClick;
            referencedByTab.Controls.Add(_referencedByListView);
            _tabControl.TabPages.Add(referencedByTab);

            // Source code tab
            var sourceCodeTab = new TabPage("Source Code");
            _sourceCodeTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9),
                WordWrap = false
            };
            sourceCodeTab.Controls.Add(_sourceCodeTextBox);
            _tabControl.TabPages.Add(sourceCodeTab);

            Controls.Add(_tabControl);
            Controls.Add(headerPanel);
        }

        private ListView CreateListView()
        {
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listView.Columns.Add("Name", 150);
            listView.Columns.Add("Type", 100);
            listView.Columns.Add("Owner", 100);
            listView.Columns.Add("Dependency Type", 80);

            return listView;
        }

        /// <summary>
        /// Sets the entity to display
        /// </summary>
        public void SetEntity(DatabaseEntity entity)
        {
            _currentEntity = entity;

            if (entity == null)
            {
                ClearDisplay();
                return;
            }

            // Update header
            _nameLabel.Text = entity.Name ?? "Unknown";
            _typeLabel.Text = "Type: " + entity.EntityType;
            _statusLabel.Text = "Status: " + (entity.Status ?? "N/A");
            _createdLabel.Text = "Created: " + (entity.Created?.ToString("g") ?? "N/A");
            _lastModifiedLabel.Text = "Last Modified: " + (entity.LastDdlTime?.ToString("g") ?? "N/A");

            // Update color indicator
            _typeLabel.BackColor = DependencyGraphControl.GetEntityColor(entity.EntityType);

            // Update dependencies list
            _dependenciesListView.Items.Clear();
            if (entity.Dependencies != null)
            {
                foreach (var dep in entity.Dependencies)
                {
                    var item = new ListViewItem(dep.ReferencedName);
                    item.SubItems.Add(dep.ReferencedType.ToString());
                    item.SubItems.Add(dep.ReferencedOwner);
                    item.SubItems.Add(dep.DependencyType ?? "N/A");
                    item.Tag = dep;
                    _dependenciesListView.Items.Add(item);
                }
            }

            // Update referenced by list
            _referencedByListView.Items.Clear();
            if (entity.ReferencedBy != null)
            {
                foreach (var dep in entity.ReferencedBy)
                {
                    var item = new ListViewItem(dep.ObjectName);
                    item.SubItems.Add(dep.ObjectType.ToString());
                    item.SubItems.Add(dep.OwnerName);
                    item.SubItems.Add(dep.DependencyType ?? "N/A");
                    item.Tag = dep;
                    _referencedByListView.Items.Add(item);
                }
            }

            // Update source code
            _sourceCodeTextBox.Text = entity.SourceCode ?? "No source code available";
        }

        private void ClearDisplay()
        {
            _nameLabel.Text = "No entity selected";
            _typeLabel.Text = "";
            _typeLabel.BackColor = BackColor;
            _statusLabel.Text = "";
            _createdLabel.Text = "";
            _lastModifiedLabel.Text = "";
            _dependenciesListView.Items.Clear();
            _referencedByListView.Items.Clear();
            _sourceCodeTextBox.Text = "";
        }

        private void OnDependencyDoubleClick(object sender, EventArgs e)
        {
            if (_dependenciesListView.SelectedItems.Count > 0)
            {
                var dep = _dependenciesListView.SelectedItems[0].Tag as EntityDependency;
                if (dep != null)
                {
                    var entity = new DatabaseEntity
                    {
                        SchemaOwner = dep.ReferencedOwner,
                        Name = dep.ReferencedName,
                        EntityType = dep.ReferencedType
                    };
                    RelatedEntityClicked?.Invoke(this, new EntitySelectedEventArgs(entity));
                }
            }
        }

        private void OnReferencedByDoubleClick(object sender, EventArgs e)
        {
            if (_referencedByListView.SelectedItems.Count > 0)
            {
                var dep = _referencedByListView.SelectedItems[0].Tag as EntityDependency;
                if (dep != null)
                {
                    var entity = new DatabaseEntity
                    {
                        SchemaOwner = dep.OwnerName,
                        Name = dep.ObjectName,
                        EntityType = dep.ObjectType
                    };
                    RelatedEntityClicked?.Invoke(this, new EntitySelectedEventArgs(entity));
                }
            }
        }
    }
}
