using System.Collections.Generic;
using Xunit;
using Typography.TextBreak;
using static Typography.TextBreak.WordKind;

public class WordKindTests
{
    static object[] Row(string input, params (string section, WordKind wordKind)[] output) =>
        new object[] { input, output };
    public static IEnumerable<object[]> WordKindData =>
        new[] {
            Row("Hi!", ("Hi", Text), ("!", Punc)),
            Row("We are #1",
                ("We", Text), (" ", Whitespace), ("are", Text), (" ", Whitespace), ("#", Punc), ("1", Number)
            ),
            Row("1337 5P34K",
                ("1337", Number), (" ", Whitespace), ("5", Number), ("P34K", Text)
            ),
            Row("In\u000Bbetween\u000Care\u0020spaces",
                ("In", Text), ("\u000B", Whitespace), ("between", Text),
                ("\u000C", Whitespace), ("are", Text), ("\u0020", Whitespace), ("spaces", Text)
            ),
            Row("!@#$%^&*()", 
                ("!", Punc), ("@", Punc), ("#", Punc), ("$", Punc), ("%", Punc),
                ("^", Punc), ("&", Punc), ("*", Punc), ("(", Punc), (")", Punc)
            ),
            Row("1st line\r2nd line\n3rd line\r\n4th line\u00855th line",
                ("1", Number), ("st", Text), (" ", Whitespace), ("line", Text),
                ("\r", NewLine), ("2", Number), ("nd", Text), (" ", Whitespace),
                ("line", Text), ("\n", NewLine),
                ("3", Number), ("rd", Text), (" ", Whitespace), ("line", Text),
                ("\r\n", NewLine), ("4", Number), ("th", Text), (" ", Whitespace),
                ("line", Text), ("\u0085", NewLine),
                ("5", Number), ("th", Text), (" ", Whitespace), ("line", Text)
            )
        };
    [Theory]
    [MemberData(nameof(WordKindData))]
    public void WordKindTest(string input, (string section, WordKind wordKind)[] output)
    {
        var outputList = new List<BreakAtInfo> { new BreakAtInfo(0, Unknown) };
        var customBreaker = new CustomBreaker(vis => outputList.Add(new BreakAtInfo(vis.LatestBreakAt, vis.LatestWordKind)));

        customBreaker.BreakWords(input);

        for (int i = 0; i < outputList.Count - 1; i++)
        {
            Assert.Equal
            (
                output[i].section,
                input.Substring(outputList[i].breakAt,
                                outputList[i + 1].breakAt - outputList[i].breakAt)
            );
            Assert.Equal(output[i].wordKind, outputList[i + 1].wordKind);
        }
    }
}