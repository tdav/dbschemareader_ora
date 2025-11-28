using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using DatabaseSchemaViewer.Controls;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseSchemaViewer
{
    /// <summary>
    /// Form for viewing database entity dependencies
    /// </summary>
    public partial class DependencyViewerForm : Form
    {
        private readonly DatabaseSchema _schema;
        private DependencyGraph _graph;
        private TreeView _entityTreeView;
        private DependencyGraphControl _graphControl;
        private EntityFilterPanel _filterPanel;
        private EntityDetailsPanel _detailsPanel;
        private SplitContainer _mainSplitter;
        private SplitContainer _leftSplitter;
        private SplitContainer _rightSplitter;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyViewerForm"/> class
        /// </summary>
        public DependencyViewerForm(DatabaseSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException("schema");
            InitializeComponent();
            BuildDependencyGraph();
            PopulateTreeView();
        }

        private void InitializeComponent()
        {
            Text = "Dependency Analyzer";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;

            // Create tool strip
            _toolStrip = new ToolStrip();
            
            var resetViewButton = new ToolStripButton("Reset View");
            resetViewButton.Click += (s, e) => _graphControl.ResetView();
            _toolStrip.Items.Add(resetViewButton);

            var centerButton = new ToolStripButton("Center on Selected");
            centerButton.Click += (s, e) => _graphControl.CenterOnSelected();
            _toolStrip.Items.Add(centerButton);

            _toolStrip.Items.Add(new ToolStripSeparator());

            var checkCircularButton = new ToolStripButton("Find Circular Dependencies");
            checkCircularButton.Click += FindCircularDependencies;
            _toolStrip.Items.Add(checkCircularButton);

            // Create status strip
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            _statusStrip.Items.Add(_statusLabel);

            // Main splitter (left panel vs right content)
            _mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 250
            };

            // Left splitter (tree view vs filter panel)
            _leftSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // Entity tree view
            _entityTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false
            };
            _entityTreeView.AfterSelect += EntityTreeViewAfterSelect;

            // Filter panel
            _filterPanel = new EntityFilterPanel
            {
                Dock = DockStyle.Fill
            };
            _filterPanel.FilterChanged += FilterChanged;
            _filterPanel.SearchChanged += SearchChanged;

            _leftSplitter.Panel1.Controls.Add(_entityTreeView);
            _leftSplitter.Panel2.Controls.Add(_filterPanel);

            // Right splitter (graph vs details)
            _rightSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 450
            };

            // Graph control
            _graphControl = new DependencyGraphControl
            {
                Dock = DockStyle.Fill
            };
            _graphControl.EntitySelected += GraphEntitySelected;

            // Details panel
            _detailsPanel = new EntityDetailsPanel
            {
                Dock = DockStyle.Fill
            };
            _detailsPanel.RelatedEntityClicked += RelatedEntityClicked;

            _rightSplitter.Panel1.Controls.Add(_graphControl);
            _rightSplitter.Panel2.Controls.Add(_detailsPanel);

            _mainSplitter.Panel1.Controls.Add(_leftSplitter);
            _mainSplitter.Panel2.Controls.Add(_rightSplitter);

            Controls.Add(_mainSplitter);
            Controls.Add(_statusStrip);
            Controls.Add(_toolStrip);
        }

        private void BuildDependencyGraph()
        {
            var builder = new DependencyGraphBuilder();
            _graph = builder.BuildFromSchema(_schema);
            _graphControl.Graph = _graph;
            UpdateStatus();
        }

        private void PopulateTreeView()
        {
            _entityTreeView.BeginUpdate();
            _entityTreeView.Nodes.Clear();

            // Group by entity type
            var groupedEntities = _graph.Nodes
                .GroupBy(e => e.EntityType)
                .OrderBy(g => g.Key.ToString());

            foreach (var group in groupedEntities)
            {
                var typeNode = new TreeNode(GetDisplayName(group.Key) + " (" + group.Count() + ")")
                {
                    Tag = group.Key,
                    BackColor = DependencyGraphControl.GetEntityColor(group.Key)
                };

                foreach (var entity in group.OrderBy(e => e.Name))
                {
                    var entityNode = new TreeNode(entity.Name)
                    {
                        Tag = entity
                    };
                    typeNode.Nodes.Add(entityNode);
                }

                _entityTreeView.Nodes.Add(typeNode);
            }

            _entityTreeView.EndUpdate();
        }

        private void EntityTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            var entity = e.Node.Tag as DatabaseEntity;
            if (entity != null)
            {
                SelectEntity(entity);
            }
        }

        private void GraphEntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (e.Entity != null)
            {
                SelectEntity(e.Entity);
                SelectEntityInTreeView(e.Entity);
            }
        }

        private void RelatedEntityClicked(object sender, EntitySelectedEventArgs e)
        {
            if (e.Entity != null)
            {
                var graphEntity = _graph.FindEntity(e.Entity.Name, e.Entity.SchemaOwner, e.Entity.EntityType);
                if (graphEntity != null)
                {
                    SelectEntity(graphEntity);
                    SelectEntityInTreeView(graphEntity);
                }
            }
        }

        private void SelectEntity(DatabaseEntity entity)
        {
            _graphControl.SelectedEntity = entity;
            _detailsPanel.SetEntity(entity);
            UpdateStatus();
        }

        private void SelectEntityInTreeView(DatabaseEntity entity)
        {
            foreach (TreeNode typeNode in _entityTreeView.Nodes)
            {
                foreach (TreeNode entityNode in typeNode.Nodes)
                {
                    if (entityNode.Tag == entity)
                    {
                        _entityTreeView.SelectedNode = entityNode;
                        entityNode.EnsureVisible();
                        return;
                    }
                }
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            var selectedTypes = _filterPanel.GetSelectedTypes();
            _graphControl.SetVisibleTypes(selectedTypes);
        }

        private void SearchChanged(object sender, SearchChangedEventArgs e)
        {
            _graphControl.SetSearchFilter(e.SearchText);
        }

        private void FindCircularDependencies(object sender, EventArgs e)
        {
            if (_graph == null)
                return;

            var cycles = _graph.FindCircularDependencies().ToList();

            if (cycles.Count == 0)
            {
                MessageBox.Show("No circular dependencies found.", "Circular Dependencies", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var message = string.Format("Found {0} circular dependency cycle(s):\n\n", cycles.Count);
                
                for (int i = 0; i < Math.Min(cycles.Count, 5); i++)
                {
                    var cycle = cycles[i];
                    message += string.Format("Cycle {0}: {1}\n", i + 1, 
                        string.Join(" -> ", cycle.Select(c => c.Name)));
                }

                if (cycles.Count > 5)
                {
                    message += string.Format("\n... and {0} more cycles.", cycles.Count - 5);
                }

                MessageBox.Show(message, "Circular Dependencies", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateStatus()
        {
            var nodeCount = _graph?.Nodes.Count ?? 0;
            var edgeCount = _graph?.Edges.Count ?? 0;
            var selected = _graphControl.SelectedEntity?.Name ?? "None";
            
            _statusLabel.Text = string.Format("Entities: {0} | Dependencies: {1} | Selected: {2}",
                nodeCount, edgeCount, selected);
        }

        private static string GetDisplayName(DatabaseEntityType type)
        {
            switch (type)
            {
                case DatabaseEntityType.PackageBody:
                    return "Package Body";
                case DatabaseEntityType.MaterializedView:
                    return "Materialized View";
                default:
                    return type.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _entityTreeView?.Dispose();
                _graphControl?.Dispose();
                _filterPanel?.Dispose();
                _detailsPanel?.Dispose();
                _mainSplitter?.Dispose();
                _leftSplitter?.Dispose();
                _rightSplitter?.Dispose();
                _toolStrip?.Dispose();
                _statusStrip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
