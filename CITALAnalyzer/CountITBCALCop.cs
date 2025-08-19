using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Text.Json;



namespace CountITBCALCop
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class CountITBCALCopAnalyzers
    {
        private static ResourceManager? resourceMan;
        private static CultureInfo? resourceCulture;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (CountITBCALCopAnalyzers.resourceMan is null)
                    CountITBCALCopAnalyzers.resourceMan = new ResourceManager("CountITBCALCop.CountITBCALCopAnalyzers", typeof(CountITBCALCopAnalyzers).Assembly);
                return CountITBCALCopAnalyzers.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => CountITBCALCopAnalyzers.resourceCulture ?? CultureInfo.CurrentUICulture;
            set => CountITBCALCopAnalyzers.resourceCulture = value;
        }

        internal static string AnalyzerPrefix => GetFromResourceManager();

        internal static LocalizableString GetLocalizableString(string nameOfLocalizableResource)
        {
            return new LocalizableResourceString(
                nameOfLocalizableResource,
                CountITBCALCopAnalyzers.ResourceManager,
                typeof(CountITBCALCopAnalyzers)
                );
        }

        private static string GetFromResourceManager(
            [CallerMemberName] string? resourceName = null)
        {
            if (resourceName is null)
                throw new ArgumentNullException(nameof(resourceName));

            string? value = CountITBCALCopAnalyzers.ResourceManager
                .GetString(resourceName, CountITBCALCopAnalyzers.resourceCulture);

            if (value is null)
                throw new InvalidOperationException(
                    $"Embedded resource '{resourceName}' not found.");

            return value;
        }
    }
}