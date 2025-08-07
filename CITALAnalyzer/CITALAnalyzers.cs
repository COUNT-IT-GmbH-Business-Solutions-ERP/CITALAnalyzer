using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Text.Json;



namespace CITALAnalyzer
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class CITALAnalyzerAnalyzers
    {
        private static ResourceManager? resourceMan;
        private static CultureInfo? resourceCulture;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (CITALAnalyzerAnalyzers.resourceMan is null)
                    CITALAnalyzerAnalyzers.resourceMan = new ResourceManager("CITALAnalyzer.CITALAnalyzerAnalyzers", typeof(CITALAnalyzerAnalyzers).Assembly);
                return CITALAnalyzerAnalyzers.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => CITALAnalyzerAnalyzers.resourceCulture ?? CultureInfo.CurrentUICulture;
            set => CITALAnalyzerAnalyzers.resourceCulture = value;
        }

        internal static string AnalyzerPrefix => GetFromResourceManager();

        internal static LocalizableString GetLocalizableString(string nameOfLocalizableResource)
        {
            return new LocalizableResourceString(
                nameOfLocalizableResource,
                CITALAnalyzerAnalyzers.ResourceManager,
                typeof(CITALAnalyzerAnalyzers)
                );
        }

        private static string GetFromResourceManager(
            [CallerMemberName] string? resourceName = null)
        {
            if (resourceName is null)
                throw new ArgumentNullException(nameof(resourceName));

            string? value = CITALAnalyzerAnalyzers.ResourceManager
                .GetString(resourceName, CITALAnalyzerAnalyzers.resourceCulture);

            if (value is null)
                throw new InvalidOperationException(
                    $"Embedded resource '{resourceName}' not found.");

            return value;
        }
    }
}