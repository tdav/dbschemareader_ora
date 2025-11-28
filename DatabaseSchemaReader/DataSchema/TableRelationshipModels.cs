using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Кластер связанных таблиц
    /// </summary>
    [Serializable]
    public class TableCluster
    {
        /// <summary>
        /// Идентификатор кластера
        /// </summary>
        public int ClusterId { get; set; }

        /// <summary>
        /// Имя кластера
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Таблицы в кластере
        /// </summary>
        public List<DatabaseTable> Tables { get; set; } = new List<DatabaseTable>();

        /// <summary>
        /// Количество таблиц в кластере
        /// </summary>
        public int TableCount => Tables.Count;

        /// <summary>
        /// Количество связей в кластере
        /// </summary>
        public int RelationshipCount { get; set; }

        /// <summary>
        /// Центральная таблица кластера (с наибольшим количеством связей)
        /// </summary>
        public DatabaseTable CentralTable { get; set; }
    }

    /// <summary>
    /// Связи конкретной таблицы
    /// </summary>
    [Serializable]
    public class TableRelationships
    {
        /// <summary>
        /// Таблица
        /// </summary>
        public DatabaseTable Table { get; set; }

        /// <summary>
        /// Таблицы, на которые ссылается данная таблица (родители)
        /// </summary>
        public List<TableReference> ParentTables { get; set; } = new List<TableReference>();

        /// <summary>
        /// Таблицы, которые ссылаются на данную таблицу (дети)
        /// </summary>
        public List<TableReference> ChildTables { get; set; } = new List<TableReference>();

        /// <summary>
        /// Общее количество связей
        /// </summary>
        public int TotalRelationships => ParentTables.Count + ChildTables.Count;

        /// <summary>
        /// Является ли таблица изолированной
        /// </summary>
        public bool IsIsolated => TotalRelationships == 0;
    }

    /// <summary>
    /// Ссылка на таблицу через FK
    /// </summary>
    [Serializable]
    public class TableReference
    {
        /// <summary>
        /// Связанная таблица
        /// </summary>
        public DatabaseTable Table { get; set; }

        /// <summary>
        /// Имя внешнего ключа
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Колонки внешнего ключа
        /// </summary>
        public List<string> ForeignKeyColumns { get; set; } = new List<string>();

        /// <summary>
        /// Колонки, на которые ссылается внешний ключ
        /// </summary>
        public List<string> ReferencedColumns { get; set; } = new List<string>();

        /// <summary>
        /// Имя ограничения
        /// </summary>
        public string ConstraintName { get; set; }
    }

    /// <summary>
    /// Статистика связей в схеме
    /// </summary>
    [Serializable]
    public class TableRelationshipStatistics
    {
        /// <summary>
        /// Общее количество таблиц
        /// </summary>
        public int TotalTables { get; set; }

        /// <summary>
        /// Количество связанных таблиц
        /// </summary>
        public int LinkedTables { get; set; }

        /// <summary>
        /// Количество изолированных таблиц
        /// </summary>
        public int IsolatedTables { get; set; }

        /// <summary>
        /// Общее количество связей
        /// </summary>
        public int TotalRelationships { get; set; }

        /// <summary>
        /// Количество кластеров
        /// </summary>
        public int ClusterCount { get; set; }

        /// <summary>
        /// Размер наибольшего кластера
        /// </summary>
        public int LargestClusterSize { get; set; }

        /// <summary>
        /// Таблица с наибольшим количеством связей
        /// </summary>
        public DatabaseTable MostConnectedTable { get; set; }

        /// <summary>
        /// Максимальное количество связей
        /// </summary>
        public int MaxConnectionsCount { get; set; }
    }
}
