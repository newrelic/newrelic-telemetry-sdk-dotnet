using System;

namespace OpenTelemetry.Exporter.NewRelic
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PackageVersionAttribute : Attribute
    {
        public string PackageVersion { get; }
        public PackageVersionAttribute(string version) 
        {
            PackageVersion = version;
        }
    }
}
