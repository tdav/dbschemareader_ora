# DatabaseSchemaViewer

A simple Windows Forms UI (net48) for browsing and analyzing database schemas using `DatabaseSchemaReader`.

## Features
- Connect to databases via ADO.NET providers
- Read full schema: tables, views, columns, keys, indexes, procedures, functions
- Treeview navigation of schema objects
- Code generation:
  - POCO classes
  - NHibernate and EF Code First mappings
- SQL generation:
  - Table DDL (with cross-database translation)
  - CRUD stored procedures (SqlServer, Oracle, MySQL, DB2)
- Schema comparison to another database

## Requirements
- Windows with .NET Framework 4.8
- Supported providers installed (e.g., `System.Data.SqlClient`, Oracle ODP.NET, MySQL, PostgreSQL, SQLite, etc.)

## Build
- Open `DatabaseSchemaViewer.csproj` in Visual Studio 2022
- Restore NuGet packages
- Build the solution (msbuild or VS)

> Note: `dotnet build` does not support legacy TFMs in this solution; use Visual Studio/MSBuild.

## Run
- Start the application from Visual Studio (F5) or run the built `DatabaseSchemaViewer.exe`

## Usage
1. Launch the app
2. Choose provider and enter connection string
3. Click "Read Schema" to populate the tree
4. Use menu options for code generation, DDL, stored procedures, and schema comparison

## Limitations
- UI targets net48; cross-platform is not supported for this app
- Some providers may not expose all metadata consistently

## License
See repository `README.md` and NuGet page for licensing and package details.

# DatabaseSchemaViewer — описание проекта

Приложение Windows Forms на .NET Framework 4.8 для просмотра и анализа схем баз данных на основе библиотеки `DatabaseSchemaReader`.

## Структура проекта и назначение файлов

### Точка входа
- `Program.cs`
  - Точка входа приложения. Инициализирует визуальные стили и запускает основную форму `Form1`.

### Основные формы
- `Form1.cs` / `Form1.Designer.cs`
  - Главная форма приложения.
  - Возможности:
    - Подключение к базе данных через провайдера ADO.NET и строку подключения.
    - Чтение полной схемы (таблицы, представления, столбцы, ключи, индексы, процедуры, функции).
    - Отображение объектов в `TreeView` (через помощник `SchemaToTreeview`).
    - Запуск генерации кода (форма `CodeGenForm`).
    - Генерация SQL-скриптов (форма `ScriptForm`).
    - Просмотр связей таблиц (форма `TableRelationshipForm`).
    - Просмотр зависимостей (форма `DependencyViewerForm`).
    - Сравнение схем (форма `CompareForm`).

- `CodeGenForm.cs` / `CodeGenForm.Designer.cs`
  - Форма генерации кода.
  - Функциональность:
    - Генерация POCO-классов по таблицам.
    - Генерация маппингов NHibernate и EF Code First.
    - Запуск задач генерации через `CodeWriterRunner` / `TaskRunner`.

- `ScriptForm.cs` / `ScriptForm.Designer.cs`
  - Форма генерации SQL.
  - Функциональность:
    - Генерация DDL для таблиц с возможностью трансляции синтаксиса.
    - Генерация CRUD-хранимых процедур (SqlServer, Oracle, MySQL, DB2).
    - Запуск задач SQL через `SqlTasks`.

- `TableRelationshipForm.cs` / `TableRelationshipForm.Designer.cs`
  - Форма просмотра связей между таблицами.
  - Функциональность:
    - Визуализация внешних ключей и связей.
    - Навигация по структуре связей.

- `DependencyViewerForm.cs`
  - Форма просмотра зависимостей объектов базы данных (таблиц, представлений, процедур).
  - Использует `Controls.DependencyGraphControl` для отображения графа зависимостей.

- `CompareForm.cs` / `CompareForm.Designer.cs`
  - Форма сравнения схем двух баз данных.
  - Функциональность:
    - Настройка подключения к исходной и целевой БД.
    - Запуск сравнения через `CompareRunner`.
    - Отображение различий и генерация миграционного скрипта.

### Вспомогательные классы и контролы
- `SchemaToTreeview.cs`
  - Помощник для преобразования модели схемы (`DatabaseSchemaReader`) в узлы `TreeView` главной формы.

- `CodeWriterRunner.cs`
  - Оркестрация операций генерации кода.
  - Интерфейс между формой `CodeGenForm` и генераторами из `DatabaseSchemaReader`.

- `SqlTasks.cs`
  - Набор операций для генерации SQL-скриптов и выполнения типовых задач.

- `TaskRunner.cs`
  - Базовая инфраструктура для запуска длительных задач (генерация кода, скриптов) с отображением прогресса и обработкой ошибок.

- `CompareRunner.cs`
  - Логика сравнения двух схем (инициализация чтения, вызов сравнения, формирование результата/скрипта).

- `Controls.EntityDetailsPanel.cs`
  - Пользовательский `Panel` для показа подробностей по выбранному объекту (таблица, колонка, индекс и т.п.).

- `Controls.EntityFilterPanel.cs`
  - Пользовательский `Panel` для фильтрации списка объектов (по имени, типу, схеме и т.д.).

- `Controls.DependencyGraphControl.cs`
  - Пользовательский контрол для отображения графа зависимостей объектов базы данных.

### Ресурсы и свойства
- `Properties\AssemblyInfo.cs`
  - Метаинформация сборки (название, версия и т.д.).

- `Properties\Resources.Designer.cs`
  - Автогенерируемый доступ к ресурсам (иконки, строки и пр.).

- `Properties\Settings.Designer.cs`
  - Автогенерируемые настройки приложения (сохранение параметров подключения и т.п.).

## Требования
- Windows, .NET Framework 4.8
- Установленные провайдеры ADO.NET (например, `System.Data.SqlClient`, ODP.NET для Oracle, MySQL, PostgreSQL, SQLite и др.)

## Сборка
- Открыть `DatabaseSchemaViewer.csproj` в Visual Studio 2022
- Восстановить NuGet-пакеты
- Собрать решение (в VS или через MSBuild)
  
Примечание: `dotnet build` не поддерживает устаревшие целевые фреймворки (net35/net40) в этом решении, используйте Visual Studio/MSBuild.

## Запуск
- Запустить из Visual Studio (F5) или выполнить `DatabaseSchemaViewer.exe` из папки сборки

## Использование
1. Открыть приложение
2. Выбрать провайдера и ввести строку подключения
3. Нажать «Читать схему» для заполнения дерева
4. Использовать меню для генерации кода, DDL, CRUD процедур, сравнения схем и просмотра зависимостей

## Ограничения
- UI ориентирован на .NET Framework 4.8 и Windows
- Метаданные разных провайдеров могут отличаться по полноте и совместимости

## Лицензия
См. корневой `README.md` и страницу пакета на NuGet для подробностей о лицензировании.
