using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;

namespace YandexTranslateDLL
{
    public class YandexTranslate : Yandex
    {
        protected override Uri GetRequestUri(Language from, Language to, string text, string version, string key)
        {
            return new Uri(String.Format("https://translate.yandex.net/api/{0}/tr/translate?key={1}&text={2}&lang={3}-{4}", version, key, text, from, to));
        }

        public Task<WebResponse> TranslateAsyncRaw(Language from, Language to, string text)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(GetRequestUri(from, to, text, version, key));
            return request.GetResponseAsync();
        }
        public WebResponse TranslateRaw(Language from, Language to, string text)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(GetRequestUri(from, to, text, version, key));
            return request.GetResponse();
        }
        public string Translate(Language from, Language to, string text)
        {
            WebResponse response = TranslateRaw(from, to, text);
            return CutText(response);
        }
        public Task<string> TranslateAsyns(Language from, Language to, string text)
        {
            Task<WebResponse> response = TranslateAsyncRaw(from, to, text);
            return response.ContinueWith<string>(CutTextAsyns);
        }
        private string CutText(WebResponse response)
        {
            var reader = new StreamReader(response.GetResponseStream());
            reader.ReadLine();
            string text = reader.ReadLine();
            int startIndex = text.IndexOf("<text>");
            int endIndex = text.LastIndexOf("</text>");
            text = text.Substring(startIndex + 6, endIndex - startIndex - 6);
            return text;
        }
        private string CutTextAsyns(Task<WebResponse> task)
        {
            return CutText(task.Result);
        }

        public YandexTranslate(string key, string version) : base(key, version)
        {
        }
        public YandexTranslate(string key) : base(key)
        {
        }
    }
    public class YandexDictionary : Yandex
    {
        public class Translation
        {
            protected XmlNode node;
            public Word Value;

            public Translation(XmlNode node)
            {
                this.node = node;
                Value = new Word(node.SelectSingleNode("text").InnerText, node.Attributes["pos"]?.Value, node.Attributes["gen"]?.Value);
            }
            public List<Word> Synonyms
            {
                get
                {
                    var syn = node.SelectNodes("syn");
                    List<Word> list = new List<Word>();
                    foreach (XmlNode n in syn)
                    {
                        list.Add(new Word(n.InnerText, n.Attributes["pos"]?.Value, n.Attributes["gen"]?.Value));
                    }
                    return list;
                }
            }
            public override string ToString()
            {
                return Value.ToString();
            }
        }
        public struct Word
        {
            public string Text;
            public string PartOfSpeech;
            public string Gender;
            public Word(string text, string pos = "", string gen = "")
            {
                this.Text = text;
                this.PartOfSpeech = pos;
                this.Gender = gen;
            }
            public override string ToString()
            {
                return Text;
            }
        }
        public class DictionaryResponse
        {
            XmlDocument xml = new XmlDocument();
            private const string translationsPath = "/DicResult/def/tr";
            public DictionaryResponse(string source)
            {
                xml.Load(source);
            }
            public List<Translation> Translations
            {
                get
                {
                    List<Translation> list = new List<Translation>();
                    foreach (XmlNode n in xml.SelectNodes("/DicResult/def/tr"))
                    {
                        list.Add(new Translation(n));
                    }
                    return list;
                }
            }
        }
        public YandexDictionary(string key, string version) : base(key, version)
        {
        }
        public YandexDictionary(string key) : base(key)
        {
        }
        protected override Uri GetRequestUri(Language from, Language to, string text, string version, string key)
        {
            return new Uri(String.Format("https://dictionary.yandex.net/api/{0}/dicservice/lookup?key={1}&text={2}&lang={3}-{4}", version, key, text, from, to));
        }
        public DictionaryResponse Translate(Language from, Language to, string text)
        {
            return new DictionaryResponse(GetRequestUri(from, to, text, version, key).ToString());
        }
        public Task<DictionaryResponse> TranslateAsyns(Language from, Language to, string text)
        {
            Task<DictionaryResponse> task = new Task<DictionaryResponse>(() => { return new DictionaryResponse(GetRequestUri(from, to, text, version, key).ToString()); });
            task.Start();
            return task;
        }
    }
    public abstract class Yandex
    {
        public enum Language
        {
            sq, en, ar, hy
, az
, af
, eu
, be
, bg
, bs
, cy
, vi
, hu
, ht
, gl
, nl
, el
, ka
, da
, he
, id
, ga
, it
, es
, kk
, ca
, ky
, zh
, ko
, la
, lv
, lt
, mg
, ms
, mt
, mk
, mn
, de
, no
, fa
, pl
, pt
, ro
, ru
, sr
, sk
, sl
, sw
, tg
, th
, tl
, tt
, tr
, uz
, uk
, fi
, fr
, hr
, cs
, sv
, et
, ja
        }
        protected string key;
        protected string version = "v1";
        public Yandex(string key, string version) : this(key)
        {
            this.version = version;
        }
        public Yandex(string key)
        {
            this.key = key;
        }
        protected abstract Uri GetRequestUri(Language from, Language to, string text, string version, string key);
    }
}
