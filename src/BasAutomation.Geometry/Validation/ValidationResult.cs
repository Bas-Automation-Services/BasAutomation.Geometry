namespace BasAutomation.Geometry.Validation;

/// <summary>
/// Represents the result of validating a cross-section for extrusion.
/// </summary>
public class ValidationResult
{
    private readonly List<ValidationIssue> issues;

    /// <summary>
    /// Gets all validation issues found.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues => issues.AsReadOnly();

    /// <summary>
    /// Gets whether the cross-section is valid (no errors, warnings are allowed).
    /// </summary>
    public bool IsValid => !HasErrors;

    /// <summary>
    /// Gets whether any errors were found.
    /// </summary>
    public bool HasErrors => issues.Any(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Gets whether any warnings were found.
    /// </summary>
    public bool HasWarnings => issues.Any(i => i.Severity == ValidationSeverity.Warning);

    /// <summary>
    /// Gets all error issues.
    /// </summary>
    public IEnumerable<ValidationIssue> Errors => issues.Where(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Gets all warning issues.
    /// </summary>
    public IEnumerable<ValidationIssue> Warnings => issues.Where(i => i.Severity == ValidationSeverity.Warning);

    /// <summary>
    /// Gets the total count of issues.
    /// </summary>
    public int IssueCount => issues.Count;

    /// <summary>
    /// Gets the count of errors.
    /// </summary>
    public int ErrorCount => Errors.Count();

    /// <summary>
    /// Gets the count of warnings.
    /// </summary>
    public int WarningCount => Warnings.Count();

    /// <summary>
    /// Initializes a new instance of the ValidationResult class.
    /// </summary>
    public ValidationResult()
    {
        issues = new List<ValidationIssue>();
    }

    /// <summary>
    /// Adds a validation issue to the result.
    /// </summary>
    internal void AddIssue(ValidationIssue issue)
    {
        issues.Add(issue);
    }

    /// <summary>
    /// Gets a summary string of the validation result.
    /// </summary>
    public string GetSummary()
    {
        if (IsValid && !HasWarnings)
        {
            return "Cross-section is valid with no issues.";
        }

        var parts = new List<string>();
        if (ErrorCount > 0)
        {
            parts.Add($"{ErrorCount} error(s)");
        }
        if (WarningCount > 0)
        {
            parts.Add($"{WarningCount} warning(s)");
        }

        return $"Cross-section validation: {string.Join(", ", parts)}";
    }

    /// <summary>
    /// Gets a detailed report of all issues.
    /// </summary>
    public string GetDetailedReport()
    {
        if (IsValid && !HasWarnings)
        {
            return "Cross-section is valid with no issues.";
        }

        var report = new System.Text.StringBuilder();
        report.AppendLine(GetSummary());
        report.AppendLine();

        if (ErrorCount > 0)
        {
            report.AppendLine("ERRORS:");
            foreach (var error in Errors)
            {
                report.AppendLine($"  {error}");
            }
            report.AppendLine();
        }

        if (WarningCount > 0)
        {
            report.AppendLine("WARNINGS:");
            foreach (var warning in Warnings)
            {
                report.AppendLine($"  {warning}");
            }
        }

        return report.ToString().TrimEnd();
    }

    public override string ToString() => GetSummary();
}
