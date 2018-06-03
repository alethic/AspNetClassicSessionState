using System.Configuration;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Describes the ASP.Net Classic Session state configuration.
    /// </summary>
    public class AspNetClassicStateConfigurationSection : ConfigurationSection
    {

        [ConfigurationProperty("prefix", DefaultValue = "ASP_")]
        public string Prefix
        {
            get => (string)this["prefix"];
            set => this["prefix"] = value;
        }

    }

}
