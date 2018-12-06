using System.Configuration;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Describes the ASP.Net Classic Session state configuration.
    /// </summary>
    public class AspNetClassicStateConfigurationSection : ConfigurationSection
    {

        /// <summary>
        /// Gets the <see cref="AspNetClassicStateConfigurationSection"/>.
        /// </summary>
        /// <returns></returns>
        public static AspNetClassicStateConfigurationSection DefaultSection => ((AspNetClassicStateConfigurationSection)ConfigurationManager.GetSection("aspNetClassicSessionState"));

        /// <summary>
        /// Determines whether ASP Classic Session state to ASP.Net forwarding is enabled.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get => (bool)this["enabled"];
            set => this["enabled"] = value;
        }

    }

}
