using System.Configuration;
using NUnit.Framework;

namespace Meriworks.Markdown.Tests
{
    [TestFixture]
    public class ConfigTest
    {
        [Test]
        public void TestLoadFromConfiguration()
        {
            var settings = ConfigurationManager.AppSettings;
            settings.Set("Markdown.AutoHyperlink", "true");
            settings.Set("Markdown.AutoNewlines", "true");
            settings.Set("Markdown.EmptyElementSuffix", ">");
            settings.Set("Markdown.EncodeProblemUrlCharacters", "true");
            settings.Set("Markdown.LinkEmails", "false");
            settings.Set("Markdown.StrictBoldItalic", "true");
            
            var markdown = new MarkdownParser(true);
            Assert.AreEqual(true, markdown.AutoHyperlink);
            Assert.AreEqual(true, markdown.AutoNewLines);
            Assert.AreEqual(">", markdown.EmptyElementSuffix);
            Assert.AreEqual(true, markdown.EncodeProblemUrlCharacters);
            Assert.AreEqual(false, markdown.LinkEmails);
            Assert.AreEqual(true, markdown.StrictBoldItalic);
        }

        [Test]
        public void TestNoLoadFromConfigFile()
        {
            foreach (var markdown in new[] {new MarkdownParser(), new MarkdownParser(false)})
            {
                Assert.AreEqual(false, markdown.AutoHyperlink);
                Assert.AreEqual(false, markdown.AutoNewLines);
                Assert.AreEqual(" />", markdown.EmptyElementSuffix);
                Assert.AreEqual(false, markdown.EncodeProblemUrlCharacters);
                Assert.AreEqual(true, markdown.LinkEmails);
                Assert.AreEqual(false, markdown.StrictBoldItalic);
            }
        }

        [Test]
        public void TestAutoHyperlink()
        {
            var markdown = new MarkdownParser();  
            Assert.IsFalse(markdown.AutoHyperlink);
            Assert.AreEqual("<p>foo http://example.com bar</p>\n", markdown.Transform("foo http://example.com bar"));
            markdown.AutoHyperlink = true;
            Assert.AreEqual("<p>foo <a href=\"http://example.com\">http://example.com</a> bar</p>\n", markdown.Transform("foo http://example.com bar"));
        }

        [Test]
        public void TestAutoNewLines()
        {
            var markdown = new MarkdownParser();
            Assert.IsFalse(markdown.AutoNewLines);
            Assert.AreEqual("<p>Line1\nLine2</p>\n", markdown.Transform("Line1\nLine2"));
            markdown.AutoNewLines = true;
            Assert.AreEqual("<p>Line1<br />\nLine2</p>\n", markdown.Transform("Line1\nLine2"));
        }

        [Test]
        public void TestEmptyElementSuffix()
        {
            var markdown = new MarkdownParser();
            Assert.AreEqual(" />", markdown.EmptyElementSuffix);
            Assert.AreEqual("<hr />\n", markdown.Transform("* * *"));
            markdown.EmptyElementSuffix = ">";
            Assert.AreEqual("<hr>\n", markdown.Transform("* * *"));
        }

        [Test]
        public void TestEncodeProblemUrlCharacters()
        {
            var markdown = new MarkdownParser();
            Assert.IsFalse(markdown.EncodeProblemUrlCharacters);
            Assert.AreEqual("<p><a href=\"/'*_[]()/\">Foo</a></p>\n", markdown.Transform("[Foo](/'*_[]()/)"));
            markdown.EncodeProblemUrlCharacters = true;
            Assert.AreEqual("<p><a href=\"/%27%2a%5f%5b%5d%28%29/\">Foo</a></p>\n", markdown.Transform("[Foo](/'*_[]()/)"));
        }

        [Test]
        public void TestLinkEmails()
        {
            var markdown = new MarkdownParser();
            Assert.IsTrue(markdown.LinkEmails);
            //when transforming an email, the email address link is encoded a bit randomly to protect the email address
            //from simple email scanners
            var transform = markdown.Transform("<aa@bb.com>");
            var possibleTranslationsForM = new[] {"m", "&#x6d;", "&#109;"};
            var acceptableTransformationFound = false;
            for (var i = 0; !acceptableTransformationFound && i < possibleTranslationsForM.Length; i++)
            {
                var expected = "<p><a href=\"" + possibleTranslationsForM[i];
                var actual = transform.Substring(0, expected.Length);
                if (string.Equals(actual, expected))
                {
                    acceptableTransformationFound = true;
                }
            }
            Assert.IsTrue(acceptableTransformationFound,"Transformation of email was not successful. Actual:"+transform);

            markdown.LinkEmails = false;
            Assert.AreEqual("<p><aa@bb.com></p>\n", markdown.Transform("<aa@bb.com>"));
        }

        [Test]
        public void TestStrictBoldItalic()
        {
            var markdown = new MarkdownParser();
            Assert.IsFalse(markdown.StrictBoldItalic);
            Assert.AreEqual("<p>before<strong>bold</strong>after before<em>italic</em>after</p>\n", markdown.Transform("before**bold**after before_italic_after"));
            markdown.StrictBoldItalic = true;
            Assert.AreEqual("<p>before*bold*after before_italic_after</p>\n", markdown.Transform("before*bold*after before_italic_after"));
        }
    }
}
