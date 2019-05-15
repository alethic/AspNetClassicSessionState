using System.Configuration;

namespace AspNetClassicSessionState
{

    /// <summary>
    /// Describes the ASP.Net Classic Session state configuration.
    /// </summary>
    public class AspNetClassicSessionConfigurationSection : ConfigurationSection
    {

        /// <summary>
        /// Default if unavailable.
        /// </summary>
        static readonly AspNetClassicSessionConfigurationSection _defaultSection = new AspNetClassicSessionConfigurationSection()
        {
            Enabled = true,
            Prefix = "ASP::",
        };

        /// <summary>
        /// Gets the <see cref="AspNetClassicSessionConfigurationSection"/>.
        /// </summary>
        /// <returns></returns>
        public static AspNetClassicSessionConfigurationSection DefaultSection => (AspNetClassicSessionConfigurationSection)ConfigurationManager.GetSection("aspNetClassicSession") ?? _defaultSection;

        /// <summary>
        /// Determines whether ASP Classic Session state to ASP.Net forwarding is enabled.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get => (bool)this["enabled"];
            set => this["enabled"] = value;
        }

        /// <summary>
        /// Gets or sets the prefix of state variables stored in ASP.Net Session State.
        /// </summary>
        [ConfigurationProperty("prefix", DefaultValue = "ASP::")]
        public string Prefix
        {
            get => (string)this["prefix"];
            set => this["prefix"] = value;
        }

    }

}
