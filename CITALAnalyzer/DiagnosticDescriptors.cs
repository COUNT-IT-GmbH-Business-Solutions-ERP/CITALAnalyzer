using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace CITALAnalyzer;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor Rule0001CheckForPrefixInVariableName = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0001",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0001CheckForPrefixInVariableNameTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0001CheckForPrefixInVariableNameFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0001CheckForPrefixInVariableNameDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0002CheckForMissingCaptions = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0002",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0002CheckForMissingCaptionsTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0002CheckForMissingCaptionsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0002CheckForMissingCaptionsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0003CheckIfDefaultDataClassification = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0003",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0003CheckIfDefaultDataClassificationTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0003CheckIfDefaultDataClassificationFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0003CheckIfDefaultDataClassificationDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0004NameOfEventSubscriberHasToMatchEvent = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0004",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0004NameOfEventSubscriberHasToMatchEventTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0004NameOfEventSubscriberHasToMatchEventFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0004NameOfEventSubscriberHasToMatchEventDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0005CheckForUnnecessaryParamsInEventSub = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0005",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0005CheckForUnnecessaryParamsInEventSubTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0005CheckForUnnecessaryParamsInEventSubFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0005CheckForUnnecessaryParamsInEventSubDescription"),
        helpLinkUri: "");
    
    public static readonly DiagnosticDescriptor Rule0006EmptyCaptionLocked = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0006",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0006EmptyCaptionLockedTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0006EmptyCaptionLockedFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0006EmptyCaptionLockedDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0007VariablesNameContainsALObjectAndNoSpecialChars = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0007",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0007VariablesNameContainsALObjectAndNoSpecialCharsTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0007VariablesNameContainsALObjectAndNoSpecialCharsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0007VariablesNameContainsALObjectAndNoSpecialCharsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0008EventSubsInCorrectCodeunit = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0008",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0008EventSubsInCorrectCodeunitTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0008EventSubsInCorrectCodeunitFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0008EventSubsInCorrectCodeunitDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0009CheckForAddMoveBeforeAfterinPageExt= new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0009",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0009CheckForAddMoveBeforeAfterinPageExtTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0009CheckForAddMoveBeforeAfterinPageExtFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0009CheckForAddMoveBeforeAfterinPageExtDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0010CallByReferenceOnlyIfVariableChangedInProcedure = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0010",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0010CallByReferenceOnlyIfVariableChangedInProcedureTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0010CallByReferenceOnlyIfVariableChangedInProcedureFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0010CallByReferenceOnlyIfVariableChangedInProcedureDescription"),
        helpLinkUri: "");
    // ##################################################################################################################################

    public static readonly DiagnosticDescriptor Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0011",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecordsTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecordsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecordsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0012UseSetAutoCalcFieldsInsteadOfCalcFields = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0012",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0012UseSetAutoCalcFieldsInsteadOfCalcFieldsTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0012UseSetAutoCalcFieldsInsteadOfCalcFieldsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0012UseSetAutoCalcFieldsInsteadOfCalcFieldsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0013",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0013UseCalcSumWhenCalculationSumOfFieldInFilterTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0013UseCalcSumWhenCalculationSumOfFieldInFilterFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0013UseCalcSumWhenCalculationSumOfFieldInFilterDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0014IfTextIsContinuouslyChangedUseTextBuilder= new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0014",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0014IfTextIsContinuouslyChangedUseTextBuilderTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0014IfTextIsContinuouslyChangedUseTextBuilderFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0014IfTextIsContinuouslyChangedUseTextBuilderDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0015DefineProcedureAsSourceExpressionInPageField = new(
        id: CITALAnalyzerAnalyzers.AnalyzerPrefix + "0015",
        title: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0015DefineProcedureAsSourceExpressionInPageFieldTitle"),
        messageFormat: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0015DefineProcedureAsSourceExpressionInPageFieldFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CITALAnalyzerAnalyzers.GetLocalizableString("Rule0015DefineProcedureAsSourceExpressionInPageFieldDescription"),
        helpLinkUri: "");

}