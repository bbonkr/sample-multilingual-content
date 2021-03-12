using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Entities
{
    public class Language : EntityBase
    {
        /// <summary>
        /// Lanaguage code
        /// <para>
        /// Constraints
        /// <list type="bullet">
        /// <item>Index</item>
        /// <item>Unique</item>
        /// </list>
        /// </para>
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Language name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description for management.
        /// </summary>
        public string Description { get; set; }

        //public virtual IList<Localization> Localizations { get; set; }
    }
}
