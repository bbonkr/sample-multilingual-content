using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Models
{
    public record PostModel(string Id, string Title, string Content, string LanguageCode);

    public record PostDetailModel(string Id, IEnumerable<PostModel> Contetents);

    public record PostSaveResultModel(string Id, IEnumerable<PostModel> Contents);

    public record PostSaveRequestModel(string Id, string CriteriaLanguageCode, IEnumerable<PostModel> PostContents, bool UseTranslation);
}
