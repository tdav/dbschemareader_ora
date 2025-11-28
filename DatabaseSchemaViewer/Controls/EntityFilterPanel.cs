using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer.Controls
{
    /// <summary>
    /// Panel for filtering entities by type
    /// </summary>
    public class EntityFilterPanel : UserControl
    {
        private readonly Dictionary<DatabaseEntityType, CheckBox> _checkBoxes;
        private TextBox _searchTextBox;
        private Button _selectAllButton;
        private Button _clearAllButton;
        private FlowLayoutPanel _checkBoxPanel;

        /// <summary>
        /// Event raised when filter changes
        /// </summary>
        public event EventHandler FilterChanged;

        /// <summary>
        /// Event raised when search text changes
        /// </summary>
        public event EventHandler<SearchChangedEventArgs> SearchChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFilterPanel"/> class
        /// </summary>
        public EntityFilterPanel()
        {
            _checkBoxes = new Dictionary<DatabaseEntityType, CheckBox>();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Search panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(5)
            };

            var searchLabel = new Label
            {
                Text = "Search:",
                AutoSize = true,
                Location = new System.Drawing.Point(5, 8)
            };

            _searchTextBox = new TextBox
            {
                Location = new System.Drawing.Point(55, 5),
                Width = 150
            };
            _searchTextBox.TextChanged += (s, e) =>
            {
                SearchChanged?.Invoke(this, new SearchChangedEventArgs(_searchTextBox.Text));
            };

            searchPanel.Controls.AddRange(new Control[] { searchLabel, _searchTextBox });

            // Button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(5)
            };

            _selectAllButton = new Button
            {
                Text = "Select All",
                AutoSize = true,
                Location = new System.Drawing.Point(5, 3)
            };
            _selectAllButton.Click += SelectAllClick;

            _clearAllButton = new Button
            {
                Text = "Clear All",
                AutoSize = true,
                Location = new System.Drawing.Point(90, 3)
            };
            _clearAllButton.Click += ClearAllClick;

            buttonPanel.Controls.AddRange(new Control[] { _selectAllButton, _clearAllButton });

            // CheckBox panel
            _checkBoxPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(5)
            };

            // Add checkboxes for each entity type
            foreach (DatabaseEntityType type in Enum.GetValues(typeof(DatabaseEntityType)))
            {
                var checkBox = new CheckBox
                {
                    Text = GetDisplayName(type),
                    Checked = true,
                    AutoSize = true,
                    Tag = type,
                    BackColor = DependencyGraphControl.GetEntityColor(type),
                    Padding = new Padding(3)
                };
                checkBox.CheckedChanged += CheckBoxCheckedChanged;
                _checkBoxes[type] = checkBox;
                _checkBoxPanel.Controls.Add(checkBox);
            }

            Controls.Add(_checkBoxPanel);
            Controls.Add(buttonPanel);
            Controls.Add(searchPanel);
        }

        /// <summary>
        /// Gets the selected entity types
        /// </summary>
        public IEnumerable<DatabaseEntityType> GetSelectedTypes()
        {
            return _checkBoxes
                .Where(kvp => kvp.Value.Checked)
                .Select(kvp => kvp.Key);
        }

        /// <summary>
        /// Gets the search text
        /// </summary>
        public string SearchText
        {
            get { return _searchTextBox.Text; }
        }

        private void CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SelectAllClick(object sender, EventArgs e)
        {
            foreach (var checkBox in _checkBoxes.Values)
            {
                checkBox.Checked = true;
            }
        }

        private void ClearAllClick(object sender, EventArgs e)
        {
            foreach (var checkBox in _checkBoxes.Values)
            {
                checkBox.Checked = false;
            }
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
    }

    /// <summary>
    /// Event arguments for search text changes
    /// </summary>
    public class SearchChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the search text
        /// </summary>
        public string SearchText { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchChangedEventArgs"/> class
        /// </summary>
        /// <param name="searchText">The search text</param>
        public SearchChangedEventArgs(string searchText)
        {
            SearchText = searchText;
        }
    }
}
