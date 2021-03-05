using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Models
{
    /// <summary>
    /// The string to translate.
    /// </summary>
    public record TranslationRequestInputModel(string Text);

    public class TranslationRequestModel
    {
        /// <summary>
        /// Required
        /// <para>
        /// Each array element is the string to translate.
        /// </para>
        /// <see cref="TranslationRequestInputModel"/>
        /// </summary>
        public IEnumerable<TranslationRequestInputModel> Inputs { get; init; }

        /// <summary>
        /// Required
        /// <para>
        /// Specifies the language of the output text. The target language must be one of the supported languages included in the translation scope. For example, use to=de to translate to German. It's possible to translate to multiple languages simultaneously by repeating the parameter in the query string. For example, use to=de&to=it to translate to German and Italian.
        /// </para>
        /// </summary>
        public IEnumerable<string> ToLanguages { get; init; }

        /// <summary>
        /// Optional 
        /// <para>
        /// Specifies the language of the input text. Find which languages are available to translate from by looking up supported languages using the translation scope. If the from parameter is not specified, automatic language detection is applied to determine the source language.
        /// </para>
        /// </summary>
        public string FromLanguage { get; init; }

        /// <summary>
        /// Optional
        /// <para>
        /// Defines whether the text being translated is plain text or HTML text. Any HTML needs to be a well-formed, complete element. Possible values are: plain (default) or html.
        /// </para>
        /// <see cref="TextTypes" />
        /// </summary>
        public string TextType { get; init; } = TextTypes.Plain;

        /// <summary>
        /// Optional
        /// <para>
        /// A string specifying the category (domain) of the translation. This parameter is used to get translations from a customized system built with Custom Translator. Add the Category ID from your Custom Translator project details to this parameter to use your deployed customized system. Default value is: general.
        /// </para>
        /// <see cref="Categories"/>
        /// </summary>
        public string Category { get; init; } = Categories.General;

        /// <summary>
        /// Optional
        /// <para>
        /// Specifies how profanities should be treated in translations. Possible values are: NoAction (default), Marked or Deleted. To understand ways to treat profanity,
        /// </para>
        /// <see cref="ProfanityActions"/>
        /// </summary>
        public string ProfanityAction { get; init; } = ProfanityActions.NoAction;

        /// <summary>
        /// Optional
        /// <para>
        /// Specifies how profanities should be marked in translations. Possible values are: Asterisk (default) or Tag. To understand ways to treat profanity
        /// </para>
        /// <see cref="ProfanityMarkers"/>
        /// </summary>
        public string ProfanityMarker { get; init; } = ProfanityMarkers.Asterisk;
    }

    public class TranslationResultModel
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }

        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }

        public TextResult Transliteration { get; set; }

        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }

    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }

        public int[] TransSentLen { get; set; }
    }

    public class TextTypes
    {
        public const string Plain = "plain";
        public const string Html = "html";
    }

    public class Categories
    {
        public const string General = "general";
    }

    public class ProfanityActions
    {
        public const string NoAction = "NoAction";
        public const string Deleted = "Deleted";
        public const string Marked = "Marked";
    }

    public class ProfanityMarkers
    {
        public const string Asterisk = "Asterisk";
        public const string Tag = "Tag";
    }

    public class TranslationErrorResultModel
    {
        public ErrorModel Error { get; init; }
    }

    public class ErrorModel
    {
        public int Code { get; init; }

        public string Message { get; init; }
    }
}
