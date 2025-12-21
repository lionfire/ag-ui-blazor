namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Categorizes the risk level associated with a tool operation.
/// </summary>
public enum ToolRiskLevel
{
    /// <summary>
    /// The tool operation is considered safe with no side effects.
    /// Examples: reading data, querying information.
    /// </summary>
    Safe,

    /// <summary>
    /// The tool operation may have side effects that warrant user awareness.
    /// Examples: modifying files, sending emails.
    /// </summary>
    Risky,

    /// <summary>
    /// The tool operation could cause significant damage or is irreversible.
    /// Examples: deleting data, executing system commands, financial transactions.
    /// </summary>
    Dangerous
}
