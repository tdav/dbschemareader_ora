using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Анализатор связей между таблицами
    /// </summary>
    public class TableRelationshipAnalyzer
    {
        private readonly DatabaseSchema _schema;

        /// <summary>
        /// Инициализирует новый экземпляр класса TableRelationshipAnalyzer
        /// </summary>
        /// <param name="schema">Схема базы данных</param>
        public TableRelationshipAnalyzer(DatabaseSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        /// <summary>
        /// Получить все связанные таблицы (имеют FK связи)
        /// </summary>
        /// <returns>Список связанных таблиц</returns>
        public List<DatabaseTable> GetLinkedTables()
        {
            var linkedTables = new HashSet<DatabaseTable>();

            foreach (var table in _schema.Tables)
            {
                // Таблица имеет FK (ссылается на другие таблицы)
                if (table.ForeignKeys.Any(fk => !string.IsNullOrEmpty(fk.RefersToTable)))
                {
                    linkedTables.Add(table);

                    // Добавляем таблицы, на которые ссылаются
                    foreach (var fk in table.ForeignKeys)
                    {
                        var referencedTable = fk.ReferencedTable(_schema);
                        if (referencedTable != null)
                        {
                            linkedTables.Add(referencedTable);
                        }
                    }
                }

                // Таблица имеет дочерние таблицы (другие таблицы ссылаются на неё)
                if (table.ForeignKeyChildren.Any())
                {
                    linkedTables.Add(table);
                    foreach (var child in table.ForeignKeyChildren)
                    {
                        linkedTables.Add(child);
                    }
                }
            }

            return linkedTables.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Получить все несвязанные (изолированные) таблицы
        /// </summary>
        /// <returns>Список изолированных таблиц</returns>
        public List<DatabaseTable> GetIsolatedTables()
        {
            var linkedTables = new HashSet<DatabaseTable>(GetLinkedTables());
            return _schema.Tables
                .Where(t => !linkedTables.Contains(t))
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Получить группы связанных таблиц (кластеры)
        /// </summary>
        /// <returns>Список кластеров</returns>
        public List<TableCluster> GetTableClusters()
        {
            var clusters = new List<TableCluster>();
            var visited = new HashSet<DatabaseTable>();
            var linkedTables = GetLinkedTables();
            int clusterId = 1;

            foreach (var table in linkedTables)
            {
                if (visited.Contains(table))
                    continue;

                var clusterTables = new List<DatabaseTable>();
                var queue = new Queue<DatabaseTable>();
                queue.Enqueue(table);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    if (visited.Contains(current))
                        continue;

                    visited.Add(current);
                    clusterTables.Add(current);

                    // Добавляем родительские таблицы (на которые ссылается текущая)
                    foreach (var fk in current.ForeignKeys)
                    {
                        var referencedTable = fk.ReferencedTable(_schema);
                        if (referencedTable != null && !visited.Contains(referencedTable))
                        {
                            queue.Enqueue(referencedTable);
                        }
                    }

                    // Добавляем дочерние таблицы (которые ссылаются на текущую)
                    foreach (var child in current.ForeignKeyChildren)
                    {
                        if (!visited.Contains(child))
                        {
                            queue.Enqueue(child);
                        }
                    }
                }

                if (clusterTables.Any())
                {
                    var cluster = new TableCluster
                    {
                        ClusterId = clusterId++,
                        Tables = clusterTables.OrderBy(t => t.Name).ToList(),
                        RelationshipCount = CountClusterRelationships(clusterTables)
                    };

                    // Находим центральную таблицу (с наибольшим количеством связей)
                    cluster.CentralTable = clusterTables
                        .OrderByDescending(t => GetConnectionCount(t))
                        .FirstOrDefault();

                    cluster.Name = cluster.CentralTable?.Name ?? $"Cluster_{cluster.ClusterId}";

                    clusters.Add(cluster);
                }
            }

            return clusters.OrderByDescending(c => c.TableCount).ToList();
        }

        /// <summary>
        /// Получить все связи для конкретной таблицы
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <returns>Связи таблицы</returns>
        public TableRelationships GetTableRelationships(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            var table = _schema.FindTableByName(tableName);
            if (table == null)
                return null;

            return GetTableRelationships(table);
        }

        /// <summary>
        /// Получить все связи для конкретной таблицы
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <returns>Связи таблицы</returns>
        public TableRelationships GetTableRelationships(DatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var relationships = new TableRelationships
            {
                Table = table,
                ParentTables = new List<TableReference>(),
                ChildTables = new List<TableReference>()
            };

            // Родительские таблицы (на которые ссылается данная таблица через FK)
            foreach (var fk in table.ForeignKeys)
            {
                var referencedTable = fk.ReferencedTable(_schema);
                if (referencedTable != null)
                {
                    var referencedColumns = fk.ReferencedColumns(_schema)?.ToList() ?? new List<string>();
                    relationships.ParentTables.Add(new TableReference
                    {
                        Table = referencedTable,
                        ForeignKeyName = fk.Name,
                        ForeignKeyColumns = fk.Columns.ToList(),
                        ReferencedColumns = referencedColumns,
                        ConstraintName = fk.RefersToConstraint
                    });
                }
            }

            // Дочерние таблицы (которые ссылаются на данную таблицу)
            foreach (var childTable in table.ForeignKeyChildren)
            {
                foreach (var fk in childTable.ForeignKeys)
                {
                    if (string.Equals(fk.RefersToTable, table.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var referencedColumns = fk.ReferencedColumns(_schema)?.ToList() ?? new List<string>();
                        relationships.ChildTables.Add(new TableReference
                        {
                            Table = childTable,
                            ForeignKeyName = fk.Name,
                            ForeignKeyColumns = fk.Columns.ToList(),
                            ReferencedColumns = referencedColumns,
                            ConstraintName = fk.RefersToConstraint
                        });
                    }
                }
            }

            return relationships;
        }

        /// <summary>
        /// Получить цепочку связей между двумя таблицами (BFS поиск)
        /// </summary>
        /// <param name="fromTable">Исходная таблица</param>
        /// <param name="toTable">Целевая таблица</param>
        /// <returns>Путь между таблицами или null если путь не найден</returns>
        public List<DatabaseTable> FindPathBetweenTables(string fromTable, string toTable)
        {
            if (string.IsNullOrEmpty(fromTable))
                throw new ArgumentNullException(nameof(fromTable));
            if (string.IsNullOrEmpty(toTable))
                throw new ArgumentNullException(nameof(toTable));

            var startTable = _schema.FindTableByName(fromTable);
            var endTable = _schema.FindTableByName(toTable);

            if (startTable == null || endTable == null)
                return null;

            if (startTable == endTable)
                return new List<DatabaseTable> { startTable };

            // BFS поиск пути
            var visited = new HashSet<DatabaseTable>();
            var queue = new Queue<List<DatabaseTable>>();
            queue.Enqueue(new List<DatabaseTable> { startTable });

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path.Last();

                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                // Проверяем соседние таблицы
                var neighbors = GetAdjacentTables(current);
                foreach (var neighbor in neighbors)
                {
                    if (visited.Contains(neighbor))
                        continue;

                    var newPath = new List<DatabaseTable>(path) { neighbor };

                    if (neighbor == endTable)
                        return newPath;

                    queue.Enqueue(newPath);
                }
            }

            return null; // Путь не найден
        }

        /// <summary>
        /// Получить все таблицы на N уровней связи от указанной
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="depth">Глубина поиска</param>
        /// <returns>Таблицы в пределах указанной глубины</returns>
        public List<DatabaseTable> GetRelatedTablesWithinDepth(string tableName, int depth)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));
            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be non-negative");

            var startTable = _schema.FindTableByName(tableName);
            if (startTable == null)
                return new List<DatabaseTable>();

            if (depth == 0)
                return new List<DatabaseTable> { startTable };

            var result = new HashSet<DatabaseTable> { startTable };
            var currentLevel = new HashSet<DatabaseTable> { startTable };

            for (int i = 0; i < depth; i++)
            {
                var nextLevel = new HashSet<DatabaseTable>();

                foreach (var table in currentLevel)
                {
                    var neighbors = GetAdjacentTables(table);
                    foreach (var neighbor in neighbors)
                    {
                        if (!result.Contains(neighbor))
                        {
                            nextLevel.Add(neighbor);
                            result.Add(neighbor);
                        }
                    }
                }

                if (nextLevel.Count == 0)
                    break;

                currentLevel = nextLevel;
            }

            return result.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Получить статистику связей таблиц
        /// </summary>
        /// <returns>Статистика связей</returns>
        public TableRelationshipStatistics GetRelationshipStatistics()
        {
            var linkedTables = GetLinkedTables();
            var isolatedTables = GetIsolatedTables();
            var clusters = GetTableClusters();

            int totalRelationships = 0;
            DatabaseTable mostConnected = null;
            int maxConnections = 0;

            foreach (var table in _schema.Tables)
            {
                var connectionCount = GetConnectionCount(table);
                totalRelationships += table.ForeignKeys.Count;

                if (connectionCount > maxConnections)
                {
                    maxConnections = connectionCount;
                    mostConnected = table;
                }
            }

            return new TableRelationshipStatistics
            {
                TotalTables = _schema.Tables.Count,
                LinkedTables = linkedTables.Count,
                IsolatedTables = isolatedTables.Count,
                TotalRelationships = totalRelationships,
                ClusterCount = clusters.Count,
                LargestClusterSize = clusters.Any() ? clusters.Max(c => c.TableCount) : 0,
                MostConnectedTable = mostConnected,
                MaxConnectionsCount = maxConnections
            };
        }

        #region Private Methods

        private int GetConnectionCount(DatabaseTable table)
        {
            return table.ForeignKeys.Count + table.ForeignKeyChildren.Count;
        }

        private int CountClusterRelationships(List<DatabaseTable> tables)
        {
            int count = 0;
            var tableSet = new HashSet<DatabaseTable>(tables);

            foreach (var table in tables)
            {
                foreach (var fk in table.ForeignKeys)
                {
                    var referencedTable = fk.ReferencedTable(_schema);
                    if (referencedTable != null && tableSet.Contains(referencedTable))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private IEnumerable<DatabaseTable> GetAdjacentTables(DatabaseTable table)
        {
            var neighbors = new HashSet<DatabaseTable>();

            // Родительские таблицы (на которые ссылается)
            foreach (var fk in table.ForeignKeys)
            {
                var referencedTable = fk.ReferencedTable(_schema);
                if (referencedTable != null)
                {
                    neighbors.Add(referencedTable);
                }
            }

            // Дочерние таблицы (которые ссылаются)
            foreach (var child in table.ForeignKeyChildren)
            {
                neighbors.Add(child);
            }

            return neighbors;
        }

        #endregion
    }
}
