using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Analyzes Oracle packages to extract procedures, functions, variables, and types
    /// </summary>
    public class OraclePackageAnalyzer
    {
        /// <summary>
        /// Extracts stored procedures from a package
        /// </summary>
        /// <param name="package">The database package</param>
        /// <returns>List of extracted procedures</returns>
        public List<DatabaseStoredProcedure> ExtractProcedures(DatabasePackage package)
        {
            var procedures = new List<DatabaseStoredProcedure>();
            if (package == null || string.IsNullOrEmpty(package.Definition))
                return procedures;

            var sourceCode = package.Definition;
            
            // Pattern to match PROCEDURE declarations
            var procedurePattern = new Regex(
                @"\bPROCEDURE\s+(\w+)\s*(?:\(([^)]*)\))?\s*(?:IS|AS)?",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var matches = procedurePattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var procedureName = match.Groups[1].Value;
                var parameters = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

                var proc = new DatabaseStoredProcedure
                {
                    SchemaOwner = package.SchemaOwner,
                    Name = procedureName,
                    Package = package.Name
                };

                // Parse parameters if present
                if (!string.IsNullOrEmpty(parameters))
                {
                    ExtractArguments(proc, parameters);
                }

                procedures.Add(proc);
            }

            return procedures;
        }

        /// <summary>
        /// Extracts functions from a package
        /// </summary>
        /// <param name="package">The database package</param>
        /// <returns>List of extracted functions</returns>
        public List<DatabaseFunction> ExtractFunctions(DatabasePackage package)
        {
            var functions = new List<DatabaseFunction>();
            if (package == null || string.IsNullOrEmpty(package.Definition))
                return functions;

            var sourceCode = package.Definition;
            
            // Pattern to match FUNCTION declarations
            var functionPattern = new Regex(
                @"\bFUNCTION\s+(\w+)\s*(?:\(([^)]*)\))?\s*RETURN\s+(\w+)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var matches = functionPattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var functionName = match.Groups[1].Value;
                var returnType = match.Groups[3].Value;

                var func = new DatabaseFunction
                {
                    SchemaOwner = package.SchemaOwner,
                    Name = functionName,
                    ReturnType = returnType
                };

                functions.Add(func);
            }

            return functions;
        }

        /// <summary>
        /// Extracts package-level variables from a package
        /// </summary>
        /// <param name="package">The database package</param>
        /// <returns>List of variable names</returns>
        public List<string> ExtractVariables(DatabasePackage package)
        {
            var variables = new List<string>();
            if (package == null || string.IsNullOrEmpty(package.Definition))
                return variables;

            var sourceCode = package.Definition;
            
            // Pattern to match variable declarations (not inside procedures/functions)
            var variablePattern = new Regex(
                @"^\s*(\w+)\s+(\w+(?:\s*\([^)]+\))?)\s*(?::=|;)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var matches = variablePattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                var varName = match.Groups[1].Value;
                
                // Skip keywords
                if (!IsKeyword(varName))
                {
                    variables.Add(varName);
                }
            }

            return variables;
        }

        /// <summary>
        /// Extracts type definitions from a package
        /// </summary>
        /// <param name="package">The database package</param>
        /// <returns>List of type names</returns>
        public List<string> ExtractTypes(DatabasePackage package)
        {
            var types = new List<string>();
            if (package == null || string.IsNullOrEmpty(package.Definition))
                return types;

            var sourceCode = package.Definition;
            
            // Pattern to match TYPE declarations
            var typePattern = new Regex(
                @"\bTYPE\s+(\w+)\s+IS\s+",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var matches = typePattern.Matches(sourceCode);
            foreach (Match match in matches)
            {
                types.Add(match.Groups[1].Value);
            }

            return types;
        }

        private void ExtractArguments(DatabaseStoredProcedure proc, string parameterList)
        {
            // Split by comma (simple approach)
            var paramParts = parameterList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var ordinal = 0;

            foreach (var paramPart in paramParts)
            {
                var trimmed = paramPart.Trim();
                
                // Pattern: param_name [IN|OUT|IN OUT] data_type
                var paramPattern = new Regex(
                    @"(\w+)\s+(IN\s+OUT|IN|OUT)?\s*(\w+(?:\s*\([^)]+\))?)",
                    RegexOptions.IgnoreCase);
                
                var match = paramPattern.Match(trimmed);
                if (match.Success)
                {
                    var arg = new DatabaseArgument
                    {
                        Name = match.Groups[1].Value,
                        DatabaseDataType = match.Groups[3].Value,
                        Ordinal = ordinal++
                    };

                    var direction = match.Groups[2].Value.ToUpperInvariant();
                    arg.In = string.IsNullOrEmpty(direction) || direction.Contains("IN");
                    arg.Out = direction.Contains("OUT");

                    proc.Arguments.Add(arg);
                }
            }
        }

        private static bool IsKeyword(string word)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PROCEDURE", "FUNCTION", "TYPE", "BEGIN", "END", "IF", "THEN",
                "ELSE", "ELSIF", "LOOP", "WHILE", "FOR", "RETURN", "IS", "AS",
                "DECLARE", "EXCEPTION", "WHEN", "OTHERS", "CURSOR", "OPEN",
                "CLOSE", "FETCH", "INTO", "SELECT", "FROM", "WHERE", "AND",
                "OR", "NOT", "NULL", "TRUE", "FALSE"
            };
            return keywords.Contains(word);
        }
    }
}
