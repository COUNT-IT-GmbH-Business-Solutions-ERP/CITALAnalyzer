using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace CountITBCALCop;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor Rule0000CheckProjectStructureAzureDevOps = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0000",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0000CheckProjectStructureAzureDevOpsTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0000CheckProjectStructureAzureDevOpsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0000CheckProjectStructureAzureDevOpsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0002CheckForMissingCaptions = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0002",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0002CheckForMissingCaptionsTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0002CheckForMissingCaptionsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0002CheckForMissingCaptionsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0003CheckIfDefaultDataClassification = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0003",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0003CheckIfDefaultDataClassificationTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0003CheckIfDefaultDataClassificationFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0003CheckIfDefaultDataClassificationDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0004NameOfEventSubscriberHasToMatchEvent = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0004",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0004NameOfEventSubscriberHasToMatchEventTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0004NameOfEventSubscriberHasToMatchEventFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0004NameOfEventSubscriberHasToMatchEventDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0005CheckForUnnecessaryParamsInEventSub = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0005",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0005CheckForUnnecessaryParamsInEventSubTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0005CheckForUnnecessaryParamsInEventSubFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0005CheckForUnnecessaryParamsInEventSubDescription"),
        helpLinkUri: "");
    
    public static readonly DiagnosticDescriptor Rule0006EmptyCaptionLocked = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0006",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0006EmptyCaptionLockedTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0006EmptyCaptionLockedFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0006EmptyCaptionLockedDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0007VariablesNameContainsALObjectAndNoSpecialChars = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0007",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0007VariablesNameContainsALObjectAndNoSpecialCharsTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0007VariablesNameContainsALObjectAndNoSpecialCharsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0007VariablesNameContainsALObjectAndNoSpecialCharsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0008EventSubsInCorrectCodeunit = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0008",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0008EventSubsInCorrectCodeunitTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0008EventSubsInCorrectCodeunitFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0008EventSubsInCorrectCodeunitDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0009CheckForAddMoveBeforeAfterinPageExt= new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0009",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0009CheckForAddMoveBeforeAfterinPageExtTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0009CheckForAddMoveBeforeAfterinPageExtFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0009CheckForAddMoveBeforeAfterinPageExtDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0010CallByReferenceOnlyIfVariableChangedInProcedure = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0010",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0010CallByReferenceOnlyIfVariableChangedInProcedureTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0010CallByReferenceOnlyIfVariableChangedInProcedureFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0010CallByReferenceOnlyIfVariableChangedInProcedureDescription"),
        helpLinkUri: "");
    // ##################################################################################################################################

    public static readonly DiagnosticDescriptor Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0011",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecordsTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecordsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecordsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0012UseSetAutoCalcFieldsInsteadOfCalcFields = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0012",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0012UseSetAutoCalcFieldsInsteadOfCalcFieldsTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0012UseSetAutoCalcFieldsInsteadOfCalcFieldsFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0012UseSetAutoCalcFieldsInsteadOfCalcFieldsDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0013",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0013UseCalcSumWhenCalculationSumOfFieldInFilterTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0013UseCalcSumWhenCalculationSumOfFieldInFilterFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0013UseCalcSumWhenCalculationSumOfFieldInFilterDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0014IfTextIsContinuouslyChangedUseTextBuilder= new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0014",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0014IfTextIsContinuouslyChangedUseTextBuilderTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0014IfTextIsContinuouslyChangedUseTextBuilderFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0014IfTextIsContinuouslyChangedUseTextBuilderDescription"),
        helpLinkUri: "");

    public static readonly DiagnosticDescriptor Rule0015DefineProcedureAsSourceExpressionInPageField = new(
        id: CountITBCALCopAnalyzers.AnalyzerPrefix + "0015",
        title: CountITBCALCopAnalyzers.GetLocalizableString("Rule0015DefineProcedureAsSourceExpressionInPageFieldTitle"),
        messageFormat: CountITBCALCopAnalyzers.GetLocalizableString("Rule0015DefineProcedureAsSourceExpressionInPageFieldFormat"),
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: CountITBCALCopAnalyzers.GetLocalizableString("Rule0015DefineProcedureAsSourceExpressionInPageFieldDescription"),
        helpLinkUri: "");

}