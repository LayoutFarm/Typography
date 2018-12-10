using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.TextBreak;

[TestClass]
public class FrenchTests
{
    [TestMethod]
    public void EngEngine()
    {
        //Text source: https://en.wikibooks.org/wiki/French/Texts/Simple/Le_Corbeau_et_le_Renard
        const string Le_Corbeau_et_le_Renard = @"
Maître Corbeau, sur un arbre perché,
Tenait en son bec un fromage.
Maître Renard, par l’odeur alléché,
Lui tint à peu près ce langage :
« Hé ! bonjour, Monsieur du Corbeau.
Que vous êtes joli ! Que vous me semblez beau !
Sans mentir, si votre ramage
Se rapporte à votre plumage,
Vous êtes le Phénix des hôtes de ces bois. »
A ces mots le Corbeau ne se sent pas de joie ;
Et pour montrer sa belle voix,
Il ouvre un large bec, laisse tomber sa proie.
Le Renard s’en saisit, et dit : « Mon bon Monsieur,
Apprenez que tout flatteur
Vit aux dépens de celui qui l’écoute :
Cette leçon vaut bien un fromage, sans doute. »
Le Corbeau, honteux et confus,
Jura, mais un peu tard, qu’on ne l’y prendrait plus.";
        const string Le_Corbeau_et_le_Renard__Broken = @"
|Maître| |Corbeau|,| |sur| |un| |arbre| |perché|,|
|Tenait| |en| |son| |bec| |un| |fromage.|
|Maître| |Renard|,| |par| |l|’|odeur| |alléché|,|
|Lui| |tint| |à| |peu| |près| |ce| |langage| |:|
|«| |Hé| |!| |bonjour|,| |Monsieur| |du| |Corbeau.|
|Que| |vous| |êtes| |joli| |!| |Que| |vous| |me| |semblez| |beau| |!|
|Sans| |mentir|,| |si| |votre| |ramage|
|Se| |rapporte| |à| |votre| |plumage|,|
|Vous| |êtes| |le| |Phénix| |des| |hôtes| |de| |ces| |bois.| |»|
|A| |ces| |mots| |le| |Corbeau| |ne| |se| |sent| |pas| |de| |joie| |;|
|Et| |pour| |montrer| |sa| |belle| |voix|,|
|Il| |ouvre| |un| |large| |bec|,| |laisse| |tomber| |sa| |proie.|
|Le| |Renard| |s|’|en| |saisit|,| |et| |dit| |:| |«| |Mon| |bon| |Monsieur|,|
|Apprenez| |que| |tout| |flatteur|
|Vit| |aux| |dépens| |de| |celui| |qui| |l|’|écoute| |:|
|Cette| |leçon| |vaut| |bien| |un| |fromage|,| |sans| |doute.| |»|
|Le| |Corbeau|,| |honteux| |et| |confus|,|
|Jura|,| |mais| |un| |peu| |tard|,| |qu|’|on| |ne| |l|’|y| |prendrait| |plus.|";
        string BreakText(string text, string seperator = "|")
        {
            var breaker = new CustomBreaker { ThrowIfCharOutOfRange = true };
            var breakList = new List<BreakAtInfo>();
            breaker.SetNewBreakHandler(vis => breakList.Add(new BreakAtInfo(vis.LatestBreakAt, vis.LatestWordKind)));


#warning Use `breaker.BreakWords(text, breakList);` once #156 is merged

            breaker.BreakWords(text);
            //breaker.CopyBreakResults(breakList);


            var sb = new StringBuilder(text);
            //reverse to ensure earlier inserts do not affect later ones
            foreach (var @break in breakList.Select(i => i.breakAt).Reverse())
                sb = sb.Insert(@break, seperator);
            return sb.ToString();
        }
        var brokenString = BreakText(Le_Corbeau_et_le_Renard);
        Assert.AreEqual(Le_Corbeau_et_le_Renard__Broken, brokenString);
    }
    [TestMethod]
    public void WordKindTest()
    {
        var breaker = new CustomBreaker { ThrowIfCharOutOfRange = true };
        var breakList = new List<BreakSpan>();
        char[] test = "«Maître leçon»".ToCharArray();

        breaker.SetNewBreakHandler(vis => breakList.Add(vis.GetBreakSpan()));


#warning Use `breaker.BreakWords("«Maître leçon»", breakList);` once #156 is merged

        breaker.BreakWords(test, 0, test.Length);

        Assert.AreEqual(breakList.Count, 5);
        void BreakSpanEqual(BreakSpan actual, BreakSpan expected)
        {
            Assert.AreEqual(expected.startAt, actual.startAt);
            Assert.AreEqual(expected.len, actual.len);
            Assert.AreEqual(expected.wordKind, actual.wordKind);
        }
        BreakSpanEqual(breakList[0], new BreakSpan { startAt = 0, len = 1, wordKind = WordKind.Punc });
        BreakSpanEqual(breakList[1], new BreakSpan { startAt = 1, len = 6, wordKind = WordKind.Text });
        BreakSpanEqual(breakList[2], new BreakSpan { startAt = 7, len = 1, wordKind = WordKind.Whitespace });
        BreakSpanEqual(breakList[3], new BreakSpan { startAt = 8, len = 5, wordKind = WordKind.Text });
        BreakSpanEqual(breakList[4], new BreakSpan { startAt = 13, len = 1, wordKind = WordKind.Punc });
    }
}