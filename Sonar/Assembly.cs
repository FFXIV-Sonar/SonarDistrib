using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007", Justification = "Handled by Fody")]
[assembly: SuppressMessage("Globalization", "CA1303", Justification = "<Pending>")]
[assembly: SuppressMessage("Design", "CA1040", Justification = "MessagePack and identification")]
[assembly: SuppressMessage("Design", "CA1028", Justification = "")]
[assembly: SuppressMessage("Major Code Smell", "S3358", Justification = "<Pending>")]
[assembly: SuppressMessage("Major Code Smell", "S1117", Justification = "this. is used for instance variables")]
[assembly: SuppressMessage("Major Code Smell", "S3264", Justification = "Custom invokers")]
[assembly: InternalsVisibleTo("SonarServer")]
[assembly: InternalsVisibleTo("SonarResourceGen")]
[assembly: InternalsVisibleTo("SonarTesting")]
[assembly: InternalsVisibleTo("SonarTests")]
[assembly: InternalsVisibleTo("SonarBenchmarks")]
