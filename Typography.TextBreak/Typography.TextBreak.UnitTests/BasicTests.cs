using System.Collections.Generic;
using Xunit;
using Typography.TextBreak;

public class BasicTests
{

    void BasicTest(string input, string[] output, bool breakNumberAfterText = false)
    {
        var outputList = new List<int> { 0 };
        var customBreaker = new CustomBreaker(vis => outputList.Add(vis.LatestBreakAt));

        customBreaker.BreakNumberAfterText = breakNumberAfterText;
        //
        customBreaker.BreakWords(input);


        //customBreaker.CopyBreakResults(outputList);
        for (int i = 0; i < outputList.Count - 1; i++)
        {
            Assert.Equal
            (
                output[i],
                input.Substring(outputList[i], outputList[i + 1] - outputList[i])
            );
        }
    }

    [Theory]
    [InlineData("Hi!", new[] { "Hi", "!" })]
    [InlineData("We are #1", new[] { "We", " ", "are", " ", "#", "1" })]
    [InlineData("1337 5P34K", new[] { "1337", " ", "5", "P34K" })]
    [InlineData("!@#$%^&*()", new[] { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")" })]
    [InlineData("1st line\r2nd line\n3rd line\r\n4th line\u00855th line", 
        new[] { "1", "st", " ", "line", "\r", "2", "nd", " ", "line", "\n",
                "3", "rd", " ", "line", "\r\n", "4", "th", " ", "line", "\u0085",
                "5", "th", " ", "line" })]
    [InlineData("6+23-456*78/9", new[] { "6", "+", "23", "-456", "*", "78", "/", "9" })]
    [InlineData("<>_____DisplayClass", new[] { "<", ">", "_", "_", "_", "_", "_", "DisplayClass" })]
    [InlineData("In\u000Bbetween\u000Care\u0020spaces", 
        new[] { "In", "\u000B", "between", "\u000C", "are", "\u0020", "spaces" })]
    public void Basic(string input, string[] output) => BasicTest(input, output);

    [Theory]
    [InlineData("\0\x1\x2\x3\x4\x5\x6\x7\x8\x9",
        new[] { "\0", "\x1", "\x2", "\x3", "\x4", "\x5", "\x6", "\x7", "\x8", "\x9" })]
    public void Control(string input, string[] output) => BasicTest(input, output);

    [Theory]
    [InlineData("\u0100", new[] { "\u0100" })]
    [InlineData("\u3DB4", new[] { "\u3DB4" })]
    [InlineData("\uFFFF", new[] { "\uFFFF" })]
    [InlineData("\r\n‸", new[] { "\r\n", "‸" })]
    [InlineData("\r\n‸\r\n", new[] { "\r\n", "‸", "\r\n" })]
    [InlineData("\r\n‸12a\r\n", new[] { "\r\n", "‸", "12", "a", "\r\n" })]
    public void OutOfRange(string input, string[] output) => BasicTest(input, output);

    [Theory]
    [InlineData("😀", new[] { "😀" })]
    [InlineData("😂", new[] { "😂" })]
    [InlineData("😂😂", new[] { "😂", "😂" })]
    [InlineData("😂A😂", new[] { "😂", "A", "😂" })]
    [InlineData("😂A123😂", new[] { "😂", "A123", "😂" })]
    public void Surrogates(string input, string[] output) => BasicTest(input, output);


    [Theory]
    [InlineData("A123", new[] { "A", "123" })]
    public void BreakNumAfterText(string input, string[] output)
    {
        BasicTest(input, output, true);
    }

    [Theory]
    [InlineData("a.m", new[] { "a.m" })]
    [InlineData("a.m.", new[] { "a.m." })]
    [InlineData(".a.m", new[] { ".", "a.m" })]
    [InlineData(".a.m.", new[] { ".", "a.m." })]
    [InlineData("9 a.m.", new[] { "9", " ", "a.m." })]
    public void DontBreakPerioidInTextSpan(string input, string[] output)
    {
        BasicTest(input, output, true);
    }
}
