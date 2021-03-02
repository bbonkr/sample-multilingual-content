using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Models
{
    public record PostModel(string Id, string Title, string Content);

    public record PostDetailModel(string Id, IEnumerable< PostSaveModel> Contetents);

    public record PostSaveModel(string Title, string Content, string LanguageCode);

    public record PostSaveResultModel(string Id, IEnumerable<PostSaveModel> Contents);

    public record PostSaveRequestModel(string Id, string CriteriaLanguageCode, IEnumerable<PostSaveModel> PostContents, bool UseTranslation);
    
}
