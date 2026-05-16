using FluentAssertions;
using ScientificActivityBusinessLogics.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Unit
{
    public class HelperTests
    {
        [Theory]
        [InlineData("ГУСЬКОВ ГЛЕБ ЮРЬЕВИЧ", "Гуськов Глеб Юрьевич")]
        [InlineData("ИВАНОВ ИВАН ИВАНОВИЧ", "Иванов Иван Иванович")]
        [InlineData("  ПЕТРОВА   АННА   СЕРГЕЕВНА  ", "Петрова Анна Сергеевна")]
        public void NormalizeFullName_WhenELibraryReturnsUpperCaseName_ShouldReturnReadableName(
            string source,
            string expected)
        {
            // Act
            var result = ELibraryTextHelper.NormalizeFullName(source);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("информационные системы (Ульяновск) SPIN-код: 2896-1945, AuthorID: 812005", "Информационные системы")]
        [InlineData("ПРИКЛАДНАЯ МАТЕМАТИКА (Москва) SPIN-код: 1111-2222, AuthorID: 123456", "Прикладная математика")]
        [InlineData("кафедра программной инженерии", "Кафедра программной инженерии")]
        public void NormalizeDepartment_WhenDepartmentContainsExtraELibraryData_ShouldRemoveSpinAndAuthorId(
            string source,
            string expected)
        {
            // Act
            var result = ELibraryTextHelper.NormalizeDepartment(source);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("2413-1709", "2413-1709")]
        [InlineData(" 2413-1709 ", "2413-1709")]
        [InlineData("2413 1709", "2413-1709")]
        [InlineData("ISSN: 2413-1709", "2413-1709")]
        public void NormalizeIssn_WhenIssnContainsSpacesOrExtraSymbols_ShouldReturnUnifiedFormat(
            string source,
            string expected)
        {
            // Act
            var result = ScientificIdentifierHelper.NormalizeIssn(source);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("1234-567Х", "1234-567X")]
        [InlineData("1234-567х", "1234-567X")]
        [InlineData("ISSN 1234-567Х", "1234-567X")]
        public void NormalizeIssn_WhenIssnContainsCyrillicX_ShouldReplaceItWithLatinX(
            string source,
            string expected)
        {
            // Act
            var result = ScientificIdentifierHelper.NormalizeIssn(source);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void BuildPublicationKey_WhenTitleAndYearAreSpecified_ShouldReturnNormalizedPublicationKey()
        {
            // Arrange
            var title = "  Метод   расширения словаря языковой модели  ";
            var year = 2026;

            // Act
            var result = DuplicateKeyHelper.BuildPublicationKey(title, year);

            // Assert
            result.Should().Be("МЕТОД РАСШИРЕНИЯ СЛОВАРЯ ЯЗЫКОВОЙ МОДЕЛИ|2026");
        }

        [Fact]
        public void BuildConferenceKey_WhenConferenceDataIsSpecified_ShouldReturnNormalizedConferenceKey()
        {
            // Arrange
            var title = "  Международная   конференция по информационным системам  ";
            var startDate = new DateTime(2026, 6, 10);
            var city = "  Ульяновск  ";

            // Act
            var result = DuplicateKeyHelper.BuildConferenceKey(title, startDate, city);

            // Assert
            result.Should().Be("МЕЖДУНАРОДНАЯ КОНФЕРЕНЦИЯ ПО ИНФОРМАЦИОННЫМ СИСТЕМАМ|2026-06-10|УЛЬЯНОВСК");
        }

        [Fact]
        public void ExtractTags_WhenStringContainsDifferentSeparators_ShouldReturnUniqueTags()
        {
            // Arrange
            var source = "Информационные системы; машинное обучение, Анализ данных\nИнформационные системы\rПрограммная инженерия";

            // Act
            var result = RecommendationTagHelper.ExtractTags(source);

            // Assert
            result.Should().BeEquivalentTo(new List<string>
            {
                "информационные системы",
                "машинное обучение",
                "анализ данных",
                "программная инженерия"
            });
        }

        [Theory]
        [InlineData("Информационные системы", "информационные системы")]
        [InlineData("  МАШИННОЕ   ОБУЧЕНИЕ  ", "машинное обучение")]
        [InlineData("Анализ    данных", "анализ данных")]
        public void NormalizeTag_WhenTagHasDifferentCaseOrExtraSpaces_ShouldReturnLowerCaseTag(
            string source,
            string expected)
        {
            // Act
            var result = RecommendationTagHelper.NormalizeTag(source);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ExtractTags_WhenStringContainsEmptyTags_ShouldRemoveEmptyValues()
        {
            // Arrange
            var source = "Информационные системы; ; ; машинное обучение,\n,\r,   ; Анализ данных";

            // Act
            var result = RecommendationTagHelper.ExtractTags(source);

            // Assert
            result.Should().HaveCount(3);
            result.Should().NotContain(string.Empty);
            result.Should().BeEquivalentTo(new List<string>
            {
                "информационные системы",
                "машинное обучение",
                "анализ данных"
            });
        }

        [Fact]
        public void HasIntersection_WhenTagSetsHaveCommonTag_ShouldReturnTrue()
        {
            // Arrange
            var researcherTags = new List<string>
            {
                "Информационные системы",
                "Машинное обучение"
            };

            var resourceTags = new List<string>
            {
                "Химия",
                "информационные системы",
                "Биология"
            };

            // Act
            var result = RecommendationTagHelper.HasIntersection(researcherTags, resourceTags);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasIntersection_WhenTagSetsDoNotHaveCommonTags_ShouldReturnFalse()
        {
            // Arrange
            var researcherTags = new List<string>
            {
                "Информационные системы",
                "Машинное обучение"
            };

            var resourceTags = new List<string>
            {
                "Химия",
                "Биология",
                "Медицина"
            };

            // Act
            var result = RecommendationTagHelper.HasIntersection(researcherTags, resourceTags);

            // Assert
            result.Should().BeFalse();
        }
    }
}
