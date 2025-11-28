using System;
using System.Collections.Generic;
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

            ExtractMatches(sourceCode, fromPattern, tables);
            ExtractMatches(sourceCode, insertPattern, tables);
            ExtractMatches(sourceCode, updatePattern, tables);
            ExtractMatches(sourceCode, deletePattern, tables);
            ExtractMatches(sourceCode, joinPattern, tables);

            return new List<string>(tables);
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

            // Pattern for package.procedure/function calls
            var packagePattern = new Regex(
                @"\b([a-zA-Z_][\w$#]*)\.([a-zA-Z_][\w$#]*)\s*(?:\(|;)",
                RegexOptions.IgnoreCase);

            var matches = packagePattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var packageName = match.Groups[1].Value;
                if (!IsBuiltInPackage(packageName) && !IsSqlKeyword(packageName))
                {
                    packages.Add(packageName);
                }
            }

            return new List<string>(packages);
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
            "DUAL", "TABLE", "VIEW", "INDEX", "CREATE", "DROP", "ALTER", "TRUNCATE"
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
    }
}
