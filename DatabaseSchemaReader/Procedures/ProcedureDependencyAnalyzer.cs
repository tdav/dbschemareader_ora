using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Analyzes procedure/function source code to extract dependencies
    /// </summary>
    public class ProcedureDependencyAnalyzer
    {
        /// <summary>
        /// Extracts table references from source code
        /// </summary>
        /// <param name="sourceCode">The PL/SQL source code</param>
        /// <returns>List of table names</returns>
        public List<string> ExtractTableReferences(string sourceCode)
        {
            var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sourceCode))
                return new List<string>();

            // Remove comments before analysis
            var cleanedCode = RemoveComments(sourceCode);

            // Pattern for FROM clause
            var fromPattern = new Regex(
                @"\bFROM\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);

            // Pattern for INSERT INTO
            var insertPattern = new Regex(
                @"\bINSERT\s+INTO\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);

            // Pattern for UPDATE
            var updatePattern = new Regex(
                @"\bUPDATE\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);

            // Pattern for DELETE FROM
            var deletePattern = new Regex(
                @"\bDELETE\s+(?:FROM\s+)?([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);

            // Pattern for JOIN
            var joinPattern = new Regex(
                @"\bJOIN\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);

            // Pattern for MERGE INTO
            var mergePattern = new Regex(
                @"\bMERGE\s+INTO\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);

            ExtractMatches(cleanedCode, fromPattern, tables);
            ExtractMatches(cleanedCode, insertPattern, tables);
            ExtractMatches(cleanedCode, updatePattern, tables);
            ExtractMatches(cleanedCode, deletePattern, tables);
            ExtractMatches(cleanedCode, joinPattern, tables);
            ExtractMatches(cleanedCode, mergePattern, tables);

            return new List<string>(tables);
        }

        /// <summary>
        /// Analyzes source code and returns categorized dependencies
        /// </summary>
        /// <param name="sourceCode">The PL/SQL source code</param>
        /// <returns>Categorized dependencies</returns>
        public SourceCodeDependencyResult AnalyzeSourceCode(string sourceCode)
        {
            var result = new SourceCodeDependencyResult();
            if (string.IsNullOrEmpty(sourceCode))
                return result;

            // Remove comments before analysis
            var cleanedCode = RemoveComments(sourceCode);

            // Extract tables from FROM clauses
            var fromPattern = new Regex(
                @"\bFROM\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);
            ExtractMatchesToList(cleanedCode, fromPattern, result.ReadTables);

            // Extract JOIN tables
            var joinPattern = new Regex(
                @"\bJOIN\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);
            ExtractMatchesToList(cleanedCode, joinPattern, result.ReadTables);

            // Extract INSERT tables
            var insertPattern = new Regex(
                @"\bINSERT\s+INTO\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);
            ExtractMatchesToList(cleanedCode, insertPattern, result.InsertTables);

            // Extract UPDATE tables
            var updatePattern = new Regex(
                @"\bUPDATE\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);
            ExtractMatchesToList(cleanedCode, updatePattern, result.UpdateTables);

            // Extract DELETE tables
            var deletePattern = new Regex(
                @"\bDELETE\s+(?:FROM\s+)?([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);
            ExtractMatchesToList(cleanedCode, deletePattern, result.DeleteTables);

            // Extract MERGE tables
            var mergePattern = new Regex(
                @"\bMERGE\s+INTO\s+([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)",
                RegexOptions.IgnoreCase);
            ExtractMatchesToList(cleanedCode, mergePattern, result.MergeTables);

            // Extract package calls
            result.PackageCalls.AddRange(ExtractPackageCalls(cleanedCode));

            // Check for dynamic SQL
            result.ContainsDynamicSql = ContainsDynamicSql(sourceCode);

            return result;
        }

        /// <summary>
        /// Checks if source code contains dynamic SQL
        /// </summary>
        /// <param name="sourceCode">The PL/SQL source code</param>
        /// <returns>True if dynamic SQL is found</returns>
        public bool ContainsDynamicSql(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
                return false;

            // Pattern for EXECUTE IMMEDIATE
            var executeImmediatePattern = new Regex(
                @"\bEXECUTE\s+IMMEDIATE\b",
                RegexOptions.IgnoreCase);

            // Pattern for DBMS_SQL
            var dbmsSqlPattern = new Regex(
                @"\bDBMS_SQL\.",
                RegexOptions.IgnoreCase);

            return executeImmediatePattern.IsMatch(sourceCode) || 
                   dbmsSqlPattern.IsMatch(sourceCode);
        }

        /// <summary>
        /// Removes comments from PL/SQL source code
        /// </summary>
        /// <param name="sourceCode">The source code</param>
        /// <returns>Source code without comments</returns>
        private string RemoveComments(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
                return sourceCode;

            // Remove single-line comments (-- ...)
            var singleLinePattern = new Regex(@"--.*$", RegexOptions.Multiline);
            var result = singleLinePattern.Replace(sourceCode, string.Empty);

            // Remove multi-line comments (/* ... */)
            var multiLinePattern = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
            result = multiLinePattern.Replace(result, string.Empty);

            return result;
        }

        private void ExtractMatchesToList(string sourceCode, Regex pattern, List<string> tables)
        {
            var matches = pattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var name = match.Groups[1].Value;
                if (!IsSqlKeyword(name) && !string.IsNullOrEmpty(name) && !tables.Contains(name))
                {
                    tables.Add(name);
                }
            }
        }

        /// <summary>
        /// Extracts procedure calls from source code
        /// </summary>
        /// <param name="sourceCode">The PL/SQL source code</param>
        /// <returns>List of procedure names</returns>
        public List<string> ExtractProcedureCalls(string sourceCode)
        {
            var procedures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sourceCode))
                return new List<string>();

            // Pattern for procedure calls (not SELECT/INSERT/UPDATE/DELETE/FROM)
            // Look for identifiers followed by semicolon or with parameters
            var procPattern = new Regex(
                @"(?<!\.)\b([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)\s*\(",
                RegexOptions.IgnoreCase);

            var matches = procPattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var name = match.Groups[1].Value;
                if (!IsSqlKeyword(name) && !IsBuiltInFunction(name))
                {
                    procedures.Add(name);
                }
            }

            return new List<string>(procedures);
        }

        /// <summary>
        /// Extracts function calls from source code
        /// </summary>
        /// <param name="sourceCode">The PL/SQL source code</param>
        /// <returns>List of function names</returns>
        public List<string> ExtractFunctionCalls(string sourceCode)
        {
            var functions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sourceCode))
                return new List<string>();

            // Pattern for function calls in expressions
            var funcPattern = new Regex(
                @":=\s*([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)\s*\(|" +
                @"\(\s*([a-zA-Z_][\w$#]*(?:\.[a-zA-Z_][\w$#]*)?)\s*\(",
                RegexOptions.IgnoreCase);

            var matches = funcPattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var name = !string.IsNullOrEmpty(match.Groups[1].Value) 
                    ? match.Groups[1].Value 
                    : match.Groups[2].Value;
                
                if (!string.IsNullOrEmpty(name) && !IsSqlKeyword(name) && !IsBuiltInFunction(name))
                {
                    functions.Add(name);
                }
            }

            return new List<string>(functions);
        }

        /// <summary>
        /// Extracts package calls from source code
        /// </summary>
        /// <param name="sourceCode">The PL/SQL source code</param>
        /// <returns>List of package names</returns>
        public List<string> ExtractPackageCalls(string sourceCode)
        {
            var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sourceCode))
                return new List<string>();

            // Pattern for package.procedure/function calls - must be followed by ( for method call
            // Pattern: word.word( - captures package name and method name
            var packagePattern = new Regex(
                @"\b([a-zA-Z_][\w$#]*)\.([a-zA-Z_][\w$#]*)\s*\(",
                RegexOptions.IgnoreCase);

            var matches = packagePattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var packageName = match.Groups[1].Value;
                if (IsValidPackageName(packageName))
                {
                    packages.Add(packageName);
                }
            }

            return new List<string>(packages);
        }

        /// <summary>
        /// Checks if a name is a valid package name (not a built-in, keyword, or schema)
        /// </summary>
        private static bool IsValidPackageName(string name)
        {
            return !string.IsNullOrEmpty(name) && 
                   !IsBuiltInPackage(name) && 
                   !IsSqlKeyword(name) && 
                   !IsSchemaName(name);
        }

        private void ExtractMatches(string sourceCode, Regex pattern, HashSet<string> results)
        {
            var matches = pattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var name = match.Groups[1].Value;
                if (!IsSqlKeyword(name) && !string.IsNullOrEmpty(name))
                {
                    results.Add(name);
                }
            }
        }

        private static readonly HashSet<string> SqlKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE", "AND", "OR",
            "INTO", "VALUES", "SET", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER",
            "ON", "AS", "IN", "NOT", "NULL", "IS", "LIKE", "BETWEEN", "EXISTS",
            "HAVING", "GROUP", "BY", "ORDER", "ASC", "DESC", "LIMIT", "OFFSET",
            "UNION", "ALL", "DISTINCT", "TOP", "CASE", "WHEN", "THEN", "ELSE",
            "END", "IF", "BEGIN", "DECLARE", "CURSOR", "OPEN", "CLOSE", "FETCH",
            "DUAL", "TABLE", "VIEW", "INDEX", "CREATE", "DROP", "ALTER", "TRUNCATE",
            "MERGE", "USING", "MATCHED"
        };

        private static readonly HashSet<string> BuiltInFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "NVL", "NVL2", "DECODE", "COALESCE", "NULLIF", "CASE",
            "TO_CHAR", "TO_DATE", "TO_NUMBER", "TO_TIMESTAMP",
            "UPPER", "LOWER", "INITCAP", "TRIM", "LTRIM", "RTRIM",
            "SUBSTR", "INSTR", "LENGTH", "REPLACE", "TRANSLATE",
            "ROUND", "TRUNC", "ABS", "CEIL", "FLOOR", "MOD", "POWER",
            "SQRT", "SIGN", "SIN", "COS", "TAN", "LOG", "EXP",
            "SYSDATE", "SYSTIMESTAMP", "CURRENT_DATE", "CURRENT_TIMESTAMP",
            "ADD_MONTHS", "MONTHS_BETWEEN", "LAST_DAY", "NEXT_DAY",
            "EXTRACT", "COUNT", "SUM", "AVG", "MIN", "MAX",
            "FIRST", "LAST", "LEAD", "LAG", "ROW_NUMBER", "RANK",
            "DENSE_RANK", "ROWNUM", "ROWID", "SYS_GUID", "RAWTOHEX",
            "HEXTORAW", "CAST", "CONVERT", "LISTAGG", "XMLAGG"
        };

        private static readonly HashSet<string> BuiltInPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DBMS_OUTPUT", "DBMS_SQL", "DBMS_METADATA", "DBMS_LOB",
            "DBMS_UTILITY", "DBMS_RANDOM", "DBMS_SCHEDULER", "DBMS_JOB",
            "DBMS_LOCK", "DBMS_SESSION", "DBMS_FLASHBACK", "DBMS_CRYPTO",
            "UTL_FILE", "UTL_HTTP", "UTL_SMTP", "UTL_RAW", "UTL_ENCODE",
            "SYS", "STANDARD", "DUAL"
        };

        // Common Oracle schema names that should not be considered packages
        private static readonly HashSet<string> CommonSchemaNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HR", "SCOTT", "SH", "OE", "PM", "IX", "BI", "SYSTEM", "SYSAUX",
            "DBA", "ALL", "USER", "V$", "GV$", "PUBLIC", "APEX", "ORDS"
        };

        private static bool IsSqlKeyword(string word)
        {
            return SqlKeywords.Contains(word);
        }

        private static bool IsBuiltInFunction(string name)
        {
            return BuiltInFunctions.Contains(name);
        }

        private static bool IsBuiltInPackage(string name)
        {
            return BuiltInPackages.Contains(name);
        }

        private static bool IsSchemaName(string name)
        {
            return CommonSchemaNames.Contains(name);
        }
    }
}
