using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Typography.TextBreak;

[TestClass]
public class BasicTests
{

    public void BasicTest(string input, string[] output, bool breakNumberAfterText = false)
    {
        var outputList = new List<int> { 0 };
        var customBreaker = new CustomBreaker();
        customBreaker.SetNewBreakHandler(vis => outputList.Add(vis.LatestBreakAt));

        customBreaker.BreakNumberAfterText = breakNumberAfterText;
        //
        customBreaker.BreakWords(input);


        //customBreaker.CopyBreakResults(outputList);
        for (int i = 0; i < outputList.Count - 1; i++)
        {
            Assert.AreEqual
            (
                output[i],
                input.Substring(outputList[i], outputList[i + 1] - outputList[i])
            );
        }
    }

    [DataTestMethod]
    [DataRow("Hi!", 0, new[] { "Hi", "!" })]
    [DataRow("We are #1", 0, new[] { "We", " ", "are", " ", "#", "1" })]
    [DataRow("1337 5P34K", 0, new[] { "1337", " ", "5", "P34K" })]
    [DataRow("!@#$%^&*()", 0, new[] { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")" })]
    [DataRow("1st line\r2nd line\n3rd line\r\n4th line\u00855th line", 0,
        new[] { "1", "st", " ", "line", "\r", "2", "nd", " ", "line", "\n",
                "3", "rd", " ", "line", "\r\n", "4", "th", " ", "line", "\u0085",
                "5", "th", " ", "line" })]
    [DataRow("6+23-456*78/9", 0, new[] { "6", "+", "23", "-456", "*", "78", "/", "9" })]
    [DataRow("<>_____DisplayClass", 0, new[] { "<", ">", "_", "_", "_", "_", "_", "DisplayClass" })]
    [DataRow("In\u000Bbetween\u000Care\u0020spaces", 0,
        new[] { "In", "\u000B", "between", "\u000C", "are", "\u0020", "spaces" })]
    public void Basic(string input, int _, string[] output) => BasicTest(input, output);

    [DataTestMethod]
    [DataRow("\0\x1\x2\x3\x4\x5\x6\x7\x8\x9", 0,
        new[] { "\0", "\x1", "\x2", "\x3", "\x4", "\x5", "\x6", "\x7", "\x8", "\x9" })]
    public void Control(string input, int _, string[] output) => BasicTest(input, output);

    [DataTestMethod]
    [DataRow("\u0100", 0, new[] { "\u0100" })]
    [DataRow("\u3DB4", 0, new[] { "\u3DB4" })]
    [DataRow("\uFFFF", 0, new[] { "\uFFFF" })]
    [DataRow("\r\n‸", 0, new[] { "\r\n", "‸" })]
    [DataRow("\r\n‸\r\n", 0, new[] { "\r\n", "‸", "\r\n" })]
    [DataRow("\r\n‸12a\r\n", 0, new[] { "\r\n", "‸", "12", "a", "\r\n" })]
    public void OutOfRange(string input, int _, string[] output) => BasicTest(input, output);

    [DataTestMethod]
    [DataRow("😀", 0, new[] { "😀" })]
    [DataRow("😂", 0, new[] { "😂" })]
    [DataRow("😂😂", 0, new[] { "😂", "😂" })]
    [DataRow("😂A😂", 0, new[] { "😂", "A", "😂" })]
    [DataRow("😂A123😂", 0, new[] { "😂", "A123", "😂" })]
    public void Surrogates(string input, int _, string[] output) => BasicTest(input, output);


    [DataTestMethod]
    [DataRow("A123", 0, new[] { "A", "123" })]
    public void BreakNumAfterText(string input, int _, string[] output)
    {
        BasicTest(input, output, true);
    }

    [DataTestMethod]
    [DataRow("a.m", 0, new[] { "a.m" })]
    [DataRow("a.m.", 0, new[] { "a.m." })]
    [DataRow("a.m", 0, new[] { "a.m" })]
    [DataRow("9 a.m.", 0, new[] { "9", " ", "a.m." })]
    public void DontBreakPerioidInTextSpan(string input, int _, string[] output)
    {
        BasicTest(input, output, true);
    }
}
