using NetTools.Serialization.Attributes;
using NetTools.Serialization.JsonSchemaAttributes;

namespace Modules.Google.SearchConsole.Events.Classes
{
    /// <summary>
    /// Contains the fields in a row result from Google Search Console API.
    /// </summary>
    [JsonSchemaTitle("Row")]
    [JsonSchemaDescription("Contains the fields in a row result from Google Search Console API.")]
    public class Row
    {
        /// <summary>
        /// The ranking position of the search result in the SERP
        /// </summary>
        [JsonSchemaTitle("Position")]
        [JsonSchemaDescription("The ranking position of the search result in the SERP.")]
        public double Position { get; set; }

        /// <summary>
        /// The total number of clicks on this search result.
        /// </summary>
        [JsonSchemaTitle("Clicks")]
        [JsonSchemaDescription("The total number of clicks on this search result.")]
        public double Clicks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonSchemaTitle("Impressions")]
        [JsonSchemaDescription(".")]
        public double Impressions { get; set; }

        /// <summary>
        /// The "Click Through Rate" (or ctr) shows a pre-calculated method for  how often the result has been clicked.
        /// </summary>
        [JsonSchemaTitle("Click Through Rate")]
        [JsonSchemaDescription("The \"Click Through Rate\" (or ctr) shows a pre-calculated method for how often the result has been clicked.")]
        public double Ctr { get; set; }

        /// <summary>
        /// The keywords (or search terms) used to make the search where the result appeared.
        /// </summary>
        [JsonSchemaTitle("Keyword")]
        [JsonSchemaDescription("The keywords (or search terms) used to make the search where the result appeared.")]
        [JsonPath("keys[0]")]
        public string Keyword { get; set; }

        /// <summary>
        /// The URL that appeared in the search results.
        /// </summary>
        [JsonSchemaTitle("Url")]
        [JsonSchemaDescription("The URL that appeared in the search results.")]
        [JsonPath("keys[1]")]
        public string Url { get; set; }

        /// <summary>
        /// The country where the search was performed.
        /// </summary>
        [JsonSchemaTitle("Country")]
        [JsonSchemaDescription("The country where the search was performed.")]
        [JsonPath("keys[2]")]
        public string Country { get; set; }

        /// <summary>
        /// The device used to search. This may be a mobile, desktop, or other device type.
        /// </summary>
        [JsonSchemaTitle("Device")]
        [JsonSchemaDescription("The device used to search. This may be a mobile, desktop, or other device type.")]
        [JsonPath("keys[3]")]
        public string Device { get; set; }
    } 
}
