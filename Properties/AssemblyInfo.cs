using MelonLoader;
using System.Reflection;
using System.Runtime.InteropServices;
using DealersSendTexts;

[assembly: MelonInfo(typeof(DealerText), DealerText.ModName, DealerText.Version, "GuysWeForgotDre")]
[assembly: MelonGame("TVGS", "Schedule I")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(DealerText.ModName)]
[assembly: AssemblyDescription(DealerText.ModDesc)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(DealerText.ModName)]
[assembly: AssemblyCopyright("Copyright ©  2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("51af033c-2fba-4fd9-950e-a5fb45e211c4")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion(DealerText.Version + ".0")]
[assembly: AssemblyFileVersion(DealerText.Version + ".0")]