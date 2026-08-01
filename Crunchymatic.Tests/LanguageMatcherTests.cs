using AwesomeAssertions;
using Crunchymatic.Web.Services;

namespace Crunchymatic.Tests;

public static class LanguageMatcherTests
{
    [Theory]
    [InlineData("[CR] Some Show - S01E01 [00p].01.eng.English.ass", "en-US")]
    [InlineData("subtitle-589561-en-US-1784840353.ass", "en-US")]
    [InlineData("SomeShow_S1E1.0_SMSSIM0001_123456_subtitle_full_it-it.ass", "it-IT")]
    // has dashes in that are part of the language being matched, and also part of the file name
    [InlineData("[CR] Some Show - S01E01 [00p].02.spa-419.Spanish (Latin America).ass", "es-419")]
    [InlineData("[CR] Some Show - S01E01 [00p].03.spa-ES.Spanish (Spain).ass", "es-ES")]
    [InlineData("[CR] Some Show - S01E01 [00p].04.zh-HK.Chinese (Hong Kong).ass", "zh-HK")]
    [InlineData("subtitle-1-es-419-2.ass", "es-419")]
    [InlineData("subtitle-1-pt-BR-2.ass", "pt-BR")]
    [InlineData("subtitle-1-zh-CN-2.ass", "zh-CN")]
    // case shouldn't matter
    [InlineData("SOME SHOW.ENG.ass", "en-US")]
    [InlineData("subtitle-1-EN-us-2.ass", "en-US")]
    // shenanigans
    [InlineData("[CR] Ara Ara - S01E01 [1080p].05.ita.Italian.ass", "it-IT")]
    [InlineData("[CR] May I Ask For One Final Thing - S01E01 [1080p].05.ita.Italian.ass", "it-IT")]
    public static void LanguageMatcher_KnownShapes_Matches(string fileName, string expectedLanguageCode)
    {
        // act
        var matched = LanguageMatcher.TryMatch(fileName, out var languageCode);

        // assert
        matched.Should().BeTrue();
        languageCode.Should().Be(expectedLanguageCode);
    }

    [Theory]
    // episode IDs and whatnot have nothing to go on
    [InlineData("G0DUMX0XG.ass")]
    [InlineData("subtitle-123456-1234567890.ass")]
    // clbuttic type stuff - shouldnt trigger but just to be safe
    [InlineData("penguin.ass")]
    [InlineData("frfr.ass")]
    public static void LanguageMatcher_NothingToGoOn_DoesNotMatch(string fileName)
    {
        // act
        var matched = LanguageMatcher.TryMatch(fileName, out var languageCode);

        // assert
        matched.Should().BeFalse();
        languageCode.Should().BeNull();
    }
}
