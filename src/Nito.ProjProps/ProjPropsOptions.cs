using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;

#pragma warning disable CA1812
internal sealed class ProjPropsOptions
{
    public string? Name { get; set; }
    public string Project { get; set; } = null!;
    public OutputFormatEnum OutputFormat { get; set; }
    public List<(string Name, string Value)> Properties { get; set; } = new List<(string, string)>();
    public bool ExcludeEnvironment { get; set; } = true;
    public bool ExcludeGlobal { get; set; } = true;
    public bool ExcludeImported { get; set; }
    public bool ExcludeReserved { get; set; } = true;
    public bool ExcludePrimary { get; set; }
    public bool ProjectSearch { get; set; }
    public bool Debug { get; set; }
 
    public static RootCommand DefineOptions(RootCommand command)
    {
        command.AddOption(new Option<string>("--name", "Only include properties whose names match the specified regular expression."));
        command.AddOption(new Option<string>("--project", "Project file, or directory containing a single project file."));
        command.AddOption(new Option<bool>("--project-search", "Use the first project found in any subdirectory."));
        command.AddOption(new Option<OutputFormatEnum>("--output-format", () => OutputFormatEnum.SingleLinePercentEncode, "The formatting used to display project properties."));
        command.AddOption(new Option<List<(string, string)>>("--properties")
        {
            Description = "Adds an msbuild property: <name>=<value>",
            Argument = new Argument<List<(string, string)>>(
                argumentResult => argumentResult.Tokens.Select(token =>
                {
                    var arg = token.Value;
                    var index = arg.IndexOf('=', StringComparison.Ordinal);
                    var name = arg.Substring(0, index);
                    var value = arg.Substring(index + 1);
                    return (name, value);
                }).ToList())
                .WithArity(ArgumentArity.OneOrMore)
                .WithValidator(argumentResult => argumentResult.Tokens.Select(token =>
                {
                    var arg = token.Value;
                    var index = arg.IndexOf('=', StringComparison.Ordinal);
                    if (index == -1)
                        return $"Property setter {arg} does not contain '='.";
                    if (arg.AsSpan()[0..index].IsWhiteSpace())
                        return $"Invalid property name {arg.Substring(0, index)} for property setter {arg}.";
                    return null;
                }).FirstOrDefault(x => x != null)),
        });
        command.AddOption(new Option<bool>("--exclude-environment", () => true, "Exclude properties set from environment variables"));
        command.AddOption(new Option<bool>("--exclude-global", () => true, "Exclude global properties"));
        command.AddOption(new Option<bool>("--exclude-imported", "Exclude imported properties"));
        command.AddOption(new Option<bool>("--exclude-reserved", () => true, "Exclude reserved/builtin properties"));
        command.AddOption(new Option<bool>("--exclude-primary", "Exclude properties defined in the primary project file"));
        command.AddOption(new Option<bool>("--debug", "Enable debug logging"));
        return command;
    }

    public enum OutputFormatEnum
    {
        SingleLinePercentEncode,
        Json,
        SingleValueOnly,
    }
}
#pragma warning restore
