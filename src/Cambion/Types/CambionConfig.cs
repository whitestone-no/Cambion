namespace Whitestone.Cambion.Types
{
    internal class CambionConfig
    {
        public CambionConfigTransport Transport { get; set; }
        public CambionConfigSerializer Serializer { get; set; }
    }

    internal class CambionConfigTransport
    {
        internal const string Key = "Transport";
        internal const string ConfigurationKey = "Configuration";

        public string Name { get; set; }
    }

    internal class CambionConfigSerializer
    {
        internal const string Key = "Serializer";

        public string Name { get; set; }
    }
}
