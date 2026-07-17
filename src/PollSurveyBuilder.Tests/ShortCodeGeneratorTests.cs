using PollSurveyBuilder.Application.Common;
using Xunit;

namespace PollSurveyBuilder.Tests
{
    public class ShortCodeGeneratorTests
    {
        [Fact]
        public void Generate_ReturnsRequestedLength()
        {
            var code = ShortCodeGenerator.Generate(6);
            Assert.Equal(6, code.Length);
        }

        [Fact]
        public void Generate_ExcludesAmbiguousCharacters()
        {
            var ambiguous = new[] { '0', 'O', '1', 'l', 'I' };

            for (int i = 0; i < 200; i++)
            {
                var code = ShortCodeGenerator.Generate(8);
                Assert.DoesNotContain(code, c => ambiguous.Contains(c));
            }
        }

        [Fact]
        public void Generate_ProducesDifferentCodesAcrossCalls()
        {
            var codes = Enumerable.Range(0, 50).Select(_ => ShortCodeGenerator.Generate()).ToHashSet();
            // 50 random 6-char codes from a 55-char alphabet should essentially never collide.
            Assert.True(codes.Count > 45);
        }
    }
}
