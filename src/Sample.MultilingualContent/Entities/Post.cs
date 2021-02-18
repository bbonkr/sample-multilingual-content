using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Entities
{
    public class Post : EntityBase
    {
        public string TitleId { get; set; }

        public string ContentId { get; set; }

        public virtual LocalizationSet Title { get; set; }

        public virtual LocalizationSet Content { get; set; }
    }
}
