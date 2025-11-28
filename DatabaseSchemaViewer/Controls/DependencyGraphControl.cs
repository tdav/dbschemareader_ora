using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer.Controls
{
    /// <summary>
    /// Custom control for visualizing dependency graphs
    /// </summary>
    public class DependencyGraphControl : UserControl
    {
        private static readonly DatabaseEntityType[] AllEntityTypes = 
            (DatabaseEntityType[])Enum.GetValues(typeof(DatabaseEntityType));

        private DependencyGraph _graph;
        private readonly Dictionary<DatabaseEntity, PointF> _nodePositions;
        private readonly Dictionary<DatabaseEntity, SizeF> _nodeSizes;
        private DatabaseEntity _selectedEntity;
        private DatabaseEntity _hoveredEntity;
        private PointF _offset;
        private float _zoom;
        private Point _lastMousePosition;
        private bool _isDragging;
        private HashSet<DatabaseEntityType> _visibleTypes;
        private string _searchFilter;

        /// <summary>
        /// Event raised when an entity is selected
        /// </summary>
        public event EventHandler<EntitySelectedEventArgs> EntitySelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyGraphControl"/> class
        /// </summary>
        public DependencyGraphControl()
        {
            _nodePositions = new Dictionary<DatabaseEntity, PointF>();
            _nodeSizes = new Dictionary<DatabaseEntity, SizeF>();
            _visibleTypes = new HashSet<DatabaseEntityType>(AllEntityTypes);
            _zoom = 1.0f;
            _offset = PointF.Empty;
            _searchFilter = string.Empty;

            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.White;

            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;
            MouseDoubleClick += OnMouseDoubleClick;
        }

        /// <summary>
        /// Gets or sets the dependency graph
        /// </summary>
        public DependencyGraph Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                CalculateLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the selected entity
        /// </summary>
        public DatabaseEntity SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                _selectedEntity = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Sets the visible entity types
        /// </summary>
        public void SetVisibleTypes(IEnumerable<DatabaseEntityType> types)
        {
            _visibleTypes = new HashSet<DatabaseEntityType>(types);
            CalculateLayout();
            Invalidate();
        }

        /// <summary>
        /// Sets the search filter
        /// </summary>
        public void SetSearchFilter(string filter)
        {
            _searchFilter = filter ?? string.Empty;
            Invalidate();
        }

        /// <summary>
        /// Resets the view to default zoom and position
        /// </summary>
        public void ResetView()
        {
            _zoom = 1.0f;
            _offset = PointF.Empty;
            Invalidate();
        }

        /// <summary>
        /// Centers the view on the selected entity
        /// </summary>
        public void CenterOnSelected()
        {
            if (_selectedEntity != null && _nodePositions.ContainsKey(_selectedEntity))
            {
                var pos = _nodePositions[_selectedEntity];
                _offset = new PointF(
                    Width / 2f - pos.X * _zoom,
                    Height / 2f - pos.Y * _zoom);
                Invalidate();
            }
        }

        private void CalculateLayout()
        {
            _nodePositions.Clear();
            _nodeSizes.Clear();

            if (_graph == null || _graph.Nodes.Count == 0)
                return;

            var visibleNodes = _graph.Nodes
                .Where(n => _visibleTypes.Contains(n.EntityType))
                .ToList();

            if (visibleNodes.Count == 0)
                return;

            // Group nodes by type for layered layout
            var nodesByType = visibleNodes.GroupBy(n => n.EntityType).ToList();

            using (var g = CreateGraphics())
            {
                var font = Font;
                var padding = 20f;
                var yOffset = 50f;

                foreach (var typeGroup in nodesByType)
                {
                    var xOffset = 50f;
                    var maxHeight = 0f;

                    foreach (var node in typeGroup)
                    {
                        var displayName = node.Name ?? "Unknown";
                        var size = g.MeasureString(displayName, font);
                        size.Width += 20;
                        size.Height += 10;

                        _nodeSizes[node] = size;
                        _nodePositions[node] = new PointF(xOffset + size.Width / 2, yOffset + size.Height / 2);

                        xOffset += size.Width + padding;
                        maxHeight = Math.Max(maxHeight, size.Height);

                        // Wrap to next row if needed
                        if (xOffset > 1000)
                        {
                            xOffset = 50f;
                            yOffset += maxHeight + padding;
                            maxHeight = 0f;
                        }
                    }

                    yOffset += maxHeight + padding * 2;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(_offset.X, _offset.Y);
            g.ScaleTransform(_zoom, _zoom);

            if (_graph == null)
                return;

            // Draw edges first
            DrawEdges(g);

            // Draw nodes
            DrawNodes(g);
        }

        private void DrawEdges(Graphics g)
        {
            using (var pen = new Pen(Color.Gray, 1))
            {
                pen.EndCap = LineCap.ArrowAnchor;

                foreach (var edge in _graph.Edges)
                {
                    var sourceNode = _graph.Nodes.FirstOrDefault(n =>
                        string.Equals(n.SchemaOwner, edge.OwnerName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(n.Name, edge.ObjectName, StringComparison.OrdinalIgnoreCase));

                    var targetNode = _graph.Nodes.FirstOrDefault(n =>
                        string.Equals(n.SchemaOwner, edge.ReferencedOwner, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(n.Name, edge.ReferencedName, StringComparison.OrdinalIgnoreCase));

                    if (sourceNode == null || targetNode == null)
                        continue;

                    if (!_visibleTypes.Contains(sourceNode.EntityType) ||
                        !_visibleTypes.Contains(targetNode.EntityType))
                        continue;

                    if (!_nodePositions.ContainsKey(sourceNode) || !_nodePositions.ContainsKey(targetNode))
                        continue;

                    var sourcePos = _nodePositions[sourceNode];
                    var targetPos = _nodePositions[targetNode];

                    // Highlight edges for selected entity
                    if (_selectedEntity != null &&
                        (sourceNode == _selectedEntity || targetNode == _selectedEntity))
                    {
                        pen.Color = Color.Blue;
                        pen.Width = 2;
                    }
                    else
                    {
                        pen.Color = Color.LightGray;
                        pen.Width = 1;
                    }

                    g.DrawLine(pen, sourcePos, targetPos);
                }
            }
        }

        private void DrawNodes(Graphics g)
        {
            foreach (var node in _graph.Nodes)
            {
                if (!_visibleTypes.Contains(node.EntityType))
                    continue;

                if (!_nodePositions.ContainsKey(node))
                    continue;

                var pos = _nodePositions[node];
                var size = _nodeSizes[node];
                var rect = new RectangleF(
                    pos.X - size.Width / 2,
                    pos.Y - size.Height / 2,
                    size.Width,
                    size.Height);

                // Check if matches search filter
                var matchesFilter = string.IsNullOrEmpty(_searchFilter) ||
                    (node.Name != null && node.Name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

                // Get node color
                var nodeColor = GetEntityColor(node.EntityType);
                if (!matchesFilter)
                {
                    nodeColor = Color.FromArgb(100, nodeColor);
                }

                // Draw node background
                using (var brush = new SolidBrush(nodeColor))
                {
                    g.FillRectangle(brush, rect);
                }

                // Draw border
                var borderColor = node == _selectedEntity ? Color.Blue :
                                 node == _hoveredEntity ? Color.DarkBlue : Color.DarkGray;
                var borderWidth = node == _selectedEntity ? 3 : 1;
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                // Draw text
                var displayName = node.Name ?? "Unknown";
                using (var brush = new SolidBrush(Color.Black))
                using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(displayName, Font, brush, rect, format);
                }
            }
        }

        /// <summary>
        /// Gets the color for an entity type
        /// </summary>
        public static Color GetEntityColor(DatabaseEntityType type)
        {
            switch (type)
            {
                case DatabaseEntityType.Table:
                    return ColorTranslator.FromHtml("#4A90D9"); // Blue
                case DatabaseEntityType.View:
                    return ColorTranslator.FromHtml("#7EC8E3"); // Light blue
                case DatabaseEntityType.Procedure:
                    return ColorTranslator.FromHtml("#50C878"); // Green
                case DatabaseEntityType.Function:
                    return ColorTranslator.FromHtml("#90EE90"); // Light green
                case DatabaseEntityType.Package:
                case DatabaseEntityType.PackageBody:
                    return ColorTranslator.FromHtml("#FFA500"); // Orange
                case DatabaseEntityType.Trigger:
                    return ColorTranslator.FromHtml("#FF6B6B"); // Red
                case DatabaseEntityType.Sequence:
                    return ColorTranslator.FromHtml("#808080"); // Gray
                case DatabaseEntityType.Index:
                    return ColorTranslator.FromHtml("#FFD700"); // Yellow
                case DatabaseEntityType.Type:
                    return ColorTranslator.FromHtml("#9370DB"); // Purple
                case DatabaseEntityType.MaterializedView:
                    return ColorTranslator.FromHtml("#6495ED"); // Cornflower blue
                case DatabaseEntityType.Synonym:
                    return ColorTranslator.FromHtml("#DDA0DD"); // Plum
                case DatabaseEntityType.Constraint:
                    return ColorTranslator.FromHtml("#F0E68C"); // Khaki
                default:
                    return Color.LightGray;
            }
        }

        private DatabaseEntity GetEntityAtPoint(Point screenPoint)
        {
            if (_graph == null)
                return null;

            // Convert screen point to graph coordinates
            var graphX = (screenPoint.X - _offset.X) / _zoom;
            var graphY = (screenPoint.Y - _offset.Y) / _zoom;

            foreach (var node in _graph.Nodes)
            {
                if (!_visibleTypes.Contains(node.EntityType))
                    continue;

                if (!_nodePositions.ContainsKey(node))
                    continue;

                var pos = _nodePositions[node];
                var size = _nodeSizes[node];
                var rect = new RectangleF(
                    pos.X - size.Width / 2,
                    pos.Y - size.Height / 2,
                    size.Width,
                    size.Height);

                if (rect.Contains(graphX, graphY))
                    return node;
            }

            return null;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var entity = GetEntityAtPoint(e.Location);
                if (entity != null)
                {
                    SelectedEntity = entity;
                    EntitySelected?.Invoke(this, new EntitySelectedEventArgs(entity));
                }
                else
                {
                    _isDragging = true;
                    _lastMousePosition = e.Location;
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _offset.X += e.X - _lastMousePosition.X;
                _offset.Y += e.Y - _lastMousePosition.Y;
                _lastMousePosition = e.Location;
                Invalidate();
            }
            else
            {
                var entity = GetEntityAtPoint(e.Location);
                if (entity != _hoveredEntity)
                {
                    _hoveredEntity = entity;
                    Cursor = entity != null ? Cursors.Hand : Cursors.Default;
                    Invalidate();
                }
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            var oldZoom = _zoom;
            _zoom *= (e.Delta > 0) ? 1.1f : 0.9f;
            _zoom = Math.Max(0.1f, Math.Min(5f, _zoom));

            // Adjust offset to zoom toward mouse position
            var zoomRatio = _zoom / oldZoom;
            _offset.X = e.X - (e.X - _offset.X) * zoomRatio;
            _offset.Y = e.Y - (e.Y - _offset.Y) * zoomRatio;

            Invalidate();
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var entity = GetEntityAtPoint(e.Location);
            if (entity != null)
            {
                CenterOnSelected();
            }
        }
    }

    /// <summary>
    /// Event arguments for entity selection
    /// </summary>
    public class EntitySelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the selected entity
        /// </summary>
        public DatabaseEntity Entity { get; private set; }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public EntitySelectedEventArgs(DatabaseEntity entity)
        {
            Entity = entity;
        }
    }
}
