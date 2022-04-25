using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("HLA.WatchDog")]
[assembly: AssemblyDescription("A Watch Dog for HLAirships")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(HLAirships.LegalMamboJambo.Company)]
[assembly: AssemblyProduct(HLAirships.LegalMamboJambo.Product)]
[assembly: AssemblyCopyright(HLAirships.LegalMamboJambo.Copyright)]
[assembly: AssemblyTrademark(HLAirships.LegalMamboJambo.Trademark)]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
[assembly: AssemblyVersion(HLAirshipsCore.Version.Number)]
[assembly: AssemblyFileVersion(HLAirshipsCore.Version.Number)]

[assembly: KSPAssembly("HLA.WatchDog", HLAirshipsCore.Version.major, HLAirshipsCore.Version.minor)]﻿
[assembly: KSPAssemblyDependency("KSPe", 2, 4)]﻿
[assembly: KSPAssemblyDependency("KSPe.UI", 2, 4)]﻿
