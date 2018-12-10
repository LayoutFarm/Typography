//MIT, 2016-2017, WinterDev
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Typography.TextBreak;
using Typography.TextBreak.ICU;

namespace TextBreakerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void cmdReadDict_Click(object sender, EventArgs e)
        {
#if TEST_ICU
            LayoutFarm.TextBreaker.ICU.DictionaryData.LoadData("../../../icu58/brkitr/thaidict.dict");
#endif
        }


        string icu_currentLocale = "th-TH";
        private void Form1_Load(object sender, EventArgs e)
        {
            InitIcuLib();
            //thai
            //currentLocale = "th-TH";
            //string test1 = "ผู้ใหญ่บหาผ้าใหม่ให้สะใภ้ใช้คล้องคอใฝ่ใจเอาใส่ห่อมิหลงใหลใครขอดูจะใคร่ลงเรือใบดูน้ำใสและปลาปูสิ่งใดอยู่ในตู้มิใช่อยู่ใต้ตั่งเตียงบ้าใบถือใยบัวหูตามัวมาให้เคียงเล่าเท่าอย่าละเลี่ยงยี่สิบม้วนจำจงดี";
            // string test1 = "ขาย =";
            //string test1 = "แป้นพิมลาว";            
            //string test1 = "ผ้าใหม่";
            //string test1 = "ประ";

            //----------------
            //
            //lao
            icu_currentLocale = "lo-LA";
            //string test1 = "ສະບາຍດີແປ້ນພິມລາວ";
            //string test1 = "ສາທາລະນະລັດ ປະຊາທິປະໄຕ ປະຊາຊົນລາວ";
            string test1 = "ABCD1234567890ສາທາລະນະລັດ ປະຊາທິປະໄຕ ປະຊາຊົນລາວ ผู้ใหญ่หาผ้าใหม่ให้สะใภ้ใช้คล้องคอ" +
            "ใฝ่ใจเอาใส่ห่อมิหลงใหลใครขอดูจะใคร่ลงเรือใบดูน้ำใสและปลาปูสิ่งใดอยู่ในตู้มิใช่อยู่ใต้ตั่งเตียงบ้าใบถือใยบัวหูตามัวมาใกล้เคียงเล่าท่องอย่าละเลี่ยงยี่สิบม้วนจำจงดี";
            //----------------
            this.textBox1.Text = test1;

        }

        static bool icuLoaded;
        static void InitIcuLib()
        {
            if (icuLoaded) return;
            //

            string icu_dataFile = @"../../icudt57l.dat";
            Typography.TextBreak.ICU.NativeTextBreaker.SetICUDataFile(icu_dataFile);
            icuLoaded = true;
        }

        NativeTextBreaker nativeTextBreak;
        private void cmdIcuTest_Click(object sender, EventArgs e)
        {

            if (nativeTextBreak == null)
            {
                nativeTextBreak = new NativeTextBreaker(Typography.TextBreak.ICU.TextBreakKind.Word, icu_currentLocale);
            }

            char[] textBuffer = this.textBox1.Text.ToCharArray();
            this.listBox1.Items.Clear();
            nativeTextBreak.DoBreak(textBuffer, 0, textBuffer.Length, bounds =>
            {
                //sub string               
                string s = new string(textBuffer, bounds.startIndex, bounds.length);
                this.listBox1.Items.Add(bounds.startIndex + " " + s);

            });

        }
        private void cmdManaged_Click(object sender, EventArgs e)
        {

            //some lang eg. Thai, Lao, need dictionary breaking
            //we use dic data from icu-project

            //1. create dictionary based breaking engine 
            //TODO: dic should be read once
            var dicProvider = new IcuSimpleTextFileDictionaryProvider() { DataDir = "../../../icu62/brkitr" };
            CustomBreakerBuilder.Setup(dicProvider);
            CustomBreaker breaker1 = CustomBreakerBuilder.NewCustomBreaker();
            breaker1.BreakNumberAfterText = true;
            char[] test = this.textBox1.Text.ToCharArray();
            this.listBox1.Items.Clear();

            breaker1.SetNewBreakHandler(vis =>
            {
                BreakSpan span = vis.GetBreakSpan();
                string s = new string(test, span.startAt, span.len);
                this.listBox1.Items.Add(span.startAt + " " + s);

            });
            breaker1.BreakWords(test, 0, test.Length);

            //foreach (BreakSpan span in breaker1.GetBreakSpanIter())
            //{
            //   
            //    this.listBox1.Items.Add(span.startAt + " " + s);
            //}
        }
        static bool StringStartsWithChars(string srcString, string value)
        {
            int findingLen = value.Length;
            if (findingLen > srcString.Length)
            {
                return false;
            }
            //
            unsafe
            {
                fixed (char* srcStringBuff = srcString)
                fixed (char* findingChar = value)
                {
                    char* srcBuff1 = srcStringBuff;
                    char* findChar1 = findingChar;
                    for (int i = 0; i < findingLen; ++i)
                    {
                        //compare by values
                        if (*srcBuff1 != *findChar1)
                        {
                            return false;
                        }
                        srcBuff1++;
                        findChar1++;
                    }
                    //MATCH all
                    return true;
                }
            }
        }
        private void cmdPerformace1_Click(object sender, EventArgs e)
        {
            //do this performance test in release mode


            int ntimes = 10000;
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

            System.GC.Collect();//clear 
            stopWatch.Start();
            ParseWithManaged(ntimes);
            stopWatch.Stop();
            long ms1 = stopWatch.ElapsedMilliseconds;
            //----------------------------
            //Icu
            System.GC.Collect();//clear
            stopWatch.Reset();
            stopWatch.Start();
            ParseWithIcu(ntimes);
            stopWatch.Stop();
            long ms2 = stopWatch.ElapsedMilliseconds;
            //----------------------------
            MessageBox.Show("Managed: " + ms1.ToString() + "ms, Native Icu: " + ms2.ToString() + "ms");
        }
        void ParseWithManaged(int ntimes)
        {

            //-------------------
            var dicProvider = new IcuSimpleTextFileDictionaryProvider() { DataDir = "../../../icu58/brkitr_src" };
            CustomBreakerBuilder.Setup(dicProvider);
            CustomBreaker breaker1 = CustomBreakerBuilder.NewCustomBreaker();
            breaker1.SetNewBreakHandler(vis => { }); //just break, do nothing about result
            char[] test = this.textBox1.Text.ToCharArray();
            //-------------
            for (int i = ntimes - 1; i >= 0; --i)
            {
                breaker1.BreakWords(test, 0, test.Length);
            }
        }
        void ParseWithIcu(int ntimes)
        {

            //-------------------
            if (nativeTextBreak == null)
            {
                nativeTextBreak = new NativeTextBreaker(Typography.TextBreak.ICU.TextBreakKind.Word, icu_currentLocale);
            }

            char[] textBuffer = this.textBox1.Text.ToCharArray();
            for (int i = ntimes - 1; i >= 0; --i)
            {
                nativeTextBreak.DoBreak(textBuffer, 0, textBuffer.Length, bounds =>
                {
                    //sub string            
                });
            }
        }


    }
}

