using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Resources;

[assembly: AssemblyCompany("JH De Stip")]
[assembly: AssemblyProduct("Stipstonks")]
[assembly: AssemblyCopyright("Copyright © 2022 JH De Stip")]
[assembly: NeutralResourcesLanguage("nl")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
[assembly: ComVisible(false)]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
